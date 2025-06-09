namespace ProductionControl.DataAccess.Classes.Models.Dtos
{
	public class ResultSheetResponseDto
	{
		public List<IndicatorDto> Indicators { get; set; }
		public List<EmployeesInIndicatorDto> NNList { get; set; }
		public List<EmployeesInIndicatorDto> Underday { get; set; }
		public List<EmployeesInIndicatorDto> Overday { get; set; }
		public List<EmployeesInIndicatorDto> Night { get; set; }
		public List<EmployeesInIndicatorDto> Vacation { get; set; }
		public List<EmployeesInIndicatorDto> ADVacation { get; set; }
		public List<EmployeesInIndicatorDto> SickLeave { get; set; }
		public List<EmployeesInIndicatorDto> Demobilized { get; set; }
		public List<EmployeesInIndicatorDto> ParentalLeave { get; set; }
		public List<EmployeesInIndicatorDto> InvalidLeave { get; set; }
		public List<EmployeesInIndicatorDto> Dismissal { get; set; }
		public List<EmployeesInIndicatorDto> Lunching { get; set; }
	}
}
