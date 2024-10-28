using NUnit.Framework;
using AbcUserManagement.DataAccess;
using AbcUserManagement.Models;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using Dapper;
using Microsoft.Extensions.Logging;
using Moq;

namespace AbcUserManagement.IntegrationTests
{
    [TestFixture]
    public class UserRepositoryTests
    {
        private IUserRepository _userRepository;
        private SqlConnection _dbConnection;
        private Mock<ILogger<UserRepository>> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _dbConnection = new SqlConnection("YourConnectionStringHere");
            _loggerMock = new Mock<ILogger<UserRepository>>();
            _userRepository = new UserRepository(_dbConnection, _loggerMock.Object);
        }

        [Test]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var userId = 1;
            var user = new User { Id = userId, Username = "test", PasswordHash = "hash", Role = Role.User, CompanyId = 1 };
            await _dbConnection.ExecuteAsync("INSERT INTO Users (Id, Username, PasswordHash, Role, CompanyId) VALUES (@Id, @Username, @PasswordHash, @Role, @CompanyId)", user).ConfigureAwait(false);

            // Act
            var result = await _userRepository.GetUserByIdAsync(userId).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(user.Username, result.Username);
        }

        [Test]
        public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = 999;

            // Act
            var result = await _userRepository.GetUserByIdAsync(userId).ConfigureAwait(false);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task AddUserAsync_ShouldAddUser()
        {
            // Arrange
            var user = new User { Username = "test", PasswordHash = "hash", Role = Role.User, CompanyId = 1 };

            // Act
            await _userRepository.AddUserAsync(user).ConfigureAwait(false);

            // Assert
            var result = await _dbConnection.QuerySingleOrDefaultAsync<User>("SELECT * FROM Users WHERE Username = @Username", new { user.Username }).ConfigureAwait(false);
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task UpdateUserAsync_ShouldUpdateUser()
        {
            // Arrange
            var user = new User { Id = 1, Username = "test", PasswordHash = "hash", Role = Role.User, CompanyId = 1 };
            await _dbConnection.ExecuteAsync("INSERT INTO Users (Id, Username, PasswordHash, Role, CompanyId) VALUES (@Id, @Username, @PasswordHash, @Role, @CompanyId)", user).ConfigureAwait(false);

            user.Username = "updated_test";

            // Act
            await _userRepository.UpdateUserAsync(user).ConfigureAwait(false);

            // Assert
            var result = await _dbConnection.QuerySingleOrDefaultAsync<User>("SELECT * FROM Users WHERE Id = @Id", new { user.Id }).ConfigureAwait(false);
            Assert.AreEqual("updated_test", result.Username);
        }

        [Test]
        public async Task DeleteUserAsync_ShouldDeleteUser()
        {
            // Arrange
            var user = new User { Id = 1, Username = "test", PasswordHash = "hash", Role = Role.User, CompanyId = 1 };
            await _dbConnection.ExecuteAsync("INSERT INTO Users (Id, Username, PasswordHash, Role, CompanyId) VALUES (@Id, @Username, @PasswordHash, @Role, @CompanyId)", user).ConfigureAwait(false);

            // Act
            await _userRepository.DeleteUserAsync(user.Id).ConfigureAwait(false);

            // Assert
            var result = await _dbConnection.QuerySingleOrDefaultAsync<User>("SELECT * FROM Users WHERE Id = @Id", new { user.Id }).ConfigureAwait(false);
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetUserByUsernameAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var username = "test";
            var user = new User { Id = 1, Username = username, PasswordHash = "hash", Role = Role.User, CompanyId = 1 };
            await _dbConnection.ExecuteAsync("INSERT INTO Users (Id, Username, PasswordHash, Role, CompanyId) VALUES (@Id, @Username, @PasswordHash, @Role, @CompanyId)", user).ConfigureAwait(false);

            // Act
            var result = await _userRepository.GetUserByUsernameAsync(username).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(user.Username, result.Username);
        }

        [Test]
        public async Task GetUserByUsernameAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var username = "nonexistent";

            // Act
            var result = await _userRepository.GetUserByUsernameAsync(username).ConfigureAwait(false);

            // Assert
            Assert.IsNull(result);
        }
    }
}