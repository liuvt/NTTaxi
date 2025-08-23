namespace NTTaxi.Libraries.Workers.Interfaces
{
    public interface IAliWorker
    {
        void Start();
        void Stop();
        bool IsRunning { get; }
        event Action? StatusChanged;

    }
}
