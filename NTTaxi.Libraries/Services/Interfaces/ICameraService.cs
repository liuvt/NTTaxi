using NTTaxi.Libraries.Models.Cameras;

namespace NTTaxi.Libraries.Services.Interfaces;

public interface ICameraService
{
    Task<bool> GetAuthenticationAsync(CameraLoginRequest user, CancellationToken cancellationToken = default);
}
