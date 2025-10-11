using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetSuiteRAG.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEmbeddingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_query_exemplars_embedded_at",
                table: "query_exemplars");

            migrationBuilder.DropIndex(
                name: "ix_glossary_terms_embedded_at",
                table: "glossary_terms");

            migrationBuilder.DropColumn(
                name: "embedded_at",
                table: "query_exemplars");

            migrationBuilder.DropColumn(
                name: "embedding_json",
                table: "query_exemplars");

            migrationBuilder.DropColumn(
                name: "embedded_at",
                table: "glossary_terms");

            migrationBuilder.DropColumn(
                name: "embedding_json",
                table: "glossary_terms");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "embedded_at",
                table: "query_exemplars",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "embedding_json",
                table: "query_exemplars",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "embedded_at",
                table: "glossary_terms",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "embedding_json",
                table: "glossary_terms",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_query_exemplars_embedded_at",
                table: "query_exemplars",
                column: "embedded_at");

            migrationBuilder.CreateIndex(
                name: "ix_glossary_terms_embedded_at",
                table: "glossary_terms",
                column: "embedded_at");
        }
    }
}
