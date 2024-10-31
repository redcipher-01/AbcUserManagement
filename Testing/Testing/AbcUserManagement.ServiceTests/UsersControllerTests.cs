using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AbcUserManagement.Controllers;
using AbcUserManagement.Models;
using AbcUserManagement.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace AbcUserManagement.UnitTests
{
    [TestFixture]
    public class UsersControllerTests
    {
        private Mock<IUserService> _userServiceMock;
        private Mock<ILogger<UsersController>> _loggerMock;
        private UsersController _usersController;

        [SetUp]
        public void SetUp()
        {
            _userServiceMock = new Mock<IUserService>();
            _loggerMock = new Mock<ILogger<UsersController>>();
            _usersController = new UsersController(_userServiceMock.Object, _loggerMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("CompanyId", "1")
            }, "mock"));

            _usersController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test]
        public async Task GetUsers_ShouldReturnOk_WhenUsersExist()
        {
            // Arrange
            var companyId = 1;
            var users = new List<User>
            {
                new User { Id = 1, Username = "test1", CompanyId = companyId },
                new User { Id = 2, Username = "test2", CompanyId = companyId }
            };
            _userServiceMock.Setup(service => service.GetUsersByCompanyIdAsync(companyId, "Admin")).ReturnsAsync(users);

            // Act
            var result = await _usersController.GetUsers().ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.AreEqual(users, okResult.Value);
        }

        [Test]
        public async Task GetUserById_ShouldReturnOk_WhenUserExists()
        {
            // Arrange
            var userId = 1;
            var user = new User { Id = userId, Username = "test", CompanyId = 1 };
            _userServiceMock.Setup(service => service.GetUserByIdAsync(userId, 1, "Admin")).ReturnsAsync(user);

            // Act
            var result = await _usersController.GetUserById(userId).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.AreEqual(user, okResult.Value);
        }

        [Test]
        public async Task GetUserById_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = 1;
            _userServiceMock.Setup(service => service.GetUserByIdAsync(userId, 1, "Admin")).ReturnsAsync((User)null);

            // Act
            var result = await _usersController.GetUserById(userId).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public async Task AddUser_ShouldReturnCreatedAtAction_WhenUserIsAdded()
        {
            // Arrange
            var userRequest = new UserRequest { Username = "test", Password = "password", Role = "User", CompanyId = 1 };
            var user = new User
            {
                Username = userRequest.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRequest.Password),
                Role = Role.User,
                CompanyId = userRequest.CompanyId
            };
            _userServiceMock.Setup(service => service.AddUserAsync(It.IsAny<User>(), 1, "Admin", "admin")).Returns(Task.CompletedTask);

            // Act
            var result = await _usersController.AddUser(userRequest).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<CreatedAtActionResult>(result);
            var createdAtActionResult = result as CreatedAtActionResult;
            Assert.AreEqual("GetUserById", createdAtActionResult.ActionName);
        }

        [Test]
        public async Task UpdateUser_ShouldReturnNoContent_WhenUserIsUpdated()
        {
            // Arrange
            var userRequest = new UserRequest { Username = "test", Password = "password", Role = "User", CompanyId = 1 };
            var user = new User
            {
                Id = 1,
                Username = userRequest.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRequest.Password),
                Role = Role.User,
                CompanyId = userRequest.CompanyId
            };
            _userServiceMock.Setup(service => service.GetUserByIdAsync(user.Id, 1, "Admin")).ReturnsAsync(user);
            _userServiceMock.Setup(service => service.UpdateUserAsync(It.IsAny<User>(), 1, "Admin", "admin")).Returns(Task.CompletedTask);

            // Act
            var result = await _usersController.UpdateUser(user.Id, userRequest).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<NoContentResult>(result);
        }

        [Test]
        public async Task DeleteUser_ShouldReturnNoContent_WhenUserIsDeleted()
        {
            // Arrange
            var userId = 1;
            var user = new User { Id = userId, CompanyId = 1 };
            _userServiceMock.Setup(service => service.GetUserByIdAsync(userId, 1, "Admin")).ReturnsAsync(user);
            _userServiceMock.Setup(service => service.DeleteUserAsync(userId, 1, "Admin")).Returns(Task.CompletedTask);

            // Act
            var result = await _usersController.DeleteUser(userId).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<NoContentResult>(result);
        }

        [Test]
        public async Task AddUser_ShouldReturnBadRequest_WhenCompanyIdMismatch()
        {
            // Arrange
            var userRequest = new UserRequest { Username = "test", Password = "password", Role = "User", CompanyId = 2 };

            // Act
            var result = await _usersController.AddUser(userRequest).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task UpdateUser_ShouldReturnBadRequest_WhenCompanyIdMismatch()
        {
            // Arrange
            var userRequest = new UserRequest { Username = "test", Password = "password", Role = "User", CompanyId = 2 };
            var user = new User
            {
                Id = 1,
                Username = userRequest.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRequest.Password),
                Role = Role.User,
                CompanyId = 1
            };
            _userServiceMock.Setup(service => service.GetUserByIdAsync(user.Id, 1, "Admin")).ReturnsAsync(user);

            // Act
            var result = await _usersController.UpdateUser(user.Id, userRequest).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task DeleteUser_ShouldReturnBadRequest_WhenCompanyIdMismatch()
        {
            // Arrange
            var userId = 1;
            var user = new User { Id = userId, CompanyId = 2 };
            _userServiceMock.Setup(service => service.GetUserByIdAsync(userId, 1, "Admin")).ReturnsAsync(user);

            // Act
            var result = await _usersController.DeleteUser(userId).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
    }
}