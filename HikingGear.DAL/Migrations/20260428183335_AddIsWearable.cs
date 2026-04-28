using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HikingGear.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddIsWearable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsWearable",
                table: "GearItems",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsWearable",
                table: "GearItems");
        }
    }
}
