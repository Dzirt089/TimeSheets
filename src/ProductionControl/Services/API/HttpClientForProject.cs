﻿using System.Net.Http;

namespace ProductionControl.Services.API
{
	public class HttpClientForProject
	{
		public static HttpClient _client;

		public HttpClientForProject()
		{
			_client = new HttpClient();
		}
		public HttpClient GetHttpClient()
		{
			return _client;
		}
	}
}
