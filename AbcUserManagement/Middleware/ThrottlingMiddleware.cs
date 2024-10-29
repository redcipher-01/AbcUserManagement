using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AbcUserManagement.Middleware
{
    public class ThrottlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ThrottlingMiddleware> _logger;
        private static readonly ConcurrentDictionary<string, (DateTime Timestamp, int Count)> _requests = new();

        public ThrottlingMiddleware(RequestDelegate next, ILogger<ThrottlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var userId = context.User.FindFirst("UserId")?.Value;

            if (userId != null)
            {
                _logger.LogInformation("Processing request for user: {UserId}", userId);

                if (_requests.TryGetValue(userId, out var requestInfo))
                {
                    var (timestamp, count) = requestInfo;
                    if (timestamp.AddMinutes(1) > DateTime.UtcNow)
                    {
                        if (count >= 10)
                        {
                            _logger.LogWarning("User {UserId} has exceeded the request limit.", userId);
                            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                            await context.Response.WriteAsync("Too many requests. Please try again later.");
                            return;
                        }
                        _requests[userId] = (timestamp, count + 1);
                    }
                    else
                    {
                        _requests[userId] = (DateTime.UtcNow, 1);
                    }
                }
                else
                {
                    _requests[userId] = (DateTime.UtcNow, 1);
                }
            }

            await _next(context).ConfigureAwait(false);
        }
    }
}