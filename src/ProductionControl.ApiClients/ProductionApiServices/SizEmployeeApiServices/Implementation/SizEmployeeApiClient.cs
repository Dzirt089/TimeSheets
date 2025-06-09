using ProductionControl.ApiClients.ProductionApiServices.SizEmployeeApiServices.Interfaces;
using ProductionControl.DataAccess.Classes.EFClasses.Sizs;

namespace ProductionControl.ApiClients.ProductionApiServices.SizEmployeeApiServices.Implementation
{
	public class SizEmployeeApiClient : BaseApiClient, ISizEmployeeApiClient
	{
		public SizEmployeeApiClient(IHttpClientFactory httpClientFactory)
			: base(httpClientFactory.CreateClient("ProductionApi"))
		{
			_httpClient.BaseAddress = new Uri(_httpClient.BaseAddress, "SizEmployee/");
		}

		public Task<List<SizUsageRate>> GetSizUsageRateAsync(CancellationToken token = default) =>
			GetTJsonTAsync<List<SizUsageRate>>("GetSizUsageRate", token);
	}
}
