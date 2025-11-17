using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using censudex_api.src.Dto;

namespace censudex_api.src.Models
{
    public class TokenValidationResponse
    {
        public bool IsValid { get; set; }
        public required string Message { get; set; }
        public List<ClaimDto>? Claims { get; set; }
    }
}