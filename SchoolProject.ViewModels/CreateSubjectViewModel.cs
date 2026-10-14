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

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "MajorId không hợp lệ.")]
        public int MajorId { get; set; }

        // Ví dụ:
        // YearLevels = [1, 2]
        // → môn này dành cho sinh viên năm 1 và năm 2
        public List<int> YearLevels { get; set; }
            = new List<int>();
    }
}