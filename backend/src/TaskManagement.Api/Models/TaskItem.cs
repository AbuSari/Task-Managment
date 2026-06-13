using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Api.Models;

/// <summary>المهمة — تمثل صفاً واحداً في "شيت" المهام</summary>
public class TaskItem
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public TaskStatus Status { get; set; } = TaskStatus.New;

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    /// <summary>نسبة الإنجاز 0..100</summary>
    public int ProgressPercent { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    /// <summary>عدد الساعات المقدّرة</summary>
    public decimal? EstimatedHours { get; set; }

    /// <summary>عدد الساعات الفعلية</summary>
    public decimal? ActualHours { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>المستخدم المسند إليه المهمة</summary>
    public int? AssignedToUserId { get; set; }
    public User? AssignedTo { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
