using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Application.Services;
using Core.Application.Validators;
using Core.Domain.Entities;
using Core.Domain.Enums;

namespace UnitTests.Application;

public sealed class UserAndEmployeeServiceTests
{
    private readonly Mock<IUserRepository> mockUsers = new();
    private readonly Mock<IEmployeeRepository> mockEmployees = new();
    private readonly Mock<IPasswordHasher> mockHasher = new();
    private readonly Mock<IUnitOfWork> mockUnitOfWork = TestDoubles.UnitOfWork();
    private readonly Mock<IAuditService> mockAudit = new();
    private readonly Mock<ICurrentUser> mockCurrentUser = new();
    private readonly User admin = new("admin", "admin@distribucionpolar.local", "Administrador", "HASH-ADMIN", UserRole.Admin);

    public UserAndEmployeeServiceTests()
    {
        mockHasher.Setup(h => h.Hash(It.IsAny<string>())).Returns<string>(p => $"HASH-{p}");
        mockCurrentUser.Setup(u => u.UserId).Returns(admin.Id);
        mockUsers.Setup(r => r.GetForUpdateAsync(admin.Id, It.IsAny<CancellationToken>())).ReturnsAsync(admin);
    }

    private UserService Users() => new(
        mockUsers.Object, mockHasher.Object, mockUnitOfWork.Object, mockAudit.Object, mockCurrentUser.Object,
        new CreateUserRequestValidator(), new UpdateUserRequestValidator(), new ResetPasswordRequestValidator(), new ChangePasswordRequestValidator());

    private EmployeeService Employees() => new(
        mockEmployees.Object, mockUnitOfWork.Object, mockAudit.Object, new CreateEmployeeRequestValidator(), new UpdateEmployeeRequestValidator());

    [Fact]
    public async Task CreateUser_HashesThePassword_AndNormalizesTheUsername()
    {
        User? added = null;
        mockUsers.Setup(r => r.Add(It.IsAny<User>())).Callback<User>(u => added = u);

        var response = await Users().CreateAsync(new CreateUserRequest(" Vendedor 2 ", "v2@distribucionpolar.local", "Vendedor Dos", "Clave2026!", UserRole.Employee, "Ever Mendoza", false));

        Assert.Equal("vendedor2", response.Username);
        Assert.Equal("HASH-Clave2026!", added!.PasswordHash);
        mockAudit.Verify(a => a.RecordAsync(It.Is<AuditEntry>(e => e.Action == "user_created"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateUser_WithTakenUsername_IsRejected()
    {
        mockUsers.Setup(r => r.UsernameOrEmailExistsAsync("admin", It.IsAny<string>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Users().CreateAsync(new CreateUserRequest("admin", "otro@distribucionpolar.local", "Otro", "Clave2026!", UserRole.Admin, null, false)));
        mockUsers.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUser_AdminCannotDeactivateThemselves()
    {
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Users().UpdateAsync(admin.Id, new UpdateUserRequest(admin.Email, admin.FullName, UserRole.Admin, false, null, true)));

        Assert.Equal("No puede desactivarse ni cambiar su propio rol.", error.Message);
        Assert.True(admin.IsActive);
    }

    [Fact]
    public async Task ChangeOwnPassword_WithWrongCurrentPassword_IsRejected()
    {
        mockHasher.Setup(h => h.Verify("mala-clave", "HASH-ADMIN")).Returns(false);

        await Assert.ThrowsAsync<InvalidOperationException>(() => Users().ChangeOwnPasswordAsync(new ChangePasswordRequest("mala-clave", "Nueva2026!")));
        Assert.Equal("HASH-ADMIN", admin.PasswordHash);
    }

    [Fact]
    public async Task ChangeOwnPassword_StoresTheNewHash()
    {
        mockHasher.Setup(h => h.Verify("Admin2026!", "HASH-ADMIN")).Returns(true);

        await Users().ChangeOwnPasswordAsync(new ChangePasswordRequest("Admin2026!", "Nueva2026!"));

        Assert.Equal("HASH-Nueva2026!", admin.PasswordHash);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_UnknownUser_ThrowsKeyNotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Users().ResetPasswordAsync(Guid.NewGuid(), new ResetPasswordRequest("Nueva2026!")));
    }

    [Fact]
    public async Task CreateEmployee_UsesTheDefaultRole()
    {
        var response = await Employees().CreateAsync(new CreateEmployeeRequest("Dilene Vera", null));

        Assert.Equal(("Dilene Vera", "Empleado", true), (response.Name, response.Role, response.IsActive));
        mockEmployees.Verify(r => r.Add(It.IsAny<Employee>()), Times.Once);
    }

    [Fact]
    public async Task UpdateEmployee_Deactivates_InsteadOfDeleting()
    {
        var employee = new Employee("Ender Contreras", "Vendedor");
        mockEmployees.Setup(r => r.GetForUpdateAsync(employee.Id, It.IsAny<CancellationToken>())).ReturnsAsync(employee);

        var response = await Employees().UpdateAsync(employee.Id, new UpdateEmployeeRequest("Ender Contreras", "Vendedor", false));

        Assert.False(response.IsActive);
        mockAudit.Verify(a => a.RecordAsync(It.Is<AuditEntry>(e => e.Action == "employee_updated"), It.IsAny<CancellationToken>()), Times.Once);
    }
}
