namespace InternshipPortal.Models;

public class FeedbackTimer
{
    public int Id { get; set; }

    public int InternshipId { get; set; }

    public DateTime StartUtc { get; set; }

    public DateTime EndUtc { get; set; }

    public Internship? Internship { get; set; }
}
