using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace XatiCraft.Migrations
{
    /// <inheritdoc />
    public partial class InitalCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    price = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("products_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product_metadata",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    original_file = table.Column<string>(type: "text", nullable: false),
                    file_key = table.Column<string>(type: "text", nullable: false),
                    location = table.Column<string>(type: "text", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("product_metadata_pkey", x => x.id);
                    table.ForeignKey(
                        name: "product_metadata_product_id_fkey",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_product_metadata_product_id",
                table: "product_metadata",
                column: "product_id");
            
            migrationBuilder.Sql(@"
CREATE OR REPLACE VIEW v_products AS
SELECT
    p.id,
    p.title,
    p.description,
    p.price,
    p.""timestamp"",
    COALESCE(
        jsonb_agg(
            jsonb_build_object(
                'id', pm.id,
                'original_file', pm.original_file,
                'file_key', pm.file_key,
                'location', pm.location,
                'timestamp', pm.""timestamp""
            )
        ) FILTER (WHERE pm.id IS NOT NULL),
        '[]'::jsonb
    ) AS metadata
FROM products p
LEFT JOIN product_metadata pm ON pm.product_id = p.id
GROUP BY p.id;
");
            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_metadata");

            migrationBuilder.DropTable(
                name: "products");
            
            migrationBuilder.Sql("DROP VIEW IF EXISTS v_products;");
        }
    }
}
