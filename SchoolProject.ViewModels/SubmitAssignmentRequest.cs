using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.ViewModels
{
    public class SubmitAssignmentRequest
    {

        public IFormFile File { get; set; }
    }
}
