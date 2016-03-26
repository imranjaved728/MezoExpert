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
using System.IO;
using WebApplication2.Helpers;
using WebApplication2.DBEntities;

namespace WebApplication2.Controllers
{
   
    public class AccountController : BaseController
    {
        #region Properties
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext _dbContext;
     

        public AccountController()
        {
           _dbContext = new ApplicationDbContext();
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
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

        #region Login and Register
        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await UserManager.FindAsync(model.UserName, model.Password);
            if (user == null)
                user = await UserManager.FindByEmailAsync(model.UserName);
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result= SignInStatus.Failure;
            if(user!=null)
               result= await SignInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, shouldLockout: false);
            else
                result=await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: false);

            switch (result)
            {
                case SignInStatus.Success:
                  

                    if (UserManager.IsInRole(user.Id, "Student"))
                        {
                            return RedirectToAction("Index", "Students");
                        }
                        //role Admin go to Admin page
                        else if (UserManager.IsInRole(user.Id, "Admin"))
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        else
                            return RedirectToAction("Index", "Tutors");
                    
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:

                default:
                    ModelState.AddModelError("", "Username and password do not match.");
                    ViewBag.error = "login";
                    return View("../Home/Index",model);
            }
        }

      
        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Register(StudentRegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Username, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
              
                if (result.Succeeded)
                {
                    var roleresult = UserManager.AddToRole(user.Id, "Student");
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    Student stu = new Student();
                    stu.StudentID = new Guid(user.Id);
                    stu.DateCreated = DateTime.Today;
                    Random rnd = new Random();
                    int filename = rnd.Next(1, 4);
                    stu.ProfileImage = "/Profiles/default/" + filename + ".png";
                    stu.Username = user.UserName;
                    _dbContext.Students.Add(stu);
                    _dbContext.SaveChanges();

                    Notifications notify = new Notifications();
                    notify.ID = Guid.NewGuid();
                    notify.isRead = false;
                    notify.Message = "/Profiles/default/admin.png^Admin^You have successfully created your account. You can click ask question to post your first question.";
                    notify.UserName = stu.Username;
                    notify.postedTime = DateTime.Now;
                    _dbContext.notifications.Add(notify);

                    Notifications notify2 = new Notifications();
                    notify2.ID = Guid.NewGuid();
                    notify2.isRead = false;
                    notify2.Message = "/Profiles/default/admin.png^Admin^We now have Arabic Language support as well.";
                    notify2.UserName = stu.Username;
                    notify2.postedTime = DateTime.Now;
                    _dbContext.notifications.Add(notify2);
                    _dbContext.SaveChanges();

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Students");
                }
                else {

                    var userE = _dbContext.Users.Where(c => c.Email == model.Email).FirstOrDefault();
                    var userU = _dbContext.Users.Where(c => c.UserName == model.Username).FirstOrDefault();
                    if (userE != null)
                        ModelState.AddModelError("", "Email already exists.");
                    if (userU != null)
                        ModelState.AddModelError("", "Username already exists.");
                    ViewBag.error = "registerS";
                    return View("../Home/Index", model);
                }
            }

            // If we got this far, something failed, redisplay form
            // return View(model);
            return RedirectToAction("Index", "Home");
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> RegisterTutor(TutorRegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
            
                if (result.Succeeded)
                {
                    var roleresult = UserManager.AddToRole(user.Id, "Tutor");
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    Tutor tutor = Mapper.Map<TutorRegisterModel, Tutor>(model);
                    tutor.TutorID = new Guid(user.Id);
                    tutor.DateCreated = DateTime.Today;
                    tutor.IsCompletedProfile = false;
                    tutor.Username = user.UserName;
                    Random rnd = new Random();
                    int filename = rnd.Next(1, 4);
                    tutor.ProfileImage = "/Profiles/default/"+filename+".png";
                    _dbContext.Tutors.Add(tutor);
                    _dbContext.SaveChanges();

                    Notifications notify = new Notifications();
                    notify.ID = Guid.NewGuid();
                    notify.isRead = false;
                    notify.Message = "/Profiles/default/admin.png^Admin^Please complete your profile so that you have full access.";
                    notify.UserName = tutor.Username;
                    notify.postedTime = DateTime.Now;
                    _dbContext.notifications.Add(notify);
                    _dbContext.SaveChanges();
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Tutors");
                }
                else
                {
                    var userE= _dbContext.Users.Where(c => c.Email == model.Email).FirstOrDefault();
                    var userU = _dbContext.Users.Where(c => c.UserName == model.UserName).FirstOrDefault();
                    if (userE != null)
                        ModelState.AddModelError("", "Email already exists.");
                    if(userU!=null)
                        ModelState.AddModelError("", "Username already exists.");
                    ViewBag.error = "registerT";
                    return View("../Home/Index", model);
                }
               
            }

            // If we got this far, something failed, redisplay form
            // return View("Index",model);
            return RedirectToAction("Index", "Home");
        }


        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }
        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            if (loginInfo.Login.LoginProvider == "Google")
            {
                var externalIdentity = AuthenticationManager.GetExternalIdentityAsync(DefaultAuthenticationTypes.ExternalCookie);
                var emailClaim = externalIdentity.Result.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                var lastNameClaim = externalIdentity.Result.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname);
                var givenNameClaim = externalIdentity.Result.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName);

                var email = emailClaim.Value;
                var firstName = givenNameClaim.Value;
                var lastname = lastNameClaim.Value;
            }
            //else
            //{
            //    var identity = AuthenticationManager.GetExternalIdentity(DefaultAuthenticationTypes.ExternalCookie);
            //    var access_token = identity.FindFirstValue("FacebookAccessToken");
            //    var fb = new FacebookClient(access_token);
            //    dynamic myInfo = fb.Get("/me?fields=email"); // specify the email field
            //    loginInfo.Email = myInfo.email;
            //}

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            
            string username = "";
            if (result == SignInStatus.Success)
                username = SignInManager.AuthenticationManager.AuthenticationResponseGrant.Identity.GetUserName();
            switch (result)
            {
                case SignInStatus.Success:
                    if (_dbContext.Tutors.Where(c => c.Username == username).FirstOrDefault() != null)
                        return RedirectToAction("Index","Tutors");
                    else if(_dbContext.Students.Where(c => c.Username == username).FirstOrDefault()!=null)
                        return RedirectToAction("Index", "Students");
                    else
                        return RedirectToAction("Index", "Admin");

                   // return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email,Username=loginInfo.DefaultUserName });
            }
        }
        [AllowAnonymous]
        public ActionResult ExternalLoginCallbackRedirect(string returnUrl)
        {
            return RedirectPermanent("/Account/ExternalLoginCallback");
        }
        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser
                {
                    UserName = model.Username,
                    Email = model.Email,

                };
               
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (model.SignupAS == "1")
                    {
                        var roleresult = UserManager.AddToRole(user.Id, Status.Student);
                        Student stu = new Student();
                        stu.StudentID = new Guid(user.Id);
                        stu.DateCreated = DateTime.Today;
                        Random rnd = new Random();
                        int filename = rnd.Next(1, 4);
                        stu.ProfileImage = "/Profiles/default/" + filename + ".png";
                        stu.Username = user.UserName;
                        _dbContext.Students.Add(stu);

                        Notifications notify = new Notifications();
                        notify.ID = Guid.NewGuid();
                        notify.isRead = false;
                        notify.Message = "/Profiles/default/admin.png^Admin^You have successfully created your account. You can click ask question to post your first question.";
                        notify.UserName = stu.Username;
                        notify.postedTime = DateTime.Now;
                        _dbContext.notifications.Add(notify);

                        Notifications notify2 = new Notifications();
                        notify2.ID = Guid.NewGuid();
                        notify2.isRead = false;
                        notify2.Message = "/Profiles/default/admin.png^Admin^We now have Arabic Language support as well.";
                        notify2.UserName = stu.Username;
                        notify2.postedTime = DateTime.Now;
                        _dbContext.notifications.Add(notify2);

                        _dbContext.SaveChanges();

                    }
                    else
                    {
                        var roleresult = UserManager.AddToRole(user.Id, Status.Tutor);
                        Tutor tutor = new Tutor();
                        tutor.DateOfBirth = DateTime.Today;
                        tutor.TutorID = new Guid(user.Id);
                        tutor.DateCreated = DateTime.Today;
                        tutor.IsCompletedProfile = false;
                        tutor.Username = user.UserName;
                        Random rnd = new Random();
                        int filename = rnd.Next(1, 4);
                        tutor.ProfileImage = "/Profiles/default/" + filename + ".png";
                        _dbContext.Tutors.Add(tutor);

                        Notifications notify = new Notifications();
                        notify.ID = Guid.NewGuid();
                        notify.isRead = false;
                        notify.Message = "/Profiles/default/admin.png^Admin^Please complete your profile so that you have full access.";
                        notify.UserName = tutor.Username;
                        notify.postedTime = DateTime.Now;
                        _dbContext.notifications.Add(notify);

                        _dbContext.SaveChanges();
                    }


                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                       
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }
        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
                if (_dbContext != null)
                {
                    _dbContext.Dispose();
                    _dbContext = null;
                }
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

               
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

                string body;
                using (var sr = new StreamReader(Server.MapPath("\\Helpers\\") + "passwordreset.html"))
                {
                    body = sr.ReadToEnd();
                }
                try
                {
                    Mailer.GmailUsername = "support@mezoexperts.com";
                    Mailer.GmailPassword = "123123";

                    var email = model.Email;
                    Mailer mailer = new Mailer();
                    mailer.ToEmail = model.Email;
                    mailer.Subject = "Reset your Password MezoExperts.com";
                    mailer.Body = string.Format(body, "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                    mailer.IsHtml = true;
                    mailer.Send();
                        
                    
                }
                catch (Exception e)
                {

                }

                //await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }


        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        /*
        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }
       

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }
         */



        /*
            //
            // GET: /Account/VerifyCode
            [AllowAnonymous]
            public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
            {
                // Require that the user has already logged in via username/password or external login
                if (!await SignInManager.HasBeenVerifiedAsync())
                {
                    return View("Error");
                }
                return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
            }

            //
            // POST: /Account/VerifyCode
            [HttpPost]
            [AllowAnonymous]
            [ValidateAntiForgeryToken]
            public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // The following code protects for brute force attacks against the two factor codes. 
                // If a user enters incorrect codes for a specified amount of time then the user account 
                // will be locked out for a specified amount of time. 
                // You can configure the account lockout settings in IdentityConfig
                var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
                switch (result)
                {
                    case SignInStatus.Success:
                        return RedirectToLocal(model.ReturnUrl);
                    case SignInStatus.LockedOut:
                        return View("Lockout");
                    case SignInStatus.Failure:
                    default:
                        ModelState.AddModelError("", "Invalid code.");
                        return View(model);
                }
            }
            */

    }
}