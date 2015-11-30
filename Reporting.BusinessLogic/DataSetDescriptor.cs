namespace Reporting.BusinessLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class DataSetDescriptor
    {
        private readonly Dictionary<string, TableDescriptor> _tables;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSetDescriptor"/> class
        /// </summary>
        /// <param name="connectionString">The connection string</param>
        /// <param name="userName">The user name</param>
        public DataSetDescriptor(string connectionString, string userName)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            if (userName == null) throw new ArgumentNullException(nameof(userName));

            _tables = new Dictionary<string, TableDescriptor>();

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

        public IReadOnlyDictionary<string, TableDescriptor> Tables
        {
            get { return _tables; }
        }

        public void AddTable(TableDescriptor table)
        {
            _tables.Add(table.Name, table);
        }

        public string BuildSql()
        {
            return string.Join("; ", _tables.Values.Select(td => td.BuildSql()));
        }
    }
}