using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.ViewModels
{
    public class UpdateSessionViewModel
    {
        [Required(ErrorMessage = "Tên học kỳ không được để trống")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime Start { get; set; }

        [Required]
        public DateTime End { get; set; }

        [Range(1, 100)]
        public int MaxCredits { get; set; }

        [Range(1, 20)]
        public int SemesterNumber { get; set; }
        public bool IsFinalSemester { get; set; } = false;
    }
}
