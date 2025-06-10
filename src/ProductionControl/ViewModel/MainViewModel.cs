using AutoMapper;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MahApps.Metro.Controls.Dialogs;

using Microsoft.Win32;

using ProductionControl.ApiClients.DefinitionOfNonWorkingDaysApiServices.Interfaces;
using ProductionControl.ApiClients.ProductionApiServices.EmployeesExternalOrganizationsApiServices.Interfaces;
using ProductionControl.ApiClients.ProductionApiServices.EmployeeSheetApiServices.Interfaces;
using ProductionControl.ApiClients.ProductionApiServices.ReportsApiServices.Interfaces;
using ProductionControl.ApiClients.ProductionApiServices.ResultSheetsApiServices.Interfaces;
using ProductionControl.DataAccess.Classes.ApiModels.Dtos;
using ProductionControl.DataAccess.Classes.EFClasses.EmployeesExternalOrganizations;
using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;
using ProductionControl.DataAccess.Classes.HttpModels;
using ProductionControl.Services.ErrorLogsInformation;
using ProductionControl.UIModels.Dtos.EmployeesFactory;
using ProductionControl.UIModels.Dtos.ExternalOrganization;
using ProductionControl.UIModels.Model.EmployeesFactory;
using ProductionControl.UIModels.Model.ExternalOrganization;
using ProductionControl.UIModels.Model.GlobalPropertys;
using ProductionControl.Views;

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;


namespace ProductionControl.ViewModel
{
	public class MainViewModel : ObservableObject
	{
		#region Поля		
		private readonly IDefinitionOfNonWorkingDaysApiClient _daysApi;
		private readonly IEmployeeSheetApiClient _employeeSheetApi;
		private readonly IResultSheetsApiClient _resultSheetsApi;
		private readonly IReportsApiClient _reportsApi;
		private readonly IEmployeesExternalOrganizationsApiClient _employeeExOrgSheetApi;

		private readonly IDialogCoordinator _coordinator;
		private readonly IErrorLogger _errorLogger;
		private readonly IMapper _mapper;

		private readonly Dispatcher dispatcher = Application.Current.Dispatcher;

		private const string textError = "Произошла непредвиденная ошибка. Пожалуйста, свяжитесь с Тех. Отделом.";

		private StaffViewModel StaffViewModel { get; set; }
		private StaffExternalOrgViewModel ExternalOrgViewModel { get; set; }
		private FAQViewModel FAQViewModel { get; set; }
		private GlobalSettingsProperty GlobalProperty { get; set; }
		/// <summary>
		/// Данные с именами сотрудника и его компьютера
		/// </summary>
		private GlobalEmployeeSessionInfo UserDataCurrent { get; set; }
		#endregion

		#region Конструктор

		/// <summary>
		/// Инициализирует новый экземпляр класса MainViewModel.
		/// </summary>	
		/// <param name="daysApi">Интерфейс сервиса «Определение нерабочих дней».</param>		
		/// <param name="coordinator">Интерфейс координатора диалогов</param>
		/// <param name="errorLogger">Интерфейс регистратора ошибок.</param>

		public MainViewModel(
			StaffViewModel staffViewModel,
			FAQViewModel fAQViewModel,
			StaffExternalOrgViewModel staffExternalOrgView,

			GlobalSettingsProperty globalProperty,
			GlobalEmployeeSessionInfo userData,

			IDefinitionOfNonWorkingDaysApiClient daysApi,
			IEmployeeSheetApiClient employeeSheetApi,
			IResultSheetsApiClient resultSheetsApi,
			IReportsApiClient reportsApi,
			IEmployeesExternalOrganizationsApiClient employeeExOrgSheetApi,

			IDialogCoordinator coordinator,
			IErrorLogger errorLogger,
			IMapper mapper
			)
		{
			Visibility = Visibility.Collapsed;
			VisibilityButtonAdditionally = Visibility.Collapsed;
			FlagShowResultSheet = false;

			StaffViewModel = staffViewModel;
			FAQViewModel = fAQViewModel;
			ExternalOrgViewModel = staffExternalOrgView;

			GlobalProperty = globalProperty;
			UserDataCurrent = userData;

			_daysApi = daysApi;
			_employeeSheetApi = employeeSheetApi;
			_resultSheetsApi = resultSheetsApi;
			_reportsApi = reportsApi;
			_employeeExOrgSheetApi = employeeExOrgSheetApi;

			_coordinator = coordinator;
			_mapper = mapper;
			_errorLogger = errorLogger;
		}
		#endregion
		public void Dispose()
		{
			if (TimeSheets != null)
			{
				foreach (var item in TimeSheets)
				{
					item.WorkerHours.CollectionChanged -= TimeSheets_CollectionChanged;
					item.WorkerHours.AsParallel().ForAll(underItem =>
					{
						underItem.PropertyChanged -= Item_PropertyChanged;
					});
				}
			}

			if (TimeSheetsExOrg != null)
			{
				foreach (var item in TimeSheetsExOrg)
				{
					item.WorkerHours.CollectionChanged -= TimeSheetsExOrg_CollectionChanged;
					item.WorkerHours.AsParallel().ForAll(underItem =>
					{
						underItem.PropertyChanged -= ItemExOrg_PropertyChanged;
					});
				}
			}

			StaffViewModel.Dispose();

			LoadTOChanged -= MainViewModel_LoadTOChanged;
			LoadSOChanged -= MainViewModelExOrg_LoadTOChanged;

			LoadApplyFilterExOrgHanged -= MainViewModel_LoadApplyFilterExOrghanged;
			LoadApplyFilterHanged -= MainViewModel_LoadApplyFilterHanged;

			UpdateDateLunchChanged -= MainViewModel_UpdateDateLunchChanged;
		}

		#region Инициализация

		/// <summary>
		/// Асинхронно инициализирует ViewModel.
		/// </summary>
		public async Task InitializeAsync()
		{
			try
			{

				CashListNoWorksDict = [];
				NoWorkDaysTO = [];
				Indicators = [];

				LoadTOChanged += MainViewModel_LoadTOChanged;
				LoadSOChanged += MainViewModelExOrg_LoadTOChanged;
				LoadApplyFilterExOrgHanged += MainViewModel_LoadApplyFilterExOrghanged;
				LoadApplyFilterHanged += MainViewModel_LoadApplyFilterHanged;
				UpdateDateLunchChanged += MainViewModel_UpdateDateLunchChanged;

				await SetMonthAndYear();

				if (AccessButtonAddition())
					VisibilityButtonAdditionally = Visibility.Visible;
				else
					VisibilityButtonAdditionally = Visibility.Collapsed;

				if (LunchAndMonthlySummaryAccess())
				{
					VisibilityCreateReportMonthlySummary = Visibility.Visible;
					CreateReportMonthlySummaryCmd = new AsyncRelayCommand(CreateReportMonthlySummaryAsync);
					FormulateReportEveryDayCmd = new AsyncRelayCommand(FormulateReportForLunchEveryDayAsync);
				}
				else VisibilityCreateReportMonthlySummary = Visibility.Collapsed;

				if (AccesForExOrg())
				{
					VisibilityForExOrg = Visibility.Visible;
					ShowStaffExOrgWindowCmd = new AsyncRelayCommand(ShowStaffExOrgWindowAsync);

					if (SOAccess())
					{
						VisibilityButtonForExOrg = Visibility.Visible;
						CreateReportMonthlySummaryForEmployeeExpOrgCmd = new AsyncRelayCommand(CreateReportMonthlySummaryForEmployeeExpOrgAsync);
						RunCreateReportCmd = new AsyncRelayCommand(RunCreateReportAsync);
						CloseSelectedDateCmd = new AsyncRelayCommand(CloseSelectedDateAsync);
					}
					else VisibilityButtonForExOrg = Visibility.Collapsed;


				}
				else VisibilityForExOrg = Visibility.Collapsed;

				if (LunchAndMonthlySummaryAccess())
				{
					VisibilityCreateReportMonthlySummary = Visibility.Visible;
					CreateReportMonthlySummaryCmd = new AsyncRelayCommand(CreateReportMonthlySummaryAsync);
					FormulateReportEveryDayCmd = new AsyncRelayCommand(FormulateReportForLunchEveryDayAsync);
				}
				else VisibilityCreateReportMonthlySummary = Visibility.Collapsed;


				if (MainAccess())
				{
					Visibility = Visibility.Visible;
					CreateReportMonthlySummaryForEmployeeExpOrgCmd = new AsyncRelayCommand(CreateReportMonthlySummaryForEmployeeExpOrgAsync);
					RunCreateReportCmd = new AsyncRelayCommand(RunCreateReportAsync);
					CloseSelectedDateCmd = new AsyncRelayCommand(CloseSelectedDateAsync);
					RunCustomDialogForDismissalCmd = new AsyncRelayCommand(RunCustomDialogAsyncForDismissal);
					HandlerCommandDismissOrRescindDismissalCmd = new AsyncRelayCommand(HandlerCommandDismissOrRescindDismissalAsync);
					IsLunchCmd = new AsyncRelayCommand(IsLunchingAsync);
					UpdCmd = new AsyncRelayCommand(UpdAsync);
					CloseCmd = new AsyncRelayCommand(CloseAsync);

					UpdateDataEmployeeChangesCmd = new AsyncRelayCommand(UpdateDataTableAsync);
					FormulateReportForLunchLastMonhtCmd =
						new AsyncRelayCommand(FormulateReportForLunchLastMonhtAsync);

					RunCustomDialogForLunchCmd = new AsyncRelayCommand(RunCustomDialogAsyncForLunch);
					UpdLunchCmd = new AsyncRelayCommand(UpdLunchAsync);
					CloseLunchCmd = new AsyncRelayCommand(CloseLunchAsync);

					ShowStaffWindowCmd = new AsyncRelayCommand(ShowStaffWindowAsync);
					ShowStaffExOrgWindowCmd = new AsyncRelayCommand(ShowStaffExOrgWindowAsync);
				}
				else
					Visibility = Visibility.Collapsed;

				if (AccessForRegions043_044())
				{
					Visibility043_044 = Visibility.Visible;
					PlanLaborCmd = new AsyncRelayCommand(PlanLaborAsync);
				}
				else
					Visibility043_044 = Visibility.Collapsed;

				ShowFAQWindowCmd = new AsyncRelayCommand(ShowFAQWindowAsync);
				UpdateScheduleCmd = new AsyncRelayCommand(MainViewModel_LoadTOChanged);
				ShowResultSheetCmd = new AsyncRelayCommand(ShowResultSheet);
				UpdateResultSheetCmd = new AsyncRelayCommand(InitResultSheetAsync);
				CreateReportResultSheetCmd = new AsyncRelayCommand(CreateReportResultSheetAsync);
				UpdateScheduleOxRegCmd = new AsyncRelayCommand(MainViewModelExOrg_LoadTOChanged);


				ItemMonthsTO = ListMonthsTO?[DateTime.Now.Month - 1] ?? new(1, string.Empty);
				ItemYearsTO = ListYearsTO.Where(x => !string.IsNullOrEmpty(x.Name) &&
							x.Name.Contains($"{DateTime.Now.Year}")).FirstOrDefault() ?? new(1, string.Empty);

				ItemMonthsTOExOrg = ListMonthsTOExOrg?[DateTime.Now.Month - 1] ?? new(1, string.Empty);
				ItemYearsTOExOrg = ListYearsTOExOrg.Where(x => !string.IsNullOrEmpty(x.Name) &&
							x.Name.Contains($"{DateTime.Now.Year}")).FirstOrDefault() ?? new(1, string.Empty);

				StartDate = new DateTime(day: 1, month: ItemMonthsTO.Id, year: ItemYearsTO.Id);
				EndDate = new DateTime(day: MaxDayTO, month: ItemMonthsTO.Id, year: ItemYearsTO.Id);

				EmployeeAccesses = await GetAccessRightsAsync().ConfigureAwait(false);

				NamePeople = EmployeeAccesses?.FirstOrDefault()?.NamePeople ?? string.Empty;

				UserDataCurrent.NameEmployee = NamePeople;

				await GetDepartmentProductionsAsync().ConfigureAwait(false);
				await GetDepartmentProductionsExOrgAsync(UserDataCurrent.UserName, NamesDepartment).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
				throw;
			}
		}

