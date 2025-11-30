using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.FaqDTOs;
using API_TI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_TI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FaqController : BaseApiController
    {
        private readonly IFaqService _faqService;

        public FaqController(IFaqService faqService)
        {
            _faqService = faqService;
        }

        /// <summary>
        /// Get all active FAQs (Public)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var response = await _faqService.GetAllFaqsAsync();
            return FromApiResponse(response);
        }

        /// <summary>
        /// Get FAQ by ID (Public)
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _faqService.GetFaqByIdAsync(id);
            return FromApiResponse(response);
        }

        /// <summary>
        /// Create FAQ (Admin)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Create([FromBody] CreateFaqRequest request)
        {
            if (!ModelState.IsValid)
                return FromApiResponse(ApiResponse<FaqDto>.Fail(2, "Datos inválidos", 2));

            var response = await _faqService.CreateFaqAsync(request);
            return FromApiResponse(response);
        }

        /// <summary>
        /// Update FAQ (Admin)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateFaqRequest request)
        {
            if (!ModelState.IsValid)
                return FromApiResponse(ApiResponse<FaqDto>.Fail(2, "Datos inválidos", 2));

            var response = await _faqService.UpdateFaqAsync(id, request);
            return FromApiResponse(response);
        }

        /// <summary>
        /// Delete FAQ (Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _faqService.DeleteFaqAsync(id);
            return FromApiResponse(response);
        }
    }
}