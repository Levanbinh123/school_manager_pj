using System.ComponentModel.DataAnnotations;

namespace SchoolProject.ViewModels
{
    public class CloseAcademicYearViewModel
    {
        [Required]
        public bool? ConfirmReviewed { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 20)]
        public string Confirmation { get; set; } = string.Empty;

        [Required]
        public string SignatureData { get; set; } = string.Empty;
    }
}