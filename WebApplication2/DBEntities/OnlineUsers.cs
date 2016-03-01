using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication2.DBEntities
{
    public class OnlineUsers
    {
        [Key]
        public string Username { get; set; }
        public bool Status { get; set; }
    }
}