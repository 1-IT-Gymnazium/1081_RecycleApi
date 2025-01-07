﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recycle.Data.Migrations
{
    /// <inheritdoc />
    public partial class RegionsRemoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Region",
                table: "Locations");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId1",
                table: "ProductParts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ProductParts_ProductId1",
                table: "ProductParts",
                column: "ProductId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductParts_Products_ProductId1",
                table: "ProductParts",
                column: "ProductId1",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductParts_Products_ProductId1",
                table: "ProductParts");

            migrationBuilder.DropIndex(
                name: "IX_ProductParts_ProductId1",
                table: "ProductParts");

            migrationBuilder.DropColumn(
                name: "ProductId1",
                table: "ProductParts");

            migrationBuilder.AddColumn<int>(
                name: "Region",
                table: "Locations",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
