namespace Reporting.BusinessLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a data set definition
    /// </summary>
    public class DataSetDescriptor
    {
        /// <summary>
        /// The data set tables
        /// </summary>
        private readonly Dictionary<string, TableDescriptor> _tables = new Dictionary<string, TableDescriptor>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSetDescriptor"/> class
        /// </summary>
        /// <param name="connectionString">The connection string</param>
        /// <param name="userName">The user name</param>
        public DataSetDescriptor(string connectionString, string userName)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            if (userName == null) throw new ArgumentNullException(nameof(userName));

            ConnectionString = connectionString;
            UserName = userName;
        }

        /// <summary>
        /// Gets the connection string
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Gets the user name
        /// </summary>
        public string UserName { get; }

        /// <summary>
        /// Gets the data set tables
        /// </summary>
        public IReadOnlyDictionary<string, TableDescriptor> Tables => _tables;

        /// <summary>
        /// Adds the specified table to the data set
        /// </summary>
        /// <param name="table">The table to add</param>
        public void AddTable(TableDescriptor table)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));

            _tables.Add(table.Name, table);
        }

        /// <summary>
        /// Returns a (partial) SQL query that represents the current entity
        /// </summary>
        /// <returns>A (partial) SQL query that represents the current entity</returns>
        public string BuildSql() => string.Join("; ", _tables.Values.Select(td => td.BuildSql()));
    }
}