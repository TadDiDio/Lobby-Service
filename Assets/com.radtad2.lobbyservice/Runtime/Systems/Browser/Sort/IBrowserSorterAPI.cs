using System.Collections.Generic;

namespace LobbyService
{
    public interface IBrowserSorterAPI
    {
        /// <summary>
        /// Applies all sorters in the order they were added.
        /// </summary>
        /// <param name="descriptors">The list of descriptors to sort.</param>
        public void ApplySorters(List<LobbyDescriptor> descriptors);
        
        /// <summary>
        /// Adds a sorter for use during ApplySorters..
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