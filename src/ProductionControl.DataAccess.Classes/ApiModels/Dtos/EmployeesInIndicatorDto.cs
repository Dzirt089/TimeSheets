namespace ProductionControl.DataAccess.Classes.ApiModels.Dtos
{
	/// <summary>Показатели (данные) сотрудников на выбранный <see cref="IndicatorDto"/></summary>
	public record EmployeesInIndicatorDto
	{
		/// <summary>Выбранный показатель <see cref="IndicatorDto"/></summary>
		public IndicatorDto? IndicatorItem { get; set; }

		/// <summary>Наименование участка, для отчёта в апишке</summary>
		public string? NameDepartmentForApi { get; set; }

		/// <summary>Табельный номер сотрудника</summary>
		public long EmployeeID { get; set; }

		/// <summary>Полное имя сотрудника</summary>
		public string? FullName { get; set; }

		/// <summary>Кол-во дней</summary>
		public int CountDays { get; set; }

		/// <summary>Кол-во часов</summary>
		public double CountHours { get; set; }

		/// <summary>
		/// День показателя
		/// </summary>
		public string? Date { get; set; }
	}
}
