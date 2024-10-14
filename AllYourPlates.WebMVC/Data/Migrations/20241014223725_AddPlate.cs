using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AllYourPlates.WebMVC.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPlate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Plate",
                columns: table => new
                {
                    PlateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plate", x => x.PlateId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Plate");
        }
    }
}
