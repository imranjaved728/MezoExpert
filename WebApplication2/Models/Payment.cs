using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WebApplication2.DBEntities;

namespace WebApplication2.Models
{
    public class Payment
    {
       public Payment()
        {
            Payments = new List<PaypalPayments>();
        }
        public string Balance { get; set; }
        [Required]
        public float Amount { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public Boolean Requested { get; set; }
        public string DateRecieved { get; set; }

        public List<PaypalPayments> Payments { get; set; }
    }
}