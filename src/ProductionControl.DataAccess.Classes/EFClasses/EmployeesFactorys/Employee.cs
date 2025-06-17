using ProductionControl.DataAccess.Classes.EFClasses.Sizs;

namespace ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys
{
	/// <summary>
	/// Модель класса, описывающая сотрудника.
	/// </summary>
	public class Employee
	{
		public long EmployeeID { get; set; }

		/// <summary>
		/// Номер пропуска, который выдан сотруднику для прохода на территорию предприятия
		/// </summary>
		public string? CardNumber { get; set; }

		/// <summary>Полные ФИО сотрудника</summary>
		public string? FullName { get; set; }

		/// <summary>Сокращённое имя сотрудника (например: Сидоров С.С.)</summary>
		public string? ShortName { get; set; }

		/// <summary>Номер участка, где работает(закреплен) сотрудник</summary>
		public string DepartmentID { get; set; }

		public DepartmentProduction? DepartmentProduction { get; set; }

		/// <summary>Номер графика работы, который закрепили за сотрудником</summary>
		public string? NumGraf { get; set; }

		/// <summary>Дата трудоустройства</summary>
		public DateTime DateEmployment { get; set; }

		/// <summary>Дата увольнения</summary>
		public DateTime DateDismissal { get; set; }

		/// <summary>Флаг, который обозначает что сотрудник уволен или нет</summary>
		public bool IsDismissal { get; set; }

		/// <summary>Флаг, который обозначает что сотрудник обедает на производстве или нет. Для того, чтобы заказывать на него обед\ужин или нет</summary>
		public bool IsLunch { get; set; }

		/// <summary>ID нормы выдачи СИЗ-ов</summary>
		public int? UsageNormID { get; set; }

		/// <summary>
		/// Норма выдачи СИЗ
		/// </summary>		
		public UsageNorm? UsageNorm { get; set; }

		public IEnumerable<ShiftData>? Shifts { get; set; }

		public Employee()
		{
			// чтобы разработчики могли добавлять продукты в категорию,
			// мы должны инициализировать свойство навигации в пустую коллекцию
			Shifts = new HashSet<ShiftData>();
		}
	}
}
