namespace InternshipPortal.Models;

public class Feedback
{
    public int Id { get; set; }

    public int InternshipId { get; set; }

    public int StudentUserId { get; set; }

    public int Rating { get; set; }

    public string? Comments { get; set; }

    public DateTime SubmittedAtUtc { get; set; }

    public Internship? Internship { get; set; }

    public PortalUser? StudentUser { get; set; }
}
