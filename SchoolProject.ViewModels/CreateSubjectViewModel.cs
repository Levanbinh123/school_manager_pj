using System.ComponentModel.DataAnnotations;

namespace SchoolProject.ViewModels
{
    public class CreateSubjectViewModel
    {
        [Required(ErrorMessage = "Tên môn học không được để trống.")]
        public string Name { get; set; } = string.Empty;

        [Range(
            1,
            10,
            ErrorMessage = "Số tín chỉ phải từ 1 đến 10.")]
        public int Credits { get; set; }

      
    
    }
}