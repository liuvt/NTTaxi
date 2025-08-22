using NTTaxi.Libraries.Models.Alis;

namespace NTTaxi.Libraries.GoogleSheetServers.Interfaces
{
    public interface IAliGgSheetServer
    {
        //Order Ali
        Task<bool> AppendOrderAliAsync(List<OrderAli> models);
        Task<bool> ClearOrderAliAsync();
        //Khuyến mãi
        Task<bool> AppendPromoteAliAsync(List<PromoteAli> models);
        Task<bool> ClearPromoteAliAsync();
        //Cancel Order Ali & Switchboard
        Task<bool> AppendCancelOrderAliAsync(List<CancelOrder> models);
        Task<bool> ClearCancelOrderAliAsync();

    }
}
