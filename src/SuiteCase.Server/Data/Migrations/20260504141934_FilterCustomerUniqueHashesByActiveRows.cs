using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuiteCase.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class FilterCustomerUniqueHashesByActiveRows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_NationalIdHash",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_PassportNumberHash",
                table: "Customers");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_NationalIdHash",
                table: "Customers",
                column: "NationalIdHash",
                unique: true,
                filter: "[NationalIdHash] IS NOT NULL AND [DeletedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PassportNumberHash",
                table: "Customers",
                column: "PassportNumberHash",
                unique: true,
                filter: "[PassportNumberHash] IS NOT NULL AND [DeletedAt] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_NationalIdHash",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_PassportNumberHash",
                table: "Customers");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_NationalIdHash",
                table: "Customers",
                column: "NationalIdHash",
                unique: true,
                filter: "[NationalIdHash] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PassportNumberHash",
                table: "Customers",
                column: "PassportNumberHash",
                unique: true,
                filter: "[PassportNumberHash] IS NOT NULL");
        }
    }
}
