using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetSuiteRAG.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialFieldDefinitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "field_definitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    record_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    field_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    label = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    field_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    is_custom_field = table.Column<bool>(type: "boolean", nullable: false),
                    valid_operators = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    required_join = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    aliases = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    supports_filter = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    supports_column = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    supports_summary = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    schema_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    source = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_field_definitions", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_field_definitions_is_custom_field",
                table: "field_definitions",
                column: "is_custom_field");

            migrationBuilder.CreateIndex(
                name: "ix_field_definitions_label",
                table: "field_definitions",
                column: "label");

            migrationBuilder.CreateIndex(
                name: "ix_field_definitions_record_type",
                table: "field_definitions",
                column: "record_type");

            migrationBuilder.CreateIndex(
                name: "ix_field_definitions_record_type_field_id",
                table: "field_definitions",
                columns: new[] { "record_type", "field_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_field_definitions_required_join",
                table: "field_definitions",
                column: "required_join");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "field_definitions");
        }
    }
}
