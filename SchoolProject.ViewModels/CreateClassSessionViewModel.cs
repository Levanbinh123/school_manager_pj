using System.ComponentModel.DataAnnotations;

namespace SchoolProject.ViewModels
{
    public class CreateClassSessionViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "ClassId không hợp lệ.")]
        public int ClassId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "SubjectId không hợp lệ.")]
        public int SubjectId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "SessionId không hợp lệ.")]
        public int SessionId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "TeacherId không hợp lệ.")]
        public int? HomeroomTeacherId { get; set; }
        // true = có điểm danh
        // false = không điểm danh
        public bool HasAttendance { get; set; } = false;
        //so buoi hoc cua lop nay
        [Range(1, 1000, ErrorMessage = "Tổng số buổi học phải từ 1 đến 1000.")]
        public int TotalSessions { get; set; }
    }
}