using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

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
        [Required]
        [Display(Name = "Email Address")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Enter a valid Email Address please.")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

       public ExternalLoginListViewModel externalLogin;
    }
    public class TutorRegisterModel
    {
        [Required]
        [Display(Name = "First Name")]
        [DataType(DataType.Text)]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [DataType(DataType.Text)]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Date of Birth")]
        public string DOB { get; set; }

        [Required]
        [Display(Name = "Username")]
        [DataType(DataType.Text)]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Email Address")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Enter a valid Email Address please.")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class TutorUpdateModel
    {
        [Required]
        [Display(Name = "First Name")]
        [DataType(DataType.Text)]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [DataType(DataType.Text)]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Date of Birth")]
        public string DOB { get; set; }

        //[Required]
        //[Display(Name = "Username")]
        //[DataType(DataType.Text)]
        //public string UserName { get; set; }

        //[Required]
        //[Display(Name = "Email Address")]
        //[DataType(DataType.EmailAddress, ErrorMessage = "Enter a valid Email Address please.")]
        //public string Email { get; set; }

        [Required]
        [Display(Name = "Degree")]
        [DataType(DataType.Text)]
        public string Degree { get; set; }

        [Required]
        [Display(Name = "University")]
        [DataType(DataType.Text)]
        public string University { get; set; }

        [Required]
        [StringLength(256, ErrorMessage = "The {0} must be at least {2} characters long and maximum 256 characters long.", MinimumLength = 100)]
        [Display(Name = "About Me")]
        [DataType(DataType.Text)]
        public string AboutMe { get; set; }

        [Required]
        [StringLength(256, ErrorMessage = "The {0} must be at least {2} characters long and maximum 256 characters long.", MinimumLength = 100)]
        [Display(Name = "Experience")]
        [DataType(DataType.Text)]
        public string Experience { get; set; }

        [Required]
        [Display(Name = "City")]
        [DataType(DataType.Text)]
        public string City { get; set; }


        [Required]
        [Display(Name = "Country")]
        [DataType(DataType.Text)]
        public string Country { get; set; }

        [DataType(DataType.Text)]
        public string ProfileImage { get; set; }

        [DataType(DataType.Text)]
        public string []Expertise { get; set; }
    }

    public class StudentRegisterModel
    {
        [Required]
        [Display(Name = "Email Address")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Enter a valid Email Address please.")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Username")]
        [StringLength(100, ErrorMessage = "Username must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        public string Username { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

       
    }

    public class StudentUpdateModel
    {
        [Display(Name = "First Name")]
        [DataType(DataType.Text)]
        public string FirstName { get; set; }


        [Display(Name = "Last Name")]
        [DataType(DataType.Text)]
        public string LastName { get; set; }


        [Display(Name = "Date of Birth")]
        public string DateOfBirth { get; set; }


        [Display(Name = "Username")]
        [DataType(DataType.Text)]
        public string UserName { get; set; }

        [Display(Name = "Degree")]
        [DataType(DataType.Text)]
        public string Degree { get; set; }

        [Display(Name = "Country")]
        [DataType(DataType.Text)]
        public string Country { get; set; }

        [Display(Name = "City")]
        [DataType(DataType.Text)]
        public string City { get; set; }

        [Display(Name = "University")]
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