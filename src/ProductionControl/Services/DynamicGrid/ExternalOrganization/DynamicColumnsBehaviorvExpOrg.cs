using Microsoft.Xaml.Behaviors;

using ProductionControl.Models.Entitys.ExternalOrganization;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;

namespace ProductionControl.Services.DynamicGrid.ExternalOrganization
{
	/// <summary>
	/// Класс, который представляет собой поведение для динамического управления столбцами в DataGrid.
	/// </summary>
	[MarkupExtensionReturnType(typeof(DynamicColumnsBehaviorvExpOrg))]
	public class DynamicColumnsBehaviorvExpOrg : Behavior<DataGrid>
	{
		/// <summary>
		/// Метод, который вызывается при присоединении поведения к DataGrid.
		/// </summary>		
		protected override void OnAttached()
		{
			try
			{
				base.OnAttached();
				// Подписка на изменения свойства ItemsSource
				var item_source_property = DependencyPropertyDescriptor
					.FromProperty(
						ItemsControl.ItemsSourceProperty,
						AssociatedType /* typeof(DataGrid) */);

				item_source_property.AddValueChanged(AssociatedObject, OnItemSourceChanged);
				// Подписка на событие обновления источника данных
				AssociatedObject.SourceUpdated += OnSourceUpdated;
				// Отключение автоматической генерации столбцов
				AssociatedObject.AutoGenerateColumns = false;
				// Обновление источника данных
				UpdateItemSource(AssociatedObject.ItemsSource);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка: " + ex.Message);
				throw;
			}
		}
		/// <summary>
		/// Асинхронный метод, который вызывается при изменении источника данных DataGrid.
		/// </summary>
		private async void OnItemSourceChanged(object? sender, EventArgs e)
		{
			try
			{
				if (sender is not DataGrid { ItemsSource: { } source }) return;
				// Скрытие DataGrid перед обновлением
				HideDataGridAsync();
				// Асинхронное обновление источника данных
				await Task.Run(() => UpdateItemSource(source)).ConfigureAwait(false);
				// Отображение DataGrid после обновления
				await ShowDataGridAsync();
			}
			catch (OperationCanceledException)
			{
				MessageBox.Show("Выполнение было прервано из-за превышения времени выполнения. Попробуйте ещё раз.");
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка: " + ex.Message);
				throw;
			}
		}
		/// <summary>
		/// Метод для скрытия DataGrid.
		/// </summary>
		private void HideDataGridAsync()
		{
			Dispatcher.BeginInvoke(() => AssociatedObject.Visibility = Visibility.Collapsed,
				DispatcherPriority.Normal);
		}
		/// <summary>
		/// Асинхронный метод для отображения DataGrid.
		/// </summary>
		private async Task ShowDataGridAsync()
		{
			await Dispatcher.InvokeAsync(() => AssociatedObject.Visibility = Visibility.Visible,
				DispatcherPriority.Loaded);
		}

