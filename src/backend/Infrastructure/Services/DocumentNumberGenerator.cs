using Core.Application.Abstractions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

/// <summary>
/// Correlativo atómico con una sola sentencia SQL (INSERT … ON CONFLICT … DO UPDATE … RETURNING):
/// PostgreSQL bloquea la fila del contador, así que dos usuarios nunca reciben el mismo número.
/// Corre dentro de la transacción del proceso: si este se revierte, el número no se consume.
/// </summary>
public sealed class DocumentNumberGenerator(ApplicationDbContext context) : IDocumentNumberGenerator
{
    public async Task<int> NextAsync(string sequence, int year, int month, int floor = 0, CancellationToken cancellationToken = default)
    {
        var result = await context.Database.SqlQuery<int>($"""
            INSERT INTO "DocumentCounters" ("Sequence", "Year", "Month", "CurrentNumber")
            VALUES ({sequence}, {year}, {month}, {floor + 1})
            ON CONFLICT ("Sequence", "Year", "Month")
            DO UPDATE SET "CurrentNumber" = GREATEST("DocumentCounters"."CurrentNumber", {floor}) + 1
            RETURNING "CurrentNumber" AS "Value"
            """).ToListAsync(cancellationToken);
        return result.Single();
    }
}
