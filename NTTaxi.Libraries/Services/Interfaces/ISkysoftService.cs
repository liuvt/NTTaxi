namespace NTTaxi.Libraries.Services.Interfaces
{
    public interface ISkysoftService
    {
        Task<string> GetCookieAsync();
        Task<string> AuthenticationAsync();
    }
}
