using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MahApps.Metro.Controls.Dialogs;

using Microsoft.Win32;

using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

using TimeSheets.Entitys;
using TimeSheets.Models;
using TimeSheets.Services.Interfaces;
using TimeSheets.Views;

namespace TimeSheets.ViewModel
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
	public class StaffViewModel(
		ITimeSheetDbService timeSheetDb,
		IErrorLogger errorLogger,
		IDialogCoordinator coordinator) : ObservableObject
	{
		private readonly IDialogCoordinator _coordinator = coordinator;
		private readonly ITimeSheetDbService _timeSheetDb = timeSheetDb;
		private readonly IErrorLogger _errorLogger = errorLogger;
		private StaffView? ExternalOrgView { get; set; }

		/// <summary>
		/// Инициализирует данные для представления.
		/// </summary>
		/// <param name="externalOrgView">Представление для инициализации.</param>
		public async Task Initializing(StaffView externalOrgView)
		{
			try
			{
				ExternalOrgView = externalOrgView;
				ShowDismissalEmployee = false;
				CreateNewEmployeeFlag = false;

				UserDataCurrent = await _timeSheetDb.GetLocalUserAsync() ??
						new() { MachineName = string.Empty, UserName = string.Empty };
				DepartmentProductions = await _timeSheetDb.GetAllDepartmentsAsync(UserDataCurrent);

				HidenElemets();
				VisibilityButtons = Visibility.Visible;
				LoadPhotoCmd = new AsyncRelayCommand(LoadPhotoAsync);
				CreateNewEmployeeCmd = new AsyncRelayCommand(CreateNewEmployeeAsync);
				EditEmployeeCmd = new AsyncRelayCommand(EditEmployeeAsync);
				DismissalEmployeeCmd = new AsyncRelayCommand(DismissalEmployeeAsync);
				RefreshCmd = new AsyncRelayCommand(RefreshAsync);
				CloseWindowCmd = new RelayCommand(Close);
				SaveDataForEmployeeCmd = new AsyncRelayCommand(SaveEmployeeAsync);
				UpdCmd = new AsyncRelayCommand(UpdAsync);
				CloseCmd = new AsyncRelayCommand(CloseAsync);
				await RefreshAsync();
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex, user: UserDataCurrent.UserName,
					machine: UserDataCurrent.MachineName).ConfigureAwait(false);
			}
		}

		#region Methods


		/// <summary>
		/// Сохраняет данные сотрудника внешней организации.
		/// </summary>
		private async Task SaveEmployeeAsync()
		{
			try
			{
				if (NewEmployeeForCartoteca is null) return;

				var idEmpLast = NewEmployeeForCartoteca.EmployeeID;

				if (CreateNewEmployeeFlag)
				{
					if (await _timeSheetDb.CheckingDoubleEmployeeAsync(idEmpLast, UserDataCurrent))
					{
						MessageBox.Show(
@"При добавлении нового сотрудника выяснилось: 
что введеный табельный номер уже принадлежит другому сотруднику. 

Замените табельный номер и повторите попытку");
						return;
					}
				}

				if (NewEmployeeForCartoteca.DateDismissal != DateTime.Parse("31.12.1876") && NewEmployeeForCartoteca.IsDismissal == false)
					NewEmployeeForCartoteca.IsDismissal = true;

				if (NewEmployeeForCartoteca != null)
					await _timeSheetDb.UpdateEmployeeAsync(NewEmployeeForCartoteca, UserDataCurrent);
				else
					await _timeSheetDb.AddEmployeeAsync(NewEmployeeForCartoteca, UserDataCurrent);

				CreateNewEmployeeFlag = false;

				await RefreshAsync();

				SelectedEmployeeForCartoteca = EmployeesForCartoteca.FirstOrDefault(x => x.EmployeeID == idEmpLast);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex, user: UserDataCurrent.UserName,
					machine: UserDataCurrent.MachineName).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Обновляет список с данными сотрудников.
		/// </summary>
		private async Task RefreshAsync()
		{
			try
			{
				EmployeesForCartotecas = await _timeSheetDb.GetEmployeeForCartotecasAsync(UserDataCurrent);

				EmployeesForCartoteca = new ObservableCollection<Employee>(EmployeesForCartotecas);
				DubleEmployeesForCartoteca = new List<Employee>(EmployeesForCartotecas);

				SetCollectionsEmployees(DubleEmployeesForCartoteca);
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex, user: UserDataCurrent.UserName,
					machine: UserDataCurrent.MachineName).ConfigureAwait(false);
			}
		}
		/// <summary>
		/// Увольняет сотрудника внешней организации.
		/// </summary>
		private async Task DismissalEmployeeAsync()
		{
			try
			{
				if (NewEmployeeForCartoteca is null)
					return;

				if (NewEmployeeForCartoteca.IsDismissal == true)
				{
					NewEmployeeForCartoteca.DateDismissal = DateTime.Parse("31.12.1876");
					NewEmployeeForCartoteca.IsDismissal = false;
				}
				else
				{
					await RunCustomDialogAsyncForDismissal();
				}
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex, user: UserDataCurrent.UserName,
					machine: UserDataCurrent.MachineName).ConfigureAwait(false);
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
				if (NewEmployeeForCartoteca is null) return;
				if (ManualDateDismissal == DefaultDateDismissal) return;


				var check = await _timeSheetDb
					.UpdateDismissalDataEmployeeAsync(
					ManualDateDismissal, NewEmployeeForCartoteca.EmployeeID, UserDataCurrent);

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
				await _errorLogger.ProcessingErrorLogAsync(ex, user: UserDataCurrent.UserName,
					machine: UserDataCurrent.MachineName).ConfigureAwait(false);
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
				await _errorLogger.ProcessingErrorLogAsync(ex, user: UserDataCurrent.UserName, machine: UserDataCurrent.MachineName).ConfigureAwait(false);
			}
		}
		/// <summary>
		/// Редактирует данные сотрудника внешней организации.
		/// </summary>
		private async Task EditEmployeeAsync()
		{
			try
			{
				if (SelectedEmployeeForCartoteca is null) return;
				ShowAndHidenElemets();
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex, user: UserDataCurrent.UserName,
					machine: UserDataCurrent.MachineName).ConfigureAwait(false);
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
				if (NewEmployeeForCartoteca is null) return;

				OpenFileDialog openFileDialog = new()
				{
					Filter = "Image files(*.png;*.jpg)|*.png;*.jpg|All files (*.*)|*.*"
				};

				if (openFileDialog.ShowDialog() == true)
				{
					string filePath = openFileDialog.FileName;
					Photo = await File.ReadAllBytesAsync(filePath);

					if (NewEmployeeForCartoteca != null)
					{
						var emplPhoto = new EmployeePhoto();
						emplPhoto.Photo = Photo;
						NewEmployeeForCartoteca.EmployeePhotos = emplPhoto;
					}
				}
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex, user: UserDataCurrent.UserName,
					machine: UserDataCurrent.MachineName).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Асинхронно подготавливает данные для нового сотрудника внешней организации.
		/// </summary>
		private async Task CreateNewEmployeeAsync()
		{
			try
			{
				CreateNewEmployeeFlag = true;

				NewEmployeeForCartoteca = new()
				{
					DateDismissal = DateTime.Parse("31.12.1876"),
					DateEmployment = DateTime.Now.Date,
					DepartmentID = ItemDepartmentProductions.DepartmentID
				};
				ShowAndHidenElemets();
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex, user: UserDataCurrent.UserName,
					machine: UserDataCurrent.MachineName).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Устанавливает коллекцию сотрудников внешних организаций в зависимости от их статуса увольнения.
		/// </summary>
		/// <param name="dubleEmployeesForCartoteca">Список сотрудников внешних организаций.</param>
		private void SetCollectionsEmployees(List<Employee> dubleEmployeesForCartoteca)
		{
			if (EmployeesForCartoteca is null || dubleEmployeesForCartoteca is null) return;

			List<Employee> employeeExes = [];
			if (ShowDismissalEmployee)
				employeeExes = dubleEmployeesForCartoteca.Where(x => x.IsDismissal).ToList();

			else if (!ShowDismissalEmployee)
				employeeExes = dubleEmployeesForCartoteca.Where(x => x.IsDismissal == false).ToList();

			EmployeesForCartoteca = new ObservableCollection<Employee>(employeeExes);
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
		/// Получает или задает коллекцию участков.
		/// </summary>
		public List<DepartmentProduction>? DepartmentProductions
		{
			get => _departmentProductions;
			set => SetProperty(ref _departmentProductions, value);
		}
		private List<DepartmentProduction>? _departmentProductions;

		/// <summary>
		/// Выбранный участок из коллекцию участков.
		/// </summary>
		public DepartmentProduction? ItemDepartmentProductions
		{
			get => _itemDepartmentProductions;
			set
			{
				SetProperty(ref _itemDepartmentProductions, value);

				if (ItemDepartmentProductions != null && NewEmployeeForCartoteca != null)
					NewEmployeeForCartoteca.DepartmentID = ItemDepartmentProductions.DepartmentID;
			}
		}
		private DepartmentProduction? _itemDepartmentProductions;

		/// <summary>
		/// Получает или задает коллекцию сотрудников внешних организаций для картотеки.
		/// </summary>
		public ObservableCollection<Employee>? EmployeesForCartoteca
		{
			get => _employeesForCartoteca;
			set
			{
				SetProperty(ref _employeesForCartoteca, value);
				if (EmployeesForCartoteca != null && EmployeesForCartoteca.Count > 0)
				{
					SelectedEmployeeForCartoteca = EmployeesForCartoteca.FirstOrDefault();
				}
				else
				{
					SelectedEmployeeForCartoteca = null;
					NewEmployeeForCartoteca = null;
				}
			}
		}
		private ObservableCollection<Employee>? _employeesForCartoteca;

		/// <summary>
		/// Получает или задает коллекцию сотрудников внешних организаций (дублирующая).
		/// </summary>
		public List<Employee>? DubleEmployeesForCartoteca
		{
			get => _dubleEmployeesForCartoteca;
			set => SetProperty(ref _dubleEmployeesForCartoteca, value);
		}
		private List<Employee>? _dubleEmployeesForCartoteca;

		public bool CreateNewEmployeeFlag { get; private set; }
		public DateTime DefaultDateDismissal { get; private set; } = DateTime.Parse("31.12.1876");

		/// <summary>
		/// Задает нового сотрудника внешней организации для картотеки.
		/// </summary>
		public Employee? NewEmployeeForCartoteca
		{
			get => _newEmployeeForCartoteca;
			set
			{
				SetProperty(ref _newEmployeeForCartoteca, value);
			}
		}
		private Employee? _newEmployeeForCartoteca;

		/// <summary>
		/// Получает  выбранного сотрудника внешней организации для картотеки.
		/// </summary>
		public Employee? SelectedEmployeeForCartoteca
		{
			get => _itemEmployeeForCartoteca;
			set
			{
				SetProperty(ref _itemEmployeeForCartoteca, value);

				if (SelectedEmployeeForCartoteca != null)
				{
					NewEmployeeForCartoteca = SelectedEmployeeForCartoteca;
					ItemDepartmentProductions = DepartmentProductions
						.Where(x => x.DepartmentID == NewEmployeeForCartoteca.DepartmentID)
						.FirstOrDefault();

					HidenElemets();
					CreateNewEmployeeFlag = false;
				}
				else HidenElemets();
			}
		}
		private Employee? _itemEmployeeForCartoteca;

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
		public bool ShowDismissalEmployee
		{
			get => _showDismissalEmployee;
			set
			{
				SetProperty(ref _showDismissalEmployee, value);
				SetCollectionsEmployees(DubleEmployeesForCartoteca!);
			}
		}
		private bool _showDismissalEmployee;

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
		/// Получает или задает фото сотрудника внешней организации.
		/// </summary>
		public byte[]? Photo { get; private set; }
		/// <summary>
		/// Получает или задает данные текущего пользователя.
		/// </summary>
		public LocalUserData UserDataCurrent { get; private set; } = new LocalUserData { MachineName = string.Empty, UserName = string.Empty };
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
		public DateTime StartDate { get; private set; } = DateTime.Now.Date;
		/// <summary>
		/// Получает или задает конечную дату.
		/// </summary>
		public DateTime EndDate { get; private set; }

		public List<Employee>? EmployeesForCartotecas { get; private set; }

		#endregion

		#region Commands
		public ICommand? RefreshCmd { get; set; }
		public ICommand? CloseWindowCmd { get; set; }
		public ICommand? SaveDataForEmployeeCmd { get; set; }
		public ICommand? DismissalEmployeeCmd { get; set; }
		public ICommand? EditEmployeeCmd { get; set; }
		public ICommand? CreateNewEmployeeCmd { get; set; }
		public ICommand? LoadPhotoCmd { get; set; }
		public ICommand? CloseCmd { get; private set; }
		public ICommand? UpdCmd { get; private set; }
		#endregion
	}
}
