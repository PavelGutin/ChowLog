using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AllYourPlates.WebMVC.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserToPlate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Plate",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Plate_UserId",
                table: "Plate",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Plate_AspNetUsers_UserId",
                table: "Plate",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Plate_AspNetUsers_UserId",
                table: "Plate");

            migrationBuilder.DropIndex(
                name: "IX_Plate_UserId",
                table: "Plate");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Plate");
        }
    }
}
