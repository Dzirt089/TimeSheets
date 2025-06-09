using ProductionControl.DataAccess.Classes.Models.Dtos;

namespace ProductionControl.ServiceLayer.ServicesAPI.Interfaces
{
	public interface IReportService
	{
		/// <summary>
		/// Формируем Excel отчёт по переданным данным
		/// </summary>
		/// <param name="dataForReports">Список с данными</param>
		/// <param name="date">Прошлая дата</param>
		/// <returns>строка пути, где храниться отчёт</returns>
		Task<string> CreateReportExcelForLunchLastMonhtAsync(
			List<DataForReportLunchLastMonthDto> dataForReports,
			DateTime date, CancellationToken token);

		Task<string> CreateReportForMonthlySummaryEmployeeExpOrgAsync(
			List<EmployeesExOrgForReportDto> summaries, DateTime startDate, DateTime endDate, CancellationToken token);

		Task<string> CreateReportForMonthlySummaryAsync(
			List<MonthlySummaryDto> summaries, CancellationToken token);

		/// <summary>
		/// Формируем и отправляем список обедов, для заказа. Каждое утро.
		/// </summary>
		Task ProcessingDataReportForLUnchEveryDayAsync(CancellationToken token);

		/// <summary>
		/// Обрабатываем данные и формируем отчёт Excel за прошлый месяц по обедам
		/// </summary>
		/// <param name="totalSum">Сумма по счёту за обеды</param>
		Task ProcessingDataForReportLunchLastMonth(string totalSum, CancellationToken token);

		/// <summary>
		/// Получаем путь для сохранения файлов 
		/// </summary>
		/// <param name="fileName">Имя файла</param>
		/// <returns>полный путь</returns>
		Task<string> GetPathDiskServerAsync(string fileName, CancellationToken token);

		/// <summary>
		/// Получаем путь для сохранения расчетных файлов
		/// </summary>
		/// <param name="fileName">Имя файла</param>
		/// <returns>полный путь</returns>
		Task<string> GetPathDiskMAsync(string fileName, CancellationToken token);

		/// <summary>
		/// Получаем путь для сохранения расчетных файлов
		/// </summary>
		/// <param name="fileName">Имя файла</param>
		/// <returns>полный путь</returns>
		Task<string> GetPathDiskMSizAsync(string fileName, CancellationToken token);

		/// <summary>
		/// Excel отчёт по выбранному итогу табеля
		/// </summary>
		/// <param name="indicators"></param>
		/// <returns></returns>
		Task<string> CreateReportForResultSheetAsync(
			List<EmployeesInIndicatorDto> indicators, CancellationToken token);

		/// <summary>
		/// Excel отчёт для Ведомости
		/// </summary>
		/// <param name="indicators"></param>
		/// <returns></returns>   
		Task<string> CreateReportForSIZAsync(
			List<EmployeeForSizDto> forSizs, CancellationToken token);
	}
}
