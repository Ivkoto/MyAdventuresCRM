using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuiteCase.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnforceGroupProgramIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Groups_GroupId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Programs_ProgramId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Groups_ParentGroupId",
                table: "Groups");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Groups_Id_ProgramId",
                table: "Groups",
                columns: new[] { "Id", "ProgramId" });

            migrationBuilder.CreateIndex(
                name: "IX_Groups_ParentGroupId_ProgramId",
                table: "Groups",
                columns: new[] { "ParentGroupId", "ProgramId" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_GroupId_ProgramId",
                table: "Bookings",
                columns: new[] { "GroupId", "ProgramId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Groups_GroupId_ProgramId",
                table: "Bookings",
                columns: new[] { "GroupId", "ProgramId" },
                principalTable: "Groups",
                principalColumns: new[] { "Id", "ProgramId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Groups_ParentGroupId_ProgramId",
                table: "Groups",
                columns: new[] { "ParentGroupId", "ProgramId" },
                principalTable: "Groups",
                principalColumns: new[] { "Id", "ProgramId" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Groups_GroupId_ProgramId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Groups_ParentGroupId_ProgramId",
                table: "Groups");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Groups_Id_ProgramId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_ParentGroupId_ProgramId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_GroupId_ProgramId",
                table: "Bookings");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Groups_GroupId",
                table: "Bookings",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Programs_ProgramId",
                table: "Bookings",
                column: "ProgramId",
                principalTable: "Programs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Groups_ParentGroupId",
                table: "Groups",
                column: "ParentGroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
