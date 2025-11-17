using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace censudex_api.src.Dto
{
    public class RegisterClientDto
    {
        public required string Name { get; set; }
        public required string Surename { get; set; }
        public required string Email { get; set; }
        public required string Username { get; set; }
        public required string Birthdate { get; set; }
        public required string Address { get; set; }
        public required string TelephoneNumber { get; set; }
        public required string Password { get; set; }
    }
}