using MailerVKT;

namespace ProductionControl.ServiceLayer.ServicesAPI.Interfaces
{
	public interface IErrorLogger
	{
		Task SendMailPlanLaborAsync(string message);
		Task SendMailTestAsync(List<string> path, string text);

		Task SendMailReportMonthlySummaryAsync(string path);

		Task LogErrorAsync(Exception ex, string user = "Not user", string machine = "Machine Server", string application = "API Production Control");

		void ProcessingErrorLog(Exception ex, string user = "Not user", string machine = "Machine Server", string application = "API Production Control");

		Task ProcessingErrorLogAsync(Exception ex, string user = "Not user", string machine = "Machine Server", string application = "API Production Control");
		Task ProcessingLogAsync(string v);

		Task SendMailWithOrderLunchEveryDayAsync(string v);

		Task SendMailReportAsync(string path);

		Task SendMailReportNowAsync(List<string> path, string text);

		Task SendMailReportAsync(MailParameters mail);
	}
}
