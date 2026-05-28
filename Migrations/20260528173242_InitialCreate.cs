using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternshipPortal.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "INTERNSHIPS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BacklogsCount = table.Column<int>(type: "int", nullable: true),
                    Cgpa = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: true),
                    StreamBranch = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Stipend = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    Duration = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_INTERNSHIPS", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SKILLS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StackName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SKILLS", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "USER",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Password = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Role = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ADMINPROFILE",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Department = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADMINPROFILE", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_ADMINPROFILE_USER_UserId",
                        column: x => x.UserId,
                        principalTable: "USER",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "INTERNSHIPAPPLICATIONS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InternshipId = table.Column<int>(type: "int", nullable: false),
                    StudentUserId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AppliedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ReviewedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ReviewedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_INTERNSHIPAPPLICATIONS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_INTERNSHIPAPPLICATIONS_INTERNSHIPS_InternshipId",
                        column: x => x.InternshipId,
                        principalTable: "INTERNSHIPS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_INTERNSHIPAPPLICATIONS_USER_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "USER",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_INTERNSHIPAPPLICATIONS_USER_StudentUserId",
                        column: x => x.StudentUserId,
                        principalTable: "USER",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "INTERNSHIPS_USERS",
                columns: table => new
                {
                    InternshipId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_INTERNSHIPS_USERS", x => new { x.InternshipId, x.UserId });
                    table.ForeignKey(
                        name: "FK_INTERNSHIPS_USERS_INTERNSHIPS_InternshipId",
                        column: x => x.InternshipId,
                        principalTable: "INTERNSHIPS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_INTERNSHIPS_USERS_USER_UserId",
                        column: x => x.UserId,
                        principalTable: "USER",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PASSWORDRESETTOKENS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UsedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PASSWORDRESETTOKENS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PASSWORDRESETTOKENS_USER_UserId",
                        column: x => x.UserId,
                        principalTable: "USER",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "STUDENTPROFILE",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CollegeName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cgpa = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: true),
                    BacklogsCount = table.Column<int>(type: "int", nullable: true),
                    StreamBranch = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STUDENTPROFILE", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_STUDENTPROFILE_USER_UserId",
                        column: x => x.UserId,
                        principalTable: "USER",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "USER_SKILLS",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_SKILLS", x => new { x.UserId, x.SkillId });
                    table.ForeignKey(
                        name: "FK_USER_SKILLS_SKILLS_SkillId",
                        column: x => x.SkillId,
                        principalTable: "SKILLS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_USER_SKILLS_USER_UserId",
                        column: x => x.UserId,
                        principalTable: "USER",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_INTERNSHIPAPPLICATIONS_InternshipId",
                table: "INTERNSHIPAPPLICATIONS",
                column: "InternshipId");

            migrationBuilder.CreateIndex(
                name: "IX_INTERNSHIPAPPLICATIONS_ReviewedByUserId",
                table: "INTERNSHIPAPPLICATIONS",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_INTERNSHIPAPPLICATIONS_StudentUserId_InternshipId",
                table: "INTERNSHIPAPPLICATIONS",
                columns: new[] { "StudentUserId", "InternshipId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_INTERNSHIPS_USERS_UserId",
                table: "INTERNSHIPS_USERS",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PASSWORDRESETTOKENS_Token",
                table: "PASSWORDRESETTOKENS",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PASSWORDRESETTOKENS_UserId",
                table: "PASSWORDRESETTOKENS",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_USER_Email",
                table: "USER",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USER_SKILLS_SkillId",
                table: "USER_SKILLS",
                column: "SkillId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ADMINPROFILE");

            migrationBuilder.DropTable(
                name: "INTERNSHIPAPPLICATIONS");

            migrationBuilder.DropTable(
                name: "INTERNSHIPS_USERS");

            migrationBuilder.DropTable(
                name: "PASSWORDRESETTOKENS");

            migrationBuilder.DropTable(
                name: "STUDENTPROFILE");

            migrationBuilder.DropTable(
                name: "USER_SKILLS");

            migrationBuilder.DropTable(
                name: "INTERNSHIPS");

            migrationBuilder.DropTable(
                name: "SKILLS");

            migrationBuilder.DropTable(
                name: "USER");
        }
    }
}
