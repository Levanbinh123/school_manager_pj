namespace SchoolProject.ViewModels
{
    public class ClassViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

       // public int? GradeId { get; set; }

      //  public string? GradeName { get; set; }

        public List<ClassSessionViewModel> ClassSessions { get; set; }
            = new List<ClassSessionViewModel>();
    }
}