namespace CareerTrack.Middlware
{
    namespace CareerTrack.Middleware
    {
        public class SecurityHeadersMiddleware
        {
            private readonly RequestDelegate _next;

            public SecurityHeadersMiddleware(RequestDelegate next)
            {
                _next = next;
            }

            public async Task InvokeAsync(HttpContext context)
            {
               
                context.Response.Headers["Content-Security-Policy"] =
                    "default-src 'self'; " +
                    "script-src 'self'; " +
                    "style-src 'self'; " +
                    "img-src 'self' data:; " +
                    "font-src 'self'; " +
                    "connect-src 'self'; " +
                    "frame-ancestors 'none'; " +
                    "base-uri 'self'; " +
                    "form-action 'self'";

                context.Response.Headers["X-Content-Type-Options"] = "nosniff";

                context.Response.Headers["X-Frame-Options"] = "DENY";

                context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

                await _next(context);
            }
        }

        public static class SecurityHeadersMiddlewareExtensions
        {
            public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
            {
                return builder.UseMiddleware<SecurityHeadersMiddleware>();
            }
        }
    }
}
