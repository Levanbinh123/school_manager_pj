using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolProject.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddAcademicYearCloseConfirmation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CloseConfirmation",
                table: "AcademicYears",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CloseSignature",
                table: "AcademicYears",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAt",
                table: "AcademicYears",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClosedByUserId",
                table: "AcademicYears",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CloseConfirmation",
                table: "AcademicYears");

            migrationBuilder.DropColumn(
                name: "CloseSignature",
                table: "AcademicYears");

            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "AcademicYears");

            migrationBuilder.DropColumn(
                name: "ClosedByUserId",
                table: "AcademicYears");
        }
    }
}
