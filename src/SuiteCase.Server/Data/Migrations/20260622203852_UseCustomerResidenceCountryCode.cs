using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuiteCase.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class UseCustomerResidenceCountryCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResidenceCountry",
                table: "Customers");

            migrationBuilder.AddColumn<string>(
                name: "ResidenceCountryCode",
                table: "Customers",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "BG");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResidenceCountryCode",
                table: "Customers");

            migrationBuilder.AddColumn<string>(
                name: "ResidenceCountry",
                table: "Customers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
