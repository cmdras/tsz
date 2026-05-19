using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class RemoveCounters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Counters");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Counters",
                columns: table => new
                {
                    Key = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Value = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Counters", x => x.Key);
                });
        }
    }
}
