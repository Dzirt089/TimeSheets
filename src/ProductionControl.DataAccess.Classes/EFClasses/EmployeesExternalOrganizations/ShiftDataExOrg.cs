namespace ProductionControl.DataAccess.Classes.EFClasses.EmployeesExternalOrganizations
{
	public class ShiftDataExOrg
	{
		public int EmployeeExOrgID { get; set; }

		/// <summary>
		/// Дата табеля
		/// </summary>
		public DateTime WorkDate { get; set; }

		public string DepartmentID { get; set; }

		/// <summary>
		/// Часы отработанные в смене, вкл переработки
		/// </summary>
		public string? Hours { get; set; }

		public byte? CodeColor { get; set; }

		public EmployeeExOrg EmployeeExOrg { get; set; }

	}
}
