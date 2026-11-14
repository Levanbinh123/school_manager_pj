using System.ComponentModel.DataAnnotations;

namespace SchoolProject.ViewModels
{
    public class CreateMajorViewModel
    {
        [Required(ErrorMessage = "Tên ngành không được để trống.")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Range(1, 300,
            ErrorMessage = "Số tín chỉ phải từ 1 đến 300.")]
        public int RequiredCredits { get; set; } = 120;
    }
}