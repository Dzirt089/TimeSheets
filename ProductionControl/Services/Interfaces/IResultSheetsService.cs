using ProductionControl.Entitys;
using ProductionControl.Entitys.ResultTimeSheet;

using System.Collections.ObjectModel;

namespace ProductionControl.Services.Interfaces
{
	public interface IResultSheetsService
	{
		/// <summary>
		/// Подготавливаем данными первый показатель "Фактически отработанное время"
		/// </summary>
		/// <param name="TimeSheets">Коллекция информации по табелю на сотрудников производства</param>
		/// <param name="userDataCurrent">Данные с именами сотрудника и его компьютера</param>
		/// <param name="shadowid">Теневой "id" для удобства работы с коллекциями итогов табеля</param>
		Task<Indicator> GetDataForIndicatorOneAsync(
			ObservableCollection<TimeSheetItem> TimeSheets,
			LocalUserData userDataCurrent,
			int shadowid);

		Task<List<EmployeesInIndicator>> ProcessTimeSheetsOverdayOrUnderdayAsync(
	ObservableCollection<TimeSheetItem> TimeSheets,
	LocalUserData userDataCurrent,
	bool overday);

		Task<List<EmployeesInIndicator>> ProcessTimeSheetNightHoursAsync(
			ObservableCollection<TimeSheetItem> TimeSheets,
			LocalUserData userDataCurrent);

		Task<Indicator?> CreateIndicatorAsync(string currentLongValue,
			int shadowid, List<EmployeesInIndicator> employeesIns, LocalUserData userDataCurrent);

		Task<List<EmployeesInIndicator>> ProcessTimeSheets(
			ObservableCollection<TimeSheetItem> timeSheets,
			string currentShortValue,
			LocalUserData userDataCurrent);



		Task<List<EmployeesInIndicator>> ProcessTimeSheetsDismissalAsync(
			ObservableCollection<TimeSheetItem> TimeSheets,
			LocalUserData userDataCurrent);



		/// <summary>
		/// Метод, который рассчитывает данные показателей и списков сотрудников по ним.
		/// </summary>
		/// <param name="TimeSheets">Коллекция информации по табелю на сотрудников производства</param>
		/// <param name="userDataCurrent">Данные с именами сотрудника и его компьютера</param>
		Task<(
			ObservableCollection<Indicator> Indicators,
			List<EmployeesInIndicator> NNList,
			List<EmployeesInIndicator> Underday,
			List<EmployeesInIndicator> Overday,
			List<EmployeesInIndicator> Night,
			List<EmployeesInIndicator> Vacation,
			List<EmployeesInIndicator> ADVacation,
			List<EmployeesInIndicator> SickLeave,
			List<EmployeesInIndicator> Demobilized,
			List<EmployeesInIndicator> ParentalLeave,
			List<EmployeesInIndicator> InvalidLeave,
			List<EmployeesInIndicator> Dismissal,
			List<EmployeesInIndicator> Lunching)>
			ShowResultSheet(ObservableCollection<TimeSheetItem> TimeSheets,
			LocalUserData userDataCurrent);
	}
}
