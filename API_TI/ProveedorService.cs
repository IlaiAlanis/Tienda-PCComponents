using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.dbModels;
using API_TI.Models.DTOs.ProveedorDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Helpers;
using API_TI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Services.Implementations
{
    public class ProveedorService : BaseService, IProveedorService
    {
        private readonly TiPcComponentsContext _context;

        public ProveedorService(
            TiPcComponentsContext context,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<ProveedorDto>>> GetAllAsync()
        {
            try
            {
                var proveedores = await _context.Proveedors
                    .Where(p => p.EstaActivo)
                    .OrderBy(p => p.NombreProveedor)
                    .ToListAsync();

                var dtoList = Mapper.ToProveedorDto(proveedores);

                return ApiResponse<List<ProveedorDto>>.Ok(dtoList);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<List<ProveedorDto>>(9000);
            }
        }

        public async Task<ApiResponse<ProveedorDto>> GetByIdAsync(int id)
        {
            try
            {
                var proveedor = await _context.Proveedors.FindAsync(id);
                if (proveedor == null)
                    return await ReturnErrorAsync<ProveedorDto>(500);
                
                var dtoList = Mapper.ToProveedorDto(proveedor);

                return ApiResponse<ProveedorDto>.Ok(dtoList);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<ProveedorDto>(9000);
            }
        }

        public async Task<ApiResponse<ProveedorDto>> CreateAsync(CreateProveedorRequest request)
        {
            try
            {
                if (await _context.Proveedors.AnyAsync(p => p.Correo == request.Correo))
                    return await ReturnErrorAsync<ProveedorDto>(501, "Correo ya registrado");

                var proveedor = new Proveedor
                {
                    PaisId = request.PaisId,
                    EstadoId = request.EstadoId,
                    CiudadId = request.CiudadId,
                    CodigoPostal = request.CodigoPostal,
                    NombreProveedor = request.NombreProveedor,
                    NombreContacto = request.NombreContacto,
                    Telefono = request.Telefono,
                    Correo = request.Correo,
                    Direccion = request.Direccion,
                    EstaActivo = true,
                    FechaCreacion = DateTime.UtcNow
                };

                _context.Proveedors.Add(proveedor);
                await _context.SaveChangesAsync();

                await AuditAsync("Proveedor.Created", new { ProveedorId = proveedor.IdProveedor });

                var dtoList = Mapper.ToProveedorDto(proveedor);

                return ApiResponse<ProveedorDto>.Ok(dtoList, "Proveedor creado");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<ProveedorDto>(9000);
            }
        }

        public async Task<ApiResponse<ProveedorDto>> UpdateAsync(int id, CreateProveedorRequest request)
        {
            try
            {
                var proveedor = await _context.Proveedors.FindAsync(id);
                if (proveedor == null)
                    return await ReturnErrorAsync<ProveedorDto>(500);

                if (await _context.Proveedors.AnyAsync(p => p.Correo == request.Correo && p.IdProveedor != id))
                    return await ReturnErrorAsync<ProveedorDto>(501, "Correo ya registrado");

                proveedor.PaisId = request.PaisId;
                proveedor.EstadoId = request.EstadoId;
                proveedor.CiudadId = request.CiudadId;
                proveedor.CodigoPostal = request.CodigoPostal;
                proveedor.NombreProveedor = request.NombreProveedor;
                proveedor.NombreContacto = request.NombreContacto;
                proveedor.Telefono = request.Telefono;
                proveedor.Correo = request.Correo;
                proveedor.Direccion = request.Direccion;
                proveedor.FechaActualizacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await AuditAsync("Proveedor.Updated", new { ProveedorId = id });

                var dtoList = Mapper.ToProveedorDto(proveedor);

                return ApiResponse<ProveedorDto>.Ok(dtoList, "Proveedor actualizado");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<ProveedorDto>(9000);
            }
        }

        public async Task<ApiResponse<object>> DeleteAsync(int id)
        {
            try
            {
                var proveedor = await _context.Proveedors.FindAsync(id);
                if (proveedor == null)
                    return await ReturnErrorAsync<object>(500);

                // Check if supplier has associated products
                var hasProducts = await _context.Productos.AnyAsync(p => p.ProveedorId == id);
               
                if (hasProducts)
                    return await ReturnErrorAsync<object>(502, "No se puede eliminar: el proveedor tiene productos asociados");

                proveedor.EstaActivo = false;
                proveedor.FechaActualizacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                await AuditAsync("Proveedor.Deleted", new { ProveedorId = id });
                return ApiResponse<object>.Ok(null, "Proveedor desactivado");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<object>(9000);
            }
        }

        
    }
}
