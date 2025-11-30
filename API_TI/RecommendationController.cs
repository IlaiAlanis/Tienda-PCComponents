using API_TI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API_TI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationController : BaseApiController
    {
        private readonly IRecommendationService _recommendationService;

        public RecommendationController(IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        [HttpGet("for-you")]
        [Authorize]
        public async Task<IActionResult> GetRecommendations()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var response = await _recommendationService.GetRecommendationsAsync(userId);
            return FromApiResponse(response);
        }

        [HttpGet("similar/{productoId}")]
        public async Task<IActionResult> GetSimilar(int productoId)
        {
            var response = await _recommendationService.GetSimilarProductsAsync(productoId);
            return FromApiResponse(response);
        }
    }
}
