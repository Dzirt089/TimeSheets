using AutoMapper;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MahApps.Metro.Controls.Dialogs;

using Microsoft.Win32;

using ProductionControl.DataAccess.Classes.EFClasses.EmployeesExternalOrganizations;
using ProductionControl.DataAccess.Classes.HttpModels;
using ProductionControl.Services.ErrorLogsInformation;
using ProductionControl.UIModels.Dtos.ExternalOrganization;
using ProductionControl.UIModels.Model.ExternalOrganization;
using ProductionControl.UIModels.Model.GlobalPropertys;
using ProductionControl.Views;

using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ProductionControl.ViewModel
{
	/// <summary>
	/// ViewModel для работы с внешними организациями сотрудников.
	/// </summary>
	/// <remarks>
	/// Конструктор StaffExternalOrgViewModel.
	/// </remarks>
	/// <param name="dialogCoordinator">Сервис для управления диалогами.</param>
	/// <param name="timeSheetDb">Сервис для работы с базой данных табелей.</param>
	/// <param name="errorLogger">Сервис для логирования ошибок.</param>
	public class StaffExternalOrgViewModel(
		IMapper mapper,
		ITimeSheetDbService timeSheetDb,
		IErrorLogger errorLogger,
		IDialogCoordinator coordinator,
		GlobalEmployeeSessionInfo userData) : ObservableObject
	{
		private readonly IDialogCoordinator _coordinator = coordinator;
		private readonly ITimeSheetDbService _timeSheetDb = timeSheetDb;
		private readonly IErrorLogger _errorLogger = errorLogger;
		private readonly IMapper _mapper = mapper;
		private StaffExternalOrgView? ExternalOrgView { get; set; }

		/// <summary>
		/// Инициализирует данные для представления.
		/// </summary>
		/// <param name="externalOrgView">Представление для инициализации.</param>
		public async Task Initializing(StaffExternalOrgView externalOrgView)
		{
			try
			{
				AllCategoryes = [
				new() { Categoryes = 1},
				new() { Categoryes = 2},
				new() { Categoryes = 3}
				];

				ExternalOrgView = externalOrgView;
				ShowDismissalEmployeeExOrg = false;
				VisibilityAddMainRegion = Visibility.Visible;

				ValueDepartmentID = userData.UserName.GetDepartmentAsync();

				HidenElemets();

				if (MainAccess())
				{
					VisibilityButtons = Visibility.Visible;

					LoadPhotoCmd = new AsyncRelayCommand(LoadPhotoAsync);
					CreateNewEmployeeExOrgCmd = new AsyncRelayCommand(CreateNewEmployeeExOrgAsync);
					EditEmployeeExOrgCmd = new AsyncRelayCommand(EditEmployeeExOrgAsync);
					DismissalEmployeeExOrgCmd = new AsyncRelayCommand(DismissalEmployeeExOrgAsync);
					RefreshCmd = new AsyncRelayCommand(RefreshAsync);

					UpdCmd = new AsyncRelayCommand(UpdAsync);
					CloseCmd = new AsyncRelayCommand(CloseAsync);
				}
				else
					VisibilityButtons = Visibility.Collapsed;

				CloseExOrgCmd = new RelayCommand(Close);
				SaveDataForEmployeeExOrgCmd = new AsyncRelayCommand(SaveEmployeeExOrgAsync);

				StartDate = new DateTime(day: 1, month: DateTime.Now.Month, year: DateTime.Now.Year);
				EndDate = new DateTime(day: DateTime.DaysInMonth(month: DateTime.Now.Month, year: DateTime.Now.Year), month: DateTime.Now.Month, year: DateTime.Now.Year);

				await RefreshAsync();
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
				throw;
			}
		}

		#region Methods

		/// <summary>
		/// Проверяет, имеет ли текущий пользователь доступ к основным функциям.
		/// </summary>
		/// <returns>True, если доступ есть, иначе false.</returns>
		private bool MainAccess()
		{
			return userData.UserName.Equals("okad01", StringComparison.OrdinalIgnoreCase) ||
					userData.UserName.Equals("teho19", StringComparison.OrdinalIgnoreCase) ||
					userData.UserName.Equals("teho12", StringComparison.OrdinalIgnoreCase) ||
					userData.UserName.Equals("ceh07", StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Сохраняет данные сотрудника внешней организации.
		/// </summary>
		private async Task SaveEmployeeExOrgAsync()
		{
			try
			{
				if (NewEmployeeForCartotecaExOrg is null) return;

				int idEmpLast = NewEmployeeForCartotecaExOrg.EmployeeExOrgID;

				if (CheckingBoolUpdate())
				{
					DataForUpdateEmloyeeExOrg dataForUpdateEmloyeeExOrg = new DataForUpdateEmloyeeExOrg
					{
						ExOrg = _mapper.Map<EmployeeExOrg>(NewEmployeeForCartotecaExOrg),
						ValueDepId = ValueDepartmentID,
						AddWorkInReg = AddWorkingInReg
					};

					await _timeSheetDb.UpdateEmployeeExOrgAsync(dataForUpdateEmloyeeExOrg);
				}
				else
				{
					var newEmployee = _mapper.Map<EmployeeExOrg>(NewEmployeeForCartotecaExOrg);
					await _timeSheetDb.AddEmployeeExOrgAsync(newEmployee);
				}

				VisibilityAddMainRegion = Visibility.Visible;

				await RefreshAsync();

				SelectedEmployeeForCartotecaExOrg = EmployeesForCartotecaExOrg.FirstOrDefault(x => x.EmployeeExOrgID == idEmpLast);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
				throw;
			}
		}

		/// <summary>
		/// Обновляет список с данными сотрудников.
		/// </summary>
		private async Task RefreshAsync()
		{
			try
			{
				EmployeesForCartotecaExOrgList = await _timeSheetDb.GetEmployeeExOrgsAllAsync()
					.ConfigureAwait(false);

				InfoWorksEmployees = await _timeSheetDb.GetEmployeeExOrgsOnDateAsync(
					StartDate, EndDate, ValueDepartmentID)
					.ConfigureAwait(false);

				EmployeesForCartotecaExOrg = new ObservableCollection<EmployeeExOrgDto>(EmployeesForCartotecaExOrgList);
				DubleEmployeesForCartotecaExOrg = new List<EmployeeExOrgDto>(EmployeesForCartotecaExOrgList);

				SetCollectionsEmployeesExOrg(DubleEmployeesForCartotecaExOrg);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
				throw;
			}
		}
		/// <summary>
		/// Увольняет сотрудника внешней организации.
		/// </summary>
		private async Task DismissalEmployeeExOrgAsync()
		{
			try
			{
				if (NewEmployeeForCartotecaExOrg is null) return;

				if (NewEmployeeForCartotecaExOrg.IsDismissal == true)
				{
					NewEmployeeForCartotecaExOrg.DateDismissal = DateTime.Parse("31.12.1876");
					NewEmployeeForCartotecaExOrg.IsDismissal = false;
				}
				else
				{
					await RunCustomDialogAsyncForDismissal();
				}
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
				throw;
			}
		}
		public CustomDialog CustomDialogs { get; private set; }

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
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
				throw;
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
				if (NewEmployeeForCartotecaExOrg is null) return;
				if (ManualDateDismissal == DefaultDateDismissal) return;

				IdEmployeeDateTime idEmployeeDateTime =
					new IdEmployeeDateTime { Date = ManualDateDismissal, IdEmployee = NewEmployeeForCartotecaExOrg.EmployeeExOrgID };

				var check = await _timeSheetDb.UpdateDismissalDataEmployeeAsync(idEmployeeDateTime);

				//Закрываем окно
				await _coordinator.HideMetroDialogAsync(this, CustomDialogs);

				//Обновляем табель после изменений
				if (check != true)
					await _coordinator.ShowMessageAsync(this, "Ошибка",
						"Не найден сотрудник по его табельному номеру");
				else await RefreshAsync();
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
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
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
				throw;
			}
		}

		/// <summary>
		/// Редактирует данные сотрудника внешней организации.
		/// </summary>
		private async Task EditEmployeeExOrgAsync()
		{
			try
			{
				if (SelectedEmployeeForCartotecaExOrg is null) return;
				ShowAndHidenElemets();
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
				throw;
			}
		}

		/// <summary>
		/// Показать и скрыть элементы управления.
		/// </summary>
		private void ShowAndHidenElemets()
		{
			IsEnabledDateDismissal = false;
			IsEnabledTextBox = true;
			VisibilityButtonLoad = Visibility.Visible;
		}

		/// <summary>
		/// Закрывает представление внешних организаций сотрудников.
		/// </summary>
		private void Close() => ExternalOrgView!.Close();

		/// <summary>
		/// Загружает фото сотрудника внешней организации.
		/// </summary>
		private async Task LoadPhotoAsync()
		{
			try
			{
				if (NewEmployeeForCartotecaExOrg is null) return;

				OpenFileDialog openFileDialog = new()
				{
					Filter = "Image files(*.png;*.jpg)|*.png;*.jpg|All files (*.*)|*.*"
				};

				if (openFileDialog.ShowDialog() == true)
				{
					string filePath = openFileDialog.FileName;
					Photo = await File.ReadAllBytesAsync(filePath);

					if (NewEmployeeForCartotecaExOrg != null)
					{
						var empPh = new EmployeePhotoDto();
						empPh.Photo = Photo;
						NewEmployeeForCartotecaExOrg.EmployeePhotos = empPh;
					}
				}
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
				throw;
			}
		}

		/// <summary>
		/// Асинхронно подготавливает данные для нового сотрудника внешней организации.
		/// </summary>
		private async Task CreateNewEmployeeExOrgAsync()
		{
			try
			{
				NewEmployeeForCartotecaExOrg = new()
				{
					DateDismissal = DateTime.Parse("31.12.1876"),
					DateEmployment = DateTime.Now.Date
				};
				VisibilityAddMainRegion = Visibility.Hidden;
				ShowAndHidenElemets();
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex).ConfigureAwait(false);
				throw;
			}
		}

		/// <summary>
		/// Устанавливает коллекцию сотрудников внешних организаций в зависимости от их статуса увольнения.
		/// </summary>
		/// <param name="dubleEmployeesForCartotecaExOrg">Список сотрудников внешних организаций.</param>
		private void SetCollectionsEmployeesExOrg(List<EmployeeExOrgDto> dubleEmployeesForCartotecaExOrg)
		{
			if (EmployeesForCartotecaExOrg is null || dubleEmployeesForCartotecaExOrg is null) return;

			List<EmployeeExOrgDto> employeeExes = [];
			if (ShowDismissalEmployeeExOrg)
				employeeExes = dubleEmployeesForCartotecaExOrg.Where(x => x.IsDismissal).ToList();

			else if (!ShowDismissalEmployeeExOrg)
				employeeExes = dubleEmployeesForCartotecaExOrg.Where(x => x.IsDismissal == false).ToList();

			EmployeesForCartotecaExOrg = new ObservableCollection<EmployeeExOrgDto>(employeeExes);
		}

		/// <summary>
		/// Скрывает элементы управления.
		/// </summary>
		private void HidenElemets()
		{
			VisibilityButtonLoad = Visibility.Hidden;
			IsEnabledTextBox = false;
			IsEnabledDateDismissal = false;
		}

		#endregion

		#region Property

		/// <summary>
		/// Получает или задает коллекцию сотрудников внешних организаций для картотеки.
		/// </summary>
		public ObservableCollection<EmployeeExOrgDto>? EmployeesForCartotecaExOrg
		{
			get => _employeesForCartotecaExOrg;
			set
			{
				SetProperty(ref _employeesForCartotecaExOrg, value);
				if (EmployeesForCartotecaExOrg != null && EmployeesForCartotecaExOrg.Count > 0)
				{
					SelectedEmployeeForCartotecaExOrg = EmployeesForCartotecaExOrg.FirstOrDefault();
				}
				else
				{
					SelectedEmployeeForCartotecaExOrg = null;
					NewEmployeeForCartotecaExOrg = null;
				}
			}
		}
		private ObservableCollection<EmployeeExOrgDto>? _employeesForCartotecaExOrg;


		public ObservableCollection<CategoryExOrg>? AllCategoryes
		{
			get => _categoryes;
			set => SetProperty(ref _categoryes, value);
		}
		private ObservableCollection<CategoryExOrg>? _categoryes;


		public CategoryExOrg ItemCategory
		{
			get => _itemCategory;
			set
			{
				SetProperty(ref _itemCategory, value);
				if (NewEmployeeForCartotecaExOrg != null && ItemCategory != null)
					NewEmployeeForCartotecaExOrg.NumCategory = ItemCategory.Categoryes;
			}
		}
		private CategoryExOrg _itemCategory;


		/// <summary>
		/// Получает или задает коллекцию сотрудников внешних организаций (дублирующая).
		/// </summary>
		public List<EmployeeExOrgDto>? DubleEmployeesForCartotecaExOrg
		{
			get => _dubleEmployeesForCartotecaExOrg;
			set => SetProperty(ref _dubleEmployeesForCartotecaExOrg, value);
		}
		private List<EmployeeExOrgDto>? _dubleEmployeesForCartotecaExOrg;

		/// <summary>
		/// Задает нового сотрудника внешней организации для картотеки.
		/// </summary>
		public EmployeeExOrgDto? NewEmployeeForCartotecaExOrg
		{
			get => _newEmployeeForCartotecaExOrg;
			set
			{
				SetProperty(ref _newEmployeeForCartotecaExOrg, value);
				if (CheckingBool())
				{
					var temp = InfoWorksEmployees
						.Where(x => x.EmployeeExOrgID == NewEmployeeForCartotecaExOrg.EmployeeExOrgID)
						.Any(x => x.ShiftDataExOrgs
							.Any(z => z.Hours.TryParseDouble(out _)));

					EnabledAddWorkingInReg = ShowDismissalEmployeeExOrg == true ? false : !temp;

					AddWorkingInReg = NewEmployeeForCartotecaExOrg.EmployeeExOrgAddInRegions
						.Where(x => x.DepartmentID == ValueDepartmentID)
						.Select(x => x.WorkingInTimeSheetEmployeeExOrg)
						.FirstOrDefault();
				}
			}
		}
		private EmployeeExOrgDto? _newEmployeeForCartotecaExOrg;

		/// <summary>
		/// Получает  выбранного сотрудника внешней организации для картотеки.
		/// </summary>
		public EmployeeExOrgDto? SelectedEmployeeForCartotecaExOrg
		{
			get => _itemEmployeeForCartotecaExOrg;
			set
			{
				SetProperty(ref _itemEmployeeForCartotecaExOrg, value);

				if (SelectedEmployeeForCartotecaExOrg != null)
				{
					NewEmployeeForCartotecaExOrg = SelectedEmployeeForCartotecaExOrg;
					ItemCategory = AllCategoryes.Where(x => x.Categoryes == NewEmployeeForCartotecaExOrg.NumCategory).FirstOrDefault();

					HidenElemets();
					VisibilityAddMainRegion = Visibility.Visible;
				}
				else HidenElemets();
			}
		}

		private EmployeeExOrgDto? _itemEmployeeForCartotecaExOrg;

		/// <summary>
		/// Получает или задает флаг доступности текстового поля.
		/// </summary>
		public bool IsEnabledTextBox
		{
			get => _isEnabledTextBox;
			set => SetProperty(ref _isEnabledTextBox, value);
		}
		private bool _isEnabledTextBox;

		/// <summary>
		/// Получает или задает флаг доступности даты увольнения.
		/// </summary>
		public bool IsEnabledDateDismissal
		{
			get => _isEnabledDateDismissal;
			set => SetProperty(ref _isEnabledDateDismissal, value);
		}
		private bool _isEnabledDateDismissal;

		/// <summary>
		/// Получает или задает флаг видимости для увольнения сотрудника внешней организации.
		/// </summary>
		public bool ShowDismissalEmployeeExOrg
		{
			get => _showDismissalEmployeeExOrg;
			set
			{
				SetProperty(ref _showDismissalEmployeeExOrg, value);
				SetCollectionsEmployeesExOrg(DubleEmployeesForCartotecaExOrg!);
			}
		}
		private bool _showDismissalEmployeeExOrg;

		public bool EnabledAddWorkingInReg
		{
			get => _enabledAddWorkingInReg;
			set
			{
				if (NewEmployeeForCartotecaExOrg is null) return;
				SetProperty(ref _enabledAddWorkingInReg, value);
			}
		}
		private bool _enabledAddWorkingInReg;

		public bool AddWorkingInReg
		{
			get => _addWorkingInReg;
			set
			{
				if (NewEmployeeForCartotecaExOrg is null) return;
				SetProperty(ref _addWorkingInReg, value);
			}
		}
		private bool _addWorkingInReg;

		private bool CheckingBool()
		{
			return NewEmployeeForCartotecaExOrg != null && !string.IsNullOrEmpty(ValueDepartmentID)
								&& NewEmployeeForCartotecaExOrg.EmployeeExOrgID > 0;
		}

		private bool CheckingBoolUpdate()
		{
			return NewEmployeeForCartotecaExOrg != null && (!string.IsNullOrEmpty(ValueDepartmentID) || string.IsNullOrEmpty(ValueDepartmentID) && MainAccess()) && NewEmployeeForCartotecaExOrg.EmployeeExOrgID > 0;
		}

		/// <summary>
		/// Получает или задает видимость основной области добавления на свой участок сотрудника.
		/// </summary>
		public Visibility VisibilityAddMainRegion
		{
			get => _visibilityAddMainRegion;
			set => SetProperty(ref _visibilityAddMainRegion, value);
		}
		private Visibility _visibilityAddMainRegion;

		/// <summary>
		/// Получает или задает видимость кнопки загрузки.
		/// </summary>
		public Visibility VisibilityButtonLoad
		{
			get => _visibilityButtonLoad;
			set => SetProperty(ref _visibilityButtonLoad, value);
		}
		private Visibility _visibilityButtonLoad;

		/// <summary>
		/// Получает или задает видимость кнопок.
		/// </summary>
		public Visibility VisibilityButtons
		{
			get => _visibilityButtons;
			set => SetProperty(ref _visibilityButtons, value);
		}
		private Visibility _visibilityButtons;

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
		/// По-умолчанию дата в ИС-ПРО ставится "31.12.1876". 
		/// </summary>
		public DateTime DefaultDateDismissal { get; private set; } = DateTime.Parse("31.12.1876");

		/// <summary>
		/// Получает или задает фото сотрудника внешней организации.
		/// </summary>
		public byte[]? Photo { get; private set; }

		/// <summary>
		/// Получает или задает текущий месяц.
		/// </summary>
		public int CurrentMonth { get; private set; }
		/// <summary>
		/// Получает или задает текущий год.
		/// </summary>
		public int CurrentYear { get; private set; }
		/// <summary>
		/// Получает или задает количество дней в текущем месяце.
		/// </summary>
		public int CurrentDays { get; private set; }
		/// <summary>
		/// Получает или задает начальную дату.
		/// </summary>
		public DateTime StartDate { get; private set; }
		/// <summary>
		/// Получает или задает конечную дату.
		/// </summary>
		public DateTime EndDate { get; private set; }
		public List<EmployeeExOrgDto>? EmployeesForCartotecaExOrgList { get; private set; }
		public List<EmployeeExOrgDto> InfoWorksEmployees { get; private set; }

		/// <summary>
		/// Получает или задает ID департамента.
		/// </summary>
		public string? ValueDepartmentID { get; private set; }

		#endregion

		#region Commands
		public ICommand? RefreshCmd { get; set; }
		public ICommand? CloseExOrgCmd { get; set; }
		public ICommand? SaveDataForEmployeeExOrgCmd { get; set; }
		public ICommand? DismissalEmployeeExOrgCmd { get; set; }
		public ICommand? EditEmployeeExOrgCmd { get; set; }
		public ICommand? CreateNewEmployeeExOrgCmd { get; set; }
		public ICommand? LoadPhotoCmd { get; set; }
		public ICommand? CloseCmd { get; private set; }
		public ICommand? UpdCmd { get; private set; }

		#endregion
	}
}
