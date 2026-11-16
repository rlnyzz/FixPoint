using System.Net;
using System.Text.Json;

namespace FixPoint.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Произошло необработанное исключение: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        // По умолчанию отдаем 500
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        // Формируем ответ
        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = "Внутренняя ошибка сервера. Пожалуйста, попробуйте позже.",
            Detailed = exception.Message // В продакшене это поле лучше скрыть!
        };

        // В зависимости от типа исключения можно менять код ответа
        // Например, если это кастомное исключение бизнес-логики
        if (exception is ArgumentException || exception is InvalidOperationException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = exception.Message,
                Detailed = (string?)null
            };
        }

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}