		public CustomDialog CustomDialogsSelectedDate { get; private set; }

		private async Task RunCreateReportAsync()
		{
			try
			{
				//Закрываем окно
				await _coordinator.HideMetroDialogAsync(this, CustomDialogsSelectedDate);

				var controller = await _coordinator.ShowProgressAsync(this, "Пожалуйста, подождите!", "Идет формирование заявки...");

				controller.SetIndeterminate();

				StartEndDateTime startEndDate = new StartEndDateTime { StartDate = StardPeriod, EndDate = EndPeriod };
				var resultCheck = await _reportsApi.CreateReportMonthlySummaryForEmployeeExpOrgsAsync(startEndDate).ConfigureAwait(false);

				await controller.CloseAsync();

				if (resultCheck)
					await _coordinator.ShowMessageAsync(this, "Формирование отчётов", "Отчёты сформированы");
				else
					await _coordinator.ShowMessageAsync(this, "Формирование отчётов", "Ошибки в формировании отчётов");
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
			}
		}

		private async Task CreateReportMonthlySummaryForEmployeeExpOrgAsync()
		{
			try
			{
				StardPeriod = StartDate;
				EndPeriod = EndDate;

				//Создаём новое кастомное окно 
				CustomDialogsSelectedDate = new CustomDialog
				{
					//Привязываем кастомное окно к нашей View-модели
					Content = new SelectedPeriodDates(this)
				};

				//Настраиваем поведение анимации
				MetroDialogSettings settings = new() { AnimateShow = true, AnimateHide = true };

				//Показываем окно
				await _coordinator.ShowMetroDialogAsync(this, CustomDialogsSelectedDate, settings);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
			}
		}

		private bool AccesForExOrg()
		{
			ValueDepartmentID = UserDataCurrent.UserName.GetDepartmentAsync();//Environment.UserName.GetDepartmentAsync();
			if (string.IsNullOrEmpty(ValueDepartmentID) && !SOAccess()) return false;
			else return true;
		}

		private async Task CreateReportMonthlySummaryAsync()
		{
			try
			{
				var controller = await _coordinator.ShowProgressAsync(this, "Пожалуйста, подождите!", "Идет формирование заявки...");
				controller.SetIndeterminate();

				DateTime date = new DateTime(day: 1, month: ItemMonthsTO.Id, year: ItemYearsTO.Id);
				var resultCheck = await _reportsApi.CreateReportMonthlySummaryAsync(date).ConfigureAwait(false);

				await controller.CloseAsync();

				if (resultCheck)
				{
					await _coordinator.ShowMessageAsync(this, "Формирование отчёта", "Отчёт сформирован");
					await SetTimeSheetItemsAsync();
				}
				else
					await _coordinator.ShowMessageAsync(this, "Формирование отчёта", "Ошибки в формировании отчёта");
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
			}
		}

