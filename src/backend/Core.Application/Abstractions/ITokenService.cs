using Core.Domain.Entities;

namespace Core.Application.Abstractions;

public sealed record AccessToken(string Token, DateTime ExpiresAt);

public interface ITokenService
{
    AccessToken GenerateToken(User user);
}
