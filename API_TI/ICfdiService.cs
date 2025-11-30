namespace API_TI.Services.Interfaces
{
    public interface ICfdiService
    {
        Task<string> GenerateCfdiXmlAsync(int facturaId);
        Task<byte[]> GeneratePdfAsync(int facturaId);
    }
}