		/// <summary>
		/// Проверка прав на компы Наливайко, Мастеров 43/44 участков, программистов. Чтобы показывать общую кнопку "Дополнительно", только им.
		/// </summary>
		/// <returns></returns>
		private bool AccessButtonAddition()
		{
			return UserDataCurrent.UserName.Equals("okad01", StringComparison.OrdinalIgnoreCase) ||
					UserDataCurrent.UserName.Equals("brvp03", StringComparison.OrdinalIgnoreCase) ||
					UserDataCurrent.UserName.Equals("ceh06", StringComparison.OrdinalIgnoreCase) ||
					UserDataCurrent.UserName.Equals("teho19", StringComparison.OrdinalIgnoreCase) ||
					UserDataCurrent.UserName.Equals("teho12", StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Проверка прав на компы Мастеров 43/44 участков, программистов. Чтобы показывать кнопку "Плановая трудоемкость", только им.
		/// </summary>
		/// <returns></returns>
		private bool AccessForRegions043_044()
		{
			return UserDataCurrent.UserName.Equals("ceh06", StringComparison.OrdinalIgnoreCase) ||
					UserDataCurrent.UserName.Equals("teho19", StringComparison.OrdinalIgnoreCase) ||
					UserDataCurrent.UserName.Equals("teho12", StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Проверка прав на компы Наливайко, программистов. Чтобы показывать вложенные кнопки в "Дополнительно", только им.
		/// </summary>
		/// <returns></returns>
		private bool MainAccess()
		{
			return UserDataCurrent.UserName.Equals("okad01", StringComparison.OrdinalIgnoreCase) ||
					UserDataCurrent.UserName.Equals("teho19", StringComparison.OrdinalIgnoreCase) ||
					UserDataCurrent.UserName.Equals("teho12", StringComparison.OrdinalIgnoreCase);
		}

		private bool SOAccess()
		{
			return UserDataCurrent.UserName.Equals("okad01", StringComparison.OrdinalIgnoreCase) ||
					UserDataCurrent.UserName.Equals("teho19", StringComparison.OrdinalIgnoreCase) ||
					UserDataCurrent.UserName.Equals("teho12", StringComparison.OrdinalIgnoreCase) ||
					UserDataCurrent.UserName.Equals("ceh07", StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Проверка прав на компы Наливайко, программистов. Чтобы показывать вложенные кнопки в "Дополнительно", только им.
		/// </summary>
		/// <returns></returns>
		private bool LunchAndMonthlySummaryAccess()
		{
			return UserDataCurrent.UserName.Equals("okad01", StringComparison.OrdinalIgnoreCase) ||
					UserDataCurrent.UserName.Equals("brvp03", StringComparison.OrdinalIgnoreCase) ||
					UserDataCurrent.UserName.Equals("teho19", StringComparison.OrdinalIgnoreCase) ||
					UserDataCurrent.UserName.Equals("teho12", StringComparison.OrdinalIgnoreCase);
		}

		public event Func<Task> LoadTOChanged;

		#endregion

		#region Табель для сотрудников сторонних организаций

		#region Handler

		public event Func<Task> LoadSOChanged;

		/// <summary>
		/// Обновляет свойство IsLoadedTO на основе выбранных месяца, года и отдела.
		/// </summary>
		private void UpdateIsLoadedSO()
		{
			if (ItemMonthsTO != null && ItemYearsTO != null && NamesDepartmentItem != null && MaxDayTO > 0)
				if (LoadSOChanged is not null)
					dispatcher.InvokeAsync(async () => await LoadSOChanged.Invoke());
		}

		#endregion

		#region Search		

		/// <summary>
		/// Применяем фильтр напрямую к коллекции. Так как DataGrid не работает с фильтрами ICollectionView как ListView
		/// </summary>
		/// <returns></returns>
		private async Task ApplyFilterExOrg()
		{
			try
			{
				var filteredList = DoubleTimeSheetsExOrgForSearch?
					.Where(item => string.IsNullOrEmpty(FilterNameExOrg) ||
						item.FioShiftOverday.ShortName.Contains(FilterNameExOrg, StringComparison.OrdinalIgnoreCase))
					.ToList() ?? [];

				TimeSheetsExOrg = new ObservableCollection<TimeSheetItemExOrg>(filteredList);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
			}
		}

		#region Handler

		public event Func<Task> LoadApplyFilterExOrgHanged;
		private CancellationTokenSource? _filterCancellationTokenSourceExOrg;

		/// <summary>
		/// Обновляет свойство IsLoadedTO на основе выбранных месяца, года и отдела.
		/// </summary>
		private void UpdateIsLoadApplyFilterExOrgHanged()
		{

			if (LoadApplyFilterExOrgHanged is not null)
				dispatcher.InvokeAsync(async () => await LoadApplyFilterExOrgHanged.Invoke());
		}


		private async Task MainViewModel_LoadApplyFilterExOrghanged()
		{
			//Задержка, перед применением фильтра, для плавного поиска при наборе текста в поиске
			var token = _filterCancellationTokenSourceExOrg?.Token ?? CancellationToken.None;

			try
			{
				await Task.Delay(400, token).ConfigureAwait(false);
				await ApplyFilterExOrg().ConfigureAwait(false);
			}
			catch (OperationCanceledException) { }
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
			}

		}


		#endregion

		public string FilterNameExOrg
		{
			get => _filterNameExOrg;
			set
			{
				SetProperty(ref _filterNameExOrg, value);

				_filterCancellationTokenSourceExOrg?.Cancel();
				_filterCancellationTokenSourceExOrg?.Dispose();
				_filterCancellationTokenSourceExOrg = new CancellationTokenSource();

				UpdateIsLoadApplyFilterExOrgHanged();
			}
		}

		private string _filterNameExOrg;

		#endregion

		#region Methods
		private async Task GetDepartmentProductionsExOrgAsync(string userName, IEnumerable<DepartmentProductionDto> departments)
		{
			try
			{
				NamesDepartmentOExOrg = [];

				ValueDepartmentID = userName.GetDepartmentAsync();

				//Программистам  и Фафашкину доп. на просмотр
				if (string.IsNullOrEmpty(ValueDepartmentID) && !SOAccess()) return;

				if (SOAccess())
				{
					//Разрешенные участки для работы с людьми из СО
					List<string> departmentsIdVirtual = ["048", "049", "044", "045", "015", "051"];

					//Копия 
					var tempDep = new List<DepartmentProductionDto>(departments);

					//Фафашкину даём доступ на СО виртуальные участки, для работы с ними
					if (UserDataCurrent.UserName.Contains("ceh07", StringComparison.OrdinalIgnoreCase))
					{
						if (!tempDep.Any(x => x.DepartmentID.Contains("015")))
							tempDep.Add(new DepartmentProductionDto
							{
								DepartmentID = "015",
								NameDepartment = "АХО",
							});

						if (!tempDep.Any(x => x.DepartmentID.Contains("045")))
							tempDep.Add(new DepartmentProductionDto
							{
								DepartmentID = "045",
								NameDepartment = "ПДГ",
							});
					}

					//Выбираем реальные участки для формирования виртуальных
					List<DepartmentProductionDto>? tempNames = tempDep
						.Where(x => departmentsIdVirtual.Any(z => z == x.DepartmentID))
						.ToList();

					//Составляем временный список виртуальных участков для СО по всем разрешенным участкам
					var tempListVirtDep = tempNames
						.Select(x => new DepartmentProductionDto
						{
							DepartmentID = x.DepartmentID,
							NameDepartment = $" СО для, {x.NameDepartment}",
						})
						.ToList();
					NamesDepartmentOExOrg.AddItems(tempListVirtDep);

					//Добавляем общий список для отображения полной инфы по СО для Наливайко Н.Б. и программистам и Фафашкину
					var virtualDepartmen = new DepartmentProductionDto
					{
						DepartmentID = "000",
						NameDepartment = $" Все сотрудники СО",
					};

					NamesDepartmentOExOrg.Add(virtualDepartmen);
				}
				else
				{
					var tempName = NamesDepartment
						.Where(x => x.DepartmentID == ValueDepartmentID)
						.Select(x => x.NameDepartment)
						.FirstOrDefault();

					var virtualDepartmen = new DepartmentProductionDto
					{
						DepartmentID = ValueDepartmentID,
						NameDepartment = $" СО для, {tempName}",
					};
					NamesDepartmentOExOrg.Add(virtualDepartmen);
				}
				NamesDepartmentItemOExOrg = NamesDepartmentOExOrg.FirstOrDefault();
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);

				await ShowErrorInfoAsync(textError);
			}
		}

		/// <summary>
		/// Рассчитывает элементы табеля учета рабочего времени для ТО.
		/// </summary>
		private async Task SetTimeSheetItemsExOrgAsync()
		{
			try
			{
				//Конфигурируем период дат, из выбранных в приложении месяца и года
				StartDateExOrg = new DateTime(day: 1, month: ItemMonthsTOExOrg.Id, year: ItemYearsTOExOrg.Id);
				EndDateExOrg = new DateTime(day: MaxDayTOExOrg, month: ItemMonthsTOExOrg.Id, year: ItemYearsTOExOrg.Id);

				NoWorkDaysTO = await SetNoWorkDaysTOAsync(ItemYearsTOExOrg.Id, ItemMonthsTOExOrg.Id).ConfigureAwait(false);

				//Проверяем, если прав нет и стоит заглушка в выбранном участке - то выходим из расчёта
				if (string.IsNullOrEmpty(ValueDepartmentID) && !SOAccess()) return;

				DataForTimeSheetExOrgs dataForTimeSheetEx = new DataForTimeSheetExOrgs
				{
					ValueDepartmentID = NamesDepartmentItemOExOrg.DepartmentID,
					StartDate = StartDateExOrg,
					EndDate = EndDateExOrg,
					ItemMonthsTO = _mapper.Map<MonthsOrYearsDto>(ItemMonthsTOExOrg),
					ItemYearsTO = _mapper.Map<MonthsOrYearsDto>(ItemYearsTOExOrg),
					NoWorkDaysTO = NoWorkDaysTO,
					FlagAllEmployeeExOrg = GlobalProperty.FlagAllEmployeeExOrg
				};
				var response = await _employeeExOrgSheetApi.SetDataForTimeSheetExOrgAsync(dataForTimeSheetEx).ConfigureAwait(false);
				//Временное решение, отменяет флаг уволеннения, чтобы прошла валидация при маппинге данных из апи в модель. Без неё, смены\часы не ставятся
				foreach (var iteResp in response)
				{
					if (iteResp.WorkerHours.Any(x => x.EmployeeExOrg.IsDismissal))
					{
						iteResp.WorkerHours.Foreach(x =>
						{
							if (x.EmployeeExOrg.IsDismissal)
								x.EmployeeExOrg.IsDismissal = false;
						});
					}
				}

				var tempShifts = _mapper.Map<List<TimeSheetItemExOrg>>(response);
				//Возвращяем флаг уволнения. Проверяем по дате уволнения (не равна по умолчанию) и флаг уволнения отменен - то ставим флаг уволнения в true
				foreach (var itemTemp in tempShifts)
				{
					if (itemTemp.WorkerHours.Any(x => x.EmployeeExOrg.DateDismissal != DateTime.Parse("31.12.1876")))
					{
						itemTemp.WorkerHours.Foreach(x =>
						{
							if (x.EmployeeExOrg.DateDismissal != DateTime.Parse("31.12.1876") && x.EmployeeExOrg.IsDismissal == false)
								x.EmployeeExOrg.IsDismissal = true;
						});
					}
				}


				//Если сотрудник уволен в выбранном месяце, то его ФИО красятся в красный. Все остальные случаи - в черный
				foreach (var item in tempShifts)
				{
					var isDismissal = item.WorkerHours.Any(x => x.EmployeeExOrg.DateDismissal.Month == ItemMonthsTOExOrg.Id &&
																x.EmployeeExOrg.DateDismissal.Year == ItemYearsTOExOrg.Id);
					if (isDismissal)
						item.Brush = Brushes.Red;
					else
						item.Brush = Brushes.Black;

					item.WorkerHours.Foreach(x =>
					{
						Brush brush = x.GetBrushARGB();
						x.Brush = brush;
					});
				}


				//Готовые данные табеля отдаём ресурсу для отрисовки табеля в приложении
				if (!string.IsNullOrEmpty(FilterName))
				{
					DoubleTimeSheetsExOrgForSearch = new ObservableCollection<TimeSheetItemExOrg>(tempShifts);
					await ApplyFilterExOrg();
				}
				else
				{
					//Готовые данные табеля отдаём ресурсу для отрисовки табеля в приложении
					TimeSheetsExOrg = new ObservableCollection<TimeSheetItemExOrg>(tempShifts);
					DoubleTimeSheetsExOrgForSearch = new ObservableCollection<TimeSheetItemExOrg>(tempShifts);
				}
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);

				await ShowErrorInfoAsync(textError);
			}
		}
		#endregion

		#region Property

		/// <summary>
		/// Коллекция информации по табелю на сотрудников производства
		/// </summary>
		public ObservableCollection<TimeSheetItemExOrg> TimeSheetsExOrg
		{
			get => _timeSheetsExOrg;
			set
			{
				if (_timeSheetsExOrg != null)
				{
					foreach (var item in _timeSheetsExOrg)
					{
						item.WorkerHours.CollectionChanged -= TimeSheetsExOrg_CollectionChanged;
						item.WorkerHours.AsParallel().ForAll(underItem =>
						{
							underItem.PropertyChanged -= ItemExOrg_PropertyChanged;
						});
					}
				}

				SetProperty(ref _timeSheetsExOrg, value);

				if (_timeSheetsExOrg != null)
				{
					foreach (var item in _timeSheetsExOrg)
					{
						item.WorkerHours.CollectionChanged += TimeSheetsExOrg_CollectionChanged;
						item.WorkerHours.AsParallel().ForAll(underItem =>
						{
							underItem.PropertyChanged += ItemExOrg_PropertyChanged;
						});
					}
				}
			}
		}
		private ObservableCollection<TimeSheetItemExOrg> _timeSheetsExOrg;

		/// <summary>
		/// Выбранный сотрудник с его графиком
		/// </summary>
		public TimeSheetItemExOrg TimeSheetOneExOrg
		{
			get => _timeSheetOneExOrg;
			set => SetProperty(ref _timeSheetOneExOrg, value);
		}
		private TimeSheetItemExOrg _timeSheetOneExOrg;

		/// <summary>Стартовая дата</summary>
		public DateTime StartDateExOrg { get; private set; }

		/// <summary>Финишная дата</summary>
		public DateTime EndDateExOrg { get; private set; }

		/// <summary>Макс день месяца</summary>
		public int MaxDayTOExOrg
		{
			get => _maxDayTOExOrg;
			set => SetProperty(ref _maxDayTOExOrg, value);

		}
		private int _maxDayTOExOrg;

		/// <summary>
		/// Выбранный год Табель ТО
		/// </summary>
		public MonthsOrYears ItemYearsTOExOrg
		{
			get => _itemYearsTOExOrg;
			set
			{
				SetProperty(ref _itemYearsTOExOrg, value);
				if (ItemYearsTOExOrg != null && ItemMonthsTOExOrg != null)
					MaxDayTOExOrg = DateTime.DaysInMonth(ItemYearsTOExOrg.Id, ItemMonthsTOExOrg.Id);
				UpdateIsLoadedSO();
			}
		}
		private MonthsOrYears _itemYearsTOExOrg;

		/// <summary>
		/// Список для выбора года Табель ТО
		/// </summary>
		public ObservableCollection<MonthsOrYears> ListYearsTOExOrg
		{
			get => _listYearsTOExOrg;
			set => SetProperty(ref _listYearsTOExOrg, value);
		}
		private ObservableCollection<MonthsOrYears> _listYearsTOExOrg;

		/// <summary>
		/// Список месяцев в году, для отображения его на форме Табель ТО
		/// </summary>
		public ObservableCollection<MonthsOrYears>? ListMonthsTOExOrg
		{
			get => _listMonthsTOExOrg;
			set => SetProperty(ref _listMonthsTOExOrg, value);
		}
		private ObservableCollection<MonthsOrYears>? _listMonthsTOExOrg;

		/// <summary>
		/// Выбранный месяц на форме Табель ТО
		/// </summary>
		public MonthsOrYears ItemMonthsTOExOrg
		{
			get => _itemMonthTOExOrg;
			set
			{
				SetProperty(ref _itemMonthTOExOrg, value);
				if (ItemYearsTOExOrg != null && ItemMonthsTOExOrg != null)
					MaxDayTOExOrg = DateTime.DaysInMonth(ItemYearsTOExOrg.Id, ItemMonthsTOExOrg.Id);
				UpdateIsLoadedSO();
			}
		}
		private MonthsOrYears _itemMonthTOExOrg;


		/// <summary>
		/// Список участков предприятия
		/// </summary>
		public ObservableCollection<DepartmentProductionDto> NamesDepartmentOExOrg
		{
			get => _namesDepartmentOExOrg;
			set
			{
				SetProperty(ref _namesDepartmentOExOrg, value);
			}
		}
		private ObservableCollection<DepartmentProductionDto> _namesDepartmentOExOrg;

		/// <summary>
		/// Выбранный участок предприятия
		/// </summary>
		public DepartmentProductionDto NamesDepartmentItemOExOrg
		{
			get => _namesDepartmentItemOExOrg;
			set
			{
				SetProperty(ref _namesDepartmentItemOExOrg, value);

				if (NamesDepartmentItemOExOrg.DepartmentID.Contains("000"))
					GlobalProperty.FlagAllEmployeeExOrg = true;
				else
					GlobalProperty.FlagAllEmployeeExOrg = false;

				UpdateIsLoadedSO();
			}
		}
		private DepartmentProductionDto _namesDepartmentItemOExOrg;
		#endregion

		#region Event Handlers

		/// <summary>
		/// Событие возникает, когда данные готовы к расчёту. И запускается ассинхронный метод расчёта
		/// </summary>
		private async Task MainViewModelExOrg_LoadTOChanged()
		{
			try
			{
				await SetTimeSheetItemsExOrgAsync().ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
			}
		}



		/// <summary>
		/// Событие на отслеживание изменений у каждого из свойств класса ShiftData
		/// Чтобы реагировать на уровень-два кода выше, чем данные свойств класса ShiftData
		/// </summary>
		private async void ItemExOrg_PropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			try
			{
				if (e.PropertyName == nameof(ShiftDataExOrgDto.Hours) || e.PropertyName == nameof(ShiftDataExOrgDto.CodeColor))
				{
					if (sender is ShiftDataExOrgDto shiftDataExOrgDto)
					{
						var shiftDataExOrg = _mapper.Map<ShiftDataExOrg>(shiftDataExOrgDto);
						await _employeeExOrgSheetApi.SetTotalWorksDaysExOrgAsync(shiftDataExOrg).ConfigureAwait(false);
					}
				}

			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
			}
		}

		/// <summary>
		/// Событие для отслеживания изменений у самой ObservableCollection (удаление, добавление).
		/// Для того, чтобы при частичных изменениях, новые данные всегда были подписаны на событие. 
		/// А удаляемые - отписаны (чтобы данные удалились)
		/// </summary>
		private void TimeSheetsExOrg_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			try
			{
				if (e.NewItems != null)
				{
					foreach (ShiftDataExOrgDto item in e.NewItems)
						item.PropertyChanged += ItemExOrg_PropertyChanged;
				}
				if (e.OldItems != null)
				{
					foreach (ShiftDataExOrgDto item in e.OldItems)
					{
						item.PropertyChanged -= ItemExOrg_PropertyChanged;
					}
				}
			}
			catch (Exception ex)
			{
				_errorLogger.ProcessingErrorLog(ex);

				ShowErrorInfoAsync(textError).ConfigureAwait(false);
			}
		}

		#endregion

		#endregion

		#region Журналы Табеля для ручного ведения

		#region UseControl

		#region Properties

		/// <summary>Доп свойство для хранения фокуса на выбранном сотруднике при работе с обедами в кастомном окне</summary>
		public TimeSheetItem? TimeSheetOneFocus { get; private set; }

		/// <summary>
		/// св-во для хранения ручного проставление даты уволнения для табеля
		/// </summary>
		public DateTime ManualDateDismissal
		{
			get => _manualDateDismissal;
			set => SetProperty(ref _manualDateDismissal, value);
		}
		private DateTime _manualDateDismissal;

		/// <summary>
		/// св-во для хранения даты для обеда, за прошлый период
		/// </summary>
		public DateTime ManualLastDateLunch
		{
			get => _manualLastDateLunch;
			set
			{
				SetProperty(ref _manualLastDateLunch, value);
				UpdateIsManualLastDateLunch();
			}
		}
		private DateTime _manualLastDateLunch;

		#region Handle для события изменения даты обеда

		/// <summary>
		/// Устанавливает дату обеда для ручного проставления через событие UpdateDateLunchChanged.
		/// </summary>
		private void UpdateIsManualLastDateLunch()
		{
			if (UpdateDateLunchChanged is not null)
				dispatcher.InvokeAsync(async () => await UpdateDateLunchChanged.Invoke());
		}

		/// <summary>
		/// Событие для обновления текста обеда в кастомном окне
		/// </summary>
		public event Func<Task> UpdateDateLunchChanged;

		/// <summary>
		/// Асинхронный обработчик события изменения даты обеда.
		/// </summary>
		/// <returns></returns>
		private async Task MainViewModel_UpdateDateLunchChanged()
		{
			try
			{
				await SetTextIsLunchAsync().ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
			}
		}

		#endregion

		public string TextIsLunch
		{
			get => _textIsLunch;
			set => SetProperty(ref _textIsLunch, value);
		}
		private string _textIsLunch;
		public CustomDialog CustomDialogsForLunch { get; private set; }
		public CustomDialog CustomDialogs { get; private set; }

		/// <summary>
		/// По-умолчанию дата в ИС-ПРО ставится "31.12.1876". 
		/// </summary>
		public DateTime DefaultDateDismissal { get; private set; } = DateTime.Parse("31.12.1876");
		#endregion

		#region Command

		public ICommand UpdCmd { get; private set; }
		public ICommand UpdLunchCmd { get; private set; }
		public ICommand CloseCmd { get; private set; }
		public ICommand HandlerCommandDismissOrRescindDismissalCmd { get; private set; }

		public Visibility Visibility { get => _visibility; set => SetProperty(ref _visibility, value); }
		private Visibility _visibility;
		public Visibility Visibility043_044 { get => _visibility043_044; set => SetProperty(ref _visibility043_044, value); }
		private Visibility _visibility043_044;
		public Visibility VisibilityButtonAdditionally { get => _visibilityBA; set => SetProperty(ref _visibilityBA, value); }
		private Visibility _visibilityBA;
		public Visibility VisibilityForExOrg { get => _visibilityForExOrg; set => SetProperty(ref _visibilityForExOrg, value); }
		private Visibility _visibilityForExOrg;

		public Visibility VisibilityButtonForExOrg { get => _visibilityButtonForExOrg; set => SetProperty(ref _visibilityButtonForExOrg, value); }
		private Visibility _visibilityButtonForExOrg;

		public Visibility VisibilityCreateReportMonthlySummary { get; private set; }
		public ICommand RunCustomDialogForDismissalCmd { get; private set; }
		public ICommand RunCustomDialogForLunchCmd { get; private set; }
		public ICommand CloseLunchCmd { get; private set; }
		public ICommand ShowResultSheetCmd { get; private set; }
		public ICommand ShowFAQWindowCmd { get; private set; }
		public ICommand ShowStaffWindowCmd { get; private set; }

		public ICommand ShowStaffExOrgWindowCmd { get; private set; }

		public ICommand PlanLaborCmd { get; private set; }

		#endregion

		#region Methods

		private async Task SetTextIsLunchAsync()
		{
			try
			{
				//Проверки
				if (TimeSheetOne is null) return;
				if (ManualLastDateLunch > DateTime.Now.Date)
				{
					TextIsLunch = "Заказы на будущие дни нельзя редактировать";
					return;
				}

				//Получаем табельный номер сотрудника
				var idEmployee = TimeSheetOne.WorkerHours
					.Select(x => x.EmployeeID)
					.FirstOrDefault();

				IdEmployeeDateTime idEmployeeDateTime = new IdEmployeeDateTime { IdEmployee = idEmployee, Date = ManualLastDateLunch };
				var response = await _employeeSheetApi.GetEmployeeIdAndDateAsync(idEmployeeDateTime).ConfigureAwait(false);
				var itemEmployee = _mapper.Map<EmployeeDto>(response);

				if (itemEmployee == null) return;

				if (itemEmployee.IsDismissal)
				{
					TextIsLunch = "Сотрудник уволен. Нельзя редактировать";
					return;
				}

				var isEats = itemEmployee?.Shifts?
					.FirstOrDefault(x => x.WorkDate == ManualLastDateLunch)?.IsHaveLunch;

				if (isEats == null) return;
				if (isEats == true) TextIsLunch = "Заказан";
				else TextIsLunch = "Не заказан";
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
				throw;
			}
		}

		/// <summary>
		/// Асинхронный обработчик закрытия кастомного диалога
		/// </summary>
		/// <returns></returns>
		private async Task CloseAsync()
		{
			try
			{
				await _coordinator.HideMetroDialogAsync(this, CustomDialogs);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
			}
		}
		/// <summary>
		/// Асинхронный обработчик закрытия кастомного диалога
		/// </summary>
		/// <returns></returns>
		private async Task CloseSelectedDateAsync()
		{
			try
			{
				await _coordinator.HideMetroDialogAsync(this, CustomDialogsSelectedDate);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
			}
		}
		/// <summary>
		/// Асинхронный обработчик закрытия кастомного диалога
		/// </summary>
		/// <returns></returns>
		private async Task CloseLunchAsync()
		{
			try
			{
				await _coordinator.HideMetroDialogAsync(this, CustomDialogsForLunch);
				await SetTimeSheetItemsAsync();

				if (TimeSheetOneFocus != null && TimeSheets != null && TimeSheets.Count > 0)
				{
					TimeSheetOne = TimeSheets.FirstOrDefault(x => TimeSheetOneFocus.Id == x.Id);
					TimeSheetOneFocus = null;
				}
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
			}
		}

		/// <summary>
		/// Асинхронный метод по обновлению даты уволнения у выбранного сотрудника
		/// </summary>
		private async Task UpdAsync()
		{
			try
			{
				//Проверки
				if (TimeSheetOne is null) return;
				if (ManualDateDismissal == DefaultDateDismissal) return;

				//Получаем табельный номер сотрудника
				var idEmployee = TimeSheetOne.WorkerHours
					.Select(x => x.EmployeeID)
					.FirstOrDefault();

				IdEmployeeDateTime idEmployeeDateTime = new IdEmployeeDateTime { Date = ManualDateDismissal, IdEmployee = idEmployee };

				var check = await _employeeSheetApi.UpdateDismissalDataEmployeeAsync(idEmployeeDateTime);

				//Закрываем окно
				await _coordinator.HideMetroDialogAsync(this, CustomDialogs);

				//Обновляем табель после изменений
				if (check == true)
					await SetTimeSheetItemsAsync();
				else
					await _coordinator.ShowMessageAsync(this, "Ошибка",
						"Не найден сотрудник по его табельному номеру");
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
			}
		}

		/// <summary>
		/// Асинхронный метод запуска кастомного диалогового окна 
		/// для установки даты уволнения сотрудника
		/// </summary>
		private async Task RunCustomDialogAsyncForDismissal()
		{
			try
			{
				ManualDateDismissal = StartDate;
				//Создаём новое кастомное окно 
				CustomDialogs = new CustomDialog
				{
					//Привязываем кастомное окно к нашей View-модели
					Content = new DismissalEmployee(this)
				};

				//Настраиваем поведение анимации
				MetroDialogSettings settings = new() { AnimateShow = true, AnimateHide = true };

				//Показываем окно
				await _coordinator.ShowMetroDialogAsync(this, CustomDialogs, settings);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
			}
		}

		/// <summary>
		/// Метод по распредеоению логики, увольняем сотрудника или наоборот - отменяем увольнение
		/// </summary>
		private async Task HandlerCommandDismissOrRescindDismissalAsync()
		{
			try
			{
				if (TimeSheetOne is null) return;

				//Получаем табельный номер сотрудника
				var idEmployee = TimeSheetOne.WorkerHours.Select(x => x.EmployeeID).FirstOrDefault();
				if (idEmployee == 0) return;

				IdEmployeeDateTime idEmployeeDateTime = new IdEmployeeDateTime { Date = DefaultDateDismissal, IdEmployee = idEmployee };
				var check = await _employeeSheetApi.CancelDismissalEmployeeAsync(idEmployeeDateTime);

				if (check == true)
					//Обновляем табель после изменений
					await SetTimeSheetItemsAsync();
				else
					await RunCustomDialogAsyncForDismissal();
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
			}
		}

		private async Task UpdLunchAsync()
		{
			try
			{
				//Проверки
				if (TimeSheetOne is null) return;
				if (ManualLastDateLunch > DateTime.Now.Date) return;

				//Получаем табельный номер сотрудника
				var idEmployee = TimeSheetOne.WorkerHours.Select(x => x.EmployeeID).FirstOrDefault();

				IdEmployeeDateTime idEmployeeDateTime = new IdEmployeeDateTime { IdEmployee = idEmployee, Date = ManualLastDateLunch };
				var check = await _employeeSheetApi.UpdateLunchEmployeeAsync(idEmployeeDateTime);

				if (check == true)
				{
					await SetTextIsLunchAsync().ConfigureAwait(false);
				}
				else if (check == false)
				{
					//Закрываем окно
					await _coordinator.HideMetroDialogAsync(this, CustomDialogsForLunch);
					await _coordinator.ShowMessageAsync(this, "Ошибка", "Данный сотрудник уволен, нельзя проставить ему обед");
				}
				else
				{
					//Закрываем окно
					await _coordinator.HideMetroDialogAsync(this, CustomDialogsForLunch);
					await _coordinator.ShowMessageAsync(this, "Ошибка", "Не найден сотрудник по его табельному номеру");
				}
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
			}
		}

		/// <summary>
		/// Асинхронный метод запуска кастомного диалогового окна 
		/// для установки даты уволнения сотрудника
		/// </summary>
		private async Task RunCustomDialogAsyncForLunch()
		{
			try
			{
				TimeSheetOneFocus = null;
				if (TimeSheetOne != null) TimeSheetOneFocus = TimeSheetOne;

				ManualLastDateLunch = StartDate;
				//Создаём новое кастомное окно 
				CustomDialogsForLunch = new CustomDialog
				{
					//Привязываем кастомное окно к нашей View-модели
					Content = new EditingLastOrNowDayForLunchEmployee(this)
				};

				//Настраиваем поведение анимации
				MetroDialogSettings settings = new() { AnimateShow = true, AnimateHide = true };

				//Показываем окно
				await _coordinator.ShowMetroDialogAsync(this, CustomDialogsForLunch, settings);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
			}
		}
		#endregion

		#endregion

		#region Search

		#region Handler

		public event Func<Task> LoadApplyFilterHanged;
		private CancellationTokenSource? _filterCancellationTokenSource;

		/// <summary>
		/// Вызываем обработчик события на UI-потоке
		/// </summary>
		private void UpdateIsLoadApplyFilterHanged()
		{

			if (LoadApplyFilterHanged is not null)
				dispatcher.InvokeAsync(async () => await LoadApplyFilterHanged.Invoke());
		}

		/// <summary>
		/// Точка входа фильтра
		/// </summary>
		/// <returns></returns>
		private async Task MainViewModel_LoadApplyFilterHanged()
		{
			// Захватываем токен на момент вызова
			var token = _filterCancellationTokenSource?.Token ?? CancellationToken.None;

			try
			{
				// Ждём 400 мс или отмены
				await Task.Delay(400, token).ConfigureAwait(false);
				// Если токен не отменился — применяем фильтр
				await ApplyFilter().ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				// тишина — debounce отменён, придёт новый вызов
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
			}

		}


		#endregion

		/// <summary>
		/// Применяем фильтр напрямую к коллекции. Так как DataGrid не работает с фильтрами ICollectionView как ListView
		/// </summary>
		/// <returns></returns>
		private async Task ApplyFilter()
		{
			try
			{
				var filteredList = DoubleTimeSheetsForSearch?
					.Where(item => string.IsNullOrEmpty(FilterName) ||
						item.FioShiftOverday.ShortName.Contains(FilterName, StringComparison.OrdinalIgnoreCase))
					.ToList() ?? [];

				TimeSheets = new ObservableCollection<TimeSheetItem>(filteredList);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
			}
		}


		/// <summary>
		/// Свойство для поиска по ФИО на форме табеля
		/// </summary>
		public string FilterName
		{
			get => _filterName;
			set
			{
				SetProperty(ref _filterName, value);

				// каждый раз новый CTS, предыдущий отменяем
				_filterCancellationTokenSource?.Cancel();
				_filterCancellationTokenSource?.Dispose();
				_filterCancellationTokenSource = new CancellationTokenSource();

				//Вызываем обработчик фильтра
				UpdateIsLoadApplyFilterHanged();
			}
		}
		private string _filterName;

		#endregion

		#region Methods	

		/// <summary>
		/// Асинхронный метод для планирования рабочего времени.
		/// Получает данные о рабочих часах и сверхурочных часах для указанных регионов,
		/// обрабатывает их и отправляет результаты по электронной почте.
		/// </summary>
		private async Task PlanLaborAsync()
		{
			try
			{
				StartEndDateTime startEndDate = new StartEndDateTime { StartDate = StartDate, EndDate = EndDate };

				// Получение данных о рабочих часах и сверхурочных часах за указанный период для текущего пользователя
				var response = await _employeeSheetApi
					.GetTotalWorkingHoursWithOverdayHoursForRegions043and044Async(startEndDate).ConfigureAwait(false);

				var list = _mapper.Map<List<EmployeeDto>>(response);

				list = list.Where(x => x.ValidateEmployee(StartDate.Month, years: StartDate.Year)).ToList();

				StartEndDateTime startEndDateTime = new StartEndDateTime
				{
					StartDate = StartDate,
					EndDate = EndDate
				};

				var response2 = await _employeeExOrgSheetApi
					.GetTotalWorkingHoursWithOverdayHoursForRegions044EmployeeExpOrgsAsync(startEndDateTime)
					.ConfigureAwait(false);
				var listExpOrgs = _mapper.Map<List<EmployeeExOrgDto>>(response2);

				listExpOrgs = listExpOrgs.Where(x => x.ValidateEmployee(months: StartDate.Month, years: StartDate.Year)).ToList();
				listExpOrgs = listExpOrgs.Where(x => x.EmployeeExOrgAddInRegions != null && x.EmployeeExOrgAddInRegions.Any()).ToList();

				// Списки для хранения общих рабочих часов и сверхурочных часов
				List<double> totalHourse = [];
				List<double> totalOverday = [];
				List<int> days = [];
				// Итерация по каждому дню в указанном периоде
				for (var date = StartDate; date <= EndDate; date = date.AddDays(1))
				{
					days.Add(date.Day);
					double summaForDay = 0;
					double summaOverday = 0;

					bool isNotWeekend = false;
					bool isPreHoliday = false;

					// Обработка данных для каждого элемента в списке
					foreach (var item in list)
					{
						// Подсчет общего количества рабочих часов для указанного дня
						summaForDay += item.Shifts
							.Where(x => x.ValidationWorkingDaysOnDate(date))
							.Select(s => double.TryParse(s.Hours, out double tempValue) ? tempValue : 0)
							.SingleOrDefault();

						// Подсчет общего количества сверхурочных часов для указанного дня
						summaOverday += item.Shifts
							.Where(x => x.ValidationOverdayDaysOnDate(date))
							.Select(s => double.TryParse(s.Overday?.Replace(".", ","), out double tempValue)
										 ? tempValue : 0)
							.SingleOrDefault();
					}

					// Проверка, является ли день не выходным
					isNotWeekend = list.Any(z => z.Shifts.Any(x => x.ValidationWorkingDaysOnDate(date)));

					// Проверка, является ли день предпраздничным
					isPreHoliday = list
						.SelectMany(x => x.Shifts)
						.Where(z => z.ValidationWorkingDaysOnDate(date))
						.Select(s => s.IsPreHoliday)
						.FirstOrDefault();

					// Корректировка общего количества рабочих часов в зависимости от типа дня
					if (isNotWeekend)
					{
						if (isPreHoliday)
							summaForDay -= 14;
						else
							summaForDay -= 16;
					}

					double sumHoursInday = 0;

					foreach (var item in listExpOrgs)
					{
						sumHoursInday += item.ShiftDataExOrgs
							.Where(x => x.ValidationWorkingDaysOnDate(date))
							.Select(x => x.Hours.TryParseDouble(out double res) ? res : 0)
							.SingleOrDefault();
					}

					// Добавление подсчитанных значений в соответствующие списки
					totalHourse.Add(summaForDay + sumHoursInday);
					totalOverday.Add(summaOverday);
				}

				// Формирование HTML-сообщения с результатами
				var message = new StringBuilder();
				message.Append($"<table border='1' cols='{totalHourse.Count}' style='font-family:\"Courier New\", Courier, monospace'>");
				message.Append($"<tr>");

				foreach (var item in days)
					message.Append($"<td style='padding:5px'>{item}</td>");

				message.Append($"<tr>");
				foreach (var item in totalHourse)
					message.Append($"<td style='padding:5px'>{Math.Round(item, 1)}</td>");

				message.Append($"<tr>");
				foreach (var item in totalOverday)
					message.Append($"<td style='padding:5px'>{item}</td>");

				message.Append($"</table>");


				// Отправка сформированного сообщения по электронной почте
				await _errorLogger.SendMailPlanLaborAsync(message.ToString()).ConfigureAwait(false);

				await ShowErrorInfoAsync("Плановая трудоемкость успешно отправлена Вам на почту!");
			}
			catch (Exception ex)
			{
				// Логирование ошибки
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
			}
		}


		/// <summary>
		/// Устанавливает начальные значения месяца и года для производственного временного листа.
		/// </summary>
		private async Task SetMonthAndYear()
		{
			try
			{
				ListMonthsTO =
				[
					new MonthsOrYears(1, "Январь"),
					new MonthsOrYears(2, "Февраль"),
					new MonthsOrYears(3, "Март"),
					new MonthsOrYears(4, "Апрель"),
					new MonthsOrYears(5, "Май"),
					new MonthsOrYears(6, "Июнь"),
					new MonthsOrYears(7, "Июль"),
					new MonthsOrYears(8, "Август"),
					new MonthsOrYears(9, "Сентябрь"),
					new MonthsOrYears(10, "Октябрь"),
					new MonthsOrYears(11, "Ноябрь"),
					new MonthsOrYears(12, "Декабрь")
				];

				ListYearsTO = [];

				var currentYear = DateTime.Now.Year;

				if (DateTime.Now.Month == 12) currentYear += 1;

				for (int i = 2020; i <= currentYear; i++)
					ListYearsTO.Add(new MonthsOrYears(i, i.ToString()));

				ListMonthsTOExOrg = ListMonthsTO;
				ListYearsTOExOrg = ListYearsTO;
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
			}
		}

		/// <summary>
		/// Обновляет свойство IsLoadedTO на основе выбранных месяца, года и отдела.
		/// </summary>
		private void UpdateIsLoadedTO()
		{
			if (ItemMonthsTO != null && ItemYearsTO != null && NamesDepartmentItem != null && MaxDayTO > 0)
				if (LoadTOChanged is not null)
					dispatcher.InvokeAsync(async () => await LoadTOChanged.Invoke());
		}


		/// <summary>
		/// Конфигурируем список участков 
		/// </summary>
		private async Task GetDepartmentProductionsAsync()
		{
			try
			{
				if (EmployeeAccesses != null && EmployeeAccesses.Count > 0)
				{
					var tempNamesDepart = EmployeeAccesses.Select(x => x.DepartmentProduction).ToList() ?? [];

					if (tempNamesDepart is null || tempNamesDepart.Count == 0)
					{
						NamesDepartment = [];
						await _coordinator.ShowMessageAsync(this, "Права доступа",
							@" У Вас нет прав доступа к просмотру и\или редактирования для Табеля.");
					}
					else
					{
						tempNamesDepart.ForEach(x => x.FullNameDepartment = $"{x.DepartmentID} : {x.NameDepartment}");
						NamesDepartment = tempNamesDepart;
					}
				}
				else
				{
					NamesDepartment = [];
					await _coordinator.ShowMessageAsync(this, "Права доступа",
						@" У Вас нет прав доступа к просмотру и\или редактирования для Табеля.");
				}
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
			}
		}

		/// <summary>
		/// запрашиваем все данные по доступам.
		/// Затем ищем в данных, есть ли имя локального компьютера в тех данных
		/// Если есть, то получаем список прав
		/// </summary>
		/// <returns></returns>
		private async Task<List<EmployeeAccessRightDto>> GetAccessRightsAsync()
		{
			try
			{
				var response = await _employeeSheetApi.GetAccessRightsEmployeeAsync(UserDataCurrent.UserName);
				var employeeAccesses = _mapper.Map<List<EmployeeAccessRightDto>>(response);

				if (employeeAccesses is null || employeeAccesses.Count == 0)
				{
					//Если нет данных, инициируем ошибку. Для привлечения внимания к проблеме с доступом
					if (employeeAccesses is null || employeeAccesses.Count == 0)
						throw new Exception(@"Ошибка авторизации в методе GetDepartmentProductionsAsync. Либо нет прав для данного пользователя, либо 
					Таблица с правами доступа пуста!");
				}
				return employeeAccesses;
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
				return [];
			}
		}

		/// <summary>
		/// Устанавливает нерабочие дни для определенного месяца и года.
		/// </summary>
		/// <param name="year">Год (пример: 2024)</param>
		/// <param name="month">Номер месяца (пример: 5).</param>
		/// <returns>Список нерабочих дней.</returns>
		private async Task<List<int>> SetNoWorkDaysTOAsync(int year, int month)
		{
			try
			{
				if (CashListNoWorksDict.TryGetValue((month, year), out var _listNoWork))
					return _listNoWork;
				else
				{
					var listNoWork = await SetDaysNoWorkInMonthAsync(year, month).ConfigureAwait(false);
					CashListNoWorksDict[(month, year)] = listNoWork;
					return listNoWork;
				}
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
				return [];
			}
		}

		/// <summary>
		/// Обновляет таблицу данных, сравнивая и синхронизируя локальные и удаленные данные.
		/// </summary>
		private async Task UpdateDataTableAsync()
		{
			try
			{
				var controller =
					await _coordinator.ShowProgressAsync(this, "Пожалуйста, подождите!", "Идет обновление данных персонала...");

				controller.SetIndeterminate();

				var periodDate = new DateTime(year: ItemYearsTO.Id, month: ItemMonthsTO.Id, 1);

				string report = await _employeeSheetApi.UpdateDataTableNewEmployeeAsync(periodDate).ConfigureAwait(false);

				if (!string.IsNullOrEmpty(report))
					await SetTimeSheetItemsAsync().ConfigureAwait(false);

				await controller.CloseAsync();

				if (string.IsNullOrEmpty(report)) report = "Затронуто 0 записей.";
				await _coordinator.ShowMessageAsync(this, "Обновление персонала.", $"Обновление завершено!\n{report}");
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
			}
		}

		/// <summary>
		/// Рассчитывает элементы табеля учета рабочего времени для ТО.
		/// </summary>
		private async Task SetTimeSheetItemsAsync()
		{
			try
			{
				//Конфигурируем период дат, из выбранных в приложении месяца и года
				StartDate = new DateTime(day: 1, month: ItemMonthsTO.Id, year: ItemYearsTO.Id);
				EndDate = new DateTime(day: MaxDayTO, month: ItemMonthsTO.Id, year: ItemYearsTO.Id);

				NoWorkDaysTO = await SetNoWorkDaysTOAsync(ItemYearsTO.Id, ItemMonthsTO.Id).ConfigureAwait(false);

				//Проверяем, если прав нет и стоит заглушка в выбранном участке - то выходим из расчёта
				if (EmployeeAccesses is null || EmployeeAccesses.Count == 0
					|| NamesDepartmentItem.DepartmentID == "0000") return;

				EmployeeAccessRightDto? employeeAccessRight = EmployeeAccesses
					.FirstOrDefault(x => x.DepartmentID == NamesDepartmentItem.DepartmentID);

				if (employeeAccessRight is null) return;

				if (employeeAccessRight.RightEditOrSee == true)
					CheckingSeeOrWriteBool = await CheckingSeeOrWriteAsync(employeeAccessRight).ConfigureAwait(false);
				else
				{
					CheckingSeeOrWriteBool = false;

					DataForClearLastDeport dataForClear = new DataForClearLastDeport
					{
						LastAccessRightBool = LastAccessRightBool,
						LastSelectedDepartmentID = LastSelectedDepartmentID,
						EmployeeAccesses = _mapper.Map<List<EmployeeAccessRight>>(EmployeeAccesses),
					};

					if (LastAccessRightBool == true && !string.IsNullOrEmpty(LastSelectedDepartmentID))
						await _employeeSheetApi.ClearLastDeport(dataForClear).ConfigureAwait(false);
				}

				LastSelectedDepartmentID = NamesDepartmentItem.DepartmentID;
				LastAccessRightBool = CheckingSeeOrWriteBool;

				DataForTimeSheet dataForTimeSheet = new DataForTimeSheet
				{
					NamesDepartmentItem = _mapper.Map<DepartmentProduction>(NamesDepartmentItem),
					StartDate = StartDate,
					EndDate = EndDate,
					ItemMonthsTO = _mapper.Map<MonthsOrYearsDto>(ItemMonthsTO),
					ItemYearsTO = _mapper.Map<MonthsOrYearsDto>(ItemYearsTO),
					NoWorkDaysTO = NoWorkDaysTO,
					CheckingSeeOrWriteBool = CheckingSeeOrWriteBool
				};
				List<TimeSheetItemDto>? response = await _employeeSheetApi.SetDataForTimeSheetAsync(dataForTimeSheet).ConfigureAwait(false);
				//Временное решение, отменяет флаг уволеннения, чтобы прошла валидация при маппинге данных из апи в модель. Без неё, смены\часы не ставятся
				foreach (var itemResp in response)
				{
					if (itemResp.WorkerHours.Any(x => x.Employee.IsDismissal))
					{
						itemResp.WorkerHours.Foreach(x =>
						{
							if (x.Employee.IsDismissal)
								x.Employee.IsDismissal = false;
						});
					}
				}

				List<TimeSheetItem>? tempShifts = _mapper.Map<List<TimeSheetItem>>(response);
				//Возвращяем флаг уволнения. Проверяем по дате уволнения (не равна по умолчанию) и флаг уволнения отменен - то ставим флаг уволнения в true
				foreach (var itemTemp in tempShifts)
				{
					if (itemTemp.WorkerHours.Any(x => x.Employee.DateDismissal != DateTime.Parse("31.12.1876")))
					{
						itemTemp.WorkerHours.Foreach(x =>
						{
							if (x.Employee.DateDismissal != DateTime.Parse("31.12.1876") && x.Employee.IsDismissal == false)
								x.Employee.IsDismissal = true;
						});
					}
				}


				foreach (var item in tempShifts)
				{
					var isDismissal = item.WorkerHours.Any(x => x.Employee.DateDismissal.Month == ItemMonthsTO.Id &&
																x.Employee.DateDismissal.Year == ItemYearsTO.Id);
					if (isDismissal)
						item.Brush = Brushes.Red;
					else
						item.Brush = Brushes.Black;

					item.WorkerHours.Foreach(x =>
					{
						if (!string.IsNullOrEmpty(x.Shift))
							x.Brush = x.Shift.GetBrush();
					});
				}

				////Готовые данные табеля отдаём ресурсу для отрисовки табеля в приложении
				//TimeSheets = tempShifts;
				//DoubleTimeSheetsForSearch = new ObservableCollection<TimeSheetItem>(tempShifts);

				if (!string.IsNullOrEmpty(FilterName))
				{
					DoubleTimeSheetsForSearch = new ObservableCollection<TimeSheetItem>(tempShifts);
					await ApplyFilter();
				}
				else
				{
					//Готовые данные табеля отдаём ресурсу для отрисовки табеля в приложении
					TimeSheets = new ObservableCollection<TimeSheetItem>(tempShifts);
					DoubleTimeSheetsForSearch = new ObservableCollection<TimeSheetItem>(tempShifts);
				}

				if (ResultsSheet != null)
					await InitResultSheetAsync();
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
			}
		}

		/// <summary>
		/// Синхронный метод по очистке данных при завершении программы
		/// </summary>
		/// <returns></returns>
		public bool ClearIdAccessRightFromDepartment()
		{
			try
			{
				//Проверяем при переходе на другой участок, были ли предыдущие. Если да, то проводим проверки и очищаем данные.
				if (LastAccessRightBool != null && LastSelectedDepartmentID != null)
				{
					DataClearIdAccessRight dataClearId = new DataClearIdAccessRight
					{
						LastSelectedDepartmentID = LastSelectedDepartmentID,
						EmployeeAccesses = _mapper.Map<List<EmployeeAccessRight>>(EmployeeAccesses)
					};

					if (LastAccessRightBool == true && LastSelectedDepartmentID != string.Empty)
					{
						var result = _employeeSheetApi.ClearIdAccessRightFromDepartmentDbSync(dataClearId);
						return result;
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				_errorLogger.ProcessingErrorLog(ex);
				ShowErrorInfoAsync(textError)
					.ConfigureAwait(false);
				return true;
			}
		}

		/// <summary>
		/// Проверка, занят ли выбранный участок, другим человеком на редактирование. Если нет - сами занимаем. Если да - узнаём кто.
		/// </summary>
		/// <param name="employeeAccessRight">Право доступа текущего пользователя</param>
		/// <returns>Возвращаем право на редактирование или просмотр </returns>
		private async Task<bool> CheckingSeeOrWriteAsync(EmployeeAccessRightDto employeeAccessRight)
		{
			try
			{
				//Доп проверка от дурака
				if (employeeAccessRight.RightEditOrSee == false) return false;

				//Если прошлый запрос на редактирование был успешный, то перед переходом на другой участок - освобождаем занятый нами же участок 
				if (LastAccessRightBool == true && !string.IsNullOrEmpty(LastSelectedDepartmentID))
				{
					var DataForClearLastDeport = new DataForClearLastDeport
					{
						LastAccessRightBool = LastAccessRightBool,
						LastSelectedDepartmentID = LastSelectedDepartmentID,
						EmployeeAccesses = _mapper.Map<List<EmployeeAccessRight>>(EmployeeAccesses)
					};
					await _employeeSheetApi.ClearLastDeport(DataForClearLastDeport);
				}

				var currentMonths = DateTime.Now.Month;
				var currentYears = DateTime.Now.Year;
				var currentDay = DateTime.Now.Day;

				//Получаем состояние выбранного участка из БД
				var response = await _employeeSheetApi.GetDepartmentProductionAsync(employeeAccessRight.DepartmentID)
					.ConfigureAwait(false);

				var itemDepartment = _mapper.Map<DepartmentProductionDto>(response);

				//Если нет данных об участке
				if (itemDepartment is null) return false;


				//Табель на следующий месяц для редактирования доступен с 15 числа актуального месяца. В данном случае идет проверка для отказа доступа.  
				if (ValidationRightDateForFutureMonthWithFifteenDay(currentMonths, currentYears, currentDay)) return false;

				//Табель за прошлый месяц доступен для редактирования только Наливайко Н.Б. В данном случае идет проверка для отказа доступа остальным. 
				if (ValidationRightLastMonth(employeeAccessRight, currentMonths, currentYears)) return false;

				//Если текущий пользователь уже удерживает за собой участок на редактировании, то доступ продлен.
				if (itemDepartment.AccessRight == employeeAccessRight.EmployeeAccessRightId) return true;

				//Табель за прошлый месяц доступен для редактирования только Наливайко Н.Б.
				//Табель доступен всем у кого права на текущий месяц и будущий месяц (с 15 числа актуального месяца)

				if (TotalValidationRightForCurrentMonth(employeeAccessRight, currentMonths, currentYears, currentDay))
				{
					//Если участок не занят уже другим пользователем
					if (itemDepartment.AccessRight == 0)
					{
						//Если не занят, то занимаем собой. И обновляем и сохраняем таблицу в БД
						itemDepartment.AccessRight = employeeAccessRight.EmployeeAccessRightId;
						var request = _mapper.Map<DepartmentProduction>(itemDepartment);
						return await _employeeSheetApi.UpdateDepartamentAsync(request).ConfigureAwait(false);
					}
					else
					{
						var request = _mapper.Map<DepartmentProduction>(itemDepartment);
						//Узнаём кто занял занял участок на редактирование
						var response2 = await _employeeSheetApi.GetEmployeeByIdAsync(request);
						EmployeeAccessRightDto? employeeOccupied = _mapper.Map<EmployeeAccessRightDto>(response2);

						//Сообщаем об этом пользователю
						await _coordinator.ShowMessageAsync(this, "Данные по сотруднику",
@$"Сотрудник: {employeeOccupied?.NamePeople ?? "Не известен"}, 
с именем комп.: {employeeOccupied?.NameUsers ?? "Не известен"}. 
Занял на редактирование участок: {itemDepartment.NameDepartment}.
Вам дан доступ только на просмотр.");

						return false;
					}
				}
				else
					return false;
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
				return false;
				throw;
			}
		}

		private bool TotalValidationRightForCurrentMonth(EmployeeAccessRightDto employeeAccessRight, int currentMonths, int currentYears, int currentDay)
		{
			bool previousDateCheck = StartDate.Month == (currentMonths == 1 ? 12 : currentMonths - 1)
										&& StartDate.Year == (currentMonths == 1 ? currentYears - 1 : currentYears)
										&& employeeAccessRight.NamePeople.Equals("Наливайко Н.Б.");

			bool nowDateCheck = StartDate.Month == currentMonths && StartDate.Year == currentYears;

			bool nextDateCheck = StartDate.Month == (currentMonths == 12 ? 1 : currentMonths + 1)
										&& StartDate.Year == (currentMonths == 12 ? currentYears + 1 : currentYears)
										&& currentDay >= 15;

			return previousDateCheck || nowDateCheck || nextDateCheck;
		}

		private bool ValidationRightLastMonth(EmployeeAccessRightDto employeeAccessRight, int currentMonths, int currentYears)
		{
			int previousMonth = currentMonths == 1 ? 12 : currentMonths - 1;
			int previousYear = currentMonths == 1 ? currentYears - 1 : currentYears;

			return StartDate.Month <= previousMonth
				   && StartDate.Year == previousYear
				   && !employeeAccessRight.NamePeople.Equals("Наливайко Н.Б.");
		}

		private bool ValidationRightDateForFutureMonthWithFifteenDay(int currentMonths, int currentYears, int currentDay)
		{
			int nextMonth = currentMonths == 12 ? 1 : currentMonths + 1;
			int nextYear = currentMonths == 12 ? currentYears + 1 : currentYears;

			return StartDate.Month == nextMonth && StartDate.Year == nextYear && currentDay < 15;
		}

		/// <summary>
		/// Формируем заказ обедов на актуальную дату, до 9:30 можно перезаписать данные.
		/// Заказ присылается на почту.
		/// </summary>
		private async Task FormulateReportForLunchEveryDayAsync()
		{
			try
			{
				var currentDate = DateTime.Now;
				if (currentDate.Hour <= 10)
				{

					var controller = await _coordinator.ShowProgressAsync(this, "Пожалуйста, подождите!", "Идет формирование заявки...");
					controller.SetIndeterminate();

					await _employeeSheetApi.CleareDataForFormulateReportForLunchEveryDayDbAsync().ConfigureAwait(false);

					//Дёргаем апишку, чтобы она сформировала заказ обедов на сегодня, и записала новые данные
					var resultCheck = await _reportsApi.GetOrderForLunchEveryDayAsync();

					await controller.CloseAsync();

					if (resultCheck)
					{
						await _coordinator.ShowMessageAsync(this, "Формирование отчёта", "Отчёт сформирован");
						await SetTimeSheetItemsAsync();
					}
					else
						await _coordinator.ShowMessageAsync(this, "Формирование отчёта", "Ошибки в формировании отчёта");
				}
				else
					await _coordinator.ShowMessageAsync(this, "Формирование отчёта", "Заказывать обед уже поздно. Данные не поменять.");
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
				throw;
			}
		}

		/// <summary>
		/// Запускаем формирование на сервере отчёта в Excel, по обедам за прошлый месяц.
		/// Также человек должен внести в диалогое окно сумму по счёту, на основе которой будет распределение денежной массы
		/// </summary>
		private async Task FormulateReportForLunchLastMonhtAsync()
		{
			try
			{
				//сумма по счёту, на основе которой будет распределение денежной массы на все обеды в прошлом месяце
				var totalSum = await _coordinator.ShowInputAsync(this, "Формируем отчёт по обедам за прошлый месяц", "Введите окончательную сумму за обеды за прошлый месяц");

				if (decimal.TryParse(totalSum, out decimal totalSumDecimal))
				{
					var controller = await _coordinator.ShowProgressAsync(this, "Пожалуйста, подождите!", "Идет формирование отчёта Excel...");

					controller.SetIndeterminate();

					var resultCheck = await _reportsApi.CreateOrderLunchLastMonthAsync(totalSum);

					await controller.CloseAsync();

					if (resultCheck)
						await _coordinator.ShowMessageAsync(this, "Формирование отчёта Excel", "Отчёт сформирован и отправлен на почту");
					else
						await _coordinator.ShowMessageAsync(this, "Формирование отчёта Excel", "Ошибки в формировании отчёта. Сообщите разработчикам ТО");
				}
				else
					await _coordinator.ShowMessageAsync(this, "Формирование отчёта Excel", "Вы ввели некорректную сумму. Исправьте и повторите, пожалуйста.");
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
			}
		}

		/// <summary>
		/// Если сотрудник обедает, то в его данных отображается инфа, что он кушает, и на него заказывается обед
		/// </summary>
		/// <param name="item">Сотрудник, выбранный в табеле</param>
		private async Task IsLunchingAsync()
		{
			try
			{
				if (TimeSheetOne is null) return;

				//Получаем табельный номер сотрудника
				var idEmployee = TimeSheetOne.WorkerHours.Select(x => x.EmployeeID).FirstOrDefault();
				if (idEmployee == 0) return;

				var check = await _employeeSheetApi.UpdateIsLunchingDbAsync(
					idEmployee).ConfigureAwait(false);

				if (check == true)
					//Обновляем табель после изменений
					await SetTimeSheetItemsAsync().ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);

				await ShowErrorInfoAsync(textError);
				throw;
			}
		}
		#endregion

		#region Event Handlers

		/// <summary>
		/// Событие возникает, когда данные готовы к расчёту. И запускается ассинхронный метод расчёта
		/// </summary>
		private async Task MainViewModel_LoadTOChanged()
		{
			try
			{
				await SetTimeSheetItemsAsync().ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
			}
		}

		/// <summary>
		/// Событие на отслеживание изменений у каждого из свойств класса ShiftData
		/// Чтобы реагировать на уровень-два кода выше, чем данные свойств класса ShiftData
		/// </summary>
		private async void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			try
			{
				if (e.PropertyName == nameof(ShiftDataDto.Hours) ||
				e.PropertyName == nameof(ShiftDataDto.Overday)
				|| e.PropertyName == nameof(ShiftDataDto.Shift))

				{
					if (sender is ShiftDataDto shiftDataDto)
					{
						var shiftData = _mapper.Map<ShiftData>(shiftDataDto);
						await _employeeSheetApi.SetTotalWorksDaysAsync(shiftData).ConfigureAwait(false);
					}
				}
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);
				await ShowErrorInfoAsync(textError);
			}
		}

		/// <summary>
		/// Событие для отслеживания изменений у самой ObservableCollection (удаление, добавление).
		/// Для того, чтобы при частичных изменениях, новые данные всегда были подписаны на событие. 
		/// А удаляемые - отписаны (чтобы данные удалились)
		/// </summary>
		private void TimeSheets_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			try
			{
				if (e.NewItems != null)
				{
					foreach (ShiftDataDto item in e.NewItems)
						item.PropertyChanged += Item_PropertyChanged;
				}
				if (e.OldItems != null)
				{
					foreach (ShiftDataDto item in e.OldItems)
					{
						item.PropertyChanged -= Item_PropertyChanged;
					}
				}
			}
			catch (Exception ex)
			{
				_errorLogger.ProcessingErrorLog(ex);

				ShowErrorInfoAsync(textError).ConfigureAwait(false);
				throw;
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// Дублированые данные, для хранения полноценного табеля при использовании поиска в табеле
		/// </summary>
		public ObservableCollection<TimeSheetItem> DoubleTimeSheetsForSearch { get; private set; }

		/// <summary>
		/// Список прав и разрешений 
		/// </summary>
		public List<EmployeeAccessRightDto> EmployeeAccesses { get; private set; }
		public string NamePeople { get; private set; }



		/// Сво-во отражает в каком варианте грузить шаблоны для табеля. 
		/// Если <see cref="true"/> - то право на редактирование открыто.
		/// Если <see cref="false"/> - то право на просмотр открыто.
		public bool CheckingSeeOrWriteBool { get; private set; }

		/// <summary>
		/// Днные по поледней записи, какой участок занимали в режиме редактирования
		/// </summary>
		public string? LastSelectedDepartmentID { get; private set; }

		/// <summary>
		/// Данные по поледней записи прав на чтение\запись
		/// </summary>
		public bool? LastAccessRightBool { get; private set; }

		/// <summary>
		/// Список участков предприятия
		/// </summary>
		public IEnumerable<DepartmentProductionDto> NamesDepartment
		{
			get => _namesDepartment;
			set
			{

				SetProperty(ref _namesDepartment, value);

				if (NamesDepartment != null || NamesDepartment?.Count() > 0)
				{
					NamesDepartmentItem = NamesDepartment.FirstOrDefault();
				}
				else
				{
					NamesDepartmentItem = new DepartmentProductionDto { DepartmentID = "0000", NameDepartment = @"<Нет доступа>" };
				}

			}
		}
		private IEnumerable<DepartmentProductionDto> _namesDepartment;

		/// <summary>
		/// Выбранный участок предприятия
		/// </summary>
		public DepartmentProductionDto NamesDepartmentItem
		{
			get => _namesDepartmentItem;
			set
			{
				SetProperty(ref _namesDepartmentItem, value);
				UpdateIsLoadedTO();
			}
		}
		private DepartmentProductionDto _namesDepartmentItem;

		/// <summary>
		/// Список не рабочих дней в месяце для Табеля ТО
		/// </summary>
		public List<int> NoWorkDaysTO { get => _noWorkDaysTimeSheet; set => SetProperty(ref _noWorkDaysTimeSheet, value); }
		private List<int> _noWorkDaysTimeSheet;

		/// <summary>Стартовая дата</summary>
		public DateTime StartDate { get; private set; }

		/// <summary>Финишная дата</summary>
		public DateTime EndDate { get; private set; }

		public DateTime StardPeriod { get => _stardPeriod; set => SetProperty(ref _stardPeriod, value); }
		private DateTime _stardPeriod;
		public DateTime EndPeriod { get => _endPeriod; set => SetProperty(ref _endPeriod, value); }
		private DateTime _endPeriod;

		/// <summary>Макс день месяца</summary>
		public int MaxDayTO
		{
			get => _maxDayTO;
			set
			{
				SetProperty(ref _maxDayTO, value);
			}
		}
		private int _maxDayTO;

		/// <summary>
		/// Выбранный год Табель ТО
		/// </summary>
		public MonthsOrYears ItemYearsTO
		{
			get => _itemYearsTO;
			set
			{
				SetProperty(ref _itemYearsTO, value);
				if (ItemYearsTO != null && ItemMonthsTO != null)
					MaxDayTO = DateTime.DaysInMonth(ItemYearsTO.Id, ItemMonthsTO.Id);
				UpdateIsLoadedTO();
			}
		}
		private MonthsOrYears _itemYearsTO;

		/// <summary>
		/// Список для выбора года Табель ТО
		/// </summary>
		public ObservableCollection<MonthsOrYears> ListYearsTO { get => _listYearsTO; set => SetProperty(ref _listYearsTO, value); }
		private ObservableCollection<MonthsOrYears> _listYearsTO;

		/// <summary>
		/// Список месяцев в году, для отображения его на форме Табель ТО
		/// </summary>
		public ObservableCollection<MonthsOrYears>? ListMonthsTO
		{
			get => _listMonthsTO;
			set => SetProperty(ref _listMonthsTO, value);
		}
		private ObservableCollection<MonthsOrYears>? _listMonthsTO;

		/// <summary>
		/// Выбранный месяц на форме Табель ТО
		/// </summary>
		public MonthsOrYears ItemMonthsTO
		{
			get => _itemMonthTO;
			set
			{
				SetProperty(ref _itemMonthTO, value);
				if (ItemYearsTO != null && ItemMonthsTO != null)
					MaxDayTO = DateTime.DaysInMonth(ItemYearsTO.Id, ItemMonthsTO.Id);
				UpdateIsLoadedTO();
			}
		}
		private MonthsOrYears _itemMonthTO;

		/// <summary>
		/// Коллекция информации по табелю на сотрудников производства
		/// </summary>
		public ObservableCollection<TimeSheetItem> TimeSheets
		{
			get => _timeSheets;
			set
			{
				if (_timeSheets != null)
				{
					foreach (var item in _timeSheets)
					{
						item.WorkerHours.CollectionChanged -= TimeSheets_CollectionChanged;
						item.WorkerHours.AsParallel().ForAll(underItem =>
						{
							underItem.PropertyChanged -= Item_PropertyChanged;
						});
					}
				}

				SetProperty(ref _timeSheets, value);

				if (_timeSheets != null)
				{
					foreach (var item in _timeSheets)
					{
						item.WorkerHours.CollectionChanged += TimeSheets_CollectionChanged;
						item.WorkerHours.AsParallel().ForAll(underItem =>
						{
							underItem.PropertyChanged += Item_PropertyChanged;
						});
					}
				}
			}
		}
		private ObservableCollection<TimeSheetItem> _timeSheets;

		/// <summary>
		/// Выбранный сотрудник с его графиком
		/// </summary>
		public TimeSheetItem TimeSheetOne
		{
			get => _timeSheetOne;
			set => SetProperty(ref _timeSheetOne, value);
		}
		private TimeSheetItem _timeSheetOne;

		#endregion

		#region Commands
		public ICommand UpdateDataEmployeeChangesCmd { get; set; }
		public ICommand FormulateReportEveryDayCmd { get; set; }
		public ICommand FormulateReportForLunchLastMonhtCmd { get; set; }
		public ICommand UpdateScheduleCmd { get; set; }
		public ICommand UpdateScheduleOxRegCmd { get; set; }
		public ICommand IsLunchCmd { get; set; }
		public ICommand CreateReportMonthlySummaryCmd { get; set; }
		public ICommand CreateReportMonthlySummaryForEmployeeExpOrgCmd { get; set; }
		public ICommand RunCreateReportCmd { get; set; }
		public ICommand CloseSelectedDateCmd { get; set; }


		#endregion

		#endregion

		#region Итоги Табеля

		#region Commands

		public ICommand UpdateResultSheetCmd { get; set; }
		public ICommand CreateReportResultSheetCmd { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Сохраняем отчёт по выбранному показателю в Итогах Табеля
		/// </summary>
		/// <returns></returns>
		private async Task CreateReportResultSheetAsync()
		{
			try
			{
				if (ItemIndicator is null)
				{
					await _coordinator.ShowMessageAsync(this, $"Создание отчёта для Итогов Табеля"
						, @"Нет данных для отчёта. Выберите показатель с данными и попробуйте снова!");
					return;
				}

				if (EmpIndicators.Count == 0 || EmpIndicators is null)
				{
					await _coordinator.ShowMessageAsync(this, $"Создание отчёта для {ItemIndicator.DescriptionIndicator}"
						, @"Нет данных для отчёта. Выберите показатель с данными и попробуйте снова!");
					return;
				}

				var dialog = new SaveFileDialog
				{
					FileName = $"Показатели для '{ItemIndicator.DescriptionIndicator}', Участка  {NamesDepartmentItem.NameDepartment}.",
					DefaultExt = ".xlsx",
					Filter = "Excel documents (.xlsx)|*.xlsx"
				};

				bool? resultSave = dialog.ShowDialog();
				if (resultSave == true)
				{
					var path = await _reportsApi.GetReportResultSheetsAsync([.. EmpIndicators]);

					if (string.IsNullOrEmpty(path)) return;

					File.Copy(path, dialog.FileName, true);

					await _coordinator.ShowMessageAsync(this, $"Создание отчёта для {ItemIndicator.DescriptionIndicator}", "Готово");
				}
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);

				await ShowErrorInfoAsync("Не удача! В создании отчёта для Итогов Табеля");
				throw;
			}
		}

		/// <summary>
		/// Инициируем данные с итогами табеля
		/// </summary>
		private async Task InitResultSheetAsync()
		{
			try
			{
				Indicators = [];

				if (ValidationForTimeSheetsAndUserDataCurrent()) return;

				DataForTimeSheet dataForTimeSheet = new DataForTimeSheet
				{
					NamesDepartmentItem = _mapper.Map<DepartmentProduction>(NamesDepartmentItem),
					StartDate = StartDate,
					EndDate = EndDate,
					ItemMonthsTO = _mapper.Map<MonthsOrYearsDto>(ItemMonthsTO),
					ItemYearsTO = _mapper.Map<MonthsOrYearsDto>(ItemYearsTO),
					NoWorkDaysTO = NoWorkDaysTO,
					CheckingSeeOrWriteBool = CheckingSeeOrWriteBool
				};

				Tuplet = await _resultSheetsApi.GetDataResultSheetAsync(dataForTimeSheet);

				Indicators = new ObservableCollection<IndicatorDto>(Tuplet.Indicators);

				TextEmpIndicator = string.Empty;
				TextIndicator = @$"Участок:  {NamesDepartmentItem.NameDepartment}.";
				TextCountEmployee = $"Кол-во работников:  {TimeSheets.Count}.";
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);

				await ShowErrorInfoAsync(textError);
			}
		}

		private bool ValidationForTimeSheetsAndUserDataCurrent()
		{
			return TimeSheets is null || TimeSheets.Count == 0;
		}

		/// <summary>
		/// Вызываем окно с итогами табеля
		/// </summary>
		/// <returns></returns>
		private async Task ShowResultSheet()
		{
			try
			{
				if (ValidationForTimeSheetsAndUserDataCurrent()) return;

				if (ResultsSheet == null)
				{
					FlagShowResultSheet = true;
					await InitResultSheetAsync();

					ResultsSheet = new(this);
					ResultsSheet.Closed += (sender, args) => ResultsSheet = null;
					ResultsSheet.Show();
				}
				else
				{
					ResultsSheet.Activate();
				}
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);

				await ShowErrorInfoAsync(textError);
			}
		}

		/// <summary>
		/// Получает данные о нерабочих днях в указанном месяце и году.
		/// </summary>
		/// <param name="year">Year.</param>
		/// <param name="month">Month.</param>
		/// <returns>Список нерабочих дней.</returns>
		private async Task<List<int>> SetDaysNoWorkInMonthAsync(int year, int month)
		{
			try
			{
				return await _daysApi.GetWeekendsInMonthAsync(year, month).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);

				await ShowErrorInfoAsync(textError);
				return [];
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// Окно с Итогами табеля
		/// </summary>
		public ResultsSheet? ResultsSheet
		{
			get => _resultsSheet;
			set => SetProperty(ref _resultsSheet, value);
		}
		private ResultsSheet? _resultsSheet;

		/// <summary>
		/// Выбранный показатель в итогах табеля
		/// </summary>
		public IndicatorDto ItemIndicator
		{
			get => _itemIndicator;
			set
			{
				SetProperty(ref _itemIndicator, value);

				if (ItemIndicator != null)
				{
					EmpIndicators = ItemIndicator.ShadowId switch
					{
						1 => [],
						2 => new ObservableCollection<EmployeesInIndicatorDto>(Tuplet.NNList),
						3 => new ObservableCollection<EmployeesInIndicatorDto>(Tuplet.Underday),
						4 => new ObservableCollection<EmployeesInIndicatorDto>(Tuplet.Overday),
						5 => new ObservableCollection<EmployeesInIndicatorDto>(Tuplet.Night),
						6 => new ObservableCollection<EmployeesInIndicatorDto>(Tuplet.Vacation),
						7 => new ObservableCollection<EmployeesInIndicatorDto>(Tuplet.ADVacation),
						8 => new ObservableCollection<EmployeesInIndicatorDto>(Tuplet.SickLeave),
						9 => new ObservableCollection<EmployeesInIndicatorDto>(Tuplet.Demobilized),
						10 => new ObservableCollection<EmployeesInIndicatorDto>(Tuplet.ParentalLeave),
						11 => new ObservableCollection<EmployeesInIndicatorDto>(Tuplet.InvalidLeave),
						12 => new ObservableCollection<EmployeesInIndicatorDto>(Tuplet.Dismissal),
						13 => new ObservableCollection<EmployeesInIndicatorDto>(Tuplet.Lunching),
						_ => [],
					};
					EmpIndicators.Foreach(x =>
					{
						x.IndicatorItem = ItemIndicator;
						x.NameDepartmentForApi = NamesDepartmentItem.NameDepartment ?? string.Empty;
					});

					if (ItemIndicator.ShadowId == 1) TextEmpIndicator = string.Empty;
					else
						TextEmpIndicator = $"Работники показ-ля:  {ItemIndicator.DescriptionIndicator}";
				}
				else EmpIndicators = [];
			}
		}
		private IndicatorDto _itemIndicator;

		/// <summary>
		/// Список сотрудников с данных показателей в итогах табеля
		/// </summary>
		public ObservableCollection<EmployeesInIndicatorDto> EmpIndicators
		{
			get => _empIndicators;
			set => SetProperty(ref _empIndicators, value);
		}
		private ObservableCollection<EmployeesInIndicatorDto> _empIndicators;

		/// <summary>
		/// Список показателей в итогах табеля
		/// </summary>
		public ObservableCollection<IndicatorDto> Indicators
		{
			get => _indicators;
			set => SetProperty(ref _indicators, value);
		}
		private ObservableCollection<IndicatorDto> _indicators;

		/// <summary>
		/// Кортеж со списками данных для итогов табеля
		/// </summary>
		public ResultSheetResponseDto Tuplet { get; private set; }

		/// <summary>
		/// Информация над показателями, к которым они относятся (участок, кол-во людей)
		/// </summary>
		public string TextIndicator
		{
			get => _textIndicator;
			set => SetProperty(ref _textIndicator, value);
		}
		private string _textIndicator;

		/// <summary>
		/// КОл-во людей для отображения в итогах табеля
		/// </summary>
		public string TextCountEmployee
		{
			get => _textCountEmployee;
			set => SetProperty(ref _textCountEmployee, value);
		}
		private string _textCountEmployee;

		/// <summary>
		/// Отображает текст, работников на выбранный показатель.
		/// </summary>
		public string TextEmpIndicator
		{
			get => _textEmpIndicator;
			set => SetProperty(ref _textEmpIndicator, value);
		}
		private string _textEmpIndicator;

		/// <summary>
		/// Словарь кэширования результатов для моментального получения уже просчитанных не рабочих дней в месяце года
		/// </summary>
		public ConcurrentDictionary<(int Month, int Year), List<int>> CashListNoWorksDict
		{
			get => _cashListNoWorksDict;
			set => SetProperty(ref _cashListNoWorksDict, value);
		}
		private ConcurrentDictionary<(int Month, int Year), List<int>> _cashListNoWorksDict;

		public bool FlagShowResultSheet { get; private set; }
		#endregion

		#endregion

		#region Вызов окон Картотеки и СПРАВКИ
		/// <summary>
		/// Окно с картотекой
		/// </summary>
		public StaffView? StaffView
		{
			get => _staffView;
			set => SetProperty(ref _staffView, value);
		}
		private StaffView? _staffView;


		public StaffExternalOrgView? StaffExOrgView
		{
			get => _staffExOrgView;
			set => SetProperty(ref _staffExOrgView, value);
		}
		private StaffExternalOrgView? _staffExOrgView;

		/// <summary>
		/// Окно со справкой
		/// </summary>
		public FAQ? FAQ
		{
			get => _faq;
			set => SetProperty(ref _faq, value);
		}
		public string ValueDepartmentID { get; private set; }
		public ObservableCollection<TimeSheetItemExOrg> DoubleTimeSheetsExOrgForSearch { get; private set; }


		private FAQ? _faq;

		/// <summary>
		/// Вызов окна справки
		/// </summary>
		private async Task ShowFAQWindowAsync()
		{
			try
			{
				if (FAQ == null)
				{
					FAQ = new();
					FAQ.Closed += (sender, args) => FAQ = null;
					FAQ.DataContext = FAQViewModel;
					FAQ.Show();
				}
				else
				{
					FAQ.Activate();
				}
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);

				await ShowErrorInfoAsync(textError);
				throw;
			}
		}

		/// <summary>
		/// Вызов окна картотеки
		/// </summary>
		private async Task ShowStaffWindowAsync()
		{
			try
			{
				if (StaffView == null)
				{
					StaffView = new();
					StaffView.Closed += (sender, args) => StaffView = null;
					await StaffViewModel.InitiazinigStaffAsync(StaffView);
					StaffView.DataContext = StaffViewModel;
					StaffView.Show();
				}
				else
				{
					StaffView.Activate();
				}
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);

				await ShowErrorInfoAsync(textError);
			}
		}

