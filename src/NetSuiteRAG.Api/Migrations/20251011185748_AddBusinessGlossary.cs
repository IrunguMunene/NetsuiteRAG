using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetSuiteRAG.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessGlossary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "glossary_terms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    term = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    definition = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    synonyms = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    examples = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: true),
                    embedding_json = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    embedded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_glossary_terms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "query_exemplars",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    natural_query = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    saved_search_plan_json = table.Column<string>(type: "jsonb", nullable: false),
                    explanation = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    tags = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    difficulty = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    record_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: true),
                    embedding_json = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    embedded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    retrieval_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_retrieved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_query_exemplars", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_glossary_terms_category",
                table: "glossary_terms",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "ix_glossary_terms_embedded_at",
                table: "glossary_terms",
                column: "embedded_at");

            migrationBuilder.CreateIndex(
                name: "ix_glossary_terms_is_active",
                table: "glossary_terms",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_glossary_terms_term",
                table: "glossary_terms",
                column: "term");

            migrationBuilder.CreateIndex(
                name: "ix_query_exemplars_difficulty",
                table: "query_exemplars",
                column: "difficulty");

            migrationBuilder.CreateIndex(
                name: "ix_query_exemplars_embedded_at",
                table: "query_exemplars",
                column: "embedded_at");

            migrationBuilder.CreateIndex(
                name: "ix_query_exemplars_is_active",
                table: "query_exemplars",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_query_exemplars_record_type",
                table: "query_exemplars",
                column: "record_type");

            migrationBuilder.CreateIndex(
                name: "ix_query_exemplars_retrieval_count",
                table: "query_exemplars",
                column: "retrieval_count");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "glossary_terms");

            migrationBuilder.DropTable(
                name: "query_exemplars");
        }
    }
}
