
using System.ComponentModel.DataAnnotations;

namespace SchoolProject.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        public DateTime DOB { get; set; }

        public DateTime DateOfJoin { get; set; }
            = DateTime.Now;

        // ApplicationUser.Id
        [Required]
        public string KeyId { get; set; } = string.Empty;

        public int MajorId { get; set; }

        public Major Major { get; set; } = null!;

        public int YearLevel { get; set; }

        public ICollection<Enroll> Enrolls { get; set; }
            = new HashSet<Enroll>();

        ///
        public StudentStatus Status { get; set; } = StudentStatus.Studying;
        // Thời gian bắt đầu bảo lưu
        public DateTime? LeaveStartDate { get; set; }

        // Ngày dự kiến quay lại học
        public DateTime? ExpectedReturnDate { get; set; }

        // Ngày thực tế quay lại học
        public DateTime? ActualReturnDate { get; set; }

        // Lý do bảo lưu
        public string? LeaveReason { get; set; }

        // Lý do đình chỉ hoặc thôi học
        public string? StatusReason { get; set; }

        // Soft Delete
        public bool IsDeleted { get; set; } = false;
        public ICollection<StudentLeave> StudentLeaves { get; set; }
    = new HashSet<StudentLeave>();
    }
    public enum StudentStatus
    {
        Studying = 1,   // Đang học
        Suspended = 2,  // Đình chỉ
        OnLeave = 3,    // Đang bảo lưu
        DroppedOut = 4, // Thôi học
        Graduated = 5  // Tốt nghiệp
    }
    public enum StudentLeaveStatus
    {
        Pending = 1,   // Chờ duyệt
        Approved = 2,  // Đã duyệt
        Rejected = 3,  // Bị từ chối
        Completed = 4  // Đã kết thúc bảo lưu
    }
}
