using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.ProductoDTOs;
using API_TI.Services;
using API_TI.Services.Implementations;
using API_TI.Services.Interfaces;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API_TI.Controllers
{
    /// <summary>
    /// Gestión de productos
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductoController : BaseApiController
    {
        private readonly IProductoService _productoService;
        private readonly IProductoImagenService _imagenService;
        //private readonly ICloudinaryService _cloudinaryService;
        private readonly TiPcComponentsContext _context;
        private readonly LocalImageService _imageService;
        public ProductoController(
            IProductoService productoService,
            IProductoImagenService imagenService,
            TiPcComponentsContext context,
            //ICloudinaryService cloudinaryService,
            LocalImageService imageService

        )
        {
            _productoService = productoService;
            _imagenService = imagenService;
            _context = context;
            //_cloudinaryService = cloudinaryService;
            _imageService = imageService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        [HttpPost("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromBody] ProductoSearchRequest request)
        {
            var response = await _productoService.SearchAsync(request);
            return FromApiResponse(response);
        }

        [HttpGet("featured")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFeatured()
        {
            var response = await _productoService.GetFeaturedAsync();
            return FromApiResponse(response);
        }

        /// <summary>
        /// Obtiene todos los productos activos
        /// </summary>
        /// <returns>Lista de productos</returns>

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var response = await _productoService.GetAllAsync();
            return FromApiResponse(response);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _productoService.GetByIdAsync(id);
            return FromApiResponse(response);
        }

        [HttpPost]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Create([FromBody] CreateProductoDto dto)
        {
            if (!ModelState.IsValid)
                return FromApiResponse(ApiResponse<ProductoDto>.Fail(2, "Datos inválidos", 2));

            var response = await _productoService.CreateAsync(dto, GetUserId());
            return FromApiResponse(response);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductoDto dto)
        {
            if (!ModelState.IsValid)
                return FromApiResponse(ApiResponse<ProductoDto>.Fail(2, "Datos inválidos", 2));

            var response = await _productoService.UpdateAsync(id, dto, GetUserId());
            return FromApiResponse(response);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _productoService.DeleteAsync(id, GetUserId());
            return FromApiResponse(response);
        }

        


        [HttpGet("{id}/stock")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckStock(int id, [FromQuery] int cantidad = 1)
        {
            var response = await _productoService.CheckStockAsync(id, cantidad);
            return FromApiResponse(response);
        }


        // Variations
        [HttpGet("variations/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVariation(int id)
        {
            var response = await _productoService.GetByIdVariationAsync(id);
            return FromApiResponse(response);
        }

        [HttpPost("variations")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> CreateVariation([FromBody] ProductoVariacionCreateDto dto)
        {
            var response = await _productoService.CreateVariationAsync(dto, GetUserId());
            return FromApiResponse(response);
        }

        [HttpPut("variations/{id}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> UpdateVariation(int id, [FromBody] ProductoVariacionCreateDto dto)
        {
            var response = await _productoService.UpdateVariationAsync(id, dto, GetUserId());
            return FromApiResponse(response);
        }

        [HttpDelete("variations/{id}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> DeleteVariation(int id)
        {
            var response = await _productoService.DeleteVariationAsync(id, GetUserId());
            return FromApiResponse(response);
        }


        // Images
        //[HttpGet("{id}/images")]
        //public async Task<IActionResult> GetImages(int id)
        //{
        //    var response = await _imagenService.GetProductImagesAsync(id);
        //    return FromApiResponse(response);
        //}

        //[HttpPost("{id}/images")]
        //[Authorize(Policy = "RequireAdmin")]
        //[RequestSizeLimit(5 * 1024 * 1024)] // 5MB
        //public async Task<IActionResult> UploadImage(int id, IFormFile file)
        //{
        //    if (file == null || file.Length == 0)
        //        return FromApiResponse(ApiResponse<ProductoImagenDto>.Fail(2, "Archivo requerido", 2));

        //    var response = await _imagenService.UploadImageAsync(id, file, GetUserId());
        //    return FromApiResponse(response);
        //}


        //[HttpDelete("images/{imagenId}")]
        //[Authorize(Policy = "RequireAdmin")]
        //public async Task<IActionResult> DeleteImage(int imagenId)
        //{
        //    var response = await _imagenService.DeleteImageAsync(imagenId, GetUserId());
        //    return FromApiResponse(response);
        //}

        //[HttpPut("images/{imagenId}/set-primary")]
        //[Authorize(Policy = "RequireAdmin")]
        //public async Task<IActionResult> SetPrimaryImage(int imagenId)
        //{
        //    var response = await _imagenService.SetPrimaryImageAsync(imagenId, GetUserId());
        //    return FromApiResponse(response);
        //}

        [HttpGet("{id}/images")]
        [AllowAnonymous]
        public async Task<IActionResult> GetImages(int id)
       => FromApiResponse(await _imagenService.GetProductImagesAsync(id));

        [HttpPost("{id}/images")]
        [Authorize(Policy = "RequireAdmin")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadImage(int id, IFormFile file)
            => FromApiResponse(await _imagenService.UploadImageAsync(id, file, GetUserId()));

        [HttpDelete("images/{imagenId}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> DeleteImage(int imagenId)
            => FromApiResponse(await _imagenService.DeleteImageAsync(imagenId, GetUserId()));

        [HttpPut("images/{imagenId}/set-primary")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> SetPrimaryImage(int imagenId)
            => FromApiResponse(await _imagenService.SetPrimaryImageAsync(imagenId, GetUserId()));
    }
}
