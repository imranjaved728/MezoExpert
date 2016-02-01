using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication2.DBEntities;
using WebApplication2.Models;
using Microsoft.AspNet.Identity;
using AutoMapper;

namespace WebApplication2.Controllers
{
    public class QuestionsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
       
        // GET: Questions
        public async Task<ActionResult> Index()
        {
            var questions = db.Questions.Include(q => q.student);
            return View(await questions.ToListAsync());
        }

        // GET: Questions/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Question question = await db.Questions.FindAsync(id);
            if (question == null)
            {
                return HttpNotFound();
            }
            return View(question);
        }

        // GET: Questions/Create
        public ActionResult Create()
        {
            ViewBag.TutorID = new SelectList(db.Tutors, "TutorID", "FirstName");
            return View();
        }
/*
        // GET: Questions/Create
        public ActionResult Post()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Post([Bind(Include = "QuestionID,StudentID,TutorID,Title,Details,Status,Amount,CategoryID,DueDate,PostedTime")] QuestionViewModel question, HttpPostedFileBase[] files)
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
        }*/

        // POST: Questions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "QuestionID,StudentID,TutorID,Title,Details,Status,Amount,DueDate,PostedTime")] Question question)
        {
            if (ModelState.IsValid)
            {
                question.QuestionID = Guid.NewGuid();
                question.PostedTime = DateTime.Now;
                //initial posted Question Status
                question.Status = "Posted";
                //user posting question id
                question.StudentID= new Guid(User.Identity.GetUserId());
                db.Questions.Add(question);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.StudentID = new SelectList(db.Students, "StudentID", "FirstName", question.StudentID);
            return View(question);
        }

        // GET: Questions/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Question question = await db.Questions.FindAsync(id);
            if (question == null)
            {
                return HttpNotFound();
            }
            ViewBag.StudentID = new SelectList(db.Students, "StudentID", "FirstName", question.StudentID);
            return View(question);
        }

        // POST: Questions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "QuestionID,StudentID,TutorID,Title,Details,Status,Amount,DueDate,PostedTime")] Question question)
        {
            if (ModelState.IsValid)
            {
                db.Entry(question).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.StudentID = new SelectList(db.Students, "StudentID", "FirstName", question.StudentID);
            return View(question);
        }

        // GET: Questions/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Question question = await db.Questions.FindAsync(id);
            if (question == null)
            {
                return HttpNotFound();
            }
            return View(question);
        }

        // POST: Questions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            Question question = await db.Questions.FindAsync(id);
            db.Questions.Remove(question);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
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
