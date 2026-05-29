using InternshipPortal.Data;
using InternshipPortal.DTOs;
using InternshipPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.Controllers;

[ApiController]
[Route("api/students")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class StudentsController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StudentLookupResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var students = await context.Users
            .AsNoTracking()
            .Where(user => user.Role == UserRole.Student)
            .OrderBy(user => user.Id)
            .Select(user => new StudentLookupResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            })
            .ToListAsync(cancellationToken);

        return Ok(students);
    }
}