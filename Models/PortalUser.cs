namespace InternshipPortal.Models;

public class PortalUser
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public ICollection<Skill> Skills { get; set; } = new List<Skill>();

    public StudentProfile? StudentProfile { get; set; }

    public AdminProfile? AdminProfile { get; set; }

    public ICollection<Internship> InternshipsApplied { get; set; } = new List<Internship>();

    public ICollection<InternshipApplication> InternshipApplications { get; set; } = new List<InternshipApplication>();

    public ICollection<InternshipApplication> ReviewedInternshipApplications { get; set; } = new List<InternshipApplication>();
}