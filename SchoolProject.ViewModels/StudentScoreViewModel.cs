using SchoolProject.Models;

namespace SchoolProject.ViewModels
{
    public class StudentScoreViewModel
    {
        public int Id { get; set; }

        public int EnrollId { get; set; }

        public int SubjectId { get; set; }

        public double AttendanceScore { get; set; }

        public double QuizScore { get; set; }

        public double MidtermScore { get; set; }

        public double FinalScore { get; set; }

        public double TotalScore { get; set; }

        public string LetterGrade { get; set; }

        public StudentScoreViewModel()
        {
        }

        public StudentScoreViewModel(
            StudentScore score)
        {
            Id = score.Id;

            EnrollId = score.EnrollId;

            SubjectId =
                score.Enroll?.SubjectId ?? 0;

            AttendanceScore =
                score.AttendanceScore;

            QuizScore =
                score.QuizScore;

            MidtermScore =
                score.MidtermScore;

            FinalScore =
                score.FinalScore;

            TotalScore =
                score.TotalScore;

            LetterGrade =
                score.LetterGrade;
        }
    }
}