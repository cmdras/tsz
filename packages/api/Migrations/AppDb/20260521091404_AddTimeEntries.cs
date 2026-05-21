using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class AddTimeEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TimeEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ContractTaskId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LeaveTypeId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Hours = table.Column<decimal>(type: "decimal(4,1)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeEntries", x => x.Id);
                    table.CheckConstraint("CK_TimeEntries_ExactlyOneFK", "(ContractTaskId IS NULL) != (LeaveTypeId IS NULL)");
                    table.ForeignKey(
                        name: "FK_TimeEntries_LeaveTypes_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimeEntries_Tasks_ContractTaskId",
                        column: x => x.ContractTaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimeEntries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WeekSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    WeekStart = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeekSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeekSubmissions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_ContractTaskId",
                table: "TimeEntries",
                column: "ContractTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_LeaveTypeId",
                table: "TimeEntries",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_UserId_Date_ContractTaskId",
                table: "TimeEntries",
                columns: new[] { "UserId", "Date", "ContractTaskId" },
                unique: true,
                filter: "ContractTaskId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_UserId_Date_LeaveTypeId",
                table: "TimeEntries",
                columns: new[] { "UserId", "Date", "LeaveTypeId" },
                unique: true,
                filter: "LeaveTypeId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WeekSubmissions_UserId_WeekStart",
                table: "WeekSubmissions",
                columns: new[] { "UserId", "WeekStart" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimeEntries");

            migrationBuilder.DropTable(
                name: "WeekSubmissions");
        }
    }
}
