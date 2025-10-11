using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace NetSuiteRAG.Api.Data.Seeds;

/// <summary>
/// Seeds the database with business glossary terms and query exemplars.
/// </summary>
public static class GlossarySeeder
{
    /// <summary>
    /// Seeds glossary terms and query exemplars if they don't already exist.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">Logger for tracking seeding progress.</param>
    public static async Task SeedAsync(AppDbContext context, ILogger logger)
    {
        try
        {
            // Check if glossary terms already exist
            var existingTermsCount = await context.GlossaryTerms.CountAsync();

            if (existingTermsCount == 0)
            {
                logger.LogInformation("Seeding glossary terms...");

                var terms = GlossaryTermsData.GetGlossaryTerms();
                await context.GlossaryTerms.AddRangeAsync(terms);
                await context.SaveChangesAsync();

                logger.LogInformation("Seeded {Count} glossary terms", terms.Count);
            }
            else
            {
                logger.LogInformation("Glossary terms already seeded ({Count} terms exist)", existingTermsCount);
            }

            // Check if query exemplars already exist
            var existingExemplarsCount = await context.QueryExemplars.CountAsync();

            if (existingExemplarsCount == 0)
            {
                logger.LogInformation("Seeding query exemplars...");

                var exemplars = QueryExemplarsData.GetQueryExemplars();
                await context.QueryExemplars.AddRangeAsync(exemplars);
                await context.SaveChangesAsync();

                logger.LogInformation("Seeded {Count} query exemplars", exemplars.Count);
            }
            else
            {
                logger.LogInformation("Query exemplars already seeded ({Count} exemplars exist)", existingExemplarsCount);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding glossary data");
            throw;
        }
    }
}
