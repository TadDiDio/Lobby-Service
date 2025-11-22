using LobbyService.Procedure;

namespace LobbyService
{
    /// <summary>
    /// Safely no-ops all procedure operations.
    /// </summary>
    public class NullProcedureModule : IProcedureAPI
    {
        public void RegisterProcedureLocally(LobbyProcedure procedure) { }
        public bool Broadcast(string key, params string[] args) => false;
        public bool Target(LobbyMember target, string key, params string[] args) => false;
        public void Dispose() { }
    }
}