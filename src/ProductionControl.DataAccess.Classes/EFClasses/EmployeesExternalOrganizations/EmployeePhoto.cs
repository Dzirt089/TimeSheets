namespace ProductionControl.DataAccess.Classes.EFClasses.EmployeesExternalOrganizations
{
	public class EmployeePhoto
	{
		public int EmployeeExOrgID { get; set; }

		/// <summary>
		/// Фотографии людей
		/// </summary>
		public byte[]? Photo { get; set; }

		/// <summary>
		/// Связь с сотрудниками СО
		/// </summary>
		public EmployeeExOrg? EmployeeExOrg { get; set; }
	}
}
