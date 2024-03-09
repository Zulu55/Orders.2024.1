using Orders.Shared.Responses;

namespace Orders.Backend.Services
{
    namespace Orders.Backend.Services
    {
        public interface IApiService
        {
            Task<ActionResponse<T>> GetAsync<T>(string servicePrefix, string controller);
        }
    }
}