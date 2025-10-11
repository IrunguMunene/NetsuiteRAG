using Microsoft.EntityFrameworkCore;
using NetSuiteRAG.Shared.Models;

namespace NetSuiteRAG.Api.Data;

/// <summary>
/// Application database context for NetSuite RAG system.
/// Manages field definitions, queries, templates, and audit logs.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets or sets the field definitions table.
    /// </summary>
    public DbSet<FieldDefinition> FieldDefinitions => Set<FieldDefinition>();

    /// <summary>
    /// Configures the database model using Fluent API.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure FieldDefinition entity
        modelBuilder.Entity<FieldDefinition>(entity =>
        {
            entity.ToTable("field_definitions");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.RecordType)
                .HasColumnName("record_type")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.FieldId)
                .HasColumnName("field_id")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Label)
                .HasColumnName("label")
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(e => e.FieldType)
                .HasColumnName("field_type")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(2000);

            entity.Property(e => e.IsCustomField)
                .HasColumnName("is_custom_field")
                .IsRequired();

            entity.Property(e => e.ValidOperators)
                .HasColumnName("valid_operators")
                .HasConversion(
                    v => string.Join(',', v.Select(o => o.ToString())),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(s => Enum.Parse<OperatorType>(s))
                          .ToList())
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(e => e.RequiredJoin)
                .HasColumnName("required_join")
                .HasMaxLength(200);

            entity.Property(e => e.Aliases)
                .HasColumnName("aliases")
                .HasConversion(
                    v => v == null ? null : string.Join(',', v),
                    v => string.IsNullOrEmpty(v)
                        ? null
                        : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                .HasMaxLength(1000);

            entity.Property(e => e.SupportsFilter)
                .HasColumnName("supports_filter")
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.SupportsColumn)
                .HasColumnName("supports_column")
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.SupportsSummary)
                .HasColumnName("supports_summary")
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.Property(e => e.SchemaUrl)
                .HasColumnName("schema_url")
                .HasMaxLength(1000);

            entity.Property(e => e.Source)
                .HasColumnName("source")
                .HasMaxLength(200)
                .IsRequired();

            // Create indexes for common queries
            entity.HasIndex(e => new { e.RecordType, e.FieldId })
                .HasDatabaseName("ix_field_definitions_record_type_field_id")
                .IsUnique();

            entity.HasIndex(e => e.RecordType)
                .HasDatabaseName("ix_field_definitions_record_type");

            entity.HasIndex(e => e.RequiredJoin)
                .HasDatabaseName("ix_field_definitions_required_join");

            entity.HasIndex(e => e.IsCustomField)
                .HasDatabaseName("ix_field_definitions_is_custom_field");

            // Create a GIN index for full-text search on label (PostgreSQL-specific)
            entity.HasIndex(e => e.Label)
                .HasDatabaseName("ix_field_definitions_label_gin")
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");
        });
    }
}
