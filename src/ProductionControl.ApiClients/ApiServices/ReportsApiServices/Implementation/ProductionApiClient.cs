using ProductionControl.ApiClients.ApiServices.ReportsApiServices.Interfaces;
using ProductionControl.DataAccess.Classes.Models.Dtos;

using System.Net.Http.Json;

namespace ProductionControl.ApiClients.ApiServices.ReportsApiServices.Implementation
{
	public class ProductionApiClient : IProductionApiClient
	{
		private readonly HttpClient _httpClient;

		public ProductionApiClient(IHttpClientFactory httpClientFactory)
		{
			_httpClient = httpClientFactory.CreateClient("ProductionApi");
		}

		/// <summary>
		/// Тест
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public async Task GetInit(CancellationToken token = default)
		{
			var response = await _httpClient.GetAsync("/", token);
			var res = response.EnsureSuccessStatusCode();
		}

		/// <summary>
		/// Обрабатываем данные и формируем сводную таблицу за месяц для сотрудников СО в Excel 
		/// </summary>
		public async Task<HttpResponseMessage> CreateReportMonthlySummaryForEmployeeExpOrgsAsync(
			DateTime startPeriod, DateTime endPeriod, CancellationToken token = default)
		{
			var response = await _httpClient.GetAsync(
				$"/CreateReportForMonthlySummaryEmployeeExpOrg/{startPeriod}/{endPeriod}", token);
			return response;
		}

		/// <summary>
		/// Обрабатываем данные и формируем сводную таблицу за  месяц  в Excel 
		/// </summary>
		public async Task<HttpResponseMessage> CreateReportMonthlySummaryAsync(
			int month, int year, CancellationToken token = default)
		{
			var response = await _httpClient.GetAsync(
				$"/CreateReportForMonthlySummary/{month}/{year}", token);
			return response;
		}

		/// <summary>
		/// Формируем и отправляем список обедов, для заказа. Каждое утро.
		/// </summary>
		public async Task<HttpResponseMessage> GetOrderForLunchEveryDayAsync(CancellationToken token = default)
		{
			var response = await _httpClient.GetAsync("/GetOrderForLunch", token);
			return response;
		}

		/// <summary>
		/// Обрабатываем данные и формируем отчёт Excel за прошлый месяц по обедам
		/// </summary>
		/// <param name="totalSum">Сумма по счёту за обеды</param>
		public async Task<HttpResponseMessage> CreateOrderLunchLastMonthAsync(string totalSum, CancellationToken token = default)
		{
			var response = await _httpClient.GetAsync(
				$"/CreateOrderLunchLastMonth/{totalSum}", token);
			return response;
		}

		public async Task<string> GetReportResultSheetsAsync(List<EmployeesInIndicatorDto> indica, CancellationToken token = default)
		{
			var response = await _httpClient.PostAsJsonAsync(
				$"/CreateReportForResultSheet", indica, token);

			var result = await response.Content.ReadAsStringAsync(token);
			return result;
		}
	}
}
