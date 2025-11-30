namespace API_TI.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailVerificationAsync(string email, string code);
        Task SendPasswordResetAsync(string email, string code);
        Task SendOrderConfirmationEmailAsync(string email, string orderNumber, decimal total);
        Task SendPaymentConfirmationEmailAsync(string email, string orderNumber, decimal amount);
        Task SendOrderProcessingEmailAsync(string email, string orderNumber);
        Task SendOrderShippedEmailAsync(string email, string orderNumber, string trackingNumber);
        Task SendOrderDeliveredEmailAsync(string email, string orderNumber);
        Task SendOrderCancelledEmailAsync(string email, string orderNumber);
        Task SendRefundConfirmationEmailAsync(string email, string orderNumber, decimal amount);
        Task SendContactNotificationToAdminAsync(string userName, string userEmail, string motivo, string mensaje);
        Task SendContactConfirmationAsync(string email, string userName);

    }
}
