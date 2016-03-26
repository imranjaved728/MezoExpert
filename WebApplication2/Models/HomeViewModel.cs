using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using ClassLibrary1;

namespace WebApplication2.Models
{
    public class HomeViewModel
    {
        public HomeViewModel()
        {
            Login = new LoginModel();
            RegisterClient = new StudentRegisterModel();
            RegisterExpert = new TutorRegisterModel();
        }
        public StudentRegisterModel RegisterClient;
        public TutorRegisterModel RegisterExpert;
        public LoginModel Login;
    }
    public class LoginModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources),
           ErrorMessageResourceName = "UserNameRequired")]
        [Display(Name = "UserName", ResourceType = typeof(Resources))]
        [DataType(DataType.EmailAddress, ErrorMessage = "Enter a valid Email Address please.")]
        public string UserName { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources),
           ErrorMessageResourceName = "PasswordRequired")]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(Resources))]
        public string Password { get; set; }

        [Display(Name = "RememberMe", ResourceType = typeof(Resources))]
        public bool RememberMe { get; set; }

       public ExternalLoginListViewModel externalLogin;
    }
    public class TutorRegisterModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources),
            ErrorMessageResourceName = "FirstNameRequired")]
        [Display(Name = "FirstName", ResourceType = typeof(Resources))]
        [DataType(DataType.Text)]
        public string FirstName { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources),
            ErrorMessageResourceName = "LastNameRequired")]       
        [Display(Name = "LastName", ResourceType = typeof(Resources))]
        [DataType(DataType.Text)]
        public string LastName { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources),
            ErrorMessageResourceName = "DateOfBirthRequired")]
        [Display(Name = "DOB", ResourceType = typeof(Resources))]
        public string DOB { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources),
          ErrorMessageResourceName = "UserNameRequired")]
        [Display(Name = "UserName", ResourceType = typeof(Resources))]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Username can contain only numbers and alphabets")]
        public string UserName { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources),
          ErrorMessageResourceName = "EmailRequired")]
        [Display(Name = "Email", ResourceType = typeof(Resources))]
        [DataType(DataType.EmailAddress, ErrorMessage = "Enter a valid Email Address please.")]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources),
         ErrorMessageResourceName = "PasswordRequired")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(Resources))]
        public string Password { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources),
         ErrorMessageResourceName = "ConfirmRequired")]
        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPassword", ResourceType = typeof(Resources))]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public ExternalLoginListViewModel externalLogin;
    }

    public class TutorUpdateModel
    {
        [Required]
        [Display(Name = "FirstName", ResourceType = typeof(Resources))]
        [DataType(DataType.Text)]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "LastName", ResourceType = typeof(Resources))]
        [DataType(DataType.Text)]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "DOB", ResourceType = typeof(Resources))]
        public string DOB { get; set; }

        //[Required]
        [Display(Name = "UserName", ResourceType = typeof(Resources))]
        [DataType(DataType.Text)]
         public string UserName { get; set; }

        //[Required]
        //[Display(Name = "Email Address")]
        //[DataType(DataType.EmailAddress, ErrorMessage = "Enter a valid Email Address please.")]
        //public string Email { get; set; }

        [Required]
        [Display(Name = "Degree", ResourceType = typeof(Resources))]
        [DataType(DataType.Text)]
        public string Degree { get; set; }

        [Required]
        [Display(Name = "University", ResourceType = typeof(Resources))]
        [DataType(DataType.Text)]
        public string University { get; set; }

        [Display(Name = "TimeZone", ResourceType = typeof(Resources))]
        [DataType(DataType.Text)]
        public string timeZone { get; set; }

        [Required]
        [StringLength(256, ErrorMessage = "The {0} must be at least {2} characters long and maximum 256 characters long.", MinimumLength = 100)]
        [Display(Name = "AboutMe", ResourceType = typeof(Resources))]
        [DataType(DataType.Text)]
        public string AboutMe { get; set; }

        [Required]
        [StringLength(256, ErrorMessage = "The {0} must be at least {2} characters long and maximum 256 characters long.", MinimumLength = 100)]
        [Display(Name = "Experience", ResourceType = typeof(Resources))]
        [DataType(DataType.Text)]
        public string Experience { get; set; }

        [Required]
        [Display(Name = "City", ResourceType = typeof(Resources))]
        [DataType(DataType.Text)]
        public string City { get; set; }


        [Required]
        [Display(Name = "Country", ResourceType = typeof(Resources))]
        [DataType(DataType.Text)]
        public string Country { get; set; }

        [DataType(DataType.Text)]
        public string ProfileImage { get; set; }

        [Display(Name = "Rating", ResourceType = typeof(Resources))]
         public float Rating { get; set; }

        [DataType(DataType.Text)]
        public string []Expertise { get; set; }

        public Boolean isOnline { get; set; }
    }

    public class StudentRegisterModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources),
        ErrorMessageResourceName = "EmailRequired")]
       // [Display(Name = "Email", ResourceType = typeof(Resource))]
        [DataType(DataType.EmailAddress, ErrorMessage = "Enter a valid Email Address please.")]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources),
       ErrorMessageResourceName = "UserNameRequired")]
       // [Display(Name = "UserName", ResourceType = typeof(Resource))]
        [StringLength(100, ErrorMessage = "Username must be at least {2} characters long.", MinimumLength = 6)]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Username can contain only numbers and alphabets")]
        public string Username { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources),
       ErrorMessageResourceName = "PasswordRequired")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
       // [Display(Name = "Password", ResourceType = typeof(Resource))]
         public string Password { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources),
        ErrorMessageResourceName = "ConfirmRequired")]
        [DataType(DataType.Password)]
       // [Display(Name = "ConfirmPassword", ResourceType = typeof(Resource))]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public ExternalLoginListViewModel externalLogin;
    }

    public class StudentUpdateModel
    {
        [Display(Name = "FirstName", ResourceType = typeof(Resources))]
        [DataType(DataType.Text)]
        public string FirstName { get; set; }


        [Display(Name = "LastName", ResourceType = typeof(Resources))]
        [DataType(DataType.Text)]
        public string LastName { get; set; }


        [Display(Name = "DateOfBirth", ResourceType = typeof(Resources))]
        public string DateOfBirth { get; set; }


        [Display(Name = "UserName", ResourceType = typeof(Resources))]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Username can contain only numbers and alphabets")]
        public string UserName { get; set; }

        [Display(Name = "Degree", ResourceType = typeof(Resources))]
        [DataType(DataType.Text)]
        public string Degree { get; set; }

        [Display(Name = "TimeZone", ResourceType = typeof(Resources))]
        [DataType(DataType.Text)]
        public string timeZone { get; set; }



        [Display(Name = "Country", ResourceType = typeof(Resources))]
        [DataType(DataType.Text)]
        public string Country { get; set; }

        [Display(Name = "City", ResourceType = typeof(Resources))]
        [DataType(DataType.Text)]
        public string City { get; set; }

        [Display(Name = "University", ResourceType = typeof(Resources))]
        [DataType(DataType.Text)]
        public string University { get; set; }

        [DataType(DataType.Text)]
        public string ProfileImage { get; set; }
    }

    public class TutorHome
    {
        public TutorHome()
        {
            ActiveJobs = new List<QuestionViewModel>();
            CompledJobs = new List<QuestionViewModel>();
        }
       public IEnumerable<QuestionViewModel> ActiveJobs { get; set; }
       public IEnumerable<QuestionViewModel> CompledJobs { get; set; }
    }

}