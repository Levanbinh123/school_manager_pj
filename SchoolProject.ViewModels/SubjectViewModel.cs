using SchoolProject.Models;


namespace SchoolProject.ViewModels
{
    public class SubjectViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public int Credits {  get; set; }

        public SubjectViewModel()
        {
        }

        public SubjectViewModel(Subject subject)
        {
            Id = subject.Id;
            Name = subject.Name;
            Credits = subject.Credits;
        }
    }
}