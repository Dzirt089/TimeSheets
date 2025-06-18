using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;
using ProductionControl.DataAccess.Classes.Utils;

using System.Collections.ObjectModel;

namespace ProductionControl.DataAccess.Classes.ApiModels.Dtos
{
	/// <summary>
	/// Главный класс для работы с табелем учёта рабочего времени сотрудников ТО
	/// </summary>
	public class TimeSheetItemDto
	{
		/// <summary>
		/// Класс для работы с табелем учёта рабочего времени сотрудников ТО
		/// </summary>
		/// <param name="id"> ID или порядковый номер</param>
		/// <param name="fioShiftOverday">Список ФИО сотрудников ТО</param>
		/// <param name="workerHours">Коллекция модели учёта времени для сотрудника. Где индекс - это день в месяце</param>
		/// <param name="noWorksDay">Список выходных дней</param>
		public TimeSheetItemDto(
			int id,
			ShiftDataEmployeeDto fioShiftOverday,
			ObservableCollection<ShiftData> workerHours,
			List<int> noWorksDay,
			bool accessSeeOrWrite,
			bool lunch
			)
		{
			Id = id;
			FioShiftOverday = fioShiftOverday;
			WorkerHours = workerHours;
			NoWorksDays = noWorksDay;
			AccessSeeOrWrite = accessSeeOrWrite;
			IsLunch = lunch;
			SetTotalWorksDays();
			SetCalendarDayAndHours();
		}

		public TimeSheetItemDto() { }

		/// <summary>
		/// Сво-во отражает в каком варианте грузить шаблоны для табеля. 
		/// Если <see cref="true"/> - то право на редактирование открыто.
		/// Если <see cref="false"/> - то право на просмотр открыто.
		/// </summary>
		public bool AccessSeeOrWrite { get; set; }

		/// <summary>Флаг, который обозначает что сотрудник обедает на производстве или нет. Для того, чтобы заказывать на него обед\ужин или нет</summary>
		public bool IsLunch { get; set; }

		/// <summary>
		/// ID или порядковый номер. 
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Список ФИО сотрудников ТО
		/// </summary>
		public ShiftDataEmployeeDto FioShiftOverday { get; set; }

		/// <summary>
		/// Список выходных дней
		/// </summary>
		public List<int> NoWorksDays { get; set; }

		/// <summary>
		/// Коллекция модели учёта времени для сотрудника. Где индекс - это день в месяце
		/// </summary>
		public ObservableCollection<ShiftData> WorkerHours { get; set; }

		/// <summary>
		/// Общее кол-во рабочих дней, которые посетил сотрудник
		/// </summary>
		public int TotalWorksDays { get; set; }

		/// <summary>
		/// Общее кол-во рабочих часов, без учёта переработок\недоработок
		/// </summary>
		public double TotalWorksHoursWithoutOverday { get; set; }

		/// <summary>
		/// Общее кол-во рабочих часов c учётом переработок\недоработок
		/// </summary>
		public double TotalWorksHoursWithOverday { get; set; }

		public int TotalLunch { get; set; }

		/// <summary>
		/// Общее кол-во рабочих часов в ночную смену без учёта переработок\недоработок
		/// </summary>
		public double TotalNightHours { get; set; }

		/// <summary>
		/// Общее кол-во рабочих часов в дневную смену без учёта переработок\недоработок
		/// </summary>
		public double TotalDaysHours { get; set; }

		/// <summary>
		/// Общее кол-во часов переработок\недоработок
		/// </summary>
		public double TotalOverdayHours { get; set; }

		/// <summary>
		/// Кол-во календарных рабочих дней
		/// </summary>
		public int CalendarWorksDay { get; set; }
		public int CountPreholiday { get; set; }

		/// <summary>
		/// Кол-во календарных рабочих часов за месяц
		/// </summary>
		public int CalendarWorksHours { get; set; }

		/// <summary>
		/// Установка плановых показателей:
		/// Месячная норма рабочих дней и часов по производственному календарю 
		/// </summary>
		public void SetCalendarDayAndHours()
		{
			CalendarWorksDay = WorkerHours.Count - NoWorksDays.Count;
			CountPreholiday = WorkerHours.Where(x => x.IsPreHoliday == true).Count();
			CalendarWorksHours = CalendarWorksDay * 8 - CountPreholiday;
		}

		/// <summary>
		/// Установка общего кол-во рабочих дней, которые посетил сотрудник
		/// </summary>
		public void SetTotalWorksDays()
		{
			// Проверка, что список WorkerHours не пуст и не равен null
			if (WorkerHours.Count == 0 || WorkerHours is null) return;

			// Подсчет количества предпраздничных дней
			CountPreholiday = WorkerHours.Where(x => x.IsPreHoliday == true && x.Shift != null && x.Shift.GetShiftHours() != 0).Count();

			bool daysShift = false;
			bool nightShift = false;

			var shift = WorkerHours.Where(x => x.IsPreHoliday).Select(s => s.Shift).FirstOrDefault();
			if (int.TryParse(shift, out int numberShift))
			{
				switch (numberShift)
				{
					case 1: daysShift = true; break;
					case 2: nightShift = true; break;
					case 3: nightShift = true; break;
					case 4: daysShift = true; break;
					case 5: daysShift = true; break;
					case 7: daysShift = true; break;
				}
			}

			// Подсчет общего количества рабочих дней
			TotalWorksDays = WorkerHours
				.AsParallel()
				.Where(x => x.ValidationWorkingDays())
				.Count();

			// Подсчет общего количества сверхурочных часов
			var tempTotalOverHours = WorkerHours
				.AsParallel()
				.Where(e => e.ValidationOverdayDays())
				.Sum(r => double.TryParse(r.Overday?.Replace(".", ","), out double tempValue) ?
				tempValue : 0);
			TotalOverdayHours = Math.Round(tempTotalOverHours, 1);

			// Подсчет общего количества дневных рабочих часов
			var tempTotalDaysHours = WorkerHours
				.AsParallel()
				.Where(x => x.ValidationWorkingDays())
				.Sum(x => x.Shift?.GetDaysHours() ?? 0);

			var tempCheckTDH = Math.Round(tempTotalDaysHours, 1);

			if (daysShift)
				tempCheckTDH -= CountPreholiday;

			TotalDaysHours = tempCheckTDH < 0 ? 0 : tempCheckTDH;

			// Подсчет общего количества ночных рабочих часов
			var tempTotalNightHours = WorkerHours
				.AsParallel()
				.Where(x => x.ValidationWorkingDays())
				.Sum(y => y.Shift?.GetNightHours() ?? 0);

			if (nightShift)
				tempTotalNightHours -= CountPreholiday;

			TotalNightHours = Math.Round(tempTotalNightHours, 1);

			// Подсчет общего количества рабочих часов без сверхурочных
			var tempTotalWorksHoursWithoutOverday = WorkerHours
				.AsParallel()
				.Sum(x => x.Shift?.GetShiftHours() ?? 0);
			var tempCheckTWHWO = Math.Round(tempTotalWorksHoursWithoutOverday, 1) - CountPreholiday;

			TotalWorksHoursWithoutOverday = tempCheckTWHWO < 0 ? 0 : tempCheckTWHWO;

			// Подсчет общего количества рабочих часов с учетом сверхурочных
			TotalWorksHoursWithOverday = Math.Round(TotalWorksHoursWithoutOverday + TotalOverdayHours, 1);

			// Подсчет количества обедов
			TotalLunch = WorkerHours.AsParallel().Where(e => e.IsHaveLunch == true).Count();
		}
	}
}
