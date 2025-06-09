using MailerVKT;

using ProductionControl.DataAccess.EntityFramework.DbContexts;
using ProductionControl.ServiceLayer.Mail;
using ProductionControl.ServiceLayer.ServicesAPI.Interfaces;

namespace ProductionControl.ServiceLayer.ServicesAPI.Implementation
{
	public class ErrorLogger(
		MailService mailService,
		ProductionControlDbContext dbContext)
		: IErrorLogger
	{
		/// <summary>
		/// Составляем логи на основе переданного исключения
		/// </summary>
		public async Task LogErrorAsync(Exception ex,
			string user = "Not user",
			string machine = "Machine Server",
			string application = "API Production Control")
		{

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

			//Текст письма формируем сразу, иначе не читаемая каша в нём
			await mailService.SendMailAsync(new MailParameters
			{
				Text = @$"
<pre>
У пользователя: {user}, комп.: {machine}.

Сводка об ошибке: 

Message: {ex.Message}.


StackTrace: {ex.StackTrace}.


Source: {ex.Source}.


InnerException: {ex?.InnerException}.
</pre>",
				Recipients = ["teho19@vkt-vent.ru"],
				Subject = "Errors in Production Control",
				SenderName = "API Production Control",
			});
		}

		/// <summary>
		/// Синхронная версия метода по составлению лога на основе переданного исключения
		/// </summary>
		public void ProcessingErrorLog(
			Exception ex,
			string user = "Not user",
			string machine = "Machine Server",
			string application = "API Production Control")
		{
			LogErrorAsync(ex, user, machine, application).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Асинхронная версия метода по составлению лога на основе переданного исключения
		/// </summary>
		public async Task ProcessingErrorLogAsync(
			Exception ex,
			string user = "Not user",
			string machine = "Machine Server",
			string application = "API Production Control")
		{
			await LogErrorAsync(ex, user, machine, application);
		}
		/// <summary>
		/// Метод для ведения информационных сообщений.
		/// </summary>
		/// <param name="v">Текст информации для письма</param>
		/// <returns></returns>
		public async Task ProcessingLogAsync(string v)
		{
			await mailService.SendMailAsync(new MailParameters
			{
				Recipients = ["teho19@vkt-vent.ru"],
				//RecipientsBcc = ["progto@vkt-vent.ru"],
				Subject = "Logging in Production Control",
				SenderName = "API Production Control",
				Text = v,
			});
		}
		/// <summary>
		/// Метод по отправке отчёта Excel по почте
		/// </summary>
		/// <param name="path">Путь к файлу</param>
		public async Task SendMailReportAsync(string path)
		{
			await mailService.SendMailAsync(new MailParameters
			{
				Recipients = ["teho19@vkt-vent.ru"/*, "ok@vkt-vent.ru"*/],
				//RecipientsBcc = ["progto@vkt-vent.ru"],
				Subject = "Отчёт по обедам Поляны за прошлый месяц",
				SenderName = "Табель",
				Attachs = [path],
			});
		}

		/// <summary>
		/// Метод по отправке отчётов Excel по почте
		/// </summary>
		/// <param name="path">Путь к файлу</param>
		public async Task SendMailReportNowAsync(List<string> path, string text)
		{
			await mailService.SendMailAsync(new MailParameters
			{
				Recipients = ["teho19@vkt-vent.ru"/*, "ok@vkt-vent.ru", "sveta@vkt.cc"*/],
				//RecipientsBcc = ["progto@vkt-vent.ru"],
				Subject = $"Отчёт по обедам Поляны за {DateTime.Now.Date:d}",
				SenderName = "Табель",
				Text = text,
				Attachs = path,
			});
		}

		/// <summary>
		/// Метод по отправке отчётов Excel по почте
		/// </summary>
		/// <param name="path">Путь к файлу</param>
		public async Task SendMailReportMonthlySummaryAsync(string path)
		{
			await mailService.SendMailAsync(new MailParameters
			{
				Recipients = ["teho19@vkt-vent.ru"/*, "ok@vkt-vent.ru", "sveta@vkt.cc"*/],
				//RecipientsBcc = ["progto@vkt-vent.ru"],
				Subject = $"Сводная таблица",
				SenderName = "Табель",
				Attachs = [path],
			});
		}

		/// <summary>
		/// Метод по отправке отчёта Excel по почте
		/// </summary>
		/// <param name="path">Путь к файлу</param>
		public async Task SendMailReportAsync(MailParameters mail)
		{
			await mailService.SendMailAsync(mail);
		}
		/// <summary>
		/// Метод по отправке заказа обедов по почте в письме
		/// </summary>
		/// <param name="v">Заказ обедов</param>
		public async Task SendMailWithOrderLunchEveryDayAsync(string v)
		{
			await mailService.SendMailAsync(new MailParameters
			{
				Recipients = ["teho19@vkt-vent.ru"/*, "ok@vkt-vent.ru", "sveta@vkt.cc"*/],
				//RecipientsBcc = ["progto@vkt-vent.ru"],
				Subject = $"Отчёт по обедам Поляны за {DateTime.Now.Date:d}",
				SenderName = "Табель",
				Text = v,
			});
		}

		/// <summary>
		/// Метод по отправке отчётов Excel по почте
		/// </summary>
		/// <param name="path">Путь к файлу</param>
		public async Task SendMailTestAsync(List<string> path, string text)
		{
			await mailService.SendMailAsync(new MailParameters
			{
				Recipients = ["teho19@vkt-vent.ru"/*, "ok@vkt-vent.ru"*/],
				//	RecipientsCopy = ["ceh07@vkt-vent.ru"],
				//	RecipientsBcc = ["progto@vkt-vent.ru"],
				Subject = $"Отчёт по сотрудникам СО",
				SenderName = "Табель",
				Text = text,
				Attachs = path,
			});
		}
	}
}
