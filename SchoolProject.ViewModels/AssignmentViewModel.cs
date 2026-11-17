using SchoolProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.ViewModels
{
    public class AssignmentViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime DueDate { get; set; }

        public int ClassSessionId { get; set; }

        public AssignmentType Type { get; set; }
        public string? AttachmentUrl { get; set; }
        public bool IsSubmitted { get; set; }=false;
        public DateTime? SubmittedAt { get; set; }
        public string? SubmissionFileUrl {  get; set; }
        public double? Score { get; set; }

        public AssignmentViewModel()
        {
        }

        public AssignmentViewModel(Assignment assignment)
        {
            Id = assignment.Id;
            Title = assignment.Title;
            Description = assignment.Description;
            DueDate = assignment.DueDate;
         
            ClassSessionId = assignment.ClassSessionId;
            Type = assignment.Type;
            AttachmentUrl = assignment.AttachmentUrl;
        }
    }
}
