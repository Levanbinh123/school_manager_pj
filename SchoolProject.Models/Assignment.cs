using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Models
{
    public class Assignment
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime DueDate { get; set; }

        public int SubjectId { get; set; }

        public Subject Subject { get; set; }
        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; }
        public AssignmentType Type { get; set; }

        public int ClassSessionId { get; set; }
        public ClassSession ClassSession { get; set; }

        public ICollection<Submission> Submissions { get; set; }
            = new HashSet<Submission>();
    }
}
