using ProductionControl.DataAccess.Classes.ApiModels.Dtos;

namespace ProductionControl.ServiceLayer.ResultSheetServicesAPI.Interfaces
{
	public interface IResultSheetsService
	{
		/// <summary>
		/// Подготавливаем данными первый показатель "Фактически отработанное время"
		/// </summary>
		/// <param name="TimeSheets">Коллекция информации по табелю на сотрудников производства</param>
		/// <param name="shadowid">Теневой "id" для удобства работы с коллекциями итогов табеля</param>
		Task<IndicatorDto> GetDataForIndicatorOneAsync(
			List<TimeSheetItemDto> TimeSheets,
			int shadowid, CancellationToken token);

		Task<List<EmployeesInIndicatorDto>> ProcessTimeSheetsAD(
			List<TimeSheetItemDto> timeSheets,
			string currentShortValue, CancellationToken token);

		Task<List<EmployeesInIndicatorDto>> ProcessTimeSheetsOverdayOrUnderdayAsync(
			List<TimeSheetItemDto> TimeSheets,
			bool overday, CancellationToken token);

		Task<List<EmployeesInIndicatorDto>> ProcessTimeSheetNightHoursAsync(
			List<TimeSheetItemDto> TimeSheets, CancellationToken token);

		Task<IndicatorDto?> CreateIndicatorAsync(string currentLongValue,
			int shadowid, List<EmployeesInIndicatorDto> employeesIns, CancellationToken token);

		Task<List<EmployeesInIndicatorDto>> ProcessTimeSheets(
			List<TimeSheetItemDto> timeSheets,
			string currentShortValue, CancellationToken token);

		Task<List<EmployeesInIndicatorDto>> ProcessTimeSheetsDismissalAsync(
			List<TimeSheetItemDto> TimeSheets, CancellationToken token);

		/// <summary>
		/// Метод, который рассчитывает данные показателей и списков сотрудников по ним.
		/// </summary>
		/// <param name="TimeSheets">Коллекция информации по табелю на сотрудников производства</param>
		Task<ResultSheetResponseDto> ShowResultSheet(List<TimeSheetItemDto> TimeSheets, CancellationToken token);
	}
}
