using ProductionControl.ApiClients.ApiServices.ResultSheetsApiServices.Interfaces;
using ProductionControl.DataAccess.Classes.Models.Dtos;

using System.Net.Http.Json;

namespace ProductionControl.ApiClients.ApiServices.ResultSheetsApiServices.Implementation
{
	public class ResultSheetsApiClient : IResultSheetsApiClient
	{
		private readonly HttpClient _httpClient;
		public ResultSheetsApiClient(IHttpClientFactory httpClientFactory)
		{
			_httpClient = httpClientFactory.CreateClient("ProductionApi");
		}

		public async Task<ResultSheetResponseDto> GetDataResultSheetAsync(List<TimeSheetItemDto> copyTimeSheet, CancellationToken token = default)
		{
			var response = await _httpClient.PostAsJsonAsync(
				$"/GetDataResultSheet", copyTimeSheet, token);

			var result = await response.Content.ReadFromJsonAsync<ResultSheetResponseDto>(token);

			return result;
		}
	}
}
