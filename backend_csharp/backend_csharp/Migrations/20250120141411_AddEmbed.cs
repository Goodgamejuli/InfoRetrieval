using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_csharp.Migrations
{
    /// <inheritdoc />
    public partial class AddEmbed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Embed",
                table: "DatabaseSongs",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Embed",
                table: "DatabaseSongs");
        }
    }
}
