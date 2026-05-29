using Microsoft.AspNetCore.Http;

namespace InternshipPortal.DTOs;

public class StudyMaterialResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NoteUrl { get; set; }
    public string? VideoUrl { get; set; }
    public List<StudentLookupResponse> AssignedStudents { get; set; } = [];
}

public class StudyMaterialUploadRequest
{
    public string Name { get; set; } = string.Empty;
    public string? NoteUrl { get; set; }
    public string? VideoUrl { get; set; }
    public IFormFile? NoteFile { get; set; }
    public IFormFile? VideoFile { get; set; }
}

public class CreateStudyMaterialRequest
{
    public string Name { get; set; } = string.Empty;
    public string? NoteUrl { get; set; }
    public string? VideoUrl { get; set; }
}

public class UpdateStudyMaterialRequest
{
    public string Name { get; set; } = string.Empty;
    public string? NoteUrl { get; set; }
    public string? VideoUrl { get; set; }
}

public class StudentLookupResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
