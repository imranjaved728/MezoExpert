using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication2.Models;
using AutoMapper;
using WebApplication2.DBEntities;
using Microsoft.AspNet.Identity;
using System.IO;
using Newtonsoft.Json;
using Microsoft.AspNet.SignalR;
using SignalRChat;
using WebApplication2.App_Start;
using PayPal.Sample;
using PayPal.Api;
using System.Web.Helpers;
using System.Net.Mail;
using WebApplication2.Helpers;
using System.Web.Mail;
using System.Threading;

namespace WebApplication2.Controllers
{
    [CustomAuthorize(Roles = "Student,Admin")]
    public class StudentsController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private PayPal.Api.Payment payment;
       
        private PayPal.Api.Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution() { payer_id = payerId };
            this.payment = new PayPal.Api.Payment() { id = paymentId };
            return this.payment.Execute(apiContext, paymentExecution);
        }


       

        [HttpGet]
        public async Task<ActionResult>Sessions(Guid SessionId)
        {
         
            var session=await db.sessions.FindAsync(SessionId);
            if (session.question.student.Username == User.Identity.Name)
            {
                ChatModel obj = new ChatModel();
                obj.session = session;
                obj.status = db.online.Where(c => c.Username == session.tutor.Username).FirstOrDefault().Status;
                obj.session.Replies = obj.session.Replies.OrderBy(c => c.PostedTime).ToList();
                return View(obj);
            }
            else
            {
                return RedirectToAction("Unauthorized", "Home", "");
            }
        }

        public void DeleteSessionMessageTutor(string sessionId, string sendTo, string message, IHubContext context)
        {
            //var name = Context.User.Identity.Name;
            using (var db = new ApplicationDbContext())
            {
                var user = db.Useras.Where(c => c.UserName == sendTo && c.SessionId == sessionId).FirstOrDefault();
                if (user == null)
                {
                    // context.Clients.Caller.showErrorMessage("Could not find that user.");
                }
                else
                {
                    db.Entry(user)
                        .Collection(u => u.Connections)
                        .Query()
                        .Where(c => c.Connected == true)
                        .Load();

                    if (user.Connections == null)
                    {
                        //  Clients.Caller.showErrorMessage("The user is no longer connected.");
                    }
                    else
                    {
                        foreach (var connection in user.Connections)
                        {
                            context.Clients.Client(connection.ConnectionID)
                                 .recieverSessionClosed(message);
                        }
                    }
                }
            }
        }
        [HttpGet]
        public async Task<ActionResult> DeleteSession(string sessionId)
        {
            var session = db.sessions.Find(new Guid(sessionId));
            if (session.question.student.Username == User.Identity.Name)
            {

                if (session.Status != Status.Hired)
                {
                    session.isStudentDelete = true;
                    session.isClosed = true;
                    db.Entry(session).State = EntityState.Modified;

                    await db.SaveChangesAsync();

                    var context = GlobalHost.ConnectionManager.GetHubContext<TutorStudentChat>();
                    var tutor = db.sessions.Where(c => c.SessionID == new Guid(sessionId)).FirstOrDefault().tutor;
                    var username = tutor.Username;
                    var message = "";
                    DeleteSessionMessageTutor(sessionId, username, message, context);
                    SendNotification(username, session.question.student.Username, session.question.student.ProfileImage, "Session has been closed.",true, "Tutors", "Sessions", "SessionId=" + sessionId);


                }
            }

            return RedirectToAction("inbox");

        }


        public async Task<ActionResult> Inbox()
        {
            var user = new Guid(User.Identity.GetUserId());
            var MineSessions = db.Questions.Where(c => c.StudentID == user).ToList();
            List<StudentInbox> list = new List<StudentInbox>();
            var onlineusers = db.online.Where(c => c.Status == true).ToList();
            foreach (var question in MineSessions)
            {
                foreach (var session in question.Sessions.Where(c=>c.isStudentDelete== false))
                {
                    StudentInbox obj = new StudentInbox();
                    obj.SenderName = session.tutor.Username;
                    var online= onlineusers.Where(c => c.Username == obj.SenderName).FirstOrDefault();
                    obj.onlineStatus = online == null ? false : online.Status;
                    obj.SenderProfile = session.tutor.ProfileImage;
                    var lastreply = session.Replies.OrderBy(c => c.PostedTime);
                    obj.PostedTime = lastreply.LastOrDefault().PostedTime;
                    obj.SessionId = session.SessionID;
                    obj.Status = session.Status;
                    obj.newMessage = session.NewMessageStudent;
                    obj.LastMessage = lastreply.LastOrDefault().Details;
                    list.Add(obj);

                }
            }
            return View(list);
           
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Payment(Models.Payment model)
        {
           
            var apiContext = Configuration.GetAPIContext();
            try
            {
                string payerId = Request.Params["PayerID"];

                if (string.IsNullOrEmpty(payerId))
                {
                    // ###Items
                    // Items within a transaction.
                    var itemList = new PayPal.Api.ItemList()
                    {
                        items = new List<Item>()
                    {
                        new Item()
                        {
                            name = "Mezo Experts",
                            currency = "USD",
                            price = model.Amount.ToString(),
                            quantity = "1",
                            sku = "sku"
                        }
                    }
                    };

                    // ###Payer
                    // A resource representing a Payer that funds a payment
                    // Payment Method
                    // as `paypal`
                    var payer = new PayPal.Api.Payer() { payment_method = "paypal" };

                    // ###Redirect URLS
                    // These URLs will determine how the user is redirected from PayPal oncef they have either approved or canceled the payment.
                    var baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Students/AccountSettings?";
                    var guid = Convert.ToString((new Random()).Next(100000));
                    var redirectUrl = baseURI + "guid=" + guid;
                    var redirUrls = new RedirectUrls()
                    {
                        cancel_url = redirectUrl + "&cancel=true",
                        return_url = redirectUrl
                    };

                    // ###Details
                    // Let's you specify details of a payment amount.
                    var details = new PayPal.Api.Details()
                    {
                        tax = "0",
                        shipping = "0",
                        subtotal = model.Amount.ToString()
                    };

                    // ###Amount
                    // Let's you specify a payment amount.
                    var amount = new PayPal.Api.Amount()
                    {
                        currency = "USD",
                        total = model.Amount.ToString(), // Total must be equal to sum of shipping, tax and subtotal.
                        details = details
                    };

                    // ###Transaction
                    // A transaction defines the contract of a
                    // payment - what is the payment for and who
                    // is fulfilling it. 
                    var transactionList = new List<PayPal.Api.Transaction>();

                    // The Payment creation API requires a list of
                    // Transaction; add the created `Transaction`
                    // to a List
                    transactionList.Add(new PayPal.Api.Transaction()
                    {
                        description = "Mezo Experts Services",
                        invoice_number = Common.GetRandomInvoiceNumber(),
                        amount = amount,
                        item_list = itemList
                    });

                    // ###Payment
                    // A Payment Resource; create one using
                    // the above types and intent as `sale` or `authorize`
                    var payment = new PayPal.Api.Payment()
                    {
                        intent = "sale",
                        payer = payer,
                        transactions = transactionList,
                        redirect_urls = redirUrls,

                    };

                    // Create a payment using a valid APIContext

                    var createdPayment = payment.Create(apiContext);

                    var links = createdPayment.links.GetEnumerator();

                    string paypalRedirectUrl = null;

                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;

                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            //saving the payapalredirect URL to which user will be redirected for payment
                            paypalRedirectUrl = lnk.href;
                        }
                    }

                    // saving the paymentID in the key guid
                    Session.Add(guid, createdPayment.id);
                    PaypalPayments payments = new PaypalPayments();
                    payments.amount = model.Amount.ToString();
                    payments.ID = Guid.NewGuid();
                    payments.status = Status.Offered;
                    payments.paymentId = createdPayment.id;
                    payments.token = createdPayment.token;
                    payments.guid = guid;
                    payments.UserId = new Guid(User.Identity.GetUserId());
                    db.payments.Add(payments);
                    db.SaveChanges();
                   
                    return Redirect(paypalRedirectUrl);
                }

                return null;
            }
            catch (Exception ex)
            {

                return View("FailureView");
            }
            // return  Json(new { result = createdPayment.links[0].href, redirect = createdPayment.links[1].href, execute = createdPayment.links[2].href });

            return null;
        }


        public async Task<ActionResult> AccountSettings()
        {

            string payerId = Request.Params["PayerID"];

            if (string.IsNullOrEmpty(payerId))
            {

            }
            else
            {
                try { 

                    var apiContext = Configuration.GetAPIContext();
                    var guid = Request.Params["guid"];
                    var paymentId = Request.Params["paymentId"];

                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);

                    if (executedPayment.state.ToLower() == "approved")
                    {
                        var payments = db.payments.Where(c => c.paymentId == paymentId).FirstOrDefault();
                        payments.status = Status.Approved;

                        var user = db.Students.Where(c => c.Username == User.Identity.Name).FirstOrDefault();
                        user.CurrentBalance = user.CurrentBalance + (float)Convert.ToDouble(payments.amount);
                        db.Entry(user).State = EntityState.Modified;
                        db.Entry(payments).State = EntityState.Modified;
                        db.SaveChanges();

                    }

                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("FailureView");
                    }
            }
             catch (Exception ex)
            {
                    Models.Payment model = new Models.Payment();
                    model.Balance = db.Students.Where(c => c.Username == User.Identity.Name).FirstOrDefault().CurrentBalance.ToString();
                    return View(model);
            }
        }
           

            Models.Payment obj = new Models.Payment();
            obj.Balance = db.Students.Where(c => c.Username == User.Identity.Name).FirstOrDefault().CurrentBalance.ToString();
           return View(obj);
           

        }

        public ActionResult Index()
        {
            
            var user = new Guid(User.Identity.GetUserId());
           
            var MineSessions = db.Questions.Where(c => c.StudentID == user).ToList();
            List<StudentInbox> list = new List<StudentInbox>();
            var onlineusers = db.online.Where(c => c.Status == true).ToList();

            foreach (var question in MineSessions)
            {
                var hiredSession = question.Sessions.Where(c => c.Status == Status.Hired || c.Status== Status.Conflict);
                foreach (var session in hiredSession)
                {
                    StudentInbox obj = new StudentInbox();
                    obj.SenderName = session.tutor.Username;
                    obj.SenderProfile = session.tutor.ProfileImage;
                    var online = onlineusers.Where(c => c.Username == obj.SenderName).FirstOrDefault();
                    obj.onlineStatus = online == null ? false : online.Status;

                    var lastreply = session.Replies.OrderBy(c => c.PostedTime);
                    obj.PostedTime = lastreply.LastOrDefault().PostedTime;
                    obj.SessionId = session.SessionID;
                    obj.Status = session.Status;
                    obj.LastMessage = lastreply.LastOrDefault().Details;
                    obj.newMessage = session.NewMessageStudent;
                    list.Add(obj);

                }
            }
            StudentHomeModel model = new StudentHomeModel();
            model.obj = list;
            model.questions= MineSessions.OrderBy(c => c.PostedTime).ToList();

            Session["noticounter"] = db.notifications.Where(c => c.UserName == User.Identity.Name && c.isRead == false).Count();
            var result = db.notifications.Where(c => c.UserName == User.Identity.Name).OrderByDescending(c => c.postedTime).Take(5);
            Session["notifications"] = result.ToList();

            return View(model);
        }

        public ActionResult PostQuestion(string id,string question)
        {
            if(id!=null)
            {
                var tutorId = db.Tutors.Where(c=>c.TutorID==new Guid(id)).First();
                ViewBag.Username = tutorId.Username;
                ViewBag.Id = id;
            }
            

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            QuestionViewModel obj = new QuestionViewModel();
            obj.Details = question;

           
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> PostQuestion([Bind(Include = "QuestionID,StudentID,TutorID,Title,Details,Status,Amount,CategoryID,DueDate,PostedTime")] QuestionViewModel question)
        {
            if (ModelState.IsValid)
            {
                Question quest = Mapper.Map<QuestionViewModel, Question>(question);
                quest.QuestionID = Guid.NewGuid();
                quest.PostedTime = DateTime.Now;
                //initial posted Question Status
                quest.Status = Status.Posted;
                quest.TutorID = question.TutorID;
                quest.Details=quest.Details.Replace(Environment.NewLine, "<br/>");

                bool singleTutor = false;
                string sessionId = null;
                string postId = quest.QuestionID.ToString();
                if(question.TutorID!=null)
                {
                    singleTutor = true;
                    Session obj = new Session();
                    obj.SessionID = Guid.NewGuid();
                    sessionId = obj.SessionID.ToString();
                    obj.TutorID = question.TutorID; 
                    obj.QuestionID = quest.QuestionID;
                    obj.PostedTime = DateTime.Now;
                    obj.Status = Status.Posted;
                    obj.NewMessageTutor = true;
                    db.sessions.Add(obj);

                    Reply rep = new Reply();
                    rep.ReplyID = Guid.NewGuid();
                    rep.SessionID = obj.SessionID;
                    rep.ReplierID = quest.StudentID;
                    rep.PostedTime = DateTime.Now;
                    rep.Details =quest.Title;
                    db.Replies.Add(rep);
                }


                //user posting question id
                quest.StudentID = new Guid(User.Identity.GetUserId());
                db.Questions.Add(quest);
                await db.SaveChangesAsync();

                var student=db.Students.Find(quest.StudentID);
       
                string response = quest.QuestionID +"$"+quest.Title+ "$" + student.ProfileImage + "%" + User.Identity.Name +  "$" + quest.Amount+"$"+quest.PostedTime+"$" + quest.DueDate ;

                string body;
                using (var sr = new StreamReader(Server.MapPath("\\Helpers\\") + "email.html"))
                {
                    body = sr.ReadToEnd();
                }
               
                //send emails
                if (singleTutor == false)
                {
                    var tutors = db.Tutors.Where(c => c.IsCompletedProfile == true).ToList();
                    var allusers = db.Users.ToList();

                   

                            foreach (var v in tutors)
                            {
                                try
                                {
                                    var emailUser = allusers.Where(c => c.UserName == v.Username).FirstOrDefault();
                                    SendNotification(emailUser.UserName, student.Username, student.ProfileImage, "New Question posted.",true, "Tutors", "QuestionDetails", "PostId=" + postId);

                                    if (emailUser != null)
                                    {
                                        var email = emailUser.Email;
                                        Mailer mailer = new Mailer();
                                        mailer.ToEmail = email;
                                        mailer.Subject = "New Question Posted on MezoExperts.com";
                                        mailer.Body = string.Format(body, question.Title, quest.Details);
                                        mailer.IsHtml = true;
                                       await mailer.Send();
                                    }
                                    // Send the emails here
                               //
                                 }
                                catch (Exception e)
                                {

                                }
                        }
                    

                }
                else
                {
                    try
                    {
                        var tutor = db.Users.Find(quest.TutorID.ToString());
                      
                        SendNotification(tutor.UserName, student.Username, student.ProfileImage, "I have a new task for you.",true, "Tutors", "Sessions", "SessionId=" + sessionId);
                        
                        Mailer mailer = new Mailer();
                        mailer.ToEmail = tutor.Email;
                        mailer.Subject = "New Question Posted on MezoExperts.com";
                        mailer.Body = string.Format(body, question.Title, quest.Details);
                        mailer.IsHtml = true;
                        await mailer.Send();


                    }
                    catch (Exception e)
                    {

                    }
                }

                return new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = response }
                };
               
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return null;
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> RejectPayment(Offer question)
        {
            var user = User.Identity.GetUserId();
            var sessionId = question.SessionId;
            var session = db.sessions.Where(c => c.SessionID == sessionId && c.question.StudentID == new Guid(user)).First();
            if (session.Status == Status.Invoiced)
            {
                session.Status = Status.Conflict;
                session.isClosed = true;
                var quest = session.question;
                quest.Status = Status.Conflict;
                db.Entry(quest).State = EntityState.Modified;
                db.Entry(session).State = EntityState.Modified;
                Reply obj = new Reply();
                obj.ReplyID = Guid.NewGuid();
                obj.ReplierID = new Guid(User.Identity.GetUserId());
                obj.SessionID = sessionId;
                obj.PostedTime = DateTime.Now;
                obj.Details = " Automatically Generated Message: I have Rejected the payment for " + session.OfferedFees + "$. Now admin will handle the dispute in 7 days. ";
                session.Replies.Add(obj);

                db.SaveChanges();


                var context = GlobalHost.ConnectionManager.GetHubContext<TutorStudentChat>();
                var username = User.Identity.Name;
                var imgsrc = db.Students.Where(c => c.Username == username).FirstOrDefault().ProfileImage;
                string message = generateMessage(username, obj.Details, imgsrc, obj.PostedTime.ToString(), obj.ReplyID.ToString(),false);
                SendChatMessageStudentReciever(obj.SessionID.ToString(), username, message, context); //send message to urself 
                var tutor = db.sessions.Where(c => c.SessionID == obj.SessionID).FirstOrDefault().tutor;
                var username2 = tutor.Username;
                SendChatMessageTutorReciever(obj.SessionID.ToString(), username2, message, context); //send message to other person 

                var message2 = " <button type=\"button\" id=\"rejected\" disabled class=\"btn btn-primary\">Rejected Invoice</button>";

                SendButtonStudent(sessionId.ToString(), username2, message2, context); //send message to other person 
                SendNotification(username2, username, imgsrc, "I have approved the payment of $" + session.OfferedFees,true, "Tutors", "Sessions", "SessionId=" + sessionId);
                //close sessions
                DeleteSessionMessageTutor(sessionId.ToString(), username2, "", context);
                DeleteSessionMessageTutor(sessionId.ToString(), username, "", context);

                try
                {

                    string body;
                    using (var sr = new StreamReader(Server.MapPath("\\Helpers\\") + "passwordreset.html"))
                    {
                        body = sr.ReadToEnd();
                    }
                    
                    var email = db.Users.Where(c => c.Id == quest.TutorID.ToString()).FirstOrDefault().Email;
                    Mailer mailer = new Mailer();
                    mailer.ToEmail = email;
                    mailer.Subject = "Invoice Rejected on MezoExperts.com";
                    mailer.Body = string.Format(body, username + " rejected the invoice. Now administrator will handle the session.");
                    mailer.IsHtml = true;
                    await mailer.Send();


                }
                catch (Exception e)
                {

                }

                return new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "success" }
                };

            }
            else
            {
                return new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "fail" }
                };

            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Rate(Offer question)
        {
            var user = User.Identity.GetUserId();
            var sessionId = question.SessionId;
            var session = db.sessions.Where(c => c.SessionID == sessionId && c.question.StudentID == new Guid(user)).First();
            if (session.Status == Status.Approved)
            {
                session.ratings = Convert.ToDouble(question.Rating);
                db.Entry(session).State = EntityState.Modified;

                var tutorSession = db.sessions.Where(c => c.TutorID == session.TutorID && c.Status==Status.Approved).ToList();
                int count = 0;
                float sum = 0;
                foreach (var t  in tutorSession)
                {
                    if (t.ratings != null) { 
                        sum = sum +(float) t.ratings.Value;
                        count++;
                    }
                }

                float rating = 0;

                if(count>0)
                    rating=sum / count;

                var tutor = db.Tutors.Where(c => c.TutorID == session.TutorID.Value).FirstOrDefault();
                tutor.Rating = rating;
                db.Entry(tutor).State = EntityState.Modified;
                db.SaveChanges();

                SendNotification(tutor.Username, session.question.student.Username, session.question.student.ProfileImage, "I have rated you " + rating+" star for your work.",true, "Tutors", "Sessions", "SessionId=" + sessionId);


                return new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "success" }
                };

            }
            else
            {
                return new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "fail" }
                };

            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ApprovePayment(Offer question)
        {
            var user = User.Identity.GetUserId();
            var sessionId = question.SessionId;
            var session = db.sessions.Where(c => c.SessionID == sessionId && c.question.StudentID == new Guid(user)).First();
            if (session.Status == Status.Invoiced)
            {
                session.Status = Status.Approved;
                session.isClosed = true;
                var quest = session.question;
                quest.Status = Status.Approved;
                db.Entry(quest).State = EntityState.Modified;
                db.Entry(session).State = EntityState.Modified;
               
                Reply obj = new Reply();
                obj.ReplyID = Guid.NewGuid();
                obj.ReplierID = new Guid(User.Identity.GetUserId());
                obj.SessionID = sessionId;
                obj.PostedTime = DateTime.Now;
                obj.Details = " Automatically Generated Message: I have Approved the payment for " + session.OfferedFees + "$. ";
                session.Replies.Add(obj);

                var tutor = db.sessions.Where(c => c.SessionID == obj.SessionID).FirstOrDefault().tutor;
                tutor.CurrentEarning = tutor.CurrentEarning + (float)session.OfferedFees;
                db.Entry(tutor).State = EntityState.Modified; 

                db.SaveChanges();


                var context = GlobalHost.ConnectionManager.GetHubContext<TutorStudentChat>();
                var username = User.Identity.Name;
                var imgsrc = db.Students.Where(c => c.Username == username).FirstOrDefault().ProfileImage;
                string message = generateMessage(username, obj.Details, imgsrc, obj.PostedTime.ToString(), obj.ReplyID.ToString(),false);
                SendChatMessageStudentReciever(obj.SessionID.ToString(), username, message, context); //send messageFp to urself 
                //var tutor = db.sessions.Where(c => c.SessionID == obj.SessionID).FirstOrDefault().tutor;
                var username2 = tutor.Username;
                SendChatMessageTutorReciever(obj.SessionID.ToString(), username2, message, context); //send message to other person 

                var message2 = " <button type=\"button\" id=\"accepted\" disabled class=\"btn btn-primary\">Accepted Invoice</button>";

                SendButtonStudent(sessionId.ToString(), username2, message2, context); //send message to other person 
                SendNotification(username2, session.question.student.Username, session.question.student.ProfileImage, "I have approved the payment of $" + session.OfferedFees,true, "Tutors", "Sessions", "SessionId=" + sessionId);
                //close sessions
                DeleteSessionMessageTutor(sessionId.ToString(), username2, "", context);
                DeleteSessionMessageTutor(sessionId.ToString(), username, "", context);
                try
                {

                    string body;
                    using (var sr = new StreamReader(Server.MapPath("\\Helpers\\") + "passwordreset.html"))
                    {
                        body = sr.ReadToEnd();
                    }
                    
                    var email = db.Users.Where(c => c.Id == quest.TutorID.ToString()).FirstOrDefault().Email;
                    Mailer mailer = new Mailer();
                    mailer.ToEmail = email;
                    mailer.Subject = "Invoice Approved on MezoExperts.com";
                    mailer.Body = string.Format(body, username+" approved the Invoice.");
                    mailer.IsHtml = true;
                    await mailer.Send();


                }
                catch (Exception e)
                {

                }

                return new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "success" }
                };

            }
            else
            {
                return new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "fail" }
                };

            }
        }

        private void SendNotification(string sendTo,string username,string image, string message,bool addDb,string controller,string action,string param)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<notifications>();
            //var name = Context.User.Identity.Name;
            using (var db = new ApplicationDbContext())
            {
                var user = db.notifyConnections.Where(c => c.userName == sendTo).FirstOrDefault();
                if (user != null) {

                    string finalmessage = image + "^" + username + "^" + message;
                    string link= controller + "/" + action + "?" + param; 
                    if (addDb == true)
                    {
                        Notifications notify = new Notifications();
                        notify.ID = Guid.NewGuid();
                        notify.isRead = false;
                        notify.Message = finalmessage;
                        notify.UserName = sendTo;
                        notify.postedTime = DateTime.Now;
                        notify.link = link;
                        db.notifications.Add(notify);
                        db.SaveChanges();
                    }
                    var counter = db.notifications.Where(c => c.UserName == sendTo && c.isRead == false).Count();

                    finalmessage = finalmessage + "^" + counter+"^"+ link ;

                    context.Clients.Client(user.connectionId)
                                 .recieverNotifier(finalmessage);

                    
                 }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Hire(Offer question)
        {
            var userId = User.Identity.GetUserId();
            var sessionId = question.SessionId;
            var session = db.sessions.Where(c => c.SessionID == sessionId && c.question.StudentID == new Guid(userId)).First();
            var user = session.question.student;
            if(user.CurrentBalance < session.OfferedFees)
            {
                return new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "You donot have enough funds to hire the expert.Go to Account Settings and add funds first." }
                };
            }
            else if(session.Status == Status.Offered)
            {
                session.Status = Status.Hired;
                var quest = session.question;
                quest.Status = Status.Hired;
                user.CurrentBalance = user.CurrentBalance - (float)session.OfferedFees;
                db.Entry(user).State = EntityState.Modified; 
                db.Entry(quest).State = EntityState.Modified;
                db.Entry(session).State = EntityState.Modified;
               

                Reply obj = new Reply();
                obj.ReplyID = Guid.NewGuid();
                obj.ReplierID = new Guid(User.Identity.GetUserId());
                obj.SessionID = sessionId;
                obj.PostedTime = DateTime.Now;
                obj.Details = " Automatically Generated Message: I have Hired you for $" + session.OfferedFees + ". You can start working on task." ;
                session.Replies.Add(obj);

                

                db.SaveChanges();

           
                var context = GlobalHost.ConnectionManager.GetHubContext<TutorStudentChat>();
                var username = User.Identity.Name;
                var imgsrc = db.Students.Where(c => c.Username == username).FirstOrDefault().ProfileImage;
                string message = generateMessage(username, obj.Details, imgsrc, obj.PostedTime.ToString(), obj.ReplyID.ToString(),false);
                SendChatMessageStudentReciever(obj.SessionID.ToString(), username, message, context); //send message to urself 
                var tutor = db.sessions.Where(c => c.SessionID == obj.SessionID).FirstOrDefault().tutor;
                var username2 = tutor.Username;
                SendChatMessageTutorReciever(obj.SessionID.ToString(), username2, message, context); //send message to other person 

                //send button
               
                var message2 = "  <button type=\"button\" id=\"hire\" class=\"btn btn-primary\" data-toggle=\"modal\" data-target=\"#invoiceNewModal\">Send Invoice</button>";
                                                                                       
                SendButtonStudent(sessionId.ToString(), username2, message2, context); //send message to other person 
                SendNotification(username2, username,imgsrc, "I have hired you for $"+session.OfferedFees,true,"Tutors","Sessions", "SessionId="+sessionId);

                try
                {

                    string body;
                    using (var sr = new StreamReader(Server.MapPath("\\Helpers\\") + "passwordreset.html"))
                    {
                        body = sr.ReadToEnd();
                    }
                    

                    var email = db.Users.Where(c=>c.Id==quest.TutorID.ToString()).FirstOrDefault().Email;
                    Mailer mailer = new Mailer();
                    mailer.ToEmail = email;
                    mailer.Subject = "Hired on MezoExperts.com";
                    mailer.Body = string.Format(body, username + " has hired you for the job.");
                    mailer.IsHtml = true;
                    await mailer.Send();


                }
                catch (Exception e)
                {

                }

                return new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "success" }
                };

            }
            else
            {
                return new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "fail" }
                };

            }
        }

        public void SendButtonStudent(string sessionId, string sendTo, string message, IHubContext context)
        {
            //var name = Context.User.Identity.Name;
            using (var db = new ApplicationDbContext())
            {
                var user = db.Useras.Where(c => c.UserName == sendTo && c.SessionId == sessionId).FirstOrDefault();
                if (user == null)
                {
                    // context.Clients.Caller.showErrorMessage("Could not find that user.");
                }
                else
                {
                    db.Entry(user)
                        .Collection(u => u.Connections)
                        .Query()
                        .Where(c => c.Connected == true)
                        .Load();

                    if (user.Connections == null)
                    {
                        //  Clients.Caller.showErrorMessage("The user is no longer connected.");
                    }
                    else
                    {
                        foreach (var connection in user.Connections)
                        {
                            context.Clients.Client(connection.ConnectionID)
                                 .recieverButtons(message);
                        }
                    }
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Chat(ChatModel reply)
        {
            Reply obj = new Reply();
            obj.ReplyID = Guid.NewGuid();
            obj.ReplierID= new Guid(User.Identity.GetUserId());
            obj.SessionID = reply.sessionID;
            obj.PostedTime = DateTime.Now;
            reply.replyDetail=reply.replyDetail.Replace(Environment.NewLine, "<br/>");
            obj.Details = reply.replyDetail;

            db.Replies.Add(obj);

            var session = db.sessions.Where(c => c.SessionID == obj.SessionID).FirstOrDefault();
            session.NewMessageTutor = true;
            db.Entry(session).State = EntityState.Modified;

            await db.SaveChangesAsync();

            var context = GlobalHost.ConnectionManager.GetHubContext<TutorStudentChat>();
            var username = User.Identity.Name;
            var imgsrc = db.Students.Where(c => c.Username == username).FirstOrDefault().ProfileImage;

           //part moved up
            var tutor = db.sessions.Where(c => c.SessionID == obj.SessionID).FirstOrDefault().tutor;
            var username2 = tutor.Username;
            var status = db.online.Where(c => c.Username == username2).FirstOrDefault().Status;

            string message = generateMessage(username, obj.Details, imgsrc, obj.PostedTime.ToString(),obj.ReplyID.ToString(), status);
            SendChatMessageStudentReciever(obj.SessionID.ToString(), username, message, context); //send message to urself 

            SendChatMessageTutorReciever(obj.SessionID.ToString(), username2, message, context); //send message to other person 
            //SendNotification(username2, imgsrc,"I have hired you for ");
          
        return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { result = obj.ReplyID +"$" + obj.SessionID }
            };
          /*  return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { result = "empty" }
            }; */
        }

        public string generateMessage(string username, string detail, string imgsrc,string postedTime,string replyID, bool online)
        {
             string filestring = "<div id=\""+replyID+"\"></div>";
          
            string message = "";
            message = message + "<li class=\"" + online + "\"></li><li class=\"media\">" +
                            "<div class=\"comment\"> " +
                                    "<a href=\"#\" class=\"pull-left\"><img src=\"" + imgsrc + "\" alt=\"\" class=\"img-circle imgSize\"> </a>" +
                                     " <div class=\"media-body\">" +
                                     " <a style=\"text-decoration:none\" href=\"@Url.Action(\"Profile\", \"Home\", new { username = tutor.Username })\"> <strong class=\"text-success userText username\">" + username + "</strong><br /><br /></a>" +
                                       detail +
                                      filestring +
                                     "<div class=\"clearfix\"></div>" +
                                     " </div>" +
                                     "<div style=\"margin-bottom:20px\">" +
                                              "<small class=\"text-muted pull-right\">" + postedTime + "</small>"+
                                       " </div>"+
                                     "<hr>" +
                               "</div>" +
                               "</li>";
            return message;
        }
        public void SendChatMessageTutorRecieverFile(string sessionId, string sendTo, string message, IHubContext context)
        {
            //var name = Context.User.Identity.Name;
            using (var db = new ApplicationDbContext())
            {
                var user = db.Useras.Where(c => c.UserName == sendTo && c.SessionId == sessionId).FirstOrDefault();
                if (user == null)
                {
                    // context.Clients.Caller.showErrorMessage("Could not find that user.");
                }
                else
                {
                    db.Entry(user)
                        .Collection(u => u.Connections)
                        .Query()
                        .Where(c => c.Connected == true)
                        .Load();

                    if (user.Connections == null)
                    {
                        //  Clients.Caller.showErrorMessage("The user is no longer connected.");
                    }
                    else
                    {
                        foreach (var connection in user.Connections)
                        {
                            context.Clients.Client(connection.ConnectionID)
                                 .recieverStudentFile(message);
                        }
                    }
                }
            }
        }

        public void SendChatMessageTutorReciever(string sessionId, string sendTo, string message, IHubContext context)
        {
            //var name = Context.User.Identity.Name;
            using (var db = new ApplicationDbContext())
            {
                var user = db.Useras.Where(c => c.UserName == sendTo && c.SessionId == sessionId).FirstOrDefault();
                if (user == null)
                {
                    // context.Clients.Caller.showErrorMessage("Could not find that user.");
                    var session = db.sessions.Where(c => c.SessionID == new Guid(sessionId)).FirstOrDefault();
                    var Username = session.question.student.Username;
                    var img = session.question.student.ProfileImage;

                    var notiAlready = db.notifications.Where(c => c.sessionId == sessionId && c.UserName == sendTo).FirstOrDefault();
                    string link = "Tutors/Sessions"  + "?SessionId=" + sessionId;
                    if (notiAlready == null)
                    {
                        Notifications notify = new Notifications();
                        notify.ID = Guid.NewGuid();
                        notify.isRead = false;
                        notify.Message = session.question.student.ProfileImage + "^" + Username + "^" + "has sent you a message.";
                        notify.UserName = sendTo;
                        notify.sessionId = sessionId;
                        notify.postedTime = DateTime.Now;
                        notify.link = link;
                        notify.counts = 1;
                        db.notifications.Add(notify);

                    }
                    else
                    {
                        notiAlready.counts = notiAlready.counts + 1;
                        notiAlready.isRead = false;
                        notiAlready.postedTime = DateTime.Now;
                        db.Entry(notiAlready).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                    SendNotification(sendTo, Username, session.question.student.ProfileImage, "has sent you a message.", false, "Tutors", "Sessions", "SessionId=" + sessionId);
                }
                else
                {
                    db.Entry(user)
                        .Collection(u => u.Connections)
                        .Query()
                        .Where(c => c.Connected == true)
                        .Load();

                    if (user.Connections == null)
                    {
                            var session = db.sessions.Where(c => c.SessionID == new Guid(sessionId)).FirstOrDefault();
                            var Username = session.question.student.Username;
                            var img = session.question.student.ProfileImage;
                            string link = "Tutors/Sessions" + "?SessionId=" + sessionId;
                            var notiAlready = db.notifications.Where(c => c.sessionId == sessionId && c.UserName == sendTo).FirstOrDefault();
                            if (notiAlready == null)
                            {
                                Notifications notify = new Notifications();
                                notify.ID = Guid.NewGuid();
                                notify.isRead = false;
                                notify.Message = session.question.student.ProfileImage+"^"+Username + "^" + "has sent you a message.";
                                notify.UserName = sendTo;
                                notify.sessionId = sessionId;
                                notify.postedTime = DateTime.Now;
                                notify.link = link;
                                db.notifications.Add(notify);
                             
                            }
                            else
                            {
                                notiAlready.counts = notiAlready.counts + 1;
                                notiAlready.isRead = false;
                                notiAlready.postedTime = DateTime.Now;
                                db.Entry(notiAlready).State = EntityState.Modified;
                        }
                        db.SaveChanges();
                        SendNotification(sendTo, Username, session.question.student.ProfileImage, "has sent you a message.",false, "Tutors", "Sessions", "SessionId=" + sessionId);
                    }

                    else
                    {
                        foreach (var connection in user.Connections)
                        {
                            context.Clients.Client(connection.ConnectionID)
                                 .recieverStudent2(message);
                        }
                        

                    }
                }
            }
        }
        public void SendChatMessageStudentReciever(string sessionId, string sendTo, string message, IHubContext context)
        {
            //var name = Context.User.Identity.Name;
            using (var db = new ApplicationDbContext())
            {
                var user = db.Useras.Where(c => c.UserName == sendTo && c.SessionId == sessionId).FirstOrDefault();
                if (user == null)
                {
                    // context.Clients.Caller.showErrorMessage("Could not find that user.");
                }
                else
                {
                    db.Entry(user)
                        .Collection(u => u.Connections)
                        .Query()
                        .Where(c => c.Connected == true)
                        .Load();

                    if (user.Connections == null)
                    {
                        //  Clients.Caller.showErrorMessage("The user is no longer connected.");
                    }
                    else
                    {
                        foreach (var connection in user.Connections)
                        {
                            context.Clients.Client(connection.ConnectionID)
                                 .recieverStudent(message);
                        }
                    }
                }
            }
        }
        [HttpPost]
        public async Task<ActionResult> UploadChatFile()
        {
            var user = new Guid(User.Identity.GetUserId());

            Guid sessionId = new Guid(Request.Form[1]);
            Guid replyId = new Guid(Request.Form[0]);

            if (!System.IO.Directory.Exists(Server.MapPath("~/UserFiles/Questions/" + sessionId)))
            {
                System.IO.Directory.CreateDirectory(Server.MapPath("~/UserFiles/Questions/" + sessionId));
            }

            if (!System.IO.Directory.Exists(Server.MapPath("~/UserFiles/Questions/" + sessionId+"/"+replyId)))
            {
                System.IO.Directory.CreateDirectory(Server.MapPath("~/UserFiles/Questions/" + sessionId + "/" + replyId));
            }
            string path = "";
            var fileName = "";
            string filestring = replyId + "$";
            string filestringTutor = replyId + "$";

            for (int i = 0; i < Request.Files.Count; i++)
            {
                var file = Request.Files[i];

                if (file != null)
                {
                    fileName = Path.GetFileName(file.FileName);
                    path = Path.Combine(Server.MapPath("~/UserFiles/Questions/" + sessionId + "/" + replyId), fileName);

                    file.SaveAs(path);

                    Files qf = new Files();
                    qf.FileID = Guid.NewGuid();
                    qf.ReplyID = replyId;
                    qf.Path = "~/UserFiles/Questions/" + sessionId + "/" + replyId + "/" + fileName;
                    db.Files.Add(qf);
                    await db.SaveChangesAsync();

                    filestring = filestring + "<br />";
                    filestringTutor= filestringTutor +"<br />";
                    var pathhtml = qf.Path.Split('/');
                    filestring = filestring + "<strong class=\'text-info\'><a target = \'_blank\' href=\'/Students/Download?fileName="+qf.Path+"\'>" + pathhtml[pathhtml.Length - 1] + "</a></strong><br />";
                    filestringTutor= filestringTutor+ "<strong class=\'text-info\'><a target = \'_blank\' href=\'/Tutors/Download?fileName=" + qf.Path + "\'>" + pathhtml[pathhtml.Length - 1] + "</a></strong><br />";

                }

            }

            var context = GlobalHost.ConnectionManager.GetHubContext<TutorStudentChat>();
            var tutorusername = db.sessions.Where(c => c.SessionID == sessionId).FirstOrDefault().tutor.Username;
            SendChatMessageTutorRecieverFile(sessionId.ToString(), tutorusername, filestringTutor, context); //send message to urself 


            return Json(new { result =  filestring });

        }

        [HttpPost]
        public async Task<ActionResult> UploadQuestionFile()
        {
            var user = new Guid(User.Identity.GetUserId());
            if (!System.IO.Directory.Exists(Server.MapPath("~/UserFiles/Questions/" + user)))
            {
                System.IO.Directory.CreateDirectory(Server.MapPath("~/UserFiles/Questions/" + user));
            }
            string path = "";
            var fileName = "";
            Guid QuestionID = new Guid(Request.Form[0]);
            for (int i = 0; i < Request.Files.Count; i++)
            {
                var file = Request.Files[i];

                if (file != null)
                {

                    fileName = Path.GetFileName(file.FileName);
                    path = Path.Combine(Server.MapPath("~/UserFiles/Questions/" + user), fileName);
                  
                    file.SaveAs(path);

                    Files qf = new Files();
                    qf.FileID = Guid.NewGuid();
                    qf.QuestionID = QuestionID;
                    qf.Path = "~/UserFiles/Questions/" + user + "/" + fileName;
                    db.Files.Add(qf);
                    await db.SaveChangesAsync();
                }

            }

            return Json(new { result = "true"});

        }

        public FileResult Download(string fileName)
        {
            var path = Server.MapPath(fileName);
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            var file = fileName.Split('/');
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, file[file.Length - 1]);

            // return File(virtualFilePath, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(virtualFilePath));
        }
        


        [HttpPost]
        public ActionResult UploadProfile()
        {
            var user= new Guid(User.Identity.GetUserId());

            if (!System.IO.Directory.Exists(Server.MapPath("~/Profiles/Students/" + user)))
            {
                System.IO.Directory.CreateDirectory(Server.MapPath("~/Profiles/Students/" + user));
            }
            string path="";
            var fileName="";
            for (int i = 0; i < Request.Files.Count; i++)
            {
                var file = Request.Files[i];

                 fileName = Path.GetFileName(file.FileName);
                   
                    path = Path.Combine(Server.MapPath("~/Profiles/Students/" + user), fileName);
               
                //file.SaveAs(path);
                WebImage img = new WebImage(file.InputStream);

                if (img.Width > 1000)
                    img.Resize(1000, 1000);
                img.Save(path);

                Student loaddb =  db.Students.Find(user);
                loaddb.ProfileImage = "/Profiles/Students/" + user + "/" + fileName;
                db.Entry(loaddb).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(new { result = "/Profiles/Students/" + user+ "/"+fileName });

            }

        public async Task<ActionResult> EditProfile()
        {
            Guid user = new Guid(User.Identity.GetUserId());
            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student student = await db.Students.FindAsync(user);
            StudentUpdateModel stu = Mapper.Map<Student, StudentUpdateModel>(student);
            stu.DateOfBirth = student.DateOfBirth.HasValue ? student.DateOfBirth.Value.ToString("MM/dd/yyyy") : string.Empty;

            if (student == null)
            {
                return HttpNotFound();
            }

            return View(stu);
            
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditProfile([Bind(Include = "FirstName,LastName,DateOfBirth,Country,City,University,Degree,ProfileImage")] StudentUpdateModel student)
        {
            if (ModelState.IsValid)
            {
            
                var userId = new Guid(User.Identity.GetUserId());
              
                Student loaddb = await db.Students.FindAsync(userId);
                
                loaddb.FirstName = student.FirstName;
                loaddb.LastName = student.LastName;
                loaddb.Country = student.Country;
                loaddb.City = student.City;
                loaddb.University = student.University;
                loaddb.Degree = student.Degree;
                //loaddb.ProfileImage = student.ProfileImage;
                if (!String.IsNullOrWhiteSpace(student.DateOfBirth))
                     loaddb.DateOfBirth =Convert.ToDateTime(student.DateOfBirth);
    

                db.Entry(loaddb).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return Json(new { result = "true"});
                //return RedirectToAction("Index");
            }

            return View(student);
        }


        

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
