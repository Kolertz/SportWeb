using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportWeb.Migrations
{
    /// <inheritdoc />
    public partial class IsPublicFavourites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublicFavourites",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublicFavourites",
                table: "Users");
        }
    }
}
