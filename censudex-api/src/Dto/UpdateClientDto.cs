using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace censudex_api.src.Dto
{
    public class UpdateClientDto
    {
        public string? Name { get; set; }
        public string? Surename { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? Birthdate { get; set; }
        public string? Address { get; set; }
        public string? TelephoneNumber { get; set; }
        public string? Password { get; set; }
    }
}