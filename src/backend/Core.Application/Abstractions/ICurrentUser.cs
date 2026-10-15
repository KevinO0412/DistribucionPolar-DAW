using System.Net;

namespace Core.Application.Abstractions;

/// <summary>Usuario autenticado de la petición en curso (leído del JWT).</summary>
public interface ICurrentUser
{
    Guid? UserId { get; }

    string? Username { get; }

    string? Role { get; }

    bool IsAdmin { get; }

    IPAddress? IpAddress { get; }
}
