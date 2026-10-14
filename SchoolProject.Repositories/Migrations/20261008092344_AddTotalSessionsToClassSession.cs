using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolProject.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalSessionsToClassSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalSessions",
                table: "ClassSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalSessions",
                table: "ClassSessions");
        }
    }
}
