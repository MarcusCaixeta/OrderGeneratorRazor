using QuickFix;

namespace OrderGenerator.Contracts.Interfaces
{
    public interface IFixSessionManager
    {
        SessionID? CurrentSessionID { get; }
        void SetSessionID(SessionID sessionID);
        Task<bool> WaitForLogonAsync(int timeoutMs = 5000);
        void NotifyLogon(SessionID sessionID);
        void NotifyLogout(SessionID sessionID);
    }
}
