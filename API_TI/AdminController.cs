using API_TI.Data;
using API_TI.Models.Auth;
using API_TI.Models.DTOs.AdminDTOs;
using API_TI.Models.DTOs.UsuarioDTOs;
using API_TI.Services.Implementations;
using API_TI.Services.Interfaces;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static Google.Apis.Requests.BatchRequest;

namespace API_TI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequireAdmin")]
    public class AdminController : BaseApiController
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        private int GetAdminId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var response = await _adminService.GetDashboardStatsAsync();
            return FromApiResponse(response);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] UserListRequest request)
        {
            var response = await _adminService.GetUsersAsync(request);
            return FromApiResponse(response);
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserDetails(int id)
        {
            var response = await _adminService.GetUserDetailsAsync(id);
            return FromApiResponse(response);
        }

        [HttpPut("users/{id}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var response = await _adminService.ToggleUserStatusAsync(GetAdminId(), id);
            return FromApiResponse(response);
        }
    }
}
