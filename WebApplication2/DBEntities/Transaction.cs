using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication2.DBEntities
{
    public class Transaction
    {
        public Guid TransactionID { get; set; }
        //public Guid? StudentID { get; set; }
        public Guid SessionID { get; set; }
        public Double OfferedFees { get; set; }
        public String Status { get; set; }
        public virtual Session session { get; set; }
    }
}