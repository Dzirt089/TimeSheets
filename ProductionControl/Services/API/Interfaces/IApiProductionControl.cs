using ProductionControl.Entitys;
using ProductionControl.Entitys.ResultTimeSheet;

using System.Net.Http;

namespace ProductionControl.Services.API.Interfaces
{
	public interface IApiProductionControl
	{
		/// <summary>
		/// Формируем и отправляем список обедов, для заказа. Каждое утро.
		/// </summary>
		Task<HttpResponseMessage>
			GetOrderForLunchEveryDayAsync(LocalUserData user);

		/// <summary>
		/// Обрабатываем данные и формируем отчёт Excel за прошлый месяц по обедам
		/// </summary>
		/// <param name="totalSum">Сумма по счёту за обеды</param>
		Task<HttpResponseMessage> CreateOrderLunchLastMonthAsync(
			string totalSum, LocalUserData user);

		Task<string> GetReportResultSheetsAsync(
			List<EmployeesInIndicator> indica, LocalUserData user);
	}
}
