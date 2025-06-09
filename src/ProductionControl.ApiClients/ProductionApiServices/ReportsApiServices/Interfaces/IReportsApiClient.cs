using ProductionControl.DataAccess.Classes.ApiModels.Dtos;
using ProductionControl.DataAccess.Classes.HttpModels;

namespace ProductionControl.ApiClients.ProductionApiServices.ReportsApiServices.Interfaces
{
	public interface IReportsApiClient
	{
		/// <summary>
		/// Обрабатываем данные и формируем сводную таблицу за месяц для сотрудников СО в Excel 
		/// </summary>
		Task<bool> CreateReportMonthlySummaryForEmployeeExpOrgsAsync(StartEndDateTime startEndDate, CancellationToken token = default);

		/// <summary>
		/// Обрабатываем данные и формируем сводную таблицу за  месяц  в Excel 
		/// </summary>
		Task<bool> CreateReportMonthlySummaryAsync(DateTime date, CancellationToken token = default);

		/// <summary>
		/// Формируем и отправляем список обедов, для заказа. Каждое утро.
		/// </summary>
		Task<bool> GetOrderForLunchEveryDayAsync(CancellationToken token = default);

		/// <summary>
		/// Обрабатываем данные и формируем отчёт Excel за прошлый месяц по обедам
		/// </summary>
		/// <param name="totalSum">Сумма по счёту за обеды</param>
		Task<bool> CreateOrderLunchLastMonthAsync(string totalSum, CancellationToken token = default);

		Task<string> GetReportResultSheetsAsync(List<EmployeesInIndicatorDto> indica, CancellationToken token = default);
	}
}
