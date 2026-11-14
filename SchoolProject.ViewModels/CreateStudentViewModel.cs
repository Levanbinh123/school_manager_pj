using SchoolProject.Models;
using System.ComponentModel.DataAnnotations;

namespace SchoolProject.ViewModels
{
    public class CreateStudentViewModel
    {
        [Required(ErrorMessage = "FirstName không được để trống.")]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "LastName không được để trống.")]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public DateTime DOB { get; set; }

        public DateTime DateOfJoin { get; set; }
            = DateTime.Now;

        // Năm học hiện tại của sinh viên
        [Range(1, int.MaxValue,
            ErrorMessage = "Năm học phải lớn hơn hoặc bằng 1.")]
        public int YearLevel { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "MajorId không hợp lệ.")]
        public int MajorId { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự.")]
        public string Password { get; set; } = string.Empty;

        public Student ConvertModel()
        {
            return new Student
            {
                FirstName = FirstName,
                LastName = LastName,
                DOB = DOB,
                DateOfJoin = DateOfJoin,
                MajorId = MajorId,
                YearLevel = YearLevel
            };
        }
    }
}