using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication2.DBEntities
{
    public class Files
    {
        [Key]
        public Guid FileID { get; set; }
        public Guid? QuestionID { get; set; }
        public Guid? ReplyID { get; set; }
        public String Path { get; set; }
    }
}