using System.Collections.Generic;
using System.Linq;

namespace LobbyService
{
    public class CompositeLobbySorterComparer : IComparer<LobbyDescriptor>
    {
        private readonly IReadOnlyList<ILobbySorter> _sorters;

        public CompositeLobbySorterComparer(IEnumerable<ILobbySorter> sorters)
        {
            _sorters = sorters.ToArray();
        }

        public int Compare(LobbyDescriptor x, LobbyDescriptor y)
        {
            foreach (var sorter in _sorters)
            {
                int result = sorter.CompareTo(x, y);
                if (result != 0) return result;
            }

            return 0;
        }
    }
}
