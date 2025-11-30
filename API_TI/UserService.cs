using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.UsuarioDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Services.Implementations
{
    public class UserService : BaseService, IUserService
    {
        private readonly TiPcComponentsContext _context;

        public UserService(
            TiPcComponentsContext context,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
        }

        public async Task<ApiResponse<UserProfileDto>> GetProfileAsync(int userId)
        {
            try
            {
                var user = await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.IdUsuario == userId);

                if (user == null)
                    return await ReturnErrorAsync<UserProfileDto>(200, "Usuario no encontrado");

                var profile = new UserProfileDto
                {
                    IdUsuario = user.IdUsuario,
                    NombreUsuario = user.NombreUsuario,
                    ApellidoPaterno = user.ApellidoPaterno ?? string.Empty,
                    ApellidoMaterno = user.ApellidoMaterno,
                    Correo = user.Correo,
                    CorreoVerificado = user.CorreoVerificado,
                    FechaNacimiento = user.FechaNacimiento,
                    Rol = user.Rol?.NombreRol,
                    FechaCreacion = user.FechaCreacion,
                    UltimoLogin = user.UltimoLoginUsuario,
                    AvatarUrl = null // Add avatar URL logic if available
                };

                return ApiResponse<UserProfileDto>.Ok(profile);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<UserProfileDto>(9000);
            }
        }

        public async Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(int userId, UpdateProfileRequest request)
        {
            try
            {
                var user = await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.IdUsuario == userId);

                if (user == null)
                    return await ReturnErrorAsync<UserProfileDto>(200, "Usuario no encontrado");

                // Handle password change if provided
                if (!string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    // Validate current password is provided
                    if (string.IsNullOrWhiteSpace(request.CurrentPassword))
                        return await ReturnErrorAsync<UserProfileDto>(3, "Se requiere la contraseña actual para cambiarla");

                    // Verify current password
                    if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.ContrasenaHash))
                        return await ReturnErrorAsync<UserProfileDto>(203, "Contraseña actual incorrecta");

                    // Validate new password length
                    if (request.NewPassword.Length < 8)
                        return await ReturnErrorAsync<UserProfileDto>(3, "La nueva contraseña debe tener al menos 8 caracteres");

                    // Ensure new password is different
                    if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.ContrasenaHash))
                        return await ReturnErrorAsync<UserProfileDto>(5, "La nueva contraseña debe ser diferente a la actual");

                    // Hash and update password
                    user.ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, 12);
                }

                // Update profile fields
                if (!string.IsNullOrWhiteSpace(request.Nombre))
                    user.NombreUsuario = request.Nombre.Trim();

                if (!string.IsNullOrWhiteSpace(request.ApellidoPaterno))
                    user.ApellidoPaterno = request.ApellidoPaterno.Trim();

                if (!string.IsNullOrWhiteSpace(request.ApellidoMaterno))
                    user.ApellidoMaterno = request.ApellidoMaterno.Trim();

                if (request.FechaNacimiento.HasValue)
                    user.FechaNacimiento = request.FechaNacimiento;

                // Handle email change
                if (!string.IsNullOrWhiteSpace(request.Correo) && request.Correo != user.Correo)
                {
                    // Check if email already exists
                    var emailExists = await _context.Usuarios
                        .AnyAsync(u => u.Correo == request.Correo && u.IdUsuario != userId);

                    if (emailExists)
                        return await ReturnErrorAsync<UserProfileDto>(201, "El email ya está registrado");

                    user.Correo = request.Correo;
                    user.CorreoVerificado = false; // Require re-verification
                }

                user.FechaActualizacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                await AuditAsync("User.Profile.Updated", new { UserId = userId });

                var profile = new UserProfileDto
                {
                    IdUsuario = user.IdUsuario,
                    NombreUsuario = user.NombreUsuario,
                    ApellidoPaterno = user.ApellidoPaterno ?? string.Empty,
                    ApellidoMaterno = user.ApellidoMaterno,
                    Correo = user.Correo,
                    CorreoVerificado = user.CorreoVerificado,
                    FechaNacimiento = user.FechaNacimiento,
                    Rol = user.Rol?.NombreRol,
                    FechaCreacion = user.FechaCreacion,
                    UltimoLogin = user.UltimoLoginUsuario
                };

                return ApiResponse<UserProfileDto>.Ok(profile, "Perfil actualizado exitosamente");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<UserProfileDto>(9000);
            }
        }

        public async Task<ApiResponse<object>> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            try
            {
                var user = await _context.Usuarios.FindAsync(userId);
                if (user == null)
                    return await ReturnErrorAsync<object>(200, "Usuario no encontrado");

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.ContrasenaHash))
                    return await ReturnErrorAsync<object>(203, "Contraseña actual incorrecta");

                // Validate new password
                if (request.NewPassword.Length < 8)
                    return await ReturnErrorAsync<object>(3, "La contraseña debe tener al menos 8 caracteres");

                if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.ContrasenaHash))
                    return await ReturnErrorAsync<object>(5, "La nueva contraseña debe ser diferente a la actual");

                // Hash new password
                user.ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, 12);
                user.FechaActualizacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await AuditAsync("User.Password.Changed", new { UserId = userId });

                return ApiResponse<object>.Ok(null, "Contraseña actualizada exitosamente");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<object>(9000);
            }
        }

        public async Task<ApiResponse<object>> UpdateEmailAsync(int userId, UpdateEmailRequest request)
        {
            try
            {
                var user = await _context.Usuarios.FindAsync(userId);
                if (user == null)
                    return await ReturnErrorAsync<object>(200, "Usuario no encontrado");

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.ContrasenaHash))
                    return await ReturnErrorAsync<object>(203, "Contraseña incorrecta");

                // Check if email already exists
                var exists = await _context.Usuarios
                    .AnyAsync(u => u.Correo == request.NewEmail && u.IdUsuario != userId);

                if (exists)
                    return await ReturnErrorAsync<object>(201, "El email ya está registrado");

                user.Correo = request.NewEmail;
                user.CorreoVerificado = false; // Require re-verification
                user.FechaActualizacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await AuditAsync("User.Email.Changed", new
                {
                    UserId = userId,
                    NewEmail = request.NewEmail
                });

                return ApiResponse<object>.Ok(null, "Email actualizado. Verifica tu nuevo correo");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<object>(9000);
            }
        }

        public async Task<ApiResponse<object>> DeleteAccountAsync(int userId, string password)
        {
            try
            {
                var user = await _context.Usuarios.FindAsync(userId);
                if (user == null)
                    return await ReturnErrorAsync<object>(200, "Usuario no encontrado");

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(password, user.ContrasenaHash))
                    return await ReturnErrorAsync<object>(203, "Contraseña incorrecta");

                // Soft delete
                user.EstaActivo = false;
                user.FechaActualizacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await AuditAsync("User.Account.Deleted", new { UserId = userId });

                return ApiResponse<object>.Ok(null, "Cuenta eliminada exitosamente");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<object>(9000);
            }
        }
    }
}