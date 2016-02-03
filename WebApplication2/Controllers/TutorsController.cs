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

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "Tutor")]
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

            if (IsCompletedProfile == true)
                return View();
            else
                return RedirectToAction("EditProfile");

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
            }

            return Json(new { result = "/Profiles/Tutors/" + user + "/" + fileName });

        }
        public async Task<ActionResult> EditProfile()
        {
            Guid user = new Guid(User.Identity.GetUserId());
            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tutor tutor = await db.Tutors.FindAsync(user);
            TutorUpdateModel t = Mapper.Map<Tutor, TutorUpdateModel>(tutor);

            if (tutor == null)
            {
                return HttpNotFound();
            }
            ViewBag.Expertise = new SelectList(db.Categories, "CategoryID", "CategoryName", t.Expertise);
            
            return View(t);

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
                loaddb.ProfileImage = tutor.ProfileImage;
                if (!String.IsNullOrWhiteSpace(tutor.DOB))
                    loaddb.DateOfBirth = Convert.ToDateTime(tutor.DOB);

                //look this thing later.

                IEnumerable<TutorExperties> obj =loaddb.tutorExperties.AsEnumerable();
                foreach (var category in tutor.Expertise)
                {
                    var result = obj.Where(c => c.CategoryID == new Guid(category)).ToList();
                    if(result.Count==0) //dont exist add it
                          db.TutorsExpertise.Add(new TutorExperties { TutorID = loaddb.TutorID, CategoryID = new Guid(category) });
                    
                    //add logic to remove as well.

                }

                loaddb.IsCompletedProfile = true;
                db.Entry(loaddb).State = EntityState.Modified;
                await db.SaveChangesAsync();
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
