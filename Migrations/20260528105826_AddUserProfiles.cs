using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternshipPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cgpa",
                table: "USER");

            migrationBuilder.DropColumn(
                name: "CollegeName",
                table: "USER");

            migrationBuilder.DropColumn(
                name: "StreamBranch",
                table: "USER");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "USER",
                type: "varchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "USER",
                type: "varchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "")
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
                name: "STUDENTPROFILE",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CollegeName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cgpa = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: true),
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

            migrationBuilder.CreateIndex(
                name: "IX_USER_Email",
                table: "USER",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ADMINPROFILE");

            migrationBuilder.DropTable(
                name: "STUDENTPROFILE");

            migrationBuilder.DropIndex(
                name: "IX_USER_Email",
                table: "USER");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "USER");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "USER");

            migrationBuilder.AddColumn<decimal>(
                name: "Cgpa",
                table: "USER",
                type: "decimal(3,2)",
                precision: 3,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CollegeName",
                table: "USER",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "StreamBranch",
                table: "USER",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
