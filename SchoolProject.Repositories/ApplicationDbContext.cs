using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolProject.Models;
using System.Reflection.Emit;

namespace SchoolProject.Repositories
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // =====================================================
        // DbSets
        // =====================================================
        public GradeConfiguration? GradeConfiguration { get; set; }
        public DbSet<SePayPayment> SePayPayments { get; set; }
        public DbSet<StudyPeriod> StudyPeriods { get; set; }
        public DbSet<TuitionRate> sTuitionRates { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Session> YearlySessions { get; set; }
        // public DbSet<Grade> Grades { get; set; }
        // public DbSet<AssignGrade> AssignGrades { get; set; }
        public DbSet<StudentLeave> StudentLeaves { get; set; }
        public DbSet<TeacherSession> TeacherSessions { get; set; }
        public DbSet<Enroll> Enrolls { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        //  public DbSet<GradeSubject> GradeSubjects { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<StudentScore> StudentScores { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<Fee> Fees { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<ClassSession> ClassSessions { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Major> Majors { get; set; }
        public DbSet<MajorSubject> MajorSubjects { get; set; }
        // public DbSet<SubjectYear> SubjectYears { get; set; }
        public DbSet<SessionSubject> SessionSubjects { get; set; }
        public DbSet<AcademicYear> AcademicYears { get; set; }
        public DbSet<VnPayPayment> VnPayPayments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<AcademicYear>()
    .Property(x => x.CloseSignature)
    .HasColumnType("longtext");
            builder.Entity<AcademicYear>(entity =>
             {
                 entity.HasKey(x => x.Id);



                 entity.HasMany(x => x.Sessions)
                     .WithOne(x => x.AcademicYearInfo)
                     .HasForeignKey(x => x.AcademicYearId)
                     .OnDelete(DeleteBehavior.Restrict);
             });
            //trong so tinh diem
            builder.Entity<GradeConfiguration>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => x.ClassSessionId)
                      .IsUnique();

                entity.Property(x => x.AttendanceWeight)
                      .HasPrecision(5, 2);

                entity.Property(x => x.QuizWeight)
                      .HasPrecision(5, 2);

                entity.Property(x => x.MidtermWeight)
                      .HasPrecision(5, 2);

                entity.Property(x => x.FinalWeight)
                      .HasPrecision(5, 2);

                entity.HasOne(x => x.ClassSession)
                      .WithOne(x => x.GradeConfiguration)
                      .HasForeignKey<GradeConfiguration>(
                          x => x.ClassSessionId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            //bao luu
            builder.Entity<StudentLeave>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Reason)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(x => x.Status)
                    .HasConversion<int>();

                entity.HasOne(x => x.Student)
                    .WithMany(x => x.StudentLeaves)
                    .HasForeignKey(x => x.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            // =====================================================
            // Student -> Major
            // =====================================================

            builder.Entity<Student>()
                .HasOne(x => x.Major)
                .WithMany(x => x.Students)
                .HasForeignKey(x => x.MajorId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================================================
            // Subject -> Major
            // =====================================================

            builder.Entity<MajorSubject>()
                .HasKey(x => new { x.MajorId, x.SubjectId });

            builder.Entity<MajorSubject>()
                .HasOne(x => x.Major)
                .WithMany(x => x.MajorSubjects)
                .HasForeignKey(x => x.MajorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MajorSubject>()
                .HasOne(x => x.Subject)
                .WithMany(x => x.MajorSubjects)
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // SubjectYear -> Subject
            // =====================================================

            // =====================================================
            // Fee -> Student
            // =====================================================

            builder.Entity<Fee>()
                .HasOne(x => x.Student)
                .WithMany()
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================================================
            // Fee -> Subject
            // =====================================================

            builder.Entity<Fee>()
                .HasOne(x => x.Subject)
                .WithMany()
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================================================
            // Fee -> Session
            // =====================================================

            builder.Entity<Fee>()
                .HasOne(x => x.Session)
                .WithMany()
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Fee>()
                .HasIndex(x => new
                {
                    x.StudentId,
                    x.SubjectId,
                    x.SessionId
                })
                .IsUnique();

            // =====================================================
            // TuitionRate -> Session
            // =====================================================
            builder.Entity<TuitionRate>()
    .HasOne(x => x.Session)
    .WithMany(x => x.TuitionRates)
    .HasForeignKey(x => x.SessionId)
    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TuitionRate>()
                .HasIndex(x => x.SessionId)
                .IsUnique();

            // =====================================================
            // VnPayPayment -> Fee
            // =====================================================

            builder.Entity<VnPayPayment>()
                .HasOne(x => x.Fee)
                .WithMany(x => x.VnPayPayments)
                .HasForeignKey(x => x.FeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================================================
            // AssignGrade -> Grade
            // =====================================================

            //builder.Entity<AssignGrade>()
            //    .HasOne(x => x.Grade)
            //    .WithMany(x => x.AssignGrades)
            //    .HasForeignKey(x => x.GradeId)
            //    .OnDelete(DeleteBehavior.SetNull);

            // =====================================================
            // AssignGrade -> Teacher
            // =====================================================

            //builder.Entity<AssignGrade>()
            //    .HasIndex(x => new
            //    {
            //        x.TeacherId,
            //        x.GradeId
            //    })
            //    .IsUnique();

            //builder.Entity<AssignGrade>()
            //    .HasOne(x => x.Teacher)
            //    .WithMany(x => x.AssignGrades)
            //    .HasForeignKey(x => x.TeacherId)
            //    .OnDelete(DeleteBehavior.SetNull);

            // =====================================================
            // Enroll -> Student
            // =====================================================

            builder.Entity<Enroll>()
                .HasOne(x => x.Student)
                .WithMany(x => x.Enrolls)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================================================
            // Enroll unique
            // Student + Subject + Session
            // =====================================================

            builder.Entity<Enroll>()
                .HasIndex(x => new
                {
                    x.StudentId,
                    x.SubjectId,
                    x.SessionId
                })
                .IsUnique();

            // =====================================================
            // Enroll -> ClassSession
            // =====================================================

            builder.Entity<Enroll>()
                .HasOne(x => x.ClassSession)
                .WithMany(x => x.Enrolls)
                .HasForeignKey(x => x.ClassSessionId)
                .OnDelete(DeleteBehavior.SetNull);

            // =====================================================
            // Enroll -> Subject
            // =====================================================

            builder.Entity<Enroll>()
                .HasOne(x => x.Subject)
                .WithMany()
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================================================
            // Enroll -> Session
            // =====================================================

            builder.Entity<Enroll>()
                .HasOne(x => x.Session)
                .WithMany()
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================================================
            // TeacherSession
            // =====================================================

            builder.Entity<TeacherSession>()
                .HasIndex(x => new
                {
                    x.TeacherId,
                    x.SessionId
                })
                .IsUnique();

            builder.Entity<TeacherSession>()
                .HasOne(x => x.Teacher)
                .WithMany(x => x.TeacherSessions)
                .HasForeignKey(x => x.TeacherId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<TeacherSession>()
                .HasOne(x => x.Session)
                .WithMany(x => x.TeacherSessions)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.SetNull);

            // =====================================================
            // GradeSubject
            // =====================================================

            //builder.Entity<GradeSubject>()
            //    .HasKey(x => new
            //    {
            //        x.GradeId,
            //        x.SubjectId
            //    });

            builder.Entity<Attendance>()
            .HasOne(x => x.Schedule)
            .WithMany()
            .HasForeignKey(x => x.ScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

            // =====================================================
            // Attendance unique
            // Một sinh viên chỉ có một kết quả điểm danh
            // trong cùng một lịch học và ngày học
            // =====================================================

            builder.Entity<Attendance>()
                .HasIndex(x => new
                {
                    x.ScheduleId,
                    x.EnrollId,
                    x.Date
                })
                .IsUnique();

            // =====================================================
            // StudentScore
            // =====================================================

            builder.Entity<StudentScore>()
                .HasIndex(x => new
                {
                    x.EnrollId,

                })
                .IsUnique();

            builder.Entity<StudentScore>()
                .HasOne(x => x.Enroll)
                .WithMany(x => x.StudentScores)
                .HasForeignKey(x => x.EnrollId)
                .OnDelete(DeleteBehavior.Cascade);


            // =====================================================
            // AuditLog -> User
            // =====================================================

            builder.Entity<AuditLog>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // Assignment -> ClassSession
            // =====================================================

            builder.Entity<Assignment>()
                .HasOne(x => x.ClassSession)
                .WithMany()
                .HasForeignKey(x => x.ClassSessionId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================================================
            // Submission -> Assignment
            // =====================================================

            builder.Entity<Submission>()
                .HasOne(x => x.Assignment)
                .WithMany(x => x.Submissions)
                .HasForeignKey(x => x.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // =====================================================
            // Submission -> Student
            // =====================================================

            builder.Entity<Submission>()
                .HasOne(x => x.Student)
                .WithMany()
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================================================
            // Student chỉ nộp 1 lần / Assignment
            // =====================================================

            builder.Entity<Submission>()
                .HasIndex(x => new
                {
                    x.AssignmentId,
                    x.StudentId
                })
                .IsUnique();

       
            // ClassSession -> Class
            // =====================================================

            builder.Entity<ClassSession>()
                .HasOne(x => x.Class)
                .WithMany(x => x.ClassSessions)
                .HasForeignKey(x => x.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================================================
            // ClassSession -> Subject
            // =====================================================

            builder.Entity<ClassSession>()
                .HasOne(x => x.Subject)
                .WithMany()
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================================================
            // ClassSession -> Session
            // =====================================================

            builder.Entity<ClassSession>()
                .HasOne(x => x.Session)
                .WithMany(x => x.ClassSessions)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================================================
            // ClassSession -> HomeroomTeacher
            // =====================================================

            builder.Entity<ClassSession>()
                .HasOne(x => x.HomeroomTeacher)
                .WithMany()
                .HasForeignKey(x => x.HomeroomTeacherId)
                .OnDelete(DeleteBehavior.SetNull);

            // =====================================================
            // ClassSession unique
            // Class + Subject + Session
            // =====================================================

            builder.Entity<ClassSession>()
                .HasIndex(x => new
                {
                    x.ClassId,
                    x.SubjectId,
                    x.SessionId
                })
                .IsUnique();

            // =====================================================
            // Schedule -> ClassSession
            // =====================================================

            builder.Entity<Schedule>()
                .HasOne(x => x.ClassSession)
                .WithMany()
                .HasForeignKey(x => x.ClassSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // =====================================================
            // Schedule -> Subject
            // =====================================================

            builder.Entity<Schedule>()
                .HasOne(x => x.Subject)
                .WithMany()
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================================================
            // Schedule -> Teacher
            // =====================================================

            builder.Entity<Schedule>()
                .HasOne(x => x.Teacher)
                .WithMany()
                .HasForeignKey(x => x.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================================================
            // ApplicationUser -> Student
            // =====================================================

            builder.Entity<ApplicationUser>()
                .HasOne(x => x.Student)
                .WithOne()
                .HasForeignKey<ApplicationUser>(x => x.StudentId)
                .OnDelete(DeleteBehavior.SetNull);

            // =====================================================
            // ApplicationUser -> Teacher
            // =====================================================

            builder.Entity<ApplicationUser>()
                .HasOne(x => x.Teacher)
                .WithOne()
                .HasForeignKey<ApplicationUser>(x => x.TeacherId)
                .OnDelete(DeleteBehavior.SetNull);

            // =====================================================
            // SessionSubject -> Session
            // =====================================================

            builder.Entity<SessionSubject>()
                .HasOne(x => x.Session)
                .WithMany()
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================================================
            // SessionSubject -> Subject
            // =====================================================

            builder.Entity<SessionSubject>()
                .HasOne(x => x.Subject)
                .WithMany()
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================================================
            // SessionSubject unique
            // Session + Subject
            // =====================================================

            builder.Entity<SessionSubject>()
                .HasIndex(x => new
                {
                    x.SessionId,
                    x.SubjectId
                })
                .IsUnique();
            ///payment 2
            // =====================================================
            // SePayPayment
            // =====================================================

            builder.Entity<SePayPayment>(entity =>
            {
                // Tên bảng đã tồn tại trong MySQL
                entity.ToTable("sepaypayment");

                entity.HasKey(x => x.Id);

                entity.HasIndex(x => x.OrderInvoiceNumber)
                    .IsUnique();

                entity.Property(x => x.OrderInvoiceNumber)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.Amount)
                    .HasPrecision(18, 2);

                entity.Property(x => x.Status)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.HasOne(x => x.Fee)
                    .WithMany(x => x.SePayPayments)
                    .HasForeignKey(x => x.FeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Student)
                    .WithMany()
                    .HasForeignKey(x => x.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}