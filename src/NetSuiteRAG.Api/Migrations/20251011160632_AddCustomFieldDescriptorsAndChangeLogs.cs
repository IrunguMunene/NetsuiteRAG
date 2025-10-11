using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetSuiteRAG.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomFieldDescriptorsAndChangeLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "custom_field_descriptors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    field_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    record_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    label = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    is_mandatory = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    default_value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_stale = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_custom_field_descriptors", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "custom_field_change_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    custom_field_descriptor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    field_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    change_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    old_value = table.Column<string>(type: "jsonb", nullable: true),
                    new_value = table.Column<string>(type: "jsonb", nullable: true),
                    detected_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    change_description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_custom_field_change_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_custom_field_change_logs_custom_field_descriptors_custom_fi~",
                        column: x => x.custom_field_descriptor_id,
                        principalTable: "custom_field_descriptors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_custom_field_change_logs_change_type",
                table: "custom_field_change_logs",
                column: "change_type");

            migrationBuilder.CreateIndex(
                name: "ix_custom_field_change_logs_descriptor_id",
                table: "custom_field_change_logs",
                column: "custom_field_descriptor_id");

            migrationBuilder.CreateIndex(
                name: "ix_custom_field_change_logs_detected_at",
                table: "custom_field_change_logs",
                column: "detected_at");

            migrationBuilder.CreateIndex(
                name: "ix_custom_field_change_logs_field_id",
                table: "custom_field_change_logs",
                column: "field_id");

            migrationBuilder.CreateIndex(
                name: "ix_custom_field_descriptors_is_stale",
                table: "custom_field_descriptors",
                column: "is_stale");

            migrationBuilder.CreateIndex(
                name: "ix_custom_field_descriptors_label",
                table: "custom_field_descriptors",
                column: "label");

            migrationBuilder.CreateIndex(
                name: "ix_custom_field_descriptors_last_seen_at",
                table: "custom_field_descriptors",
                column: "last_seen_at");

            migrationBuilder.CreateIndex(
                name: "ix_custom_field_descriptors_record_type",
                table: "custom_field_descriptors",
                column: "record_type");

            migrationBuilder.CreateIndex(
                name: "ix_custom_field_descriptors_record_type_field_id",
                table: "custom_field_descriptors",
                columns: new[] { "record_type", "field_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "custom_field_change_logs");

            migrationBuilder.DropTable(
                name: "custom_field_descriptors");
        }
    }
}
