using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.dbModels;
using API_TI.Models.DTOs.CategoriaDTOs;
using API_TI.Models.DTOs.DescuentoDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Helpers;
using API_TI.Services.Interfaces;
using Azure.Core;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Services.Implementations
{
    public class CategoriaService : BaseService, ICategoriaService
    {
        private readonly TiPcComponentsContext _context;

        public CategoriaService(
            TiPcComponentsContext context, 
            IErrorService errorService, 
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<CategoriaDto>>> GetAllAsync()
        {
            var categorias = await _context.Categoria
                .AsNoTracking()
                .Where(x => x.EstaActivo == true)
                .ToListAsync();

            var dtoList = Mapper.ToCategoriaDto(categorias);

            return ApiResponse<List<CategoriaDto>>.Ok(dtoList, "Lista de categorias obtenidas correctamente");
        }

        public async Task<ApiResponse<CategoriaDto>> GetByIdAsync(int id)
        {
            var categoria = await _context.Categoria
                .FindAsync(id);

            if (categoria == null)
                return await ReturnErrorAsync<CategoriaDto>(400);

            var dtoList = Mapper.ToCategoriaDto(categoria);

            return ApiResponse<CategoriaDto>.Ok(dtoList, "Categoría obtenida correctamente");
        }

        public async Task<ApiResponse<CategoriaDto>> CreateAsync(CreateCategoriaDto dto)
        {
            var existCategoria = await _context.Categoria.AnyAsync(c => c.NombreCategoria == dto.Nombre);

            if (existCategoria)
                return await ReturnErrorAsync<CategoriaDto>(401);

            var categoria = new Categorium
            {
                NombreCategoria = dto.Nombre,
                Descripcion = dto.Descripcion,
                EstaActivo = true
            };

            await using var txr = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Categoria.Add(categoria);
                await _context.SaveChangesAsync();
                await txr.CommitAsync();
            }
            catch (Exception ex)
            {
                await txr.RollbackAsync();
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<CategoriaDto>(9000);
            }

            var dtoList = Mapper.ToCategoriaDto(categoria);

            return await GetByIdAsync(categoria.IdCategoria);
        }

        public async Task<ApiResponse<CategoriaDto>> UpdateAsync(UpdateCategoriaDto dto)
        {
            var categoria = await _context.Categoria
                .FindAsync(dto.IdCategoria);

            if (categoria == null)
                return await ReturnErrorAsync<CategoriaDto>(2200, $"Error la categoría con id: {dto.IdCategoria} no fue encontrada");

            categoria.NombreCategoria = dto.Nombre ?? categoria.NombreCategoria;
            categoria.Descripcion = dto.Descripcion ?? categoria.Descripcion;
            categoria.FechaActualizacion = DateTime.UtcNow;

            await using var txr = await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.SaveChangesAsync();
                await txr.CommitAsync();
            }
            catch (Exception ex)
            {
                await txr.RollbackAsync();
                return await ReturnErrorAsync<CategoriaDto>(2108, $"Error al actualizar descuento: {ex.Message}");
            }

            var dtoList = Mapper.ToCategoriaDto(categoria);

            return await GetByIdAsync(categoria.IdCategoria);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var categoria = await _context.Categoria
                .FindAsync(id);

            if (categoria == null)
                return await ReturnErrorAsync<bool>(2200, $"Error la categoría con id: {id} no fue encontrado");

            
            categoria.EstaActivo = false;
            categoria.FechaActualizacion = DateTime.UtcNow;

            await using var txr = await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.SaveChangesAsync();
                await txr.CommitAsync();
            }
            catch (Exception ex)
            {
                await txr.RollbackAsync();
                return await ReturnErrorAsync<bool>(2109, $"Error al eliminar la cateegoría: {ex.Message}");
            }

            return ApiResponse<bool>.Ok(true, "Categoría eliminado correctamente.");
        }
    }
}
