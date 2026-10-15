using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Application.Services;
using Core.Application.Validators;
using Core.Domain.Entities;
using Core.Domain.Enums;
using FluentValidation;

namespace UnitTests.Application;

public sealed class AuthServiceTests
{
    private static readonly DateTime ExpiresAt = new(2026, 10, 1, 1, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IUserRepository> mockUsers = new();
    private readonly Mock<IPasswordHasher> mockHasher = new();
    private readonly Mock<ITokenService> mockTokens = new();
    private readonly AuthService service;
    private readonly User admin = new("admin", "admin@distribucionpolar.local", "Administrador", "HASH-ADMIN", UserRole.Admin);

    public AuthServiceTests()
    {
        service = new AuthService(mockUsers.Object, mockHasher.Object, mockTokens.Object, new LoginRequestValidator());

        mockHasher.Setup(h => h.Verify("Admin2026!", "HASH-ADMIN")).Returns(true);
        mockTokens.Setup(t => t.GenerateToken(It.IsAny<User>()))
            .Returns<User>(u => new AccessToken($"token-{u.Username}", ExpiresAt));
    }

    [Fact]
    public async Task LoginAsync_WithUsername_NormalizesAndReturnsTokenAndRole()
    {
        mockUsers.Setup(r => r.GetByUsernameOrEmailAsync("admin", It.IsAny<CancellationToken>())).ReturnsAsync(admin);

        var response = await service.LoginAsync(new LoginRequest(" ADMIN ", "Admin2026!"));

        Assert.Equal("token-admin", response.Token);
        Assert.Equal("Bearer", response.TokenType);
        Assert.Equal("Admin", response.Role);
        Assert.Equal(ExpiresAt, response.ExpiresAt);
        mockTokens.Verify(t => t.GenerateToken(admin), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithEmail_LowercasesBeforeQuerying()
    {
        mockUsers.Setup(r => r.GetByUsernameOrEmailAsync("admin@distribucionpolar.local", It.IsAny<CancellationToken>())).ReturnsAsync(admin);

        var response = await service.LoginAsync(new LoginRequest("Admin@DistribucionPolar.local", "Admin2026!"));

        Assert.Equal("admin", response.Username);
        mockUsers.Verify(r => r.GetByUsernameOrEmailAsync("admin@distribucionpolar.local", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_IsUnauthorized_AndIssuesNoToken()
    {
        mockUsers.Setup(r => r.GetByUsernameOrEmailAsync("admin", It.IsAny<CancellationToken>())).ReturnsAsync(admin);

        var error = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(new LoginRequest("admin", "otraclave")));

        Assert.Equal("Usuario o clave incorrectos.", error.Message);
        mockTokens.Verify(t => t.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithUnknownUser_GivesTheSameGenericMessage()
    {
        mockUsers.Setup(r => r.GetByUsernameOrEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var error = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(new LoginRequest("nadie", "Clave2026!")));

        Assert.Equal("Usuario o clave incorrectos.", error.Message);
        mockHasher.Verify(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_IsUnauthorized()
    {
        var user = new User("inactivo", "inactivo@distribucionpolar.local", "Inactivo", "HASH-INACTIVO", UserRole.Employee);
        user.Deactivate();
        mockUsers.Setup(r => r.GetByUsernameOrEmailAsync("inactivo", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        mockHasher.Setup(h => h.Verify("Inactivo2026!", "HASH-INACTIVO")).Returns(true);

        var error = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(new LoginRequest("inactivo", "Inactivo2026!")));

        Assert.Equal("Este usuario está inactivo.", error.Message);
        mockTokens.Verify(t => t.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithShortPassword_ThrowsValidation_BeforeQuerying()
    {
        await Assert.ThrowsAsync<ValidationException>(() => service.LoginAsync(new LoginRequest("admin", "123")));
        mockUsers.Verify(r => r.GetByUsernameOrEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
