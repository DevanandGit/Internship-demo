using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternshipPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddInternshipApplicationWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "INTERNSHIPAPPLICATIONS");
        }
    }
}
