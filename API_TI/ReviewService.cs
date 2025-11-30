using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.dbModels;
using API_TI.Models.DTOs.AdminDTOs;
using API_TI.Models.DTOs.ReviewDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Helpers;
using API_TI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace API_TI.Services.Implementations
{
    public class ReviewService : BaseService, IReviewService
    {
        private readonly TiPcComponentsContext _context;

        public ReviewService(
            TiPcComponentsContext context,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
        }

        public async Task<ApiResponse<PagedResult<ReviewDto>>> GetProductReviewsAsync(int productoId, int page, int pageSize)
        {
            try
            {
                var query = _context.ProductoResenas
                    .Include(r => r.Usuario)
                    .Where(r => r.ProductoId == productoId)
                    .OrderByDescending(r => r.FechaCreacion);

                var total = await query.CountAsync();
                var reviews = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var dtoList = Mapper.ToReviewDto(reviews);

                var result = new PagedResult<ReviewDto>(
                    dtoList,
                    total,
                    page,
                    pageSize
                );

                return ApiResponse<PagedResult<ReviewDto>>.Ok(result);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<PagedResult<ReviewDto>>(9000);
            }
        }

        public async Task<ApiResponse<ReviewDto>> CreateReviewAsync(CreateReviewRequest request, int usuarioId)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(request.ProductoId);
                if (producto == null)
                    return await ReturnErrorAsync<ReviewDto>(300);

                // Verify user purchased product
                var hasPurchased = await _context.OrdenItems
                    .AnyAsync(oi => oi.Orden.UsuarioId == usuarioId &&
                                   oi.ProductoId == request.ProductoId &&
                                   oi.Orden.EstatusVentaId >= 4); // Paid orders

                if (!hasPurchased)
                    return await ReturnErrorAsync<ReviewDto>(5, "Debe comprar el producto para dejar una reseña");

                // Check if already reviewed
                if (await _context.ProductoResenas.AnyAsync(r =>
                    r.ProductoId == request.ProductoId && r.UsuarioId == usuarioId))
                    return await ReturnErrorAsync<ReviewDto>(6, "Ya has dejado una reseña para este producto");

                var review = new ProductoResena
                {
                    ProductoId = request.ProductoId,
                    UsuarioId = usuarioId,
                    Calificacion = request.Calificacion,
                    Comentario = request.Comentario,
                    //Verificado = true,
                    FechaCreacion = DateTime.UtcNow
                };

                _context.ProductoResenas.Add(review);
                await _context.SaveChangesAsync();

                // Update product average rating
                await UpdateProductRatingAsync(request.ProductoId);

                await AuditAsync("Review.Created", new { ReviewId = review.IdResena, ProductoId = request.ProductoId }, usuarioId);

                // Reload with user info
                await _context.Entry(review).Reference(r => r.Usuario).LoadAsync();

                var dtoList = Mapper.ToReviewDto(review);

                return ApiResponse<ReviewDto>.Ok(dtoList, "Reseña creada exitosamente");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<ReviewDto>(9000);
            }
        }

        public async Task<ApiResponse<ReviewDto>> UpdateReviewAsync(int reviewId, CreateReviewRequest request, int usuarioId)
        {
            try
            {
                var review = await _context.ProductoResenas
                    .Include(r => r.Usuario)
                    .FirstOrDefaultAsync(r => r.IdResena == reviewId);

                if (review == null)
                    return await ReturnErrorAsync<ReviewDto>(5, "Reseña no encontrada");

                if (review.UsuarioId != usuarioId)
                    return await ReturnErrorAsync<ReviewDto>(101);

                review.Calificacion = request.Calificacion;
                review.Comentario = request.Comentario;
                review.FechaCreacion = DateTime.UtcNow; // Update timestamp

                await _context.SaveChangesAsync();
                await UpdateProductRatingAsync(review.ProductoId);

                await AuditAsync("Review.Updated", new { ReviewId = reviewId }, usuarioId);

                var dtoList = Mapper.ToReviewDto(review);

                return ApiResponse<ReviewDto>.Ok( dtoList, "Reseña actualizada");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<ReviewDto>(9000);
            }
        }

        public async Task<ApiResponse<object>> DeleteReviewAsync(int reviewId, int usuarioId)
        {
            try
            {
                var review = await _context.ProductoResenas.FindAsync(reviewId);
                if (review == null)
                    return await ReturnErrorAsync<object>(5, "Reseña no encontrada");

                if (review.UsuarioId != usuarioId)
                    return await ReturnErrorAsync<object>(101);

                var productoId = review.ProductoId;
                _context.ProductoResenas.Remove(review);
                await _context.SaveChangesAsync();

                await UpdateProductRatingAsync(productoId);
                await AuditAsync("Review.Deleted", new { ReviewId = reviewId }, usuarioId);

                return ApiResponse<object>.Ok(null, "Reseña eliminada");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<object>(9000);
            }
        }

        private async Task UpdateProductRatingAsync(int productoId)
        {
            var reviews = await _context.ProductoResenas
                .Where(r => r.ProductoId == productoId)
                .ToListAsync();

            if (reviews.Any())
            {
                var avgRating = reviews.Average(r => r.Calificacion);
                // Store in producto table if you add CalificacionPromedio column
                // producto.CalificacionPromedio = (decimal)avgRating;
            }
        }

       
    }
}