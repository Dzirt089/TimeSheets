using ProductionControl.DataAccess.Classes.Models.Dtos;

namespace ProductionControl.ApiClients.ApiServices.ReportsApiServices.Interfaces
{
	public interface IProductionApiClient
	{
		Task GetInit(CancellationToken token = default);

		/// <summary>
		/// Обрабатываем данные и формируем сводную таблицу за месяц для сотрудников СО в Excel 
		/// </summary>
		Task<HttpResponseMessage> CreateReportMonthlySummaryForEmployeeExpOrgsAsync(
			DateTime startPeriod, DateTime endPeriod, CancellationToken token = default);

		/// <summary>
		/// Обрабатываем данные и формируем сводную таблицу за  месяц  в Excel 
		/// </summary>
		Task<HttpResponseMessage> CreateReportMonthlySummaryAsync(
			int month, int year, CancellationToken token = default);

		/// <summary>
		/// Формируем и отправляем список обедов, для заказа. Каждое утро.
		/// </summary>
		Task<HttpResponseMessage> GetOrderForLunchEveryDayAsync(CancellationToken token = default);

		/// <summary>
		/// Обрабатываем данные и формируем отчёт Excel за прошлый месяц по обедам
		/// </summary>
		/// <param name="totalSum">Сумма по счёту за обеды</param>
		Task<HttpResponseMessage> CreateOrderLunchLastMonthAsync(string totalSum, CancellationToken token = default);

		Task<string> GetReportResultSheetsAsync(List<EmployeesInIndicatorDto> indica, CancellationToken token = default);
	}
}
