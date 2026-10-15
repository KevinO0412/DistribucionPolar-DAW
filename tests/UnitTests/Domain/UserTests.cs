using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Exceptions;

namespace UnitTests.Domain;

public sealed class UserTests
{
    [Fact]
    public void Constructor_NormalizesUsernameAndEmail()
    {
        var user = new User(" Ana Gomez ", "Ana.Gomez@DistribucionPolar.Local", "Ana Gomez", "hash", UserRole.Employee);

        Assert.Equal("anagomez", user.Username);
        Assert.Equal("ana.gomez@distribucionpolar.local", user.Email);
        Assert.True(user.IsActive);
    }

    [Fact]
    public void Deactivate_MarksUserAsInactive()
    {
        var user = new User("admin", "admin@distribucionpolar.local", "Admin", "hash", UserRole.Admin);

        user.Deactivate();

        Assert.False(user.IsActive);
    }

    [Fact]
    public void Constructor_RejectsUnknownRole()
    {
        Assert.Throws<DomainException>(() => new User("x", "x@x.com", "X", "hash", (UserRole)99));
    }
}
