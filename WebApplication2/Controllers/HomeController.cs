using AutoMapper;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApplication2.App_Start;
using WebApplication2.DBEntities;
using WebApplication2.Helpers;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{

    public class HomeController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            Session["noticounter"] = db.notifications.Where(c => c.UserName == User.Identity.Name && c.isRead == false).Count();
            var result = db.notifications.Where(c => c.UserName == User.Identity.Name).OrderByDescending(c => c.postedTime).Take(5);
            Session["notifications"] = result.ToList();
            return View();
        }

        public async Task<ActionResult> Profile(String username)
        {
            if (username == null || username == "")
            {
                return HttpNotFound();
            }
            Tutor tutor = db.Tutors.Where(c => c.Username == username).First();
            TutorUpdateModel tmodel = Mapper.Map<Tutor, TutorUpdateModel>(tutor);

            if (tutor == null)
            {
                return HttpNotFound();
            }
            ViewBag.id = tutor.TutorID;
            ViewBag.Expertise = new MultiSelectList(db.Categories, "CategoryID", "CategoryName", tmodel.Expertise);
            var result = db.online.Where(c => c.Username == tutor.Username).FirstOrDefault();
            if (result == null)
                tmodel.isOnline = false;
            else
                tmodel.isOnline = result.Status;
            return View(tmodel);

        }
        public ActionResult Chat()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        
        public ActionResult viewAllNotifications()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = User.Identity.Name;
                var notificationsAll = db.notifications.Where(c => c.UserName == user).OrderByDescending(c => c.postedTime);
                return View(notificationsAll);
            }
            else
            {
                return RedirectToAction("Unauthorized");
            }
            
        }

        [HttpPost]
        public async Task<ActionResult> UpdateSession()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = User.Identity.Name;
                var counter = db.notifications.Where(c => c.UserName == User.Identity.Name && c.isRead == false).Count();
                Session["noticounter"] = counter;

                if (counter > 0)
                {
                    var result = db.notifications.Where(c => c.UserName == User.Identity.Name).OrderByDescending(c => c.postedTime).Take(10);
                    Session["notifications"] = result.ToList();
                }
                
            }

            return null;
        }

        [HttpPost]
        public async Task<ActionResult> ReadNotifications()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = User.Identity.Name;
                var result = db.notifications.Where(c => c.UserName == user && c.isRead==false);

                foreach(var noti in result)
                {
                    noti.isRead = true;
                     db.Entry(noti).State = EntityState.Modified;
                }
               await  db.SaveChangesAsync();

                Session["noticounter"] = db.notifications.Where(c => c.UserName == User.Identity.Name && c.isRead == false).Count();
            }

            return null;
        }
        
        public ActionResult Language(string language)
        {
            if (language.CompareTo("English")==0)
            
                SetCulture("en");
            
            else
                SetCulture("ar");

            string cultureName = null;
            HttpCookie cultureCookie = Request.Cookies["_culture"];
            if (cultureCookie != null)
                cultureName = cultureCookie.Value;
            else
                cultureName = Request.UserLanguages != null && Request.UserLanguages.Length > 0 ?
                        Request.UserLanguages[0] :  // obtain it from HTTP header AcceptLanguages
                        null;
            // Validate culture name
             cultureName = CultureHelper.GetImplementedCulture(cultureName); // This is safe

            // Modify current thread's cultures            
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;


            return View("Index");
        }
        public ActionResult SetCulture(string culture)
        {
            // Validate input
            culture = CultureHelper.GetImplementedCulture(culture);
            // Save culture in a cookie
            HttpCookie cookie = Request.Cookies["_culture"];
            if (cookie != null)
                cookie.Value = culture;   // update cookie value
            else
            {
                cookie = new HttpCookie("_culture");
                cookie.Value = culture;
                cookie.Expires = DateTime.Now.AddYears(1);
            }
            Response.Cookies.Add(cookie);
            return RedirectToAction("Index");
        }

        public ActionResult Search(string search)
        {
            var top10 = db.Tutors.Where(c=>c.IsCompletedProfile==true).OrderByDescending(c => c.Rating).Take(10).ToList();
            var result = db.Tutors.Where(c => c.Username.Contains( search ) && c.IsCompletedProfile==true).ToList();
            var tutorExpertise = db.TutorsExpertise.Where(c => c.category.CategoryName.Contains(search)).ToList();
            var onlineUsers = db.online.Where(c => c.Status == true).ToList(); ;

            SearchViewModel obj = new SearchViewModel();
            if (!string.IsNullOrEmpty(search))
            {
                foreach (var v in tutorExpertise)
                {
                    var isTutor = onlineUsers.Where(c => c.Username == v.tutor.Username).FirstOrDefault();
                    if (isTutor != null)
                        obj.OnlineResults.Add(true);
                    else
                        obj.OnlineResults.Add(false);

                    obj.Results.Add(v.tutor);
                }

                foreach (var v in result)
                {
                    var isTutor = onlineUsers.Where(c => c.Username == v.Username).FirstOrDefault();
                    if (isTutor != null)
                        obj.OnlineResults.Add(true);
                    else
                        obj.OnlineResults.Add(false);
                    obj.Results.Add(v);
                }
            }

            foreach(var v in top10)
            {
                var isTutor = onlineUsers.Where(c => c.Username == v.Username).FirstOrDefault();
                if (isTutor != null)
                    obj.OnlineTop10.Add(true);
                else
                    obj.OnlineTop10.Add(false);
            }

            obj.Top10 = top10;
            return View(obj);
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            /*Mailer.GmailUsername = "cs.solutions.ca@gmail.com";
            Mailer.GmailPassword = "*****";

            Mailer mailer = new Mailer();
            mailer.ToEmail = "imranjaved728@gmail.com";
            mailer.Subject = "New Question Posted on MezoExperts.com";
            mailer.Body = "We can help you.";
            mailer.IsHtml = true;
            mailer.Send();*/


            /*Mailer.GmailHost = "smtpout.secureserver.net ";
            Mailer.GmailPort = 3535;
            Mailer.GmailSSL = false;*/

            /*Mailer.GmailUsername = "support@mezoexperts.com";
            Mailer.GmailPassword = "123123";

            Mailer mailer = new Mailer();
            mailer.ToEmail = "imranjaved728@gmail.com";
            mailer.Subject = "New Question Posted on MezoExperts.com";
            mailer.Body = "We can help you.";
            mailer.IsHtml = true;
            mailer.Send();*/

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Contact([Bind(Include = "Name,Email,Message")] ContactUs model)
        {
            if (ModelState.IsValid)
            {
                model.ContactUsID = Guid.NewGuid();
                if(User.Identity.IsAuthenticated)
                    model.Username = User.Identity.Name;
                db.contactus.Add(model);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            
            return View("Index");
        }

        public async Task<ActionResult> Canvas()
        {
            return View();
        }


        public ActionResult Unauthorized()
        {
            return View();
        }
    }
}