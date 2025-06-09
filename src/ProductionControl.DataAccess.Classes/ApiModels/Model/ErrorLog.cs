namespace ProductionControl.DataAccess.Classes.Models.Model
{
	public class ErrorLog
	{
		public int Id { get; set; }
		public DateTime Timestamp { get; set; }
		public string? Message { get; set; }
		public string? StackTrace { get; set; }
		public string? Source { get; set; }
		public string? InnerException { get; set; }
		public string? User { get; set; }
		public string? Machine { get; set; }
		public string? Application { get; set; }
	}
}
