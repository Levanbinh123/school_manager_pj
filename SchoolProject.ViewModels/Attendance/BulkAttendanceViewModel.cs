using System.ComponentModel.DataAnnotations;

namespace SchoolProject.ViewModels
{
    public class BulkAttendanceViewModel
    {
        [Required]
        public int ScheduleId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Danh sách điểm danh không được để trống.")]
        public List<BulkAttendanceItemViewModel> Attendances { get; set; }
            = new();
    }

    public class BulkAttendanceItemViewModel
    {
        [Required]
        public int EnrollId { get; set; }

        public bool Present { get; set; }

        public string? Note { get; set; }
    }

    public class BulkAttendanceResultViewModel
    {
        public int Total { get; set; }

        public int Created { get; set; }

        public int Updated { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}