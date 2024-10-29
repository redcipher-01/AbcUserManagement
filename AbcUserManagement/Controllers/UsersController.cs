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
                var user = await _userService.GetUserByIdAsync(id).ConfigureAwait(false);
                if (user == null || user.CompanyId != companyId || (role == Role.User.ToString() && user.Role == Role.Admin))
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
                var createdBy = User.FindFirst(ClaimTypes.Name)?.Value;
                if (string.IsNullOrEmpty(createdBy))
                {
                    return Unauthorized("User is not authenticated.");
                }

                if (userRequest.CompanyId != companyId)
                {
                    return BadRequest("Cannot create a user for a different company.");
                }

                if (!Enum.TryParse(userRequest.Role, true, out Role role))
                {
                    return BadRequest("Invalid role specified.");
                }

                var user = new User
                {
                    Username = userRequest.Username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRequest.Password),
                    Role = role,
                    CompanyId = userRequest.CompanyId
                };

                _logger.LogInformation("Adding user: {Username}", user.Username);
                await _userService.AddUserAsync(user, createdBy).ConfigureAwait(false);
                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
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
                var user = await _userService.GetUserByIdAsync(id).ConfigureAwait(false);
                if (user == null)
                {
                    return NotFound();
                }

                if (user.CompanyId != companyId)
                {
                    return BadRequest("Cannot update a user from a different company.");
                }

                var modifiedBy = User.FindFirst(ClaimTypes.Name)?.Value;
                if (string.IsNullOrEmpty(modifiedBy))
                {
                    return Unauthorized("User is not authenticated.");
                }

                if (!Enum.TryParse(userRequest.Role, true, out Role role))
                {
                    return BadRequest("Invalid role specified.");
                }

                user.Username = userRequest.Username;
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRequest.Password);
                user.Role = role;
                user.CompanyId = userRequest.CompanyId;

                _logger.LogInformation("Updating user: {Id}", user.Id);
                await _userService.UpdateUserAsync(user, modifiedBy).ConfigureAwait(false);
                return NoContent();
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
                var user = await _userService.GetUserByIdAsync(id).ConfigureAwait(false);
                if (user == null)
                {
                    return NotFound();
                }

                if (user.CompanyId != companyId)
                {
                    return BadRequest("Cannot delete a user from a different company.");
                }

                var deletedBy = User.FindFirst(ClaimTypes.Name)?.Value;
                if (string.IsNullOrEmpty(deletedBy))
                {
                    return Unauthorized("User is not authenticated.");
                }

                _logger.LogInformation("Deleting user: {Id} by {DeletedBy}", id, deletedBy);
                await _userService.DeleteUserAsync(id).ConfigureAwait(false);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}