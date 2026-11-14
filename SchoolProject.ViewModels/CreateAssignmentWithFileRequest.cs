using Microsoft.AspNetCore.Http;

namespace SchoolProject.ViewModels
{
    public class CreateAssignmentWithFileRequest
    {
        // JSON chứa các thuộc tính của Assignment
        public string AssignmentJson { get; set; } = string.Empty;

        // File không bắt buộc
        public IFormFile? File { get; set; }
    }
}