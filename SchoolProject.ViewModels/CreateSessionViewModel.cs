using SchoolProject.Models;
using System.ComponentModel.DataAnnotations;

namespace SchoolProject.ViewModels
{
    public class CreateSessionViewModel
    {
        [Required(ErrorMessage = "Tên học kỳ không được để trống")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime Start { get; set; }

        [Required]
        public DateTime End { get; set; }

        [Range(1, 100)]
        public int MaxCredits { get; set; } = 30;

        [Range(1, int.MaxValue)]
        public int AcademicYearId { get; set; }

        [Range(1, 20)]
        public int SemesterNumber { get; set; }

        public bool IsFinalSemester { get; set; } = false;
    }
}