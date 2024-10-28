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
    public class AuthControllerTests
    {
        private HttpClient _client;
        private Mock<ILogger<AuthController>> _loggerMock;

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
            _loggerMock = new Mock<ILogger<AuthController>>();
        }

        [Test]
        public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // Arrange
            var loginRequest = new LoginRequest { Username = "test", Password = "password" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest).ConfigureAwait(false);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<dynamic>().ConfigureAwait(false);
            Assert.IsNotNull(result.Token);
        }

        [Test]
        public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
        {
            // Arrange
            var loginRequest = new LoginRequest { Username = "invalid", Password = "password" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
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