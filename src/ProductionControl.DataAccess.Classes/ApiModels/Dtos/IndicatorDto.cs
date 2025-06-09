namespace ProductionControl.DataAccess.Classes.Models.Dtos
{
	/// <summary>Показатели в иотогах табеля</summary>
	public record IndicatorDto
	{
		/// <summary>Индентификатор для облегчения внутренней работы(навигации)</summary>
		public int ShadowId { get; set; }

		/// <summary>Подробное описание показателя</summary>
		public string DescriptionIndicator { get; set; }

		/// <summary>Кол-во дней</summary>
		public int CountDays { get; set; }

		/// <summary>Кол-во часов</summary>
		public double CountHours { get; set; }

		/// <summary>Кол-во работников</summary>
		public int CountEmployeesInDepartment { get; set; }

		/// <summary>Выбранный период (месяц)</summary>
		public string SelectedMonht { get; set; }

		/// <summary>Выбранный период (год)</summary>
		public string SelectedYear { get; set; }

		/// <summary>Выбранный участок</summary>
		public string SelectedDepartment { get; set; }
	}
}
