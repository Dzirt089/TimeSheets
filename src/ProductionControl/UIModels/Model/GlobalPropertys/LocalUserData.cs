namespace ProductionControl.Models.Entitys.GlobalPropertys
{
	/// <summary>
	/// Содержит имена сотрудника и его компьютера
	/// </summary>
	public class LocalUserData
	{
		/// <summary>
		/// Сокращенное ФИО сотрудника
		/// </summary>
		public string NameEmployee { get; set; } = string.Empty;

		/// <summary>
		/// Имя компьютера, на которой работает сотрудник
		/// </summary>
		public string UserName { get; set; } = Environment.UserName;

		/// <summary>
		/// Имя компьютера, на которой работает сотрудник
		/// </summary>
		public string MachineName { get; set; } = Environment.MachineName;

		public string ApplicationName { get; set; } = "Production Control";
	}
}
