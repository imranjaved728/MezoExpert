using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication3.Entities
{
    //for command pattern
    public interface Command
    {
        void Execute(string message);

       
    }
}
