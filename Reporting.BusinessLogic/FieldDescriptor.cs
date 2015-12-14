namespace Reporting.BusinessLogic
{
    using System;

    /// <summary>
    /// Represents a field definition
    /// </summary>
    public class FieldDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldDescriptor"/> class
        /// </summary>
        /// <param name="name">The name of the field</param>
        /// <param name="alias">The name alias of the field</param>
        /// <param name="function">The aggregate function applied to the field</param>
        /// <param name="filter">The filter applied to the field (if any)</param>
        /// <param name="references">The name of the table this FK references (if it is an FK)</param>
        public FieldDescriptor(
            string name,
            string alias = null,
            string function = null,
            string filter = null,
            string references = null)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("The name cannot be empty", nameof(name));

            if (function != null && alias == null)
                throw new ArgumentException($"Function {function} is defined but no alias is assigned", nameof(alias));

            Name = name;
            Alias = alias;
            Function = function;
            Filter = filter;
            References = references;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldDescriptor"/> class
        /// </summary>
        /// <param name="name">The name of the field</param>
        public FieldDescriptor(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("The name cannot be empty", nameof(name));

            Name = name;
            IsPrimaryKey = true;
        }

        /// <summary>
        /// Gets the name of the field
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the name alias of the field
        /// </summary>
        public string Alias { get; }

        /// <summary>
        /// Gets the aggregate function applied to the field
        /// </summary>
        public string Function { get; }

        /// <summary>
        /// Gets the filter applied to the field (if any)
        /// </summary>
        public string Filter { get; }

        /// <summary>
        /// Gets a value indicating whether the field is a PK
        /// </summary>
        public bool IsPrimaryKey { get; }

        /// <summary>
        /// Gets the name of the table this FK references (if it is an FK)
        /// </summary>
        public string References { get; }

        /// <summary>
        /// Gets the table-qualified name of the field
        /// </summary>
        public string TableQualifiedName => $"{ParentTable.Name}.{Name}";

        /// <summary>
        /// Gets either the alias or the name of the field
        /// </summary>
        public string AliasOrName => Alias ?? Name;

        /// <summary>
        /// Gets the <see cref="TableDescriptor"/> that defines the field
        /// </summary>
        public TableDescriptor ParentTable { get; internal set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString() => BuildSql();

        /// <summary>
        /// Returns a (partial) SQL query that represents the current entity
        /// </summary>
        /// <returns>A (partial) SQL query that represents the current entity</returns>
        public string BuildSql()
        {
            var aliasSuffix = Alias == null ? string.Empty : $" AS {Alias}";

            return $"{Function?.Replace("#", TableQualifiedName) ?? TableQualifiedName}{aliasSuffix}";
        }
    }
}