using AbcUserManagement.DataAccess;
using AbcUserManagement.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AbcUserManagement.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Service: Getting user by ID: {Id}", id);
                return await _userRepository.GetUserByIdAsync(id).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error getting user by ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetUsersByCompanyIdAsync(int companyId, string role)
        {
            try
            {
                _logger.LogInformation("Service: Getting users by company ID: {CompanyId}", companyId);
                var users = await _userRepository.GetUsersByCompanyIdAsync(companyId).ConfigureAwait(false);

                if (role == Role.User.ToString())
                {
                    users = users.Where(u => u.Role != Role.Admin);
                }

                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error getting users by company ID: {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            try
            {
                _logger.LogInformation("Service: Getting user by username: {Username}", username);
                return await _userRepository.GetUserByUsernameAsync(username).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error getting user by username: {Username}", username);
                throw;
            }
        }

        public async Task AddUserAsync(User user, string createdBy)
        {
            try
            {
                _logger.LogInformation("Service: Adding user: {Username}", user.Username);
                user.CreatedBy = createdBy;
                user.CreatedDate = DateTime.UtcNow;
                await _userRepository.AddUserAsync(user).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error adding user: {Username}", user.Username);
                throw;
            }
        }

        public async Task UpdateUserAsync(User user, string modifiedBy)
        {
            try
            {
                _logger.LogInformation("Service: Updating user: {Id}", user.Id);
                user.ModifiedBy = modifiedBy;
                user.ModifiedDate = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error updating user: {Id}", user.Id);
                throw;
            }
        }

        public async Task DeleteUserAsync(int id)
        {
            try
            {
                _logger.LogInformation("Service: Deleting user: {Id}", id);
                await _userRepository.DeleteUserAsync(id).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error deleting user: {Id}", id);
                throw;
            }
        }
    }
}