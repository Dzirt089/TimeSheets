using ProductionControl.DataAccess.Classes.ApiModels.Dtos;
using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;

namespace ProductionControl.DataAccess.Classes.HttpModels
{
	public class DataForTimeSheet
	{
		public DepartmentProduction NamesDepartmentItem { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public MonthsOrYearsDto ItemMonthsTO { get; set; }
		public MonthsOrYearsDto ItemYearsTO { get; set; }
		public List<int> NoWorkDaysTO { get; set; }
		public bool CheckingSeeOrWriteBool { get; set; }
	}
}
