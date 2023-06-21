using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeoProfs_App.Migrations
{
    /// <inheritdoc />
    public partial class addDiffrentReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DifffrentReason",
                table: "Verlofaanvragen",
                type: "longtext",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DifffrentReason",
                table: "Verlofaanvragen");
        }
    }
}
