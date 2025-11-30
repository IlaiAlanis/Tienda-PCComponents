using API_TI.Models.dbModels;

namespace API_TI.Services.Interfaces
{
    public interface IImpuestoService
    {
        Task<Impuesto> GetApplicableTaxAsync(int direccionId);
        Task<decimal> CalculateTaxAsync(decimal subtotal, int direccionId);
    }
}
