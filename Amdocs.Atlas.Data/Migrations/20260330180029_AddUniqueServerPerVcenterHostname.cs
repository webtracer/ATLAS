using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Amdocs.Atlas.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueServerPerVcenterHostname : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Servers_VcenterId_Hostname",
                table: "Servers",
                columns: new[] { "VcenterId", "Hostname" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Servers_VcenterId_Hostname",
                table: "Servers");
        }
    }
}
