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

    public class StudentRegisterModel
    {
        [Required]
        [Display(Name = "Email Address")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Enter a valid Email Address please.")]
        public string UserName { get; set; }

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

}