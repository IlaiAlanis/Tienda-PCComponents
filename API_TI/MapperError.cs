using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace API_TI.Services.Helpers
{
    public static class MapperError
    {
        private static readonly Dictionary<int, ApiErrorType> ErrorMapping = new()
        {
            [100] = ApiErrorType.Unauthorized,     
            [101] = ApiErrorType.Forbid,           
            [102] = ApiErrorType.Unauthorized,     
            [103] = ApiErrorType.Forbid,           
            [104] = ApiErrorType.Unauthorized,    
            [105] = ApiErrorType.Unauthorized,     
            [106] = ApiErrorType.Unauthorized,     
            [107] = ApiErrorType.Unauthorized,     
            [108] = ApiErrorType.BadRequest,       
            [109] = ApiErrorType.BadRequest,       

            [200] = ApiErrorType.NotFound,         
            [201] = ApiErrorType.Conflict,         
            [202] = ApiErrorType.Forbid,           
            [203] = ApiErrorType.Unauthorized,    
            [204] = ApiErrorType.Forbid,           

            [300] = ApiErrorType.NotFound,         
            [301] = ApiErrorType.Conflict,         
            [302] = ApiErrorType.Forbid,           
            [303] = ApiErrorType.Conflict,         
            [304] = ApiErrorType.BadRequest,       
            [305] = ApiErrorType.Forbid,          

            [400] = ApiErrorType.NotFound,
            [401] = ApiErrorType.Conflict,
            [402] = ApiErrorType.Conflict,
            [403] = ApiErrorType.Forbid,

            [450] = ApiErrorType.NotFound,
            [451] = ApiErrorType.Conflict,
            [452] = ApiErrorType.Conflict,
            [453] = ApiErrorType.Forbid,

            [500] = ApiErrorType.NotFound,
            [501] = ApiErrorType.Conflict,
            [502] = ApiErrorType.Conflict,
            [503] = ApiErrorType.Forbid,

            [600] = ApiErrorType.NotFound,
            [601] = ApiErrorType.BadRequest,
            [602] = ApiErrorType.BadRequest,
            [603] = ApiErrorType.Conflict,
            [604] = ApiErrorType.BadRequest,

            [700] = ApiErrorType.NotFound,
            [701] = ApiErrorType.Forbid,
            [702] = ApiErrorType.Conflict,
            [703] = ApiErrorType.Conflict,
            [704] = ApiErrorType.PaymentRequired,

            [800] = ApiErrorType.NotFound,
            [801] = ApiErrorType.ServerError,
            [802] = ApiErrorType.Conflict,
            [803] = ApiErrorType.BadRequest,
            [804] = ApiErrorType.BadRequest,
            [805] = ApiErrorType.ServerError,
            [806] = ApiErrorType.ServerError,
            [807] = ApiErrorType.ServerError,
            [808] = ApiErrorType.ServerError,

            [850] = ApiErrorType.NotFound,          
            [851] = ApiErrorType.BadRequest,        
            [852] = ApiErrorType.BadRequest,        
            [853] = ApiErrorType.Forbid,            
            [854] = ApiErrorType.Conflict,         
            [855] = ApiErrorType.Conflict,          
            [856] = ApiErrorType.Forbid,            
            [857] = ApiErrorType.Forbid,            
            [858] = ApiErrorType.Forbid,            
            [859] = ApiErrorType.Forbid,            
            [860] = ApiErrorType.BadRequest,       
            [861] = ApiErrorType.BadRequest,        
            [862] = ApiErrorType.Conflict,          
            [863] = ApiErrorType.Forbid,            
            [864] = ApiErrorType.Forbid,           
            [865] = ApiErrorType.BadRequest,        
            [866] = ApiErrorType.Forbid,           
            [867] = ApiErrorType.Forbid,            
            [868] = ApiErrorType.ServerError,       
            [869] = ApiErrorType.Forbid,            
            [870] = ApiErrorType.BadRequest,       
            [871] = ApiErrorType.Conflict,         

            [900] = ApiErrorType.NotFound,
            [901] = ApiErrorType.Conflict,
            [902] = ApiErrorType.BadRequest,
            [903] = ApiErrorType.Conflict,

            [1000] = ApiErrorType.NotFound,
            [1001] = ApiErrorType.Forbid,
            [1002] = ApiErrorType.Conflict,
            [1003] = ApiErrorType.Conflict,
            [1004] = ApiErrorType.Conflict,

            [9000] = ApiErrorType.ServerError,
            [9001] = ApiErrorType.ServerError,
            [9002] = ApiErrorType.ServerError,
            [9003] = ApiErrorType.ServerError,
            [9004] = ApiErrorType.ServerError,
            [9005] = ApiErrorType.Conflict,
            [9999] = ApiErrorType.ServerError
        };
        public static ApiErrorType MapperErrors(int code, int severity)
        {
            // Direct lookup first
            if (ErrorMapping.TryGetValue(code, out var errorType))
                return errorType;

            // System errors
            if (code >= 9000) return ApiErrorType.ServerError;

            // Range-based fallbacks
            return code switch
            {
                >= 100 and < 200 => ApiErrorType.Unauthorized,
                >= 200 and < 300 => ApiErrorType.BadRequest,
                >= 300 and < 1000 => ApiErrorType.BadRequest,
                0 => ApiErrorType.Ok,
                6 => ApiErrorType.Conflict,
                _ => ApiErrorType.ServerError
            };
        }


        public enum ApiErrorType
        {
            Ok,
            BadRequest,
            Unauthorized,
            Forbid,
            NotFound,
            Conflict,
            PaymentRequired,  // Add if needed for payments
            ServerError
        }
    }
}
