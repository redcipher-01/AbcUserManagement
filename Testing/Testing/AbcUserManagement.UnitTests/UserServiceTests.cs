using Moq;
using NUnit.Framework;
using AbcUserManagement.Services;
using AbcUserManagement.DataAccess;
using AbcUserManagement.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace AbcUserManagement.UnitTests
{
    [TestFixture]
    public class UserServiceTests
    {
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<ILogger<UserService>> _loggerMock;
        private IUserService _userService;

        [SetUp]
        public void SetUp()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _loggerMock = new Mock<ILogger<UserService>>();
            _userService = new UserService(_userRepositoryMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var userId = 1;
            var expectedUser = new User { Id = userId, Username = "test" };
            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.GetUserByIdAsync(userId).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(expectedUser, result);
        }

        [Test]
        public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = 1;
            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync((User)null);

            // Act
            var result = await _userService.GetUserByIdAsync(userId).ConfigureAwait(false);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetUsersByCompanyIdAsync_ShouldReturnAllUsers_WhenRoleIsAdmin()
        {
            // Arrange
            var companyId = 1;
            var role = Role.Admin.ToString();
            var expectedUsers = new List<User>
            {
                new User { Id = 1, Username = "admin1", Role = Role.Admin, CompanyId = companyId },
                new User { Id = 2, Username = "user1", Role = Role.User, CompanyId = companyId }
            };
            _userRepositoryMock.Setup(repo => repo.GetUsersByCompanyIdAsync(companyId)).ReturnsAsync(expectedUsers);

            // Act
            var result = await _userService.GetUsersByCompanyIdAsync(companyId, role).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(expectedUsers, result);
        }

        [Test]
        public async Task GetUsersByCompanyIdAsync_ShouldReturnNonAdminUsers_WhenRoleIsUser()
        {
            // Arrange
            var companyId = 1;
            var role = Role.User.ToString();
            var expectedUsers = new List<User>
            {
                new User { Id = 1, Username = "admin1", Role = Role.Admin, CompanyId = companyId },
                new User { Id = 2, Username = "user1", Role = Role.User, CompanyId = companyId }
            };
            _userRepositoryMock.Setup(repo => repo.GetUsersByCompanyIdAsync(companyId)).ReturnsAsync(expectedUsers);

            // Act
            var result = await _userService.GetUsersByCompanyIdAsync(companyId, role).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("user1", result.First().Username);
        }

        [Test]
        public async Task GetUsersByCompanyIdAsync_ShouldReturnEmptyList_WhenNoUsersExist()
        {
            // Arrange
            var companyId = 1;
            var role = Role.Admin.ToString();
            _userRepositoryMock.Setup(repo => repo.GetUsersByCompanyIdAsync(companyId)).ReturnsAsync(new List<User>());

            // Act
            var result = await _userService.GetUsersByCompanyIdAsync(companyId, role).ConfigureAwait(false);

            // Assert
            Assert.IsEmpty(result);
        }

        [Test]
        public async Task AddUserAsync_ShouldCallRepositoryMethod()
        {
            // Arrange
            var user = new User { Username = "test", PasswordHash = "hash", Role = Role.User, CompanyId = 1 };
            var createdBy = "admin";

            // Act
            await _userService.AddUserAsync(user, createdBy).ConfigureAwait(false);

            // Assert
            _userRepositoryMock.Verify(repo => repo.AddUserAsync(user), Times.Once);
        }

        [Test]
        public async Task UpdateUserAsync_ShouldCallRepositoryMethod()
        {
            // Arrange
            var user = new User { Id = 1, Username = "test", PasswordHash = "hash", Role = Role.User, CompanyId = 1 };
            var modifiedBy = "admin";

            // Act
            await _userService.UpdateUserAsync(user, modifiedBy).ConfigureAwait(false);

            // Assert
            _userRepositoryMock.Verify(repo => repo.UpdateUserAsync(user), Times.Once);
        }

        [Test]
        public async Task DeleteUserAsync_ShouldCallRepositoryMethod()
        {
            // Arrange
            var userId = 1;

            // Act
            await _userService.DeleteUserAsync(userId).ConfigureAwait(false);

            // Assert
            _userRepositoryMock.Verify(repo => repo.DeleteUserAsync(userId), Times.Once);
        }

        [Test]
        public async Task GetUserByUsernameAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var username = "test";
            var expectedUser = new User { Id = 1, Username = username };
            _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync(username)).ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.GetUserByUsernameAsync(username).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(expectedUser, result);
        }

        [Test]
        public async Task GetUserByUsernameAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var username = "test";
            _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync(username)).ReturnsAsync((User)null);

            // Act
            var result = await _userService.GetUserByUsernameAsync(username).ConfigureAwait(false);

            // Assert
            Assert.IsNull(result);
        }
    }
}