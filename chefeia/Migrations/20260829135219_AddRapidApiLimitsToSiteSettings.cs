using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chefeia.Migrations
{
    /// <inheritdoc />
    public partial class AddRapidApiLimitsToSiteSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RapidApiMonthlyCreditLimit",
                table: "SiteSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RapidApiMonthlyRequestLimit",
                table: "SiteSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "RapidApiMonthlyCreditLimit", "RapidApiMonthlyRequestLimit" },
                values: new object[] { 100000, 100 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RapidApiMonthlyCreditLimit",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "RapidApiMonthlyRequestLimit",
                table: "SiteSettings");
        }
    }
}
