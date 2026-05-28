namespace InternshipPortal.DTOs;

public sealed class UserProfileResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public StudentProfileResponse? StudentProfile { get; set; }

    public AdminProfileResponse? AdminProfile { get; set; }

    public List<AppliedInternshipResponse> AppliedInternships { get; set; } = [];
}

public sealed class StudentProfileResponse
{
    public string CollegeName { get; set; } = string.Empty;

    public decimal? Cgpa { get; set; }

    public int? BacklogsCount { get; set; }

    public string? StreamBranch { get; set; }

    public List<string> Skills { get; set; } = [];
    public List<StudyMaterialResponse> AssignedStudyMaterials { get; set; } = [];
}

public sealed class AdminProfileResponse
{
    public string Department { get; set; } = string.Empty;
}

public sealed class InternshipRequest
{
    public string Name { get; set; } = string.Empty;

    public int? BacklogsCount { get; set; }

    public decimal? Cgpa { get; set; }

    public string? StreamBranch { get; set; }

    public decimal? Stipend { get; set; }

    public string Duration { get; set; } = string.Empty;

    public List<int> StudentIds { get; set; } = [];
}