using Core.Application.DTOs;

namespace Core.Application.Abstractions;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
