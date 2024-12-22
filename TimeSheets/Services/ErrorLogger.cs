using Microsoft.EntityFrameworkCore;

using TimeSheets.DAL;
using TimeSheets.Models;
using TimeSheets.Services.Interfaces;

using System.Windows;

namespace TimeSheets.Services
{
	/// <summary>
	/// Класс для логгирования ошибок в БД и дублирование в письме
	/// </summary>
	/// <param name="context">Фабрика dbContect EF Core</param>
	/// <param name="mailService">Сервис по отправке писем, сконфигурирорван под предприятие</param>
	public class ErrorLogger(
		IDbContextFactory<ShiftTimesDbContext> context) : IErrorLogger
	{

		private readonly IDbContextFactory<ShiftTimesDbContext> _context = context;

		/// <summary>
		/// Составляем логи на основе переданного исключения
		/// </summary>
		public async Task LogErrorAsync(Exception ex,
			string user = "User",
			string machine = "Machine",
			string application = "Production Control")
		{
			using var dbContext = await _context.CreateDbContextAsync();

			var errorLog = new ErrorLog
			{
				Timestamp = DateTime.Now,
				Message = ex.Message,
				StackTrace = ex.StackTrace,
				Source = ex.Source,
				InnerException = ex.InnerException?.ToString(),
				User = user,
				Machine = machine,
				Application = application
			};
			dbContext.ErrorLogs?.Add(errorLog);
			await dbContext.SaveChangesAsync();			
		}

		/// <summary>
		/// Синхронная версия метода по составлению лога на основе переданного исключения
		/// </summary>
		public void ProcessingErrorLog(
			Exception ex,
			string user = "User",
			string machine = "Machine",
			string application = "Production Control")
		{
			LogErrorAsync(ex, user, machine, application).GetAwaiter().GetResult();
			MessageBox.Show("Произошла ошибка в работе программы. Пожалуйста, обратитесь в Тех.Отдел к разработчикам.");
		}

		/// <summary>
		/// Асинхронная версия метода по составлению лога на основе переданного исключения
		/// </summary>
		public async Task ProcessingErrorLogAsync(
			Exception ex,
			string user = "User",
			string machine = "Machine",
			string application = "Production Control")
		{
			await LogErrorAsync(ex, user, machine, application);
			MessageBox.Show("Произошла ошибка в работе программы. Пожалуйста, обратитесь в Тех.Отдел к разработчикам.");
		}				
	}
}
