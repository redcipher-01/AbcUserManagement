using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using AbcUserManagement;
using System.Net.Http.Json;
using AbcUserManagement.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Net;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using AbcUserManagement.Controllers;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Text.Encodings.Web;

namespace AbcUserManagement.ServiceTests
{
    [TestFixture]
    public class UsersControllerTests
    {
        private HttpClient _client;
        private Mock<ILogger<UsersController>> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            var appFactory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.AddAuthentication("Test")
                            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
                    });
                });

            _client = appFactory.CreateClient();
            _loggerMock = new Mock<ILogger<UsersController>>();
        }

        [Test]
        public async Task GetUsers_AsAdmin_ShouldReturnAllUsers()
        {
            // Arrange
            var adminClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("CompanyId", "1")
            };
            var adminIdentity = new ClaimsIdentity(adminClaims, "TestAuthType");
            var adminPrincipal = new ClaimsPrincipal(adminIdentity);

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test");

            // Act
            var response = await _client.GetAsync("/api/users").ConfigureAwait(false);

            // Assert
            response.EnsureSuccessStatusCode();
            var users = await response.Content.ReadFromJsonAsync<IEnumerable<User>>().ConfigureAwait(false);
            Assert.IsNotNull(users);
            Assert.IsTrue(users.Any());
        }

        [Test]
        public async Task GetUsers_AsUser_ShouldReturnUsersFromSameCompany()
        {
            // Arrange
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "user"),
                new Claim(ClaimTypes.Role, "User"),
                new Claim("CompanyId", "1")
            };
            var userIdentity = new ClaimsIdentity(userClaims, "TestAuthType");
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test");

            // Act
            var response = await _client.GetAsync("/api/users").ConfigureAwait(false);

            // Assert
            response.EnsureSuccessStatusCode();
            var users = await response.Content.ReadFromJsonAsync<IEnumerable<User>>().ConfigureAwait(false);
            Assert.IsNotNull(users);
            Assert.IsTrue(users.All(u => u.CompanyId == 1));
        }

        [Test]
        public async Task GetUsers_AsUser_ShouldNotReturnAdminUsers()
        {
            // Arrange
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "user"),
                new Claim(ClaimTypes.Role, "User"),
                new Claim("CompanyId", "1")
            };
            var userIdentity = new ClaimsIdentity(userClaims, "TestAuthType");
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test");

            // Act
            var response = await _client.GetAsync("/api/users").ConfigureAwait(false);

            // Assert
            response.EnsureSuccessStatusCode();
            var users = await response.Content.ReadFromJsonAsync<IEnumerable<User>>().ConfigureAwait(false);
            Assert.IsNotNull(users);
            Assert.IsTrue(users.All(u => u.Role != Role.Admin));
        }

        [Test]
        public async Task GetUserById_AsUser_ShouldReturnUserFromSameCompany()
        {
            // Arrange
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "user"),
                new Claim(ClaimTypes.Role, "User"),
                new Claim("CompanyId", "1")
            };
            var userIdentity = new ClaimsIdentity(userClaims, "TestAuthType");
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test");

            var userId = 1; // Replace with a valid user ID from the same company

            // Act
            var response = await _client.GetAsync($"/api/users/{userId}").ConfigureAwait(false);

            // Assert
            response.EnsureSuccessStatusCode();
            var user = await response.Content.ReadFromJsonAsync<User>().ConfigureAwait(false);
            Assert.IsNotNull(user);
            Assert.AreEqual(1, user.CompanyId);
        }

        [Test]
        public async Task GetUserById_AsUser_ShouldReturnForbidden_ForDifferentCompany()
        {
            // Arrange
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "user"),
                new Claim(ClaimTypes.Role, "User"),
                new Claim("CompanyId", "1")
            };
            var userIdentity = new ClaimsIdentity(userClaims, "TestAuthType");
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test");

            var userId = 2; // Replace with a valid user ID from a different company

            // Act
            var response = await _client.GetAsync($"/api/users/{userId}").ConfigureAwait(false);

            // Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Test]
        public async Task AddUser_AsAdmin_ShouldAddUser()
        {
            // Arrange
            var adminClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("CompanyId", "1")
            };
            var adminIdentity = new ClaimsIdentity(adminClaims, "TestAuthType");
            var adminPrincipal = new ClaimsPrincipal(adminIdentity);

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test");

            var user = new User { Username = "test", PasswordHash = "hash", Role = Role.User, CompanyId = 1 };

            // Act
            var response = await _client.PostAsJsonAsync("/api/users", user).ConfigureAwait(false);

            // Assert
            response.EnsureSuccessStatusCode();
            var createdUser = await response.Content.ReadFromJsonAsync<User>().ConfigureAwait(false);
            Assert.AreEqual(user.Username, createdUser.Username);
        }

        [Test]
        public async Task UpdateUser_AsAdmin_ShouldUpdateUser()
        {
            // Arrange
            var adminClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("CompanyId", "1")
            };
            var adminIdentity = new ClaimsIdentity(adminClaims, "TestAuthType");
            var adminPrincipal = new ClaimsPrincipal(adminIdentity);

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test");

            var user = new User { Id = 1, Username = "test", PasswordHash = "hash", Role = Role.User, CompanyId = 1 };
            var addResponse = await _client.PostAsJsonAsync("/api/users", user).ConfigureAwait(false);
            addResponse.EnsureSuccessStatusCode();

            user.Username = "updated_test";

            // Act
            var updateResponse = await _client.PutAsJsonAsync($"/api/users/{user.Id}", user).ConfigureAwait(false);

            // Assert
            updateResponse.EnsureSuccessStatusCode();
            var getResponse = await _client.GetAsync($"/api/users/{user.Id}").ConfigureAwait(false);
            getResponse.EnsureSuccessStatusCode();
            var updatedUser = await getResponse.Content.ReadFromJsonAsync<User>().ConfigureAwait(false);
            Assert.AreEqual("updated_test", updatedUser.Username);
        }

        [Test]
        public async Task DeleteUser_AsAdmin_ShouldDeleteUser()
        {
            // Arrange
            var adminClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("CompanyId", "1")
            };
            var adminIdentity = new ClaimsIdentity(adminClaims, "TestAuthType");
            var adminPrincipal = new ClaimsPrincipal(adminIdentity);

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test");

            var user = new User { Id = 1, Username = "test", PasswordHash = "hash", Role = Role.User, CompanyId = 1 };
            var addResponse = await _client.PostAsJsonAsync("/api/users", user).ConfigureAwait(false);
            addResponse.EnsureSuccessStatusCode();

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/users/{user.Id}").ConfigureAwait(false);

            // Assert
            deleteResponse.EnsureSuccessStatusCode();
            var getResponse = await _client.GetAsync($"/api/users/{user.Id}").ConfigureAwait(false);
            Assert.AreEqual(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        private class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
        {
            public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
                : base(options, logger, encoder, clock)
            {
            }

            protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, "testuser"),
                    new Claim(ClaimTypes.Role, "User"),
                    new Claim("CompanyId", "1")
                };
                var identity = new ClaimsIdentity(claims, "TestAuthType");
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, "TestAuthType");

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
        }
    }
}