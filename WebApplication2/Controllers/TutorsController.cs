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
using Microsoft.AspNet.Identity;
using System.IO;
using AutoMapper;
using WebApplication2.DBEntities;
using Microsoft.AspNet.SignalR;
using SignalRChat;
using WebApplication2.App_Start;
using PayPal.Sample;
using PayPal.Api;
using System.Web.Helpers;
using WebApplication2.Helpers;

namespace WebApplication2.Controllers
{
    [CustomAuthorize(Roles = "Tutor")]
    public class TutorsController : BaseController
    {
        private PayPal.Api.Payment payment;
        private ApplicationDbContext db = new ApplicationDbContext();


    
        private PayPal.Api.Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution() { payer_id = payerId };
            this.payment = new PayPal.Api.Payment() { id = paymentId };
            return this.payment.Execute(apiContext, paymentExecution);
        }

        private async Task<bool> isProfileCompleted()
        {
            var LoggedInUserId = new Guid(User.Identity.GetUserId());
            Tutor Tutor = await db.Tutors.FindAsync(LoggedInUserId);
            bool IsCompletedProfile = Tutor.IsCompletedProfile;
            return IsCompletedProfile;
        }

        private void SendNotification(string sendTo, string username, string image, string message,Boolean storeDB)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<notifications>();
            //var name = Context.User.Identity.Name;
            using (var db = new ApplicationDbContext())
            {
                var user = db.notifyConnections.Where(c => c.userName == sendTo).FirstOrDefault();
                if (user != null)
                {
                    string finalmessage = image + "^" + username + "^" + message;
                    if (storeDB == true)
                    { 
                        Notifications notify = new Notifications();
                        notify.ID = Guid.NewGuid();
                        notify.isRead = false;
                        notify.Message = finalmessage;
                        notify.UserName = sendTo;
                        notify.postedTime = DateTime.Now;
                        db.notifications.Add(notify);
                        db.SaveChanges();
                    }

                    var counter = db.notifications.Where(c => c.UserName == sendTo && c.isRead==false).Count();
                    
                    finalmessage = finalmessage + "^" + counter;

                  
                    context.Clients.Client(user.connectionId)
                         .recieverNotifier(finalmessage);

                   
                }
            }
        }
        // GET: Tutors
        public async Task<ActionResult> Index()
        {
           
            bool IsCompletedProfile = await isProfileCompleted();

            Session["noticounter"] = db.notifications.Where(c => c.UserName == User.Identity.Name && c.isRead == false).Count();
            var result = db.notifications.Where(c => c.UserName == User.Identity.Name).OrderByDescending(c => c.postedTime).Take(5);
            Session["notifications"] = result.ToList();

            if (IsCompletedProfile == true)
            {
                var user = new Guid(User.Identity.GetUserId());
                var MineSessions = db.sessions.Where(c => c.TutorID == user && (c.Status == Status.Hired|| c.Status==Status.Conflict)).ToList();
                return View(MineSessions);
            }
            else {
                TempData["isValidate"] = false;
                return RedirectToAction("EditProfile");
            }


        }

        public void DeleteSessionMessageStudent(string sessionId, string sendTo, string message, IHubContext context)
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
        public ActionResult DeleteSession(string sessionId)
        {
            var session = db.sessions.Find(new Guid(sessionId));
            if(session.tutor.Username== User.Identity.Name)
            {

                if (session.Status != Status.Hired)
                {
                  
                    session.isTutorDeleted = true;
                    session.isClosed = true;
                    db.Entry(session).State = EntityState.Modified;
                    db.SaveChanges();

                    var context = GlobalHost.ConnectionManager.GetHubContext<TutorStudentChat>();
                    var student = db.sessions.Where(c => c.SessionID == new Guid(sessionId)).FirstOrDefault().question.student;
                    var username = student.Username;
                    var message = "";
                    DeleteSessionMessageStudent(sessionId, username, message, context);
                    SendNotification(username, session.tutor.Username,session.tutor.ProfileImage, "Session has been closed.",false);
                }



            }

           return RedirectToAction("inbox");

        }

        public async Task<ActionResult> Inbox()
        {
            bool IsCompletedProfile = await isProfileCompleted();
            if (IsCompletedProfile == true)
            {
                var user = new Guid(User.Identity.GetUserId());
                var MineSessions = db.sessions.Where(c => c.TutorID == user && c.isTutorDeleted==false).ToList();
                TutorInbox model = new TutorInbox();
                model.sessions = MineSessions;
                var onlineusers = db.online.Where(c => c.Status == true).ToList();
                foreach (var v in MineSessions)
                {
                    var online = onlineusers.Where(c => c.Username == v.question.student.Username).FirstOrDefault();
                    var result= online == null ? false : online.Status;
                    model.online.Add(result);
                }
                return View(model);
            }
            else {
                TempData["isValidate"] = false;
                return RedirectToAction("EditProfile");
            }
        }
     

        [HttpPost]
        public ActionResult UploadProfile()
        {
            var user = new Guid(User.Identity.GetUserId());
            if (!System.IO.Directory.Exists(Server.MapPath("~/Profiles/Tutors/" + user)))
            {
                System.IO.Directory.CreateDirectory(Server.MapPath("~/Profiles/Tutors/" + user));
            }
            string path = "";
            var fileName = "";
            for (int i = 0; i < Request.Files.Count; i++)
            {
                var file = Request.Files[i];

                fileName = Path.GetFileName(file.FileName);

                path = Path.Combine(Server.MapPath("~/Profiles/Tutors/" + user), fileName);
                //file.SaveAs(path);

                WebImage img = new WebImage(file.InputStream);

                if (img.Width > 1000)
                    img.Resize(1000, 1000);
                img.Save(path);


                Tutor loaddb = db.Tutors.Find(user);
                loaddb.ProfileImage = "/Profiles/Tutors/" + user + "/" + fileName;
                db.Entry(loaddb).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(new { result = "/Profiles/Tutors/" + user + "/" + fileName });

        }


        public async Task<ActionResult> PostedRequests()
        {
            bool IsCompletedProfile =  await isProfileCompleted();
            if (IsCompletedProfile == true)
            {
                var postedRequests = db.Questions.Where(c => c.TutorID == null).ToList();
                IEnumerable<QuestionViewModel> postedQuestions = Mapper.Map<IEnumerable<Question>, IEnumerable<QuestionViewModel>>(postedRequests);
               
                return View(postedQuestions);
            }
            else
            {
                TempData["isValidate"] = false;
                return RedirectToAction("EditProfile");
            }

        }

        [HttpGet]
        public async Task<ActionResult> Sessions(Guid SessionId)
        {
            var session = await db.sessions.FindAsync(SessionId);

            if (session.tutor.Username == User.Identity.Name)
            {

                ChatModel obj = new ChatModel();
                obj.session = session;
                obj.status = db.online.Where(c => c.Username == session.question.student.Username).FirstOrDefault().Status;

                obj.session.Replies = obj.session.Replies.OrderBy(c => c.PostedTime).ToList();
                obj.offer.amount = obj.session.OfferedFees;

                return View(obj);
            }
            else
              return   RedirectToAction("Unauthorized", "Home", "");
        }


        [ValidateAntiForgeryToken]
        public async Task<ActionResult> QuestionDetails(Guid? PostId)
        {
            var userId = new Guid(User.Identity.GetUserId());
            var QuestionQuery = db.Questions.Where(c => c.QuestionID == PostId) ;
            var postedQuestion = QuestionQuery.FirstOrDefault();
            var selectedSession = postedQuestion.Sessions.Where(c => c.TutorID.Value == userId).FirstOrDefault();
            var selectedStudent = postedQuestion.student;
            var selectedTutor =selectedSession==null? await db.Tutors.FindAsync(userId) : selectedSession.tutor;
            TutorQuestionDetails chatView = new TutorQuestionDetails();
            chatView.session = selectedSession;
            chatView.tutor = selectedTutor;
            chatView.student = selectedStudent;
            chatView.question = postedQuestion;
            chatView.QuestionID = PostId.Value;
            chatView.sessionCount = postedQuestion.Sessions.Count;
        
            return View(chatView);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> QuestionsReply(TutorQuestionDetails reply)
        {
            var userId= new Guid(User.Identity.GetUserId());
            var question = db.Questions.Where(c => c.QuestionID == reply.QuestionID);
            var postedQuestion = question.FirstOrDefault();
            var selectedSession = postedQuestion.Sessions.Where(c => c.TutorID.Value == userId).FirstOrDefault();
            if (selectedSession == null)
            { 
                Session obj = new Session();
                obj.SessionID = Guid.NewGuid();
                obj.TutorID = userId;
                //obj.StudentID = reply.StudentID;
                obj.QuestionID = reply.QuestionID;
                obj.PostedTime = DateTime.Now;
                obj.Status = Status.Posted;
                db.sessions.Add(obj);
            
                Reply rep = new Reply();
                rep.ReplyID =  Guid.NewGuid();
                rep.SessionID = obj.SessionID;
                rep.ReplierID = obj.TutorID.Value;
                rep.PostedTime = DateTime.Now;
                reply.replyDetails= reply.replyDetails.Replace(Environment.NewLine, "<br/>");
                rep.Details = reply.replyDetails;
                db.Replies.Add(rep);
                await db.SaveChangesAsync();

                SendNotification(postedQuestion.student.Username, obj.tutor.Username, obj.tutor.ProfileImage, "Replied to your question.",false);

                return new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = rep.ReplyID + "$" + rep.SessionID }
                };
            }
            else
            { 
                return new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "null" }
                };
            }
        }

       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Chat(ChatModel reply)
        {
            Reply obj = new Reply();
            obj.ReplyID = Guid.NewGuid();
            obj.ReplierID = new Guid(User.Identity.GetUserId());
            obj.SessionID = reply.sessionID;
            obj.PostedTime = DateTime.Now;
            reply.replyDetail=reply.replyDetail.Replace(Environment.NewLine, "<br/>");
            obj.Details = reply.replyDetail;
            db.Replies.Add(obj);

            var session = db.sessions.Where(c => c.SessionID == obj.SessionID).FirstOrDefault();
            session.NewMessageStudent = true;
            db.Entry(session).State = EntityState.Modified;
            
            await db.SaveChangesAsync();

            var context = GlobalHost.ConnectionManager.GetHubContext<TutorStudentChat>();
            var username = User.Identity.Name;
           
            //part moved up
            var student = db.sessions.Where(c => c.SessionID == obj.SessionID).FirstOrDefault().question.student;
            var username2 = student.Username;
            var status = db.online.Where(c => c.Username == username2).FirstOrDefault().Status;

            var imgsrc = db.Tutors.Where(c => c.Username == username).FirstOrDefault().ProfileImage;
            string message = generateMessage(username, obj.Details, imgsrc, obj.PostedTime.ToString(),obj.ReplyID.ToString(), status);
            SendChatTutorReciever(obj.SessionID.ToString(),username, message, context); //send message to urself 

            
            SendChatStudentReiever(obj.SessionID.ToString(), username2, message, context); //send message to other person 
            //context.Clients.All.test("hello world");

            return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { result = obj.ReplyID + "$" + obj.SessionID }
            };

        }

        public string generateMessage(string username,string detail,string imgsrc,string postedTime, string replyID,bool online)
        {
            string filestring = "<div id=\"" + replyID + "\"></div>";
            string message = "";
            message=message+"<li class=\""+online+"\"></li><li class=\"media\">" +
                            "<div class=\"comment\"> " +
                                    "<a href=\"#\" class=\"pull-left\"><img src=\"" + imgsrc + "\" alt=\"\" class=\"img-circle imgSize\"> </a>" +
                                     " <div class=\"media-body\">" +
                                     " <strong class=\"text-success userText username\">" + username + "</strong><br /><br />" +
                                       detail +
                                      filestring +
                                     "<div class=\"clearfix\"></div>" +
                                     " </div>" +
                                    "<div style=\"margin-bottom:20px\">" +
                                              "<small class=\"text-muted pull-right\">" + postedTime + "</small>" +
                                       " </div>" +
                                     "<hr>" +
                               "</div>" +
                               "</li>";
            return message;
        }
        public void SendChatTutorReciever(string sessionId,string sendTo, string message,IHubContext context)
        {
            //var name = Context.User.Identity.Name;
            using (var db = new ApplicationDbContext())
            {
                var user = db.Useras.Where(c=>c.UserName==sendTo && c.SessionId==sessionId).FirstOrDefault();
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
                                .recieverTutor2(message);
                        }
                    }
                }
            }
        }
        public void SendChatStudentReiever(string sessionId, string sendTo, string message, IHubContext context)
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
                            var session = db.sessions.Where(c => c.SessionID == new Guid(sessionId)).FirstOrDefault();
                            var Username = session.tutor.Username;
                            var img = session.tutor.ProfileImage;

                            var notiAlready = db.notifications.Where(c => c.sessionId == sessionId && c.UserName == sendTo).FirstOrDefault();
                            if (notiAlready == null)
                            {
                                Notifications notify = new Notifications();
                                notify.ID = Guid.NewGuid();
                                notify.isRead = false;
                                notify.Message =     session.question.student.ProfileImage + "^" +Username+ "^"+ "has sent you a message.";
                                notify.UserName = sendTo;
                                notify.sessionId = sessionId;
                                notify.postedTime = DateTime.Now;
                                db.notifications.Add(notify);
                            }
                            else
                            {
                                notiAlready.counts = notiAlready.counts + 1;
                                notiAlready.isRead = false;
                                db.Entry(notiAlready).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            SendNotification(sendTo, session.tutor.Username, session.tutor.ProfileImage, "has sent you a message.",false);
                    }
                    else
                    {
                        foreach (var connection in user.Connections)
                        {
                            context.Clients.Client(connection.ConnectionID)
                                 .recieverTutor(message);
                        }
                    }
                }
            }
        }

        public void SendChatStudentReieverFile(string sessionId, string sendTo, string message, IHubContext context)
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
                                 .recieverTutorFile(message);
                        }
                    }
                }
            }
        }



        [HttpPost]
        public async Task<ActionResult> UploadQuestionFile()
        {
            var user = new Guid(User.Identity.GetUserId());

            Guid sessionId = new Guid(Request.Form[1]);
            Guid replyId = new Guid(Request.Form[0]);

            if (!System.IO.Directory.Exists(Server.MapPath("~/UserFiles/Questions/" + sessionId)))
            {
                System.IO.Directory.CreateDirectory(Server.MapPath("~/UserFiles/Questions/" + sessionId));
            }

            if (!System.IO.Directory.Exists(Server.MapPath("~/UserFiles/Questions/" + sessionId + "/" + replyId)))
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
                    filestringTutor = filestringTutor + "<br />";
                    var pathhtml = qf.Path.Split('/');
                    filestring = filestring + "<strong class=\'text-info\'><a target = \'_blank\' href=\'/Students/Download?fileName=" + qf.Path + "\'>" + pathhtml[pathhtml.Length - 1] + "</a></strong><br />";
                    filestringTutor = filestringTutor + "<strong class=\'text-info\'><a target = \'_blank\' href=\'/Tutors/Download?fileName=" + qf.Path + "\'>" + pathhtml[pathhtml.Length - 1] + "</a></strong><br />";

                }

            }
            var context = GlobalHost.ConnectionManager.GetHubContext<TutorStudentChat>();
            var StudentUsername = db.sessions.Where(c => c.SessionID == sessionId).FirstOrDefault().question.student.Username;
            SendChatStudentReieverFile(sessionId.ToString(), StudentUsername, filestring, context); //send message to urself 


            return Json(new { result = filestringTutor });

            }

        public FileResult Download(string fileName)
        {
            var path = Server.MapPath(fileName);
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            var file = fileName.Split('/');
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, file[file.Length-1]);

           // return File(virtualFilePath, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(virtualFilePath));
        }


        private MultiSelectList GetCategories(string[] selectedValues)
        {
            
            return new MultiSelectList(db.Categories, "CategoryID", "CategoryName", selectedValues);

        }

        public async Task<ActionResult> EditProfile()
        {
            Guid user = new Guid(User.Identity.GetUserId());
            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tutor tutor = await db.Tutors.FindAsync(user);
            TutorUpdateModel tmodel = Mapper.Map<Tutor, TutorUpdateModel>(tutor);
            
            if (tutor == null)
            {
                return HttpNotFound();
            }
            // ViewBag.ExpertiseVal = new SelectList(db.Categories, "CategoryID", "CategoryName", t.Expertise);
            // ViewBag.ExpertiseVal = GetCategories(t.Expertise);
            ViewBag.Expertise = new MultiSelectList(db.Categories, "CategoryID", "CategoryName", tmodel.Expertise);
            ViewBag.isValidated = TempData["isValidate"] == null ? true : TempData["isValidate"];
            return View(tmodel);

        }


        // POST: Students/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditProfile([Bind(Include = "FirstName,LastName,DOB,Country,City,University,Degree,AboutMe,Experience,ProfileImage,Expertise")] TutorUpdateModel tutor)
        {
            if (ModelState.IsValid)
            {

                var userId = new Guid(User.Identity.GetUserId());

                Tutor loaddb = await db.Tutors.FindAsync(userId);

                loaddb.FirstName = tutor.FirstName;
                loaddb.LastName = tutor.LastName;
                loaddb.Country = tutor.Country;
                loaddb.City = tutor.City;
                loaddb.AboutMe = tutor.AboutMe;
                loaddb.Experience = tutor.Experience;
                loaddb.University = tutor.University;
                loaddb.Degree= tutor.Degree;
                //loaddb.ProfileImage = tutor.ProfileImage;
                if (!String.IsNullOrWhiteSpace(tutor.DOB))
                    loaddb.DateOfBirth = Convert.ToDateTime(tutor.DOB);

                //look this thing later.


                //deleting added items
                IEnumerable<TutorExperties> obj = loaddb.tutorExperties.AsEnumerable();
                foreach (var category in obj.ToList())
                {
                    bool result = tutor.Expertise.Contains(category.CategoryID.ToString());
                    if (result == false)
                    {
                        var removedExpertise = db.TutorsExpertise.Where(c=>c.CategoryID==category.CategoryID && c.TutorID==category.TutorID).FirstOrDefault();
                        db.TutorsExpertise.Remove(removedExpertise);
                    }

                }
               
                foreach (var category in tutor.Expertise)
                {
                    var result = obj.Where(c => c.CategoryID == new Guid(category)).ToList();
                    if (result.Count == 0) //dont exist add it
                        db.TutorsExpertise.Add(new TutorExperties { TutorID = loaddb.TutorID, CategoryID = new Guid(category) });

                }

                loaddb.IsCompletedProfile = true;
                db.Entry(loaddb).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tutor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SendOffer( Offer question)
        {
            var user = User.Identity.GetUserId();
            var sessionId = question.SessionId;
            var session = db.sessions.Where(c => c.SessionID ==sessionId && c.TutorID.Value ==new Guid(user)).First();
            if(session.Status==Status.Posted || session.Status == Status.Offered)
            { 
                session.OfferedFees = question.amount;
                session.Status = Status.Offered;
                db.Entry(session).State = EntityState.Modified;

                Reply obj = new Reply();
                obj.ReplyID = Guid.NewGuid();
                obj.ReplierID = new Guid(User.Identity.GetUserId());
                obj.SessionID = sessionId;
                obj.PostedTime = DateTime.Now;
                obj.Details = " Automatically Generated Message: I have sent offer to do your work for " + question.amount + "$. Press Hire Button if you are interested in my services.";
                session.Replies.Add(obj);
                db.SaveChanges();

                var context = GlobalHost.ConnectionManager.GetHubContext<TutorStudentChat>();
                //send message reply
                var username = User.Identity.Name;
                var imgsrc = db.Tutors.Where(c => c.Username == username).FirstOrDefault().ProfileImage;
                string message = generateMessage(username, obj.Details, imgsrc, obj.PostedTime.ToString(), obj.ReplyID.ToString(),false);
                SendChatTutorReciever(obj.SessionID.ToString(), username, message, context); //send message to urself 

                var student = db.sessions.Where(c => c.SessionID == obj.SessionID).FirstOrDefault().question.student;
                var username2 = student.Username;
                SendChatStudentReiever(obj.SessionID.ToString(), username2, message, context); //send message to other person 
               
                //send button update
                var message2 = "<button type=\"button\" id=\"offer\" class=\"btn btn-primary\" data-toggle=\"modal\" data-target=\"#hireNewModal\">Hire for ("+question.amount+"$)</button>";
                SendButtonStudent(sessionId.ToString(), username2, message2, context); //send message to other person 
                SendNotification(username2, username, imgsrc, "I have offered my services for $"+question.amount,true);

                try
                {

                    string body;
                    using (var sr = new StreamReader(Server.MapPath("\\Helpers\\") + "passwordreset.html"))
                    {
                        body = sr.ReadToEnd();
                    }


                    var email = db.Users.Where(c => c.Id == session.question.StudentID.ToString()).FirstOrDefault().Email;
                    Mailer mailer = new Mailer();
                    mailer.ToEmail = email;
                    mailer.Subject = "Tutor Offered service on MezoExperts.com";
                    mailer.Body = string.Format(body, username + " has offered to do your job for $"+question.amount);
                    mailer.IsHtml = true;
                    mailer.Send();


                }
                catch (Exception e)
                {

                }

                return new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = question.amount }
                };
            }
            else
            {
                return new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "" }
                };
            }

        }

  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Invoice(Offer question)
        {
            var user = User.Identity.GetUserId();
            var sessionId = question.SessionId;
            var session = db.sessions.Where(c => c.SessionID == sessionId && c.TutorID.Value == new Guid(user)).First();
            if (session.Status == Status.Hired || session.Status==Status.Invoiced)
            {
                session.OfferedFees = question.amount;
                session.Status = Status.Invoiced;
                db.Entry(session).State = EntityState.Modified;

                Reply obj = new Reply();
                obj.ReplyID = Guid.NewGuid();
                obj.ReplierID = new Guid(User.Identity.GetUserId());
                obj.SessionID = sessionId;
                obj.PostedTime = DateTime.Now;
                obj.Details = " Automatically Generated Message: I have sent Invoice for the work for $" + question.amount + ". Press Accept Button if you are satisfied with the services and pay to tutor. Pressing Reject button will cause the admin to decide the dispute.";
                session.Replies.Add(obj);

                db.SaveChanges();

                var context = GlobalHost.ConnectionManager.GetHubContext<TutorStudentChat>();
                //send message reply
                var username = User.Identity.Name;
                var imgsrc = db.Tutors.Where(c => c.Username == username).FirstOrDefault().ProfileImage;
                string message = generateMessage(username, obj.Details, imgsrc, obj.PostedTime.ToString(), obj.ReplyID.ToString(),false);
                SendChatTutorReciever(obj.SessionID.ToString(), username, message, context); //send message to urself 

                var student = db.sessions.Where(c => c.SessionID == obj.SessionID).FirstOrDefault().question.student;
                var username2 = student.Username;
                SendChatStudentReiever(obj.SessionID.ToString(), username2, message, context); //send message to other person 

                //send button
                
                var message2 = "<button type =\"button\" id=\"accept\" class=\"btn btn-primary\" data-toggle=\"modal\" data-target=\"#approveNewModal\" style=\"margin-right:5px\">Accept </button>";
                message2 = message2 + "<button type =\"button\" id=\"reject\" class=\"btn  btn-primary\" data-toggle=\"modal\" data-target=\"#rejectNewModal\">Reject </button>";

                   SendButtonStudent(sessionId.ToString(), username2, message2, context); //send message to other person 
                   SendNotification(username2, username, imgsrc, "I have sent Invoice of $" + question.amount,true);
                try
                {

                    string body;
                    using (var sr = new StreamReader(Server.MapPath("\\Helpers\\") + "passwordreset.html"))
                    {
                        body = sr.ReadToEnd();
                    }
                    
                    var email = db.Users.Where(c => c.Id == session.question.StudentID.ToString()).FirstOrDefault().Email;
                    Mailer mailer = new Mailer();
                    mailer.ToEmail = email;
                    mailer.Subject = "Tutor sent Invoice on MezoExperts.com";
                    mailer.Body = string.Format(body, username + " has sent invoice for $" + question.amount);
                    mailer.IsHtml = true;
                    mailer.Send();


                }
                catch (Exception e)
                {

                }

                return new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = question.amount }
                };
            }
            else
            {
                return new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "" }
                };
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Payment(Models.Payment model)
        {
            //Dictionary<string, string> payPalConfig = new Dictionary<string, string>();
            //payPalConfig.Add("mode", "sandbox");
            //OAuthTokenCredential tokenCredential = new AuthTokenCredential("AXVrBytGm6RdmOYfcUFM-VoOa8TvQhVYN6-TapoUzU2oErEpO0XzbYn8qD26R3iFduECZqOQmB78bZbS", "EGz3u0h-poL7i3MbLUQQXgqiYPEbbdzX95h57JzlnKrjKRV1-MNLPApqgKt30Y7VWmgwb5UxFWja0__2", payPalConfig);
            //string accessToken = tokenCredential.GetAccessToken();
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
                    var baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Tutors/AccountSettings?";
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
            bool IsCompletedProfile = await isProfileCompleted();

            if (IsCompletedProfile == true)
            {
                Tutor loaddb = db.Tutors.Where(c => c.Username == User.Identity.Name).FirstOrDefault();
                Models.Payment obj = new Models.Payment();
                obj.Balance = db.Tutors.Where(c => c.Username == User.Identity.Name).FirstOrDefault().CurrentEarning.ToString();
                obj.Email = loaddb.paypalEmail;
                obj.Requested = loaddb.moneyStatus=="Requested"?true:false;
                return View(obj);
            }
            else
            {
                TempData["isValidate"] = false;
                return RedirectToAction("EditProfile");
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public  ActionResult updatePaypal(Models.Payment model)
        {
            Tutor loaddb = db.Tutors.Where(c => c.Username == User.Identity.Name).FirstOrDefault();
            loaddb.paypalEmail = model.Email;
            db.Entry(loaddb).State = EntityState.Modified;
            db.SaveChangesAsync();
            return View("AccountSettings",model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult requestPayment(Models.Payment model)
        {
            Tutor loaddb =  db.Tutors.Where(c => c.Username == User.Identity.Name).FirstOrDefault();
            loaddb.moneyStatus = "Requested";            
            db.Entry(loaddb).State = EntityState.Modified;
            db.SaveChanges();

            PaymentRequests obj = new PaymentRequests();
            obj.PaymentId = new Guid();
            obj.amount = loaddb.CurrentEarning;
            obj.status = "Requested";
            obj.postedTime = DateTime.Now;
            obj.UserName = loaddb.Username;
            db.paymentRequests.Add(obj);
            db.SaveChanges();


            return View("AccountSettings", model);
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
