namespace InternshipPortal.Models;

public class AdminProfile
{
    public int UserId { get; set; }

    public PortalUser User { get; set; } = null!;

    public string Department { get; set; } = string.Empty;
}