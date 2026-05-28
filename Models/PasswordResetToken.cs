namespace InternshipPortal.Models;

public class PasswordResetToken
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public PortalUser User { get; set; } = null!;

    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? UsedAtUtc { get; set; }
}