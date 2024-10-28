using System;
namespace AbcUserManagement.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public Role Role { get; set; } // Admin or User
        public int CompanyId { get; set; }
    }
}

