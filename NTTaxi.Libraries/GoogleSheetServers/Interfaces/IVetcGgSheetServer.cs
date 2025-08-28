using NTTaxi.Libraries.Models.Vetcs;

namespace NTTaxi.Libraries.GoogleSheetServers.Interfaces
{
    public interface IVetcGgSheetServer
    {
        //BLU
        Task<bool> AppendVetcAsync(List<VetcItem> models, string provinceCode);
        Task<bool> ClearVetcAsync(string provinceCode);
    }
}
