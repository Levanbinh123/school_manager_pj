using SchoolProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.ViewModels
{
    public class SubmissionViewModel
    {
        public int Id { get; set; }

        public int AssignmentId { get; set; }

        public int StudentId { get; set; }

        public string FileUrl { get; set; }

        public DateTime SubmittedAt { get; set; }

        public double? Score { get; set; }

        public SubmissionViewModel()
        {
        }

        public SubmissionViewModel(Submission submission)
        {
            Id = submission.Id;
            AssignmentId = submission.AssignmentId;
            StudentId = submission.StudentId;
            FileUrl = submission.FileUrl;
            SubmittedAt = submission.SubmittedAt;
            Score = submission.Score;
        }
    }
}
