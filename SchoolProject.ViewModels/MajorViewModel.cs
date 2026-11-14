using SchoolProject.Models;

namespace SchoolProject.ViewModels
{
    public class MajorViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int RequiredCredits { get; set; }

        public MajorViewModel() { }

        public MajorViewModel(Major major)
        {
            Id = major.Id;
            Name = major.Name;
            RequiredCredits = major.RequiredCredits;
        }
    }
}