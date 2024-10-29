using System;
namespace AbcUserManagement.Models
{
    public class UserRequest
    {
        public string Username { get; set; }
        public string Password { get; set; } 
        public string Role { get; set; }
        public int CompanyId { get; set; }
    }
}

