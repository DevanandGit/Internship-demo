namespace InternshipPortal.Models;

public class InternshipApplication
{
    public int Id { get; set; }

    public int InternshipId { get; set; }

    public Internship Internship { get; set; } = null!;

    public int StudentUserId { get; set; }

    public PortalUser StudentUser { get; set; } = null!;

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    public DateTime AppliedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ReviewedAtUtc { get; set; }

    public int? ReviewedByUserId { get; set; }

    public PortalUser? ReviewedByUser { get; set; }
}