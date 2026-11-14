using SchoolProject.Models;
//phong hoc
public class Class
{
    public int Id { get; set; }

    public string Name { get; set; }

    //  public int? GradeId { get; set; }
    //  public Grade? Grade { get; set; }
    // Sức chứa tối đa
    public int Capacity { get; set; }
    // Loại phòng: Lý thuyết, Thực hành, Phòng máy...
    public string? RoomType { get; set; }
    // Tòa nhà
    public string? Building { get; set; }
    public ICollection<ClassSession> ClassSessions { get; set; }
        = new HashSet<ClassSession>();
}