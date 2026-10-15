namespace Core.Domain.Exceptions;

/// <summary>
/// Violación de una regla de negocio. La API la traduce a 400 Bad Request (RFC 7807).
/// </summary>
public sealed class DomainException(string message) : Exception(message);
