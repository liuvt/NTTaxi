using NTTaxi.Libraries.Models.Alis;

namespace NTTaxi.Libraries.Services.Interfaces
{
    public interface IAliService
    {
        Task<bool> GetAuthenticationAsync(UserAli user);
        Task<List<OrderAli>> GetsOrderAli(SchemaJson _json, DateTime start, DateTime end);
        Task PostOrderAli(SchemaJson _json, DateTime start, DateTime end);
        Task<bool> GetsPromoteAli();
    }
}
