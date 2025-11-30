using API_TI.Data;
using API_TI.Models.ApiResponse;
using API_TI.Models.dbModels;
using API_TI.Models.DTOs.ProductoDTOs;
using API_TI.Services.Abstract;
using API_TI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Services.Implementations
{
    public class ProductoImagenService : BaseService, IProductoImagenService
    {
        private readonly TiPcComponentsContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _config;
        private readonly ILogger<ProductoImagenService> _logger;

        public ProductoImagenService(
            TiPcComponentsContext context,
            IWebHostEnvironment environment,
            IConfiguration config,
            ILogger<ProductoImagenService> logger,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
            _environment = environment;
            _config = config;
            _logger = logger;
        }

        public async Task<ApiResponse<List<ProductoImagenDto>>> GetProductImagesAsync(int productoId)
        {
            try
            {
                var images = await _context.ProductoImagens
                    .Where(i => i.ProductoId == productoId)
                    .OrderByDescending(i => i.EsPrincipal)
                    .ThenBy(i => i.Orden)
                    .ToListAsync();

                var dtos = images.Select(i => new ProductoImagenDto
                {
                    IdImagen = i.IdImagen,
                    UrlImagen = i.UrlImagen,
                    EsPrincipal = i.EsPrincipal,
                    Orden = i.Orden
                }).ToList();

                return ApiResponse<List<ProductoImagenDto>>.Ok(dtos);
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<List<ProductoImagenDto>>(9000);
            }
        }

        public async Task<ApiResponse<ProductoImagenDto>> UploadImageAsync(int productoId, IFormFile file, int usuarioId)
        {
            try
            {
                if (!await _context.Productos.AnyAsync(p => p.IdProducto == productoId))
                    return await ReturnErrorAsync<ProductoImagenDto>(300);

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                    return await ReturnErrorAsync<ProductoImagenDto>(2, "Formato de imagen inválido");

                if (file.Length > 5 * 1024 * 1024) // 5MB
                    return await ReturnErrorAsync<ProductoImagenDto>(2, "Imagen muy grande (max 5MB)");

                var url = await SaveImageAsync(file, productoId);

                var isFirstImage = !await _context.ProductoImagens.AnyAsync(i => i.ProductoId == productoId);
                var maxOrden = await _context.ProductoImagens
                    .Where(i => i.ProductoId == productoId)
                    .MaxAsync(i => (int?)i.Orden) ?? 0;

                var imagen = new ProductoImagen
                {
                    ProductoId = productoId,
                    UrlImagen = url,
                    EsPrincipal = isFirstImage,
                    Orden = maxOrden + 1
                };

                _context.ProductoImagens.Add(imagen);
                await _context.SaveChangesAsync();

                await AuditAsync("Product.ImageUploaded", new { ProductoId = productoId, ImagenId = imagen.IdImagen }, usuarioId);

                return ApiResponse<ProductoImagenDto>.Ok(new ProductoImagenDto
                {
                    IdImagen = imagen.IdImagen,
                    UrlImagen = imagen.UrlImagen,
                    EsPrincipal = imagen.EsPrincipal,
                    Orden = imagen.Orden
                });
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<ProductoImagenDto>(9000);
            }
        }

        public async Task<ApiResponse<object>> DeleteImageAsync(int imagenId, int usuarioId)
        {
            try
            {
                var imagen = await _context.ProductoImagens.FindAsync(imagenId);
                if (imagen == null)
                    return await ReturnErrorAsync<object>(5);

                await DeleteImageFileAsync(imagen.UrlImagen);

                _context.ProductoImagens.Remove(imagen);
                await _context.SaveChangesAsync();

                await AuditAsync("Product.ImageDeleted", new { ImagenId = imagenId }, usuarioId);

                return ApiResponse<object>.Ok(null, "Imagen eliminada");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<object>(9000);
            }
        }

        public async Task<ApiResponse<object>> SetPrimaryImageAsync(int imagenId, int usuarioId)
        {
            try
            {
                var imagen = await _context.ProductoImagens.FindAsync(imagenId);
                if (imagen == null)
                    return await ReturnErrorAsync<object>(5);

                var currentPrimary = await _context.ProductoImagens
                    .FirstOrDefaultAsync(i => i.ProductoId == imagen.ProductoId && i.EsPrincipal);

                if (currentPrimary != null)
                    currentPrimary.EsPrincipal = false;

                imagen.EsPrincipal = true;
                await _context.SaveChangesAsync();

                return ApiResponse<object>.Ok(null, "Imagen principal actualizada");
            }
            catch (Exception ex)
            {
                await LogTechnicalErrorAsync(9000, ex);
                return await ReturnErrorAsync<object>(9000);
            }
        }

        private async Task<string> SaveImageAsync(IFormFile file, int productoId)
        {
            var storageType = _config["Storage:Type"]; // "Local" or "Azure" or "S3"

            return storageType?.ToLower() switch
            {
                "azure" => await SaveToAzureBlobAsync(file, productoId),
                "s3" => await SaveToS3Async(file, productoId),
                _ => await SaveToLocalAsync(file, productoId)
            };
        }

        private async Task<string> SaveToLocalAsync(IFormFile file, int productoId)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "products");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{productoId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/products/{fileName}";
        }

        private async Task<string> SaveToAzureBlobAsync(IFormFile file, int productoId)
        {
            // Azure Blob Storage implementation
            // var connectionString = _config["Storage:Azure:ConnectionString"];
            // var containerName = _config["Storage:Azure:ContainerName"];
            // var blobServiceClient = new BlobServiceClient(connectionString);
            // var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            // var fileName = $"products/{productoId}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            // var blobClient = containerClient.GetBlobClient(fileName);
            // await blobClient.UploadAsync(file.OpenReadStream(), overwrite: true);
            // return blobClient.Uri.ToString();

            throw new NotImplementedException("Configure Azure Blob Storage");
        }

        private async Task<string> SaveToS3Async(IFormFile file, int productoId)
        {
            // AWS S3 implementation
            // var accessKey = _config["Storage:S3:AccessKey"];
            // var secretKey = _config["Storage:S3:SecretKey"];
            // var bucketName = _config["Storage:S3:BucketName"];
            // var region = RegionEndpoint.GetBySystemName(_config["Storage:S3:Region"]);
            // var s3Client = new AmazonS3Client(accessKey, secretKey, region);
            // var fileName = $"products/{productoId}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            // var request = new PutObjectRequest
            // {
            //     BucketName = bucketName,
            //     Key = fileName,
            //     InputStream = file.OpenReadStream()
            // };
            // await s3Client.PutObjectAsync(request);
            // return $"https://{bucketName}.s3.{_config["Storage:S3:Region"]}.amazonaws.com/{fileName}";

            throw new NotImplementedException("Configure AWS S3");
        }

        private async Task DeleteImageFileAsync(string url)
        {
            try
            {
                var storageType = _config["Storage:Type"];

                if (storageType?.ToLower() == "local")
                {
                    var filePath = Path.Combine(_environment.WebRootPath, url.TrimStart('/'));
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }
                // Add Azure/S3 deletion logic
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete image file: {Url}", url);
            }
        }
    }
}
