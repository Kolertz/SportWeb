using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportWeb.Migrations
{
    /// <inheritdoc />
    public partial class FavouriteExercises : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Users_AuthorId",
                table: "Exercises");

            migrationBuilder.CreateTable(
                name: "ExerciseUser",
                columns: table => new
                {
                    FavoriteExercisesId = table.Column<int>(type: "int", nullable: false),
                    UsersWhoFavoritedId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseUser", x => new { x.FavoriteExercisesId, x.UsersWhoFavoritedId });
                    table.ForeignKey(
                        name: "FK_ExerciseUser_Exercises_FavoriteExercisesId",
                        column: x => x.FavoriteExercisesId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseUser_Users_UsersWhoFavoritedId",
                        column: x => x.UsersWhoFavoritedId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseUser_UsersWhoFavoritedId",
                table: "ExerciseUser",
                column: "UsersWhoFavoritedId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Users_AuthorId",
                table: "Exercises",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Users_AuthorId",
                table: "Exercises");

            migrationBuilder.DropTable(
                name: "ExerciseUser");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Users_AuthorId",
                table: "Exercises",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
