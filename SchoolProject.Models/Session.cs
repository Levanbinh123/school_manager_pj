using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Models
{
    public class Session
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int MaxCredits { get; set; } = 30;
        public ICollection<ClassSession> ClassSessions { get; set; }
            = new HashSet<ClassSession>();
        public ICollection<TeacherSession> TeacherSessions { get; set; }=new HashSet<TeacherSession>();
        public ICollection<TuitionRate> TuitionRates { get; set; }
           = new HashSet<TuitionRate>();
    }
}
