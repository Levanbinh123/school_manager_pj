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

        public double TotalScore { get; set; }

        public string LetterGrade { get; set; }
    }
}
