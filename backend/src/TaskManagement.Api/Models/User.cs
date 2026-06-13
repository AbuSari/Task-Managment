using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Api.Models;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Employee;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>المهام المسندة إلى هذا المستخدم</summary>
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
}
