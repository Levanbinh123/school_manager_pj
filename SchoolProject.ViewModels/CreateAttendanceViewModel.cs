using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.ViewModels
{
    public class CreateAttendanceViewModel
    {
        [Required]
        public int ScheduleId { get; set; }

        [Required]
        public int EnrollId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public bool Present { get; set; }

        public string? Note { get; set; }
    }
}
