using SchoolProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.ViewModels
{
    public class AttendanceViewModel
    {
        public int Id { get; set; }

        public int ScheduleId { get; set; }

        public int EnrollId { get; set; }

        public DateTime Date { get; set; }

        public bool Present { get; set; }

        public string? Note { get; set; }

        public AttendanceViewModel(Attendance attendance)
        {
            Id = attendance.Id;
            ScheduleId = attendance.ScheduleId;
            EnrollId = attendance.EnrollId;
            Date = attendance.Date;
            Present = attendance.Present;
            Note = attendance.Note;
        }
    }
}
