using API_TI.Data;
using API_TI.Models.dbModels;
using API_TI.Services.Abstract;
using API_TI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_TI.Services.Implementations
{
    public class ImpuestoService : BaseService, IImpuestoService
    {
        private readonly TiPcComponentsContext _context;

        public ImpuestoService(
            TiPcComponentsContext context,
            IErrorService errorService,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService
        ) : base(errorService, httpContextAccessor, auditService)
        {
            _context = context;
        }

        public async Task<Impuesto> GetApplicableTaxAsync(int direccionId)
        {
            var direccion = await _context.Direccions
                .Include(d => d.Pais)    
                .Include(d => d.Estado)  
                .FirstOrDefaultAsync(d => d.IdDireccion == direccionId);

            if (direccion == null) return null;

            var regla = await _context.ImpuestoReglas
                .Include(r => r.Impuesto)
                .Where(r => r.Impuesto.EstaActivo)
                .Where(r => r.PaisCodigo == null || r.PaisCodigo == direccion.Pais.Codigo) // FIX
                .Where(r => r.EstadoProvincia == null || r.EstadoProvincia == direccion.Estado.NombreEstado)
                .Where(r => r.CodigoPostal == null || r.CodigoPostal == direccion.CodigoPostal)
                .OrderByDescending(r => r.Impuesto.Prioridad)
                .FirstOrDefaultAsync();

            return regla?.Impuesto ?? await _context.Impuestos
                .Where(i => i.EstaActivo && i.Codigo == "IVA")
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> CalculateTaxAsync(decimal subtotal, int direccionId)
        {
            var impuesto = await GetApplicableTaxAsync(direccionId);
            if (impuesto == null) return 0;

            if (impuesto.Tipo == "PORCENTAJE")
                return subtotal * (impuesto.Valor / 100);

            return impuesto.Valor;
        }
    }
}
