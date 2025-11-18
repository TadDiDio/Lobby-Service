using System.Collections.Generic;

namespace LobbyService
{
    public interface IBrowserSorterProvider
    {
        /// <summary>
        /// Applies all sorters.
        /// </summary>
        /// <param name="lobbies">The lobbies to sort in place.</param>
        public void ApplySorters(List<LobbyDescriptor> lobbies);
        
        /// <summary>
        /// Adds a sorter.
        /// </summary>
        /// <param name="sorter">The sorter.</param>
        /// <param name="key">The key.</param>
        /// <remarks>Sorters are applied in the order they are added.</remarks>
        public void AddSorter(ILobbySorter sorter, string key);

        /// <summary>
        /// Removes a sorter.
        /// </summary>
        /// <param name="key">The sorter to remove.</param>
        public void RemoveSorter(string key);

        /// <summary>
        /// Removes all sorters.
        /// </summary>
        public void ClearSorters();
    }
}