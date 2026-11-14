using Amazon.Runtime;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SchoolProject.Models;
using SchoolProject.Repositories;
using SchoolProject.Servies;
using SchoolProject.Servies.Academiyear;
using SchoolProject.Utilities;
using SchoolProject.ViewModels;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<VnPaySettings>(
    builder.Configuration.GetSection("VnPay"));
builder.Services.Configure<CloudflareR2Options>(
    builder.Configuration.GetSection("CloudflareR2"));

// =========================
// CORS
// =========================
builder.Services.AddCors(options =>
{
    options.AddPolicy("FlutterPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// =========================
// Database
// =========================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("DefaultConnection")
        )
    ));

// =========================
// Identity
// =========================
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// =========================
// JWT Authentication
// =========================
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwtSettings =
            builder.Configuration.GetSection("Jwt");

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,

                ValidateAudience = true,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    jwtSettings["Issuer"],

                ValidAudience =
                    jwtSettings["Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtSettings["Key"]!
                        )
                    ),

                ClockSkew = TimeSpan.Zero
            };
    });

// =========================
// Controllers
// =========================
builder.Services.AddControllers();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var options = sp.GetRequiredService<
        IOptions<CloudflareR2Options>>().Value;

    var config = new AmazonS3Config
    {
        ServiceURL = $"https://{options.AccountId}.r2.cloudflarestorage.com",
        ForcePathStyle = true,
        AuthenticationRegion = "auto"
    };

    return new AmazonS3Client(
        options.AccessKeyId,
        options.SecretAccessKey,
        config
    );
});

builder.Services.AddSingleton<IFileStorageService, CloudflareR2StorageService>();
// =========================
// Dependency Injection
// =========================
builder.Services.AddScoped<IDblnitilizer, DbInitializer>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailSender<ApplicationUser>, EmailSender>();
builder.Services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ISessionService, SessionService>();
//builder.Services.AddScoped<IGradeService, GradeService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<IAuthService,AuthService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IAttendanceService,AttendanceService>();
builder.Services.AddScoped<IFeeService, FeeService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IMajorSubjectService, MajorSubjectService>();
builder.Services.AddScoped<IAcademicYearService, AcademicYearService>();
builder.Services.AddScoped<IStudyPeriodService, StudyPeriodService>();
builder.Services.AddHostedService<ClassSessionBackgroundService>();
builder.Services.AddScoped<
    IAdminDashboardService,
    AdminDashboardService>();
builder.Services.AddScoped<
    ITuitionReportService,
    TuitionReportService>();
builder.Services.AddScoped<
    IAcademicReportService,
    AcademicReportService>();
builder.Services.AddScoped<
    IGradeConfigurationService,
    GradeConfigurationService>();
builder.Services.AddScoped<
    IStudentLeaveService,
    StudentLeaveService>();
builder.Services.AddScoped<
    IStudentScoreService,
    StudentScoreService>();
builder.Services.AddScoped<
    ITuitionRateService,
    TuitionRateService>();

builder.Services.AddScoped<
    IAssignmentService,
    AssignmentService>();
builder.Services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddScoped<IMajorService, MajorService>();
builder.Services.Configure<SePaySettings>(
    builder.Configuration.GetSection("SePay"));
builder.Services.AddScoped<ISePayService, SePayService>();
var app = builder.Build();

// =========================
// HTTP Pipeline
// =========================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

DataSeeding();

app.UseRouting();

app.UseCors("FlutterPolicy");

// QUAN TRỌNG
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// =========================
// Data Seeding
// =========================
void DataSeeding()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitilizer =
            scope.ServiceProvider
                .GetRequiredService<IDblnitilizer>();

        dbInitilizer.Initialize();
    }
}