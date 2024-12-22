using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MahApps.Metro.Controls.Dialogs;

using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Input;

using TimeSheets.DAL;
using TimeSheets.Entitys;
using TimeSheets.Entitys.ResultTimeSheet;
using TimeSheets.Models;
using TimeSheets.Services.API.Interfaces;
using TimeSheets.Services.Interfaces;
using TimeSheets.Utils;
using TimeSheets.Views;


namespace TimeSheets.ViewModel
{
	public class MainViewModel : ObservableObject
	{
		#region Поля		
		private readonly IDialogCoordinator _coordinator;
		private readonly IErrorLogger _errorLogger;
		private readonly IDbContextFactory<ShiftTimesDbContext> _context;
		private readonly IApiProductionControl _api;
		private readonly ITimeSheetDbService _timeSheetDb;
		private readonly IResultSheetsService _sheetsService;
		private StaffViewModel StaffViewModel { get; set; }
		private FAQViewModel FAQViewModel { get; set; }

		public string LocalMachineName { get; private set; }
		#endregion

		#region Конструктор

		/// <summary>
		/// Инициализирует новый экземпляр класса MainViewModel.
		/// </summary>		
		/// <param name="coordinator">Интерфейс координатора диалогов</param>
		/// <param name="errorLogger">Интерфейс регистратора ошибок.</param>
		/// <param name="context">Интерфейс фабрики DbContext</param>

		public MainViewModel(
			IDialogCoordinator coordinator,
			IErrorLogger errorLogger,
			IDbContextFactory<ShiftTimesDbContext> context,
			IApiProductionControl api,
			ITimeSheetDbService timeSheetDb,
			IResultSheetsService sheetsService,
			FAQViewModel fAQViewModel,
			StaffViewModel staffExternalOrgView
			)
		{
			try
			{
				LocalMachineName = Environment.MachineName;
				//LocalMachineName = "comp89";
				Visibility = Visibility.Collapsed;
				_errorLogger = errorLogger;
				this._coordinator = coordinator;
				_context = context;
				_api = api;
				_timeSheetDb = timeSheetDb;
				_sheetsService = sheetsService;
				FlagShowResultSheet = false;
				FAQViewModel = fAQViewModel;
				StaffViewModel = staffExternalOrgView;
			}
			catch (Exception ex)
			{
				_errorLogger?.ProcessingErrorLog(ex);
			}
		}
		#endregion

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

				Visibility = Visibility.Visible;
				RunCustomDialogForDismissalCmd = new AsyncRelayCommand(RunCustomDialogAsyncForDismissal);
				HandlerCommandDismissOrRescindDismissalCmd = new AsyncRelayCommand(HandlerCommandDismissOrRescindDismissalAsync);
				IsLunchCmd = new AsyncRelayCommand(IsLunchingAsync);
				UpdCmd = new AsyncRelayCommand(UpdAsync);
				CloseCmd = new AsyncRelayCommand(CloseAsync);
				RunCustomDialogForLunchCmd = new AsyncRelayCommand(RunCustomDialogAsyncForLunch);
				UpdLunchCmd = new AsyncRelayCommand(UpdLunchAsync);
				CloseLunchCmd = new AsyncRelayCommand(CloseLunchAsync);
				ShowStaffWindowCmd = new AsyncRelayCommand(ShowStaffWindowAsync);
				ShowFAQWindowCmd = new AsyncRelayCommand(ShowFAQWindowAsync);
				UpdateScheduleCmd = new AsyncRelayCommand(SetTimeSheetItemsAsync);
				ShowResultSheetCmd = new AsyncRelayCommand(ShowResultSheet);
				UpdateResultSheetCmd = new AsyncRelayCommand(InitResultSheetAsync);
				CreateReportResultSheetCmd = new AsyncRelayCommand(CreateReportResultSheetAsync);

				SetMonthAndYear();

				ItemMonthsTO = ListMonthsTO?[DateTime.Now.Month - 1] ?? new(1, string.Empty);
				ItemYearsTO = ListYearsTO.Where(x => !string.IsNullOrEmpty(x.Name) &&
							x.Name.Contains($"{DateTime.Now.Year}")).FirstOrDefault() ?? new(1, string.Empty);

