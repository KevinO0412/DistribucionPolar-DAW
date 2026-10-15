namespace Infrastructure.Persistence.Entities;

/// <summary>
/// Contador de numeración por secuencia, año y mes (año y mes en 0 para secuencias globales).
/// Se incrementa de forma atómica en PostgreSQL con INSERT … ON CONFLICT … RETURNING.
/// </summary>
public sealed class DocumentCounter
{
    public string Sequence { get; set; } = default!;

    public int Year { get; set; }

    public int Month { get; set; }

    public int CurrentNumber { get; set; }
}
