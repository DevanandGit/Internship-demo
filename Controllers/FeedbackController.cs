using System.Security.Claims;
using InternshipPortal.Data;
using InternshipPortal.DTOs;
using InternshipPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.Controllers;

[ApiController]
[Route("api/internships/{internshipId:int}/feedback")]
public class FeedbackController(ApplicationDbContext context) : ControllerBase
{
    [HttpPost("/api/internships/{internshipId:int}/feedback/timer")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<FeedbackTimerResponse>> CreateOrUpdateTimer(int internshipId, CreateFeedbackTimerRequest request, CancellationToken cancellationToken)
    {
        var internshipExists = await context.Internships.AnyAsync(i => i.Id == internshipId, cancellationToken);
        if (!internshipExists) return NotFound(new { message = "Internship not found." });

        var existing = await context.FeedbackTimers.FirstOrDefaultAsync(ft => ft.InternshipId == internshipId, cancellationToken);
        if (existing is null)
        {
            var timer = new FeedbackTimer
            {
                InternshipId = internshipId,
                StartUtc = request.StartUtc,
                EndUtc = request.EndUtc
            };

            context.FeedbackTimers.Add(timer);
            await context.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(null, new FeedbackTimerResponse
            {
                Id = timer.Id,
                InternshipId = timer.InternshipId,
                StartUtc = timer.StartUtc,
                EndUtc = timer.EndUtc
            });
        }

        existing.StartUtc = request.StartUtc;
        existing.EndUtc = request.EndUtc;
        await context.SaveChangesAsync(cancellationToken);

        return Ok(new FeedbackTimerResponse
        {
            Id = existing.Id,
            InternshipId = existing.InternshipId,
            StartUtc = existing.StartUtc,
            EndUtc = existing.EndUtc
        });
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Student))]
    public async Task<ActionResult<FeedbackResponse>> SubmitFeedback(int internshipId, CreateFeedbackRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        // Ensure student is an approved applicant for this internship
        var application = await context.InternshipApplications.FirstOrDefaultAsync(a => a.InternshipId == internshipId && a.StudentUserId == userId, cancellationToken);
        if (application is null || application.Status != ApplicationStatus.Approved)
        {
            return Forbid();
        }

        var internship = await context.Internships.FirstOrDefaultAsync(item => item.Id == internshipId, cancellationToken);
        if (internship is null)
        {
            return NotFound(new { message = "Internship not found." });
        }

        // Prevent duplicate feedback
        var already = await context.Feedbacks.AnyAsync(f => f.InternshipId == internshipId && f.StudentUserId == userId, cancellationToken);
        if (already) return Conflict(new { message = "Feedback already submitted." });

        var feedback = new Feedback
        {
            InternshipId = internshipId,
            StudentUserId = userId,
            Rating = request.Rating,
            Comments = request.Comments,
            SubmittedAtUtc = DateTime.UtcNow
        };

        context.Feedbacks.Add(feedback);
        await context.SaveChangesAsync(cancellationToken);

        var response = new FeedbackResponse
        {
            Id = feedback.Id,
            InternshipId = feedback.InternshipId,
            InternshipName = internship.Name,
            StudentUserId = feedback.StudentUserId,
            StudentName = null,
            StudentEmail = null,
            Rating = feedback.Rating,
            Comments = feedback.Comments,
            SubmittedAtUtc = feedback.SubmittedAtUtc
        };

        return CreatedAtAction(null, response);
    }

    [HttpGet("/api/feedbacks")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<PagedResponse<FeedbackResponse>>> ListAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? internshipId = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Feedbacks
            .AsNoTracking()
            .Include(item => item.Internship)
            .Include(item => item.StudentUser)
            .AsQueryable();
        if (internshipId.HasValue)
        {
            query = query.Where(f => f.InternshipId == internshipId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize));
        var items = await query
            .OrderByDescending(f => f.SubmittedAtUtc)
            .Skip((Math.Max(1, pageNumber) - 1) * Math.Max(1, pageSize))
            .Take(Math.Max(1, pageSize))
            .ToListAsync(cancellationToken);

        var response = new PagedResponse<FeedbackResponse>
        {
            Items = items.Select(f => new FeedbackResponse
            {
                Id = f.Id,
                InternshipId = f.InternshipId,
                InternshipName = f.Internship?.Name,
                StudentUserId = f.StudentUserId,
                StudentName = f.StudentUser?.Name,
                StudentEmail = f.StudentUser?.Email,
                Rating = f.Rating,
                Comments = f.Comments,
                SubmittedAtUtc = f.SubmittedAtUtc
            }).ToList(),
            PageNumber = Math.Max(1, pageNumber),
            PageSize = Math.Max(1, pageSize),
            TotalCount = totalCount,
            TotalPages = totalPages
        };

        return Ok(response);
    }
}
