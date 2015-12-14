namespace Reporting.BusinessLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a table definition
    /// </summary>
    public class TableDescriptor
    {
        /// <summary>
        /// The table fields
        /// </summary>
        private readonly Dictionary<string, FieldDescriptor> _fields = new Dictionary<string, FieldDescriptor>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TableDescriptor"/> class
        /// </summary>
        /// <param name="name">The name of the table</param>
        /// <param name="filter">The filter applied to the table (if any)</param>
        public TableDescriptor(string name, string filter = null)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("The name cannot be empty", nameof(name));

            Name = name;
            Filter = filter;
        }

        /// <summary>
        /// Gets the name of the table
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the filter applied to the table (if any)
        /// </summary>
        public string Filter { get; }

        /// <summary>
        /// Gets the table fields
        /// </summary>
        public virtual IReadOnlyDictionary<string, FieldDescriptor> Fields => _fields;

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString() => $"TABLE: {Name}";

        /// <summary>
        /// Adds the specified field to the table
        /// </summary>
        /// <param name="field">The field to add</param>
        public virtual void AddField(FieldDescriptor field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));

            _fields.Add(field.Name, field);

            field.ParentTable = this;
        }

        /// <summary>
        /// Returns the WHERE SQL predicate of the table
        /// </summary>
        /// <returns>The WHERE SQL predicate of the table</returns>
        public virtual string BuildWhereExpression()
        {
            var whereExpression = string.Join(
                " AND ",
                new[] {Filter}.Union(_fields.Values.Select(fd => fd.Filter))
                    .Where(expr => !string.IsNullOrWhiteSpace(expr)));

            return whereExpression;
        }

        /// <summary>
        /// Returns a (partial) SQL query that represents the current entity
        /// </summary>
        /// <returns>A (partial) SQL query that represents the current entity</returns>
        public virtual string BuildSql()
        {
            var fields = string.Join(", ", _fields.Values.Select(f => f.BuildSql()));

            var whereExpression = BuildWhereExpression();
            var whereClause = string.IsNullOrWhiteSpace(whereExpression)
                ? string.Empty
                : $" WHERE {whereExpression}";

            var sql = $"SELECT {fields} FROM {Name}{whereClause}";

            return sql;
        }
    }
}