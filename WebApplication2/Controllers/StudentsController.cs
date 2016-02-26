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

namespace WebApplication2.Controllers
{
    [CustomAuthorize(Roles = "Student,Admin")]
    public class StudentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private PayPal.Api.Payment payment;
       
        private PayPal.Api.Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution() { payer_id = payerId };
            this.payment = new PayPal.Api.Payment() { id = paymentId };
            return this.payment.Execute(apiContext, paymentExecution);
        }


        public async Task<ActionResult> Manage()
        {
            //  ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View(await db.Students.ToListAsync());
        }

        [HttpGet]
        public async Task<ActionResult>Sessions(Guid SessionId)
        {
            var session=await db.sessions.FindAsync(SessionId);
            ChatModel obj = new ChatModel();
            obj.session = session;
            obj.session.Replies=obj.session.Replies.OrderBy(c => c.PostedTime).ToList();
            return  View(obj);
        }

        public async Task<ActionResult> Inbox()
        {
            var user = new Guid(User.Identity.GetUserId());
            var MineSessions = db.Questions.Where(c => c.StudentID == user).ToList();
            List<StudentInbox> list = new List<StudentInbox>();
            foreach (var question in MineSessions)
            {
                foreach (var session in question.Sessions)
                {
                    StudentInbox obj = new StudentInbox();
                    obj.SenderName = session.tutor.Username;
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
                    // These URLs will determine how the user is redirected from PayPal once they have either approved or canceled the payment.
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
            foreach (var question in MineSessions)
            {
                var hiredSession = question.Sessions.Where(c => c.Status == Status.Hired || c.Status== Status.Conflict);
                foreach (var session in hiredSession)
                {
                    StudentInbox obj = new StudentInbox();
                    obj.SenderName = session.tutor.Username;
                    obj.SenderProfile = session.tutor.ProfileImage;
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

            return View(model);
        }

        public ActionResult PostQuestion(string id)
        {
            if(id!=null)
            {
                var tutorId = db.Tutors.Where(c=>c.TutorID==new Guid(id)).First();
                ViewBag.Username = tutorId.Username;
                ViewBag.Id = id;
            }
            
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View();
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

                if(question.TutorID!=null)
                {
                    Session obj = new Session();
                    obj.SessionID = Guid.NewGuid();
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
                string message = generateMessage(username, obj.Details, imgsrc, obj.PostedTime.ToString(), obj.ReplyID.ToString());
                SendChatMessageStudentReciever(obj.SessionID.ToString(), username, message, context); //send message to urself 
                var tutor = db.sessions.Where(c => c.SessionID == obj.SessionID).FirstOrDefault().tutor;
                var username2 = tutor.Username;
                SendChatMessageTutorReciever(obj.SessionID.ToString(), username2, message, context); //send message to other person 

                var message2 = " <button type=\"button\" id=\"rejected\" disabled class=\"btn btn-primary\">Rejected Invoice</button>";

                SendButtonStudent(sessionId.ToString(), username2, message2, context); //send message to other person 

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

                db.SaveChanges();


                var context = GlobalHost.ConnectionManager.GetHubContext<TutorStudentChat>();
                var username = User.Identity.Name;
                var imgsrc = db.Students.Where(c => c.Username == username).FirstOrDefault().ProfileImage;
                string message = generateMessage(username, obj.Details, imgsrc, obj.PostedTime.ToString(), obj.ReplyID.ToString());
                SendChatMessageStudentReciever(obj.SessionID.ToString(), username, message, context); //send message to urself 
                var tutor = db.sessions.Where(c => c.SessionID == obj.SessionID).FirstOrDefault().tutor;
                var username2 = tutor.Username;
                SendChatMessageTutorReciever(obj.SessionID.ToString(), username2, message, context); //send message to other person 

                var message2 = " <button type=\"button\" id=\"accepted\" disabled class=\"btn btn-primary\">Accepted Invoice</button>";

                SendButtonStudent(sessionId.ToString(), username2, message2, context); //send message to other person 


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
                db.Entry(quest).State = EntityState.Modified;
                db.Entry(session).State = EntityState.Modified;

                Reply obj = new Reply();
                obj.ReplyID = Guid.NewGuid();
                obj.ReplierID = new Guid(User.Identity.GetUserId());
                obj.SessionID = sessionId;
                obj.PostedTime = DateTime.Now;
                obj.Details = " Automatically Generated Message: I have Hired you for " + session.OfferedFees + "$. You can start working on task." ;
                session.Replies.Add(obj);

                

                db.SaveChanges();

           
                var context = GlobalHost.ConnectionManager.GetHubContext<TutorStudentChat>();
                var username = User.Identity.Name;
                var imgsrc = db.Students.Where(c => c.Username == username).FirstOrDefault().ProfileImage;
                string message = generateMessage(username, obj.Details, imgsrc, obj.PostedTime.ToString(), obj.ReplyID.ToString());
                SendChatMessageStudentReciever(obj.SessionID.ToString(), username, message, context); //send message to urself 
                var tutor = db.sessions.Where(c => c.SessionID == obj.SessionID).FirstOrDefault().tutor;
                var username2 = tutor.Username;
                SendChatMessageTutorReciever(obj.SessionID.ToString(), username2, message, context); //send message to other person 

                //send button
               
                var message2 = "  <button type=\"button\" id=\"hire\" class=\"btn btn-primary\" data-toggle=\"modal\" data-target=\"#invoiceNewModal\">Send Invoice</button>";
                                                                                       
                SendButtonStudent(sessionId.ToString(), username2, message2, context); //send message to other person 


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
                obj.Details = reply.replyDetail;
            
                db.Replies.Add(obj);

                var session = db.sessions.Where(c => c.SessionID == obj.SessionID).FirstOrDefault();
                session.NewMessageTutor = true;
                db.Entry(session).State = EntityState.Modified;

                await db.SaveChangesAsync();

                var context = GlobalHost.ConnectionManager.GetHubContext<TutorStudentChat>();
                var username = User.Identity.Name;
                var imgsrc = db.Students.Where(c => c.Username == username).FirstOrDefault().ProfileImage;
                string message = generateMessage(username, obj.Details, imgsrc, obj.PostedTime.ToString(),obj.ReplyID.ToString());
                SendChatMessageStudentReciever(obj.SessionID.ToString(), username, message, context); //send message to urself 
                var tutor = db.sessions.Where(c => c.SessionID == obj.SessionID).FirstOrDefault().tutor;
                var username2 = tutor.Username;
                SendChatMessageTutorReciever(obj.SessionID.ToString(), username2, message, context); //send message to other person 

            return new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = obj.ReplyID +"$" + obj.SessionID }
                };
           
        }

        public string generateMessage(string username, string detail, string imgsrc,string postedTime,string replyID)
        {
             string filestring = "<div id=\""+replyID+"\"></div>";
          
            string message = "";
            message = message + "<li class=\"media\">" +
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

        /*
        // GET: Students
        public async Task<ActionResult> Index()
        {
            return View(await db.Students.ToListAsync());
        }*/

        // GET: Students/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student student = await db.Students.FindAsync(id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        // GET: Students/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "StudentID,FirstName,LastName,DateOfBirth,Degree,University,AboutMe,City,Country,DateCreated,CurrentBalance")] Student student)
        {
            if (ModelState.IsValid)
            {
                student.StudentID = Guid.NewGuid();
                db.Students.Add(student);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(student);
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
                file.SaveAs(path);

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



        // GET: Students/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student student = await db.Students.FindAsync(id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "StudentID,FirstName,LastName,DateOfBirth,Degree,University,AboutMe,City,Country,DateCreated,CurrentBalance")] Student student)
        {
            if (ModelState.IsValid)
            {
                db.Entry(student).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(student);
        }

        // GET: Students/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student student = await db.Students.FindAsync(id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            Student student = await db.Students.FindAsync(id);
            db.Students.Remove(student);
            await db.SaveChangesAsync();
            return RedirectToAction("Manage");
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
