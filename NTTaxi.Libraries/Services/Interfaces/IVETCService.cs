using NTTaxi.Libraries.Models.Alis;
using NTTaxi.Libraries.Models.Vetcs;
namespace NTTaxi.Libraries.Services.Interfaces
{
    public interface IVETCService
    {
        Task<string> GetAuthenticationAsync(UserVETC user);
        Task<string> GetDatas();
    }
}
