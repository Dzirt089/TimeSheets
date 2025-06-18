using CommunityToolkit.Mvvm.ComponentModel;

namespace ProductionControl.UIModels.Dtos.ExternalOrganization
{
	public class EmployeeExOrgCardNumShortNameIdDto : ObservableObject
	{
		public int EmployeeExOrgID { get => _employeeExOrgID; set => SetProperty(ref _employeeExOrgID, value); }
		private int _employeeExOrgID;

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
