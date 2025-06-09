using AutoMapper;

using MahApps.Metro.Controls.Dialogs;

using MailerVKT;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using ProductionControl.ApiClients.DefinitionOfNonWorkingDaysApiServices.Implementation;
using ProductionControl.ApiClients.DefinitionOfNonWorkingDaysApiServices.Interfaces;
using ProductionControl.ApiClients.ProductionApiServices.EmployeesExternalOrganizationsApiServices.Implementation;
using ProductionControl.ApiClients.ProductionApiServices.EmployeesExternalOrganizationsApiServices.Interfaces;
using ProductionControl.ApiClients.ProductionApiServices.EmployeeSheetApiServices.Implementation;
using ProductionControl.ApiClients.ProductionApiServices.EmployeeSheetApiServices.Interfaces;
using ProductionControl.ApiClients.ProductionApiServices.ReportsApiServices.Implementation;
using ProductionControl.ApiClients.ProductionApiServices.ReportsApiServices.Interfaces;
using ProductionControl.ApiClients.ProductionApiServices.ResultSheetsApiServices.Implementation;
using ProductionControl.ApiClients.ProductionApiServices.ResultSheetsApiServices.Interfaces;
using ProductionControl.ApiClients.ProductionApiServices.SizEmployeeApiServices.Implementation;
using ProductionControl.ApiClients.ProductionApiServices.SizEmployeeApiServices.Interfaces;
using ProductionControl.Services.DynamicGrid;
using ProductionControl.Services.DynamicGrid.ExternalOrganization;
using ProductionControl.Services.ErrorLogsInformation;
using ProductionControl.UIModels.Dtos.ExternalOrganization;
using ProductionControl.UIModels.Model.GlobalPropertys;
using ProductionControl.ViewModel;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

using MailService = ProductionControl.Services.Mail.MailService;

