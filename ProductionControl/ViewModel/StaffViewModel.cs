using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using ProductionControl.Entitys;
using ProductionControl.Models;
using ProductionControl.Services.Interfaces;
using ProductionControl.Views;

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ProductionControl.ViewModel
{
	public class StaffViewModel : ObservableObject
	{
		private readonly ITimeSheetDbService _timeSheetDb;
		private readonly IErrorLogger _errorLogger;
		private StaffView StaffView { get; set; }

		public StaffViewModel(
			ITimeSheetDbService timeSheetDb,
			IErrorLogger errorLogger
			)
		{
			try
			{
				_timeSheetDb = timeSheetDb;
				_errorLogger = errorLogger;
				MainVisib = Visibility.Collapsed;
				//TODO: Сделать поиск как в табеле. Выпадающее меню для участка, графика, нормы
				LoadStaffChanged += StaffViewModel_LoadTOChanged;
			}
			catch (Exception ex)
			{
				_errorLogger?.ProcessingErrorLog(ex);
			}
		}
		public event EventHandler LoadStaffChanged;
		public ICommand StaffRefreshCmd { get; set; }
		public ICommand SaveDataForEmployeeCmd { get; set; }
		public ICommand CloseCmd { get; set; }

		#region Methods

		/// <summary>
		/// Асинхронная инициализация данными для картотеки
		/// </summary>
		/// <returns></returns>
		public async Task InitiazinigStaffAsync(StaffView staffView)
		{
			try
			{
				StaffView = null;
				EmployeesForCartoteca = null;
				UserDataCurrent = null;
				NamesDepartmentStaff = null;
				NamesDepartmentStaffItem = null;

				StaffView = staffView;
				UserDataCurrent = await _timeSheetDb
					.GetLocalUserAsync()
					.ConfigureAwait(false)
					?? new() { MachineName = string.Empty, UserName = string.Empty };

				NamesDepartmentStaff = await GetDepartmentProductionsAsync();
				NamesDepartmentStaffItem = NamesDepartmentStaff.FirstOrDefault();

				StaffRefreshCmd = new AsyncRelayCommand(GetEmployeeForCartotecasAsync);
				SaveDataForEmployeeCmd = new AsyncRelayCommand(SaveDataForEmployeeAsync);
				CloseCmd = new RelayCommand(Close);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
			}
		}

		private void Close()
		{
			StaffView.Close();
		}

		private async Task SaveDataForEmployeeAsync()
		{
			try
			{
				if (ItemEmployeeForCartoteca is null || UserDataCurrent is null) return;
				var tempID = ItemEmployeeForCartoteca.EmployeeID;
				var newEmployee = new Employee
				{
					EmployeeID = ItemEmployeeForCartoteca.EmployeeID,
					FullName = ItemEmployeeForCartoteca.FullName,
					ShortName = ItemEmployeeForCartoteca.ShortName,
					DepartmentID = ItemEmployeeForCartoteca.DepartmentID,
					NumGraf = ItemEmployeeForCartoteca.NumGraf,
					DateEmployment = ItemEmployeeForCartoteca.DateEmployment,
					DateDismissal = ItemEmployeeForCartoteca.DateDismissal,
					IsDismissal = ItemEmployeeForCartoteca.IsDismissal,
					IsLunch = ItemEmployeeForCartoteca.IsLunch,
				};

				await _timeSheetDb.SetDataEmployeeAsync(newEmployee, UserDataCurrent);
				await GetEmployeeForCartotecasAsync();
				ItemEmployeeForCartoteca = EmployeesForCartoteca.Where(x => x.EmployeeID == tempID).FirstOrDefault();
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
			}
		}
		private async void StaffViewModel_LoadTOChanged(object? sender, EventArgs e)
		{
			try
			{
				await GetEmployeeForCartotecasAsync()
					.ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Получаем список сотрудников по выбранному участку, 
		/// которые не уволенны. Т.е. работующих
		/// </summary>
		internal async Task GetEmployeeForCartotecasAsync()
		{
			try
			{
				if (NamesDepartmentStaffItem is null)
				{
					EmployeesForCartoteca = [];
					return;
				}

				EmployeesForCartoteca = await _timeSheetDb
				   .GetEmployeeForCartotecasAsync(UserDataCurrent, NamesDepartmentStaffItem)
				   .ConfigureAwait(false) ?? [];
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex,
					user: UserDataCurrent.UserName,
					machine: UserDataCurrent.MachineName)
					.ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Получаем данные по всем участкам
		/// </summary>
		/// <returns></returns>
		internal async Task<ObservableCollection<DepartmentProduction>>
			GetDepartmentProductionsAsync()
		{
			try
			{
				return await _timeSheetDb
					.GetAllDepartmentsAsync(UserDataCurrent)
					.ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex,
					user: UserDataCurrent.UserName,
					machine: UserDataCurrent.MachineName)
					.ConfigureAwait(false);
				return [];
			}
		}
		#endregion

		#region Свойства

		/// <summary>
		/// Данные с именами сотрудника и его компьютера
		/// </summary>
		public LocalUserData UserDataCurrent { get; private set; }
		public ObservableCollection<DepartmentProduction> NamesDepartmentStaff
		{
			get => _namesDepartmentStaff;
			set
			{
				SetProperty(ref _namesDepartmentStaff, value);
			}
		}

		/// <summary>
		/// Список участков предприятия
		/// </summary>
		private ObservableCollection<DepartmentProduction> _namesDepartmentStaff;

		/// <summary>
		/// Выбранный участок предприятия
		/// </summary>
		public DepartmentProduction NamesDepartmentStaffItem
		{
			get => _namesDepartmentStaffItem;
			set
			{
				NewSIZ = null;
				SetProperty(ref _namesDepartmentStaffItem, value);
				if (_namesDepartmentStaffItem != null)
				{
					LoadStaffChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}
		private DepartmentProduction _namesDepartmentStaffItem;

		/// <summary>Список сотрудников</summary>
		public ObservableCollection<Employee> EmployeesForCartoteca
		{
			get => _employeesForCartoteca;
			set
			{
				SetProperty(ref _employeesForCartoteca, value);
			}
		}
		private ObservableCollection<Employee> _employeesForCartoteca;

		/// <summary>Выбранный сотрудник из списка</summary>
		public Employee ItemEmployeeForCartoteca
		{
			get => _itemEmployeeForCartoteca;
			set
			{

				SetProperty(ref _itemEmployeeForCartoteca, value);
				if (_itemEmployeeForCartoteca != null)
				{
					MainVisib = Visibility.Visible;
				}
				else
				{
					NewSIZ = null;
					MainVisib = Visibility.Collapsed;
				}
			}
		}
		private Employee _itemEmployeeForCartoteca;

		public int? NewSIZ
		{
			get => _newSIZ;
			set
			{
				SetProperty(ref _newSIZ, value);
			}
		}
		private int? _newSIZ;
		#endregion

		#region Фильтры поиска

		#endregion

		#region Видимость элементов
		public Visibility MainVisib
		{
			get => _mainVisib;
			set => SetProperty(ref _mainVisib, value);
		}
		private Visibility _mainVisib;
		#endregion



		#region Команды
		public ICommand DeleteStaffCmd { get; }
		public ICommand EditStaffCmd { get; }
		public ICommand SaveStaffCmd { get; set; }
		public ICommand AddStaffCmd { get; }
		public ICommand ResetFiltersCmd { get; }

		#endregion
	}
}
