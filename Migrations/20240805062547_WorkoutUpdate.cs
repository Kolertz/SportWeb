using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportWeb.Migrations
{
    /// <inheritdoc />
    public partial class WorkoutUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Workouts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_IsPublic",
                table: "Workouts",
                column: "IsPublic");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Workouts_IsPublic",
                table: "Workouts");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Workouts");
        }
    }
}
