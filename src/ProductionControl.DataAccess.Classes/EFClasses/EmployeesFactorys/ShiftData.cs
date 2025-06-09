namespace ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys
{
	/// <summary>
	/// Модель учёта времени отработанного на производстве. Включает в себя часы в смене, тип смены, время переработки\недоработки
	/// </summary>
	public class ShiftData
	{
		/// <summary>Табельный номер сотрудника</summary>
		public long EmployeeID { get; set; }

		/// <summary>Навигационное сво-во, сотрудник. Для связи с <see cref="EmployeesFactorys.Employee"/> один-ко-многим</summary>
		public Employee Employee { get; set; }

		/// <summary>
		/// Дата табеля
		/// </summary>
		public DateTime WorkDate { get; set; }

		/// <summary>
		/// Часы отработанные в смене, вкл переработки
		/// </summary>
		public string? Hours { get; set; }

		/// <summary>
		/// Тип смены на производстве
		/// </summary>
		public string? Shift { get; set; }

		/// <summary>
		/// Время переработки\недоработки
		/// </summary>
		public string? Overday { get; set; }

		/// <summary>Флаг, который обозначает что сотрудник отобедал в этот день на производстве или нет.</summary>
		public bool IsHaveLunch { get; set; }

		/// <summary>Флаг, который обозначает что этот день предпраздничный нет. Если да, то день на час короче. Инфа берется в апишке из ис-про</summary>
		public bool IsPreHoliday { get; set; }
	}
}
