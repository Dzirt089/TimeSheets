using CommunityToolkit.Mvvm.ComponentModel;

namespace ProductionControl.Models.ExternalOrganization
{
	public class EmployeeExOrg : ObservableObject
	{
		public int EmployeeExOrgID
		{
			get => _employeeExOrgID;
			set => SetProperty(ref _employeeExOrgID, value);
		}
		private int _employeeExOrgID;

		public int EmployeeExOrgAddInRegionID
		{
			get => _employeeExOrgAddInRegionID;
			set => SetProperty(ref _employeeExOrgAddInRegionID, value);
		}
		private int _employeeExOrgAddInRegionID;


		/// <summary>
		/// Номер пропуска
		/// </summary>
		public int NumberPass
		{
			get => _numberPass;
			set => SetProperty(ref _numberPass, value);
		}
		private int _numberPass;

		/// <summary>Полные ФИО сотрудника</summary>
		public string? FullName
		{
			get => _fullName;
			set
			{
				SetProperty(ref _fullName, value);

				if (!string.IsNullOrEmpty(FullName))
				{
					var splitFIO = FullName.Split(' ');
					if (splitFIO.Length < 3)
						ShortName = FullName;
					else
						ShortName = $@"{splitFIO[0]} {splitFIO[1][0]}.{splitFIO[2][0]}.";
				}
				else
					ShortName = string.Empty;
			}
		}
		private string? _fullName;

		/// <summary>Сокращённое имя сотрудника (например: Сидоров С.С.)</summary>
		public string? ShortName
		{
			get => _shortName;
			set => SetProperty(ref _shortName, value);

		}
		private string? _shortName;

		/// <summary>Дата трудоустройства</summary>
		public DateTime DateEmployment
		{
			get => _dateEmployment;
			set => SetProperty(ref _dateEmployment, value);
		}
		private DateTime _dateEmployment;

		/// <summary>Дата увольнения</summary>
		public DateTime DateDismissal
		{
			get => _dateDismissal;
			set => SetProperty(ref _dateDismissal, value);
		}
		private DateTime _dateDismissal;

		/// <summary>Флаг, который обозначает что сотрудник уволен или нет</summary>
		public bool IsDismissal
		{
			get => _isDismissal;
			set => SetProperty(ref _isDismissal, value);

		}
		private bool _isDismissal;

		/// <summary>
		/// Примечание к сотруднику
		/// </summary>
		public string? Descriptions
		{
			get => _descriptions;
			set => SetProperty(ref _descriptions, value);

		}
		private string? _descriptions;

		/// <summary>
		/// Фотографии людей
		/// </summary>
		public byte[]? Photo
		{
			get => _photo;
			set => SetProperty(ref _photo, value);
		}
		private byte[]? _photo;

		public IEnumerable<EmployeeExOrgAddInRegion> EmployeeExOrgAddInRegions
		{
			get => _employeeExOrgAddInRegions;
			set => SetProperty(ref _employeeExOrgAddInRegions, value);
		}
		private IEnumerable<EmployeeExOrgAddInRegion> _employeeExOrgAddInRegions;

		public IEnumerable<ShiftDataExOrg>? ShiftDataExOrgs
		{
			get => _shiftDataExOrgs;
			set => SetProperty(ref _shiftDataExOrgs, value);
			
		}
		private IEnumerable<ShiftDataExOrg>? _shiftDataExOrgs;

		public EmployeeExOrg()
		{
			ShiftDataExOrgs = new HashSet<ShiftDataExOrg>();
			EmployeeExOrgAddInRegions = new List<EmployeeExOrgAddInRegion>();
		}
	}
}
