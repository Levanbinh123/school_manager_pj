using Microsoft.AspNetCore.Http;
using SchoolProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.ViewModels
{
    public class CreateAssignmentViewModel
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int ClassSessionId { get; set; }

        [Required]
        public AssignmentType Type { get; set; }
      
    }
}
