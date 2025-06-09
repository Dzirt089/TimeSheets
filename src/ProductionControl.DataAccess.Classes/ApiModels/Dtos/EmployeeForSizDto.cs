using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;

namespace ProductionControl.DataAccess.Classes.ApiModels.Dtos
{
	public class EmployeeForSizDto
	{
		/// <summary>Табельный номер сотрудника</summary>
		public long EmployeeID { get; set; }

		/// <summary>Сокращенное ФИО сотрудника</summary>
		public string? ShortName { get; set; }

		/// <summary>Номер участка (его ID)</summary>
		public string? DepartmentID { get; set; }

		/// <summary>Наименование участка</summary>
		public string? NameDepartnent { get; set; }

		/// <summary>Номер графика работы сотрудника</summary>
		public string? NumGraf { get; set; }

		/// <summary>Норма месяца работы в часах</summary>
		public double HoursPlanMonht { get; set; }

		/// <summary>Фактически отработанное время в течениии месяца</summary>
		public double HoursWorkinfFact { get; set; }

		/// <summary>Данные по одному экземпляру СИЗ-а</summary>
		public DataSizsForSizDto? DataSizsForSizs { get; set; }

		/// <summary>Данные для номера ведомости: с порядковым номером, месяцем и годом.</summary>
		public OrderNumberOnDate MonthlyValue { get; set; } = new();
	}
}
