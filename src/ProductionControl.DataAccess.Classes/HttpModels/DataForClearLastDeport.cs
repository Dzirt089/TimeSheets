using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;

namespace ProductionControl.DataAccess.Classes.HttpModels
{
	public class DataForClearLastDeport
	{
		public bool? LastAccessRightBool { get; set; }
		public string LastSelectedDepartmentID { get; set; }
		public List<EmployeeAccessRight> EmployeeAccesses { get; set; }
	}
}
