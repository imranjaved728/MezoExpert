using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using WebApplication2.Models;
using AutoMapper;
using Facebook;
using WebApplication2.App_Start;
using System.Data.Entity;
using System.Net;
using WebApplication2.DBEntities;

namespace WebApplication2.Controllers
{
    [CustomAuthorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        #region Properties
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext db;

        public AdminController()
        {
            db = new ApplicationDbContext();
        }

        public AdminController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        #endregion


        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        #region Tutor
        // GET: temp
        public async Task<ActionResult> IndexTutor()
        {
            return View("Tutor/Index",await db.Tutors.ToListAsync());
        }

        // GET: temp/Details/5
        public async Task<ActionResult> DetailsTutor(Guid? id)
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
            return View("Tutor/Details",tutor);
        }

        // GET: temp/Create
        public ActionResult CreateTutor()
        {
            return View("Tutor/Create");
        }

        // POST: temp/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateTutor([Bind(Include = "TutorID,FirstName,LastName,DateOfBirth,Degree,University,AboutMe,Experience,City,Country,CurrentEarning,ProfileImage,Username,IsCompletedProfile,Rating")] Tutor tutor)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = tutor.Username, Email = tutor.Degree };
                var result =  await UserManager.CreateAsync(user, "123456");

                if (result.Succeeded)
                {
                    var roleresult = UserManager.AddToRole(user.Id, "Tutor");
                   

                    tutor.TutorID = Guid.NewGuid();
                    tutor.DateCreated = DateTime.Now;
                    db.Tutors.Add(tutor);
                    await db.SaveChangesAsync();
                    return RedirectToAction("IndexTutor");
                }
            }

            return View(tutor);
        }

        // GET: temp/Edit/5
        public async Task<ActionResult> EditTutor(Guid? id)
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
            return View("Tutor/Edit", tutor);
        }

        // POST: temp/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditTutor([Bind(Include = "TutorID,FirstName,LastName,DateOfBirth,Degree,University,AboutMe,Experience,City,Country,DateCreated,CurrentEarning,ProfileImage,Username,IsCompletedProfile,Rating")] Tutor tutor)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tutor).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("IndexTutor");
            }
            return View("Tutor/Edit",tutor);
        }

        // GET: temp/Delete/5
        public async Task<ActionResult> DeleteTutor(Guid? id)
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
            return View("Tutor/Delete",tutor);
        }

        // POST: temp/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmedTutor(Guid id)
        {
            Tutor tutor = await db.Tutors.FindAsync(id);
            db.Tutors.Remove(tutor);
            await db.SaveChangesAsync();
            return RedirectToAction("IndexTutor");
        }
        #endregion
        
        #region Categories

        // GET: Categories
        public async Task<ActionResult> IndexCategories()
        {
            return View("Categories/Index",await db.Categories.ToListAsync());
        }

        // GET: Categories/Details/5
        public async Task<ActionResult> DetailsCategories(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = await db.Categories.FindAsync(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View("Categories/Details", category);
        }

        // GET: Categories/Create
        public ActionResult CreateCategories()
        {
            return View("Categories/Create");
        }

        // POST: Categories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateCategories([Bind(Include = "CategoryID,CategoryName")] Category category)
        {
            if (ModelState.IsValid)
            {
                category.CategoryID = Guid.NewGuid();
                db.Categories.Add(category);
                await db.SaveChangesAsync();
                return RedirectToAction("IndexCategories");
            }

            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<ActionResult> EditCategories(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = await db.Categories.FindAsync(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View("Categories/Edit",category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditCategories([Bind(Include = "CategoryID,CategoryName")] Category category)
        {
            if (ModelState.IsValid)
            {
                db.Entry(category).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("IndexCategories");
            }
            return View("Categories/Edit", category);
        }

        // GET: Categories/Delete/5
        public async Task<ActionResult> DeleteCategories(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = await db.Categories.FindAsync(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View("Categories/Delete",category);
        }

        // POST: Categories/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmedCategories(Guid id)
        {
            Category category = await db.Categories.FindAsync(id);
            db.Categories.Remove(category);
            await db.SaveChangesAsync();
            return RedirectToAction("IndexCategories");
        }



        #endregion

        #region Students
        // GET: Students1
        public async Task<ActionResult> IndexStudent()
        {
            return View("Student/Index",await db.Students.ToListAsync());
        }

        // GET: Students1/Details/5
        public async Task<ActionResult> DetailsStudent(Guid? id)
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
            return View("Student/Details", student);
        }

        // GET: Students1/Create
        public ActionResult CreateStudent()
        {
            return View("Student/Create");
        }

        // POST: Students1/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateStudent([Bind(Include = "StudentID,FirstName,LastName,DateOfBirth,Degree,University,City,Country,DateCreated,CurrentBalance,ProfileImage,Username")] Student student)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = student.Username, Email = student.Degree };
                var result = await UserManager.CreateAsync(user, "123456");

                if (result.Succeeded)
                {
                    var roleresult = UserManager.AddToRole(user.Id, "Student");
                    student.StudentID = Guid.NewGuid();
                    student.DateCreated = DateTime.Now;
                    db.Students.Add(student);
                    await db.SaveChangesAsync();
                    return RedirectToAction("IndexStudent");
                }
            }

            return View(student);
        }

        // GET: Students1/Edit/5
        public async Task<ActionResult> EditStudent(Guid? id)
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
            return View("Student/Edit",student);
        }

        // POST: Students1/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditStudent([Bind(Include = "StudentID,FirstName,LastName,DateOfBirth,Degree,University,City,Country,DateCreated,CurrentBalance,ProfileImage,Username")] Student student)
        {
            if (ModelState.IsValid)
            {
                db.Entry(student).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("IndexStudent");
            }
            return View("Student/Edit",student);
        }

        // GET: Students1/Delete/5
        public async Task<ActionResult> DeleteStudent(Guid? id)
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
            return View("Student/Delete",student);
        }

        // POST: Students1/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmedStudent(Guid id)
        {
            Student student = await db.Students.FindAsync(id);
            db.Students.Remove(student);
            await db.SaveChangesAsync();
            return RedirectToAction("IndexStudent");
        }

        #endregion

        #region Questions


        // GET: Questions
        public async Task<ActionResult> IndexQuestion()
        {
            var questions = db.Questions.Include(q => q.student);
            return View("Question/Index",await questions.ToListAsync());
        }

        // GET: Questions/Details/5
        public async Task<ActionResult> DetailsQuestion(Guid? id)
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
        public ActionResult CreateQuestion()
        {
            ViewBag.StudentID = new SelectList(db.Students, "StudentID", "FirstName");
            return View("Question/Create");
        }

        // POST: Questions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateQuestion([Bind(Include = "QuestionID,StudentID,TutorID,Title,Details,Status,Amount,DueDate")] Question question)
        {
            if (ModelState.IsValid)
            {
                question.PostedTime = DateTime.Now;
                question.QuestionID = Guid.NewGuid();
                db.Questions.Add(question);
                await db.SaveChangesAsync();
                return RedirectToAction("IndexQuestion");
            }

            ViewBag.StudentID = new SelectList(db.Students, "StudentID", "FirstName", question.StudentID);
            return View("Question/Create",question);
        }

        // GET: Questions/Edit/5
        public async Task<ActionResult> EditQuestion(Guid? id)
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
            return View("Question/Edit",question);
        }

        // POST: Questions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditQuestion([Bind(Include = "QuestionID,StudentID,TutorID,Title,Details,Status,Amount,DueDate,PostedTime")] Question question)
        {
            if (ModelState.IsValid)
            {
                db.Entry(question).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("IndexQuestion");
            }
            ViewBag.StudentID = new SelectList(db.Students, "StudentID", "FirstName", question.StudentID);
            return View("Question/Edit",question);
        }

        // GET: Questions/Delete/5
        public async Task<ActionResult> DeleteQuestion(Guid? id)
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
            return View("Question/Delete",question);
        }

        // POST: Questions/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmedQuestion(Guid id)
        {
            Question question = await db.Questions.FindAsync(id);
            db.Questions.Remove(question);
            await db.SaveChangesAsync();
            return RedirectToAction("IndexQuestion");
        }

        #endregion


        #region contactus

        // GET: Questions
        public async Task<ActionResult> IndexContact()
        {
            var contacts =await db.contactus.ToListAsync();
            return View("Contact/Index", contacts);
        }

        // GET: Questions/Delete/5
        public async Task<ActionResult> DeleteContact(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ContactUs question = await db.contactus.FindAsync(id);
            if (question == null)
            {
                return HttpNotFound();
            }
            return View("Contact/Delete", question);
        }

        // POST: Questions/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmedContact(Guid id)
        {
            ContactUs question = await db.contactus.FindAsync(id);
            db.contactus.Remove(question);
            await db.SaveChangesAsync();
            return RedirectToAction("IndexContact");
        }

        #endregion

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