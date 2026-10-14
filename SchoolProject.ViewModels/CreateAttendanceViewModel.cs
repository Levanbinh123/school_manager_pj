using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.ViewModels
{
    public class CreateAttendanceViewModel
    {
        public int EnrollId { get; set; }
        public int? SubjectId { get; set; }
        public DateTime Date { get; set; }
        public bool Present { get; set; }
        public string? Note { get; set; }
    }
}
