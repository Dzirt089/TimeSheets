using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Win32;

using ProductionControl.Entitys;
using ProductionControl.Models;
using ProductionControl.Services.Interfaces;
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
		ITimeSheetDbService timeSheetDb,
		IErrorLogger errorLogger) : ObservableObject
	{
		private readonly ITimeSheetDbService _timeSheetDb = timeSheetDb;
		private readonly IErrorLogger _errorLogger = errorLogger;
		private StaffExternalOrgView? ExternalOrgView { get; set; }

		/// <summary>
		/// Инициализирует данные для представления.
		/// </summary>
		/// <param name="externalOrgView">Представление для инициализации.</param>
		public async Task Initializing(StaffExternalOrgView externalOrgView)
		{
			try
			{
				ExternalOrgView = externalOrgView;
				ShowDismissalEmployeeExOrg = false;
				VisibilityAddMainRegion = Visibility.Visible;

				UserDataCurrent = await _timeSheetDb.GetLocalUserAsync() ??
						new() { MachineName = string.Empty, UserName = string.Empty };

				HidenElemets();

				VisibilityButtons = Visibility.Visible;

				LoadPhotoCmd = new AsyncRelayCommand(LoadPhotoAsync);
				CreateNewEmployeeExOrgCmd = new AsyncRelayCommand(CreateNewEmployeeExOrgAsync);
				EditEmployeeExOrgCmd = new AsyncRelayCommand(EditEmployeeExOrgAsync);
				DismissalEmployeeExOrgCmd = new AsyncRelayCommand(DismissalEmployeeExOrgAsync);
				RefreshCmd = new AsyncRelayCommand(RefreshAsync);

				CloseExOrgCmd = new RelayCommand(Close);
				SaveDataForEmployeeExOrgCmd = new AsyncRelayCommand(SaveEmployeeExOrgAsync);
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
		private async Task SaveEmployeeExOrgAsync()
		{
			try
			{
				if (NewEmployeeForCartoteca is null) return;

				var idEmpLast = NewEmployeeForCartoteca.EmployeeID;

				if (NewEmployeeForCartoteca.DateDismissal != DateTime.Parse("31.12.1876") && NewEmployeeForCartoteca.IsDismissal == false)
					NewEmployeeForCartoteca.IsDismissal = true;


				//if (NewEmployeeForCartoteca != null)
				//	await _timeSheetDb.UpdateEmployeeExOrgAsync(
				//		NewEmployeeForCartoteca, ValueDepartmentID, UserDataCurrent);
				//else
					await _timeSheetDb.AddEmployeeExOrgAsync(NewEmployeeForCartoteca, UserDataCurrent);

				VisibilityAddMainRegion = Visibility.Visible;

				await RefreshAsync();

				SelectedEmployeeForCartotecaExOrg = EmployeesForCartotecaExOrg.FirstOrDefault(x => x.EmployeeID == idEmpLast);
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
				//EmployeesForCartotecas = await _timeSheetDb.GetEmployeeExOrgsNoDismissalAsync(UserDataCurrent);

				EmployeesForCartotecaExOrg = new ObservableCollection<Employee>(EmployeesForCartotecas);
				DubleEmployeesForCartotecaExOrg = new List<Employee>(EmployeesForCartotecas);

				SetCollectionsEmployeesExOrg(DubleEmployeesForCartotecaExOrg);
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
		private async Task DismissalEmployeeExOrgAsync()
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
					IsEnabledDateDismissal = true;
					IsEnabledTextBox = false;
					VisibilityButtonLoad = Visibility.Hidden;
					NewEmployeeForCartoteca.DateDismissal = DateTime.Now.Date;
				}
			}
			catch (Exception ex)
			{
				await _errorLogger.ProcessingErrorLogAsync(ex, user: UserDataCurrent.UserName,
					machine: UserDataCurrent.MachineName).ConfigureAwait(false);
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
						NewEmployeeForCartoteca.Photo = Photo;
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
		private async Task CreateNewEmployeeExOrgAsync()
		{
			try
			{
				NewEmployeeForCartoteca = new()
				{
					DateDismissal = DateTime.Parse("31.12.1876"),
					DateEmployment = DateTime.Now.Date
				};
				VisibilityAddMainRegion = Visibility.Hidden;
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
		/// <param name="dubleEmployeesForCartotecaExOrg">Список сотрудников внешних организаций.</param>
		private void SetCollectionsEmployeesExOrg(List<Employee> dubleEmployeesForCartotecaExOrg)
		{
			if (EmployeesForCartotecaExOrg is null || dubleEmployeesForCartotecaExOrg is null) return;

			List<Employee> employeeExes = [];
			if (ShowDismissalEmployeeExOrg)
				employeeExes = dubleEmployeesForCartotecaExOrg.Where(x => x.IsDismissal).ToList();

			else if (!ShowDismissalEmployeeExOrg)
				employeeExes = dubleEmployeesForCartotecaExOrg.Where(x => x.IsDismissal == false).ToList();

			EmployeesForCartotecaExOrg = new ObservableCollection<Employee>(employeeExes);
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
		public ObservableCollection<Employee>? EmployeesForCartotecaExOrg
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
					NewEmployeeForCartoteca = null;
				}
			}
		}
		private ObservableCollection<Employee>? _employeesForCartotecaExOrg;

		/// <summary>
		/// Получает или задает коллекцию сотрудников внешних организаций (дублирующая).
		/// </summary>
		public List<Employee>? DubleEmployeesForCartotecaExOrg
		{
			get => _dubleEmployeesForCartotecaExOrg;
			set => SetProperty(ref _dubleEmployeesForCartotecaExOrg, value);

		}
		private List<Employee>? _dubleEmployeesForCartotecaExOrg;

		/// <summary>
		/// Задает нового сотрудника внешней организации для картотеки.
		/// </summary>
		public Employee? NewEmployeeForCartoteca
		{
			get => _newEmployeeForCartotecaExOrg;
			set
			{
				SetProperty(ref _newEmployeeForCartotecaExOrg, value);
			}
		}
		private Employee? _newEmployeeForCartotecaExOrg;

		/// <summary>
		/// Получает  выбранного сотрудника внешней организации для картотеки.
		/// </summary>
		public Employee? SelectedEmployeeForCartotecaExOrg
		{
			get => _itemEmployeeForCartotecaExOrg;
			set
			{
				SetProperty(ref _itemEmployeeForCartotecaExOrg, value);

				if (SelectedEmployeeForCartotecaExOrg != null)
				{
					NewEmployeeForCartoteca = SelectedEmployeeForCartotecaExOrg;
					HidenElemets();
				}
				else HidenElemets();
			}
		}
		private Employee? _itemEmployeeForCartotecaExOrg;

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

		private bool _addWorkingInReg;

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
		public DateTime StartDate { get; private set; }
		/// <summary>
		/// Получает или задает конечную дату.
		/// </summary>
		public DateTime EndDate { get; private set; }

		public List<Employee>? EmployeesForCartotecas { get; private set; }


		#endregion

		#region Commands
		public ICommand? RefreshCmd { get; set; }
		public ICommand? CloseExOrgCmd { get; set; }
		public ICommand? SaveDataForEmployeeExOrgCmd { get; set; }
		public ICommand? DismissalEmployeeExOrgCmd { get; set; }
		public ICommand? EditEmployeeExOrgCmd { get; set; }
		public ICommand? CreateNewEmployeeExOrgCmd { get; set; }
		public ICommand? LoadPhotoCmd { get; set; }
		#endregion
	}
}
