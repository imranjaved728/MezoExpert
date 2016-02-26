using AutoMapper;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApplication2.App_Start;
using WebApplication2.DBEntities;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
  
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            string culture = CultureHelper.GetImplementedCulture("");
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
            return View();
        }

        public async Task<ActionResult> Profile(String username)
        {
            if (username == null || username=="")
            {
                return HttpNotFound();
            }
            Tutor tutor =  db.Tutors.Where(c=>c.Username== username).First();
            TutorUpdateModel tmodel = Mapper.Map<Tutor, TutorUpdateModel>(tutor);

            if (tutor == null)
            {
                return HttpNotFound();
            }
            ViewBag.id = tutor.TutorID;
            ViewBag.Expertise = new MultiSelectList(db.Categories, "CategoryID", "CategoryName", tmodel.Expertise);
           
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


        public ActionResult Search(string search)
        {
            var top10 = db.Tutors.Where(c=>c.IsCompletedProfile==true).OrderByDescending(c => c.Rating).Take(10).ToList();
            var result = db.Tutors.Where(c => c.Username.Contains( search ) && c.IsCompletedProfile==true).ToList();
            var tutorExpertise = db.TutorsExpertise.Where(c => c.category.CategoryName.Contains(search)).ToList();
            SearchViewModel obj = new SearchViewModel();
            foreach (var v in tutorExpertise)
            {
                obj.Results.Add(v.tutor);
            }

            foreach (var v in result)
            {
                obj.Results.Add(v);
            }


            obj.Top10 = top10;
            return View(obj);
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Unauthorized()
        {
            return View();
        }
    }
}