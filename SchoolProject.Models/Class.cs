using SchoolProject.Models;

public class Class
{
    public int Id { get; set; }

    public string Name { get; set; }

  //  public int? GradeId { get; set; }
  //  public Grade? Grade { get; set; }

    public ICollection<ClassSession> ClassSessions { get; set; }
        = new HashSet<ClassSession>();
}