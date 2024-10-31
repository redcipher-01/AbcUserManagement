using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AbcUserManagement.Services;
using AbcUserManagement.Models;
using System.Security.Claims;

namespace AbcUserManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var companyId = int.Parse(User.FindFirst("CompanyId").Value);
                var role = User.FindFirst(ClaimTypes.Role).Value;

                _logger.LogInformation("Getting users for company ID: {CompanyId} by role: {Role}", companyId, role);

                var users = await _userService.GetUsersByCompanyIdAsync(companyId, role).ConfigureAwait(false);

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users for company ID: {CompanyId}", int.Parse(User.FindFirst("CompanyId").Value));
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var companyId = int.Parse(User.FindFirst("CompanyId").Value);
                var role = User.FindFirst(ClaimTypes.Role).Value;

                _logger.LogInformation("Getting user by ID: {Id}", id);
                var user = await _userService.GetUserByIdAsync(id, companyId, role).ConfigureAwait(false);
                if (user == null || user.CompanyId != companyId)
                {
                    return NotFound();
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddUser([FromBody] UserRequest userRequest)
        {
            try
            {
                var companyId = int.Parse(User.FindFirst("CompanyId").Value);
                var role = User.FindFirst(ClaimTypes.Role).Value;
                var createdBy = User.FindFirst(ClaimTypes.Name).Value;

                if (userRequest.CompanyId != companyId)
                {
                    return BadRequest("Cannot add user to a different company.");
                }

                var user = new User
                {
                    Username = userRequest.Username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRequest.Password),
                    Role = Enum.TryParse(userRequest.Role, true, out Role parsedRole) ? parsedRole : Role.User,
                    CompanyId = userRequest.CompanyId
                };

                _logger.LogInformation("Adding user: {Username}", user.Username);
                await _userService.AddUserAsync(user, companyId, role, createdBy).ConfigureAwait(false);
                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access while adding user: {Username}", userRequest.Username);
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user: {Username}", userRequest.Username);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserRequest userRequest)
        {
            try
            {
                var companyId = int.Parse(User.FindFirst("CompanyId").Value);
                var role = User.FindFirst(ClaimTypes.Role).Value;
                var modifiedBy = User.FindFirst(ClaimTypes.Name).Value;

                if (userRequest.CompanyId != companyId)
                {
                    return BadRequest("Cannot update user to a different company.");
                }

                var user = new User
                {
                    Id = id,
                    Username = userRequest.Username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRequest.Password),
                    Role = Enum.TryParse(userRequest.Role, true, out Role parsedRole) ? parsedRole : Role.User,
                    CompanyId = userRequest.CompanyId
                };

                _logger.LogInformation("Updating user: {Id}", user.Id);
                await _userService.UpdateUserAsync(user, companyId, role, modifiedBy).ConfigureAwait(false);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access while updating user: {Id}", id);
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var companyId = int.Parse(User.FindFirst("CompanyId").Value);
                var role = User.FindFirst(ClaimTypes.Role).Value;

                _logger.LogInformation("Deleting user: {Id} by {DeletedBy}", id, companyId);
                var user = await _userService.GetUserByIdAsync(id, companyId, role).ConfigureAwait(false);
                if (user == null || user.CompanyId != companyId)
                {
                    return NotFound();
                }

                await _userService.DeleteUserAsync(id, companyId, role).ConfigureAwait(false);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access while deleting user: {Id}", id);
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}