using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Recycle.Data.Migrations
{
    /// <inheritdoc />
    public partial class MAil : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sent",
                table: "EmailMessage");

            migrationBuilder.RenameColumn(
                name: "RecipientName",
                table: "EmailMessage",
                newName: "Sender");

            migrationBuilder.RenameColumn(
                name: "RecipientEmail",
                table: "EmailMessage",
                newName: "Receiver");

            migrationBuilder.RenameColumn(
                name: "FromName",
                table: "EmailMessage",
                newName: "ModifiedBy");

            migrationBuilder.RenameColumn(
                name: "FromEmail",
                table: "EmailMessage",
                newName: "CreatedBy");

            migrationBuilder.AddColumn<Instant>(
                name: "DeletedAt",
                table: "EmailMessage",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "EmailMessage",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Instant>(
                name: "ModifiedAt",
                table: "EmailMessage",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));

            migrationBuilder.AddColumn<Instant>(
                name: "ScheduledAt",
                table: "EmailMessage",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));

            migrationBuilder.AddColumn<Instant>(
                name: "SentAt",
                table: "EmailMessage",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "EmailMessage");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "EmailMessage");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "EmailMessage");

            migrationBuilder.DropColumn(
                name: "ScheduledAt",
                table: "EmailMessage");

            migrationBuilder.DropColumn(
                name: "SentAt",
                table: "EmailMessage");

            migrationBuilder.RenameColumn(
                name: "Sender",
                table: "EmailMessage",
                newName: "RecipientName");

            migrationBuilder.RenameColumn(
                name: "Receiver",
                table: "EmailMessage",
                newName: "RecipientEmail");

            migrationBuilder.RenameColumn(
                name: "ModifiedBy",
                table: "EmailMessage",
                newName: "FromName");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "EmailMessage",
                newName: "FromEmail");

            migrationBuilder.AddColumn<bool>(
                name: "Sent",
                table: "EmailMessage",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
