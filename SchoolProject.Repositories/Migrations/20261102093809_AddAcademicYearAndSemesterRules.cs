using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolProject.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddAcademicYearAndSemesterRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.AddColumn<bool>(
                name: "IsPromoted",
                table: "YearlySessions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PromotedAt",
                table: "YearlySessions",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SemesterNumber",
                table: "YearlySessions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
       

            migrationBuilder.DropColumn(
                name: "IsPromoted",
                table: "YearlySessions");

            migrationBuilder.DropColumn(
                name: "PromotedAt",
                table: "YearlySessions");

            migrationBuilder.DropColumn(
                name: "SemesterNumber",
                table: "YearlySessions");
        }
    }
}
