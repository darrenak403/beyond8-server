using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Beyond8.Common.Utilities;
using Microsoft.AspNetCore.Http;

namespace Beyond8.Common.Clients
{
    public class BaseClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : IBaseClient
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private AuthenticationHeaderValue? GetAuthHeader()
        {
            var authHeader = httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                return new AuthenticationHeaderValue("Bearer", token);
            }
            return null;
        }

        public async Task<TResponse> GetAsync<TResponse>(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = GetAuthHeader();

            using var response = await httpClient.SendAsync(request);
            return await ProcessResponseAsync<TResponse>(response);
        }

        public async Task<TResponse> PostAsync<TResponse>(string url, object body)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = GetAuthHeader();
            request.Content = JsonContent.Create(body, options: _jsonOptions);

            using var response = await httpClient.SendAsync(request);
            return await ProcessResponseAsync<TResponse>(response);
        }

        public async Task<TResponse> PutAsync<TResponse>(string url, object body)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Headers.Authorization = GetAuthHeader();
            request.Content = JsonContent.Create(body, options: _jsonOptions);

            using var response = await httpClient.SendAsync(request);
            return await ProcessResponseAsync<TResponse>(response);
        }

        public async Task<TResponse> DeleteAsync<TResponse>(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            request.Headers.Authorization = GetAuthHeader();

            using var response = await httpClient.SendAsync(request);
            return await ProcessResponseAsync<TResponse>(response);
        }

        public async Task<TResponse> PatchAsync<TResponse>(string url, object body)
        {
            var request = new HttpRequestMessage(HttpMethod.Patch, url);
            request.Headers.Authorization = GetAuthHeader();
            request.Content = JsonContent.Create(body, options: _jsonOptions);

            using var response = await httpClient.SendAsync(request);
            return await ProcessResponseAsync<TResponse>(response);
        }

        private static async Task<TResponse> ProcessResponseAsync<TResponse>(HttpResponseMessage response)
        {
            ApiResponse<TResponse>? apiResponse = null;

            try
            {
                apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>(_jsonOptions);
            }
            catch (JsonException)
            {
                response.EnsureSuccessStatusCode();
            }

            if (!response.IsSuccessStatusCode)
            {
                var message = apiResponse?.Message ?? $"Request failed with status code {response.StatusCode}";
                throw new HttpRequestException(message, null, response.StatusCode);
            }

            if (apiResponse == null || !apiResponse.IsSuccess)
            {
                throw new Exception(apiResponse?.Message ?? "Failed to get data from response");
            }

            return apiResponse.Data!;
        }
    }
}