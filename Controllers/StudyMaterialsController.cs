using InternshipPortal.Data;
using InternshipPortal.DTOs;
using InternshipPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.Controllers;

[ApiController]
[Route("api/studymaterials")]
public class StudyMaterialsController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<StudyMaterialResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await context.StudyMaterials
            .AsNoTracking()
            .OrderBy(item => item.Id)
            .ToListAsync(cancellationToken);

        var response = items.Select(i => new StudyMaterialResponse
        {
            Id = i.Id,
            Name = i.Name,
            NoteUrl = i.NoteUrl,
            VideoUrl = i.VideoUrl
        });

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<StudyMaterialResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await context.StudyMaterials
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        return item is null ? NotFound() : Ok(new StudyMaterialResponse
        {
            Id = item.Id,
            Name = item.Name,
            NoteUrl = item.NoteUrl,
            VideoUrl = item.VideoUrl
        });
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<StudyMaterialResponse>> Create(CreateStudyMaterialRequest request, CancellationToken cancellationToken)
    {
        var entity = new StudyMaterial
        {
            Name = request.Name,
            NoteUrl = request.NoteUrl,
            VideoUrl = request.VideoUrl
        };

        context.StudyMaterials.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        var response = new StudyMaterialResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            NoteUrl = entity.NoteUrl,
            VideoUrl = entity.VideoUrl
        };

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Update(int id, UpdateStudyMaterialRequest request, CancellationToken cancellationToken)
    {
        var entity = await context.StudyMaterials.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        if (entity is null) return NotFound();

        entity.Name = request.Name;
        entity.NoteUrl = request.NoteUrl;
        entity.VideoUrl = request.VideoUrl;

        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var entity = await context.StudyMaterials.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        if (entity is null) return NotFound();

        context.StudyMaterials.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("{materialId:int}/assign/{internId:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> AssignToStudent(int materialId, int internId, CancellationToken cancellationToken)
    {
        var material = await context.StudyMaterials.FirstOrDefaultAsync(m => m.Id == materialId, cancellationToken);
        if (material is null) return NotFound(new { message = "Study material not found." });

        var student = await context.Users
            .Include(u => u.StudyMaterials)
            .FirstOrDefaultAsync(u => u.Id == internId && u.Role == UserRole.Student, cancellationToken);

        if (student is null) return NotFound(new { message = "Student not found." });

        if (student.StudyMaterials.Any(s => s.Id == materialId))
        {
            return Conflict(new { message = "Material already assigned to student." });
        }

        student.StudyMaterials.Add(material);
        await context.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Assigned." });
    }

    [HttpDelete("{materialId:int}/assign/{internId:int}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> RemoveAssignment(int materialId, int internId, CancellationToken cancellationToken)
    {
        var student = await context.Users
            .Include(u => u.StudyMaterials)
            .FirstOrDefaultAsync(u => u.Id == internId && u.Role == UserRole.Student, cancellationToken);

        if (student is null) return NotFound(new { message = "Student not found." });

        var material = student.StudyMaterials.FirstOrDefault(s => s.Id == materialId);
        if (material is null) return NotFound(new { message = "Assignment not found." });

        student.StudyMaterials.Remove(material);
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
