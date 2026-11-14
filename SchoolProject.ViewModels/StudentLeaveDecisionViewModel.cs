using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.ViewModels
{
    public class StudentLeaveDecisionViewModel
    {
        [StringLength(500)]
        public string? AdminNote { get; set; }
    }
}
