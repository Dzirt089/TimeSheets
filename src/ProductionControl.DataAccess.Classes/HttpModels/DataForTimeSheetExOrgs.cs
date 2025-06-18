using ProductionControl.DataAccess.Classes.ApiModels.Dtos;

namespace ProductionControl.DataAccess.Classes.HttpModels
{
	public class DataForTimeSheetExOrgs
	{
		public string ValueDepartmentID { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public MonthsOrYearsDto ItemMonthsTO { get; set; }
		public MonthsOrYearsDto ItemYearsTO { get; set; }
		public List<int> NoWorkDaysTO { get; set; }
		public bool FlagAllEmployeeExOrg { get; set; }
	}
}
