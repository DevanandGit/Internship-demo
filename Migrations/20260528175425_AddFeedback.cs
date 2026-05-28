using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternshipPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FEEDBACKS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InternshipId = table.Column<int>(type: "int", nullable: false),
                    StudentUserId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comments = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SubmittedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FEEDBACKS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FEEDBACKS_INTERNSHIPS_InternshipId",
                        column: x => x.InternshipId,
                        principalTable: "INTERNSHIPS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FEEDBACKS_USER_StudentUserId",
                        column: x => x.StudentUserId,
                        principalTable: "USER",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FEEDBACKTIMERS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InternshipId = table.Column<int>(type: "int", nullable: false),
                    StartUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FEEDBACKTIMERS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FEEDBACKTIMERS_INTERNSHIPS_InternshipId",
                        column: x => x.InternshipId,
                        principalTable: "INTERNSHIPS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_FEEDBACKS_InternshipId_StudentUserId",
                table: "FEEDBACKS",
                columns: new[] { "InternshipId", "StudentUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FEEDBACKS_StudentUserId",
                table: "FEEDBACKS",
                column: "StudentUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FEEDBACKTIMERS_InternshipId",
                table: "FEEDBACKTIMERS",
                column: "InternshipId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FEEDBACKS");

            migrationBuilder.DropTable(
                name: "FEEDBACKTIMERS");
        }
    }
}
