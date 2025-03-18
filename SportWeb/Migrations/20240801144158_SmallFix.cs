using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportWeb.Migrations
{
    /// <inheritdoc />
    public partial class SmallFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Exercises");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "Categories",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "pictureUrl",
                table: "Categories",
                newName: "PictureUrl");

            migrationBuilder.AddColumn<string>(
                name: "PictureUrl",
                table: "Exercises",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "PictureUrl",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PictureUrl",
                table: "Exercises");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Categories",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "PictureUrl",
                table: "Categories",
                newName: "pictureUrl");

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Exercises",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "pictureUrl",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}