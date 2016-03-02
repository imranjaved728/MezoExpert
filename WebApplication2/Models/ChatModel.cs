using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication2.DBEntities;

namespace WebApplication2.Models
{
    public class ChatModel
    {
        public ChatModel()
        {
            offer = new Offer();
        }
        public Session session { get; set; }
        [Required]
        [Display(Name = "Message")]
        public String  replyDetail { get; set; }
        public Guid sessionID { get; set; }
        public Offer offer { get; set; }
        public Boolean status { get; set; }
    }


    public class Offer
    {
        [Required]
        [RegularExpression(@"^\d+.\d{0,2}$", ErrorMessage = "Amount can't have more than 2 decimal places")]
        public double amount { get; set; }
        public string Rating { get; set; }
        public Guid SessionId { get; set; }
    }
}