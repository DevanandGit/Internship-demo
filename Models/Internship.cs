namespace InternshipPortal.Models;

public class Internship
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int? BacklogsCount { get; set; }

    public decimal? Cgpa { get; set; }

    public string? StreamBranch { get; set; }

    public decimal? Stipend { get; set; }

    public string Duration { get; set; } = string.Empty;

    public ICollection<PortalUser> StudentsApplied { get; set; } = new List<PortalUser>();

    public ICollection<InternshipApplication> InternshipApplications { get; set; } = new List<InternshipApplication>();
}