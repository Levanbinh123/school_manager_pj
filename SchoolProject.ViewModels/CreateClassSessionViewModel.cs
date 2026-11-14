
using System.ComponentModel.DataAnnotations;

namespace SchoolProject.ViewModels
{
    public class CreateClassSessionViewModel : IValidatableObject
    {
        [Range(1, int.MaxValue, ErrorMessage = "ClassId không hợp lệ.")]
        public int ClassId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "SubjectId không hợp lệ.")]
        public int SubjectId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "SessionId không hợp lệ.")]
        public int SessionId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "TeacherId không hợp lệ.")]
        public int? HomeroomTeacherId { get; set; }

        // Có sử dụng điểm danh hay không
        public bool HasAttendance { get; set; } = false;

        // Tổng số buổi học
        [Range(1, 1000,
            ErrorMessage = "Tổng số buổi học phải từ 1 đến 1000.")]
        public int TotalSessions { get; set; }

        // Số sinh viên tối thiểu để mở lớp
        [Range(1, 1000,
            ErrorMessage = "Số sinh viên tối thiểu phải từ 1 đến 1000.")]
        public int MinStudents { get; set; } = 10;

        // Số sinh viên tối đa của lớp
        [Range(1, 1000,
            ErrorMessage = "Số sinh viên tối đa phải từ 1 đến 1000.")]
        public int MaxStudents { get; set; } = 40;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime RegistrationStart { get; set; }
        public DateTime RegistrationEnd { get; set; }
        // Kiểm tra số lượng sinh viên
        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            if (MinStudents > MaxStudents)
            {
                yield return new ValidationResult(
                    "Số sinh viên tối thiểu không được lớn hơn số sinh viên tối đa.",
                    new[] { nameof(MinStudents), nameof(MaxStudents) }
                );
            }
        }
    }
}
