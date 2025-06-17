namespace ProductionControl.DataAccess.Classes.EFClasses.EmployeesExternalOrganizations
{
	public class EmployeeExOrg
	{
		public int EmployeeExOrgID { get; set; }

		/// <summary>
		/// Номер пропуска, который выдан сотруднику для прохода на территорию предприятия
		/// </summary>
		public string? CardNumber { get; set; }

		/// <summary>
		/// Номер пропуска
		/// </summary>
		public int NumberPass { get; set; }

		/// <summary>
		/// Номер категории
		/// </summary>
		public int NumCategory { get; set; }

		/// <summary>Полные ФИО сотрудника</summary>
		public string? FullName { get; set; }

		/// <summary>
		/// Связь с БД где храняться фотографии
		/// </summary>
		public EmployeePhoto? EmployeePhotos { get; set; }

		/// <summary>Сокращённое имя сотрудника (например: Сидоров С.С.)</summary>
		public string? ShortName { get; set; }

		/// <summary>Дата трудоустройства</summary>
		public DateTime DateEmployment { get; set; }

		/// <summary>Дата увольнения</summary>
		public DateTime DateDismissal { get; set; }

		/// <summary>Флаг, который обозначает что сотрудник уволен или нет</summary>
		public bool IsDismissal { get; set; }

		/// <summary>
		/// Примечание к сотруднику
		/// </summary>
		public string? Descriptions { get; set; }

		public IEnumerable<EmployeeExOrgAddInRegion> EmployeeExOrgAddInRegions { get; set; }

		public IEnumerable<ShiftDataExOrg>? ShiftDataExOrgs { get; set; }

		public EmployeeExOrg()
		{
			ShiftDataExOrgs = new HashSet<ShiftDataExOrg>();
			EmployeeExOrgAddInRegions = new HashSet<EmployeeExOrgAddInRegion>();
		}
	}
}
