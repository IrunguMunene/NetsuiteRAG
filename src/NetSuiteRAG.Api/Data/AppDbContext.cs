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
    /// Gets or sets the custom field descriptors table.
    /// </summary>
    public DbSet<CustomFieldDescriptor> CustomFieldDescriptors => Set<CustomFieldDescriptor>();

    /// <summary>
    /// Gets or sets the custom field change log table.
    /// </summary>
    public DbSet<CustomFieldChangeLog> CustomFieldChangeLogs => Set<CustomFieldChangeLog>();

    /// <summary>
    /// Gets or sets the glossary terms table.
    /// </summary>
    public DbSet<GlossaryTerm> GlossaryTerms => Set<GlossaryTerm>();

    /// <summary>
    /// Gets or sets the query exemplars table.
    /// </summary>
    public DbSet<QueryExemplar> QueryExemplars => Set<QueryExemplar>();

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

            // Create a B-tree index on label for search queries
            entity.HasIndex(e => e.Label)
                .HasDatabaseName("ix_field_definitions_label");

            entity.Property(e => e.BusinessContext)
                .HasColumnName("business_context")
                .HasColumnType("jsonb");

            entity.Property(e => e.AmbiguityScore)
                .HasColumnName("ambiguity_score")
                .HasPrecision(5, 2);

            entity.Property(e => e.EnrichedAt)
                .HasColumnName("enriched_at");

            entity.HasIndex(e => e.EnrichedAt)
                .HasDatabaseName("ix_field_definitions_enriched_at");
        });

        // Configure CustomFieldDescriptor entity
        modelBuilder.Entity<CustomFieldDescriptor>(entity =>
        {
            entity.ToTable("custom_field_descriptors");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.FieldId)
                .HasColumnName("field_id")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.RecordType)
                .HasColumnName("record_type")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Label)
                .HasColumnName("label")
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(e => e.Type)
                .HasColumnName("type")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(2000);

            entity.Property(e => e.IsMandatory)
                .HasColumnName("is_mandatory")
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.DefaultValue)
                .HasColumnName("default_value")
                .HasMaxLength(500);

            entity.Property(e => e.IsStale)
                .HasColumnName("is_stale")
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            entity.Property(e => e.LastSeenAt)
                .HasColumnName("last_seen_at")
                .IsRequired();

            entity.Property(e => e.MetadataJson)
                .HasColumnName("metadata_json")
                .HasColumnType("jsonb");

            // Create indexes
            entity.HasIndex(e => new { e.RecordType, e.FieldId })
                .HasDatabaseName("ix_custom_field_descriptors_record_type_field_id")
                .IsUnique();

            entity.HasIndex(e => e.RecordType)
                .HasDatabaseName("ix_custom_field_descriptors_record_type");

            entity.HasIndex(e => e.IsStale)
                .HasDatabaseName("ix_custom_field_descriptors_is_stale");

            entity.HasIndex(e => e.LastSeenAt)
                .HasDatabaseName("ix_custom_field_descriptors_last_seen_at");

            entity.HasIndex(e => e.Label)
                .HasDatabaseName("ix_custom_field_descriptors_label");

            entity.Property(e => e.Aliases)
                .HasColumnName("aliases")
                .HasConversion(
                    v => v == null ? null : string.Join(',', v),
                    v => string.IsNullOrEmpty(v)
                        ? null
                        : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                .HasMaxLength(2000);

            entity.Property(e => e.BusinessContext)
                .HasColumnName("business_context")
                .HasColumnType("jsonb");

            entity.Property(e => e.AmbiguityScore)
                .HasColumnName("ambiguity_score")
                .HasPrecision(5, 2);

            entity.Property(e => e.EnrichedAt)
                .HasColumnName("enriched_at");

            entity.HasIndex(e => e.EnrichedAt)
                .HasDatabaseName("ix_custom_field_descriptors_enriched_at");
        });

        // Configure CustomFieldChangeLog entity
        modelBuilder.Entity<CustomFieldChangeLog>(entity =>
        {
            entity.ToTable("custom_field_change_logs");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.CustomFieldDescriptorId)
                .HasColumnName("custom_field_descriptor_id")
                .IsRequired();

            entity.Property(e => e.FieldId)
                .HasColumnName("field_id")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.ChangeType)
                .HasColumnName("change_type")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.OldValue)
                .HasColumnName("old_value")
                .HasColumnType("jsonb");

            entity.Property(e => e.NewValue)
                .HasColumnName("new_value")
                .HasColumnType("jsonb");

            entity.Property(e => e.DetectedAt)
                .HasColumnName("detected_at")
                .IsRequired();

            entity.Property(e => e.ChangeDescription)
                .HasColumnName("change_description")
                .HasMaxLength(1000);

            // Create indexes
            entity.HasIndex(e => e.CustomFieldDescriptorId)
                .HasDatabaseName("ix_custom_field_change_logs_descriptor_id");

            entity.HasIndex(e => e.FieldId)
                .HasDatabaseName("ix_custom_field_change_logs_field_id");

            entity.HasIndex(e => e.DetectedAt)
                .HasDatabaseName("ix_custom_field_change_logs_detected_at");

            entity.HasIndex(e => e.ChangeType)
                .HasDatabaseName("ix_custom_field_change_logs_change_type");

            // Configure relationship
            entity.HasOne(e => e.CustomFieldDescriptor)
                .WithMany()
                .HasForeignKey(e => e.CustomFieldDescriptorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure GlossaryTerm entity
        modelBuilder.Entity<GlossaryTerm>(entity =>
        {
            entity.ToTable("glossary_terms");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Term)
                .HasColumnName("term")
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(e => e.Definition)
                .HasColumnName("definition")
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(e => e.Synonyms)
                .HasColumnName("synonyms")
                .HasConversion(
                    v => v == null || v.Count == 0 ? null : string.Join('|', v),
                    v => string.IsNullOrEmpty(v)
                        ? new List<string>()
                        : v.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList())
                .HasMaxLength(2000);

            entity.Property(e => e.Category)
                .HasColumnName("category")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Examples)
                .HasColumnName("examples")
                .HasConversion(
                    v => v == null || v.Count == 0 ? null : string.Join('|', v),
                    v => string.IsNullOrEmpty(v)
                        ? new List<string>()
                        : v.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList())
                .HasMaxLength(4000);

            entity.Property(e => e.MetadataJson)
                .HasColumnName("metadata_json")
                .HasColumnType("jsonb");

            entity.Property(e => e.EmbeddingJson)
                .HasColumnName("embedding_json")
                .HasColumnType("jsonb");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            entity.Property(e => e.EmbeddedAt)
                .HasColumnName("embedded_at");

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .IsRequired()
                .HasDefaultValue(true);

            // Create indexes
            entity.HasIndex(e => e.Term)
                .HasDatabaseName("ix_glossary_terms_term");

            entity.HasIndex(e => e.Category)
                .HasDatabaseName("ix_glossary_terms_category");

            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("ix_glossary_terms_is_active");

            entity.HasIndex(e => e.EmbeddedAt)
                .HasDatabaseName("ix_glossary_terms_embedded_at");
        });

        // Configure QueryExemplar entity
        modelBuilder.Entity<QueryExemplar>(entity =>
        {
            entity.ToTable("query_exemplars");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.NaturalQuery)
                .HasColumnName("natural_query")
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(e => e.SavedSearchPlanJson)
                .HasColumnName("saved_search_plan_json")
                .HasColumnType("jsonb")
                .IsRequired();

            entity.Property(e => e.Explanation)
                .HasColumnName("explanation")
                .HasMaxLength(4000)
                .IsRequired();

            entity.Property(e => e.Tags)
                .HasColumnName("tags")
                .HasConversion(
                    v => v == null || v.Count == 0 ? null : string.Join(',', v),
                    v => string.IsNullOrEmpty(v)
                        ? new List<string>()
                        : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                .HasMaxLength(1000);

            entity.Property(e => e.Difficulty)
                .HasColumnName("difficulty")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.RecordType)
                .HasColumnName("record_type")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.MetadataJson)
                .HasColumnName("metadata_json")
                .HasColumnType("jsonb");

            entity.Property(e => e.EmbeddingJson)
                .HasColumnName("embedding_json")
                .HasColumnType("jsonb");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            entity.Property(e => e.EmbeddedAt)
                .HasColumnName("embedded_at");

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.RetrievalCount)
                .HasColumnName("retrieval_count")
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(e => e.LastRetrievedAt)
                .HasColumnName("last_retrieved_at");

            // Create indexes
            entity.HasIndex(e => e.RecordType)
                .HasDatabaseName("ix_query_exemplars_record_type");

            entity.HasIndex(e => e.Difficulty)
                .HasDatabaseName("ix_query_exemplars_difficulty");

            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("ix_query_exemplars_is_active");

            entity.HasIndex(e => e.EmbeddedAt)
                .HasDatabaseName("ix_query_exemplars_embedded_at");

            entity.HasIndex(e => e.RetrievalCount)
                .HasDatabaseName("ix_query_exemplars_retrieval_count");
        });
    }
}
