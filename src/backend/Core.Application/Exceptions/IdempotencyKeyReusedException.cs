namespace Core.Application.Exceptions;

/// <summary>
/// La clave Idempotency-Key ya se usó con otro contenido. Reintentar con la misma clave solo es válido
/// si la petición es idéntica; la API responde 422.
/// </summary>
public sealed class IdempotencyKeyReusedException(string message) : Exception(message);