				StartDate = new DateTime(day: 1, month: ItemMonthsTO.Id, year: ItemYearsTO.Id);
				EndDate = new DateTime(day: MaxDayTO, month: ItemMonthsTO.Id, year: ItemYearsTO.Id);

				UserDataCurrent = new LocalUserData
				{
					MachineName = LocalMachineName,
					UserName = string.Empty
				};
				await GetDepartmentProductionsAsync().ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
			}
		}

		public event EventHandler LoadTOChanged;

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
			set
			{
				SetProperty(ref _manualDateDismissal, value);
			}
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
				Task.Run(SetTextIsLunchAsync).GetAwaiter().GetResult();
			}
		}
		private DateTime _manualLastDateLunch;

		/// <summary>
		/// Установка актуальных сведений об обедах. Или предупреждений о не возможности установки обеда.
		/// </summary>
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
				var idEmployee = TimeSheetOne.WorkerHours.Select(x => x.EmployeeID).FirstOrDefault();

				var itemEmployee = await _timeSheetDb.GetEmployeeIdAndDateAsync(idEmployee, ManualLastDateLunch, UserDataCurrent).ConfigureAwait(false);

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
				await _errorLogger.ProcessingErrorLogAsync(ex,
					user: UserDataCurrent.UserName,
					machine: UserDataCurrent.MachineName)
					.ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Вывод промежуточных данных об обедах. И предупреждений о не возможности установки обеда.
		/// </summary>
		public string TextIsLunch
		{
			get => _textIsLunch;
			set => SetProperty(ref _textIsLunch, value);
		}
		private string _textIsLunch;

		/// <summary>
		/// Кастомный диалог для работы с обедами
		/// </summary>
		public CustomDialog CustomDialogsForLunch { get; private set; }

		/// <summary>
		/// Кастомный диалог для уволнений
		/// </summary>
		public CustomDialog CustomDialogsForDismissal { get; private set; }

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
		public Visibility Visibility { get; private set; }
		public ICommand RunCustomDialogForDismissalCmd { get; private set; }
		public ICommand RunCustomDialogForLunchCmd { get; private set; }
		public ICommand CloseLunchCmd { get; private set; }
		public ICommand ShowResultSheetCmd { get; private set; }
		public ICommand ShowFAQWindowCmd { get; private set; }
		public ICommand ShowStaffWindowCmd { get; private set; }

		#endregion

		#region Methods

		/// <summary>
		/// Асинхронный обработчик закрытия кастомного диалога
		/// </summary>
		/// <returns></returns>
		private async Task CloseAsync()
		{
			try
			{
				await _coordinator.HideMetroDialogAsync(this, CustomDialogsForDismissal);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex, user: UserDataCurrent.UserName, machine: UserDataCurrent.MachineName).ConfigureAwait(false);
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
				await _errorLogger.ProcessingErrorLogAsync(ex, user: UserDataCurrent.UserName, machine: UserDataCurrent.MachineName).ConfigureAwait(false);
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

				var check = await _timeSheetDb
					.UpdateDismissalDataEmployeeAsync(
					ManualDateDismissal, idEmployee, UserDataCurrent);

				//Закрываем окно
				await _coordinator.HideMetroDialogAsync(this, CustomDialogsForDismissal);

				//Обновляем табель после изменений
				if (check == true)
					await SetTimeSheetItemsAsync();
				else
					await _coordinator.ShowMessageAsync(this, "Ошибка",
						"Не найден сотрудник по его табельному номеру");
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex, user: UserDataCurrent.UserName,
					machine: UserDataCurrent.MachineName).ConfigureAwait(false);
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
				CustomDialogsForDismissal = new CustomDialog
				{
					//Привязываем кастомное окно к нашей View-модели
					Content = new DismissalEmployee(this)
				};

				//Настраиваем поведение анимации
				MetroDialogSettings settings = new() { AnimateShow = true, AnimateHide = true };

				//Показываем окно
				await _coordinator.ShowMetroDialogAsync(this, CustomDialogsForDismissal, settings);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex, user: UserDataCurrent.UserName, machine: UserDataCurrent.MachineName).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Метод по распределению логики, увольняем сотрудника или наоборот - отменяем увольнение
		/// </summary>
		private async Task HandlerCommandDismissOrRescindDismissalAsync()
		{
			try
			{
				if (TimeSheetOne is null) return;

				//Получаем табельный номер сотрудника
				var idEmployee = TimeSheetOne.WorkerHours.Select(x => x.EmployeeID).FirstOrDefault();
				if (idEmployee == 0) return;

				var check = await _timeSheetDb.CancelDismissalEmployeeAsync(
					idEmployee, DefaultDateDismissal, UserDataCurrent);

				if (check is null) return;

				if (check == true)
					//Обновляем табель после изменений
					await SetTimeSheetItemsAsync();
				else
					await RunCustomDialogAsyncForDismissal();
			}
			catch (Exception ex)
			{
				await _errorLogger
					.ProcessingErrorLogAsync(ex, user: UserDataCurrent.UserName,
					machine: UserDataCurrent.MachineName)
					.ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Обновление данных об обедах
		/// </summary>
		private async Task UpdLunchAsync()
		{
			try
			{
				//Проверки
				if (TimeSheetOne is null) return;
				if (ManualLastDateLunch > DateTime.Now.Date) return;

				//Получаем табельный номер сотрудника
				var idEmployee = TimeSheetOne.WorkerHours.Select(x => x.EmployeeID).FirstOrDefault();

				var check = await _timeSheetDb.UpdateLunchEmployeeAsync(
					idEmployee, ManualLastDateLunch, UserDataCurrent);

				if (check == true)
				{
					await SetTextIsLunchAsync().ConfigureAwait(false);
				}
				else if (check == false)
				{
					//Закрываем окно
					await _coordinator.HideMetroDialogAsync(this, CustomDialogsForLunch);

					await _coordinator.ShowMessageAsync(this, "Ошибка",
						"Данный сотрудник уволен, нельзя проставить ему обед");
				}
				else if (check is null)
				{
					//Закрываем окно
					await _coordinator.HideMetroDialogAsync(this, CustomDialogsForLunch);

					await _coordinator.ShowMessageAsync(this, "Ошибка",
						"Не найден сотрудник по его табельному номеру");
				}
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex, user: UserDataCurrent.UserName, machine: UserDataCurrent.MachineName).ConfigureAwait(false);
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
				await _errorLogger
					.ProcessingErrorLogAsync(ex, user: UserDataCurrent.UserName,
					machine: UserDataCurrent.MachineName)
					.ConfigureAwait(false);
			}
		}
		#endregion

		#endregion

		#region Search

		/// <summary>
		/// Применяем фильтр напрямую к коллекции. Так как DataGrid не работает с фильтрами ICollectionView как ListView
		/// </summary>
		/// <returns></returns>
		private void ApplyFilter()
		{
			try
			{
				var filteredList = DoubleTimeSheetsForSearch?
					.AsParallel()
					.Where(item => string.IsNullOrEmpty(FilterName) ||
						item.FioShiftOverday.ShortName.Contains(FilterName, StringComparison.OrdinalIgnoreCase))
					.ToList() ?? [];

				TimeSheets = new ObservableCollection<TimeSheetItem>(filteredList);
			}
			catch (Exception ex)
			{
				_errorLogger
					.ProcessingErrorLog(ex, user: UserDataCurrent.UserName,
					machine: UserDataCurrent.MachineName);
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
				try
				{
					SetProperty(ref _filterName, value);

					_filterCancellationTokenSource?.Cancel();
					_filterCancellationTokenSource = new CancellationTokenSource();

					//Задержка, перед применением фильтра, для плавного поиска при наборе текста в поиске
					Task.Delay(350, _filterCancellationTokenSource.Token)
						.ContinueWith(async t =>
						{
							if (!t.IsCanceled)
							{
								await Task.Run(() => ApplyFilter()).ConfigureAwait(false);
							}
						}, TaskContinuationOptions.RunContinuationsAsynchronously)
						.ContinueWith(async ty =>
						{
							await InitResultSheetAsync();
						});
				}
				catch (Exception ex)
				{
					_errorLogger.ProcessingErrorLog(ex, user: UserDataCurrent.UserName, machine: UserDataCurrent.MachineName);
				}
			}
		}
		private string _filterName;
		private CancellationTokenSource _filterCancellationTokenSource;
		#endregion

		#region Methods	

		/// <summary>
		/// Метод преобразования json файлов производственного календаря за 2024 и 2025 год с сайта https://xmlcalendar.ru/index.php?country=ru
		/// в список выходных дней для табеля.
		/// </summary>
		private async Task<bool> GetWeekendDaysAsync()
		{
			try
			{
				// Чтение содержимого JSON-файла
				string json = File.ReadAllText($"calendar{ItemYearsTO.Name}.json");

				// Десериализация JSON в объект
				var calendarData = JsonConvert.DeserializeObject<JObject>(json);

				// Получаем массив месяцев
				var months = (JArray)calendarData["months"];

				// Находим нужный месяц
				var selectedMonth = months.FirstOrDefault(x => x.Value<int>("month") == ItemMonthsTO.Id);

				if (selectedMonth != null)
				{
					// Получаем список выходных дней
					string daysMonthString = selectedMonth.Value<string>("days");
					var days = daysMonthString.Split(',', '*', '+');

					// Преобразуем строки в числа, списка выходных дней.
					NoWorkDaysTO = days
						.Where(x => int.TryParse(x, out _))
						.Select(x => int.Parse(x))
						.ToList();
				}
				return true;
			}
			catch (Exception ex)
			{
				await _errorLogger
					.ProcessingErrorLogAsync(ex, user: UserDataCurrent.UserName,
					machine: UserDataCurrent.MachineName)
					.ConfigureAwait(false);

				return false;
			}
		}

		/// <summary>
		/// Устанавливает начальные значения месяца и года для производственного временного листа.
		/// </summary>
		private void SetMonthAndYear()
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
			}
			catch (Exception ex)
			{
				_errorLogger
					.ProcessingErrorLog(ex, user: UserDataCurrent.UserName,
					machine: UserDataCurrent.MachineName);
			}
		}

		/// <summary>
		/// Обновляет свойство IsLoadedTO на основе выбранных месяца, года и отдела.
		/// </summary>
		private void UpdateIsLoadedTO()
		{
			if (ItemMonthsTO != null && ItemYearsTO != null && NamesDepartmentItem != null && MaxDayTO > 0)
				LoadTOChanged?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Конфигурируем список участков 
		/// </summary>
		private async Task GetDepartmentProductionsAsync()
		{
			try
			{
				NamesDepartment = await _timeSheetDb.GetAllDepartmentsAsync(UserDataCurrent);
			}
			catch (Exception ex)
			{
				await _errorLogger
					.ProcessingErrorLogAsync(ex, user: UserDataCurrent.UserName,
					machine: UserDataCurrent.MachineName)
					.ConfigureAwait(false);
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

				var checking = await GetWeekendDaysAsync();

				var tempShifts = await _timeSheetDb.SetDataForTimeSheetAsync(
					NamesDepartmentItem, StartDate, EndDate,
					ItemMonthsTO, ItemYearsTO,
					NoWorkDaysTO, UserDataCurrent)
					.ConfigureAwait(false);


				if (!string.IsNullOrEmpty(FilterName))
				{
					DoubleTimeSheetsForSearch = new ObservableCollection<TimeSheetItem>(tempShifts);
					await Task.Run(() => ApplyFilter());
				}
				else
				{
					//Готовые данные табеля отдаём ресурсу для отрисовки табеля в приложении
					TimeSheets = tempShifts;
					DoubleTimeSheetsForSearch = new ObservableCollection<TimeSheetItem>(tempShifts);
				}

				if (ResultsSheet != null)
					await InitResultSheetAsync();
			}
			catch (Exception ex)
			{
				await _errorLogger
					.ProcessingErrorLogAsync(ex, user: UserDataCurrent.UserName,
					machine: UserDataCurrent.MachineName).ConfigureAwait(false);
			}
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Событие возникает, когда данные готовы к расчёту. И запускается ассинхронный метод расчёта
		/// </summary>
		private async void MainViewModel_LoadTOChanged(object? sender, EventArgs e)
		{
			try
			{
				await SetTimeSheetItemsAsync().ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await _errorLogger
						.ProcessingErrorLogAsync(ex, user: UserDataCurrent.UserName ?? string.Empty,
						machine: UserDataCurrent.MachineName ?? string.Empty).ConfigureAwait(false);
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
				if (e.PropertyName == nameof(ShiftData.Hours) || e.PropertyName == nameof(ShiftData.Overday) || e.PropertyName == nameof(ShiftData.Shift))
					await _timeSheetDb.SetTotalWorksDaysAsync(sender, UserDataCurrent).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex, user: UserDataCurrent.UserName, machine: UserDataCurrent.MachineName);
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
					foreach (ShiftData item in e.NewItems)
						item.PropertyChanged += Item_PropertyChanged;
				}
				if (e.OldItems != null)
				{
					foreach (ShiftData item in e.OldItems)
					{
						item.PropertyChanged -= Item_PropertyChanged;
					}
				}
			}
			catch (Exception ex)
			{
				_errorLogger
				   .ProcessingErrorLog(ex, user: UserDataCurrent.UserName,
				   machine: UserDataCurrent.MachineName);
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// Дублированые данные, для хранения полноценного табеля при использовании поиска в табеле
		/// </summary>
		public ObservableCollection<TimeSheetItem> DoubleTimeSheetsForSearch { get; private set; }

		/// <summary>
		/// Данные с именами сотрудника и его компьютера
		/// </summary>
		public LocalUserData UserDataCurrent { get; private set; }

		/// <summary>
		/// Список участков предприятия
		/// </summary>
		public IEnumerable<DepartmentProduction> NamesDepartment
		{
			get => _namesDepartment;
			set
			{
				SetProperty(ref _namesDepartment, value);

				if (NamesDepartment != null || NamesDepartment?.Count() > 0)
					NamesDepartmentItem = NamesDepartment.FirstOrDefault();
				else
					NamesDepartmentItem = new DepartmentProduction { DepartmentID = "0000", NameDepartment = @"<Нет доступа>" };
			}
		}
		private IEnumerable<DepartmentProduction> _namesDepartment;

		/// <summary>
		/// Выбранный участок предприятия
		/// </summary>
		public DepartmentProduction NamesDepartmentItem
		{
			get => _namesDepartmentItem;
			set
			{
				SetProperty(ref _namesDepartmentItem, value);
				UpdateIsLoadedTO();
			}
		}
		private DepartmentProduction _namesDepartmentItem;

		/// <summary>
		/// Список не рабочих дней в месяце для Табеля ТО
		/// </summary>
		public List<int> NoWorkDaysTO { get => _noWorkDaysTimeSheet; set => SetProperty(ref _noWorkDaysTimeSheet, value); }
		private List<int> _noWorkDaysTimeSheet;

		/// <summary>Стартовая дата</summary>
		public DateTime StartDate { get; private set; }

		/// <summary>Финишная дата</summary>
		public DateTime EndDate { get; private set; }

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

		public ICommand FormulateReportForLunchLastMonhtCmd { get; set; }
		public ICommand UpdateScheduleCmd { get; set; }
		public ICommand IsLunchCmd { get; set; }

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

				var check = await _timeSheetDb.UpdateIsLunchingDbAsync(
					idEmployee, UserDataCurrent).ConfigureAwait(false);

				if (check == true)
					//Обновляем табель после изменений
					await SetTimeSheetItemsAsync().ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await _errorLogger
					.ProcessingErrorLogAsync(ex, user: UserDataCurrent.UserName,
					machine: UserDataCurrent.MachineName)
					.ConfigureAwait(false);
			}
		}
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
					var path = await _api.GetReportResultSheetsAsync(
						[.. EmpIndicators], UserDataCurrent);

					if (string.IsNullOrEmpty(path)) return;

					File.Copy(path, dialog.FileName, true);

					await _coordinator.ShowMessageAsync(this, $"Создание отчёта для {ItemIndicator.DescriptionIndicator}", "Готово");
				}
			}
			catch (Exception ex)
			{
				await _errorLogger
					.ProcessingErrorLogAsync(ex, user: UserDataCurrent.UserName,
					machine: UserDataCurrent.MachineName)
					.ConfigureAwait(false);

				await _coordinator.ShowMessageAsync(this, $"Создание отчёта для Итогов Табеля", "Не удача!");
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

				var copyTimeSheet = new ObservableCollection<TimeSheetItem>(TimeSheets.Select(x => x.Clone()));

				Tuplet = await _sheetsService.ShowResultSheet(copyTimeSheet, UserDataCurrent);

				Indicators = Tuplet.Indicators;

				TextEmpIndicator = string.Empty;
				TextIndicator = @$"Участок:  {NamesDepartmentItem.NameDepartment}.";
				TextCountEmployee = $"Кол-во работников:  {copyTimeSheet.Count}.";
			}
			catch (Exception ex)
			{
				await _errorLogger
					.ProcessingErrorLogAsync(ex, user: UserDataCurrent.UserName,
					machine: UserDataCurrent.MachineName)
					.ConfigureAwait(false);
			}
		}

		private bool ValidationForTimeSheetsAndUserDataCurrent()
		{
			return TimeSheets is null || TimeSheets.Count == 0 || UserDataCurrent is null;
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
				await _errorLogger
					.ProcessingErrorLogAsync(ex, user: UserDataCurrent.UserName,
					machine: UserDataCurrent.MachineName)
					.ConfigureAwait(false);
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
		public Indicator ItemIndicator
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
						2 => new ObservableCollection<EmployeesInIndicator>(Tuplet.NNList),
						3 => new ObservableCollection<EmployeesInIndicator>(Tuplet.Underday),
						4 => new ObservableCollection<EmployeesInIndicator>(Tuplet.Overday),
						5 => new ObservableCollection<EmployeesInIndicator>(Tuplet.Night),
						6 => new ObservableCollection<EmployeesInIndicator>(Tuplet.Vacation),
						7 => new ObservableCollection<EmployeesInIndicator>(Tuplet.ADVacation),
						8 => new ObservableCollection<EmployeesInIndicator>(Tuplet.SickLeave),
						9 => new ObservableCollection<EmployeesInIndicator>(Tuplet.Demobilized),
						10 => new ObservableCollection<EmployeesInIndicator>(Tuplet.ParentalLeave),
						11 => new ObservableCollection<EmployeesInIndicator>(Tuplet.InvalidLeave),
						12 => new ObservableCollection<EmployeesInIndicator>(Tuplet.Dismissal),
						13 => new ObservableCollection<EmployeesInIndicator>(Tuplet.Lunching),
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
		private Indicator _itemIndicator;

		/// <summary>
		/// Список сотрудников с данных показателей в итогах табеля
		/// </summary>
		public ObservableCollection<EmployeesInIndicator> EmpIndicators
		{
			get => _empIndicators;
			set => SetProperty(ref _empIndicators, value);
		}
		private ObservableCollection<EmployeesInIndicator> _empIndicators;

		/// <summary>
		/// Список показателей в итогах табеля
		/// </summary>
		public ObservableCollection<Indicator> Indicators
		{
			get => _indicators;
			set => SetProperty(ref _indicators, value);
		}
		private ObservableCollection<Indicator> _indicators;

		/// <summary>
		/// Кортеж со списками данных для итогов табеля
		/// </summary>
		public (
			ObservableCollection<Indicator> Indicators,
			List<EmployeesInIndicator> NNList,
			List<EmployeesInIndicator> Underday,
			List<EmployeesInIndicator> Overday,
			List<EmployeesInIndicator> Night,
			List<EmployeesInIndicator> Vacation,
			List<EmployeesInIndicator> ADVacation,
			List<EmployeesInIndicator> SickLeave,
			List<EmployeesInIndicator> Demobilized,
			List<EmployeesInIndicator> ParentalLeave,
			List<EmployeesInIndicator> InvalidLeave,
			List<EmployeesInIndicator> Dismissal,
			List<EmployeesInIndicator> Lunching)
			Tuplet
		{ get; private set; }

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

		/// <summary>
		/// Окно со справкой
		/// </summary>
		public FAQ? FAQ
		{
			get => _faq;
			set => SetProperty(ref _faq, value);
		}
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
				await _errorLogger.ProcessingErrorLogAsync(ex,
					user: UserDataCurrent.UserName,
					machine: UserDataCurrent.MachineName)
					.ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Вызов окна картотеки СО
		/// </summary>
		private async Task ShowStaffWindowAsync()
		{
			try
			{
				if (StaffView == null)
				{
					StaffView = new();
					StaffView.Closed += (sender, args) => StaffView = null;
					await StaffViewModel.Initializing(StaffView);
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
				await _errorLogger.ProcessingErrorLogAsync(ex,
					user: UserDataCurrent.UserName,
					machine: UserDataCurrent.MachineName)
					.ConfigureAwait(false);
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
	}

}
