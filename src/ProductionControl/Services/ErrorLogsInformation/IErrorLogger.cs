namespace ProductionControl.Services.ErrorLogsInformation
{
	public interface IErrorLogger
	{
		Task LogErrorAsync(Exception ex);

		void ProcessingErrorLog(Exception ex);

		Task ProcessingErrorLogAsync(Exception ex);

		Task SendMailPlanLaborAsync(string message);
	}
}
