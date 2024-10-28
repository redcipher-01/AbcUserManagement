using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Security.Claims;
using System.Threading.Tasks;
using AbcUserManagement.Middleware;

namespace AbcUserManagement.UnitTests
{
    [TestFixture]
    public class ThrottlingMiddlewareTests
    {
        private Mock<RequestDelegate> _nextMock;
        private Mock<ILogger<ThrottlingMiddleware>> _loggerMock;
        private ThrottlingMiddleware _middleware;

        [SetUp]
        public void SetUp()
        {
            _nextMock = new Mock<RequestDelegate>();
            _loggerMock = new Mock<ILogger<ThrottlingMiddleware>>();
            _middleware = new ThrottlingMiddleware(_nextMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task InvokeAsync_AllowsRequestsUnderLimit()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "testuser")
            }));

            _nextMock.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            // Act
            for (int i = 0; i < 10; i++)
            {
                await _middleware.InvokeAsync(context).ConfigureAwait(false);
            }

            // Assert
            Assert.AreEqual(StatusCodes.Status200OK, context.Response.StatusCode);
            _nextMock.Verify(next => next(It.IsAny<HttpContext>()), Times.Exactly(10));
        }

        [Test]
        public async Task InvokeAsync_ReturnsTooManyRequests_WhenLimitExceeded()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "testuser")
            }));

            _nextMock.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            // Act
            for (int i = 0; i < 10; i++)
            {
                await _middleware.InvokeAsync(context).ConfigureAwait(false);
            }

            await _middleware.InvokeAsync(context).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);
            _nextMock.Verify(next => next(It.IsAny<HttpContext>()), Times.Exactly(10));
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("User testuser has exceeded the request limit.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Test]
        public async Task InvokeAsync_AllowsRequestsForDifferentUsers()
        {
            // Arrange
            var context1 = new DefaultHttpContext();
            context1.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user1")
            }));

            var context2 = new DefaultHttpContext();
            context2.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user2")
            }));

            _nextMock.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            // Act
            for (int i = 0; i < 10; i++)
            {
                await _middleware.InvokeAsync(context1).ConfigureAwait(false);
                await _middleware.InvokeAsync(context2).ConfigureAwait(false);
            }

            // Assert
            Assert.AreEqual(StatusCodes.Status200OK, context1.Response.StatusCode);
            Assert.AreEqual(StatusCodes.Status200OK, context2.Response.StatusCode);
            _nextMock.Verify(next => next(It.IsAny<HttpContext>()), Times.Exactly(20));
        }

        [Test]
        public async Task InvokeAsync_AllowsRequestsForAnonymousUsers()
        {
            // Arrange
            var context = new DefaultHttpContext();

            _nextMock.Setup(next => next(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            // Act
            for (int i = 0; i < 10; i++)
            {
                await _middleware.InvokeAsync(context).ConfigureAwait(false);
            }

            // Assert
            Assert.AreEqual(StatusCodes.Status200OK, context.Response.StatusCode);
            _nextMock.Verify(next => next(It.IsAny<HttpContext>()), Times.Exactly(10));
        }
    }
}