using ProductionControl.DataAccess.Classes.EFClasses.EmployeesFactorys;

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
	}
}
