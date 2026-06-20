using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuiteCase.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameProgramTablesToTravelPrograms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingItems_ProgramPricingRules_ProgramPricingRuleId",
                table: "BookingItems");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingOptions_ProgramOptions_ProgramOptionId",
                table: "BookingOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupOptions_ProgramOptions_ProgramOptionId",
                table: "GroupOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Programs_ProgramId",
                table: "Groups");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgramOptions_Programs_ProgramId",
                table: "ProgramOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgramPricingRules_Groups_GroupId",
                table: "ProgramPricingRules");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgramPricingRules_Programs_ProgramId",
                table: "ProgramPricingRules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProgramOptions",
                table: "ProgramOptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProgramPricingRules",
                table: "ProgramPricingRules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Programs",
                table: "Programs");

            migrationBuilder.RenameTable(
                name: "Programs",
                newName: "TravelPrograms");

            migrationBuilder.RenameTable(
                name: "ProgramOptions",
                newName: "TravelProgramOptions");

            migrationBuilder.RenameTable(
                name: "ProgramPricingRules",
                newName: "TravelProgramPricingRules");

            migrationBuilder.RenameIndex(
                name: "IX_ProgramPricingRules_GroupId",
                table: "TravelProgramPricingRules",
                newName: "IX_TravelProgramPricingRules_GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_ProgramPricingRules_ProgramId",
                table: "TravelProgramPricingRules",
                newName: "IX_TravelProgramPricingRules_ProgramId");

            migrationBuilder.RenameIndex(
                name: "IX_ProgramPricingRules_ProgramId_GroupId_Name",
                table: "TravelProgramPricingRules",
                newName: "IX_TravelProgramPricingRules_ProgramId_GroupId_Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TravelPrograms",
                table: "TravelPrograms",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TravelProgramOptions",
                table: "TravelProgramOptions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TravelProgramPricingRules",
                table: "TravelProgramPricingRules",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TravelProgramOptions_TravelPrograms_ProgramId",
                table: "TravelProgramOptions",
                column: "ProgramId",
                principalTable: "TravelPrograms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TravelProgramPricingRules_Groups_GroupId",
                table: "TravelProgramPricingRules",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TravelProgramPricingRules_TravelPrograms_ProgramId",
                table: "TravelProgramPricingRules",
                column: "ProgramId",
                principalTable: "TravelPrograms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingItems_TravelProgramPricingRules_ProgramPricingRuleId",
                table: "BookingItems",
                column: "ProgramPricingRuleId",
                principalTable: "TravelProgramPricingRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingOptions_TravelProgramOptions_ProgramOptionId",
                table: "BookingOptions",
                column: "ProgramOptionId",
                principalTable: "TravelProgramOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupOptions_TravelProgramOptions_ProgramOptionId",
                table: "GroupOptions",
                column: "ProgramOptionId",
                principalTable: "TravelProgramOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_TravelPrograms_ProgramId",
                table: "Groups",
                column: "ProgramId",
                principalTable: "TravelPrograms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingItems_TravelProgramPricingRules_ProgramPricingRuleId",
                table: "BookingItems");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingOptions_TravelProgramOptions_ProgramOptionId",
                table: "BookingOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupOptions_TravelProgramOptions_ProgramOptionId",
                table: "GroupOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Groups_TravelPrograms_ProgramId",
                table: "Groups");

            migrationBuilder.DropForeignKey(
                name: "FK_TravelProgramOptions_TravelPrograms_ProgramId",
                table: "TravelProgramOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_TravelProgramPricingRules_Groups_GroupId",
                table: "TravelProgramPricingRules");

            migrationBuilder.DropForeignKey(
                name: "FK_TravelProgramPricingRules_TravelPrograms_ProgramId",
                table: "TravelProgramPricingRules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TravelProgramOptions",
                table: "TravelProgramOptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TravelProgramPricingRules",
                table: "TravelProgramPricingRules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TravelPrograms",
                table: "TravelPrograms");

            migrationBuilder.RenameIndex(
                name: "IX_TravelProgramPricingRules_GroupId",
                table: "TravelProgramPricingRules",
                newName: "IX_ProgramPricingRules_GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_TravelProgramPricingRules_ProgramId",
                table: "TravelProgramPricingRules",
                newName: "IX_ProgramPricingRules_ProgramId");

            migrationBuilder.RenameIndex(
                name: "IX_TravelProgramPricingRules_ProgramId_GroupId_Name",
                table: "TravelProgramPricingRules",
                newName: "IX_ProgramPricingRules_ProgramId_GroupId_Name");

            migrationBuilder.RenameTable(
                name: "TravelPrograms",
                newName: "Programs");

            migrationBuilder.RenameTable(
                name: "TravelProgramOptions",
                newName: "ProgramOptions");

            migrationBuilder.RenameTable(
                name: "TravelProgramPricingRules",
                newName: "ProgramPricingRules");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Programs",
                table: "Programs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProgramOptions",
                table: "ProgramOptions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProgramPricingRules",
                table: "ProgramPricingRules",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProgramOptions_Programs_ProgramId",
                table: "ProgramOptions",
                column: "ProgramId",
                principalTable: "Programs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProgramPricingRules_Groups_GroupId",
                table: "ProgramPricingRules",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProgramPricingRules_Programs_ProgramId",
                table: "ProgramPricingRules",
                column: "ProgramId",
                principalTable: "Programs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingItems_ProgramPricingRules_ProgramPricingRuleId",
                table: "BookingItems",
                column: "ProgramPricingRuleId",
                principalTable: "ProgramPricingRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingOptions_ProgramOptions_ProgramOptionId",
                table: "BookingOptions",
                column: "ProgramOptionId",
                principalTable: "ProgramOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupOptions_ProgramOptions_ProgramOptionId",
                table: "GroupOptions",
                column: "ProgramOptionId",
                principalTable: "ProgramOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Programs_ProgramId",
                table: "Groups",
                column: "ProgramId",
                principalTable: "Programs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
