using System.Diagnostics;

namespace JobScheduler
{
    public interface IProcessManager
    {
        void KillProcess(int processId);
    }

    public class ProcessManager : IProcessManager
    {
        public void KillProcess(int processId)
        {
            var process = Process.GetProcessById(processId);
            process.Kill();
        }
    }
}
