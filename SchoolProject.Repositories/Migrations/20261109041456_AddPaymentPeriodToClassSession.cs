using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolProject.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentPeriodToClassSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefundTransactionNo",
                table: "VnPayPayments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "RefundedAt",
                table: "VnPayPayments",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FundsStatus",
                table: "Fees",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "LateFee",
                table: "Fees",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentStartDate",
                table: "Fees",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RefundedAt",
                table: "Fees",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "Fees",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "TransferredAt",
                table: "Fees",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentEndDate",
                table: "ClassSessions",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentStartDate",
                table: "ClassSessions",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefundTransactionNo",
                table: "VnPayPayments");

            migrationBuilder.DropColumn(
                name: "RefundedAt",
                table: "VnPayPayments");

            migrationBuilder.DropColumn(
                name: "FundsStatus",
                table: "Fees");

            migrationBuilder.DropColumn(
                name: "LateFee",
                table: "Fees");

            migrationBuilder.DropColumn(
                name: "PaymentStartDate",
                table: "Fees");

            migrationBuilder.DropColumn(
                name: "RefundedAt",
                table: "Fees");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "Fees");

            migrationBuilder.DropColumn(
                name: "TransferredAt",
                table: "Fees");

            migrationBuilder.DropColumn(
                name: "PaymentEndDate",
                table: "ClassSessions");

            migrationBuilder.DropColumn(
                name: "PaymentStartDate",
                table: "ClassSessions");
        }
    }
}
