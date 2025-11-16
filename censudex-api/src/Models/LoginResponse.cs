using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using censudex_api.src.Models;

namespace censudex_api.src.Models
{
    public class LoginResponse
    {
        public string? Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public ClientInfo Client { get; set; }
    }
}