using API_TI.Models.DTOs.ProveedorDTOs;
using API_TI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_TI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequireAdmin")]
    public class ProveedorController : BaseApiController
    {
        private readonly IProveedorService _proveedorService;

        public ProveedorController(IProveedorService proveedorService)
        {
            _proveedorService = proveedorService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _proveedorService.GetAllAsync();
            return FromApiResponse(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _proveedorService.GetByIdAsync(id);
            return FromApiResponse(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProveedorRequest request)
        {
            var response = await _proveedorService.CreateAsync(request);
            return FromApiResponse(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateProveedorRequest request)
        {
            var response = await _proveedorService.UpdateAsync(id, request);
            return FromApiResponse(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _proveedorService.DeleteAsync(id);
            return FromApiResponse(response);
        }
    }
}
