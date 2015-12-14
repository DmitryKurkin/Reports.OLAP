namespace Reporting.BusinessLogic
{
    using System;

    /// <summary>
    /// Represents a link definition
    /// </summary>
    public class LinkDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinkDescriptor"/> class
        /// </summary>
        /// <param name="table">The table name of the link</param>
        public LinkDescriptor(string table)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (string.IsNullOrWhiteSpace(table))
                throw new ArgumentException("The table cannot be empty", nameof(table));

            Table = table;

            IsFirstLink = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkDescriptor"/> class
        /// </summary>
        public LinkDescriptor(string table, string fromKey, string toKey)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (fromKey == null) throw new ArgumentNullException(nameof(fromKey));
            if (toKey == null) throw new ArgumentNullException(nameof(toKey));
            if (string.IsNullOrWhiteSpace(table))
                throw new ArgumentException("The table cannot be empty", nameof(table));
            if (string.IsNullOrWhiteSpace(fromKey))
                throw new ArgumentException("The from key cannot be empty", nameof(fromKey));
            if (string.IsNullOrWhiteSpace(table))
                throw new ArgumentException("The to key cannot be empty", nameof(toKey));

            Table = table;
            FromKey = fromKey;
            ToKey = toKey;
        }

        /// <summary>
        /// Gets a value indicating whether this is the first link in the chain
        /// </summary>
        public bool IsFirstLink { get; }

        /// <summary>
        /// Gets the table name of the link
        /// </summary>
        public string Table { get; }

        /// <summary>
        /// Gets the 'output' column name of the link (should belong to <see cref="Table"/>)
        /// </summary>
        public string FromKey { get; }

        /// <summary>
        /// Gets the 'input' column name of the link (should belong to a different table)
        /// </summary>
        public string ToKey { get; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString() => IsFirstLink ? $"FIRST LINK: {Table}" : $"LINK: {Table}.{FromKey}->{ToKey}";

        /// <summary>
        /// Returns a (partial) SQL query that represents the current entity
        /// </summary>
        /// <returns>A (partial) SQL query that represents the current entity</returns>
        public string BuildSql()
            => IsFirstLink ? $"{Table}" : $"LEFT OUTER JOIN {Table} ON {Table}.{FromKey} = {ToKey}";
    }
}