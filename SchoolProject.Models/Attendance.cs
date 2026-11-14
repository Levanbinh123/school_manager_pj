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

        public int ScheduleId { get; set; }
        public Schedule Schedule { get; set; } = null!;

        public int EnrollId { get; set; }
        public Enroll Enroll { get; set; } = null!;

        public DateTime Date { get; set; }

        public bool Present { get; set; }

        public string? Note { get; set; }

    }
}
