using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace chefeia.Migrations
{
    /// <inheritdoc />
    public partial class CriarEventosWebhookAsaas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AsaasWebhookEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PaymentId = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CustomerId = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    SubscriptionId = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    PaymentStatus = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ExternalReference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Processed = table.Column<bool>(type: "boolean", nullable: false),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsaasWebhookEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_LastPaymentId",
                table: "UserSubscriptions",
                column: "LastPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_AsaasWebhookEvents_CustomerId",
                table: "AsaasWebhookEvents",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_AsaasWebhookEvents_EventId",
                table: "AsaasWebhookEvents",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AsaasWebhookEvents_EventType",
                table: "AsaasWebhookEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_AsaasWebhookEvents_PaymentId",
                table: "AsaasWebhookEvents",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_AsaasWebhookEvents_Processed",
                table: "AsaasWebhookEvents",
                column: "Processed");

            migrationBuilder.CreateIndex(
                name: "IX_AsaasWebhookEvents_ReceivedAt",
                table: "AsaasWebhookEvents",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AsaasWebhookEvents_SubscriptionId",
                table: "AsaasWebhookEvents",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_AsaasWebhookEvents_Success",
                table: "AsaasWebhookEvents",
                column: "Success");

            migrationBuilder.CreateIndex(
                name: "IX_AsaasWebhookEvents_UserId",
                table: "AsaasWebhookEvents",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AsaasWebhookEvents");

            migrationBuilder.DropIndex(
                name: "IX_UserSubscriptions_LastPaymentId",
                table: "UserSubscriptions");
        }
    }
}
