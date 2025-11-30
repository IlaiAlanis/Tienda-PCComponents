using API_TI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API_TI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WishlistController : BaseApiController
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        [HttpGet]
        public async Task<IActionResult> GetWishlist()
        {
            var response = await _wishlistService.GetWishlistAsync(GetUserId());
            return FromApiResponse(response);
        }

        [HttpPost("items/{productoId}")]
        public async Task<IActionResult> AddItem(int productoId)
        {
            var response = await _wishlistService.AddItemAsync(GetUserId(), productoId);
            return FromApiResponse(response);
        }

        [HttpDelete("items/{productoId}")]
        public async Task<IActionResult> RemoveItem(int productoId)
        {
            var response = await _wishlistService.RemoveItemAsync(GetUserId(), productoId);
            return FromApiResponse(response);
        }
    }
}
