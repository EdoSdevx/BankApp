using BankAppWPF.Models;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace BankAppWPF.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;

        public ApiClient()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5000/")
            };
        }

        public async Task<ApiResult<LoginResult>?> LoginAsync(
            string email,
            string password)
        {
            var request = new LoginRequest
            {
                Email = email,
                Password = password
            };

            Debug.WriteLine(
                $"[HTTP] POST {_httpClient.BaseAddress}api/auth/login");

            using var response = await _httpClient.PostAsJsonAsync(
                "api/auth/login",
                request);

            Debug.WriteLine(
                $"[HTTP] {(int)response.StatusCode} {response.ReasonPhrase}");

            var result = await ReadResultAsync<LoginResult>(response);

            if (response.IsSuccessStatusCode &&
                result?.Success == true &&
                result.Data is not null)
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", result.Data.Token);
            }

            return result;
        }

        public async Task<ApiResult<T>?> GetAsync<T>(string requestUri)
        {
            Debug.WriteLine(
                $"[HTTP] GET {_httpClient.BaseAddress}{requestUri}");

            using var response = await _httpClient.GetAsync(requestUri);

            Debug.WriteLine(
                $"[HTTP] {(int)response.StatusCode} {response.ReasonPhrase}");

            return await ReadResultAsync<T>(response);
        }

        public async Task<ApiResult<object>?> PostAsync<TRequest>(
            string requestUri,
            TRequest request)
        {
            Debug.WriteLine(
                $"[HTTP] POST {_httpClient.BaseAddress}{requestUri}");

            using var response = await _httpClient
                .PostAsJsonAsync(requestUri, request);

            Debug.WriteLine(
                $"[HTTP] {(int)response.StatusCode} {response.ReasonPhrase}");

            return await ReadResultAsync<object>(response);
        }

        public async Task<ApiResult<object>?> PutAsync<TRequest>(
            string requestUri,
            TRequest request)
        {
            Debug.WriteLine(
                $"[HTTP] PUT {_httpClient.BaseAddress}{requestUri}");

            using var response = await _httpClient
                .PutAsJsonAsync(requestUri, request);

            Debug.WriteLine(
                $"[HTTP] {(int)response.StatusCode} {response.ReasonPhrase}");

            return await ReadResultAsync<object>(response);
        }

        public void ClearAuthentication()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        private static async Task<ApiResult<T>> ReadResultAsync<T>(
            HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(json))
            {
                return new ApiResult<T>
                {
                    Success = false,
                    StatusCode = (int)response.StatusCode,
                    Message = response.StatusCode switch
                    {
                        System.Net.HttpStatusCode.Unauthorized =>
                            "Your session is not authenticated.",
                        System.Net.HttpStatusCode.Forbidden =>
                            "You are not authorized to perform this operation.",
                        _ =>
                            $"The API returned {(int)response.StatusCode} {response.ReasonPhrase}."
                    }
                };
            }

            try
            {
                return JsonSerializer.Deserialize<ApiResult<T>>(
                    json,
                    new JsonSerializerOptions(JsonSerializerDefaults.Web))
                    ?? new ApiResult<T>
                    {
                        Success = false,
                        StatusCode = (int)response.StatusCode,
                        Message = "The API returned an empty result."
                    };
            }
            catch (JsonException exception)
            {
                Debug.WriteLine($"[HTTP] Invalid JSON response: {exception}");

                return new ApiResult<T>
                {
                    Success = false,
                    StatusCode = (int)response.StatusCode,
                    Message = "The API returned an invalid response."
                };
            }
        }
    }
}
