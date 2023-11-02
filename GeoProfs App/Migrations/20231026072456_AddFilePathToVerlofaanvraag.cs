using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeoProfs_App.Migrations
{
    /// <inheritdoc />
    public partial class AddFilePathToVerlofaanvraag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Verlofaanvragen",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Verlofaanvragen",
                type: "longtext",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_Verlofaanvragen_UserId",
                table: "Verlofaanvragen",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Verlofaanvragen_AspNetUsers_UserId",
                table: "Verlofaanvragen",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Verlofaanvragen_AspNetUsers_UserId",
                table: "Verlofaanvragen");

            migrationBuilder.DropIndex(
                name: "IX_Verlofaanvragen_UserId",
                table: "Verlofaanvragen");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Verlofaanvragen");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Verlofaanvragen",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");
        }
    }
}
