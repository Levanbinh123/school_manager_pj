using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Models
{
    public class GradeConfiguration
    {
        public int Id { get; set; }

        // Mỗi lớp học phần có một cấu hình điểm
        public int ClassSessionId { get; set; }
        public ClassSession ClassSession { get; set; } = null!;

        // Trọng số tính theo phần trăm
        public decimal AttendanceWeight { get; set; } = 10;

        public decimal QuizWeight { get; set; } = 20;

        public decimal MidtermWeight { get; set; } = 30;

        public decimal FinalWeight { get; set; } = 40;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}
