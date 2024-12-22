namespace TimeSheets.Entitys
{
	/// <summary>
	/// Содержит имена сотрудника и его компьютера
	/// </summary>
	public class LocalUserData
	{
		/// <summary>
		/// Сокращенное ФИО сотрудника
		/// </summary>
		public string UserName { get; set; } = string.Empty;

		/// <summary>
		/// Имя компьютера, на которой работает сотрудник
		/// </summary>
		public string MachineName { get; set; } = string.Empty;
	}
}
