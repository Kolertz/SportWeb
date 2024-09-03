using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportWeb.Migrations
{
    /// <inheritdoc />
    public partial class Test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_favoriteExercises_UsersWhofavoritedId",
                table: "FavouriteExercises",
                newName: "IX_FavouriteExercises_UsersWhoFavouritedId");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FavoriteExercises",
                table: "FavouriteExercises");

            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteExercises_Exercises_FavoriteExercisesId",
                table: "FavouriteExercises");

            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteExercises_Users_UsersWhoFavoritedId",
                table: "FavouriteExercises");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FavouriteExercises",
                table: "FavouriteExercises",
                columns: new[] { "FavouriteExercisesId", "UsersWhoFavouritedId" });

            migrationBuilder.AddForeignKey(
                name: "FK_FavouriteExercises_Exercises_FavouriteExercisesId",
                table: "FavouriteExercises",
                column: "FavouriteExercisesId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FavouriteExercises_Users_UsersWhoFavouritedId",
                table: "FavouriteExercises",
                column: "UsersWhoFavouritedId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FavouriteExercises_Exercises_FavouriteExercisesId",
                table: "FavouriteExercises");

            migrationBuilder.DropForeignKey(
                name: "FK_FavouriteExercises_Users_UsersWhoFavouritedId",
                table: "FavouriteExercises");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FavouriteExercises",
                table: "FavouriteExercises");

            migrationBuilder.RenameTable(
                name: "FavouriteExercises",
                newName: "favoriteExercises");

            migrationBuilder.RenameColumn(
                name: "UsersWhoFavouritedId",
                table: "favoriteExercises",
                newName: "UsersWhofavoritedId");

            migrationBuilder.RenameColumn(
                name: "FavouriteExercisesId",
                table: "favoriteExercises",
                newName: "favoriteExercisesId");

            migrationBuilder.RenameIndex(
                name: "IX_FavouriteExercises_UsersWhoFavouritedId",
                table: "favoriteExercises",
                newName: "IX_favoriteExercises_UsersWhofavoritedId");
        }
    }
}