using API_TI.Models.DTOs.CarritoDTOs;
using API_TI.Models.DTOs.ProductoDTOs;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API_TI.Swagger
{
    /// <summary>
    /// Provides examples for DTOs in Swagger
    /// </summary>
    public class SwaggerExampleFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            // ===== AUTHENTICATION =====
            if (context.Type.Name == "LoginRequestDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["correo"] = new OpenApiString("admin@pccomponents.com"),
                    ["password"] = new OpenApiString("Admin123!")
                };
            }
            else if (context.Type.Name == "RegisterRequestDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["nombreUsuario"] = new OpenApiString("John Doe"),
                    ["correo"] = new OpenApiString("john.doe@example.com"),
                    ["password"] = new OpenApiString("MyPassword123!"),
                    ["apellidoPaterno"] = new OpenApiString("Doe"),
                    ["apellidoMaterno"] = new OpenApiString("Smith")
                };
            }

            // ===== CART =====
            else if (context.Type == typeof(AddToCartRequest))
            {
                schema.Example = new OpenApiObject
                {
                    ["productoId"] = new OpenApiInteger(1),
                    ["cantidad"] = new OpenApiInteger(2)
                };
            }
            else if (context.Type == typeof(ApplyCouponDto))
            {
                schema.Example = new OpenApiObject
                {
                    ["codigoCupon"] = new OpenApiString("SUMMER2025")
                };
            }

            // ===== PRODUCTS =====
            else if (context.Type.Name == "CreateProductoDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["nombreProducto"] = new OpenApiString("NVIDIA RTX 4090"),
                    ["categoriaId"] = new OpenApiInteger(1),
                    ["marcaId"] = new OpenApiInteger(1),
                    ["sku"] = new OpenApiString("GPU-NV-4090-24GB"),
                    ["precioBase"] = new OpenApiDouble(24999.99),
                    ["cantidadStock"] = new OpenApiInteger(10),
                    ["descripcion"] = new OpenApiString("Latest generation graphics card with 24GB GDDR6X"),
                    ["estaActivo"] = new OpenApiBoolean(true)
                };
            }
            else if (context.Type.Name == "ProductoSearchRequest")
            {
                schema.Example = new OpenApiObject
                {
                    ["searchTerm"] = new OpenApiString("RTX"),
                    ["categoriaId"] = new OpenApiInteger(1),
                    ["minPrice"] = new OpenApiDouble(10000),
                    ["maxPrice"] = new OpenApiDouble(30000),
                    ["enDescuento"] = new OpenApiBoolean(false),
                    ["page"] = new OpenApiInteger(1),
                    ["pageSize"] = new OpenApiInteger(10)
                };
            }

            // ===== ADDRESSES =====
            else if (context.Type.Name == "CreateDireccionDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["nombreCompleto"] = new OpenApiString("John Doe Smith"),
                    ["telefono"] = new OpenApiString("8112345678"),
                    ["calle"] = new OpenApiString("Constitution Ave"),
                    ["numeroExterior"] = new OpenApiString("1234"),
                    ["numeroInterior"] = new OpenApiString("3B"),
                    ["colonia"] = new OpenApiString("Downtown"),
                    ["ciudad"] = new OpenApiString("Monterrey"),
                    ["estado"] = new OpenApiString("Nuevo Leon"),
                    ["codigoPostal"] = new OpenApiString("64000"),
                    ["pais"] = new OpenApiString("Mexico"),
                    ["referencias"] = new OpenApiString("Between Main St and Park Ave"),
                    ["esPredeterminada"] = new OpenApiBoolean(true)
                };
            }

            // ===== ORDERS =====
            else if (context.Type.Name == "CreateOrderDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["direccionEnvioId"] = new OpenApiInteger(1),
                    ["metodoPagoId"] = new OpenApiInteger(1),
                    ["notasOrden"] = new OpenApiString("Deliver before 5pm")
                };
            }

            // ===== REVIEWS =====
            else if (context.Type.Name == "CreateReviewDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["productoId"] = new OpenApiInteger(1),
                    ["calificacion"] = new OpenApiInteger(5),
                    ["titulo"] = new OpenApiString("Excellent product"),
                    ["comentario"] = new OpenApiString("Best graphics card I've ever bought. Exceptional performance.")
                };
            }

            // ===== DISCOUNTS =====
            else if (context.Type.Name == "CreateDescuentoDto")
            {
                schema.Example = new OpenApiObject
                {
                    ["codigo"] = new OpenApiString("SUMMER2025"),
                    ["tipo"] = new OpenApiString("Percentage"),
                    ["valor"] = new OpenApiDouble(15),
                    ["limiteUsos"] = new OpenApiInteger(100),
                    ["fechaInicio"] = new OpenApiString("2025-06-01"),
                    ["fechaFin"] = new OpenApiString("2025-08-31"),
                    ["montoMinimo"] = new OpenApiDouble(1000),
                    ["descripcion"] = new OpenApiString("Summer discount"),
                    ["estaActivo"] = new OpenApiBoolean(true)
                };
            }
        }
    }
}