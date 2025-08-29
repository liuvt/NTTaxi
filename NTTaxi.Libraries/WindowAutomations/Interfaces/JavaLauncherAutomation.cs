using NTTaxi.Libraries.Models.Skysofts;

namespace NTTaxi.Libraries.WindowAutomations.Interfaces
{
    public interface IJavaLauncherAutomation
    {
        void OpenSkysoftJnlp(string jnlpPath, UserSkysoft user);
    }
}
