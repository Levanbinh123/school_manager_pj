using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.ViewModels
{
    public class ScheduleViewModel
    {
        public int Id { get; set; }

        public int ClassSessionId { get; set; }

        public int SubjectId { get; set; }

        public int TeacherId { get; set; }

        public DayOfWeek Day { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public string Room { get; set; }

        public ScheduleViewModel()
        {
        }

        public ScheduleViewModel(Schedule schedule)
        {
            Id = schedule.Id;
            ClassSessionId = schedule.ClassSessionId;
            SubjectId = schedule.SubjectId;
            TeacherId = schedule.TeacherId;
            Day = schedule.Day;
            StartTime = schedule.StartTime;
            EndTime = schedule.EndTime;
            Room = schedule.Room;
        }
    }
}
