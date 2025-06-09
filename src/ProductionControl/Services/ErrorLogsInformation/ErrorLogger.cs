using ProductionControl.Services.Mail;
using ProductionControl.UIModels.Model.GlobalPropertys;

namespace ProductionControl.Services.ErrorLogsInformation
{
	/// <summary>
	/// Класс для логгирования ошибок в БД и дублирование в письме
	/// </summary>
	/// <param name="mailService">Сервис по отправке писем, сконфигурирорван под предприятие</param>
	public class ErrorLogger(MailService mailService, GlobalEmployeeSessionInfo userData) : IErrorLogger
	{

		/// <summary>
		/// Составляем логи на основе переданного исключения
		/// </summary>
		public async Task LogErrorAsync(Exception ex)
		{
			//Текст письма формируем сразу, иначе не читаемая каша в нём
			await mailService.SendMailAsync(new MailerVKT.MailParameters
			{
				Text = @$"
<pre>
У пользователя: {userData.UserName}, комп.: {userData.MachineName}.

Сводка об ошибке: 

Message: {ex.Message}.


StackTrace: {ex.StackTrace}.


Source: {ex.Source}.


InnerException: {ex?.InnerException}.
</pre>",
				Recipients = ["teho19@vkt-vent.ru"],
				RecipientsBcc = ["progto@vkt-vent.ru"],
				Subject = "Errors in Production Control",
				SenderName = "Production Control",
			});

		}

		/// <summary>
		/// Синхронная версия метода по составлению лога на основе переданного исключения
		/// </summary>
		public void ProcessingErrorLog(Exception ex)
		{
			LogErrorAsync(ex).ConfigureAwait(false);
		}

		/// <summary>
		/// Асинхронная версия метода по составлению лога на основе переданного исключения
		/// </summary>
		public async Task ProcessingErrorLogAsync(Exception ex)
		{
			await LogErrorAsync(ex);
		}

		public async Task SendMailPlanLaborAsync(string message)
		{

			await mailService.SendMailAsync(new MailerVKT.MailParameters
			{
				Recipients = ["ceh06@vkt-vent.ru", "teho19@vkt-vent.ru"],
				RecipientsBcc = ["progto@vkt-vent.ru"],
				SenderName = "Табель",
				Subject = "Плановая трудоемкость",
				Text = message,
			});
		}
	}
}
