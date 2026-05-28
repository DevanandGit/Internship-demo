namespace InternshipPortal.Models;

public class StudentProfile
{
    public int UserId { get; set; }

    public PortalUser User { get; set; } = null!;

    public string CollegeName { get; set; } = string.Empty;

    public decimal? Cgpa { get; set; }

    public int? BacklogsCount { get; set; }

    public string? StreamBranch { get; set; }
}