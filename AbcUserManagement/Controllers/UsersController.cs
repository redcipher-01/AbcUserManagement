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

                var users = await _userService.GetUsersByCompanyIdAsync(companyId).ConfigureAwait(false);
                if (role == Role.User.ToString())
                {
                    users = users.Where(u => u.Role != Role.Admin);
                }

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
                _logger.LogInformation("Getting user by ID: {Id}", id);
                var user = await _userService.GetUserByIdAsync(id).ConfigureAwait(false);
                if (user == null)
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
        public async Task<IActionResult> AddUser([FromBody] User user)
        {
            try
            {
                _logger.LogInformation("Adding user: {Username}", user.Username);
                await _userService.AddUserAsync(user).ConfigureAwait(false);
                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user: {Username}", user.Username);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
        {
            if (id != user.Id)
            {
                _logger.LogWarning("Update user failed: ID mismatch");
                return BadRequest();
            }

            try
            {
                _logger.LogInformation("Updating user: {Id}", user.Id);
                await _userService.UpdateUserAsync(user).ConfigureAwait(false);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {Id}", user.Id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                _logger.LogInformation("Deleting user: {Id}", id);
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