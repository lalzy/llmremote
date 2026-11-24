using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace llmremote.Migrations
{
    /// <inheritdoc />
    public partial class AddFavoriteAndCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "LLMModel",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFavorite",
                table: "LLMModel",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "LLMModel");

            migrationBuilder.DropColumn(
                name: "IsFavorite",
                table: "LLMModel");
        }
    }
}
