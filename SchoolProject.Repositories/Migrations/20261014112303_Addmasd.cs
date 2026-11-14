using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolProject.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class Addmasd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequiredCredits",
                table: "Majors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxStudents",
                table: "ClassSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinStudents",
                table: "ClassSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiredCredits",
                table: "Majors");

            migrationBuilder.DropColumn(
                name: "MaxStudents",
                table: "ClassSessions");

            migrationBuilder.DropColumn(
                name: "MinStudents",
                table: "ClassSessions");
        }
    }
}
