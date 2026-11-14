using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Models
{
    public class AcademicYear
    {
        public int Id { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsCurrent { get; set; } = false;
        // Thông tin Admin xác nhận đóng năm học
        public string? ClosedByUserId { get; set; }

        public DateTime? ClosedAt { get; set; }

        public string? CloseConfirmation { get; set; }

        // Chữ ký PNG dạng Base64
        public string? CloseSignature { get; set; }

        public AcademicYearStatus Status { get; set; }
            = AcademicYearStatus.Planned;

        public ICollection<Session> Sessions { get; set; }
            = new HashSet<Session>();
    }

    public enum AcademicYearStatus
    {
        Planned = 0,
        Active = 1,
        Completed = 2,
        Closed = 3
    }
}
