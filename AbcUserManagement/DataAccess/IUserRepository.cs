using AbcUserManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AbcUserManagement.DataAccess
{
    public interface IUserRepository
    {
        Task<User> GetUserByIdAsync(int id);
        Task<IEnumerable<User>> GetUsersByCompanyIdAsync(int companyId);
        Task<User> GetUserByUsernameAsync(string username); // Add this method
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int id);
    }
}