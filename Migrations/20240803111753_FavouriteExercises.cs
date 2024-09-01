using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportWeb.Migrations
{
    /// <inheritdoc />
    public partial class favoriteExercises : Migration
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
                    favoriteExercisesId = table.Column<int>(type: "int", nullable: false),
                    UsersWhofavoritedId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseUser", x => new { x.favoriteExercisesId, x.UsersWhofavoritedId });
                    table.ForeignKey(
                        name: "FK_ExerciseUser_Exercises_favoriteExercisesId",
                        column: x => x.favoriteExercisesId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseUser_Users_UsersWhofavoritedId",
                        column: x => x.UsersWhofavoritedId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseUser_UsersWhofavoritedId",
                table: "ExerciseUser",
                column: "UsersWhofavoritedId");

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
