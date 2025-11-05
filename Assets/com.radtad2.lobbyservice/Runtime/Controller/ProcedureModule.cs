using System;
using LobbyService.Procedure;

namespace LobbyService
{
    public class ProcedureModule : IDisposable
    {
        private LobbyModel _lobbyModel;
        private ILobbyProcedureService _service;

        public ProcedureModule(ILobbyProcedureService service, LobbyModel lobbyModel)
        {
            _service = service;
            _lobbyModel = lobbyModel;
        }

        /// <summary>
        /// Registers a procedure locally so that others can call it on this member.
        /// </summary>
        /// <param name="procedure">The procedure to register.</param>
        public void RequestRegisterProcedureLocally(LobbyProcedure procedure)
        {
            _service.RegisterProcedureLocally(procedure);
        }

        /// <summary>
        /// Broadcasts a procedure call to all members. If sent, anyone who has registered
        /// the same key local to their machine will run it.
        /// </summary>
        /// <param name="key">The unique key.</param>
        /// <param name="args">The args to pass.</param>
        /// <returns>True if send, false if failed due to network or permissions.</returns>
        public bool RequestBroadcast(string key, params string[] args)
        {
            var lobbyId = _lobbyModel.LobbyId;
            return _service.Broadcast(lobbyId, key, args);
        }

        /// <summary>
        /// Broadcasts a procedure call to a specific members. If sent, anyone who has registered
        /// the same key local to their machine will run it.
        /// </summary>
        /// <param name="member">The target.</param>
        /// <param name="key">The unique key.</param>
        /// <param name="args">The args to pass.</param>
        /// <returns>True if send, false if failed due to network or permissions.</returns>
        public bool RequestTarget(LobbyMember member, string key, params string[] args)
        {
            var lobbyId = _lobbyModel.LobbyId;
            return _service.Target(lobbyId, member, key, args);
        }

        public void Dispose() { }
    }
}
