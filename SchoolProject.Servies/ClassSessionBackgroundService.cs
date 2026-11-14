using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SchoolProject.Models;
using SchoolProject.Repositories;

namespace SchoolProject.Servies
{
    public class ClassSessionBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ClassSessionBackgroundService> _logger;

        public ClassSessionBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<ClassSessionBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "ClassSessionBackgroundService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateClassSessionStatuses(stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error updating ClassSession statuses.");
                }

                try
                {
                    await Task.Delay(
                        TimeSpan.FromMinutes(1),
                        stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation(
                "ClassSessionBackgroundService stopped.");
        }

        private async Task UpdateClassSessionStatuses(
            CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            DateTime now = DateTime.Now;

            var classSessions = await db.ClassSessions
                .Include(x => x.Enrolls)
                .Where(x =>
                    x.Status == ClassSessionStatus.Planned ||
                    x.Status == ClassSessionStatus.OpenForRegistration ||
                    x.Status == ClassSessionStatus.Full ||
                    x.Status == ClassSessionStatus.RegistrationClosed ||
                    x.Status == ClassSessionStatus.AwaitingPaymentApproval ||
                    x.Status == ClassSessionStatus.PaymentOpen ||
                    x.Status == ClassSessionStatus.PaymentClosed ||
                    x.Status == ClassSessionStatus.Confirmed ||
                    x.Status == ClassSessionStatus.InProgress)
                .ToListAsync(cancellationToken);

            bool changed = false;

            foreach (var entity in classSessions)
            {
                // 1. Đếm sinh viên chưa bị hủy đăng ký
                int actualCount = entity.Enrolls?
    .Count(x =>
        x.Status != EnrollStatus.Dropped &&
        x.Status != EnrollStatus.PaymentExpired) ?? 0;

                if (entity.EnrolledCount != actualCount)
                {
                    entity.EnrolledCount = actualCount;
                    changed = true;
                }

                // 2. Không xử lý lớp đã hủy hoặc hoàn thành
                if (entity.Status == ClassSessionStatus.Cancelled ||
                    entity.Status == ClassSessionStatus.Completed)
                {
                    continue;
                }

                // -----------------------------------------
                // PLANNED -> OPEN / REGISTRATION CLOSED
                // -----------------------------------------
                if (entity.Status == ClassSessionStatus.Planned)
                {
                    if (now >= entity.RegistrationEnd)
                    {
                        entity.Status =
                            ClassSessionStatus.RegistrationClosed;

                        changed = true;
                    }
                    else if (now >= entity.RegistrationStart)
                    {
                        entity.Status =
                            ClassSessionStatus.OpenForRegistration;

                        changed = true;
                    }
                }

                // -----------------------------------------
                // OPEN / FULL -> REGISTRATION CLOSED
                // -----------------------------------------
                if ((entity.Status == ClassSessionStatus.OpenForRegistration ||
                     entity.Status == ClassSessionStatus.Full) &&
                    now >= entity.RegistrationEnd)
                {
                    entity.Status =
                        ClassSessionStatus.RegistrationClosed;

                    changed = true;
                }

                // -----------------------------------------
                // OPEN -> FULL
                // -----------------------------------------
                if (entity.Status == ClassSessionStatus.OpenForRegistration &&
                    actualCount >= entity.MaxStudents)
                {
                    entity.Status = ClassSessionStatus.Full;
                    changed = true;
                }

                // -----------------------------------------
                // FULL -> OPEN
                // -----------------------------------------
                if (entity.Status == ClassSessionStatus.Full &&
                    actualCount < entity.MaxStudents &&
                    now < entity.RegistrationEnd)
                {
                    entity.Status =
                        ClassSessionStatus.OpenForRegistration;

                    changed = true;
                }

                // -----------------------------------------
                // REGISTRATION CLOSED -> AWAITING PAYMENT APPROVAL
                // -----------------------------------------
                if (entity.Status == ClassSessionStatus.RegistrationClosed &&
                    now >= entity.RegistrationEnd &&
                    actualCount >= entity.MinStudents)
                {
                    entity.Status =
                        ClassSessionStatus.AwaitingPaymentApproval;

                    changed = true;

                    _logger.LogInformation(
                        "ClassSession {Id} is waiting for Admin payment approval. Count: {Count}",
                        entity.Id,
                        actualCount);
                }

                // -----------------------------------------
                // PAYMENT OPEN -> PAYMENT CLOSED
                // -----------------------------------------
                if (entity.Status == ClassSessionStatus.PaymentOpen &&
                    entity.PaymentEndDate.HasValue &&
                    now >= entity.PaymentEndDate.Value)
                {
                    entity.Status = ClassSessionStatus.PaymentClosed;
                    changed = true;

                    _logger.LogInformation(
                        "Payment period closed for ClassSession {Id}.",
                        entity.Id);
                }

                // -----------------------------------------
                // CONFIRMED -> IN PROGRESS
                // -----------------------------------------
                if (entity.Status == ClassSessionStatus.Confirmed &&
                    now.Date >= entity.StartDate.Date)
                {
                    entity.Status = ClassSessionStatus.InProgress;
                    changed = true;
                }

                // -----------------------------------------
                // IN PROGRESS -> COMPLETED
                // -----------------------------------------
                if (entity.Status == ClassSessionStatus.InProgress &&
                    now.Date > entity.EndDate.Date)
                {
                    entity.Status = ClassSessionStatus.Completed;
                    changed = true;
                }
            }

            if (changed)
            {
                await db.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Updated ClassSession statuses at {Time}.",
                    now);
            }
        }
    }
}