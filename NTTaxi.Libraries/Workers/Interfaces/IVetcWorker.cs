namespace NTTaxi.Libraries.Workers.Interfaces
{
    public interface IVetcWorker
    {
        void Start();
        void Stop();
        bool IsRunning { get; }
        event Action? StatusChanged;

    }
}
