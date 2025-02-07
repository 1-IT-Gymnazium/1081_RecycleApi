using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recycle.Data.Migrations
{
    /// <inheritdoc />
    public partial class LocationThrownOut : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrashCansMaterialLocations_Locations_LocationId",
                table: "TrashCansMaterialLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_TrashCansMaterialLocations_Materials_MaterialId",
                table: "TrashCansMaterialLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_TrashCansMaterialLocations_TrashCans_TrashCanId",
                table: "TrashCansMaterialLocations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TrashCansMaterialLocations",
                table: "TrashCansMaterialLocations");

            migrationBuilder.DropIndex(
                name: "IX_TrashCansMaterialLocations_LocationId",
                table: "TrashCansMaterialLocations");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "TrashCansMaterialLocations");

            migrationBuilder.RenameTable(
                name: "TrashCansMaterialLocations",
                newName: "TrashCansMaterials");

            migrationBuilder.RenameIndex(
                name: "IX_TrashCansMaterialLocations_TrashCanId",
                table: "TrashCansMaterials",
                newName: "IX_TrashCansMaterials_TrashCanId");

            migrationBuilder.RenameIndex(
                name: "IX_TrashCansMaterialLocations_MaterialId",
                table: "TrashCansMaterials",
                newName: "IX_TrashCansMaterials_MaterialId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrashCansMaterials",
                table: "TrashCansMaterials",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TrashCansMaterials_Materials_MaterialId",
                table: "TrashCansMaterials",
                column: "MaterialId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrashCansMaterials_TrashCans_TrashCanId",
                table: "TrashCansMaterials",
                column: "TrashCanId",
                principalTable: "TrashCans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrashCansMaterials_Materials_MaterialId",
                table: "TrashCansMaterials");

            migrationBuilder.DropForeignKey(
                name: "FK_TrashCansMaterials_TrashCans_TrashCanId",
                table: "TrashCansMaterials");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TrashCansMaterials",
                table: "TrashCansMaterials");

            migrationBuilder.RenameTable(
                name: "TrashCansMaterials",
                newName: "TrashCansMaterialLocations");

            migrationBuilder.RenameIndex(
                name: "IX_TrashCansMaterials_TrashCanId",
                table: "TrashCansMaterialLocations",
                newName: "IX_TrashCansMaterialLocations_TrashCanId");

            migrationBuilder.RenameIndex(
                name: "IX_TrashCansMaterials_MaterialId",
                table: "TrashCansMaterialLocations",
                newName: "IX_TrashCansMaterialLocations_MaterialId");

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "TrashCansMaterialLocations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrashCansMaterialLocations",
                table: "TrashCansMaterialLocations",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TrashCansMaterialLocations_LocationId",
                table: "TrashCansMaterialLocations",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrashCansMaterialLocations_Locations_LocationId",
                table: "TrashCansMaterialLocations",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrashCansMaterialLocations_Materials_MaterialId",
                table: "TrashCansMaterialLocations",
                column: "MaterialId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrashCansMaterialLocations_TrashCans_TrashCanId",
                table: "TrashCansMaterialLocations",
                column: "TrashCanId",
                principalTable: "TrashCans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
