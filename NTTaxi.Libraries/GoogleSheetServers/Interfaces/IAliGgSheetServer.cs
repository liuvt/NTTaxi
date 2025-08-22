using NTTaxi.Libraries.Models.Alis;

namespace NTTaxi.Libraries.GoogleSheetServers.Interfaces
{
    public interface IAliGgSheetServer
    {
        Task<bool> AppendOrderAliAsync(List<OrderAli> models);
        Task<bool> AppendPromoteAliAsync(List<PromoteAli> models);
    }
}
