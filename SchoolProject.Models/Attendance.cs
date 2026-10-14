using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        public int EnrollId { get; set; }
        public Enroll Enroll { get; set; }
        public int? SubjectId { get; set; }
        public Subject? Subject { get; set; }
        public DateTime Date { get; set; }  
        public bool Present {  get; set; }
        public string? Note { get; set; }

    }
}
