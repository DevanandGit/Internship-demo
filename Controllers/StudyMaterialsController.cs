using InternshipPortal.Data;
using InternshipPortal.DTOs;
using InternshipPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.Controllers;

[ApiController]
[Route("api/studymaterials")]
[Route("api/study-materials")]
public class StudyMaterialsController(ApplicationDbContext context, IWebHostEnvironment environment) : ControllerBase
{
    private const string StudyMaterialRootFolder = "uploads/study-materials";

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
            .Include(material => material.AssignedTo)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        return item is null ? NotFound() : Ok(new StudyMaterialResponse
        {
            Id = item.Id,
            Name = item.Name,
            NoteUrl = item.NoteUrl,
            VideoUrl = item.VideoUrl,
            AssignedStudents = item.AssignedTo
                .OrderBy(student => student.Id)
                .Select(student => new StudentLookupResponse
                {
                    Id = student.Id,
                    Name = student.Name,
                    Email = student.Email
                })
                .ToList()
        });
    }

    [HttpPost]
    [RequestFormLimits(MultipartBodyLengthLimit = 209_715_200)]
    [RequestSizeLimit(209_715_200)]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<StudyMaterialResponse>> Create([FromForm] StudyMaterialUploadRequest request, CancellationToken cancellationToken)
    {
        var noteUrl = await SaveUploadAsync(request.NoteFile, "notes", cancellationToken) ?? NormalizeUrl(request.NoteUrl);
        var videoUrl = await SaveUploadAsync(request.VideoFile, "videos", cancellationToken) ?? NormalizeUrl(request.VideoUrl);

        var entity = new StudyMaterial
        {
            Name = request.Name.Trim(),
            NoteUrl = noteUrl,
            VideoUrl = videoUrl
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
    [RequestFormLimits(MultipartBodyLengthLimit = 209_715_200)]
    [RequestSizeLimit(209_715_200)]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Update(int id, [FromForm] StudyMaterialUploadRequest request, CancellationToken cancellationToken)
    {
        var entity = await context.StudyMaterials.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        if (entity is null) return NotFound();

        var previousNoteUrl = entity.NoteUrl;
        var previousVideoUrl = entity.VideoUrl;

        var noteUrl = await SaveUploadAsync(request.NoteFile, "notes", cancellationToken) ?? NormalizeUrl(request.NoteUrl) ?? entity.NoteUrl;
        var videoUrl = await SaveUploadAsync(request.VideoFile, "videos", cancellationToken) ?? NormalizeUrl(request.VideoUrl) ?? entity.VideoUrl;

        entity.Name = request.Name.Trim();
        entity.NoteUrl = noteUrl;
        entity.VideoUrl = videoUrl;

        await context.SaveChangesAsync(cancellationToken);

        if (!string.Equals(previousNoteUrl, entity.NoteUrl, StringComparison.OrdinalIgnoreCase))
        {
            DeleteStoredFile(previousNoteUrl);
        }

        if (!string.Equals(previousVideoUrl, entity.VideoUrl, StringComparison.OrdinalIgnoreCase))
        {
            DeleteStoredFile(previousVideoUrl);
        }

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
        DeleteStoredFile(entity.NoteUrl);
        DeleteStoredFile(entity.VideoUrl);
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

    private async Task<string?> SaveUploadAsync(IFormFile? file, string subfolder, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return null;
        }

        var webRootPath = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        var targetFolder = Path.Combine(webRootPath, StudyMaterialRootFolder, subfolder);
        Directory.CreateDirectory(targetFolder);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(targetFolder, fileName);

        await using var stream = System.IO.File.Create(physicalPath);
        await file.CopyToAsync(stream, cancellationToken);

        return $"/{StudyMaterialRootFolder}/{subfolder}/{fileName}".Replace('\\', '/');
    }

    private static string? NormalizeUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        return url.Trim();
    }

    private void DeleteStoredFile(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        var relativePath = url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var webRootPath = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        var physicalPath = Path.Combine(webRootPath, relativePath);

        if (System.IO.File.Exists(physicalPath))
        {
            System.IO.File.Delete(physicalPath);
        }
    }
}
