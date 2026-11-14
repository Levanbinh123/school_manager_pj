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
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ClassSessionStatus Status { get; set; }
    = ClassSessionStatus.Planned;
    public DateTime RegistrationStart { get; set; }
    public DateTime RegistrationEnd { get; set; }
    public int EnrolledCount { get; set; } = 0;
    // Thời điểm bắt đầu thanh toán
    public DateTime? PaymentStartDate { get; set; }

    // Thời điểm kết thúc thanh toán
    public DateTime? PaymentEndDate { get; set; }
    public GradeConfiguration? GradeConfiguration { get; set; }
}
public enum ClassSessionStatus
{
    Planned = 0,
    OpenForRegistration = 1,
    Full = 2,
    Confirmed = 3,
    InProgress = 4,
    Completed = 5,
    Cancelled = 6,
    RegistrationClosed = 7,
    AwaitingPaymentApproval = 8,
    PaymentOpen = 9,
    PaymentClosed = 10
}
//Trạng thái	Ý nghĩa
//Planned	Đã tạo lớp, chưa mở đăng ký
//OpenForRegistration	Đang nhận sinh viên
//Full	Đã đủ số lượng
//Confirmed	Đủ điều kiện mở lớp
//InProgress	Đang giảng dạy
//Completed	Đã hoàn thành
//Cancelled	Lớp bị hủy