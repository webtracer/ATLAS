using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Amdocs.Atlas.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVSphereConnectionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Passphrase",
                table: "Vcenters",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Passphrase",
                table: "Vcenters");
        }
    }
}
