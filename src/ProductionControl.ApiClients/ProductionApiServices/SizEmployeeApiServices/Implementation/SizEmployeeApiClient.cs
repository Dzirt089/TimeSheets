using ProductionControl.ApiClients.ProductionApiServices.SizEmployeeApiServices.Interfaces;
using ProductionControl.DataAccess.Classes.EFClasses.Sizs;

using System.Text.Json;

namespace ProductionControl.ApiClients.ProductionApiServices.SizEmployeeApiServices.Implementation
{
	public class SizEmployeeApiClient : BaseApiClient, ISizEmployeeApiClient
	{
		public SizEmployeeApiClient(IHttpClientFactory httpClientFactory, JsonSerializerOptions jsonOptions)
			: base(httpClientFactory.CreateClient("ProductionApi"), jsonOptions)
		{
			_httpClient.BaseAddress = new Uri(_httpClient.BaseAddress, "SizEmployee/");
		}

		public Task<List<SizUsageRate>> GetSizUsageRateAsync(CancellationToken token = default) =>
			GetTJsonTAsync<List<SizUsageRate>>("GetSizUsageRate", token);
	}
}
