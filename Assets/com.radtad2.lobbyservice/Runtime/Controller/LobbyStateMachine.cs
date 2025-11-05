using System;

namespace LobbyService
{
    public class LobbyStateMachine
    {
        public LobbyState State { get; private set; }

        /// <summary>
        /// Attempts to change state.
        /// </summary>
        /// <param name="state">The next state.</param>
        /// <returns>True if successful.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws on unrecognized state.</exception>
        public bool TryTransition(LobbyState state)
        {
            if (state == State) return true;
            if (!CanTransitionTo(state)) return false;

            State = state;
            return true;
        }

        private bool CanTransitionTo(LobbyState state)
        {
            return state switch
            {
                LobbyState.NotInLobby => true,
                LobbyState.Joining    => State is LobbyState.NotInLobby or LobbyState.InLobby,
                LobbyState.InLobby    => State is LobbyState.Joining,
                LobbyState.Leaving    => State is LobbyState.InLobby,
                _                     => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }
    }
}
