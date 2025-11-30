using System.ComponentModel.DataAnnotations;

namespace API_TI.Models.DTOs.DireccionDTOs
{
    public class ValidatePlaceRequest
    {
        [Required]
        public string PlaceId { get; set; }
    }
}
