namespace InternshipPortal.DTOs;

public class StudyMaterialResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NoteUrl { get; set; }
    public string? VideoUrl { get; set; }
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
