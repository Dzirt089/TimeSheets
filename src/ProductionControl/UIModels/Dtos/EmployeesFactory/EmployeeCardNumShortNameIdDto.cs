using CommunityToolkit.Mvvm.ComponentModel;

namespace ProductionControl.UIModels.Dtos.EmployeesFactory
{
	public class EmployeeCardNumShortNameIdDto : ObservableObject
	{
		public long EmployeeID { get => _employeeID; set => SetProperty(ref _employeeID, value); }
		private long _employeeID;

		/// <summary>
		/// Номер пропуска, который выдан сотруднику для прохода на территорию предприятия
		/// </summary>
		public string? CardNumber { get => _cardNumberg; set => SetProperty(ref _cardNumberg, value); }
		private string? _cardNumberg;

		/// <summary>Сокращённое имя сотрудника (например: Сидоров С.С.)</summary>
		public string? ShortName { get => _shortName; set => SetProperty(ref _shortName, value); }
		private string? _shortName;
	}
}
