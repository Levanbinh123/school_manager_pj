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

        public int SubjectId { get; set; }

        public int TeacherId { get; set; }

        public int ClassSessionId { get; set; }

        public AssignmentType Type { get; set; }

        public AssignmentViewModel()
        {
        }

        public AssignmentViewModel(Assignment assignment)
        {
            Id = assignment.Id;
            Title = assignment.Title;
            Description = assignment.Description;
            DueDate = assignment.DueDate;
            SubjectId = assignment.SubjectId;
            TeacherId = assignment.TeacherId;
            ClassSessionId = assignment.ClassSessionId;
            Type = assignment.Type;
        }
    }
}
