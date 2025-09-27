using NTTaxi.Libraries.Models.Gsms;
namespace NTTaxi.Libraries.Services.Interfaces
{
    public interface IGsmService
    {
        Task<GsmAuthData> GetAuthTokenAsync(GsmUser _user);
    }
}
