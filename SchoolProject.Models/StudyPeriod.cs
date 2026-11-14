using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Models
{
    public class StudyPeriod
    {
        public int Id { get; set; }

        // Ví dụ: Tiết 1, Tiết 2...
        public string Name { get; set; } = string.Empty;

        // Thứ tự tiết trong ngày
        public int PeriodNumber { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
