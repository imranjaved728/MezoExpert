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

namespace WebApplication2.Controllers
{
    [System.Web.Mvc.Authorize(Roles = "Student,Admin")]
    public class StudentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

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
                    var lastreply = session.Replies.OrderBy(c => c.PostedTime);
                    obj.PostedTime = lastreply.FirstOrDefault().PostedTime;
                    obj.SessionId = session.SessionID;
                    obj.Status = question.Status;
                    obj.LastMessage = lastreply.LastOrDefault().Details;
                    list.Add(obj);

                }
            }
            return View(list);
           
        }

        public ActionResult Index()
        {
           
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Index([Bind(Include = "QuestionID,StudentID,TutorID,Title,Details,Status,Amount,CategoryID,DueDate,PostedTime")] QuestionViewModel question)
        {
            if (ModelState.IsValid)
            {
                Question quest = Mapper.Map<QuestionViewModel, Question>(question);
                quest.QuestionID = Guid.NewGuid();
                quest.PostedTime = DateTime.Now;
                //initial posted Question Status
                quest.Status = "Posted";
               
                //user posting question id
                quest.StudentID = new Guid(User.Identity.GetUserId());
                db.Questions.Add(quest);
                await db.SaveChangesAsync();
                //try
                //{
                //    if (!System.IO.Directory.Exists(Server.MapPath("~/UserFiles/Q" + quest.QuestionID)))
                //    {
                //        System.IO.Directory.CreateDirectory(Server.MapPath("~/UserFiles/Q" + quest.QuestionID));
                //    }
                //    foreach (HttpPostedFileBase file in files)
                //    {
                //        if (file != null)
                //        {
                //            string filename = System.IO.Path.GetFileName(file.FileName);
                //            string path = System.IO.Path.Combine(
                //                       Server.MapPath("~/UserFiles/Q" + quest.QuestionID), filename);

                //            file.SaveAs(path);
                //            Files qf = new Files();
                //            qf.FileID = Guid.NewGuid();
                //            qf.QuestionID = quest.QuestionID;
                //            qf.Path = quest.QuestionID + "/" + filename;
                //            db.Files.Add(qf);
                //            await db.SaveChangesAsync();
                //        }
                //    }
                //}
                //catch
                //{
                //    ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
                //    return null;
                //   // return View(question);
                //}
                string response = quest.QuestionID +"$"+quest.Title+ "$" + quest.StudentID +  "$" + quest.Amount+"$"+quest.PostedTime+"$" + quest.DueDate ;
                //var result = new JsonResult
                //{
                //    Data = JsonConvert.SerializeObject(response)
                //};
                return new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = response }
                };
               
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return null;
            // return View(question);
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
                await db.SaveChangesAsync();

                var context = GlobalHost.ConnectionManager.GetHubContext<TutorStudentChat>();
                var username = User.Identity.Name;
                var imgsrc = db.Students.Where(c => c.Username == username).FirstOrDefault().ProfileImage;
                string message = generateMessage(username, obj.Details, imgsrc,obj.PostedTime.ToString());
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

        public string generateMessage(string username, string detail, string imgsrc,string postedTime)
        {
            string message = "";
            message = message + "<li class=\"media\">" +
                            "<div class=\"comment\"> " +
                                    "<a href=\"#\" class=\"pull-left\"><img src=\"" + imgsrc + "\" alt=\"\" class=\"img-circle\" width=\"100\" height=\"100\"> </a>" +
                                     " <div class=\"media-body\">" +
                                     " <strong class=\"text-success\">" + username + "</strong><br /><br />" +
                                       detail +
                                     "<br>" +
                                     "<div class=\"clearfix\"></div>" +
                                     " </div>" +
                                     "< span class=\"text-muted\" style=\"float:right\">"+
                                              "<small class=\"text-muted\">"+postedTime + "</small>"+
                                       " </span>"+
                                     "<div class=\"clearfix\"></div>" +
                                     "<hr>" +
                               "</div>" +
                               "</li>";
            return message;
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
                }

            }

            return Json(new { result = "true" });

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
