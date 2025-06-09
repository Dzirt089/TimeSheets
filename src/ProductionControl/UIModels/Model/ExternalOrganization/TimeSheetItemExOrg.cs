using CommunityToolkit.Mvvm.ComponentModel;

using ProductionControl.Models.Dtos.ExternalOrganization;
using ProductionControl.Models.Entitys.EmployeesFactory;

using System.Collections.ObjectModel;
using System.Windows.Media;

namespace ProductionControl.Models.Entitys.ExternalOrganization
{
	/// <summary>
	/// Главный класс для работы с табелем учёта рабочего времени сотрудников ТО
	/// </summary>
	public class TimeSheetItemExOrg : ObservableObject
	{
		/// <summary>
		/// Класс для работы с табелем учёта рабочего времени сотрудников ТО
		/// </summary>
		/// <param name="id"> ID или порядковый номер</param>
		/// <param name="fioShiftOverday">Список ФИО сотрудников ТО</param>
		/// <param name="workerHours">Коллекция модели учёта времени для сотрудника. Где индекс - это день в месяце</param>
		/// <param name="noWorksDay">Список выходных дней</param>
		public TimeSheetItemExOrg(
			int id,
			ShiftDataEmployee fioShiftOverday,
			ObservableCollection<ShiftDataExOrgDto> workerHours,
			List<int> noWorksDay,
			bool seeOrWrite
			)
		{
			Id = id;
			FioShiftOverday = fioShiftOverday;
			WorkerHours = workerHours;
			NoWorksDays = noWorksDay;
			SeeOrWrite = seeOrWrite;
			SetTotalWorksDays();
			SetCalendarDayAndHours();
		}

		public TimeSheetItemExOrg() { }

		public bool SeeOrWrite { get => _seeOrWrite; set => SetProperty(ref _seeOrWrite, value); }
		private bool _seeOrWrite;

		/// <summary>
		/// ID или порядковый номер. 
		/// </summary>
		public int Id { get => _id; set => SetProperty(ref _id, value); }
		private int _id;

		/// <summary>
		/// Цвет для окраски ФИО сотрудника в приложении Табеля. Красный - если уволен в выбранном месяце. Во всех остальных случаях - черный
		/// </summary>
		public Brush Brush { get => _brush; set => SetProperty(ref _brush, value); }
		private Brush _brush;

		/// <summary>
		/// Список ФИО сотрудников ТО
		/// </summary>
		public ShiftDataEmployee FioShiftOverday { get => _fioShiftOverday; set => SetProperty(ref _fioShiftOverday, value); }
		private ShiftDataEmployee _fioShiftOverday;

		/// <summary>
		/// Список выходных дней
		/// </summary>
		public List<int> NoWorksDays
		{
			get => _noWorksDays;
			set => SetProperty(ref _noWorksDays, value);
		}
		private List<int> _noWorksDays;

		/// <summary>
		/// Коллекция модели учёта времени для сотрудника. Где индекс - это день в месяце
		/// </summary>
		public ObservableCollection<ShiftDataExOrgDto> WorkerHours
		{
			get => _workerHours;
			set
			{
				//При каждом обновлении всей коллекции, отписываемся от старых событий. 
				//Чтобы не было утечек памяти и многократного вызова каждого события на всех (старых и новых) данных.
				//Так как не отписавшись, данные не утилизируются сборщиком мусора из-за слабой ссылки события.
				if (_workerHours != null)
				{
					//Событие на отслеживание изменений у самой ObservableCollection (удаление, добавление)
					WorkerHours.CollectionChanged -= WorkerHours_CollectionChanged;
					//foreach (var item in _workerHours)
					//{
					//	//Событие на отслеживание изменений у каждого из свойств класса ShiftDataExOrg. 
					//	item.PropertyChanged -= Item_PropertyChanged;
					//}
					_workerHours.AsParallel().ForAll(item =>
					{
						item.PropertyChanged -= Item_PropertyChanged;
					});
				}

				SetProperty(ref _workerHours, value);

				////При каждом обновлении всей коллекции, подписываем событием новые элементы.
				if (_workerHours != null)
				{
					//Событие на отслеживание изменений у самой ObservableCollection (удаление, добавление)
					WorkerHours.CollectionChanged += WorkerHours_CollectionChanged;
					//foreach (var item in _workerHours)
					//{
					//	//Событие на отслеживание изменений у каждого из свойств класса ShiftDataExOrg. 
					//	item.PropertyChanged += Item_PropertyChanged;
					//}
					_workerHours.AsParallel().ForAll(item =>
					{
						item.PropertyChanged += Item_PropertyChanged;
					});
				}
			}
		}
		private ObservableCollection<ShiftDataExOrgDto> _workerHours;

