using API_TI.Models.DTOs.CategoriaDTOs;
using API_TI.Services.Interfaces;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_TI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriaController : BaseApiController
    {
        private readonly ICategoriaService _categoriaService;

        public CategoriaController(ICategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _categoriaService.GetAllAsync();
            return FromApiResponse(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _categoriaService.GetByIdAsync(id);  
            return FromApiResponse(response);
        }

        [HttpPost]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Create(CreateCategoriaDto dto)
        {
            var response = await _categoriaService.CreateAsync(dto);
            return FromApiResponse(response);
        }

        [HttpPut]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Update(int id, UpdateCategoriaDto dto)
        {
            dto.IdCategoria = id;
            var response = await _categoriaService.UpdateAsync(dto);
            return FromApiResponse(response);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _categoriaService.DeleteAsync(id);
            return FromApiResponse(response);
        }
    }
}
