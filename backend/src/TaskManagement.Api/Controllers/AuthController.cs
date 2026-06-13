using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Models;
using TaskManagement.Api.Services;

namespace TaskManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IJwtService _jwt;

    public AuthController(AppDbContext db, IJwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (user is null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "بيانات الدخول غير صحيحة" });

        var (token, expiresAt) = _jwt.GenerateToken(user);
        return Ok(new AuthResponse(token, expiresAt, ToDto(user)));
    }

    /// <summary>إنشاء حساب — يُسمح بإنشاء مدير فقط للمدراء. التسجيل العام ينشئ موظفاً.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest req)
    {
        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
            return Conflict(new { message = "البريد الإلكتروني مستخدم مسبقاً" });

        // التسجيل العام يُنشئ موظفاً دائماً ما لم يكن الطالب مديراً
        var role = User.IsInRole(nameof(UserRole.Manager)) ? req.Role : UserRole.Employee;

        var user = new User
        {
            FullName = req.FullName,
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = role
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var (token, expiresAt) = _jwt.GenerateToken(user);
        return Ok(new AuthResponse(token, expiresAt, ToDto(user)));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Me()
    {
        var id = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _db.Users.FindAsync(id);
        return user is null ? NotFound() : Ok(ToDto(user));
    }

    private static UserDto ToDto(User u) => new(u.Id, u.FullName, u.Email, u.Role, u.IsActive);
}
