using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolProject.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAcademicYearClosed11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StudyDurationYears",
                table: "Majors",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StudyDurationYears",
                table: "Majors");
        }
    }
}
