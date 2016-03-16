using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.Entities
{
    public class Files
    {
        public Files()
        {
            FileID = new Guid();
        }

        public Guid FileID { get; set; }
        public Guid? QuestionID { get; set; }
        public Guid? ReplyID { get; set; }
        public String Path { get; set; }
    }

}