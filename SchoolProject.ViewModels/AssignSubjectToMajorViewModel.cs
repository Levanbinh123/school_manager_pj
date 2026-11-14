using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.ViewModels
{
    public class AssignSubjectToMajorViewModel
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int MajorId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int SubjectId { get; set; }

        [Required]
        [Range(1, 4, ErrorMessage = "Năm học phải từ 1 đến 4.")]
        public int YearLevel { get; set; }
    }
}
