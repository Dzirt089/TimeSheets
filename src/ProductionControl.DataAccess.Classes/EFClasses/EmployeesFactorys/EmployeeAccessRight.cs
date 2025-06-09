namespace ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys
{
	public class EmployeeAccessRight
	{
		/// <summary>
		/// Обычный индентификатор
		/// </summary>
		public int EmployeeAccessRightId { get; set; }

		/// <summary>
		/// Имя копьютера
		/// </summary>
		public string? NameUsers { get; set; }

		/// <summary>
		/// Имя сотрудника, за которым закреплён компьютер
		/// </summary>
		public string? NamePeople { get; set; }

		/// <summary>Номер участка, где работает(закреплен) сотрудник из <see cref="EmployeesFactorys.DepartmentProduction"/></summary>
		public string DepartmentID { get; set; }

		/// <summary>
		/// Навигационное св-во для связи с другой сущностью из <see cref="EmployeesFactorys.DepartmentProduction"/>. 
		/// Нужно загружать данные только в одном направлении:
		/// из прав доступа <see cref="EmployeeAccessRight"/> к департаментам <see cref="EmployeesFactorys.DepartmentProduction"/>., а не наоборот.
		/// Обязательно указываем внешний ключ (это первичный ключ из <see cref="EmployeesFactorys.DepartmentProduction"/>)
		/// </summary>
		public DepartmentProduction DepartmentProduction { get; set; }

		/// <summary>
		/// Право на редактирование (есть\нет)
		/// </summary>
		public bool? RightEditOrSee { get; set; }
	}
}
