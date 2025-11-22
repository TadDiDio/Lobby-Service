namespace LobbyService
{
    public interface ILobbySorter
    {
        /// <summary>
        /// Tells if a is greater than b.
        /// </summary>
        /// <param name="a">Lobby a.</param>
        /// <param name="b">Lobby b.</param>
        /// <returns>-1 if a less than b, 0 if equal, and 1 if a greater than b.</returns>
        public int CompareTo(LobbyDescriptor a, LobbyDescriptor b);
    }
}
