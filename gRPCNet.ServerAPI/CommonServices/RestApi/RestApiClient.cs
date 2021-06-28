using gRPCNet.ServerAPI.Models.Dto.Common;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace gRPCNet.ServerAPI.CommonServices.RestApi
{
    public class RestApiClient : IRestApiClient
    {
        private readonly HttpClient _client;
        public RestApiClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<(Result<T> result, bool hasError, string errorMessage)> GetAsync<T>(string baseAddress, string apiEndpoint, string queryParams = null)
        {
            Result<T> result = new Result<T>();
            bool hasError = false;
            string errorMessage = string.Empty;

            try
            {
                using (var response = await _client.GetAsync(FormatUrl(baseAddress, apiEndpoint, queryParams)))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        T resultContent = await response.Content.ReadAsAsync<T>();
                        result.IsSuccess = true;
                        result.Data = resultContent;
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Messages.Add($"{response.StatusCode} Entity not found");
                    }
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Messages.Add(ex.Message);
                hasError = true;
                errorMessage = ex.ToString();
            }

            return (result, hasError, errorMessage);
        }

        public async Task<(int statusCode, T resultContent, bool hasError, string errorMessage)> PostAsync<T>(string baseAddress, string apiEndpoint, string content = null, Encoding encoding = null, string mediaType = null)
        {
            T resultContent = default;
            int statusCode = 0;
            bool hasError = false;
            string errorMessage = string.Empty;

            var data = new StringContent(
                string.IsNullOrWhiteSpace(content) ? string.Empty : content,
                encoding ?? Encoding.UTF8,
                string.IsNullOrWhiteSpace(mediaType) ? "text/plain" : mediaType);
            try
            {
                using (var response = await _client.PostAsync(FormatUrl(baseAddress, apiEndpoint, null), data))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        resultContent = await response.Content.ReadAsAsync<T>();
                    }
                    statusCode = (int)response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                hasError = true;
                errorMessage = ex.ToString();
            }

            return (statusCode, resultContent, hasError, errorMessage);
        }

        public async Task<(int statusCode, T resultContent, bool hasError, string errorMessage)> PutAsync<T>(string baseAddress, string apiEndpoint, string content = null, Encoding encoding = null, string mediaType = null)
        {
            T resultContent = default;
            int statusCode = 0;
            bool hasError = false;
            string errorMessage = string.Empty;

            var data = new StringContent(
                string.IsNullOrWhiteSpace(content) ? string.Empty : content,
                encoding ?? Encoding.UTF8,
                string.IsNullOrWhiteSpace(mediaType) ? "text/plain" : mediaType);
            try
            {
                using (var response = await _client.PutAsync(FormatUrl(baseAddress, apiEndpoint, null), data))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        resultContent = await response.Content.ReadAsAsync<T>();
                    }
                    statusCode = (int)response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                hasError = true;
                errorMessage = ex.ToString();
            }

            return (statusCode, resultContent, hasError, errorMessage);
        }

        public async Task<(int statusCode, T resultContent, bool hasError, string errorMessage)> DeleteAsync<T>(string baseAddress, string apiEndpoint)
        {
            T resultContent = default;
            int statusCode = 0;
            bool hasError = false;
            string errorMessage = string.Empty;

            try
            {
                using (var response = await _client.DeleteAsync(FormatUrl(baseAddress, apiEndpoint, null)))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        resultContent = await response.Content.ReadAsAsync<T>();
                    }
                    statusCode = (int)response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                hasError = true;
                errorMessage = ex.ToString();
            }

            return (statusCode, resultContent, hasError, errorMessage);
        }

        private string FormatUrl(string baseAddress, string apiEndpoint, string queryParams = null)
        {
            StringBuilder uriBulder = new StringBuilder();
            if (!baseAddress.StartsWith("http"))
                uriBulder.AppendFormat("http://{0}", baseAddress);
            else
                uriBulder.Append(baseAddress);
            if (!baseAddress.EndsWith("/"))
                uriBulder.Append("/");
            if (!apiEndpoint.StartsWith("/"))
                uriBulder.Append(apiEndpoint);
            else
                uriBulder.Append(apiEndpoint.TrimStart('/'));
            if (!string.IsNullOrWhiteSpace(queryParams))
            {
                if (!queryParams.StartsWith("?"))
                    uriBulder.AppendFormat("?{0}", queryParams);
                else
                    uriBulder.Append(queryParams);
            }
            return uriBulder.ToString();
        }
    }
}
