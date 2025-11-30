using API_TI.Data;
using API_TI.Models.dbModels;
using API_TI.Models.DTOs.CategoriaDTOs;
using API_TI.Models.DTOs.DescuentoDTOs;
using API_TI.Models.DTOs.ErrorDTOs;
using API_TI.Models.DTOs.ProductoDTOs;
using API_TI.Services.Helpers;
using API_TI.Services.Interfaces;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Net;

namespace API_TI.Services.Implementations
{
    public class ErrorService : IErrorService
    {
        private readonly TiPcComponentsContext _context;
        private readonly ILogger<ErrorService> _logger;

        public ErrorService(
            TiPcComponentsContext context, 
            ILogger<ErrorService> logger
        )
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ErrorInfoDto>> GetAllAsync()
        {
            var error = await _context.ErrorCodigos
                .AsNoTracking()
                .ToListAsync();

            return Mapper.ToErroInforDto(error);
        }
        
        public async Task<ErrorInfoDto> GetErrorByCodeInfoAsync(int codigo)
        {
            // Validate code is in defined ranges
            if (!IsValidErrorCode(codigo))
            {
                _logger.LogWarning("Invalid error code requested: {Code}", codigo);
                codigo = 9999;
            }
            var error = await _context.ErrorCodigos.FindAsync(codigo);

            if (error == null)
            {
                return new ErrorInfoDto
                {
                    Codigo = 9999,
                    Clave = "ERROR_DESCONOCIDO",
                    Mensaje = "Error no identificado.",
                    Severidad = 3
                };
            }

            return Mapper.ToErroInforDto(error);
        }

        // This is for technical/logging only: persists technical details (stack trace) in ErrorLogs table.
        // Only middleware should call this method for unexpected exceptions.
        public async Task LogErrorAsync(int codigo, string technicalDetail, string? endpoint = null, int? usuario = null, string? email = null)
        {
            try
            {
                var errorInfo = await _context.ErrorCodigos.FindAsync(codigo);
                if (errorInfo == null)
                {
                    codigo = 9999;
                }

                var log = new ErrorLog
                {
                    Codigo = codigo,
                    Mensaje = errorInfo?.Mensaje ?? "Error no identificado.",
                    DetalleTecnico = technicalDetail,
                    EndpointApp = endpoint,
                    Fecha = DateTime.UtcNow,
                    UsuarioId = usuario
                };

                _context.ErrorLogs.Add(log);
                await _context.SaveChangesAsync();

                // Also log to Serilog structured
                _logger.LogError("Error persisted to DB {@Meta}", new
                {
                    Code = codigo,
                    Endpoint = endpoint,
                    UsuarioId = usuario,
                    CorrelationId = log.IdErrorLog // or context.TraceIdentifier in middleware
                });
            }
            catch (Exception ex)
            {
                // If persisting fails, log the failure but DO NOT throw
                _logger.LogError(ex, "Failed to persist ErrorLog to DB");
            }
        }

        private bool IsValidErrorCode(int code)
        {
            return (code >= 0 && code <= 99) ||
                   (code >= 100 && code <= 199) ||
                   (code >= 200 && code <= 299) ||
                   (code >= 300 && code <= 399) ||
                   (code >= 400 && code <= 499) ||
                   (code >= 500 && code <= 599) ||
                   (code >= 600 && code <= 699) ||
                   (code >= 700 && code <= 799) ||
                   (code >= 800 && code <= 899) ||
                   (code >= 900 && code <= 999) ||
                   (code >= 1000 && code <= 1099) ||
                   (code >= 9000 && code <= 9999);
        }
    }
}
