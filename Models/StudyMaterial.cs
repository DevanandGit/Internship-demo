namespace InternshipPortal.Models;

public class StudyMaterial
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? NoteUrl { get; set; }

    public string? VideoUrl { get; set; }

    public ICollection<PortalUser> AssignedTo { get; set; } = new List<PortalUser>();
}
