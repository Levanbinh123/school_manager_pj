using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Models
{
    public class Submission
    {
        public int Id { get; set; }

        public int AssignmentId { get; set; }

        public int StudentId { get; set; }

        public string FileUrl { get; set; }

        public DateTime SubmittedAt { get; set; }

        public double? Score { get; set; }
        public Assignment Assignment { get; set; }
        public Student Student { get; set; }
    }
}
