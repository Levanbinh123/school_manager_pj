using SchoolProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.ViewModels
{
    public class CreateAssignmentViewModel
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime DueDate { get; set; }

        public int SubjectId { get; set; }

        public int ClassSessionId { get; set; }
        public AssignmentType Type { get; set; }
    }
}
