using ProductionControl.ApiClients.ProductionApiServices.ReportsApiServices.Interfaces;
using ProductionControl.DataAccess.Classes.ApiModels.Dtos;
using ProductionControl.DataAccess.Classes.HttpModels;

namespace ProductionControl.ApiClients.ProductionApiServices.ReportsApiServices.Implementation
{
	public class ReportsApiClient : BaseApiClient, IReportsApiClient
	{
		public ReportsApiClient(IHttpClientFactory httpClientFactory) : base(httpClientFactory.CreateClient("ProductionApi"))
		{
			_httpClient.BaseAddress = new Uri(_httpClient.BaseAddress, "Reports/");
		}

		/// <summary>
		/// Обрабатываем данные и формируем сводную таблицу за месяц для сотрудников СО в Excel 
		/// </summary>
		public async Task<bool> CreateReportMonthlySummaryForEmployeeExpOrgsAsync(StartEndDateTime startEndDate, CancellationToken token = default) =>
			await PostTJsonTAsync<bool>($"/CreateReportForMonthlySummaryEmployeeExpOrg", startEndDate, token);

		/// <summary>
		/// Обрабатываем данные и формируем сводную таблицу за  месяц  в Excel 
		/// </summary>
		public async Task<bool> CreateReportMonthlySummaryAsync(DateTime date, CancellationToken token = default) =>
			await PostTJsonTAsync<bool>($"/CreateReportForMonthlySummary", date, token);

		/// <summary>
		/// Формируем и отправляем список обедов, для заказа. Каждое утро.
		/// </summary>
		public async Task<bool> GetOrderForLunchEveryDayAsync(CancellationToken token = default) =>
			await GetTJsonTAsync<bool>("/GetOrderForLunch", token);

		/// <summary>
		/// Обрабатываем данные и формируем отчёт Excel за прошлый месяц по обедам
		/// </summary>
		/// <param name="totalSum">Сумма по счёту за обеды</param>
		public async Task<bool> CreateOrderLunchLastMonthAsync(string totalSum, CancellationToken token = default) =>
			await PostTJsonTAsync<bool>($"/CreateOrderLunchLastMonth", totalSum, token);

		public async Task<string> GetReportResultSheetsAsync(List<EmployeesInIndicatorDto> indica, CancellationToken token = default) =>
			await PostTJsonTAsync<string>("/CreateReportForResultSheet", indica, token);
	}
}
