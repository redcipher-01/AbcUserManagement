using System;
using AbcUserManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AbcUserManagement.Services
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(int id);
        Task<IEnumerable<User>> GetUsersByCompanyIdAsync(int companyId, string role);
        Task<User> GetUserByUsernameAsync(string username);
        Task AddUserAsync(User user, string createdBy);
        Task UpdateUserAsync(User user, string modifiedBy);
        Task DeleteUserAsync(int id);
    }
}