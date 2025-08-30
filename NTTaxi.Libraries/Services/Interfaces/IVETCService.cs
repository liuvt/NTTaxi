using NTTaxi.Libraries.Models.Alis;
using NTTaxi.Libraries.Models.Vetcs;
namespace NTTaxi.Libraries.Services.Interfaces
{
    public interface IVETCService
    {
        Task<bool> GetAuthenticationAsync(UserVetc user, string provinceCode);
        Task<List<VetcItem>> GetsVetcAsync(GetsPayload payload);

        Task<List<VetcItem>> PostVetcAsync(GetsPayload payload, string provinceCode);
        Task<List<VetcItem>> FormatPostsVetcAsync(GetsPayload payload, string provinceCode);
    }
}
