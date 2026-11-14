using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.ViewModels
{
    public class MajorSubjectViewModel
    {
        public int MajorId { get; set; }
        public string MajorName { get; set; } = string.Empty;

        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;

        public int YearLevel { get; set; }
    }
}
