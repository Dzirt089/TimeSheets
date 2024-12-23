using CommunityToolkit.Mvvm.ComponentModel;

namespace TimeSheets.Models
{
	public class EmployeePhoto : ObservableObject
	{
		public long EmployeeID
		{
			get => _employeeExOrgID;
			set => SetProperty(ref _employeeExOrgID, value);
		}
		private long _employeeExOrgID;

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
		public Employee? Employee
		{
			get => _employee;
			set => SetProperty(ref _employee, value);
		}
		private Employee? _employee;
	}
}
