using CommunityToolkit.Mvvm.ComponentModel;

namespace ProductionControl.UIModels.Dtos.ExternalOrganization
{
	public class EmployeeExOrgAddInRegionDto : ObservableObject
	{
		public int EmployeeExOrgID
		{
			get => _employeeExOrgID;
			set => SetProperty(ref _employeeExOrgID, value);
		}
		private int _employeeExOrgID;

		public EmployeeExOrgDto EmployeeExOrg
		{
			get => _employeeExOrg;
			set => SetProperty(ref _employeeExOrg, value);
		}
		private EmployeeExOrgDto _employeeExOrg;

		public string DepartmentID
		{
			get => _departmentID;
			set => SetProperty(ref _departmentID, value);
		}
		private string _departmentID;

		public bool WorkingInTimeSheetEmployeeExOrg
		{
			get => _workingInTimeSheetEmployeeExOrg;
			set => SetProperty(ref _workingInTimeSheetEmployeeExOrg, value);

		}
		private bool _workingInTimeSheetEmployeeExOrg;
	}
}
