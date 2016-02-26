using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication2.DBEntities
{
    public class PaypalPayments
    {
        public Guid ID { get; set; }
        public string PayerId { get; set; }
        public string paymentId { get; set; }
        public string token { get; set; }
        public string guid { get; set; }
        public string amount { get; set; }
        public string status { get; set; }
        public Guid UserId { get; set; }
    }
}