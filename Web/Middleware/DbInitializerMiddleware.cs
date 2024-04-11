using Microsoft.EntityFrameworkCore;
using Repository;

namespace Web.Middleware {
    public class DbInitializerMiddleware {
        private readonly RequestDelegate _next;

        public DbInitializerMiddleware(RequestDelegate next) {
            _next = next;

        }

        public Task Invoke(HttpContext httpContext, UrlShortnerContext context) {
            if (!(httpContext.Session.Keys.Contains("starting"))) {
                context.Database.EnsureCreated();
                context.Database.Migrate();
                httpContext.Session.SetString("starting", "Yes");
            }
            return _next.Invoke(httpContext);

        }
    }
    public static class DbInitializerMiddlewareExtensions {
        public static IApplicationBuilder UseDbInitializerMiddleware(this IApplicationBuilder builder) {
            return builder.UseMiddleware<DbInitializerMiddleware>();
        }
    }
}
