using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Application.Mappings;
using Core.Domain.Entities;
using FluentValidation;

namespace Core.Application.Services;

public sealed class UserService(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    IAuditService audit,
    ICurrentUser currentUser,
    IValidator<CreateUserRequest> createValidator,
    IValidator<UpdateUserRequest> updateValidator,
    IValidator<ResetPasswordRequest> resetValidator,
    IValidator<ChangePasswordRequest> changeValidator) : IUserService
{
    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await users.GetAllAsync(cancellationToken);
        return list.Select(u => u.ToResponse()).ToList();
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        string username = User.NormalizeUsername(request.Username);
        if (await users.UsernameOrEmailExistsAsync(username, request.Email.Trim().ToLowerInvariant(), null, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe un usuario con ese username o email.");
        }

        var user = new User(username, request.Email, request.FullName, passwordHasher.Hash(request.Password), request.Role,
            request.AttendantName, request.CanChangeAttendant);
        users.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await audit.AuditAsync("user_created", "users", $"Usuario creado: {user.Username} ({user.Role})",
            nameof(User), user.Id.ToString(), after: user.ToResponse(), cancellationToken: cancellationToken);
        return user.ToResponse();
    }

    public async Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        var user = await users.GetForUpdateAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe el usuario {id}.");
        if (await users.UsernameOrEmailExistsAsync(user.Username, request.Email.Trim().ToLowerInvariant(), id, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe otro usuario con ese email.");
        }

        if (id == currentUser.UserId && (!request.IsActive || request.Role != user.Role))
        {
            throw new InvalidOperationException("No puede desactivarse ni cambiar su propio rol.");
        }

        var before = user.ToResponse();
        user.UpdateProfile(request.Email, request.FullName, request.Role, request.AttendantName, request.CanChangeAttendant);
        if (request.IsActive)
        {
            user.Activate();
        }
        else
        {
            user.Deactivate();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await audit.AuditAsync("user_updated", "users", $"Usuario actualizado: {user.Username}",
            nameof(User), user.Id.ToString(), before: before, after: user.ToResponse(), cancellationToken: cancellationToken);
        return user.ToResponse();
    }

    public async Task ResetPasswordAsync(Guid id, ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        await resetValidator.ValidateAndThrowAsync(request, cancellationToken);
        var user = await users.GetForUpdateAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe el usuario {id}.");

        user.ChangePasswordHash(passwordHasher.Hash(request.NewPassword));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await audit.AuditAsync("user_password_reset", "users", $"Clave restablecida: {user.Username}",
            nameof(User), user.Id.ToString(), cancellationToken: cancellationToken);
    }

    public async Task ChangeOwnPasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        await changeValidator.ValidateAndThrowAsync(request, cancellationToken);
        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException("Se requiere un usuario autenticado.");
        var user = await users.GetForUpdateAsync(userId, cancellationToken)
            ?? throw new UnauthorizedAccessException("El usuario del token ya no existe.");

        if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("La clave actual no es correcta.");
        }

        user.ChangePasswordHash(passwordHasher.Hash(request.NewPassword));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await audit.AuditAsync("user_password_changed", "users", $"Cambio de clave propia: {user.Username}",
            nameof(User), user.Id.ToString(), cancellationToken: cancellationToken);
    }
}
