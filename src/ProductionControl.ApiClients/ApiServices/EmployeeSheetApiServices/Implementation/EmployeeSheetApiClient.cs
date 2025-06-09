using ProductionControl.ApiClients.ApiServices.EmployeeSheetApiServices.Interfaces;

namespace ProductionControl.ApiClients.ApiServices.EmployeeSheetApiServices.Implementation
{
	public class EmployeeSheetApiClient : IEmployeeSheetApiClient
	{
		private readonly HttpClient _httpClient;

		public EmployeeSheetApiClient(IHttpClientFactory httpClientFactory)
		{
			_httpClient = httpClientFactory.CreateClient("ProductionApi");
		}

	}
}