		/// <summary>
		/// Метод, который вызывается при отсоединении поведения от DataGrid.
		/// </summary>
		protected override void OnDetaching()
		{
			try
			{
				base.OnDetaching();
				AssociatedObject.AutoGenerateColumns = true;
				AssociatedObject.SourceUpdated -= OnSourceUpdated;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка: " + ex.Message);
				throw;
			}
		}
		/// <summary>
		/// Метод, который вызывается при обновлении источника данных DataGrid.
		/// </summary>
		private void OnSourceUpdated(object? sender, DataTransferEventArgs e)
		{
			try
			{
				Task.Factory.StartNew(() => UpdateItemSource(e.Source), TaskCreationOptions.LongRunning).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка: " + ex.Message);
				throw;
			}
		}
		/// <summary>
		/// Метод для обновления источника данных.
		/// </summary>
		private void UpdateItemSource(object NewItemSource)
		{
			try
			{
				Task.Run(() => RegenerateColumns(NewItemSource as IEnumerable<TimeSheetItemExOrg>)).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка: " + ex.Message);
				throw;
			}
		}
		/// <summary>
		/// Метод для регенерации столбцов DataGrid на основе нового источника данных.
		/// </summary>
		private void RegenerateColumns(IEnumerable<TimeSheetItemExOrg>? items)
		{
			try
			{
				Dispatcher.Invoke(() =>
				{
					using (Dispatcher.DisableProcessing())
					{
						AssociatedObject.Columns.Clear();

						if (items == null || !items.Any()) return;

						AssociatedObject.Columns.AddItems(GenerateColumns(items));
						// Применение стилей
						var previewKeyDownStyle = (Style)Application.Current.FindResource("PreviewKeyDownStyleSO");
						var datagridRowStyle = (Style)Application.Current.FindResource("DataGridRowStyle");
						//var styleDataGridCell = (Style)Application.Current.FindResource("GridCellStyle");
						if (previewKeyDownStyle != null && datagridRowStyle != null)
						{
							AssociatedObject.Style = previewKeyDownStyle;
							AssociatedObject.RowStyle = datagridRowStyle;
							//AssociatedObject.CellStyle = styleDataGridCell;
						}
					}

				}, DispatcherPriority.Send);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				throw;
			}
		}
		/// <summary>
		/// Метод для генерации столбцов DataGrid на основе элементов TimeSheetItemExOrg.
		/// </summary>
		/// <param name="items">Коллекция элементов TimeSheetItemExOrg.</param>
		/// <returns>Коллекция столбцов DataGrid.</returns>
		private static ObservableCollection<DataGridColumn> GenerateColumns(IEnumerable<TimeSheetItemExOrg> items)
		{
			try
			{
				var columns = new ObservableCollection<DataGridColumn>();
				// Получение стилей из ресурсов
				var cellStyle = (Style)Application.Current.FindResource("DataGridCellStyleSO");

				var styleColHed = new Style(typeof(DataGridColumnHeader));
				styleColHed.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(5)));
				styleColHed.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.Bold));
				styleColHed.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty,
					HorizontalAlignment.Center));

				var styleWeekendCell = (Style)Application.Current.FindResource("DataGridCellWeekendStyleSO");
				var styleDayNowCell = (Style)Application.Current.FindResource("DataGridCellDayNowStyleSO");
				var styleCellCalcColl = (Style)Application.Current.FindResource("DataGridCellStyleCalcColl");


				// Статический столбец для идентификаторов
				columns.Add(new DataGridTextColumn
				{
					Header = string.Empty,
					HeaderStyle = styleColHed,
					Binding = new Binding("Id"),
					Foreground = Brushes.Black,
					Width = 20,
					CellStyle = styleCellCalcColl
				});


				// Создание шаблона для столбца с ФИО
				string cellTemplateXamlFio = $@"
					<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
						<Grid>
							<Grid.RowDefinitions>
								<RowDefinition Height='20'/>								
							</Grid.RowDefinitions>

							<TextBlock Grid.Row='0' FontSize = '14' FontWeight = 'Bold' Foreground='{{Binding Brush}}' 
							TextAlignment='Center' Text='{{Binding FioShiftOverday.ShortName}}'/>
						</Grid>
					</DataTemplate>";

				var cellTemplateFio = LoadTemplate(cellTemplateXamlFio);
				columns.Add(new DataGridTemplateColumn
				{
					Header = "ФИО",
					HeaderStyle = styleColHed,
					Width = 200,
					CellTemplate = cellTemplateFio,
					CellStyle = cellStyle
				});

				var item = items.FirstOrDefault();

				if (item == null) return columns;

				DateTime times = item.WorkerHours.Select(x => x.WorkDate).FirstOrDefault();

				int daysInMonth = DateTime.DaysInMonth(times.Year, times.Month);
				int dayNow = 0;
				if (times.Year == DateTime.Now.Year && times.Month == DateTime.Now.Month)
				{
					dayNow = DateTime.Now.Day;
				}
				List<int> itemList = item?.NoWorksDays ?? [];

				for (int i = 0; i < daysInMonth; i++)
				{
					var day = i + 1;
					string cellTemplateXaml = string.Empty;

					if (!item.SeeOrWrite)
						cellTemplateXaml = $@"
						<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
							<Grid>
								<Grid.RowDefinitions>
									<RowDefinition Height='20'/>
								</Grid.RowDefinitions>								
								<TextBlock Grid.Row='0' Text='{{Binding WorkerHours[{i}].Hours, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}}' 
				 x:Name='NameSOTextBlock' Visibility='Visible' Style='{{StaticResource ClickTextBlockStyle}}'
				Foreground='{{Binding WorkerHours[{i}].Brush , Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}}' 
				TextAlignment='Center' FontSize = '14'/>

								<TextBox Grid.Row='0' Text='{{Binding WorkerHours[{i}].Hours, Mode=TwoWay, 
								 UpdateSourceTrigger=PropertyChanged}}' 
				 x:Name='NameSOTextBox' Visibility='Collapsed' Style='{{StaticResource LostFocusTextBoxStyle}}' 
				TextAlignment='Center' FontSize = '14'
				Foreground='{{Binding WorkerHours[{i}].Brush , Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}}' />							
							</Grid>
						</DataTemplate>";
					else
						cellTemplateXaml = $@"
						<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
							<Grid>
								<Grid.RowDefinitions>
									<RowDefinition Height='20'/>
								</Grid.RowDefinitions>								
								<TextBlock Grid.Row='0' Text='{{Binding WorkerHours[{i}].Hours, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}}' 
				 x:Name='Name1TextBlock' Visibility='Visible'	Foreground='Black' TextAlignment='Center' FontSize = '13'/>
						
							</Grid>
						</DataTemplate>";


					var cellTemplate = LoadTemplate(cellTemplateXaml);

					var coll = new DataGridTemplateColumn
					{
						Header = $"{day}",
						HeaderStyle = styleColHed,
						Width = 30,
						CellTemplate = cellTemplate,
					};

					if (itemList.Contains(day))
					{
						coll.CellStyle = styleWeekendCell;
					}
					else
						if (day == dayNow)
						coll.CellStyle = styleDayNowCell;
					else
						coll.CellStyle = cellStyle;

					columns.Add(coll);
				}


				columns.Add(new DataGridTextColumn
				{
					Header = $@"Дней 
				 {item.CalendarWorksDay}",
					HeaderStyle = styleColHed,
					Binding = new Binding("TotalWorksDays"),
					Foreground = Brushes.Black,
					Width = 50,
					CellStyle = styleCellCalcColl
				});


				columns.Add(new DataGridTextColumn
				{
					Header = "Всего",
					HeaderStyle = styleColHed,
					Binding = new Binding("TotalWorksHoursWithOverday"),
					Foreground = Brushes.Black,
					Width = 50,
					CellStyle = styleCellCalcColl
				});


				return columns;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				return [];
				throw;
			}

		}
		/// <summary>
		/// Метод для загрузки шаблона данных.
		/// </summary>
		private static DataTemplate LoadTemplate(string xaml)
		{
			try
			{
				using var reader = XmlReader.Create(new StringReader(xaml));
				return (DataTemplate)XamlReader.Load(reader);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка: " + ex.Message);
				return new DataTemplate();
				throw;
			}
		}
	}
}