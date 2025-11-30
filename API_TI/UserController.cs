using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.UsuarioDTOs;
using API_TI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace API_TI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : BaseApiController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var response = await _userService.GetProfileAsync(GetUserId());
            return FromApiResponse(response);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            if (!ModelState.IsValid)
                return FromApiResponse(ApiResponse<UserProfileDto>.Fail(2, "Datos inválidos", 2));

            var response = await _userService.UpdateProfileAsync(GetUserId(), request);
            return FromApiResponse(response);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
                return FromApiResponse(ApiResponse<object>.Fail(2, "Datos incompletos", 2));

            var response = await _userService.ChangePasswordAsync(GetUserId(), request);
            return FromApiResponse(response);
        }

        [HttpPut("email")]
        public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmailRequest request)
        {
            if (!ModelState.IsValid)
                return FromApiResponse(ApiResponse<object>.Fail(2, "Datos inválidos", 2));

            var response = await _userService.UpdateEmailAsync(GetUserId(), request);
            return FromApiResponse(response);
        }

        [HttpDelete("account")]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
        {
            var response = await _userService.DeleteAccountAsync(GetUserId(), request.Password);
            return FromApiResponse(response);
        }
    }
}
