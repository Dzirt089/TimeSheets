using Newtonsoft.Json;

using ProductionControl.Services.API.Interfaces;
using ProductionControl.Services.Interfaces;

using System.Net.Http;

namespace ProductionControl.Services.API
{
	public class DefinitionOfNonWorkingDays(
		HttpClientForProject httpClient,
		IErrorLogger logger)
		: IDefinitionOfNonWorkingDays
	{

		private HttpClient _client = httpClient.GetHttpClient();
		public static string Url => Settings.Default.VKT_API;

		/// <summary> Выполняет проверку, является ли указанный день выходным </summary>
		public async Task<bool> GetWeekendDayAsync(DateTime date)
		{
			try
			{
				var response = await _client.GetStringAsync($"{Url}/Days/IsDayOff/{date}");
				return JsonConvert.DeserializeObject<bool>(response);
			}
			catch (Exception ex)
			{
				await logger.ProcessingErrorLogAsync(ex);
				return false;
				throw;
			}
		}

		/// <summary> Получаем список вых. дней в месяце</summary>
		public async Task<List<int>> GetWeekendsInMonthAsync(int year, int month)
		{
			try
			{
				var response = await _client.GetStringAsync($"{Url}/Days/WeekendsInMonth/{year}/{month}");
				return JsonConvert.DeserializeObject<List<int>>(response);
			}
			catch (Exception ex)
			{
				await logger.ProcessingErrorLogAsync(ex);
				return new List<int>();
				throw;
			}
		}
	}
}
