
using System.ComponentModel.DataAnnotations;

namespace SchoolProject.ViewModels
{
    public class CreateGradeConfigurationViewModel
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int ClassSessionId { get; set; }

        [Range(typeof(decimal), "0", "100")]
        public decimal AttendanceWeight { get; set; } = 10;

        [Range(typeof(decimal), "0", "100")]
        public decimal QuizWeight { get; set; } = 20;

        [Range(typeof(decimal), "0", "100")]
        public decimal MidtermWeight { get; set; } = 30;

        [Range(typeof(decimal), "0", "100")]
        public decimal FinalWeight { get; set; } = 40;
    }

    public class GradeConfigurationViewModel
    {
        public int Id { get; set; }

        public int ClassSessionId { get; set; }

        public decimal AttendanceWeight { get; set; }

        public decimal QuizWeight { get; set; }

        public decimal MidtermWeight { get; set; }

        public decimal FinalWeight { get; set; }

        public decimal TotalWeight =>
            AttendanceWeight + QuizWeight +
            MidtermWeight + FinalWeight;
    }
}
