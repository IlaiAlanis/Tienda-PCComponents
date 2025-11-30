using API_TI.Models.DTOs.ReviewDTOs;
using API_TI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API_TI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : BaseApiController
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [HttpGet("producto/{productoId}")]
        public async Task<IActionResult> GetProductReviews(int productoId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var response = await _reviewService.GetProductReviewsAsync(productoId, page, pageSize);
            return FromApiResponse(response);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
        {
            var response = await _reviewService.CreateReviewAsync(request, GetUserId());
            return FromApiResponse(response);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] CreateReviewRequest request)
        {
            var response = await _reviewService.UpdateReviewAsync(id, request, GetUserId());
            return FromApiResponse(response);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var response = await _reviewService.DeleteReviewAsync(id, GetUserId());
            return FromApiResponse(response);
        }
    }
}
