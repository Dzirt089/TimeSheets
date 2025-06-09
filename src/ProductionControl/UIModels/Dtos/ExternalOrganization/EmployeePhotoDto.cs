using CommunityToolkit.Mvvm.ComponentModel;

namespace ProductionControl.Models.Dtos.ExternalOrganization
{
	public class EmployeePhotoDto : ObservableObject
	{
		public int EmployeeExOrgID
		{
			get => _employeeExOrgID;
			set => SetProperty(ref _employeeExOrgID, value);
		}
		private int _employeeExOrgID;

		/// <summary>
		/// Фотографии людей
		/// </summary>
		public byte[]? Photo
		{
			get => _photo;
			set => SetProperty(ref _photo, value);
		}
		private byte[]? _photo;

		/// <summary>
		/// Связь с сотрудниками СО
		/// </summary>
		public EmployeeExOrgDto? EmployeeExOrg
		{
			get => _employeeExOrg;
			set => SetProperty(ref _employeeExOrg, value);
		}
		private EmployeeExOrgDto? _employeeExOrg;
	}
}
