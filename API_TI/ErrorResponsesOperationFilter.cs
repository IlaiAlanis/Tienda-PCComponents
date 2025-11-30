using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API_TI.Swagger
{
    /// <summary>
    /// Adds standard error responses to all endpoints
    /// </summary>
    public class ErrorResponsesOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Responses ??= new OpenApiResponses();

            // Check if authentication is required
            var hasAuthorize = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                .Union(context.MethodInfo.GetCustomAttributes(true))
                .Any(attr => attr.GetType().Name == "AuthorizeAttribute") ?? false;

            // ===== COMMON ERROR RESPONSES =====

            if (!operation.Responses.ContainsKey("400"))
            {
                operation.Responses["400"] = new OpenApiResponse
                {
                    Description = "❌ Bad Request - The sent data doesn't meet the required format",
                    Content = BuildErrorResponse(2, "The provided data is invalid", 2, new[]
                    {
                        "Verify that all required fields are present",
                        "Ensure data types are correct",
                        "Check the format of emails, phone numbers, etc."
                    })
                };
            }

            if (hasAuthorize)
            {
                operation.Responses["401"] = new OpenApiResponse
                {
                    Description = "🔒 Unauthorized - Authentication required",
                    Content = BuildErrorResponse(100, "Authentication required. Please log in.", 3, new[]
                    {
                        "Use the /api/auth/login endpoint to get a token",
                        "Include the token in the header: Authorization: Bearer {token}",
                        "Verify that the token hasn't expired"
                    })
                };

                operation.Responses["403"] = new OpenApiResponse
                {
                    Description = "⛔ Forbidden - Insufficient permissions",
                    Content = BuildErrorResponse(101, "You don't have permission to perform this action", 3, new[]
                    {
                        "This endpoint requires administrator permissions",
                        "Contact the system administrator if you need access"
                    })
                };
            }

            if (!operation.Responses.ContainsKey("404"))
            {
                operation.Responses["404"] = new OpenApiResponse
                {
                    Description = "🔍 Not Found - The requested resource doesn't exist",
                    Content = BuildErrorResponse(300, "The requested resource was not found", 2, new[]
                    {
                        "Verify that the provided ID is correct",
                        "The resource may have been deleted"
                    })
                };
            }

            if (!operation.Responses.ContainsKey("500"))
            {
                operation.Responses["500"] = new OpenApiResponse
                {
                    Description = "💥 Internal Server Error",
                    Content = BuildErrorResponse(9000, "An unexpected error occurred on the server", 4, new[]
                    {
                        "This error has been logged automatically",
                        "Contact technical support if the problem persists"
                    })
                };
            }

            // ===== SUCCESS RESPONSE (IF DOESN'T EXIST) =====
            if (!operation.Responses.ContainsKey("200") && !operation.Responses.ContainsKey("201"))
            {
                operation.Responses["200"] = new OpenApiResponse
                {
                    Description = "✅ Successful Operation",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Example = new OpenApiObject
                            {
                                ["success"] = new OpenApiBoolean(true),
                                ["data"] = new OpenApiObject
                                {
                                    ["message"] = new OpenApiString("Operation completed successfully")
                                }
                            }
                        }
                    }
                };
            }
        }

        private Dictionary<string, OpenApiMediaType> BuildErrorResponse(
            int code,
            string message,
            int severity,
            string[]? suggestions = null)
        {
            var errorObject = new OpenApiObject
            {
                ["success"] = new OpenApiBoolean(false),
                ["data"] = new OpenApiNull(),
                ["message"] = new OpenApiNull(),
                ["error"] = new OpenApiObject
                {
                    ["code"] = new OpenApiInteger(code),
                    ["message"] = new OpenApiString(message),
                    ["severity"] = new OpenApiInteger(severity)
                }
            };

            if (suggestions != null && suggestions.Length > 0)
            {
                var suggestionsArray = new OpenApiArray();
                foreach (var suggestion in suggestions)
                {
                    suggestionsArray.Add(new OpenApiString(suggestion));
                }
                ((OpenApiObject)errorObject["error"]).Add("suggestions", suggestionsArray);
            }

            return new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Example = errorObject
                }
            };
        }
    }
}