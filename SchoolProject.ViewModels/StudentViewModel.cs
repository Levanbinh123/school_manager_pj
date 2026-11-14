
using SchoolProject.Models;

namespace SchoolProject.ViewModels
{
    public class StudentViewModel
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public DateTime DOB { get; set; }

        public DateTime DateOfJoin { get; set; }

        public string? KeyId { get; set; } = string.Empty;

        public int MajorId { get; set; }

        public string? MajorName { get; set; }

        public int YearLevel { get; set; }

        public string FullName =>
            $"{FirstName} {LastName}";

        public StudentViewModel()
        {
        }

        public StudentViewModel(Student student)
        {
            Id = student.Id;
            FirstName = student.FirstName;
            LastName = student.LastName;
            DOB = student.DOB;
            DateOfJoin = student.DateOfJoin;
            KeyId = student.KeyId;
            MajorId = student.MajorId;
            MajorName = student.Major?.Name;
            YearLevel = student.YearLevel;
        }
    }
}
