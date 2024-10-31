using Moq;
using NUnit.Framework;
using AbcUserManagement.Services;
using AbcUserManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using AbcUserManagement.DataAccess;
using Microsoft.Extensions.Logging;
using System;

namespace AbcUserManagement.UnitTests
{
    [TestFixture]
    public class UserServiceTests
    {
        private Mock<IUserRepository> _userRepositoryMock;
        private UserService _userService;

        [SetUp]
        public void SetUp()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            var loggerMock = new Mock<ILogger<UserService>>();
            _userService = new UserService(_userRepositoryMock.Object, loggerMock.Object);
        }

        [Test]
        public async Task AddUserAsync_ShouldCallRepositoryMethod_ForAdmin()
        {
            // Arrange
            var user = new User { Username = "test", PasswordHash = "hash", Role = Role.User, CompanyId = 1 };
            var companyId = 1;
            var role = Role.Admin.ToString();
            var createdBy = "admin";

            // Act
            await _userService.AddUserAsync(user, companyId, role, createdBy).ConfigureAwait(false);

            // Assert
            _userRepositoryMock.Verify(repo => repo.AddUserAsync(user), Times.Once);
        }

        [Test]
        public void AddUserAsync_ShouldThrowUnauthorizedAccessException_WhenCompanyIdMismatch()
        {
            // Arrange
            var user = new User { Username = "test", PasswordHash = "hash", Role = Role.User, CompanyId = 2 };
            var companyId = 1;
            var role = Role.Admin.ToString();
            var createdBy = "admin";

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(() => _userService.AddUserAsync(user, companyId, role, createdBy));
        }

        [Test]
        public async Task UpdateUserAsync_ShouldCallRepositoryMethod_ForAdmin()
        {
            // Arrange
            var user = new User { Id = 1, Username = "test", PasswordHash = "hash", Role = Role.User, CompanyId = 1 };
            var companyId = 1;
            var role = Role.Admin.ToString();
            var modifiedBy = "admin";

            // Act
            await _userService.UpdateUserAsync(user, companyId, role, modifiedBy).ConfigureAwait(false);

            // Assert
            _userRepositoryMock.Verify(repo => repo.UpdateUserAsync(user), Times.Once);
        }

        [Test]
        public void UpdateUserAsync_ShouldThrowUnauthorizedAccessException_WhenCompanyIdMismatch()
        {
            // Arrange
            var user = new User { Id = 1, Username = "test", PasswordHash = "hash", Role = Role.User, CompanyId = 2 };
            var companyId = 1;
            var role = Role.Admin.ToString();
            var modifiedBy = "admin";

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(() => _userService.UpdateUserAsync(user, companyId, role, modifiedBy));
        }

        [Test]
        public async Task DeleteUserAsync_ShouldCallRepositoryMethod_ForAdmin()
        {
            // Arrange
            var userId = 1;
            var companyId = 1;
            var role = Role.Admin.ToString();

            // Act
            await _userService.DeleteUserAsync(userId, companyId, role).ConfigureAwait(false);

            // Assert
            _userRepositoryMock.Verify(repo => repo.DeleteUserAsync(userId), Times.Once);
        }

        [Test]
        public void DeleteUserAsync_ShouldThrowUnauthorizedAccessException_WhenCompanyIdMismatch()
        {
            // Arrange
            var userId = 1;
            var companyId = 1;
            var role = Role.Admin.ToString();
            var user = new User { Id = userId, CompanyId = 2 };
            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync(user);

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(() => _userService.DeleteUserAsync(userId, companyId, role));
        }

        [Test]
        public async Task GetUsersByCompanyIdAsync_ShouldReturnNonAdminUsers_ForFlatUser()
        {
            // Arrange
            var companyId = 1;
            var role = Role.User.ToString();
            var users = new List<User>
            {
                new User { Id = 1, Username = "admin", Role = Role.Admin, CompanyId = companyId },
                new User { Id = 2, Username = "user1", Role = Role.User, CompanyId = companyId }
            };
            _userRepositoryMock.Setup(repo => repo.GetUsersByCompanyIdAsync(companyId)).ReturnsAsync(users);

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
            var role = Role.User.ToString();
            var users = new List<User>();
            _userRepositoryMock.Setup(repo => repo.GetUsersByCompanyIdAsync(companyId)).ReturnsAsync(users);

            // Act
            var result = await _userService.GetUsersByCompanyIdAsync(companyId, role).ConfigureAwait(false);

            // Assert
            Assert.IsEmpty(result);
        }

        [Test]
        public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = 1;
            var companyId = 1;
            var role = Role.Admin.ToString();
            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync((User)null);

            // Act
            var result = await _userService.GetUserByIdAsync(userId, companyId, role).ConfigureAwait(false);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetUserByIdAsync_ShouldReturnNull_WhenCompanyIdMismatch()
        {
            // Arrange
            var userId = 1;
            var companyId = 1;
            var role = Role.Admin.ToString();
            var user = new User { Id = userId, CompanyId = 2 };
            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserByIdAsync(userId, companyId, role).ConfigureAwait(false);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetUserByIdAsync_ShouldReturnNull_WhenFlatUserTriesToAccessAdmin()
        {
            // Arrange
            var userId = 1;
            var companyId = 1;
            var role = Role.User.ToString();
            var user = new User { Id = userId, CompanyId = companyId, Role = Role.Admin };
            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserByIdAsync(userId, companyId, role).ConfigureAwait(false);

            // Assert
            Assert.IsNull(result);
        }
    }
}