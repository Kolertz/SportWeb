using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportWeb.Migrations
{
    /// <inheritdoc />
    public partial class FixUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Exercises_ExerciseId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ExerciseId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ExerciseId",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExerciseId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ExerciseId",
                table: "Users",
                column: "ExerciseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Exercises_ExerciseId",
                table: "Users",
                column: "ExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id");
        }
    }
}
