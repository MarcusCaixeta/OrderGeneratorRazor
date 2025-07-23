using OrderGenerator.WorkerService.Interfaces;
using QuickFix;

namespace OrderGenerator.WorkerService.Service
{
    public class FixSessionManager : IFixSessionManager
    {
        private TaskCompletionSource<bool> _logonTcs = new();
        public SessionID? CurrentSessionID { get; private set; }

        public void SetSessionID(SessionID sessionID) => CurrentSessionID = sessionID;

        public void NotifyLogon(SessionID sessionID)
        {
            CurrentSessionID = sessionID;
            _logonTcs.TrySetResult(true);
        }

        public void NotifyLogout(SessionID _) => _logonTcs = new();

        public async Task<bool> WaitForLogonAsync(int timeoutMs = 5000)
        {
            var completed = await Task.WhenAny(_logonTcs.Task, Task.Delay(timeoutMs));
            return completed == _logonTcs.Task && _logonTcs.Task.Result;
        }
    }

}
