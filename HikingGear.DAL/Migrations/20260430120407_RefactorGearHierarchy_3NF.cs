using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HikingGear.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RefactorGearHierarchy_3NF : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GearItems_GearCategories_CategoryId",
                table: "GearItems");

            migrationBuilder.DropForeignKey(
                name: "FK_GearItems_Trips_TripId",
                table: "GearItems");

            migrationBuilder.DropIndex(
                name: "IX_GearItems_TripId",
                table: "GearItems");

            migrationBuilder.DropColumn(
                name: "TripId",
                table: "GearItems");

            migrationBuilder.AddColumn<int>(
                name: "TripId",
                table: "GearCategories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_GearCategories_TripId",
                table: "GearCategories",
                column: "TripId");

            migrationBuilder.AddForeignKey(
                name: "FK_GearCategories_Trips_TripId",
                table: "GearCategories",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GearItems_GearCategories_CategoryId",
                table: "GearItems",
                column: "CategoryId",
                principalTable: "GearCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GearCategories_Trips_TripId",
                table: "GearCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_GearItems_GearCategories_CategoryId",
                table: "GearItems");

            migrationBuilder.DropIndex(
                name: "IX_GearCategories_TripId",
                table: "GearCategories");

            migrationBuilder.DropColumn(
                name: "TripId",
                table: "GearCategories");

            migrationBuilder.AddColumn<int>(
                name: "TripId",
                table: "GearItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_GearItems_TripId",
                table: "GearItems",
                column: "TripId");

            migrationBuilder.AddForeignKey(
                name: "FK_GearItems_GearCategories_CategoryId",
                table: "GearItems",
                column: "CategoryId",
                principalTable: "GearCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GearItems_Trips_TripId",
                table: "GearItems",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
