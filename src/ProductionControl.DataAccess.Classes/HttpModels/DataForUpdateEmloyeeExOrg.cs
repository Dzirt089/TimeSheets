using ProductionControl.DataAccess.Classes.EFClasses.EmployeesExternalOrganizations;

namespace ProductionControl.DataAccess.Classes.HttpModels
{
	public class DataForUpdateEmloyeeExOrg
	{
		public EmployeeExOrg ExOrg { get; set; }
		public string ValueDepId { get; set; }
		public bool AddWorkInReg { get; set; }
	}
}
