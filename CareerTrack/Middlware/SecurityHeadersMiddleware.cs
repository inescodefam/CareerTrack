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

                context.Response.Headers.ContentSecurityPolicy =
                    "default-src 'self'; " +
                    "script-src 'self'; " +
                    "style-src 'self' https://cdn.jsdelivr.net; " +
                    "img-src 'self' data:; " +
                    "font-src 'self' https://cdn.jsdelivr.net; " +
                    "connect-src 'self'; " +
                    "frame-ancestors 'none'; " +
                    "base-uri 'self'; " +
                    "form-action 'self'";

                context.Response.Headers.XContentTypeOptions = "nosniff";

                context.Response.Headers.XFrameOptions = "DENY";

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
