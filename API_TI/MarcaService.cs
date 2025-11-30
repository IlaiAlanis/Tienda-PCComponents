using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.dbModels;

using API_TI.Models.DTOs.MarcaDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Helpers;
using API_TI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Services.Implementations
{
    public class MarcaService : BaseService, IMarcaService
    {
        private readonly TiPcComponentsContext _context;

        public MarcaService(
            TiPcComponentsContext context, 
            IErrorService errorService, 
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<MarcaDto>>> GetAllAsync()
        {
            var marcas = await _context.Marcas
                .AsNoTracking()
                .ToListAsync();

            var dtoList = Mapper.ToMarcaDto(marcas);

            return ApiResponse<List<MarcaDto>>.Ok(dtoList, "Lista de errores obtenidos correctamente");
        }

        public async Task<ApiResponse<MarcaDto>> GetByIdAsync(int id)
        {
            var marca = await _context.Marcas
                .FindAsync(id);

            if (marca == null)
                return await ReturnErrorAsync<MarcaDto>(450);

            var dtoList = Mapper.ToMarcaDto(marca);

            return ApiResponse<MarcaDto>.Ok(dtoList, "Marca obtenido correctamente");
        }

        public async Task<ApiResponse<MarcaDto>> CreateAsync(CreateMarcaDto dto)
        {
            var existMarca = await _context.Marcas.AnyAsync(c => c.NombreMarca == dto.Nombre);

            if (existMarca)
                return await ReturnErrorAsync<MarcaDto>(451);

            var marca = new Marca
            {
                NombreMarca = dto.Nombre,
                Descripcion = dto.Descripcion,
                EstaActivo = true
            };

            await using var txr = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Marcas.Add(marca);
                await _context.SaveChangesAsync();
                await txr.CommitAsync();
            }
            catch (Exception ex)
            {
                await txr.RollbackAsync();
                //log DB
                return await ReturnErrorAsync<MarcaDto>(2107, $"Error al crear marca: {ex.Message}");
            }

            var dtoList = Mapper.ToMarcaDto(marca);

            return await GetByIdAsync(marca.IdMarca);
        }

        public async Task<ApiResponse<MarcaDto>> UpdateAsync(UpdateMarcaDto dto)
        {
            var marca = await _context.Marcas
                .FindAsync(dto.IdMarca);

            if (marca == null)
                return await ReturnErrorAsync<MarcaDto>(2400, $"Error la marca {dto.Nombre} no fue encontrado");

            marca.NombreMarca = dto.Nombre ?? marca.NombreMarca;
            marca.Descripcion = dto.Descripcion ?? marca.Descripcion;
            marca.FechaActualizacion = DateTime.UtcNow;

            await using var txr = await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.SaveChangesAsync();
                await txr.CommitAsync();
            }
            catch (Exception ex)
            {
                await txr.RollbackAsync();
                return await ReturnErrorAsync<MarcaDto>(2108, $"Error al actualizar marca: {ex.Message}");
            }

            var dtoList = Mapper.ToMarcaDto(marca);

            return await GetByIdAsync(marca.IdMarca);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var marca = await _context.Marcas
                .FindAsync(id);

            if (marca == null)
                return await ReturnErrorAsync<bool>(2400, $"Error el descuento con id: {id} no fue encontrado");

            marca.EstaActivo = false;
            marca.FechaActualizacion = DateTime.UtcNow;

            await using var txr = await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.SaveChangesAsync();
                await txr.CommitAsync();
            }
            catch (Exception ex)
            {
                await txr.RollbackAsync();
                return await ReturnErrorAsync<bool>(2109, $"Error al eliminar la marca: {ex.Message}");
            }

            return ApiResponse<bool>.Ok(true, "Marca eliminado correctamente");
        }
    }
}
