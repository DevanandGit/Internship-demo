namespace InternshipPortal.DTOs;

public class CreateFeedbackTimerRequest
{
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
}

public class FeedbackTimerResponse
{
    public int Id { get; set; }
    public int InternshipId { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
}

public class CreateFeedbackRequest
{
    public int Rating { get; set; }
    public string? Comments { get; set; }
}

public class FeedbackResponse
{
    public int Id { get; set; }
    public int InternshipId { get; set; }
    public int StudentUserId { get; set; }
    public int Rating { get; set; }
    public string? Comments { get; set; }
    public DateTime SubmittedAtUtc { get; set; }
}
