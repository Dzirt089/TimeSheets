using ProductionControl.ApiClients.ProductionApiServices.ResultSheetsApiServices.Interfaces;
using ProductionControl.DataAccess.Classes.ApiModels.Dtos;

namespace ProductionControl.ApiClients.ProductionApiServices.ResultSheetsApiServices.Implementation
{
	public class ResultSheetsApiClient : BaseApiClient, IResultSheetsApiClient
	{
		public ResultSheetsApiClient(IHttpClientFactory httpClientFactory) : base(httpClientFactory.CreateClient("ProductionApi"))
		{
			_httpClient.BaseAddress = new Uri(_httpClient.BaseAddress, "ResultSheets/");
		}

		public async Task<ResultSheetResponseDto> GetDataResultSheetAsync(List<TimeSheetItemDto> copyTimeSheet, CancellationToken token = default) =>
			await PostTJsonTAsync<ResultSheetResponseDto>($"/GetDataResultSheet", copyTimeSheet, token);
	}
}
