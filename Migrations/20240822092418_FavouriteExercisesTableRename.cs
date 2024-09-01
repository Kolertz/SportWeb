using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportWeb.Migrations
{
    /// <inheritdoc />
    public partial class favoriteExercisesTableRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Users_AuthorId",
                table: "Exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseUser_Exercises_favoriteExercisesId",
                table: "ExerciseUser");

            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseUser_Users_UsersWhofavoritedId",
                table: "ExerciseUser");

            migrationBuilder.DropForeignKey(
                name: "FK_Workouts_Users_AuthorId",
                table: "Workouts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExerciseUser",
                table: "ExerciseUser");

            migrationBuilder.RenameTable(
                name: "ExerciseUser",
                newName: "favoriteExercises");

            migrationBuilder.RenameIndex(
                name: "IX_ExerciseUser_UsersWhofavoritedId",
                table: "favoriteExercises",
                newName: "IX_favoriteExercises_UsersWhofavoritedId");

            migrationBuilder.AlterColumn<int>(
                name: "AuthorId",
                table: "Workouts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "AuthorId",
                table: "Exercises",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Users_AuthorId",
                table: "Exercises",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Workouts_Users_AuthorId",
                table: "Workouts",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Users_AuthorId",
                table: "Exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_Workouts_Users_AuthorId",
                table: "Workouts");

            migrationBuilder.RenameTable(
                name: "favoriteExercises",
                newName: "ExerciseUser");

            migrationBuilder.RenameIndex(
                name: "IX_favoriteExercises_UsersWhofavoritedId",
                table: "ExerciseUser",
                newName: "IX_ExerciseUser_UsersWhofavoritedId");

            migrationBuilder.AlterColumn<int>(
                name: "AuthorId",
                table: "Workouts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AuthorId",
                table: "Exercises",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExerciseUser",
                table: "ExerciseUser",
                columns: new[] { "favoriteExercisesId", "UsersWhofavoritedId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Users_AuthorId",
                table: "Exercises",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseUser_Exercises_favoriteExercisesId",
                table: "ExerciseUser",
                column: "favoriteExercisesId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseUser_Users_UsersWhofavoritedId",
                table: "ExerciseUser",
                column: "UsersWhofavoritedId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Workouts_Users_AuthorId",
                table: "Workouts",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
