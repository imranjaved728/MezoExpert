using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication2.DBEntities
{
    public class NotificationConnections
    {
        [Required]
        [Key]
        public Guid ID { get; set; }

        public string userName { get; set; }

        public string connectionId { get; set; }
    }
}