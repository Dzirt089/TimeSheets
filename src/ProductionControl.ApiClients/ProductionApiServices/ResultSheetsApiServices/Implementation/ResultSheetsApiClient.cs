using ProductionControl.ApiClients.ProductionApiServices.ResultSheetsApiServices.Interfaces;
using ProductionControl.DataAccess.Classes.ApiModels.Dtos;
using ProductionControl.DataAccess.Classes.HttpModels;

using System.Text.Json;

namespace ProductionControl.ApiClients.ProductionApiServices.ResultSheetsApiServices.Implementation
{
	public class ResultSheetsApiClient : BaseApiClient, IResultSheetsApiClient
	{
		public ResultSheetsApiClient(IHttpClientFactory httpClientFactory, JsonSerializerOptions jsonOptions) : base(httpClientFactory.CreateClient("ProductionApi"), jsonOptions)
		{
			_httpClient.BaseAddress = new Uri(_httpClient.BaseAddress, "ResultSheets/");
		}

		public async Task<ResultSheetResponseDto> GetDataResultSheetAsync(DataForTimeSheet dataForTimeSheet, CancellationToken token = default) =>
			await PostTJsonTAsync<ResultSheetResponseDto>($"GetDataResultSheet", dataForTimeSheet, token);
	}
}
