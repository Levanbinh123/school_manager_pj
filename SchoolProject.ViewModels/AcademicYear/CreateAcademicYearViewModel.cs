using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.ViewModels.AcademicYear
{
    using System.ComponentModel.DataAnnotations;

    namespace SchoolProject.ViewModels
    {
        public class CreateAcademicYearViewModel
        {
         

            [Required(ErrorMessage = "Ngày bắt đầu không được để trống")]
            public DateTime StartDate { get; set; }

            [Required(ErrorMessage = "Ngày kết thúc không được để trống")]
            public DateTime EndDate { get; set; }
        }
    }
}
