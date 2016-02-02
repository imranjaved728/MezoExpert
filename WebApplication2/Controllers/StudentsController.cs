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

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "Student,Admin")]
    public class StudentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public async Task<ActionResult> Manage()
        {
            //  ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View(await db.Students.ToListAsync());
        }

        
        public ActionResult Index()
        {
           
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index([Bind(Include = "QuestionID,StudentID,TutorID,Title,Details,Status,Amount,CategoryID,DueDate,PostedTime")] QuestionViewModel question, HttpPostedFileBase[] files)
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
                try
                {
                    if (!System.IO.Directory.Exists(Server.MapPath("~/UserFiles/Q" + quest.QuestionID)))
                    {
                        System.IO.Directory.CreateDirectory(Server.MapPath("~/UserFiles/Q" + quest.QuestionID));
                    }
                    foreach (HttpPostedFileBase file in files)
                    {
                        if (file != null)
                        {
                            string filename = System.IO.Path.GetFileName(file.FileName);
                            string path = System.IO.Path.Combine(
                                       Server.MapPath("~/UserFiles/Q" + quest.QuestionID), filename);

                            file.SaveAs(path);
                            Files qf = new Files();
                            qf.FileID = Guid.NewGuid();
                            qf.QuestionID = quest.QuestionID;
                            qf.Path = quest.QuestionID + "/" + filename;
                            db.Files.Add(qf);
                            await db.SaveChangesAsync();
                        }
                    }
                }
                catch
                {
                    ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
                    return View(question);
                }

                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View(question);
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
            if (!System.IO.Directory.Exists(Server.MapPath("~/Profiles/" + user)))
            {
                System.IO.Directory.CreateDirectory(Server.MapPath("~/Profiles/" + user));
            }
            string path="";
            var fileName="";
            for (int i = 0; i < Request.Files.Count; i++)
            {
                var file = Request.Files[i];

                 fileName = Path.GetFileName(file.FileName);
                   
                    path = Path.Combine(Server.MapPath("~/Profiles/" + user), fileName);
                file.SaveAs(path);
            }

            return Json(new { result = "/Profiles/" + user+ "/"+fileName });

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
        public async Task<ActionResult> EditProfile([Bind(Include = "FirstName,LastName,DateOfBirth,Country,City,University,Degree")] StudentUpdateModel student)
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
                if (!String.IsNullOrWhiteSpace(student.DateOfBirth))
                     loaddb.DateOfBirth =Convert.ToDateTime(student.DateOfBirth);
    

                db.Entry(loaddb).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
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
