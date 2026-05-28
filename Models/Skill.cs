namespace InternshipPortal.Models;

public class Skill
{
    public int Id { get; set; }

    public string StackName { get; set; } = string.Empty;

    public ICollection<PortalUser> Users { get; set; } = new List<PortalUser>();
}