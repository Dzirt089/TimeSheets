using CommunityToolkit.Mvvm.ComponentModel;

namespace ProductionControl.UIModels.Dtos.ExternalOrganization
{
	public class EmployeeExOrgDto : ObservableObject
	{
		public int EmployeeExOrgID
		{
			get => _employeeExOrgID;
			set => SetProperty(ref _employeeExOrgID, value);
		}
		private int _employeeExOrgID;

		/// <summary>
		/// Номер пропуска, который выдан сотруднику для прохода на территорию предприятия
		/// </summary>
		public string CardNumber { get => _cardNumberg; set => SetProperty(ref _cardNumberg, value); }
		private string _cardNumberg;


		/// <summary>
		/// Номер пропуска
		/// </summary>
		public int NumberPass
		{
			get => _numberPass;
			set => SetProperty(ref _numberPass, value);
		}
		private int _numberPass;

		/// <summary>
		/// Номер категории
		/// </summary>
		public int NumCategory
		{
			get => _numCategory;
			set => SetProperty(ref _numCategory, value);
		}
		private int _numCategory;

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

		/// <summary>
		/// Связь с БД где храняться фотографии
		/// </summary>
		public EmployeePhotoDto? EmployeePhotos
		{
			get => _employeePhotos;
			set => SetProperty(ref _employeePhotos, value);
		}
		private EmployeePhotoDto? _employeePhotos;

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


		public IEnumerable<EmployeeExOrgAddInRegionDto> EmployeeExOrgAddInRegions
		{
			get => _employeeExOrgAddInRegions;
			set => SetProperty(ref _employeeExOrgAddInRegions, value);
		}
		private IEnumerable<EmployeeExOrgAddInRegionDto> _employeeExOrgAddInRegions;

		public IEnumerable<ShiftDataExOrgDto>? ShiftDataExOrgs
		{
			get => _shiftDataExOrgs;
			set => SetProperty(ref _shiftDataExOrgs, value);

		}
		private IEnumerable<ShiftDataExOrgDto>? _shiftDataExOrgs;

		public EmployeeExOrgDto()
		{
			ShiftDataExOrgs = new HashSet<ShiftDataExOrgDto>();
			EmployeeExOrgAddInRegions = new List<EmployeeExOrgAddInRegionDto>();
		}
	}
}
