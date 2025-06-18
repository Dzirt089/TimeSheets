namespace ProductionControl.DataAccess.Classes.EFClasses.EmployeesExternalOrganizations
{
	public class EmployeeExOrgAddInRegion
	{
		public int EmployeeExOrgID { get; set; }

		public EmployeeExOrg EmployeeExOrg { get; set; }

		public string DepartmentID { get; set; }

		public bool WorkingInTimeSheetEmployeeExOrg { get; set; }
	}
}
