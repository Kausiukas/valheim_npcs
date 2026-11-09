using ValheimWorkers.Core;

namespace ValheimWorkers.AI
{
    public interface IWorkerTask
    {
        WorkerRole Role { get; }
        bool CanExecute(WorkerBrain brain);
        void StartTask(WorkerBrain brain);
        void UpdateTask(WorkerBrain brain, float dt);
        void StopTask(WorkerBrain brain);
        string GetStatusDescription();
        bool IsPerformingAction { get; }
    }
}
