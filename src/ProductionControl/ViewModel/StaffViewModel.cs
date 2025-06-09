using AutoMapper;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using ProductionControl.ApiClients.ProductionApiServices.EmployeeSheetApiServices.Interfaces;
using ProductionControl.ApiClients.ProductionApiServices.SizEmployeeApiServices.Interfaces;
using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;
using ProductionControl.Services.ErrorLogsInformation;
using ProductionControl.UIModels.Dtos.EmployeesFactory;
using ProductionControl.UIModels.Dtos.Siz;
using ProductionControl.UIModels.Model.GlobalPropertys;
using ProductionControl.Views;

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ProductionControl.ViewModel
{
	public class StaffViewModel : ObservableObject
	{
		private readonly IEmployeeSheetApiClient _timeSheetDb;
		private readonly ISizEmployeeApiClient _sizEmployeeApiClient;
		private readonly IErrorLogger _errorLogger;
		private readonly IMapper _mapper;

		private StaffView StaffView { get; set; }
		private readonly Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
		public StaffViewModel(
			IEmployeeSheetApiClient timeSheetDb,
			IErrorLogger errorLogger,
			GlobalEmployeeSessionInfo userData,
			IMapper mapper,
			ISizEmployeeApiClient sizEmployeeApiClient)
		{
			_timeSheetDb = timeSheetDb;
			_errorLogger = errorLogger;
			MainVisib = Visibility.Collapsed;

			//TODO: Сделать поиск как в табеле. Выпадающее меню для участка, графика, нормы
			LoadStaffChanged += StaffViewModel_LoadTOChanged;

			_mapper = mapper;
			_sizEmployeeApiClient = sizEmployeeApiClient;
		}

		public void Dispose()
		{
			LoadStaffChanged -= StaffViewModel_LoadTOChanged;
		}

		public event Func<Task> LoadStaffChanged;
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
				NamesDepartmentStaff = null;
				NamesDepartmentStaffItem = null;
				ListSizsForEmployeesOrig = null;

				StaffView = staffView;

				NamesDepartmentStaff = await GetDepartmentProductionsAsync();
				NamesDepartmentStaffItem = NamesDepartmentStaff.FirstOrDefault();


				var response = await _sizEmployeeApiClient.GetSizUsageRateAsync();
				ListSizsForEmployeesOrig = _mapper.Map<List<SizUsageRateDto>>(response);

				StaffRefreshCmd = new AsyncRelayCommand(GetEmployeeForCartotecasAsync);
				SaveDataForEmployeeCmd = new AsyncRelayCommand(SaveDataForEmployeeAsync);
				CloseCmd = new RelayCommand(Close);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
				throw;
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
				if (ItemEmployeeForCartoteca is null) return;
				var tempID = ItemEmployeeForCartoteca.EmployeeID;

				var newEmployee = new EmployeeDto
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
					UsageNormID = NewSIZ,
				};

				var request = _mapper.Map<Employee>(newEmployee);

				await _timeSheetDb.SetDataEmployeeAsync(request);
				await GetEmployeeForCartotecasAsync();
				ItemEmployeeForCartoteca = EmployeesForCartoteca.Where(x => x.EmployeeID == tempID).FirstOrDefault();
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
				throw;
			}
		}
		private async Task StaffViewModel_LoadTOChanged()
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

				var request = _mapper.Map<DepartmentProduction>(NamesDepartmentStaffItem);
				var response =
					await _timeSheetDb.GetEmployeeForCartotecasAsync(request)
					.ConfigureAwait(false);

				var employeesForCartoteca = _mapper.Map<List<EmployeeDto>>(response);

				//Проводим валидацию, где остаются работающие сотрудники и те, которых уволили в выбранном месяце
				employeesForCartoteca = employeesForCartoteca
					.Where(x => x.ValidateEmployee(DateTime.Now.Month, DateTime.Now.Year) && x.IsDismissal == false)
					.ToList();

				EmployeesForCartoteca = new ObservableCollection<EmployeeDto>(employeesForCartoteca);

			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
				throw;
			}
		}

		/// <summary>
		/// Получаем данные по всем участкам
		/// </summary>
		/// <returns></returns>
		internal async Task<ObservableCollection<DepartmentProductionDto>>
			GetDepartmentProductionsAsync()
		{
			try
			{
				var response = await _timeSheetDb.GetAllDepartmentsAsync()
					.ConfigureAwait(false);

				var result = _mapper.Map<List<DepartmentProductionDto>>(response);

				result.ForEach(x => x.FullNameDepartment = $"{x.DepartmentID} : {x.NameDepartment}");

				return new ObservableCollection<DepartmentProductionDto>(result);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
				return [];
				throw;
			}
		}
		#endregion

		#region Свойства


		public ObservableCollection<DepartmentProductionDto> NamesDepartmentStaff
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
		private ObservableCollection<DepartmentProductionDto> _namesDepartmentStaff;

		/// <summary>
		/// Выбранный участок предприятия
		/// </summary>
		public DepartmentProductionDto NamesDepartmentStaffItem
		{
			get => _namesDepartmentStaffItem;
			set
			{
				NewSIZ = null;
				SetProperty(ref _namesDepartmentStaffItem, value);
				if (_namesDepartmentStaffItem != null)
				{
					if (LoadStaffChanged is not null)
						dispatcher.InvokeAsync(async () => await LoadStaffChanged.Invoke());
				}
			}
		}
		private DepartmentProductionDto _namesDepartmentStaffItem;

		/// <summary>Список сотрудников</summary>
		public ObservableCollection<EmployeeDto> EmployeesForCartoteca
		{
			get => _employeesForCartoteca;
			set
			{
				SetProperty(ref _employeesForCartoteca, value);
			}
		}
		private ObservableCollection<EmployeeDto> _employeesForCartoteca;


		/// <summary>Коллекция по СИЗ-ам на сотрудника</summary>
		public List<SizUsageRateDto> ListSizsForEmployees
		{
			get => _listSizsForEmployees;
			set => SetProperty(ref _listSizsForEmployees, value);
		}
		private List<SizUsageRateDto> _listSizsForEmployees;
		/// <summary>Коллекция по СИЗ-ам на сотрудника</summary>
		public List<SizUsageRateDto> ListSizsForEmployeesOrig
		{
			get => _listSizsForEmployeesOrig;
			set => SetProperty(ref _listSizsForEmployeesOrig, value);
		}
		private List<SizUsageRateDto> _listSizsForEmployeesOrig;

		/// <summary>Выбранный сотрудник из списка</summary>
		public EmployeeDto ItemEmployeeForCartoteca
		{
			get => _itemEmployeeForCartoteca;
			set
			{
				ListSizsForEmployees = [];
				SetProperty(ref _itemEmployeeForCartoteca, value);
				if (_itemEmployeeForCartoteca != null)
				{
					MainVisib = Visibility.Visible;
					NewSIZ = ItemEmployeeForCartoteca.UsageNormID;
				}
				else
				{
					NewSIZ = null;
					MainVisib = Visibility.Collapsed;
				}

			}
		}
		private EmployeeDto _itemEmployeeForCartoteca;

		public int? NewSIZ
		{
			get => _newSIZ;
			set
			{
				SetProperty(ref _newSIZ, value);
				if (_newSIZ != null && ItemEmployeeForCartoteca != null)
				{
					ListSizsForEmployees = ListSizsForEmployeesOrig
						.Where(x => x.UsageNorm.UsageNormID
						== _newSIZ)
						.ToList() ?? [];
				}
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
