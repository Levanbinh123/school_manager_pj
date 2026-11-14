using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace SchoolProject.Models
{
    public class StudentLeave
    {
        public int Id { get; set; }

        // Sinh viên xin bảo lưu
        [Required]
        public int StudentId { get; set; }

        public Student Student { get; set; } = null!;

        // Thời gian bảo lưu
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime ExpectedReturnDate { get; set; }

        // Ngày sinh viên thực tế quay lại
        public DateTime? ActualReturnDate { get; set; }

        // Lý do xin bảo lưu
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        // Trạng thái đơn
        public StudentLeaveStatus Status { get; set; }
            = StudentLeaveStatus.Pending;

        // Người duyệt và thời gian duyệt
        public string? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        // Ghi chú của nhà trường
        [MaxLength(500)]
        public string? AdminNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
