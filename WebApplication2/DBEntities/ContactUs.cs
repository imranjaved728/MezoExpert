using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication2.DBEntities
{
    public class ContactUs
    {
        [Required]
        public Guid ContactUsID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public string Username { get; set; }
        [Required]
        public string Message { get; set; }

    }
}