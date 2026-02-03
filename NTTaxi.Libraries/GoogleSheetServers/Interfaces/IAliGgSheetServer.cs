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

        //Switchboard
        Task<bool> AppendSwitchboardAliAsync(List<SwitchboardAli> models);
        Task<bool> ClearSwitchboardAliAsync();

        //GSM Partner
        Task<bool> AppendPartnerGSMAliAsync(List<PartnerGSM> models);
        Task<bool> ClearPartnerGSMAliAsync();

        //Online App
        Task<bool> AppendOnlineAliAsync(List<OnlineAppAli> models);
        Task<bool> ClearOnlineAliAsync();
    }
}
