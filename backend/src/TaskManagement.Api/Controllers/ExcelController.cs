using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.Models;
using TaskManagement.Api.Services;

namespace TaskManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExcelController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IExcelService _excel;

    public ExcelController(AppDbContext db, IExcelService excel)
    {
        _db = db;
        _excel = excel;
    }

    private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    private bool IsManager => User.IsInRole(nameof(UserRole.Manager));

    private const string XlsxContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    /// <summary>تصدير المهام إلى ملف Excel (.xlsx)</summary>
    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var query = _db.Tasks.AsQueryable();
        if (!IsManager) query = query.Where(t => t.AssignedToUserId == CurrentUserId);

        var tasks = await query.OrderByDescending(t => t.UpdatedAt).ToListAsync();
        var userNames = await _db.Users.ToDictionaryAsync(u => u.Id, u => u.FullName);

        var bytes = _excel.ExportTasks(tasks, userNames);
        var fileName = $"tasks_{DateTime.UtcNow:yyyyMMdd_HHmm}.xlsx";
        return File(bytes, XlsxContentType, fileName);
    }

    /// <summary>قالب Excel فارغ بالأعمدة الصحيحة</summary>
    [HttpGet("template")]
    public IActionResult Template()
    {
        var bytes = _excel.ExportTasks(Array.Empty<TaskItem>(), new Dictionary<int, string>());
        return File(bytes, XlsxContentType, "tasks_template.xlsx");
    }

    /// <summary>استيراد مهام من ملف Excel وحفظها في قاعدة البيانات</summary>
    [HttpPost("import")]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "الرجاء رفع ملف Excel صالح" });

        var users = await _db.Users.ToListAsync();
        var byEmail = users.ToDictionary(u => u.Email.ToLower(), u => u.Id);

        List<ImportedTaskRow> rows;
        using (var stream = file.OpenReadStream())
            rows = _excel.ImportTasks(stream);

        var created = new List<TaskItem>();
        var errors = new List<object>();

        foreach (var r in rows)
        {
            var task = new TaskItem
            {
                Title = r.Title,
                Description = r.Description,
                Status = ParseEnum(r.Status, Models.TaskStatus.New),
                Priority = ParseEnum(r.Priority, TaskPriority.Medium),
                ProgressPercent = Math.Clamp(r.ProgressPercent, 0, 100),
                StartDate = r.StartDate,
                DueDate = r.DueDate,
                EstimatedHours = r.EstimatedHours,
                ActualHours = r.ActualHours,
                Notes = r.Notes,
                CreatedByUserId = CurrentUserId
            };

            if (IsManager && !string.IsNullOrWhiteSpace(r.AssignedToEmail)
                && byEmail.TryGetValue(r.AssignedToEmail.ToLower(), out var uid))
                task.AssignedToUserId = uid;
            else if (!IsManager)
                task.AssignedToUserId = CurrentUserId;

            created.Add(task);
        }

        _db.Tasks.AddRange(created);
        await _db.SaveChangesAsync();

        return Ok(new { imported = created.Count, errors });
    }

    private static TEnum ParseEnum<TEnum>(string? value, TEnum fallback) where TEnum : struct =>
        Enum.TryParse<TEnum>(value, true, out var v) ? v : fallback;
}
