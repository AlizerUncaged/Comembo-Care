namespace Hospital_Management.Middlewares;

public class Surveyor
{
    private readonly RequestDelegate _next;

    public Surveyor(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);
    }
}