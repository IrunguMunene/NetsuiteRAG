using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetSuiteRAG.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldEnrichmentColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ambiguity_score",
                table: "field_definitions",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "business_context",
                table: "field_definitions",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "enriched_at",
                table: "field_definitions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "aliases",
                table: "custom_field_descriptors",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ambiguity_score",
                table: "custom_field_descriptors",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "business_context",
                table: "custom_field_descriptors",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "enriched_at",
                table: "custom_field_descriptors",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_field_definitions_enriched_at",
                table: "field_definitions",
                column: "enriched_at");

            migrationBuilder.CreateIndex(
                name: "ix_custom_field_descriptors_enriched_at",
                table: "custom_field_descriptors",
                column: "enriched_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_field_definitions_enriched_at",
                table: "field_definitions");

            migrationBuilder.DropIndex(
                name: "ix_custom_field_descriptors_enriched_at",
                table: "custom_field_descriptors");

            migrationBuilder.DropColumn(
                name: "ambiguity_score",
                table: "field_definitions");

            migrationBuilder.DropColumn(
                name: "business_context",
                table: "field_definitions");

            migrationBuilder.DropColumn(
                name: "enriched_at",
                table: "field_definitions");

            migrationBuilder.DropColumn(
                name: "aliases",
                table: "custom_field_descriptors");

            migrationBuilder.DropColumn(
                name: "ambiguity_score",
                table: "custom_field_descriptors");

            migrationBuilder.DropColumn(
                name: "business_context",
                table: "custom_field_descriptors");

            migrationBuilder.DropColumn(
                name: "enriched_at",
                table: "custom_field_descriptors");
        }
    }
}
