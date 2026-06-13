using System.ComponentModel.DataAnnotations;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.DTOs;

public record LoginRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required] string Password);

public record RegisterRequest(
    [property: Required] string FullName,
    [property: Required, EmailAddress] string Email,
    [property: Required, MinLength(6)] string Password,
    UserRole Role);

public record AuthResponse(
    string Token,
    DateTime ExpiresAt,
    UserDto User);

public record UserDto(
    int Id,
    string FullName,
    string Email,
    UserRole Role,
    bool IsActive);
