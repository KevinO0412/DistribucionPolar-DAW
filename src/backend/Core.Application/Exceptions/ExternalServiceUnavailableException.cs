namespace Core.Application.Exceptions;

/// <summary>Un servicio externo no responde y no hay un valor de respaldo. La API responde 503.</summary>
public sealed class ExternalServiceUnavailableException(string message, Exception? innerException = null)
    : Exception(message, innerException);
