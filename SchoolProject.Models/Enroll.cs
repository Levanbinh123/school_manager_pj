using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Models
{
    public class Enroll
    {
        public int Id { get; set; }
        public int StudentId {  get; set; }
        public Student? Student {  get; set; }

        public int SubjectId { get; set; }

        public Subject? Subject { get; set; }
        public int SessionId { get; set; }

        public Session? Session { get; set; }
        public int? ClassSessionId { get; set; }
        public ClassSession? ClassSession { get; set; }
        public ICollection<Attendance> Attendances { get; set; }
       = new HashSet<Attendance>();

        public ICollection<StudentScore> StudentScores { get; set; }
            = new HashSet<StudentScore>();
        public EnrollStatus Status { get; set; } = EnrollStatus.Registered;

        public DateTime RegisteredAt { get; set; } = DateTime.Now;

    }
    public enum EnrollStatus
    {
        Registered = 1,
        Approved = 2,
        Dropped = 3,
        Completed = 4,
        Failed = 5,
        PaymentExpired = 6
    }
}
