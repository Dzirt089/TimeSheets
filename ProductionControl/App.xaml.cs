using MahApps.Metro.Controls.Dialogs;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using ProductionControl.DAL;
using ProductionControl.Services;
using ProductionControl.Services.API;
using ProductionControl.Services.API.Interfaces;
using ProductionControl.Services.DynamicGrid;
using ProductionControl.Services.Interfaces;
using ProductionControl.ViewModel;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


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
				services.AddDbContextFactory<ShiftTimesDbContext>(
					options =>
					{
						//options.UseSqlServer(Settings.Default.ConTimeSheet);
						options.UseSqlServer(Settings.Default.ConTimeSheetN);
					});
				
				services.AddTransient<StaffViewModel>();
				services.AddScoped<FAQViewModel>();
				services.AddScoped<ITimeSheetDbService, TimeSheetDbService>();
				services.AddScoped<IResultSheetsService, ResultSheetsService>();
				services.AddSingleton<HttpClientForProject>();
				services.AddScoped<IApiProductionControl, ApiProductionControl>();
				services.AddScoped<IErrorLogger, ErrorLogger>();
				services.AddSingleton<DynamicColumnsBehaviorvTO2>();
				services.AddScoped<IDialogCoordinator, DialogCoordinator>();
				services.AddScoped<MainViewModel>();
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
					MessageBox.Show("Ошибка в запуске приложения! Обратитесь, пожалуйста, к разработчикам ТО");
				}
			}
		}

		protected override async void OnStartup(StartupEventArgs e)
		{
			try
			{
				await Host.StartAsync();

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
					MessageBox.Show("Ошибка в запуске приложения! Обратитесь, пожалуйста, к разработчикам ТО");
				}

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

		/// <summary>
		/// Метод обработки двойного клика на элементе `TextBlock`, заменяя его на `TextBox`.
		/// </summary>
		/// <param name="textBlock">Элемент `TextBlock`, на который был выполнен двойной клик.</param>
		private void MethodMouse(TextBlock textBlock)
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

		/// <summary>
		/// Обработчик события потери фокуса `TextBox`.
		/// Когда `TextBox` теряет фокус, он скрывается, и на его месте снова появляется `TextBlock`.
		/// </summary>
		/// <param name="sender">Элемент, вызвавший событие (должен быть `TextBox`).</param>
		/// <param name="e">Данные события.</param>
		private void TextBox_LostFocus(object sender, RoutedEventArgs e)
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

		/// <summary>
		/// Рекурсивный метод для поиска родительского элемента заданного типа.
		/// </summary>
		/// <typeparam name="T">Тип искомого родительского элемента.</typeparam>
		/// <param name="child">Дочерний элемент, для которого ищется родитель.</param>
		/// <returns>Родительский элемент типа `T`, если найден; иначе `null`.</returns>
		private T FindParent<T>(DependencyObject child) where T : DependencyObject
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

		/// <summary>
		/// Обработчик события нажатия клавиши в `DataGrid`.
		/// Перенаправляет события клавиш Tab и Enter на соответствующие методы.
		/// </summary>
		/// <param name="sender">Элемент, вызвавший событие (должен быть `DataGrid`).</param>
		/// <param name="e">Данные события клавиатуры.</param>
		private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			DataGrid_PreviewKeyDownTab(sender, e);
			DataGrid_PreviewKeyDownEnter(sender, e);
			DataGrid_PreviewKeyDownEsc(sender, e);
		}
		private void LostFocus(TextBox textBox)
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
		private void DataGrid_PreviewKeyDownEsc(object sender, KeyEventArgs e)
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

		/// <summary>
		/// Обработчик события нажатия клавиши Tab в `DataGrid`.
		/// Перемещает фокус на следующую ячейку в строке.
		/// </summary>
		/// <param name="sender">Элемент, вызвавший событие (должен быть `DataGrid`).</param>
		/// <param name="e">Данные события клавиатуры.</param>
		private void DataGrid_PreviewKeyDownTab(object sender, KeyEventArgs e)
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

		/// <summary>
		/// Обработчик события нажатия клавиши Enter в `DataGrid`.
		/// Перемещает фокус на ячейку в следующей строке.
		/// </summary>
		/// <param name="sender">Элемент, вызвавший событие (должен быть `DataGrid`).</param>
		/// <param name="e">Данные события клавиатуры.</param>
		private void DataGrid_PreviewKeyDownEnter(object sender, KeyEventArgs e)
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

		#region Пытался придумать что-нибудь с TextBox-ом. Проблема осталась, не могу работать напрямую с ним

		//private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		//{
		//	e.Handled = true;
		//	DataGrid dataGrid = sender as DataGrid;
		//	if (dataGrid == null) return;

		//	var cell = dataGrid.CurrentCell;
		//	int columnIndex = cell.Column.DisplayIndex;
		//	int rowIndex = dataGrid.Items.IndexOf(dataGrid.CurrentItem);
		//	dataGrid.CurrentCell = new DataGridCellInfo(dataGrid.Items[rowIndex], dataGrid.Columns[columnIndex]);
		//	dataGrid.BeginEdit();

		//	var cellContent = dataGrid.Columns[columnIndex].GetCellContent(dataGrid.Items[rowIndex]);



		//	if (cellContent != null)
		//	{
		//		FindChildElement2(cellContent);
		//	}
		//}

		//	private void FindChildElement2(DependencyObject child)
		//	{
		//		if (child is Grid grid)
		//		{
		//			TextBox? textBox = grid.FindName("NameTextBox") as TextBox;
		//			if (textBox != null)
		//			{
		//				TextBoxMouse(textBox);
		//			}
		//		}
		//		else
		//		{
		//			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(child); i++)
		//			{
		//				FindChildElement2(VisualTreeHelper.GetChild(child, i));
		//			}
		//		}
		//	}
		//	private void TextBox_LostFocus_1(object sender, RoutedEventArgs e)
		//	{
		//		if (sender is TextBox box)
		//		{
		//			// Находим родительский элемент Grid
		//			var grid = FindParent<Grid>(box);
		//			if (grid != null)
		//			{
		//				// Находим соответствующий TextBlock и делаем его видимым
		//				var nameTextBox = box.Name;
		//				var textBox = grid.FindName(nameTextBox) as TextBox;
		//				if (textBox != null)
		//				{
		//					textBox.IsEnabled = false;
		//				}
		//			}
		//		}
		//	}

		//	private void TextBoxMouse(TextBox box)
		//	{
		//		// Находим родительский элемент Grid
		//		var grid = FindParent<Grid>(box);
		//		if (grid != null)
		//		{
		//			// Находим соответствующий TextBlock и делаем его видимым	
		//			var nameTextBox = box.Name;
		//			var textBox = grid.FindName(nameTextBox) as TextBox;
		//			if (textBox != null)
		//			{
		//				textBox.IsEnabled = true;
		//				textBox.Focus();
		//			}
		//		}
		//	}
		#endregion
	}

}
