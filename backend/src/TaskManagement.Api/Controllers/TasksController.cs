using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _db;

    public TasksController(AppDbContext db) => _db = db;

    private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    private bool IsManager => User.IsInRole(nameof(UserRole.Manager));

    /// <summary>المهام: المدير يرى الجميع، الموظف يرى المسندة إليه فقط.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetAll()
    {
        var query = _db.Tasks.Include(t => t.AssignedTo).AsQueryable();
        if (!IsManager)
            query = query.Where(t => t.AssignedToUserId == CurrentUserId);

        var tasks = await query.OrderByDescending(t => t.UpdatedAt).ToListAsync();
        return Ok(tasks.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskDto>> GetById(int id)
    {
        var task = await _db.Tasks.Include(t => t.AssignedTo).FirstOrDefaultAsync(t => t.Id == id);
        if (task is null) return NotFound();
        if (!IsManager && task.AssignedToUserId != CurrentUserId) return Forbid();
        return Ok(ToDto(task));
    }

    /// <summary>
    /// حفظ دفعة كاملة من صفوف الجدول (إنشاء/تعديل/حذف) في عملية واحدة — يخدم زر "حفظ" في شبكة الإكسل.
    /// </summary>
    [HttpPost("bulk-save")]
    public async Task<ActionResult<IEnumerable<TaskDto>>> BulkSave(TaskBulkSaveRequest req)
    {
        var uid = CurrentUserId;

        // الحذف
        if (req.DeletedIds.Count > 0)
        {
            var toDelete = await _db.Tasks.Where(t => req.DeletedIds.Contains(t.Id)).ToListAsync();
            foreach (var t in toDelete)
            {
                if (!IsManager && t.AssignedToUserId != uid) return Forbid();
                _db.Tasks.Remove(t);
            }
        }

        foreach (var row in req.Tasks)
        {
            if (row.Id is int id && id > 0)
            {
                var task = await _db.Tasks.FindAsync(id);
                if (task is null) continue;
                if (!IsManager && task.AssignedToUserId != uid) return Forbid();

                Apply(task, row);
                task.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var task = new TaskItem { CreatedByUserId = uid };
                Apply(task, row);
                // الموظف لا يستطيع إسناد المهمة لغيره
                if (!IsManager) task.AssignedToUserId = uid;
                _db.Tasks.Add(task);
            }
        }

        await _db.SaveChangesAsync();
        return await GetAll();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task is null) return NotFound();
        if (!IsManager && task.AssignedToUserId != CurrentUserId) return Forbid();
        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private void Apply(TaskItem task, TaskUpsertDto row)
    {
        task.Title = row.Title;
        task.Description = row.Description;
        task.Status = row.Status;
        task.Priority = row.Priority;
        task.ProgressPercent = Math.Clamp(row.ProgressPercent, 0, 100);
        task.StartDate = row.StartDate;
        task.DueDate = row.DueDate;
        task.EstimatedHours = row.EstimatedHours;
        task.ActualHours = row.ActualHours;
        task.Notes = row.Notes;

        if (row.Status == Models.TaskStatus.Completed)
        {
            task.ProgressPercent = 100;
            task.CompletedDate ??= DateTime.UtcNow;
        }
        else
        {
            task.CompletedDate = row.CompletedDate;
        }

        if (IsManager)
            task.AssignedToUserId = row.AssignedToUserId;
    }

    internal static TaskDto ToDto(TaskItem t) => new(
        t.Id, t.Title, t.Description, t.Status, t.Priority, t.ProgressPercent,
        t.StartDate, t.DueDate, t.CompletedDate, t.EstimatedHours, t.ActualHours,
        t.Notes, t.AssignedToUserId, t.AssignedTo?.FullName, t.CreatedByUserId,
        t.CreatedAt, t.UpdatedAt);
}
