namespace ProductionControl.DataAccess.Classes.ApiModels.Dtos
{
	/// <summary>
	/// Класс, который отображает краткие сведения о сотруднике в Табеле: ФИО, смена, переработка
	/// </summary>
	public class ShiftDataEmployeeDto
	{
		/// <summary>
		/// Короткое ФИО сотрудника
		/// </summary>
		public string ShortName { get; set; }

		/// <summary>
		/// Смена
		/// </summary>
		public string NameShift { get; set; }
		/// <summary>
		/// Переработка
		/// </summary>
		public string NameOverday { get; set; }
	}
}
