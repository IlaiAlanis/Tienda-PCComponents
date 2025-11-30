using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.dbModels;
using API_TI.Models.DTOs.AdminDTOs;
using API_TI.Models.DTOs.DescuentoDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Helpers;
using API_TI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace API_TI.Services.Implementations
{
    public class DescuentoAdminService : BaseService, IDescuentoAdminService
    {
        private readonly TiPcComponentsContext _context;

        public DescuentoAdminService(
            TiPcComponentsContext context,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
        }



        public async Task<ApiResponse<PagedResult<DescuentoDto>>> GetAllDiscountsAsync(
            string? search = null,
            string? status = null,
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                // Base query with includes
                var query = _context.Descuentos
                    .Include(d => d.ReglaDescuentos)
                    .Include(d => d.DescuentoAlcances)
                    .AsQueryable();

                // FILTER BY SEARCH (nombre_descuento or codigo_cupon)
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    query = query.Where(d =>
                        d.NombreDescuento.ToLower().Contains(search) ||
                        (d.Descripcion != null && d.Descripcion.ToLower().Contains(search)) ||
                        d.ReglaDescuentos.Any(r => r.CodigoCupon != null && r.CodigoCupon.ToLower().Contains(search))
                    );
                }

                // FILTER BY STATUS
                if (!string.IsNullOrWhiteSpace(status))
                {
                    var now = DateTime.UtcNow;

                    switch (status.ToLower())
                    {
                        case "active":
                            // Active: esta_activo = true AND (fecha_fin is null OR fecha_fin >= now) AND (limite_usos is null OR usos < limite)
                            query = query.Where(d =>
                                d.EstaActivo &&
                                d.ReglaDescuentos.Any(r =>
                                    r.EstaActivo &&
                                    (r.FechaFin == null || r.FechaFin >= now) &&
                                    (!r.LimiteUsosTotal.HasValue || r.UsosActuales < r.LimiteUsosTotal.Value)
                                )
                            );
                            break;

                        case "expired":
                            // Expired: fecha_fin < now
                            query = query.Where(d =>
                                d.ReglaDescuentos.Any(r => r.FechaFin != null && r.FechaFin < now)
                            );
                            break;

                        case "inactive":
                            // Inactive: esta_activo = false
                            query = query.Where(d => !d.EstaActivo);
                            break;

                        case "limit_reached":
                            // Limit reached: usos_actuales >= limite_usos_total
                            query = query.Where(d =>
                                d.ReglaDescuentos.Any(r =>
                                    r.LimiteUsosTotal.HasValue &&
                                    r.UsosActuales >= r.LimiteUsosTotal.Value
                                )
                            );
                            break;
                    }
                }

                // Get total count BEFORE pagination
                var totalCount = await query.CountAsync();

                // Apply pagination and ordering
                var descuentos = await query
                    .OrderByDescending(d => d.FechaCreacion)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Map to DTOs
                var dtos = Mapper.ToDescuentoDto(descuentos);

                // Create paged result
                var pagedResult = new PagedResult<DescuentoDto>(dtos, totalCount, page, pageSize);

                return ApiResponse<PagedResult<DescuentoDto>>.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<PagedResult<DescuentoDto>>(9000);
            }
        }

        public async Task<ApiResponse<DescuentoDetailDto>> GetDiscountByIdAsync(int id)
        {
            try
            {
                var descuento = await _context.Descuentos
                    .Include(d => d.ReglaDescuentos)
                    .Include(d => d.DescuentoAlcances)
                    .FirstOrDefaultAsync(d => d.IdDescuento == id);

                if (descuento == null)
                    return await ReturnErrorAsync<DescuentoDetailDto>(850, "Descuento no encontrado");

                var dto = Mapper.ToDescuentodetailsDto(descuento);
                return ApiResponse<DescuentoDetailDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<DescuentoDetailDto>(9000);
            }
        }

        public async Task<ApiResponse<DescuentoDto>> CreateDiscountAsync(CreateDescuentoRequest request, int adminId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate coupon code if provided
                if (!string.IsNullOrWhiteSpace(request.CodigoCupon))
                {
                    var exists = await _context.ReglaDescuentos
                        .AnyAsync(r => r.CodigoCupon == request.CodigoCupon.ToUpper());

                    if (exists)
                        return await ReturnErrorAsync<DescuentoDto>(854, "Código de cupón ya existe");
                }

                // Validate dates
                if (request.FechaInicio >= request.FechaFin)
                    return await ReturnErrorAsync<DescuentoDto>(860, "Fecha de inicio debe ser menor a fecha fin");

                // Create discount
                var descuento = new Descuento
                {
                    NombreDescuento = request.NombreDescuento,
                    Descripcion = request.Descripcion,
                    TipoDescuento = request.TipoDescuento,
                    Prioridad = request.Prioridad ?? 1,
                    EstaActivo = request.EstaActivo,
                    FechaCreacion = DateTime.UtcNow,
                    //CreadoPor = adminId
                };

                _context.Descuentos.Add(descuento);
                await _context.SaveChangesAsync();

                // Create discount rule
                var regla = new ReglaDescuento
                {
                    DescuentoId = descuento.IdDescuento,
                    CodigoCupon = request.CodigoCupon?.ToUpper(),
                    Valor = request.Valor,
                    MontoMinimoCompra = request.MontoMinimo,
                    LimiteUsosTotal = request.LimiteUsosTotal,
                    LimiteUsosPorUsuario = request.LimiteUsosPorUsuario,
                    SoloNuevosUsuarios = request.SoloNuevosUsuarios,
                    FechaInicio = request.FechaInicio,
                    FechaFin = request.FechaFin,
                    UsosActuales = 0,
                    EstaActivo = true
                };

                _context.ReglaDescuentos.Add(regla);

                // Create scope (default to GLOBAL if not specified)
                var alcance = new DescuentoAlcance
                {
                    DescuentoId = descuento.IdDescuento,
                    TipoEntidad = request.TipoEntidad ?? "GLOBAL",
                    EntidadId = request.EntidadId,
                    Incluir = true
                };

                _context.DescuentoAlcances.Add(alcance);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                await AuditAsync("DescuentoAdmin.IdDescuento.Created", new { DescuentoId = descuento.IdDescuento }, adminId);


                var dto = Mapper.ToDescuentoDto(descuento);
                return ApiResponse<DescuentoDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<DescuentoDto>(9000);
            }
        }

        public async Task<ApiResponse<DescuentoDto>> UpdateDiscountAsync(int id, UpdateDescuentoRequest request, int adminId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var descuento = await _context.Descuentos
                    .Include(d => d.ReglaDescuentos)
                    .FirstOrDefaultAsync(d => d.IdDescuento == id);

                if (descuento == null)
                    return await ReturnErrorAsync<DescuentoDto>(850, "Descuento no encontrado");

                // Update discount
                descuento.NombreDescuento = request.NombreDescuento;
                descuento.Descripcion = request.Descripcion;
                descuento.TipoDescuento = request.TipoDescuento;
                descuento.EstaActivo = request.EstaActivo;
                //descuento.FechaModificacion = DateTime.UtcNow;
                //descuento.ModificadoPor = adminId;

                // Update rule
                var regla = descuento.ReglaDescuentos.FirstOrDefault();
                if (regla != null)
                {
                    regla.Valor = request.Valor;
                    regla.MontoMinimoCompra = request.MontoMinimo;
                    regla.LimiteUsosTotal = request.LimiteUsosTotal;
                    regla.LimiteUsosPorUsuario = request.LimiteUsosPorUsuario;
                    regla.FechaInicio = request.FechaInicio;
                    regla.FechaFin = request.FechaFin;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await AuditAsync("DescuentoAdmin.IdDescuento.Updated", new { DescuentoId = descuento.IdDescuento }, adminId);

                var dto = Mapper.ToDescuentoDto(descuento);
                return ApiResponse<DescuentoDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<DescuentoDto>(9000);
            }
        }

        public async Task<ApiResponse<object>> DeleteDiscountAsync(int id, int adminId)
        {
            try
            {
                var descuento = await _context.Descuentos.FindAsync(id);

                if (descuento == null)
                    return await ReturnErrorAsync<object>(850, "Descuento no encontrado");

                // Soft delete
                descuento.EstaActivo = false;
                //descuento.FechaModificacion = DateTime.UtcNow;
                //descuento.ModificadoPor = adminId;

                await _context.SaveChangesAsync();

                await AuditAsync("DescuentoAdmin.IdDescuento.Deleted", new { DescuentoId = descuento.IdDescuento }, adminId);

                return ApiResponse<object>.Ok(null, "Descuento eliminado correctamente");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<object>(9000);
            }
        }

        public async Task<ApiResponse<object>> ValidateCouponAsync(string codigo, int usuarioId)
        {
            try
            {
                var regla = await _context.ReglaDescuentos
                    .Include(r => r.Descuento)
                    .FirstOrDefaultAsync(r => r.CodigoCupon == codigo.ToUpper() && r.EstaActivo);

                if (regla == null)
                    return await ReturnErrorAsync<object>(851, "Cupón inválido");

                var now = DateTime.UtcNow;
                if (regla.FechaInicio > now || regla.FechaFin < now)
                    return await ReturnErrorAsync<object>(852, "Cupón expirado");

                // Check total uses
                if (regla.LimiteUsosTotal.HasValue && regla.UsosActuales >= regla.LimiteUsosTotal)
                    return await ReturnErrorAsync<object>(855, "Cupón agotado");

                // Check user uses
                if (usuarioId > 0 && regla.LimiteUsosPorUsuario.HasValue)
                {
                    var userUses = await _context.DescuentoUsos
                        .CountAsync(u => u.DescuentoId == regla.DescuentoId && u.UsuarioId == usuarioId);

                    if (userUses >= regla.LimiteUsosPorUsuario)
                        return await ReturnErrorAsync<object>(862, "Ya usaste este cupón");
                }

                // Check new user requirement
                if (usuarioId > 0 && regla.SoloNuevosUsuarios)
                {
                    var hasOrders = await _context.Ordens.AnyAsync(o => o.UsuarioId == usuarioId);
                    if (hasOrders)
                        return await ReturnErrorAsync<object>(866, "Cupón solo para nuevos usuarios");
                }

                return ApiResponse<object>.Ok(new
                {
                    Valid = true,
                    Descuento = regla.Descuento.NombreDescuento,
                    Tipo = regla.Descuento.TipoDescuento,
                    Valor = regla.Valor
                });
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<object>(9000);
            }
        }
    }
}
