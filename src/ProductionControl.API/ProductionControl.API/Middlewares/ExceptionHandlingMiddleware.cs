using Microsoft.AspNetCore.Mvc;

using ProductionControl.ServiceLayer.ServicesAPI.Interfaces;

using System.ComponentModel.DataAnnotations;

namespace ProductionControl.API.Middlewares
{
	public class ExceptionHandlingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<ExceptionHandlingMiddleware> _logger;
		private readonly IHostEnvironment _env;

		public ExceptionHandlingMiddleware(
			RequestDelegate next,
			ILogger<ExceptionHandlingMiddleware> logger,
			IHostEnvironment env)
		{
			_next = next;
			_logger = logger;
			_env = env;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unhandled exception");
				var errorLogger = context.RequestServices.GetRequiredService<IErrorLogger>();
				await errorLogger.LogErrorAsync(ex, "Обработка исключений в промежуточном программном обеспечении (Middleware)");
				await HandleExceptionAsync(context, ex);
			}
		}

		private async Task HandleExceptionAsync(HttpContext context, Exception exception)
		{
			// Логируем с контекстом запроса
			_logger.LogError(exception, "Request failed: {Method} {Path}",
				context.Request.Method, context.Request.Path);

			// Защита от записи в уже начатый ответ
			if (context.Response.HasStarted)
			{
				_logger.LogWarning("Cannot write error response - response already started");
				return;
			}

			// Определяем статус код
			var statusCode = exception switch
			{
				ArgumentNullException => StatusCodes.Status400BadRequest,
				ValidationException => StatusCodes.Status422UnprocessableEntity,
				UnauthorizedAccessException => StatusCodes.Status403Forbidden,
				_ => StatusCodes.Status500InternalServerError
			};

			// Формируем объект ошибки
			var problem = new ProblemDetails
			{
				Title = "An error occurred",
				Status = statusCode,
				Instance = context.Request.Path,
				Extensions = { ["traceId"] = context.TraceIdentifier } // Для корреляции
			};

			// Детали в зависимости от среды
			if (_env.IsDevelopment())
			{
				problem.Detail = exception.ToString();
				problem.Extensions["stack"] = exception.StackTrace; // Отдельно стек
			}
			else
			{
				problem.Detail = "Please contact support";
			}

			// Дополнительные расширения для специфических исключений
			if (exception is ValidationException validationEx)
			{
				problem.Extensions["errors"] = validationEx.ValidationResult.ErrorMessage; // Детали валидации
			}

			// Записываем ответ
			context.Response.ContentType = "application/problem+json";
			context.Response.StatusCode = statusCode;
			await context.Response.WriteAsJsonAsync(problem);
		}
	}
}
