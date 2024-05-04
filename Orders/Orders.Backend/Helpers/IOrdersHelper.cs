using Orders.Shared.Responses;

namespace Orders.Backend.Helpers
{
    public interface IOrdersHelper
    {
        Task<ActionResponse<bool>> ProcessOrderAsync(string email, string remarks);
    }
}