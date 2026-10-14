using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.ViewModels
{
    public class UpdateFeeViewModel
    {
        public int SubjectId { get; set; }
        public int SessionId { get; set; }
        [Required(ErrorMessage = "Ngày hết hạn không được để trống.")]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống.")]
        public string Status { get; set; } = "Unpaid";
    }
}
