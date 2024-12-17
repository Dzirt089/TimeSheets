using CommunityToolkit.Mvvm.ComponentModel;

namespace ProductionControl.Models.ExternalOrganization
{
	public class EmployeeExOrgAddInRegion : ObservableObject
	{
		//public int EmployeeExOrgAddInRegionID
		//{
		//	get => _employeeExOrgAddInRegionID;
		//	set => SetProperty(ref _employeeExOrgAddInRegionID, value);
		//}
		//private int _employeeExOrgAddInRegionID;

		public int EmployeeExOrgID
		{
			get => _employeeExOrgID;
			set => SetProperty(ref _employeeExOrgID, value);
		}
		private int _employeeExOrgID;

		public EmployeeExOrg EmployeeExOrg
		{
			get => _employeeExOrg;
			set => SetProperty(ref _employeeExOrg, value);
		}
		private EmployeeExOrg _employeeExOrg;

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
