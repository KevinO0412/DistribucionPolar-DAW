using System.Net;
using Core.Application.Abstractions;

namespace Presentation.API.Security;

/// <summary>Usuario de la petición en curso, leído de los claims del JWT.</summary>
public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private HttpContext? Context => accessor.HttpContext;

    public Guid? UserId => Guid.TryParse(Context?.User.FindFirst("sub")?.Value, out var id) ? id : null;

    public string? Username => Context?.User.Identity?.IsAuthenticated == true ? Context.User.Identity.Name : null;

    public string? Role => Context?.User.FindFirst("role")?.Value;

    public bool IsAdmin => Role == Roles.Admin;

    public IPAddress? IpAddress => Context?.Connection.RemoteIpAddress is { } ip && ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4() : Context?.Connection.RemoteIpAddress;
}
