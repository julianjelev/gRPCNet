using gRPCNet.ServerAPI.Models.Dto.Common;
using System.Text;
using System.Threading.Tasks;

namespace gRPCNet.ServerAPI.CommonServices.RestApi
{
    public interface IRestApiClient
    {
        Task<(Result<T> result, bool hasError, string errorMessage)> GetAsync<T>(string baseAddress, string apiEndpoint, string queryParams = null);

        Task<(int statusCode, T resultContent, bool hasError, string errorMessage)> PostAsync<T>(string baseAddress, string apiEndpoint, string content = null, Encoding encoding = null, string mediaType = null);

        Task<(int statusCode, T resultContent, bool hasError, string errorMessage)> PutAsync<T>(string baseAddress, string apiEndpoint, string content = null, Encoding encoding = null, string mediaType = null);

        Task<(int statusCode, T resultContent, bool hasError, string errorMessage)> DeleteAsync<T>(string baseAddress, string apiEndpoint);
    }
}
