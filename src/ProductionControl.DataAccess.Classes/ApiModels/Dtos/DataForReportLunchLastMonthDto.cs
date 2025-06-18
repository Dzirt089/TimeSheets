namespace ProductionControl.DataAccess.Classes.ApiModels.Dtos
{
	public record DataForReportLunchLastMonthDto
	{
		/// <summary>
		/// Табельный номер сотрудника
		/// </summary>
		public long EmployeeId { get; set; }
		/// <summary>
		/// Сокращенные ФИО сотрудника
		/// </summary>
		public string ShortName { get; set; }
		/// <summary>
		/// Кол-во заказанных на сотрудника обедов за месяц
		/// </summary>
		public int CountLunch { get; set; }
		/// <summary>
		/// Общая сумма на все обеды за месяц
		/// </summary>
		public decimal TotalSum { get; set; }
		/// <summary>
		/// Общее число заказанных обедов за месяц
		/// </summary>
		public int TotalCountLunch { get; set; }
		/// <summary>
		/// Средняя цена на один обед сотрудника
		/// </summary>
		public decimal AverageAmount { get; set; }
		/// <summary>
		/// Дата начала периода месяца. Необходима для загрузки в ИС-ПРО
		/// </summary>
		public DateTime StartDate { get; set; }
	}
}
