using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication2.DBEntities
{
    public class Notifications
    {
        [Required]
        [Key]
        public Guid ID { get; set; }

        public string UserName { get; set; }

        public string Message { get; set; }

        public bool isRead { get; set; }

        public string sessionId { get; set; }

        public int counts { get; set; } //for future

        public DateTime postedTime { get; set; }
    }
}