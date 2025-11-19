namespace LobbyService
{
    /// <summary>
    /// The possible distance filter values.
    /// </summary>
    public enum LobbyDistance
    {
        Default,
        Near,
        Far,
        WorldWide,
    }

    /// <summary>
    /// The possible types of numerical comparison.
    /// </summary>
    public enum ComparisonType
    {
        NotEqual = 0,
        LessThan = 1,
        LessThanOrEqual = 2,
        Equal = 3,
        GreaterThan = 4,
        GreaterThanOrEqual = 5
    }

    /// <summary>
    /// A number filter.
    /// </summary>
    public struct LobbyNumberFilter
    {
        /// <summary>
        /// The key.
        /// </summary>
        public string Key;

        /// <summary>
        /// The value.
        /// </summary>
        public int Value;

        /// <summary>
        /// The type of comparison to make.
        /// </summary>
        public ComparisonType ComparisonType;
    }

    /// <summary>
    /// A string filter.
    /// </summary>
    public struct LobbyStringFilter
    {
        /// <summary>
        /// The key.
        /// </summary>
        public string Key;

        /// <summary>
        /// The value.
        /// </summary>
        public string Value;
    }

    /// <summary>
    /// A key and sorter pair.
    /// </summary>
    public struct LobbyKeyAndSorter
    {
        /// <summary>
        /// The key.
        /// </summary>
        public string Key;

        /// <summary>
        /// The sorter.
        /// </summary>
        public ILobbySorter Sorter;
    }
}
