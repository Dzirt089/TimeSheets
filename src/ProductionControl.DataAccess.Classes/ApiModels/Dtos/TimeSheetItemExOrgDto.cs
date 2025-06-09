using ProductionControl.DataAccess.Classes.EFClasses.EmployeesExternalOrganizations;

using System.Collections.ObjectModel;

namespace ProductionControl.DataAccess.Classes.Models.Dtos
{
	/// <summary>
	/// Главный класс для работы с табелем учёта рабочего времени сотрудников ТО
	/// </summary>
	public class TimeSheetItemExOrgDto
	{
		/// <summary>
		/// Класс для работы с табелем учёта рабочего времени сотрудников ТО
		/// </summary>
		/// <param name="id"> ID или порядковый номер</param>
		/// <param name="fioShiftOverday">Список ФИО сотрудников ТО</param>
		/// <param name="workerHours">Коллекция модели учёта времени для сотрудника. Где индекс - это день в месяце</param>
		/// <param name="noWorksDay">Список выходных дней</param>
		public TimeSheetItemExOrgDto(
			int id,
			ShiftDataEmployeeDto fioShiftOverday,
			ObservableCollection<ShiftDataExOrg> workerHours,
			List<int> noWorksDay,
			bool seeOrWrite
			)
		{
			Id = id;
			FioShiftOverday = fioShiftOverday;
			WorkerHours = workerHours;
			NoWorksDays = noWorksDay;
			SeeOrWrite = seeOrWrite;
		}

		public TimeSheetItemExOrgDto() { }

		public bool SeeOrWrite { get; set; }

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
		public ObservableCollection<ShiftDataExOrg> WorkerHours { get; set; }

		/// <summary>
		/// Общее кол-во рабочих дней, которые посетил сотрудник
		/// </summary>
		public int TotalWorksDays { get; set; }

		/// <summary>
		/// Общее кол-во рабочих часов c учётом переработок\недоработок
		/// </summary>
		public double TotalWorksHoursWithOverday { get; set; }

		/// <summary>
		/// Общее кол-во рабочих часов в ночную смену без учёта переработок\недоработок
		/// </summary>
		public double TotalNightHours { get; set; }

		/// <summary>
		/// Кол-во календарных рабочих дней
		/// </summary>
		public int CalendarWorksDay { get; set; }
	}
}
