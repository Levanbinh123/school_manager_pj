using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.ViewModels
{
    public class CreateFeeViewModel
    {
        public int StudentId { get; set; }

        public int SubjectId { get; set; }
        public int SessionId { get; set; }

        public DateTime DueDate { get; set; }
    }
}