		/// <summary>
		/// Событие на отслеживание изменений у каждого из свойств класса ShiftDataExOrg
		/// Чтобы реагировать на уровень-два кода выше, чем данные свойств класса ShiftDataExOrg
		/// </summary>
		private void Item_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(ShiftDataExOrgDto.Hours))
			{
				Task.Run(SetTotalWorksDays);
			}
		}

		/// <summary>
		/// Событие для отслеживания изменений у самой ObservableCollection (удаление, добавление).
		/// Для того, чтобы при частичных изменениях, новые данные всегда были подписаны на событие. 
		/// А удаляемые - отписаны (чтобы данные удалились)
		/// </summary>
		private void WorkerHours_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			//подписываем новые данные событием
			if (e.NewItems != null)
			{
				foreach (ShiftDataExOrgDto item in e.NewItems)
				{
					//Событие на отслеживание изменений у каждого из свойств класса ShiftDataExOrg.
					item.PropertyChanged += Item_PropertyChanged;
				}
			}
			//удаляемые данные освобождаем от подписки. Чтобы они были утилизированны GC
			if (e.OldItems != null)
			{
				foreach (ShiftDataExOrgDto item in e.OldItems)
				{
					//Событие на отслеживание изменений у каждого из свойств класса ShiftDataExOrg.
					item.PropertyChanged -= Item_PropertyChanged;
				}
			}
		}

		/// <summary>
		/// Общее кол-во рабочих дней, которые посетил сотрудник
		/// </summary>
		public int TotalWorksDays
		{
			get => _totalWorksDays;
			private set => SetProperty(ref _totalWorksDays, value);
		}
		private int _totalWorksDays;

		/// <summary>
		/// Общее кол-во рабочих часов c учётом переработок\недоработок
		/// </summary>
		public double TotalWorksHoursWithOverday
		{
			get => _totalWorksHoursWithOverday;
			private set => SetProperty(ref _totalWorksHoursWithOverday, value);
		}
		private double _totalWorksHoursWithOverday;

		/// <summary>
		/// Общее кол-во рабочих часов в ночную смену без учёта переработок\недоработок
		/// </summary>
		public double TotalNightHours
		{
			get => _totalNightHours;
			private set => SetProperty(ref _totalNightHours, value);
		}
		private double _totalNightHours;

		/// <summary>
		/// Кол-во календарных рабочих дней
		/// </summary>
		public int CalendarWorksDay
		{
			get => _calendarWorksDay;
			set => SetProperty(ref _calendarWorksDay, value);
		}
		private int _calendarWorksDay;

		/// <summary>
		/// Установка плановых показателей:
		/// Месячная норма рабочих дней и часов по производственному календарю 
		/// </summary>
		private void SetCalendarDayAndHours()
		{
			CalendarWorksDay = WorkerHours.Count - NoWorksDays.Count;
		}

		/// <summary>
		/// Установка общего кол-во рабочих дней, которые посетил сотрудник
		/// </summary>
		private void SetTotalWorksDays()
		{
			// Проверка, что список WorkerHours не пуст и не равен null
			if (WorkerHours.Count == 0 || WorkerHours is null) return;


			// Подсчет общего количества рабочих дней
			TotalWorksDays = WorkerHours
				.AsParallel()
				.Where(x => x.ValidationWorkingDays())
				.Count();

			// Подсчет общего количества рабочих часов без сверхурочных
			var tempTotalWorksHours = WorkerHours
				.AsParallel()
				.Sum(x => x?.GetWorkingDays() ?? 0);
			tempTotalWorksHours = Math.Round(tempTotalWorksHours, 1);

			// Подсчет общего количества рабочих часов с учетом сверхурочных
			TotalWorksHoursWithOverday = tempTotalWorksHours;

		}
	}
}
