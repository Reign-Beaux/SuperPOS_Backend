namespace Web.API.Middlewares;

/// <summary>
/// Middleware que agrega headers de seguridad HTTP para proteger contra ataques comunes.
/// </summary>
internal sealed class SecurityHeadersMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // X-Frame-Options: Previene ataques de Clickjacking
        // DENY = No permite que la página sea mostrada en un iframe
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // X-Content-Type-Options: Previene MIME-type sniffing
        // nosniff = El navegador debe respetar el Content-Type declarado
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // X-XSS-Protection: Habilita la protección XSS del navegador
        // 1; mode=block = Activa la protección y bloquea la página si detecta XSS
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

        // Content-Security-Policy: Define de dónde se pueden cargar recursos
        // Previene ataques XSS y de inyección de contenido
        context.Response.Headers.Append("Content-Security-Policy",
            "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data:; font-src 'self'; connect-src 'self'; frame-ancestors 'none';");

        // Referrer-Policy: Controla cuánta información del referrer se envía
        // no-referrer = No envía información del referrer
        context.Response.Headers.Append("Referrer-Policy", "no-referrer");

        // Permissions-Policy: Controla qué características del navegador están permitidas
        // Deshabilita características no necesarias para reducir superficie de ataque
        context.Response.Headers.Append("Permissions-Policy",
            "geolocation=(), microphone=(), camera=(), payment=(), usb=(), magnetometer=(), gyroscope=()");

        // Strict-Transport-Security (HSTS): Fuerza conexiones HTTPS
        // max-age=31536000 = 1 año, includeSubDomains = aplica a todos los subdominios
        // Solo se aplica si la conexión es HTTPS
        if (context.Request.IsHttps)
        {
            context.Response.Headers.Append("Strict-Transport-Security",
                "max-age=31536000; includeSubDomains");
        }

        await next(context);
    }
}
