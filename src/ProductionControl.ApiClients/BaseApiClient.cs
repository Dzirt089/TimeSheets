using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ProductionControl.ApiClients
{
	public abstract class BaseApiClient
	{
		protected readonly HttpClient _httpClient;
		private readonly JsonSerializerOptions _jsonOptions;

		protected BaseApiClient(HttpClient httpClient, JsonSerializerOptions jsonOptions)
		{
			_httpClient = httpClient;
			_jsonOptions = jsonOptions;
		}

		public T PostTJsonT<T>(string requestUri, object content)
		{
			// 1. Собираем запрос
			var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
			{
				Content = JsonContent.Create(content)  // пакетно создаёт StringContent + заголовки
			};

			// 2. Отправляем синхронно
			using var response = _httpClient.Send(request);

			// 3. Проверяем статус
			response.EnsureSuccessStatusCode();

			// 4. Получаем поток с телом
			using var stream = response.Content.ReadAsStream();

			// 5. Десериализуем синхронно через System.Text.Json
			var options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
				// ... любые ваши настройки
			};
			var result = JsonSerializer.Deserialize<T>(stream, options);

			if (result is null)
				throw new InvalidOperationException("Response content is null.");

			return result;
		}


		public async Task<T> PostTJsonTAsync<T>(string requestUri, object content, CancellationToken token = default)
		{
			var text = (Encoding.UTF8.GetByteCount(JsonSerializer.Serialize(content)));

			//var json = JsonSerializer.Serialize(content, _jsonOptions);
			//File.WriteAllText("debag_json.json", json, Encoding.UTF8);


			var response = await _httpClient.PostAsJsonAsync(requestUri, content, _jsonOptions, token);
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
