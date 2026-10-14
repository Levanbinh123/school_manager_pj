namespace SchoolProject.ViewModels
{
    public class SubmissionNotGradedViewModel
    {
        public int SubmissionId { get; set; }

        public int AssignmentId { get; set; }

        public int StudentId { get; set; }

        public string? StudentName { get; set; }

        public string? FileUrl { get; set; }

        public DateTime SubmittedAt { get; set; }

        public double? Score { get; set; }
    }
}