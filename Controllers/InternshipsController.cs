using InternshipPortal.Data;
using InternshipPortal.DTOs;
using InternshipPortal.Models;
using InternshipPortal.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.Controllers;

/// <summary>
/// Handles internship browsing, application, review, and admin maintenance operations.
/// </summary>
[ApiController]
[Route("api/internships")]
public class InternshipsController(ApplicationDbContext context, IEmailService emailService) : ControllerBase
{
    /// <summary>
    /// Gets all internships.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResponse<Internship>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        return Ok(await GetPagedInternshipsAsync(
            context.Internships.AsNoTracking().Include(item => item.StudentsApplied),
            pageNumber,
            pageSize,
            cancellationToken));
    }

    /// <summary>
    /// Gets a single internship by its identifier.
    /// </summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<Internship>> GetById(int id, CancellationToken cancellationToken)
    {
        var internship = await context.Internships
            .AsNoTracking()
            .Include(item => item.StudentsApplied)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return internship is null ? NotFound() : Ok(internship);
    }

    /// <summary>
    /// Gets internships that the current student is eligible for.
    /// </summary>
    [HttpGet("eligible")]
    [Authorize(Roles = nameof(UserRole.Student))]
    public async Task<ActionResult<PagedResponse<Internship>>> GetEligible(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var student = await context.Users
            .AsNoTracking()
            .Include(item => item.StudentProfile)
            .FirstOrDefaultAsync(item => item.Id == userId && item.Role == UserRole.Student, cancellationToken);

        if (student?.StudentProfile is null)
        {
            return NotFound(new { message = "Student profile not found." });
        }

        var studentCgpa = student.StudentProfile.Cgpa;
        var studentBacklogs = student.StudentProfile.BacklogsCount;

        var eligibleInternships = await context.Internships
            .AsNoTracking()
            .Include(item => item.StudentsApplied)
            .OrderBy(item => item.Id)
            .ToListAsync(cancellationToken);

        var filtered = eligibleInternships
            .Where(item => IsEligibleForInternship(item, studentCgpa, studentBacklogs))
            .ToList();

        return Ok(CreatePagedResponse(filtered, pageNumber, pageSize));
    }

    /// <summary>
    /// Gets internships already applied to by the current student.
    /// </summary>
    [HttpGet("applied")]
    [Authorize(Roles = nameof(UserRole.Student))]
    public async Task<ActionResult<IEnumerable<AppliedInternshipResponse>>> GetApplied(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var student = await context.Users
            .AsNoTracking()
            .Include(item => item.InternshipApplications)
                .ThenInclude(item => item.Internship)
            .FirstOrDefaultAsync(item => item.Id == userId && item.Role == UserRole.Student, cancellationToken);

        if (student is null)
        {
            return NotFound(new { message = "Student not found." });
        }

        var internships = student.InternshipApplications
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

        return Ok(internships);
    }

    /// <summary>
    /// Applies the current student to an internship.
    /// </summary>
    [HttpPost("{internshipId:int}/apply")]
    [Authorize(Roles = nameof(UserRole.Student))]
    public async Task<IActionResult> Apply(int internshipId, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var student = await context.Users
            .Include(item => item.StudentProfile)
            .Include(item => item.InternshipApplications)
            .FirstOrDefaultAsync(item => item.Id == userId && item.Role == UserRole.Student, cancellationToken);

        if (student?.StudentProfile is null)
        {
            return NotFound(new { message = "Student profile not found." });
        }

        var internship = await context.Internships.FirstOrDefaultAsync(item => item.Id == internshipId, cancellationToken);
        if (internship is null)
        {
            return NotFound(new { message = "Internship not found." });
        }

        if (student.InternshipApplications.Any(item => item.InternshipId == internshipId))
        {
            return Conflict(new { message = "You have already applied for this internship." });
        }

        var studentCgpa = student.StudentProfile.Cgpa;
        var studentBacklogs = student.StudentProfile.BacklogsCount;

        var eligible = IsEligibleForInternship(internship, studentCgpa, studentBacklogs);

        if (!eligible)
        {
            return BadRequest(new { message = "You are not eligible for this internship." });
        }

        var application = new InternshipApplication
        {
            InternshipId = internship.Id,
            StudentUserId = student.Id,
            Status = ApplicationStatus.Pending
        };

        context.InternshipApplications.Add(application);
        await context.SaveChangesAsync(cancellationToken);

        await emailService.SendAsync(
            student.Email,
            $"Applied successfully for {internship.Name}",
            $"Hello {student.Name},\n\nYou have successfully applied for internship: {internship.Name}.\nDuration: {internship.Duration}\nStipend: {(internship.Stipend.HasValue ? internship.Stipend.Value.ToString("0.00") : "N/A")}\nCGPA Requirement: {(internship.Cgpa.HasValue ? internship.Cgpa.Value.ToString("0.00") : "N/A")}\nBacklogs Requirement: {(internship.BacklogsCount.HasValue ? internship.BacklogsCount.Value.ToString() : "N/A")}\n\nWe will review your application and update you soon.\n\nRegards,\nInternship Portal",
            cancellationToken);

        return Ok(new { message = "Applied successfully." });
    }

    /// <summary>
    /// Gets all internship applications (admin). Can be filtered by internshipId via query string.
    /// Example: GET /api/internships/applications or GET /api/internships/applications?internshipId=5
    /// </summary>
    [HttpGet("applications")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<IEnumerable<AppliedStudentResponse>>> GetApplications([FromQuery] int? internshipId, CancellationToken cancellationToken)
    {
        var query = context.InternshipApplications
            .AsNoTracking()
            .Include(item => item.StudentUser)
                .ThenInclude(item => item.StudentProfile)
            .AsQueryable();

        if (internshipId.HasValue)
        {
            query = query.Where(item => item.InternshipId == internshipId.Value);
        }

        var applications = await query.OrderBy(item => item.Id).ToListAsync(cancellationToken);

        if (applications.Count == 0 && internshipId.HasValue)
        {
            var internshipExists = await context.Internships.AnyAsync(item => item.Id == internshipId.Value, cancellationToken);
            if (!internshipExists)
            {
                return NotFound(new { message = "Internship not found." });
            }

            return Ok(Array.Empty<AppliedStudentResponse>());
        }

        var response = applications
            .Select(item => new AppliedStudentResponse
            {
                ApplicationId = item.Id,
                StudentId = item.StudentUserId,
                Name = item.StudentUser.Name,
                Email = item.StudentUser.Email,
                CollegeName = item.StudentUser.StudentProfile?.CollegeName,
                Cgpa = item.StudentUser.StudentProfile?.Cgpa,
                BacklogsCount = item.StudentUser.StudentProfile?.BacklogsCount,
                StreamBranch = item.StudentUser.StudentProfile?.StreamBranch,
                Status = item.Status.ToString(),
                AppliedAtUtc = item.AppliedAtUtc
            })
            .ToList();

        return Ok(response);
    }

    /// <summary>
    /// Approves a pending internship application.
    /// </summary>
    [HttpPost("applications/{applicationId:int}/approve")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> ApproveApplication(int applicationId, ReviewApplicationRequest request, CancellationToken cancellationToken)
    {
        return await ReviewApplication(applicationId, ApplicationStatus.Approved, request, cancellationToken);
    }

    /// <summary>
    /// Rejects a pending internship application.
    /// </summary>
    [HttpPost("applications/{applicationId:int}/reject")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> RejectApplication(int applicationId, ReviewApplicationRequest request, CancellationToken cancellationToken)
    {
        return await ReviewApplication(applicationId, ApplicationStatus.Rejected, request, cancellationToken);
    }

    private async Task<IActionResult> ReviewApplication(int applicationId, ApplicationStatus targetStatus, ReviewApplicationRequest request, CancellationToken cancellationToken)
    {
        var adminIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(adminIdClaim, out var adminId))
        {
            return Unauthorized();
        }

        var application = await context.InternshipApplications
            .Include(item => item.Internship)
            .Include(item => item.StudentUser)
            .FirstOrDefaultAsync(item => item.Id == applicationId, cancellationToken);

        if (application is null)
        {
            return NotFound(new { message = "Application not found." });
        }

        if (application.Status != ApplicationStatus.Pending)
        {
            return Conflict(new { message = "This application has already been reviewed." });
        }

        application.Status = targetStatus;
        application.ReviewedAtUtc = DateTime.UtcNow;
        application.ReviewedByUserId = adminId;

        await context.SaveChangesAsync(cancellationToken);

        var subject = targetStatus == ApplicationStatus.Approved
            ? $"You have been selected for interview for {application.Internship.Name}"
            : $"Update on your application for {application.Internship.Name}";

        var body = targetStatus == ApplicationStatus.Approved
            ? $"Hello {application.StudentUser.Name},\n\nYour application suits our requirements and you have been selected for interview for the {application.Internship.Name} internship.\n\nWe appreciate your effort and look forward to speaking with you.\n\nRegards,\nInternship Portal"
            : $"Hello {application.StudentUser.Name},\n\nAfter reviewing your application for {application.Internship.Name}, we are unable to move forward at this time. Please do not lose hope. Your journey is still unfolding, and the right opportunity is on its way.\n\nKeep building, keep learning, and keep applying.\n\nRegards,\nInternship Portal";

        if (!string.IsNullOrWhiteSpace(request.Note))
        {
            body += $"\n\nNote from the reviewer: {request.Note}";
        }

        await emailService.SendAsync(application.StudentUser.Email, subject, body, cancellationToken);

        return Ok(new { message = targetStatus.ToString(), applicationId });
    }

    /// <summary>
    /// Creates a new internship.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<Internship>> Create(InternshipRequest request, CancellationToken cancellationToken)
    {
        var internship = new Internship
        {
            Name = request.Name,
            BacklogsCount = request.BacklogsCount,
            Cgpa = request.Cgpa,
            StreamBranch = request.StreamBranch,
            Stipend = request.Stipend,
            Duration = request.Duration
        };

        if (request.StudentIds.Count > 0)
        {
            internship.StudentsApplied = await context.Users
                .Where(item => request.StudentIds.Contains(item.Id) && item.Role == UserRole.Student)
                .ToListAsync(cancellationToken);
        }

        context.Internships.Add(internship);
        await context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = internship.Id }, internship);
    }

    /// <summary>
    /// Updates an existing internship.
    /// </summary>
    [HttpPatch("{id:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<Internship>> Update(int id, InternshipRequest request, CancellationToken cancellationToken)
    {
        var internship = await context.Internships
            .Include(item => item.StudentsApplied)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (internship is null)
        {
            return NotFound();
        }

        internship.Name = request.Name;
        internship.BacklogsCount = request.BacklogsCount;
        internship.Cgpa = request.Cgpa;
        internship.StreamBranch = request.StreamBranch;
        internship.Stipend = request.Stipend;
        internship.Duration = request.Duration;

        if (request.StudentIds.Count > 0)
        {
            var students = await context.Users
                .Where(item => request.StudentIds.Contains(item.Id) && item.Role == UserRole.Student)
                .ToListAsync(cancellationToken);

            internship.StudentsApplied = students;
        }

        await context.SaveChangesAsync(cancellationToken);
        return Ok(internship);
    }

    /// <summary>
    /// Deletes an internship.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var internship = await context.Internships.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (internship is null)
        {
            return NotFound();
        }

        context.Internships.Remove(internship);
        await context.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Internship deleted successfully." });
    }
    private static bool IsEligibleForInternship(Internship internship, decimal? studentCgpa, int? studentBacklogs)
    {
        var cgpaMatches = !internship.Cgpa.HasValue || !studentCgpa.HasValue || studentCgpa >= internship.Cgpa;
        var backlogMatches = !internship.BacklogsCount.HasValue || !studentBacklogs.HasValue || studentBacklogs <= internship.BacklogsCount;

        return cgpaMatches || backlogMatches;
    }

    private static PagedResponse<Internship> CreatePagedResponse(List<Internship> items, int pageNumber, int pageSize)
    {
        var safePageNumber = pageNumber < 1 ? 1 : pageNumber;
        var safePageSize = pageSize < 1 ? 10 : pageSize;
        var totalCount = items.Count;
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)safePageSize);
        var pageItems = items
            .Skip((safePageNumber - 1) * safePageSize)
            .Take(safePageSize)
            .ToList();

        return new PagedResponse<Internship>
        {
            Items = pageItems,
            PageNumber = safePageNumber,
            PageSize = safePageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    private static async Task<PagedResponse<Internship>> GetPagedInternshipsAsync(
        IQueryable<Internship> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var safePageNumber = pageNumber < 1 ? 1 : pageNumber;
        var safePageSize = pageSize < 1 ? 10 : pageSize;
        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)safePageSize);
        var items = await query
            .OrderBy(item => item.Id)
            .Skip((safePageNumber - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<Internship>
        {
            Items = items,
            PageNumber = safePageNumber,
            PageSize = safePageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}