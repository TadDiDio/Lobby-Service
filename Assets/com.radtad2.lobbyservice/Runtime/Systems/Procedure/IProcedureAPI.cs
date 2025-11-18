using System;
using LobbyService.Procedure;

namespace LobbyService
{
    public interface IProcedureAPI : IDisposable
    {
        /// <summary>
        /// Registers a procedure by key to be invoked on connected members. Late arriving members should have the
        /// most up to date function table.
        /// </summary>
        /// <param name="procedure">The procedure to run with settings about who can run it.</param>
        public void RegisterProcedureLocally(LobbyProcedure procedure);

        /// <summary>
        /// Calls a remote procedure on all connected members
        /// </summary>
        /// <param name="key">The procedure key to invoke.</param>
        /// <param name="args">The args to pass to the remote procedure.</param>
        /// <returns>
        /// True if the call was sent properly.
        /// Can fail if you don't have permissions or internet connection
        /// </returns>
        public bool Broadcast(string key, params string[] args);

        /// <summary>
        /// Calls a remote procedure on a specific member.
        /// </summary>
        /// <param name="target">The target recipient.</param>
        /// <param name="key">The procedure key to invoke.</param>
        /// <param name="args">The args to pass to the remote procedure.</param>
        /// <returns>
        /// True if the call was sent properly.
        /// Can fail if you don't have permissions or internet connection
        /// </returns>
        public bool Target(LobbyMember target, string key, params string[] args);
    }
}