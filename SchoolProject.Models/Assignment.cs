using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Models
{
    public class Assignment
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime DueDate { get; set; }

        public AssignmentType Type { get; set; }
        public string? AttachmentUrl { get; set; }

        // Assignment thuộc lớp học phần nào
        public int ClassSessionId { get; set; }
        public ClassSession ClassSession { get; set; } = null!;

        public ICollection<Submission> Submissions { get; set; }
            = new HashSet<Submission>();
    }
}
