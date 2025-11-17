using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace censudex_api.src.Models
{
    public class ClientInfo
    {
        public required string Id { get; set; }
        public required string Email { get; set; }
        public required string Username { get; set; }
        public required string Role { get; set; }
        public required string FullName { get; set; }
    }
}