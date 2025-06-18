namespace ProductionControl.DataAccess.Classes.ApiModels.Dtos
{
	public class EmployeeCardNumShortNameId
	{
		public long EmployeeID { get; set; }

		/// <summary>
		/// Номер пропуска, который выдан сотруднику для прохода на территорию предприятия
		/// </summary>
		public string? CardNumber { get; set; }

		/// <summary>Сокращённое имя сотрудника (например: Сидоров С.С.)</summary>
		public string? ShortName { get; set; }
	}
}
