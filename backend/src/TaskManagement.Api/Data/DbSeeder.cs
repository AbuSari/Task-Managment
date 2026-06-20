using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Data;

/// <summary>تهيئة قاعدة البيانات ببيانات مبدئية (مدير + موظف + مهام أمثلة)</summary>
public static class DbSeeder
{
    /// <summary>محاولة الاتصال بقاعدة البيانات عدة مرات قبل الاستسلام.</summary>
    private static async Task WaitForDatabaseAsync(AppDbContext db, ILogger? logger)
    {
        const int maxAttempts = 12;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                if (await db.Database.CanConnectAsync())
                    return;
            }
            catch (Exception ex)
            {
                logger?.LogWarning("في انتظار قاعدة البيانات (محاولة {Attempt}/{Max}): {Message}",
                    attempt, maxAttempts, ex.Message);
            }
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }

    public static async Task SeedAsync(AppDbContext db, ILogger? logger = null)
    {
        // الانتظار حتى تصبح قاعدة البيانات جاهزة (مهم عند التشغيل عبر Docker)
        await WaitForDatabaseAsync(db, logger);

        await db.Database.MigrateAsync();

        if (await db.Users.AnyAsync())
            return;

        var manager = new User
        {
            FullName = "مدير النظام",
            Email = "manager@task.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager@123"),
            Role = UserRole.Manager
        };

        var employee = new User
        {
            FullName = "موظف تجريبي",
            Email = "employee@task.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Employee@123"),
            Role = UserRole.Employee
        };

        db.Users.AddRange(manager, employee);
        await db.SaveChangesAsync();

        db.Tasks.AddRange(
            new TaskItem
            {
                Title = "إعداد تقرير المبيعات الشهري",
                Description = "تجميع أرقام المبيعات وإعداد التقرير",
                Status = Models.TaskStatus.InProgress,
                Priority = TaskPriority.High,
                ProgressPercent = 40,
                StartDate = DateTime.UtcNow.AddDays(-3),
                DueDate = DateTime.UtcNow.AddDays(4),
                EstimatedHours = 8,
                ActualHours = 3,
                AssignedToUserId = employee.Id,
                CreatedByUserId = manager.Id
            },
            new TaskItem
            {
                Title = "مراجعة طلبات العملاء",
                Status = Models.TaskStatus.New,
                Priority = TaskPriority.Medium,
                DueDate = DateTime.UtcNow.AddDays(7),
                EstimatedHours = 5,
                AssignedToUserId = employee.Id,
                CreatedByUserId = manager.Id
            }
        );
        await db.SaveChangesAsync();
    }
}
