using CommunityToolkit.Mvvm.ComponentModel;

using ProductionControl.Models.ExternalOrganization;
using ProductionControl.Utils;

using System.Collections.ObjectModel;
using System.Windows.Media;

namespace ProductionControl.Entitys.ExternalOrganization
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
			ObservableCollection<ShiftDataExOrg> workerHours,
			List<int> noWorksDay
			)
		{
			Id = id;
			FioShiftOverday = fioShiftOverday;
			WorkerHours = workerHours;
			NoWorksDays = noWorksDay;
			//SetTotalWorksDays();
			SetCalendarDayAndHours();
		}
		public TimeSheetItemExOrg() { }

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
		public ObservableCollection<ShiftDataExOrg> WorkerHours
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
		private ObservableCollection<ShiftDataExOrg> _workerHours;

		/// <summary>
		/// Событие на отслеживание изменений у каждого из свойств класса ShiftDataExOrg
		/// Чтобы реагировать на уровень-два кода выше, чем данные свойств класса ShiftDataExOrg
		/// </summary>
		private void Item_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(ShiftDataExOrg.Hours))
			{
				//Task.Run(SetTotalWorksDays);
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
				foreach (ShiftDataExOrg item in e.NewItems)
				{
					//Событие на отслеживание изменений у каждого из свойств класса ShiftDataExOrg.
					item.PropertyChanged += Item_PropertyChanged;
				}
			}
			//удаляемые данные освобождаем от подписки. Чтобы они были утилизированны GC
			if (e.OldItems != null)
			{
				foreach (ShiftDataExOrg item in e.OldItems)
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
		/// Общее кол-во рабочих часов, без учёта переработок\недоработок
		/// </summary>
		public double TotalWorksHoursWithoutOverday
		{
			get => _totalWorksHoursWithoutOverday;
			private set => SetProperty(ref _totalWorksHoursWithoutOverday, value);
		}
		private double _totalWorksHoursWithoutOverday;

		/// <summary>
		/// Общее кол-во рабочих часов c учётом переработок\недоработок
		/// </summary>
		public double TotalWorksHoursWithOverday
		{
			get => _totalWorksHoursWithOverday;
			private set => SetProperty(ref _totalWorksHoursWithOverday, value);
		}
		public int TotalLunch { get => _totalLunch; private set => SetProperty(ref _totalLunch, value); }
		private int _totalLunch;

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
		/// Общее кол-во рабочих часов в дневную смену без учёта переработок\недоработок
		/// </summary>
		public double TotalDaysHours
		{
			get => _totalDaysHours;
			private set => SetProperty(ref _totalDaysHours, value);
		}
		private double _totalDaysHours;

		/// <summary>
		/// Общее кол-во часов переработок\недоработок
		/// </summary>
		public double TotalOverdayHours
		{
			get => _totalOverdayHours;
			private set
			{
				SetProperty(ref _totalOverdayHours, value);
			}
		}
		private double _totalOverdayHours;

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
		/// Кол-во календарных рабочих часов за месяц
		/// </summary>
		public int CalendarWorksHours
		{
			get => _calendarWorksHours;
			set => SetProperty(ref _calendarWorksHours, value);
		}
		private int _calendarWorksHours;

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
		//private void SetTotalWorksDays()
		//{
		//	// Проверка, что список WorkerHours не пуст и не равен null
		//	if (WorkerHours.Count == 0 || WorkerHours is null) return;


		//	bool daysShift = false;
		//	bool nightShift = false;

		//	var shift = WorkerHours.Where(x => x.IsPreHoliday).Select(s => s.Shift).FirstOrDefault();
		//	if (int.TryParse(shift, out int numberShift))
		//	{
		//		switch (numberShift)
		//		{
		//			case 1: daysShift = true; break;
		//			case 2: nightShift = true; break;
		//			case 3: nightShift = true; break;
		//			case 4: daysShift = true; break;
		//			case 5: daysShift = true; break;
		//			case 7: daysShift = true; break;
		//		}
		//	}

		//	// Подсчет общего количества рабочих дней
		//	TotalWorksDays = WorkerHours
		//		.AsParallel()
		//		.Where(x => x.ValidationWorkingDays())
		//		.Count();

		//	// Подсчет общего количества сверхурочных часов
		//	var tempTotalOverHours = WorkerHours
		//		.AsParallel()
		//		.Where(e => e.ValidationOverdayDays())
		//		.Sum(r => double.TryParse(r.Overday?.Replace(".", ","), out double tempValue) ?
		//		tempValue : 0);
		//	TotalOverdayHours = Math.Round(tempTotalOverHours, 1);

		//	// Подсчет общего количества дневных рабочих часов
		//	var tempTotalDaysHours = WorkerHours
		//		.AsParallel()
		//		.Where(x => x.ValidationWorkingDays())
		//		.Sum(x => x.Shift?.GetDaysHours() ?? 0);

		//	var tempCheckTDH = Math.Round(tempTotalDaysHours, 1);

		//	if (daysShift)
		//		tempCheckTDH -= CountPreholiday;

		//	TotalDaysHours = tempCheckTDH < 0 ? 0 : tempCheckTDH;

		//	// Подсчет общего количества ночных рабочих часов
		//	var tempTotalNightHours = WorkerHours
		//		.AsParallel()
		//		.Where(x => x.ValidationWorkingDays())
		//		.Sum(y => y.Shift?.GetNightHours() ?? 0);

		//	if (nightShift)
		//		tempTotalNightHours -= CountPreholiday;

		//	TotalNightHours = Math.Round(tempTotalNightHours, 1);

		//	// Подсчет общего количества рабочих часов без сверхурочных
		//	var tempTotalWorksHoursWithoutOverday = WorkerHours
		//		.AsParallel()
		//		.Sum(x => x.Shift?.GetShiftHours() ?? 0);
		//	var tempCheckTWHWO = Math.Round(tempTotalWorksHoursWithoutOverday, 1) - CountPreholiday;

		//	TotalWorksHoursWithoutOverday = tempCheckTWHWO < 0 ? 0 : tempCheckTWHWO;

		//	// Подсчет общего количества рабочих часов с учетом сверхурочных
		//	TotalWorksHoursWithOverday = Math.Round(TotalWorksHoursWithoutOverday + TotalOverdayHours, 1);

		//	// Подсчет количества обедов
		//	TotalLunch = WorkerHours.AsParallel().Where(e => e.IsHaveLunch == true).Count();
		//}

		public TimeSheetItemExOrg Clone()
		{
			return new TimeSheetItemExOrg
			{
				Brush = Brush,
				CalendarWorksDay = CalendarWorksDay,
				CalendarWorksHours = CalendarWorksHours,
				FioShiftOverday = FioShiftOverday,
				Id = Id,
				NoWorksDays = NoWorksDays,
				TotalDaysHours = TotalDaysHours,
				TotalLunch = TotalLunch,
				TotalNightHours = TotalNightHours,
				TotalOverdayHours = TotalOverdayHours,
				TotalWorksDays = TotalWorksDays,
				TotalWorksHoursWithoutOverday = TotalWorksHoursWithoutOverday,
				TotalWorksHoursWithOverday = TotalWorksHoursWithOverday,
				WorkerHours = WorkerHours,
			};
		}
	}
}
