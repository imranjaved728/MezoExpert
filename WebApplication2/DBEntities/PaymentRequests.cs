using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication2.DBEntities
{
    public class PaymentRequests
    {
        [Key]
        public Guid PaymentId { get; set; }
        public string UserName { get; set; }
        public float amount { get; set; }
        public string status { get; set; }
        public DateTime postedTime { get; set; }
    }
}