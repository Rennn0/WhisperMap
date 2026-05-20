using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XcLib.Data.Postgres.XatiCraft.Migrations
{
    /// <inheritdoc />
    public partial class AddPageVisitorIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "browser",
                table: "page_visitors",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_page_visitors_page_ip_address",
                table: "page_visitors",
                columns: new[] { "page", "ip_address" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_page_visitors_page_ip_address",
                table: "page_visitors");

            migrationBuilder.AlterColumn<string>(
                name: "browser",
                table: "page_visitors",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);
        }
    }
}
