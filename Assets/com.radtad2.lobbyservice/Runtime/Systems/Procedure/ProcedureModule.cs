using System;
using LobbyService.Procedure;

namespace LobbyService
{
    public class ProcedureModule : IProcedureAPI, IDisposable
    {
        private LobbyModel _lobbyModel;
        private IProcedureProvider _service;

        public ProcedureModule(IProcedureProvider service, LobbyModel lobbyModel)
        {
            _service = service;
            _lobbyModel = lobbyModel;
        }

        public void RegisterProcedureLocally(LobbyProcedure procedure)
        {
            _service.RegisterProcedureLocally(procedure);
        }

        public bool Broadcast(string key, params string[] args)
        {
            var lobbyId = _lobbyModel.LobbyId;
            return _service.Broadcast(lobbyId, key, args);
        }

        public bool Target(LobbyMember member, string key, params string[] args)
        {
            var lobbyId = _lobbyModel.LobbyId;
            return _service.Target(lobbyId, member, key, args);
        }

        public void Dispose() { }
    }
}
