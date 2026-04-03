using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Amdocs.Atlas.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddServerVcenterIpIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "Servers",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_VcenterId_IpAddress",
                table: "Servers",
                columns: new[] { "VcenterId", "IpAddress" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Servers_VcenterId_IpAddress",
                table: "Servers");

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "Servers",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
