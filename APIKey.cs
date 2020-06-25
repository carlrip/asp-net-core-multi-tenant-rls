using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPIWithRSL
{
    public class APIKey
    {
        private readonly RequestDelegate next;
        public APIKey(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
            if (string.IsNullOrEmpty(apiKey))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid API key");
                return;
            }
            Guid apiKeyGuid;
            if (!Guid.TryParse(apiKey, out apiKeyGuid))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid API key");
                return;
            }
            context.Items["APIKey"] = apiKeyGuid;

            await next.Invoke(context);
        }
    }

    public static class APIKeyExtension
    {
        public static IApplicationBuilder UseAPIKey(this IApplicationBuilder app)
        {
            app.UseMiddleware<APIKey>();
            return app;
        }
    }
}
