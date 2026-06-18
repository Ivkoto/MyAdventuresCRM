using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuiteCase.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FirstNameLatin = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MiddleNameLatin = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastNameLatin = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NationalIdEncrypted = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    NationalIdHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    PassportNumberEncrypted = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    PassportNumberHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    PassportExpiresOn = table.Column<DateOnly>(type: "date", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    ResidenceCountry = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_FirstName_LastName",
                table: "Customers",
                columns: new[] { "FirstName", "LastName" });

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

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PhoneNumber",
                table: "Customers",
                column: "PhoneNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
