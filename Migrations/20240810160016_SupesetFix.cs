using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportWeb.Migrations
{
    /// <inheritdoc />
    public partial class SupesetFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExercisePosition",
                table: "WorkoutExercises",
                newName: "Position");

            migrationBuilder.AddColumn<int>(
                name: "SupersetId",
                table: "WorkoutExercises",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Supersets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkoutId = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supersets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Supersets_Workouts_WorkoutId",
                        column: x => x.WorkoutId,
                        principalTable: "Workouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutExercises_SupersetId",
                table: "WorkoutExercises",
                column: "SupersetId");

            migrationBuilder.CreateIndex(
                name: "IX_Supersets_WorkoutId",
                table: "Supersets",
                column: "WorkoutId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutExercises_Supersets_SupersetId",
                table: "WorkoutExercises",
                column: "SupersetId",
                principalTable: "Supersets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutExercises_Supersets_SupersetId",
                table: "WorkoutExercises");

            migrationBuilder.DropTable(
                name: "Supersets");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutExercises_SupersetId",
                table: "WorkoutExercises");

            migrationBuilder.DropColumn(
                name: "SupersetId",
                table: "WorkoutExercises");

            migrationBuilder.RenameColumn(
                name: "Position",
                table: "WorkoutExercises",
                newName: "ExercisePosition");
        }
    }
}
