namespace SchoolProject.Models
{
    public class Major
    {
        public int Id { get; set; }
        public string Name { get; set; }
        // Tổng số tín chỉ bắt buộc để tốt nghiệp
        public int RequiredCredits { get; set; } = 120;
        // Số năm đào tạo của ngành
        public int StudyDurationYears { get; set; } = 4;
        public ICollection<Student> Students { get; set; }
            = new HashSet<Student>();
        public ICollection<MajorSubject> MajorSubjects { get; set; }
            = new HashSet<MajorSubject>();
    }
}