namespace ProductionControl
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public static IHost Host { get; private set; }
		public App()
		{
			Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
				.ConfigureServices((context, services) =>
				{
					ConfigureServices(services);
				}).Build();
		}
		//Add Services in DI
		private static void ConfigureServices(IServiceCollection services)
		{
			try
			{
				#region Глобальные модели свойств

				services.AddSingleton<GlobalEmployeeSessionInfo>();
				services.AddSingleton<GlobalSettingsProperty>();

				#endregion

				#region API-Сервисы и API-клиенты
				
				services.AddHttpClient("ProductionApi", client =>
				{
					client.BaseAddress = new Uri(Settings.Default.Test_Prodaction_API);
					client.DefaultRequestHeaders.Add("Accept", "application/json");
					client.Timeout = TimeSpan.FromSeconds(30);
				});
				services.AddHttpClient("VKTApi", client =>
				{
					client.BaseAddress = new Uri(Settings.Default.VKT_API);
					client.DefaultRequestHeaders.Add("Accept", "application/json");
					client.Timeout = TimeSpan.FromSeconds(30);
				});

				services.AddScoped<IReportsApiClient, ReportsApiClient>();
				services.AddScoped<IResultSheetsApiClient, ResultSheetsApiClient>();
				services.AddScoped<IEmployeeSheetApiClient, EmployeeSheetApiClient>();
				services.AddScoped<IDefinitionOfNonWorkingDaysApiClient, DefinitionOfNonWorkingDaysApiClient>();
				services.AddScoped<IEmployeesExternalOrganizationsApiClient, EmployeesExternalOrganizationsApiClient>();
				services.AddScoped<ISizEmployeeApiClient, SizEmployeeApiClient>();

				#endregion

				#region ViewModels

				services.AddScoped<StaffViewModel>();
				services.AddScoped<StaffExternalOrgViewModel>();
				services.AddScoped<FAQViewModel>();
				services.AddScoped<MainViewModel>();

				#endregion

				#region Email и логирование ошибок

				services.AddScoped<IErrorLogger, ErrorLogger>();
				services.AddScoped<Sender>();
				services.AddScoped<MailService>();

				#endregion

				#region Регистрация сервисов для работы с динамическими колонками

				services.AddSingleton<DynamicColumnsBehaviorvTO2>();
				services.AddSingleton<DynamicColumnsBehaviorvExpOrg>();

				#endregion

				services.AddScoped<IDialogCoordinator, DialogCoordinator>();
				services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
			}
			catch (Exception ex)
			{
				try
				{
					var d = Host.Services.GetRequiredService<IErrorLogger>();
					d.ProcessingErrorLog(ex);
				}
				catch (Exception e)
				{
					var mailService = Host.Services.GetRequiredService<MailService>();
					mailService.SendMailAsync(new MailerVKT.MailParameters
					{
						Text = $"Сводка об ошибке:\n\nMachine: {Environment.UserName} \n\nMessage: {ex.Message}\n\nStackTrace: {ex.StackTrace}\n\nSource: {ex.Source}\n\nInnerException: {ex.InnerException}",
						Recipients = ["teho19@vkt-vent.ru"],
						Subject = "Errors in Production Control",
						SenderName = "Production Control",
					}).ConfigureAwait(false);
					MessageBox.Show("Ошибка в запуске приложения! Обратитесь, пожалуйста, к разработчикам ТО");
				}
			}
		}

		protected override async void OnStartup(StartupEventArgs e)
		{
			try
			{
				await Host.StartAsync();

				// Обработчик исключений UI-потока
				DispatcherUnhandledException += App_DispatcherUnhandledException;

				// Обработчик исключений в фоновых потоках
				AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

				// Обработчик необработанных исключений в Task
				TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

				var mapper = Host.Services.GetRequiredService<IMapper>();
				mapper.ConfigurationProvider.AssertConfigurationIsValid();

				MailServices = Host.Services.GetRequiredService<MailService>();
				var mainWindow = new MainWindow();
				mainWindow.Show();

				var viewModel = Host.Services.GetRequiredService<MainViewModel>();
				await viewModel.InitializeAsync();
				mainWindow.DataContext = viewModel;



				base.OnStartup(e);
			}
			catch (Exception ex)
			{
				try
				{
					var d = Host.Services.GetRequiredService<IErrorLogger>();
					d.ProcessingErrorLog(ex);
				}
				catch (Exception)
				{
					string text = "Ошибка в запуске приложения! Обратитесь, пожалуйста, к разработчикам ТО";
					SendMailWithErrors(ex, text);
				}

			}
		}

		/// <summary>
		/// Обработчик необработанных исключений в Task
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			HandleException(e.Exception);
			e.SetObserved(); // Помечаем исключение как обработанное
		}

		/// <summary>
		/// Обработчик необработанных исключений в AppDomain
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			HandleException((Exception)e.ExceptionObject);
		}

		/// <summary>
		/// Обработчик исключений UI-потока
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			HandleException(e.Exception);
			e.Handled = true; // Предотвращаем крах приложения
		}

		/// <summary>
		/// Обработчик исключений с выводом сообщения пользователю и отправкой на почту
		/// </summary>
		/// <param name="ex"></param>
		private void HandleException(Exception ex)
		{
			try
			{
				var mail = Host.Services.GetRequiredService<ErrorLogger>();
				mail.ProcessingErrorLogAsync(ex).ConfigureAwait(false);

				MessageBox.Show(
				"Произошла критическая ошибка. Приложение будет закрыто.",
				"Основная ошибка: " + ex.Message,
				MessageBoxButton.OK,
				MessageBoxImage.Error);
			}
			catch (Exception e)
			{
				// Показ сообщения пользователю
				MessageBox.Show(
				"Произошла критическая ошибка. Приложение будет закрыто.",
				"Основная ошибка: " + ex.Message + ".\n\nВторая ошибка :" + e.Message,
				MessageBoxButton.OK,
				MessageBoxImage.Error);
			}
		}

		protected override void OnExit(ExitEventArgs e)
		{
			try
			{
				var viewModel = Host.Services.GetRequiredService<MainViewModel>();
				viewModel.Dispose();
				var check = viewModel.ClearIdAccessRightFromDepartment();
			}
			catch (Exception ex)
			{
				string text = "Ошибка в завершении приложения! Обратитесь, пожалуйста, к разработчикам ТО";
				SendMailWithErrors(ex, text);
			}
			finally
			{
				Host.StopAsync();
				base.OnExit(e);
			}
		}

		/// <summary>
		/// Метод сброса таймера и очистки кликов.
		/// </summary>
		/// <param name="o">Не используется.</param>
		private void ResetTimer(object o)
		{
			DoubleClick = null;
			OneClick = null;
			Timer.Dispose();
		}

		/// <summary>
		/// Обработчик события левого щелчка мыши на элементе `TextBlock`.
		/// Если произошел двойной клик (два клика с промежутком менее 500 миллисекунд), 
		/// заменяет `TextBlock` на `TextBox` для редактирования.
		/// </summary>
		/// <param name="sender">Элемент, вызвавший событие (должен быть `TextBlock`).</param>
		/// <param name="e">Данные события мыши.</param>
		private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			try
			{
				// Если первый клик еще не зарегистрирован, сохраняем его время
				if (OneClick is null)
				{
					Timer = new Timer(ResetTimer, 0, 500, 0);
					OneClick = DateTime.Now;
					return;
				}

				// Если первый клик уже был, проверяем второй клик
				if (DoubleClick is null && OneClick != null)
				{
					Timer.Dispose();
					DoubleClick = DateTime.Now;
					TimeSpan timeSpan = (DateTime)DoubleClick - (DateTime)OneClick;
					var milisec = timeSpan.TotalMilliseconds;

					// Если второй клик произошел в течение 500 миллисекунд, обрабатываем как двойной клик
					if (milisec <= 500.00)
					{
						if (sender is TextBlock textBlock)
							MethodMouse(textBlock);
					}

					// Сбрасываем время кликов после обработки
					OneClick = null;
					DoubleClick = null;
				}
			}
			catch (Exception ex)
			{
				string text = "Произошел сбой в программе! Обратитесь за помощью к разработчикам в Тех.Отдел.";
				SendMailWithErrors(ex, text);
			}
		}

		/// <summary>
		/// Метод обработки двойного клика на элементе `TextBlock`, заменяя его на `TextBox`.
		/// </summary>
		/// <param name="textBlock">Элемент `TextBlock`, на который был выполнен двойной клик.</param>
		private void MethodMouse(TextBlock textBlock)
		{
			try
			{
				// Находим родительский элемент Grid
				var grid = FindParent<Grid>(textBlock);
				if (grid != null)
				{
					// Находим соответствующий TextBox и делаем его видимым
					NameTextBlock = textBlock.Name;
					TextBoxName = textBlock.Name.Replace("TextBlock", "TextBox");
					if (grid.FindName(TextBoxName) is TextBox textBox)
					{
						textBlock.Visibility = Visibility.Collapsed;
						textBox.Visibility = Visibility.Visible;
						textBox.Focus();
						textBox.SelectAll();
					}
				}
			}
			catch (Exception ex)
			{
				string text = "Произошел сбой в программе! Обратитесь за помощью к разработчикам в Тех.Отдел.";
				SendMailWithErrors(ex, text);
			}
		}

		/// <summary>
		/// Обработчик события потери фокуса `TextBox`.
		/// Когда `TextBox` теряет фокус, он скрывается, и на его месте снова появляется `TextBlock`.
		/// </summary>
		/// <param name="sender">Элемент, вызвавший событие (должен быть `TextBox`).</param>
		/// <param name="e">Данные события.</param>
		private void TextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			try
			{
				if (sender is TextBox textBox)
				{
					// Находим родительский элемент Grid
					var grid = FindParent<Grid>(textBox);
					if (grid != null)
					{
						// Находим соответствующий TextBlock и делаем его видимым
						var textBlockName = textBox.Name.Replace("TextBox", "TextBlock");
						if (grid.FindName(textBlockName) is TextBlock textBlock)
						{
							textBox.Visibility = Visibility.Collapsed;
							textBlock.Visibility = Visibility.Visible;
						}
					}
				}
			}
			catch (Exception ex)
			{
				string text = "Произошел сбой в программе! Обратитесь за помощью к разработчикам в Тех.Отдел.";
				SendMailWithErrors(ex, text);
			}
		}

		/// <summary>
		/// Рекурсивный метод для поиска родительского элемента заданного типа.
		/// </summary>
		/// <typeparam name="T">Тип искомого родительского элемента.</typeparam>
		/// <param name="child">Дочерний элемент, для которого ищется родитель.</param>
		/// <returns>Родительский элемент типа `T`, если найден; иначе `null`.</returns>
		private T FindParent<T>(DependencyObject child) where T : DependencyObject
		{
			try
			{
				DependencyObject parentObject = VisualTreeHelper.GetParent(child);
				if (parentObject == null) return null;

				if (parentObject is T parent)
				{
					return parent;
				}
				else
				{
					return FindParent<T>(parentObject);
				}
			}
			catch (Exception ex)
			{
				string text = "Произошел сбой в программе! Обратитесь за помощью к разработчикам в Тех.Отдел.";
				SendMailWithErrors(ex, text);

				return null;
			}
		}

		/// <summary>
		/// Время первого клика мыши на элементе `TextBlock`.
		/// Если первый клик еще не произошел, значение равно `null`.
		/// </summary>
		private DateTime? OneClick { get; set; } = null;

		/// <summary>
		/// Время второго клика мыши на элементе `TextBlock`.
		/// Если второй клик еще не произошел, значение равно `null`.
		/// </summary>
		private DateTime? DoubleClick { get; set; } = null;

		/// <summary>
		/// Таймер для автоматического сброса кликов, 
		/// если второй клик не последовал за первым в течение заданного времени.
		/// </summary>
		private Timer? Timer { get; set; } = null;

		/// <summary>
		/// Имя текущего `TextBlock`, который находится в состоянии редактирования.
		/// </summary>
		private string NameTextBlock { get; set; } = string.Empty;
		public string TextBoxName { get; private set; }

		/// <summary>
		/// Метод для поиска дочернего элемента по его типу или по имени в визуальном дереве.
		/// </summary>
		/// <param name="child">Дочерний элемент, в котором ищутся нужные элементы.</param>
		private void FindChildElement(DependencyObject child, bool keyEsc = false)
		{
			try
			{
				if (child is Grid grid)
				{
					if (grid.FindName(NameTextBlock) is TextBlock textBlock && keyEsc == false)
					{
						MethodMouse(textBlock);
					}
					else if (keyEsc)
					{
						if (grid.FindName(TextBoxName) is TextBox textBox)
						{
							LostFocus(textBox);
						}
					}
				}
				else
				{
					for (int i = 0; i < VisualTreeHelper.GetChildrenCount(child); i++)
					{
						FindChildElement(VisualTreeHelper.GetChild(child, i), keyEsc);
					}
				}
			}
			catch (Exception ex)
			{
				string text = "Произошел сбой в программе! Обратитесь за помощью к разработчикам в Тех.Отдел.";
				SendMailWithErrors(ex, text);
			}
		}

		/// <summary>
		/// Обработчик события нажатия клавиши в `DataGrid`.
		/// Перенаправляет события клавиш Tab и Enter на соответствующие методы.
		/// </summary>
		/// <param name="sender">Элемент, вызвавший событие (должен быть `DataGrid`).</param>
		/// <param name="e">Данные события клавиатуры.</param>
		private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			try
			{
				DataGrid_PreviewKeyDownTab(sender, e);
				DataGrid_PreviewKeyDownEnter(sender, e);
				DataGrid_PreviewKeyDownEsc(sender, e);
			}
			catch (Exception ex)
			{
				string text = "Произошел сбой в программе! Обратитесь за помощью к разработчикам в Тех.Отдел.";
				SendMailWithErrors(ex, text);
			}
		}
		private void LostFocus(TextBox textBox)
		{
			try
			{
				// Находим родительский элемент Grid
				var grid = FindParent<Grid>(textBox);
				if (grid != null)
				{
					// Находим соответствующий TextBlock и делаем его видимым
					var textBlockName = textBox.Name.Replace("TextBox", "TextBlock");
					if (grid.FindName(textBlockName) is TextBlock textBlock)
					{
						textBox.Visibility = Visibility.Collapsed;
						textBlock.Visibility = Visibility.Visible;
					}
				}
			}
			catch (Exception ex)
			{
				string text = "Произошел сбой в программе! Обратитесь за помощью к разработчикам в Тех.Отдел.";
				SendMailWithErrors(ex, text);
			}
		}
		private void DataGrid_PreviewKeyDownEsc(object sender, KeyEventArgs e)
		{
			try
			{
				if (e.Key == Key.Escape)
				{
					e.Handled = true; // Предотвращаем стандартное поведение

					if (sender is not DataGrid dataGrid) return;

					var cell = dataGrid.CurrentCell;
					int columnIndex = cell.Column.DisplayIndex;
					int rowIndex = dataGrid.Items.IndexOf(dataGrid.CurrentItem);

					// Устанавливаем новый фокус
					dataGrid.CurrentCell = new DataGridCellInfo(dataGrid.Items[rowIndex], dataGrid.Columns[columnIndex]);
					dataGrid.BeginEdit();

					var cellContent = dataGrid.Columns[columnIndex].GetCellContent(dataGrid.Items[rowIndex]);

					if (cellContent != null)
					{
						FindChildElement(cellContent, true);
					}
				}
			}
			catch (Exception ex)
			{
				string text = "Произошел сбой в программе! Обратитесь за помощью к разработчикам в Тех.Отдел.";
				SendMailWithErrors(ex, text);
			}
		}

		/// <summary>
		/// Обработчик события нажатия клавиши Tab в `DataGrid`.
		/// Перемещает фокус на следующую ячейку в строке.
		/// </summary>
		/// <param name="sender">Элемент, вызвавший событие (должен быть `DataGrid`).</param>
		/// <param name="e">Данные события клавиатуры.</param>
		private void DataGrid_PreviewKeyDownTab(object sender, KeyEventArgs e)
		{
			try
			{
				if (e.Key == Key.Tab)
				{
					e.Handled = true; // Предотвращаем стандартное поведение

					if (sender is not DataGrid dataGrid) return;

					var cell = dataGrid.CurrentCell;
					int columnIndex = cell.Column.DisplayIndex;
					int rowIndex = dataGrid.Items.IndexOf(dataGrid.CurrentItem);

					if (columnIndex < dataGrid.Columns.Count - 1)
					{
						columnIndex++;
					}
					else
					{
						return; // Выход за пределы правого края таблицы
					}

					// Устанавливаем новый фокус
					dataGrid.CurrentCell = new DataGridCellInfo(dataGrid.Items[rowIndex], dataGrid.Columns[columnIndex]);
					dataGrid.BeginEdit();

					var cellContent = dataGrid.Columns[columnIndex].GetCellContent(dataGrid.Items[rowIndex]);

					if (cellContent != null)
					{
						FindChildElement(cellContent);
					}
				}
			}
			catch (Exception ex)
			{
				string text = "Произошел сбой в программе! Обратитесь за помощью к разработчикам в Тех.Отдел.";
				SendMailWithErrors(ex, text);
			}
		}

		/// <summary>
		/// Обработчик события нажатия клавиши Enter в `DataGrid`.
		/// Перемещает фокус на ячейку в следующей строке.
		/// </summary>
		/// <param name="sender">Элемент, вызвавший событие (должен быть `DataGrid`).</param>
		/// <param name="e">Данные события клавиатуры.</param>
		private void DataGrid_PreviewKeyDownEnter(object sender, KeyEventArgs e)
		{
			try
			{
				if (e.Key == Key.Enter)
				{
					e.Handled = true; // Предотвращаем стандартное поведение

					if (sender is not DataGrid dataGrid) return;

					var cell = dataGrid.CurrentCell;
					int columnIndex = cell.Column.DisplayIndex;
					int rowIndex = dataGrid.Items.IndexOf(dataGrid.CurrentItem);

					if (rowIndex < dataGrid.Items.Count - 1)
					{
						rowIndex++;
					}
					else
						return;

					// Устанавливаем новый фокус
					dataGrid.CurrentCell = new DataGridCellInfo(dataGrid.Items[rowIndex], dataGrid.Columns[columnIndex]);
					dataGrid.BeginEdit();

					var cellContent = dataGrid.Columns[columnIndex].GetCellContent(dataGrid.Items[rowIndex]);

					if (cellContent != null)
					{
						FindChildElement(cellContent);
					}

				}
			}
			catch (Exception ex)
			{
				string text = "Произошел сбой в программе! Обратитесь за помощью к разработчикам в Тех.Отдел.";
				SendMailWithErrors(ex, text);
			}
		}

		/*	Пояснения к коду ниже (обработка цветами смен для СО):
		 
			TextBlockSO, ShiftDataSO, TextBoxSO: Эти свойства используются для хранения ссылок на элементы внутри ячейки.ShiftDataSO в данном коде не используется, но оставлен, так как вы упоминали о нем ранее.
			SetCellColor: Этот метод устанавливает цвет текста для TextBlockSO.
			MenuItem_Click_Color1 и MenuItem_Click_Color: Обработчики событий для пунктов контекстного меню. Вызывают SetCellColor с нужным цветом.
			FindChildElementSO: Ключевой метод. Рекурсивно обходит визуальное дерево, начиная с содержимого ячейки, и ищет TextBlock и TextBox по имени.
			DataGrid_MouseRightButtonDown: Обработчик события клика правой кнопкой мыши по DataGrid. Определяет, по какой ячейке был клик, получает ее содержимое и вызывает FindChildElementSO.

			FindVisualChild<T>: Вспомогательный метод для рекурсивного поиска визуальных потомков.

			Использование HitTestResult: Позволяет точно определить элемент, по которому был произведен клик.
			Получение DataGridRow и DataGridCell: Обеспечивает корректное получение индексов строки и столбца.
			Рекурсивный поиск: FindChildElementSO позволяет найти TextBlock даже внутри сложной структуры DataTemplate.
			e.Handled = true;: Предотвращает дальнейшую обработку события, что важно для корректной работы контекстного меню при SelectionUnit = "FullRow".
		*/

		/// <summary>
		/// TextBlock для отображения данных в ячейке.
		/// </summary>
		public TextBlock TextBlockSO { get; private set; }

		/// <summary>
		/// Объект данных, связанный с ячейкой (ShiftDataExOrg). (В данном коде не используется, но оставлен для справки)
		/// </summary>
		public ShiftDataExOrgDto ShiftDataSO { get; private set; }

		/// <summary>
		/// TextBox, который также может находиться в ячейке (если необходимо).
		/// </summary>
		public TextBox TextBoxSO { get; private set; }
		public MailService MailServices { get; private set; }

		/// <summary>
		/// Устанавливает цвет текста для TextBlockSO.
		/// </summary>
		/// <param name="sender">Источник события (MenuItem).</param>
		/// <param name="color">Цвет текста (Brush).</param>
		private void SetCellColor(object sender, SolidColorBrush color)
		{
			try
			{
				if (TextBlockSO == null) return; // Проверка на null, если TextBlock не был найден
				TextBlockSO.Foreground = color; // Устанавливаем цвет текста
			}
			catch (Exception ex)
			{
				string text = "Произошел сбой в программе! Обратитесь за помощью к разработчикам в Тех.Отдел.";
				SendMailWithErrors(ex, text);
			}
		}

		/// <summary>
		/// Обработчик события MenuItem.Click для установки черного цвета текста.
		/// </summary>
		/// <param name="sender">Источник события (MenuItem).</param>
		/// <param name="e">Аргументы события.</param>
		private void MenuItem_Click_Color1(object sender, RoutedEventArgs e)
		{
			SetCellColor(sender, Brushes.Black);
		}

		/// <summary>
		/// Обработчик события MenuItem.Click для установки красного цвета текста.
		/// </summary>
		/// <param name="sender">Источник события (MenuItem).</param>
		/// <param name="e">Аргументы события.</param>
		private void MenuItem_Click_Color(object sender, RoutedEventArgs e)
		{
			SetCellColor(sender, Brushes.Red);
		}

		/// <summary>
		/// Рекурсивно ищет TextBlock и TextBox с заданными именами внутри визуального дерева.
		/// </summary>
		/// <param name="child">Начальный DependencyObject для поиска.</param>
		private void FindChildElementSO(DependencyObject child)
		{
			try
			{
				if (child is Grid grid) // Проверяем, является ли текущий элемент Grid
				{
					if (grid.FindName("NameSOTextBlock") is TextBlock textBlock)
					{
						TextBlockSO = textBlock; // Нашли TextBlock, сохраняем ссылку
					}
					if (grid.FindName("NameSOTextBox") is TextBox textBox)
					{
						TextBoxSO = textBox; // Нашли TextBox, сохраняем ссылку
					}
				}
				else
				{
					// Рекурсивный обход дочерних элементов
					for (int i = 0; i < VisualTreeHelper.GetChildrenCount(child); i++)
					{
						FindChildElementSO(VisualTreeHelper.GetChild(child, i));
					}
				}
			}
			catch (Exception ex)
			{
				string text = "Произошел сбой в программе! Обратитесь за помощью к разработчикам в Тех.Отдел.";
				SendMailWithErrors(ex, text);
			}
		}

		/// <summary>
		/// Обработчик события MouseRightButtonDown для DataGrid.
		/// </summary>
		/// <param name="sender">Источник события (DataGrid).</param>
		/// <param name="e">Аргументы события (MouseButtonEventArgs).</param>
		private void DataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			try
			{
				DataGrid dataGrid = (DataGrid)sender;
				// Получаем точку клика относительно DataGrid
				Point clickPoint = e.GetPosition(dataGrid);

				// Получаем объект HitTestResult, содержащий информацию о том, по какому элементу был клик
				HitTestResult hitTestResult = VisualTreeHelper.HitTest(dataGrid, clickPoint);

				if (hitTestResult != null)
				{
					// Находим DataGridRow
					DataGridRow? row = ItemsControl.ContainerFromElement(dataGrid, e.OriginalSource as DependencyObject) as DataGridRow;
					if (row != null)
					{
						int rowIndex = dataGrid.ItemContainerGenerator.IndexFromContainer(row); // Индекс строки

						DataGridCellsPresenter presenter = FindVisualChild<DataGridCellsPresenter>(row); // Получаем presenter ячеек
						if (presenter == null) return; // Если presenter не найден, выходим

						for (int i = 0; i < dataGrid.Columns.Count; ++i)
						{
							var cellNew = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(i);

							if (cellNew != null && cellNew.IsMouseOver && cellNew.ContextMenu != null) // Проверяем, находится ли курсор над ячейкой
							{
								var columnIndex = cellNew.Column.DisplayIndex; // Индекс столбца

								FrameworkElement? cellContent = dataGrid.Columns[columnIndex].GetCellContent(dataGrid.Items[rowIndex]);
								if (cellContent != null) // Проверяем, получено ли содержимое ячейки
								{
									FindChildElementSO(cellContent); // Ищем TextBlock внутри содержимого ячейки
								}

								// Открываем контекстное меню для этой ячейки
								cellNew.ContextMenu.IsOpen = true;
								e.Handled = true; // Важно! Предотвращаем дальнейшую обработку события (выделение строки)
								break; // Выходим из цикла, так как нужная ячейка найдена
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				string text = "Произошел сбой в программе! Обратитесь за помощью к разработчикам в Тех.Отдел.";
				SendMailWithErrors(ex, text);
			}
		}

		private void SendMailWithErrors(Exception ex, string text)
		{
			Task.Run(async () =>
			{
				await MailServices.SendMailAsync(new MailerVKT.MailParameters
				{
					Text = $"Сводка об ошибке:\n\nMachine: {Environment.UserName}  \n\nMessage: {ex.Message}\n\nStackTrace: {ex.StackTrace}\n\nSource: {ex.Source}\n\nInnerException: {ex.InnerException}",
					Recipients = ["teho19@vkt-vent.ru"],
					RecipientsBcc = ["progto@vkt-vent.ru"],
					Subject = "Errors in Production Control",
					SenderName = "Production Control",
				});
			});
			MessageBox.Show(text);
		}

		/// <summary>
		/// Рекурсивно ищет визуального потомка заданного типа.
		/// </summary>
		/// <typeparam name="T">Тип искомого потомка.</typeparam>
		/// <param name="depObj">DependencyObject, в котором производится поиск.</param>
		/// <returns>Найденный потомок типа T или null, если не найден.</returns>
		private static T FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
		{
			try
			{
				if (depObj != null)
				{
					for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
					{
						DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
						if (child != null && child is T)
						{
							return (T)child;
						}

						T childItem = FindVisualChild<T>(child); // Рекурсивный вызов для дочерних элементов
						if (childItem != null) return childItem;
					}
				}
				return null;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Произошел сбой в программе! Обратитесь за помощью к разработчикам в Тех.Отдел.");
				return null;
			}
		}
	}

}
