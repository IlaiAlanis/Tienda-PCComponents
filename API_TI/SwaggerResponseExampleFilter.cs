using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API_TI.Swagger
{
    /// <summary>
    /// Adds successful response examples to endpoints
    /// </summary>
    public class SwaggerResponseExampleFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var actionName = context.MethodInfo.Name.ToLower();
            var controllerName = context.MethodInfo.DeclaringType?.Name.Replace("Controller", "").ToLower() ?? "";

            // Customize responses based on endpoint
            if (controllerName == "auth")
            {
                if (actionName.Contains("login"))
                {
                    AddSuccessExample(operation, new OpenApiObject
                    {
                        ["success"] = new OpenApiBoolean(true),
                        ["data"] = new OpenApiObject
                        {
                            ["token"] = new OpenApiString("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."),
                            ["user"] = new OpenApiObject
                            {
                                ["idUsuario"] = new OpenApiInteger(1),
                                ["nombreUsuario"] = new OpenApiString("Administrator"),
                                ["correo"] = new OpenApiString("admin@pccomponents.com"),
                                ["rol"] = new OpenApiString("Admin")
                            }
                        }
                    });
                }
                else if (actionName.Contains("register"))
                {
                    AddSuccessExample(operation, new OpenApiObject
                    {
                        ["success"] = new OpenApiBoolean(true),
                        ["data"] = new OpenApiObject
                        {
                            ["message"] = new OpenApiString("User registered successfully. Check your email to verify your account.")
                        }
                    });
                }
            }
            else if (controllerName == "producto")
            {
                if (actionName.Contains("getall") || actionName.Contains("search"))
                {
                    AddSuccessExample(operation, new OpenApiObject
                    {
                        ["success"] = new OpenApiBoolean(true),
                        ["data"] = new OpenApiArray
                        {
                            new OpenApiObject
                            {
                                ["idProducto"] = new OpenApiInteger(1),
                                ["nombreProducto"] = new OpenApiString("NVIDIA RTX 4090"),
                                ["precioBase"] = new OpenApiDouble(24999.99),
                                ["precioPromocional"] = new OpenApiDouble(22999.99),
                                ["cantidadStock"] = new OpenApiInteger(10),
                                ["categoria"] = new OpenApiString("Graphics Cards"),
                                ["marca"] = new OpenApiString("NVIDIA"),
                                ["imagenes"] = new OpenApiArray
                                {
                                    new OpenApiString("https://example.com/img1.jpg")
                                }
                            }
                        }
                    });
                }
            }
        }

        private void AddSuccessExample(OpenApiOperation operation, OpenApiObject example)
        {
            if (operation.Responses.ContainsKey("200"))
            {
                var response = operation.Responses["200"];
                if (response.Content.ContainsKey("application/json"))
                {
                    response.Content["application/json"].Example = example;
                }
            }
        }
    }
}