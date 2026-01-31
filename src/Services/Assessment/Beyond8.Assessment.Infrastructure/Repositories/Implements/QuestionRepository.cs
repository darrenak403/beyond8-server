using System.Text.Json;
using Beyond8.Assessment.Domain.Entities;
using Beyond8.Assessment.Domain.Repositories.Interfaces;
using Beyond8.Assessment.Infrastructure.Data;
using Beyond8.Common.Data.Implements;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Assessment.Infrastructure.Repositories.Implements;

public class QuestionRepository(AssessmentDbContext context) : PostgresRepository<Question>(context), IQuestionRepository
{
    public async Task AddRangeAsync(List<Question> questions)
    {
        await _dbSet.AddRangeAsync(questions);
        await context.SaveChangesAsync();
    }

    public async Task<(List<Question> Items, int TotalCount)> GetPagedByInstructorAsync(
        Guid instructorId,
        int pageNumber,
        int pageSize,
        string? tag = null,
        bool orderByDescending = true)
    {
        IQueryable<Question> query = _dbSet
            .Where(q => q.InstructorId == instructorId && q.IsActive);

        if (!string.IsNullOrWhiteSpace(tag))
        {
            var tagJson = JsonSerializer.Serialize(new[] { tag.Trim() });
            query = query.Where(q => EF.Functions.JsonContains(q.Tags, tagJson));
        }

        var totalCount = await query.CountAsync();

        var ordered = orderByDescending
            ? query.OrderByDescending(q => q.CreatedAt)
            : query.OrderBy(q => q.CreatedAt);

        var items = await ordered
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<(string Tag, int Count)>> GetTagCountsByInstructorAsync(Guid instructorId)
    {
        var connection = context.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT elem AS "Tag", COUNT(*)::int AS "Count"
            FROM "Questions" q
            CROSS JOIN LATERAL jsonb_array_elements_text(q."Tags"::jsonb) AS elem
            WHERE q."InstructorId" = @instructorId AND q."IsActive" = true AND q."DeletedAt" IS NULL
            GROUP BY elem
            ORDER BY "Count" DESC
            """;

        var param = command.CreateParameter();
        param.ParameterName = "instructorId";
        param.Value = instructorId;
        command.Parameters.Add(param);

        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        var results = new List<(string Tag, int Count)>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var tag = reader.GetString(0);
            var count = reader.GetInt32(1);
            results.Add((tag, count));
        }

        return results;
    }
}
