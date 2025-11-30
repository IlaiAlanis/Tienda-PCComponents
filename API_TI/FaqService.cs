using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.dbModels;
using API_TI.Models.DTOs.FaqDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Services.Implementations
{
    public class FaqService : BaseService, IFaqService
    {
        private readonly TiPcComponentsContext _context;

        public FaqService(
            TiPcComponentsContext context,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<FaqDto>>> GetAllFaqsAsync()
        {
            try
            {
                var faqs = await _context.Faqs
                    .Where(f => f.EstaActivo)
                    .OrderBy(f => f.Orden)
                    .ThenBy(f => f.Categoria)
                    .ToListAsync();

                var dtos = faqs.Select(f => MapToDto(f)).ToList();
                return ApiResponse<List<FaqDto>>.Ok(dtos);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<List<FaqDto>>(9000);
            }
        }

        public async Task<ApiResponse<FaqDto>> GetFaqByIdAsync(int id)
        {
            try
            {
                var faq = await _context.Faqs.FindAsync(id);

                if (faq == null)
                    return await ReturnErrorAsync<FaqDto>(900, "FAQ no encontrado");

                var dto = MapToDto(faq);
                return ApiResponse<FaqDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<FaqDto>(9000);
            }
        }

        public async Task<ApiResponse<FaqDto>> CreateFaqAsync(CreateFaqRequest request)
        {
            try
            {
                var faq = new Faq
                {
                    Pregunta = request.Pregunta,
                    Respuesta = request.Respuesta,
                    Categoria = request.Categoria,
                    Orden = request.Orden ?? 0,
                    EstaActivo = request.EstaActivo,
                    FechaCreacion = DateTime.UtcNow
                };

                _context.Faqs.Add(faq);
                await _context.SaveChangesAsync();

                var dto = MapToDto(faq);
                return ApiResponse<FaqDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<FaqDto>(9000);
            }
        }

        public async Task<ApiResponse<FaqDto>> UpdateFaqAsync(int id, UpdateFaqRequest request)
        {
            try
            {
                var faq = await _context.Faqs.FindAsync(id);

                if (faq == null)
                    return await ReturnErrorAsync<FaqDto>(900, "FAQ no encontrado");

                faq.Pregunta = request.Pregunta;
                faq.Respuesta = request.Respuesta;
                faq.Categoria = request.Categoria;
                faq.Orden = request.Orden ?? faq.Orden;
                faq.EstaActivo = request.EstaActivo;
                faq.FechaModificacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var dto = MapToDto(faq);
                return ApiResponse<FaqDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<FaqDto>(9000);
            }
        }

        public async Task<ApiResponse<object>> DeleteFaqAsync(int id)
        {
            try
            {
                var faq = await _context.Faqs.FindAsync(id);

                if (faq == null)
                    return await ReturnErrorAsync<object>(900, "FAQ no encontrado");

                // Soft delete
                faq.EstaActivo = false;
                faq.FechaModificacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse<object>.Ok(null, "FAQ eliminado correctamente");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<object>(9000);
            }
        }

        private FaqDto MapToDto(Faq faq)
        {
            return new FaqDto
            {
                IdFaq = faq.IdFaq,
                Pregunta = faq.Pregunta,
                Respuesta = faq.Respuesta,
                Categoria = faq.Categoria,
                Orden = faq.Orden,
                EstaActivo = faq.EstaActivo
            };
        }
    }
}
