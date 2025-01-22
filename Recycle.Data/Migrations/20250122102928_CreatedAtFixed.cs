using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recycle.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreatedAtFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "EmailMessage",
                newName: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "EmailMessage",
                newName: "CreatedBy");
        }
    }
}
