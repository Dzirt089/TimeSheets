using CommunityToolkit.Mvvm.ComponentModel;

namespace ProductionControl.Models.Dtos.EmployeesFactory
{
	public class EmployeeAccessRightDto : ObservableObject
	{
		/// <summary>
		/// Обычный индентификатор
		/// </summary>
		public int EmployeeAccessRightId
		{
			get => _employeeAccessRightId;
			set => SetProperty(ref _employeeAccessRightId, value);
		}
		private int _employeeAccessRightId;

		/// <summary>
		/// Имя копьютера
		/// </summary>
		public string? NameUsers
		{
			get => _nameUsers;
			set => SetProperty(ref _nameUsers, value);
		}
		private string? _nameUsers;

		/// <summary>
		/// Имя сотрудника, за которым закреплён компьютер
		/// </summary>
		public string? NamePeople
		{
			get => _namePeople;
			set => SetProperty(ref _namePeople, value);
		}
		private string? _namePeople;

		/// <summary>Номер участка, где работает(закреплен) сотрудник из <see cref="DepartmentProductionDto"/></summary>
		public string DepartmentID
		{
			get => _departmentID;
			set => SetProperty(ref _departmentID, value);
		}
		private string _departmentID;

		/// <summary>
		/// Навигационное св-во для связи с другой сущностью из <see cref="DepartmentProductionDto"/>. 
		/// Нужно загружать данные только в одном направлении:
		/// из прав доступа <see cref="EmployeeAccessRightDto"/> к департаментам <see cref="DepartmentProductionDto"/>., а не наоборот.
		/// Обязательно указываем внешний ключ (это первичный ключ из <see cref="DepartmentProductionDto"/>)
		/// </summary>
		public DepartmentProductionDto DepartmentProduction
		{
			get => _departmentProduction;
			set => SetProperty(ref _departmentProduction, value);
		}
		private DepartmentProductionDto _departmentProduction;

		/// <summary>
		/// Право на редактирование (есть\нет)
		/// </summary>
		public bool? RightEditOrSee
		{
			get => _rightEditOrSee;
			set => SetProperty(ref _rightEditOrSee, value);
		}
		private bool? _rightEditOrSee;
	}
}