		/// <summary>
		/// Вызов окна картотеки СО
		/// </summary>
		private async Task ShowStaffExOrgWindowAsync()
		{
			try
			{
				if (StaffExOrgView == null)
				{
					StaffExOrgView = new();
					StaffExOrgView.Closed += (sender, args) => StaffExOrgView = null;
					await ExternalOrgViewModel.Initializing(StaffExOrgView);
					StaffExOrgView.DataContext = ExternalOrgViewModel;
					StaffExOrgView.Show();
				}
				else
				{
					StaffExOrgView.Activate();
				}
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex);

				await ShowErrorInfoAsync(textError);
			}
		}
		#endregion

		#region Template SpeedTest
		//var startTime = System.Diagnostics.Stopwatch.StartNew();

		//startTime.Stop();
		//var resultTime = startTime.Elapsed;
		//string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}",
		//    resultTime.Hours,
		//    resultTime.Minutes,
		//    resultTime.Seconds,
		//    resultTime.Milliseconds);
		//MessageBox.Show(elapsedTime);
		#endregion

		/// <summary>
		/// Показывает сообщение об ошибке.
		/// </summary>
		public async Task ShowErrorInfoAsync(string text)
		{
			if (!string.IsNullOrEmpty(text))
				await _coordinator.ShowMessageAsync(this, "Информация", text);
		}
	}
}
