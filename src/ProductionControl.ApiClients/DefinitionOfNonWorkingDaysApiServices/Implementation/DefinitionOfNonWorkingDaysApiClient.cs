using ProductionControl.ApiClients.DefinitionOfNonWorkingDaysApiServices.Interfaces;

using System.Text.Json;

namespace ProductionControl.ApiClients.DefinitionOfNonWorkingDaysApiServices.Implementation
{
	public class DefinitionOfNonWorkingDaysApiClient : BaseApiClient, IDefinitionOfNonWorkingDaysApiClient
	{
		public DefinitionOfNonWorkingDaysApiClient(IHttpClientFactory httpClientFactory, JsonSerializerOptions jsonOptions) :
			base(httpClientFactory.CreateClient("VKTApi"), jsonOptions)
		{
		}

		public async Task<bool> GetWeekendDayAsync(DateTime date, CancellationToken token = default) =>
			await GetTJsonTAsync<bool>($"/Days/IsDayOff/{date}", token);

		public async Task<List<int>> GetWeekendsInMonthAsync(int year, int month, CancellationToken token = default) =>
			await GetTJsonTAsync<List<int>>($"/Days/WeekendsInMonth/{year}/{month}", token);
	}
}
