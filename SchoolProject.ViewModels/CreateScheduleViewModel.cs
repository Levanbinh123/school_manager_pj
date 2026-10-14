using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.ViewModels
{
    public class CreateScheduleViewModel
    {
        public int ClassSessionId { get; set; }

        public int SubjectId { get; set; }

        public int TeacherId { get; set; }

        public DayOfWeek Day { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public string Room { get; set; }
    }
}
