namespace SchoolProject.ViewModels
{
    public class AssignmentSummaryViewModel
    {
        public int AssignmentId { get; set; }

        public string? Title { get; set; }

        public int TotalStudents { get; set; }

        public int SubmittedStudents { get; set; }

        public int NotSubmittedStudents { get; set; }

        public int GradedSubmissions { get; set; }

        public int NotGradedSubmissions { get; set; }
    }
}