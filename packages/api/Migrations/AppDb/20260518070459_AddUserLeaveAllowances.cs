using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class AddUserLeaveAllowances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultMode",
                table: "LeaveTypes",
                type: "TEXT",
                nullable: false,
                defaultValue: "Limited");

            migrationBuilder.Sql("UPDATE \"LeaveTypes\" SET \"DefaultMode\" = 'Unlimited' WHERE \"Name\" = 'Sickness';");

            migrationBuilder.CreateTable(
                name: "UserLeaveAllowances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LeaveTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Mode = table.Column<string>(type: "TEXT", nullable: false),
                    TotalDays = table.Column<decimal>(type: "decimal(5,1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLeaveAllowances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLeaveAllowances_LeaveTypes_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserLeaveAllowances_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserLeaveAllowances_LeaveTypeId",
                table: "UserLeaveAllowances",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLeaveAllowances_UserId_LeaveTypeId_Year",
                table: "UserLeaveAllowances",
                columns: new[] { "UserId", "LeaveTypeId", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserLeaveAllowances");

            migrationBuilder.DropColumn(
                name: "DefaultMode",
                table: "LeaveTypes");
        }
    }
}
