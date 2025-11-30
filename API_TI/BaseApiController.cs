using API_TI.Models.ApiResponse;
using API_TI.Services.Helpers;
using Microsoft.AspNetCore.Mvc;
using static API_TI.Services.Helpers.MapperError;

namespace API_TI.Controllers
{
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        protected IActionResult FromApiResponse<T>(ApiResponse<T> response)
        {
            if (response.Success)
                return Ok(response);

            // unified error object required
            var err = response.Error ?? new ApiError { Code = 9999, Message = "Error desconocido", Severity = 3 };


            // map your internal code -> ApiErrorType/HTTP
            var resultType = MapperError.MapperErrors(err.Code, err.Severity);

            return resultType switch
            {
                MapperError.ApiErrorType.Ok => Ok(response),
                MapperError.ApiErrorType.BadRequest => BadRequest(response),
                MapperError.ApiErrorType.Unauthorized => Unauthorized(response),
                MapperError.ApiErrorType.Forbid => StatusCode(StatusCodes.Status403Forbidden, response),
                MapperError.ApiErrorType.NotFound => NotFound(response),
                MapperError.ApiErrorType.Conflict => Conflict(response),
                MapperError.ApiErrorType.PaymentRequired => StatusCode(StatusCodes.Status402PaymentRequired, response),
                MapperError.ApiErrorType.ServerError => StatusCode(StatusCodes.Status500InternalServerError, response),
                _ => StatusCode(StatusCodes.Status500InternalServerError, response),
            };
        }
    }
}
