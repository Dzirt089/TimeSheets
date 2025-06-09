using System.Net.Http.Json;

namespace ProductionControl.ApiClients
{
	public abstract class BaseApiClient
	{
		protected readonly HttpClient _httpClient;

		protected BaseApiClient(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public async Task<T> PostTJsonTAsync<T>(string requestUri, object content, CancellationToken token = default)
		{
			var response = await _httpClient.PostAsJsonAsync(requestUri, content, token);
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadFromJsonAsync<T>(token) ?? throw new InvalidOperationException("Response content is null.");
		}

		public async Task<T> GetTJsonTAsync<T>(string requestUri, CancellationToken token = default)
		{
			var response = await _httpClient.GetAsync(requestUri, token);
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadFromJsonAsync<T>(token) ?? throw new InvalidOperationException("Response content is null.");
		}

		public async Task<T> DeleteTJsonTAsync<T>(string requestUri, CancellationToken token = default)
		{
			var response = await _httpClient.DeleteAsync(requestUri, token);
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadFromJsonAsync<T>(token) ?? throw new InvalidOperationException("Response content is null.");
		}
	}
}
