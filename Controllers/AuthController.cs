using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InternshipPortal.Data;
using InternshipPortal.DTOs;
using InternshipPortal.Models;
using InternshipPortal.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace InternshipPortal.Controllers;

/// <summary>
/// Handles authentication, registration, and password management endpoints.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController(ApplicationDbContext context, IConfiguration configuration, IEmailService emailService) : ControllerBase
{
    private readonly PasswordHasher<PortalUser> passwordHasher = new();

    /// <summary>
    /// Registers a new student account and returns a JWT access token.
    /// </summary>
    [HttpPost("student/register")]
    public async Task<ActionResult<AuthResponse>> RegisterStudent(StudentRegistrationRequest request, CancellationToken cancellationToken)
    {
        return await RegisterUserAsync(
            request.Name,
            request.Email,
            request.Password,
            UserRole.Student,
            request.SkillIds,
            studentProfile: new StudentProfile
            {
                CollegeName = request.CollegeName,
                Cgpa = request.Cgpa,
                BacklogsCount = request.BacklogsCount,
                StreamBranch = request.StreamBranch
            },
            adminProfile: null,
            cancellationToken);
    }

    /// <summary>
    /// Registers a new admin account and returns a JWT access token.
    /// </summary>
    [HttpPost("admin/register")]
    public async Task<ActionResult<AuthResponse>> RegisterAdmin(AdminRegistrationRequest request, CancellationToken cancellationToken)
    {
        return await RegisterUserAsync(
            request.Name,
            request.Email,
            request.Password,
            UserRole.Admin,
            skillIds: [],
            studentProfile: null,
            adminProfile: new AdminProfile
            {
                Department = request.Department
            },
            cancellationToken);
    }

    /// <summary>
    /// Authenticates a user and returns a JWT access token.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Include(item => item.StudentProfile)
            .Include(item => item.AdminProfile)
            .FirstOrDefaultAsync(item => item.Email == request.Username, cancellationToken);

        if (user is null)
        {
            return Unauthorized();
        }

        var verification = passwordHasher.VerifyHashedPassword(user, user.Password, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return Unauthorized();
        }

        return Ok(CreateTokenResponse(user));
    }

    /// <summary>
    /// Generates a password reset token and sends the reset link by email.
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await context.Users.FirstOrDefaultAsync(item => item.Email == request.Email, cancellationToken);

        if (user is null)
        {
            return Ok(new ForgotPasswordResponse
            {
                Message = "If the email exists, a reset link has been generated."
            });
        }

        var token = Guid.NewGuid().ToString("N");
        context.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            Token = token,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(30)
        });

        await context.SaveChangesAsync(cancellationToken);

        var frontendBaseUrl = configuration["FrontendBaseUrl"] ?? "http://localhost:4200";
        var resetLink = $"{frontendBaseUrl}/forgot-password?token={token}";

        await emailService.SendAsync(
            user.Email,
            "Reset your password",
            $"Hello {user.Name},\n\nUse this link to reset your password:\n{resetLink}\n\nThis link will expire in 30 minutes.",
            cancellationToken);

        return Ok(new ForgotPasswordResponse
        {
            Message = "If the email exists, a reset link has been generated.",
            ResetLink = resetLink
        });
    }

    /// <summary>
    /// Resets a password using a valid reset token.
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<ActionResult<PasswordActionResponse>> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var resetToken = await context.PasswordResetTokens
            .Include(item => item.User)
            .FirstOrDefaultAsync(item => item.Token == request.Token, cancellationToken);

        if (resetToken is null || resetToken.UsedAtUtc is not null || resetToken.ExpiresAtUtc < DateTime.UtcNow)
        {
            return BadRequest(new { message = "Invalid or expired reset token." });
        }

        resetToken.User.Password = passwordHasher.HashPassword(resetToken.User, request.NewPassword);
        resetToken.UsedAtUtc = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Ok(new PasswordActionResponse { Message = "Your password has been reset successfully." });
    }

    /// <summary>
    /// Changes the password for the currently authenticated user.
    /// </summary>
    [HttpPost("change-password")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<PasswordActionResponse>> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await context.Users.FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);
        if (user is null)
        {
            return NotFound(new { message = "User not found." });
        }

        var verification = passwordHasher.VerifyHashedPassword(user, user.Password, request.CurrentPassword);
        if (verification == PasswordVerificationResult.Failed)
        {
            return BadRequest(new { message = "Current password is incorrect." });
        }

        user.Password = passwordHasher.HashPassword(user, request.NewPassword);
        await context.SaveChangesAsync(cancellationToken);

        return Ok(new PasswordActionResponse { Message = "Password updated successfully." });
    }

    private async Task<ActionResult<AuthResponse>> RegisterUserAsync(
        string name,
        string email,
        string password,
        UserRole role,
        List<int> skillIds,
        StudentProfile? studentProfile,
        AdminProfile? adminProfile,
        CancellationToken cancellationToken)
    {
        var existingUser = await context.Users.AnyAsync(item => item.Email == email, cancellationToken);
        if (existingUser)
        {
            return Conflict(new { message = "Email is already registered." });
        }

        var user = new PortalUser
        {
            Name = name,
            Email = email,
            Role = role
        };

        user.Password = passwordHasher.HashPassword(user, password);

        if (skillIds.Count > 0)
        {
            var skills = await context.Skills
                .Where(item => skillIds.Contains(item.Id))
                .ToListAsync(cancellationToken);

            if (skills.Count != skillIds.Count)
            {
                return BadRequest(new { message = "One or more skill IDs were not found." });
            }

            user.Skills = skills;
        }

        if (studentProfile is not null)
        {
            user.StudentProfile = studentProfile;
        }

        if (adminProfile is not null)
        {
            user.AdminProfile = adminProfile;
        }

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return Ok(CreateTokenResponse(user));
    }

    private AuthResponse CreateTokenResponse(PortalUser user)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddHours(8);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Email),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("uid", user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new AuthResponse
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expiresAt
        };
    }
}