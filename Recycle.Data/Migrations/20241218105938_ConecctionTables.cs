using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recycle.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConecctionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Materials_Parts_PartId",
                table: "Materials");

            migrationBuilder.DropForeignKey(
                name: "FK_Parts_Products_ProductId",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_ProductId",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Materials_PartId",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "PartId",
                table: "Materials");

            migrationBuilder.CreateTable(
                name: "PartMaterials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "ProductParts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductParts_Parts_PartId",
                        column: x => x.PartId,
                        principalTable: "Parts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductParts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
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

            migrationBuilder.CreateIndex(
                name: "IX_ProductParts_PartId",
                table: "ProductParts",
                column: "PartId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductParts_ProductId",
                table: "ProductParts",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartMaterials");

            migrationBuilder.DropTable(
                name: "ProductParts");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                table: "Parts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PartId",
                table: "Materials",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Parts_ProductId",
                table: "Parts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Materials_PartId",
                table: "Materials",
                column: "PartId");

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_Parts_PartId",
                table: "Materials",
                column: "PartId",
                principalTable: "Parts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Parts_Products_ProductId",
                table: "Parts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
