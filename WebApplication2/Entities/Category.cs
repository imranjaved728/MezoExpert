using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.Entities
{
    //singleton Patter used
    public class Category
    {
      private static Category obj;

      private string name { get; set; }
      private Guid Id { get; set; }

        //private constructor because of singlton pattern
        private Category()
        {

        }

        //get the instance if null create it
        public static Category getInstance()
        {
            if (obj == null)
            {
                obj = new Category();

            }
            return obj;
           
        }


    }
}