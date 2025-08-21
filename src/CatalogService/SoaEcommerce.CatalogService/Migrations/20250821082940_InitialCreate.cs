using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SoaEcommerce.CatalogService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Stock = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CreatedAt", "IsActive", "Name", "Price", "Stock" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2025, 8, 1, 8, 29, 39, 953, DateTimeKind.Utc).AddTicks(3420), true, "Notebook Dell Inspiron", 2999.99m, 10 },
                    { new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2025, 8, 6, 8, 29, 39, 953, DateTimeKind.Utc).AddTicks(3430), true, "Mouse Gamer RGB", 89.90m, 50 },
                    { new Guid("55555555-5555-5555-5555-555555555555"), new DateTime(2025, 8, 11, 8, 29, 39, 953, DateTimeKind.Utc).AddTicks(3430), true, "Teclado Mecânico", 299.90m, 25 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
