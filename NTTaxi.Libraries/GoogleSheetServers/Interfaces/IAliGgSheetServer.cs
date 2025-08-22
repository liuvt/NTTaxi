using NTTaxi.Libraries.Models.Alis;

namespace NTTaxi.Libraries.GoogleSheetServers.Interfaces
{
    public interface IAliGgSheetServer
    {
        Task<bool> AppendOrderAliAsync(List<OrderAli> models);
        Task<bool> ClearOrderAliAsync();
        Task<bool> AppendPromoteAliAsync(List<PromoteAli> models);
        Task<bool> ClearPromoteAliAsync();
    }
}
