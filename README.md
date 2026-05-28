# InternshipPortal

An ASP.NET Core Web API scaffold for an internship portal backend, configured for Entity Framework Core with MySQL.

## Schema

- `INTERNSHIPS`: `name`, `backlogsCount` (optional), `cgpa` (optional), `streamBranch` (optional), `stipend` (optional), `duration`, `studentsApplied` many-to-many with `USER`
- `USER`: `name`, `email`, `password`, `role` (`Admin` or `Student`), `skills` many-to-many with `SKILLS`
- `STUDENTPROFILE`: one-to-one with `USER`, `collegeName`, `cgpa` (optional), `backlogsCount` (optional), `streamBranch` (optional)
- `ADMINPROFILE`: one-to-one with `USER`, `department`
- `SKILLS`: `stackName`

## Prerequisites

- .NET 8.0.421 SDK
- A MySQL server

## Configuration

Update `appsettings.json` with a valid MySQL connection string.

## Database Setup

After the connection string is configured, create and apply migrations:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## API Endpoints

### Authentication

- `POST /api/auth/student/register`
- `POST /api/auth/admin/register`
- `POST /api/auth/login`

### Profile

- `GET /api/profile/me` with a bearer token

### Internships

- `GET /api/internships`
- `GET /api/internships/{id}`
- `GET /api/internships/eligible` for students, filtered by their `cgpa` and `backlogsCount`
- `GET /api/internships/applied` for students to see internships they applied for
- `POST /api/internships/{internshipId}/apply` for students to apply for an internship
- `GET /api/internships/{internshipId}/applications` for admins to list applications for a specific internship
- `POST /api/internships/applications/{applicationId}/approve` for admins to approve an application
- `POST /api/internships/applications/{applicationId}/reject` for admins to reject an application
- `POST /api/internships` for admins only
- `PUT /api/internships/{id}` for admins only
- `DELETE /api/internships/{id}` for admins only

### Email Delivery

Configure the `Smtp` section in `appsettings.json` with real SMTP credentials before using application email notifications.

## Run

```bash
dotnet run
```