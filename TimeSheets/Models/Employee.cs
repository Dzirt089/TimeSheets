using CommunityToolkit.Mvvm.ComponentModel;

namespace TimeSheets.Models
{
	/// <summary>
	/// Модель класса, описывающая сотрудника.
	/// </summary>
	public class Employee : ObservableObject
	{
		public long EmployeeID
		{
			get => _employeeID;
			set => SetProperty(ref _employeeID, value);
		}
		private long _employeeID;

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
			set
			{
				SetProperty(ref _shortName, value);
			}
		}
		private string? _shortName;

		/// <summary>Номер участка, где работает(закреплен) сотрудник</summary>
		public string DepartmentID
		{
			get => _departmentID;
			set => SetProperty(ref _departmentID, value);
		}
		private string _departmentID;

		public DepartmentProduction? DepartmentProduction
		{
			get => _departmentProduction;
			set => SetProperty(ref _departmentProduction, value);
		}
		private DepartmentProduction? _departmentProduction;

		/// <summary>Номер графика работы, который закрепили за сотрудником</summary>
		public string? NumGraf
		{
			get => _numGraf;
			set => SetProperty(ref _numGraf, value);
		}
		private string? _numGraf;

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
			set
			{
				SetProperty(ref _isDismissal, value);
			}
		}
		private bool _isDismissal;

		/// <summary>Флаг, который обозначает что сотрудник обедает на производстве или нет. Для того, чтобы заказывать на него обед\ужин или нет</summary>
		public bool IsLunch
		{
			get => _isLunch;
			set
			{
				SetProperty(ref _isLunch, value);
			}
		}
		private bool _isLunch;

		public IEnumerable<ShiftData>? Shifts
		{
			get => _shifts;
			set
			{
				SetProperty(ref _shifts, value);
			}
		}
		private IEnumerable<ShiftData>? _shifts;

		/// <summary>
		/// Связь с БД где храняться фотографии
		/// </summary>
		public EmployeePhoto? EmployeePhotos
		{
			get => _employeePhotos;
			set => SetProperty(ref _employeePhotos, value);
		}
		private EmployeePhoto? _employeePhotos;


		/// <summary>
		/// Описание сотрудника
		/// </summary>
		public int? NumberPass
		{
			get => _numberPass;
			set => SetProperty(ref _numberPass, value);
		}
		private int? _numberPass;
		/// <summary>
		/// Описание сотрудника
		/// </summary>
		public string? Descriptions
		{
			get => _descriptions;
			set => SetProperty(ref _descriptions, value);
		}
		private string? _descriptions;
		public Employee()
		{
			// чтобы разработчики могли добавлять продукты в категорию,
			// мы должны инициализировать свойство навигации в пустую коллекцию
			Shifts = new HashSet<ShiftData>();
		}
	}
}
