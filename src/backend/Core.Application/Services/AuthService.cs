using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Domain.Entities;
using FluentValidation;

namespace Core.Application.Services;

public sealed class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IValidator<LoginRequest> validator) : IAuthService
{
    private const string InvalidCredentials = "Usuario o clave incorrectos.";

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var login = request.Username.Contains('@')
            ? request.Username.Trim().ToLowerInvariant()
            : User.NormalizeUsername(request.Username);

        var user = await userRepository.GetByUsernameOrEmailAsync(login, cancellationToken);
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException(InvalidCredentials);
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Este usuario está inactivo.");
        }

        var token = tokenService.GenerateToken(user);

        return new AuthResponse(
            token.Token,
            "Bearer",
            token.ExpiresAt,
            user.Username,
            user.Email,
            user.FullName,
            user.Role.ToString());
    }
}
