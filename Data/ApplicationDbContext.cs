using InternshipPortal.Models;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Internship> Internships => Set<Internship>();

    public DbSet<PortalUser> Users => Set<PortalUser>();

    public DbSet<Skill> Skills => Set<Skill>();

    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();

    public DbSet<AdminProfile> AdminProfiles => Set<AdminProfile>();

    public DbSet<InternshipApplication> InternshipApplications => Set<InternshipApplication>();

    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Internship>(entity =>
        {
            entity.ToTable("INTERNSHIPS");

            entity.Property(item => item.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(item => item.BacklogsCount)
                .IsRequired(false);

            entity.Property(item => item.Cgpa)
                .HasPrecision(3, 2)
                .IsRequired(false);

            entity.Property(item => item.StreamBranch)
                .HasMaxLength(200)
                .IsRequired(false);

            entity.Property(item => item.Stipend)
                .HasPrecision(12, 2)
                .IsRequired(false);

            entity.Property(item => item.Duration)
                .HasMaxLength(100)
                .IsRequired();
        });

        modelBuilder.Entity<PortalUser>(entity =>
        {
            entity.ToTable("USER");

            entity.Property(item => item.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(item => item.Email)
                .HasMaxLength(256)
                .IsRequired();

            entity.HasIndex(item => item.Email)
                .IsUnique();

            entity.Property(item => item.Password)
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(item => item.Role)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.ToTable("SKILLS");

            entity.Property(item => item.StackName)
                .HasMaxLength(150)
                .IsRequired();
        });

        modelBuilder.Entity<Internship>()
            .HasMany(item => item.StudentsApplied)
            .WithMany(item => item.InternshipsApplied)
            .UsingEntity<Dictionary<string, object>>(
                "InternshipStudents",
                right => right.HasOne<PortalUser>()
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left.HasOne<Internship>()
                    .WithMany()
                    .HasForeignKey("InternshipId")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("INTERNSHIPS_USERS");
                    join.HasKey("InternshipId", "UserId");
                });

        modelBuilder.Entity<PortalUser>()
            .HasMany(item => item.Skills)
            .WithMany(item => item.Users)
            .UsingEntity<Dictionary<string, object>>(
                "UserSkills",
                right => right.HasOne<Skill>()
                    .WithMany()
                    .HasForeignKey("SkillId")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left.HasOne<PortalUser>()
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("USER_SKILLS");
                    join.HasKey("UserId", "SkillId");
                });

        modelBuilder.Entity<StudentProfile>(entity =>
        {
            entity.ToTable("STUDENTPROFILE");

            entity.HasKey(item => item.UserId);

            entity.Property(item => item.CollegeName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(item => item.Cgpa)
                .HasPrecision(3, 2)
                .IsRequired(false);

            entity.Property(item => item.BacklogsCount)
                .IsRequired(false);

            entity.Property(item => item.StreamBranch)
                .HasMaxLength(200)
                .IsRequired(false);

            entity.HasOne(item => item.User)
                .WithOne(item => item.StudentProfile)
                .HasForeignKey<StudentProfile>(item => item.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AdminProfile>(entity =>
        {
            entity.ToTable("ADMINPROFILE");

            entity.HasKey(item => item.UserId);

            entity.Property(item => item.Department)
                .HasMaxLength(200)
                .IsRequired();

            entity.HasOne(item => item.User)
                .WithOne(item => item.AdminProfile)
                .HasForeignKey<AdminProfile>(item => item.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InternshipApplication>(entity =>
        {
            entity.ToTable("INTERNSHIPAPPLICATIONS");

            entity.HasKey(item => item.Id);

            entity.Property(item => item.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(item => item.AppliedAtUtc)
                .IsRequired();

            entity.Property(item => item.ReviewedAtUtc)
                .IsRequired(false);

            entity.HasIndex(item => new { item.StudentUserId, item.InternshipId })
                .IsUnique();

            entity.HasOne(item => item.Internship)
                .WithMany(item => item.InternshipApplications)
                .HasForeignKey(item => item.InternshipId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item => item.StudentUser)
                .WithMany(item => item.InternshipApplications)
                .HasForeignKey(item => item.StudentUserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item => item.ReviewedByUser)
                .WithMany(item => item.ReviewedInternshipApplications)
                .HasForeignKey(item => item.ReviewedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.ToTable("PASSWORDRESETTOKENS");

            entity.HasKey(item => item.Id);

            entity.Property(item => item.Token)
                .HasMaxLength(64)
                .IsRequired();

            entity.HasIndex(item => item.Token)
                .IsUnique();

            entity.Property(item => item.ExpiresAtUtc)
                .IsRequired();

            entity.Property(item => item.UsedAtUtc)
                .IsRequired(false);

            entity.HasOne(item => item.User)
                .WithMany()
                .HasForeignKey(item => item.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}