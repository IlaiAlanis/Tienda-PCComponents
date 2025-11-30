namespace API_TI.Models.DTOs.PagoDTOs
{
    public class PaymentIntentResponse
    {
        public string ClientSecret { get; set; }
        public string PaymentIntentId { get; set; }
        public string PublicKey { get; set; }
    }
}
