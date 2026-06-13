using System.ComponentModel.DataAnnotations;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.DTOs;

public record TaskDto(
    int Id,
    string Title,
    string? Description,
    Models.TaskStatus Status,
    TaskPriority Priority,
    int ProgressPercent,
    DateTime? StartDate,
    DateTime? DueDate,
    DateTime? CompletedDate,
    decimal? EstimatedHours,
    decimal? ActualHours,
    string? Notes,
    int? AssignedToUserId,
    string? AssignedToName,
    int CreatedByUserId,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>صف قادم من الشبكة (الجدول الشبيه بالإكسل). Id == null أو 0 يعني صف جديد.</summary>
public class TaskUpsertDto
{
    public int? Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public Models.TaskStatus Status { get; set; } = Models.TaskStatus.New;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    [Range(0, 100)]
    public int ProgressPercent { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal? ActualHours { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public int? AssignedToUserId { get; set; }
}

/// <summary>حفظ دفعة كاملة من الصفوف (Save الخاص بالجدول)</summary>
public class TaskBulkSaveRequest
{
    public List<TaskUpsertDto> Tasks { get; set; } = new();
    /// <summary>معرّفات الصفوف المحذوفة في الواجهة</summary>
    public List<int> DeletedIds { get; set; } = new();
}
