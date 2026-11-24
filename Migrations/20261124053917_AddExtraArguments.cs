using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace llmremote.Migrations
{
    /// <inheritdoc />
    public partial class AddExtraArguments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Arguments",
                table: "LLMModel",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Arguments",
                table: "LLMModel");
        }
    }
}
