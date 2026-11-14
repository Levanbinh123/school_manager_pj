using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Models
{
    public class StudentScore
    {
        public int Id { get; set; }
        public int EnrollId { get; set; }
        public Enroll Enroll { get; set; }
        public double AttendanceScore { get; set; }
        public double QuizScore { get; set; }

        public double MidtermScore { get; set; }
        public double FinalScore { get; set; }

        // Điểm tổng kết hệ 10
        public double TotalScore { get; set; }

        // Điểm chữ: A, B+, B, C+, C, D+, D, F
        public string LetterGrade { get; set; } = string.Empty;

        // Điểm hệ 4
        public double GPA { get; set; }

        // Đã chốt điểm hay chưa
        public bool IsFinalized { get; set; } = false;
    }
}
