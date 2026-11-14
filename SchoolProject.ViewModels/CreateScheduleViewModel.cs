using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.ViewModels
{
    public class CreateScheduleViewModel
    {
        public int ClassSessionId { get; set; }

        public DayOfWeek Day { get; set; }

        // ID tiết bắt đầu
        public int StartPeriodId { get; set; }

        // ID tiết kết thúc
        public int EndPeriodId { get; set; }
    }
}
