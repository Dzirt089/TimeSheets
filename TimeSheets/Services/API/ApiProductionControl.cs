﻿using System.Net.Http;
using System.Net.Http.Json;

using TimeSheets.Entitys;
using TimeSheets.Entitys.ResultTimeSheet;
using TimeSheets.Services.API.Interfaces;
using TimeSheets.Services.Interfaces;

namespace TimeSheets.Services.API
{
	//TODO: Доделать API-шку под программу. Пока она не доступна в гите
	public class ApiProductionControl(
		HttpClientForProject httpClient,
		IErrorLogger errorLogger) : IApiProductionControl
	{
		private HttpClient _client = httpClient.GetHttpClient();

		/// <summary>
		/// Формируем и отправляем список обедов, для заказа. Каждое утро.
		/// </summary>
		public async Task<HttpResponseMessage> GetOrderForLunchEveryDayAsync(LocalUserData user)
		{
			try
			{
				return await _client.GetAsync("http://localhost:5044/GetOrderForLunch");
			}
			catch (Exception ex)
			{
				await errorLogger.ProcessingErrorLogAsync(ex, user: user.UserName, machine: user.MachineName).ConfigureAwait(false);
				return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// Обрабатываем данные и формируем отчёт Excel за прошлый месяц по обедам
		/// </summary>
		/// <param name="totalSum">Сумма по счёту за обеды</param>
		public async Task<HttpResponseMessage> CreateOrderLunchLastMonthAsync(
			string totalSum, LocalUserData user)
		{
			try
			{
				return await _client.GetAsync($"http://localhost:5044/CreateOrderLunchLastMonth/{totalSum}");
			}
			catch (Exception ex)
			{
				await errorLogger.ProcessingErrorLogAsync(ex, user: user.UserName, machine: user.MachineName).ConfigureAwait(false);
				return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
			}

		}

		/// <summary>
		/// Сохраняем в выбранном месте на компьютере Excel отчёт по выбранному показателю в итогах табеля
		/// </summary>
		/// <param name="indica">Список данных людей по выбранной категории</param>
		/// <returns></returns>
		public async Task<string> GetReportResultSheetsAsync(
			List<EmployeesInIndicator> indica, LocalUserData user)
		{
			try
			{
				var response = await _client.PostAsJsonAsync(
					$"http://localhost:5044/CreateReportForResultSheet", indica);

				var result = await response.Content.ReadAsStringAsync();
				return result;
			}
			catch (Exception ex)
			{
				await errorLogger.ProcessingErrorLogAsync(ex, user: user.UserName, machine: user.MachineName).ConfigureAwait(false);
				return string.Empty;
			}
		}
	}
}
