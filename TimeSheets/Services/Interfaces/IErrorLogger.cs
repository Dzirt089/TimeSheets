namespace TimeSheets.Services.Interfaces
{
	public interface IErrorLogger
	{
		Task LogErrorAsync(Exception ex,
			string user = "User",
			string machine = "Machine",
			string application = "Production Control");
		void ProcessingErrorLog(Exception ex,
			string user = "User",
			string machine = "Machine",
			string application = "Production Control");
		Task ProcessingErrorLogAsync(Exception ex,
			string user = "User",
			string machine = "Machine",
			string application = "Production Control");
	}
}
