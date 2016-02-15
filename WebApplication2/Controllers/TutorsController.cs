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

namespace WebApplication2.Controllers
{
    [System.Web.Mvc.Authorize(Roles = "Tutor")]
    public class TutorsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private async Task<bool> isProfileCompleted()
        {
            var LoggedInUserId = new Guid(User.Identity.GetUserId());
            Tutor Tutor = await db.Tutors.FindAsync(LoggedInUserId);
            bool IsCompletedProfile = Tutor.IsCompletedProfile;
            return IsCompletedProfile;
        }

        // GET: Tutors
        public async Task<ActionResult> Index()
        {
           
            bool IsCompletedProfile = await isProfileCompleted();

            //if (IsCompletedProfile == true)
            //{
            //    var postedRequests = db.Questions.Where(c => c.TutorID == null).ToList();
            //    IEnumerable<QuestionViewModel> postedQuestions = Mapper.Map<IEnumerable<Question>, IEnumerable<QuestionViewModel>>(postedRequests);
            //    TutorHome model = new TutorHome();
            //    model.ActiveJobs = postedQuestions;
            //    model.CompledJobs = postedQuestions;
            //    return View(model);
            //}

           
            if (IsCompletedProfile == true)
            {
                var user = new Guid(User.Identity.GetUserId());
                var MineSessions = db.sessions.Where(c => c.TutorID == user).ToList();
                return View(MineSessions);
            }
            else {
                TempData["isValidate"] = false;
                return RedirectToAction("EditProfile");
            }

        }


        public async Task<ActionResult> Inbox()
        {
            bool IsCompletedProfile = await isProfileCompleted();
            if (IsCompletedProfile == true)
            {
                var user = new Guid(User.Identity.GetUserId());
                var MineSessions = db.sessions.Where(c => c.TutorID == user).ToList();
                return View(MineSessions);
            }
            else {
                TempData["isValidate"] = false;
                return RedirectToAction("EditProfile");
            }
        }
        #region Admin Functionality
        // GET: Tutors
        public async Task<ActionResult> Manage()
        {
            return View(await db.Tutors.ToListAsync());
        }

        // GET: Tutors/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tutor tutor = await db.Tutors.FindAsync(id);
            if (tutor == null)
            {
                return HttpNotFound();
            }
            return View(tutor);
        }

        // GET: Tutors/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Tutors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "TutorID,FirstName,LastName,DateOfBirth,Degree,University,AboutMe,City,Country,DateCreated,CurrentEarning")] Tutor tutor)
        {
            if (ModelState.IsValid)
            {
                tutor.TutorID = Guid.NewGuid();
                db.Tutors.Add(tutor);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(tutor);
        }

        // GET: Tutors/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tutor tutor = await db.Tutors.FindAsync(id);
            if (tutor == null)
            {
                return HttpNotFound();
            }
            return View(tutor);
        }

        // POST: Tutors/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "TutorID,FirstName,LastName,DateOfBirth,Degree,University,AboutMe,City,Country,DateCreated,CurrentEarning")] Tutor tutor)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tutor).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(tutor);
        }

        // GET: Tutors/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tutor tutor = await db.Tutors.FindAsync(id);
            if (tutor == null)
            {
                return HttpNotFound();
            }
            return View(tutor);
        }

        // POST: Tutors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            Tutor tutor = await db.Tutors.FindAsync(id);
            db.Tutors.Remove(tutor);
            await db.SaveChangesAsync();
            return RedirectToAction("Manage");
        }
        #endregion

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
                file.SaveAs(path);

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
  
            ChatModel obj = new ChatModel();
            obj.session = session;
            obj.session.Replies = obj.session.Replies.OrderBy(c => c.PostedTime).ToList();
            
            return View(obj);
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
                db.sessions.Add(obj);
            
                Reply rep = new Reply();
                rep.ReplyID =  Guid.NewGuid();
                rep.SessionID = obj.SessionID;
                rep.ReplierID = obj.TutorID.Value;
                rep.PostedTime = DateTime.Now;
                rep.Details = reply.replyDetails;
                db.Replies.Add(rep);
                await db.SaveChangesAsync();
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
            obj.Details = reply.replyDetail;

            db.Replies.Add(obj);
            await db.SaveChangesAsync();

            var context = GlobalHost.ConnectionManager.GetHubContext<TutorStudentChat>();
            var username = User.Identity.Name;
            var imgsrc = db.Tutors.Where(c => c.Username == username).FirstOrDefault().ProfileImage;
            string message = generateMessage(username, obj.Details, imgsrc, obj.PostedTime.ToString(),obj.ReplyID.ToString());
            SendChatTutorReciever(obj.SessionID.ToString(),username, message, context); //send message to urself 

            var student = db.sessions.Where(c => c.SessionID == obj.SessionID).FirstOrDefault().question.student;
            var username2 = student.Username;
            SendChatStudentReiever(obj.SessionID.ToString(), username2, message, context); //send message to other person 
            //context.Clients.All.test("hello world");

            return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { result = obj.ReplyID + "$" + obj.SessionID }
            };

        }

        public string generateMessage(string username,string detail,string imgsrc,string postedTime, string replyID)
        {
            string filestring = "<div id=\"" + replyID + "\"></div>";
            string message = "";
            message=message+"<li class=\"media\">" +
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
                        //  Clients.Caller.showErrorMessage("The user is no longer connected.");
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
