using Moq;
using NUnit.Framework;
using AbcUserManagement.Services;
using AbcUserManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using AbcUserManagement.DataAccess;
using Microsoft.Extensions.Logging;

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
    }
}