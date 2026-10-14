using SchoolProject.Models;

public class ClassSession
{
    public int Id { get; set; }

    public int ClassId { get; set; }
    public Class Class { get; set; }

    public int SubjectId { get; set; }
    //so buoi hoc
    public int TotalSessions { get; set; }

    public Subject? Subject { get; set; }
    // Môn/lớp này có sử dụng điểm danh hay không
    public bool HasAttendance { get; set; } = false;
    // Số sinh viên tối thiểu để mở lớp
    public int MinStudents { get; set; } = 10;

    // Số sinh viên tối đa của lớp
    public int MaxStudents { get; set; } = 40;

    public int SessionId { get; set; }
    public Session Session { get; set; }
    //giao vien phu trach mon
    public int? HomeroomTeacherId { get; set; }
    public Teacher? HomeroomTeacher { get; set; }

    public ICollection<Enroll> Enrolls { get; set; }
        = new HashSet<Enroll>();
}