namespace Reporting.BusinessLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a table-multijoin definition
    /// </summary>
    public class MultiJoinDescriptor : TableDescriptor
    {
        /// <summary>
        /// The source tables
        /// </summary>
        private readonly List<TableDescriptor> _tables;

        /// <summary>
        /// The source table links
        /// </summary>
        private readonly List<LinkDescriptor> _links;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiJoinDescriptor"/> class
        /// </summary>
        /// <param name="name">The name of the table</param>
        /// <param name="primaryKey">The name of the primary key column which should be introduced in the result table</param>
        /// <param name="maxRows">The maximum number of rows that will be returned in the result table</param>
        /// <param name="tables">The source tables</param>
        /// <param name="links">The source table links</param>
        public MultiJoinDescriptor(
            string name,
            string primaryKey,
            int maxRows,
            IEnumerable<TableDescriptor> tables,
            IEnumerable<LinkDescriptor> links)
            : base(name)
        {
            if (primaryKey == null) throw new ArgumentNullException(nameof(primaryKey));
            if (tables == null) throw new ArgumentNullException(nameof(tables));
            if (links == null) throw new ArgumentNullException(nameof(links));

            if (maxRows < 0)
                throw new ArgumentOutOfRangeException(nameof(maxRows), "The maximum number of rows must not be negative");

            PrimaryKey = primaryKey;
            MaxRows = maxRows;
            _tables = tables.ToList();
            _links = links.ToList();

            if (_tables.Count < 2)
                throw new ArgumentException(
                    "The number of tables must be greater than 1",
                    nameof(tables));

            if (_links.Count == 0)
                throw new ArgumentException(
                    "The number of links must be greater than 0",
                    nameof(links));

            if (_links.Count != _tables.Count)
                throw new ArgumentException(
                    "The number of links must be equal to the number of tables",
                    nameof(links));

            if (_links.Count(ld => ld.IsFirstLink) != 1)
                throw new ArgumentException(
                    "There must be exactly 1 first link",
                    nameof(links));

            if (!_links.First().IsFirstLink)
                throw new ArgumentException(
                    "The first link must be the first one in the list",
                    nameof(links));

            // TODO: validate the links with respect to the table names and column names
        }

        /// <summary>
        /// Gets the table fields
        /// </summary>
        public override IReadOnlyDictionary<string, FieldDescriptor> Fields
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the name of the table which introduces the primary key of the multijoin
        /// </summary>
        public string PrimaryKey { get; }

        /// <summary>
        /// Gets the maximum number of rows that will be returned in the result table
        /// </summary>
        public int MaxRows { get; }

        /// <summary>
        /// Gets the tables of the multijoin
        /// </summary>
        public IReadOnlyCollection<TableDescriptor> Tables => _tables.AsReadOnly();

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString() => $"MULTIJOIN: {Name}";

        /// <summary>
        /// Adds the specified field to the table
        /// </summary>
        /// <param name="field">The field to add</param>
        public override void AddField(FieldDescriptor field)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns the WHERE SQL predicate of the table
        /// </summary>
        /// <returns>The WHERE SQL predicate of the table</returns>
        public override string BuildWhereExpression()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns a (partial) SQL query that represents the current entity
        /// </summary>
        /// <returns>A (partial) SQL query that represents the current entity</returns>
        public override string BuildSql()
        {
            var fields = string.Join(
                ", ",
                _tables.SelectMany(td => td.Fields.Values.Select(f => f.BuildSql())));

            var whereExpression = string.Join(
                "AND ",
                _tables.Select(td => td.BuildWhereExpression()).Where(expr => !string.IsNullOrWhiteSpace(expr)));
            var whereClause = string.IsNullOrWhiteSpace(whereExpression)
                ? string.Empty
                : $" WHERE {whereExpression}";

            var fromExpression = string.Join(" ", _links.Select(ld => ld.BuildSql()));

            var fetchClause = MaxRows > 0 ? $" FETCH FIRST {MaxRows} ROWS ONLY" : string.Empty;

            var sql =
                $"SELECT ROW_NUMBER() OVER() as {PrimaryKey}, {fields} FROM {fromExpression}{whereClause}{fetchClause}";

            return sql;
        }
    }
}