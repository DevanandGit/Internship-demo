# InternshipPortal

- ASP.NET Core Web API project targeting .NET 9.
- Entity Framework Core uses Pomelo.EntityFrameworkCore.MySql.
- The DbContext lives in `Data/ApplicationDbContext.cs`.
- The schema includes `INTERNSHIPS`, `USER`, `STUDENTPROFILE`, `ADMINPROFILE`, and `SKILLS` with many-to-many relationships for internship applicants and user skills.
- Student eligibility for internships is based on `StudentProfile.Cgpa` and `StudentProfile.BacklogsCount` compared against `Internship.Cgpa` and `Internship.BacklogsCount`.
- Students can apply to internships through `POST /api/internships/{internshipId}/apply`, view applied internships through `GET /api/internships/applied`, and admins can inspect applications by internship with `GET /api/internships/{internshipId}/applications`.
- Application status updates are handled with `POST /api/internships/applications/{applicationId}/approve` and `POST /api/internships/applications/{applicationId}/reject`.
- Email notifications are sent through SMTP settings in `appsettings.json` under `Smtp`.
- The MySQL connection string is configured in `appsettings.json` under `ConnectionStrings:DefaultConnection`.
- Authentication uses JWT bearer tokens from `Jwt` settings in `appsettings.json`.
- Use `dotnet build` to validate changes.
- After configuring a real MySQL database, create migrations with `dotnet ef migrations add <Name>` and apply them with `dotnet ef database update`.
