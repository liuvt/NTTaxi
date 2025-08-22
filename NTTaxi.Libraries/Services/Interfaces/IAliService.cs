using NTTaxi.Libraries.Models.Alis;

namespace NTTaxi.Libraries.Services.Interfaces
{
    public interface IAliService
    {
        Task<bool> GetAuthenticationAsync(UserAli user);

        Task<List<OrderAli>> GetsOrderAli(SchemaJson _json, DateTime start, DateTime end);
        Task PostOrderAli(SchemaJson _json, DateTime start, DateTime end);

        Task<List<PromoteAli>> GetsPromoteAli(SchemaJson _json, DateTime start, DateTime end);
        Task PostPromoteAli(SchemaJson _json, DateTime start, DateTime end);

        Task<List<CancelOrder>> GetsCancelOrderAli(DateTime start, DateTime end);
        Task PostCancelOrderAli(DateTime start, DateTime end);
    }
}
