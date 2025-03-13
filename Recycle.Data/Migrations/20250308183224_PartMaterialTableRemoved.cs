using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recycle.Data.Migrations
{
    /// <inheritdoc />
    public partial class PartMaterialTableRemoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartMaterials");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Parts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "PartMaterialId",
                table: "Parts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Parts_PartMaterialId",
                table: "Parts",
                column: "PartMaterialId");

            migrationBuilder.AddForeignKey(
                name: "FK_Parts_Materials_PartMaterialId",
                table: "Parts",
                column: "PartMaterialId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parts_Materials_PartMaterialId",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_PartMaterialId",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "PartMaterialId",
                table: "Parts");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Parts",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "PartMaterials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartMaterials_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartMaterials_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartMaterials_MaterialId",
                table: "PartMaterials",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_PartMaterials_PartId",
                table: "PartMaterials",
                column: "PartId");
        }
    }
}
