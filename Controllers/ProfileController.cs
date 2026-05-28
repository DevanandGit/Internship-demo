using System.Security.Claims;
using InternshipPortal.Data;
using InternshipPortal.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.Controllers;

/// <summary>
/// Exposes the authenticated user's profile information.
/// </summary>
[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController(ApplicationDbContext context) : ControllerBase
{
    /// <summary>
    /// Gets the current user's profile.
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<UserProfileResponse>> GetMe(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await context.Users
            .AsNoTracking()
            .Include(item => item.StudentProfile)
            .Include(item => item.AdminProfile)
            .Include(item => item.Skills)
            .Include(item => item.StudyMaterials)
            .Include(item => item.InternshipApplications)
                .ThenInclude(item => item.Internship)
            .FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        var response = new UserProfileResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString()
        };

        if (user.StudentProfile is not null)
        {
            response.StudentProfile = new StudentProfileResponse
            {
                CollegeName = user.StudentProfile.CollegeName,
                Cgpa = user.StudentProfile.Cgpa,
                BacklogsCount = user.StudentProfile.BacklogsCount,
                StreamBranch = user.StudentProfile.StreamBranch,
                Skills = user.Skills.Select(item => item.StackName).ToList()
            };

            response.StudentProfile.AssignedStudyMaterials = user.StudyMaterials
                .OrderBy(sm => sm.Id)
                .Select(sm => new InternshipPortal.DTOs.StudyMaterialResponse
                {
                    Id = sm.Id,
                    Name = sm.Name,
                    NoteUrl = sm.NoteUrl,
                    VideoUrl = sm.VideoUrl
                })
                .ToList();

            response.AppliedInternships = user.InternshipApplications
                .OrderBy(item => item.Id)
                .Select(item => new AppliedInternshipResponse
                {
                    ApplicationId = item.Id,
                    InternshipId = item.InternshipId,
                    Name = item.Internship.Name,
                    BacklogsCount = item.Internship.BacklogsCount,
                    Cgpa = item.Internship.Cgpa,
                    StreamBranch = item.Internship.StreamBranch,
                    Stipend = item.Internship.Stipend,
                    Duration = item.Internship.Duration,
                    Status = item.Status.ToString(),
                    AppliedAtUtc = item.AppliedAtUtc
                })
                .ToList();
        }

        if (user.AdminProfile is not null)
        {
            response.AdminProfile = new AdminProfileResponse
            {
                Department = user.AdminProfile.Department
            };
        }

        return Ok(response);
    }
}