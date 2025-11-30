using API_TI.Models.DTOs.MarcaDTOs;
using API_TI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_TI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MarcaController : BaseApiController
    {
        private readonly IMarcaService _marcaService;

        public MarcaController(IMarcaService marcaService)
        {
            _marcaService = marcaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _marcaService.GetAllAsync();
            return FromApiResponse(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _marcaService.GetByIdAsync(id);  
            return FromApiResponse(response);
        }

        [HttpPost]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Create(CreateMarcaDto dto)
        {
            var response = await _marcaService.CreateAsync(dto);
            return FromApiResponse(response);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Update(int id, UpdateMarcaDto dto)
        {
            dto.IdMarca = id;
            var response = await _marcaService.UpdateAsync(dto);
            return FromApiResponse(response);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _marcaService.DeleteAsync(id);
            return FromApiResponse(response);
        }
    }
}
