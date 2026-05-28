namespace InternshipPortal.DTOs;

public sealed class AppliedInternshipResponse
{
    public int ApplicationId { get; set; }

    public int InternshipId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int? BacklogsCount { get; set; }

    public decimal? Cgpa { get; set; }

    public string? StreamBranch { get; set; }

    public decimal? Stipend { get; set; }

    public string Duration { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime AppliedAtUtc { get; set; }
}

public sealed class AppliedStudentResponse
{
    public int ApplicationId { get; set; }

    public int StudentId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? CollegeName { get; set; }

    public decimal? Cgpa { get; set; }

    public int? BacklogsCount { get; set; }

    public string? StreamBranch { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime AppliedAtUtc { get; set; }
}