namespace TaskManagement.Api.Models;

/// <summary>أدوار المستخدمين في النظام</summary>
public enum UserRole
{
    Employee = 0,
    Manager = 1
}

/// <summary>حالة المهمة</summary>
public enum TaskStatus
{
    New = 0,
    InProgress = 1,
    OnHold = 2,
    Completed = 3,
    Cancelled = 4
}

/// <summary>أولوية المهمة</summary>
public enum TaskPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}
