using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternshipPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddStudyMaterialAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "STUDENT_STUDYMATERIALS",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    StudyMaterialId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STUDENT_STUDYMATERIALS", x => new { x.UserId, x.StudyMaterialId });
                    table.ForeignKey(
                        name: "FK_STUDENT_STUDYMATERIALS_STUDYMATERIALS_StudyMaterialId",
                        column: x => x.StudyMaterialId,
                        principalTable: "STUDYMATERIALS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_STUDENT_STUDYMATERIALS_USER_UserId",
                        column: x => x.UserId,
                        principalTable: "USER",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_STUDENT_STUDYMATERIALS_StudyMaterialId",
                table: "STUDENT_STUDYMATERIALS",
                column: "StudyMaterialId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "STUDENT_STUDYMATERIALS");
        }
    }
}
