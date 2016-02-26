using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication2.Models
{
    public class SearchViewModel
    {
        public SearchViewModel()
        {
            Results = new List<Tutor>();
            Top10 = new List<Tutor>();
        }
        public List<Tutor> Results { get; set; }
        public List<Tutor> Top10 { get; set; }
    }
}