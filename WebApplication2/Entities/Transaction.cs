using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.Entities
{
    public class Transaction
    {
        private static Transaction obj;
        private Transaction()
        {
            TransactionID = new Guid();
        }
        
        //get the instance if null create it
        public static Transaction getInstance()
        {
            if (obj == null)
            {
                obj = new Transaction();

            }
            return obj;

        }
        private Guid TransactionID { get; set; }
        private Guid SessionID { get; set; }
        private Double OfferedFees { get; set; }
        private String Status { get; set; }
        public virtual Session session { get; set; }
    }
}