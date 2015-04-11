namespace WindowsFormsControlLibraryRadarSoftCubeCreator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class TableDescriptor
    {
        private readonly Dictionary<string, FieldDescriptor> _fields;

        public TableDescriptor(string name, string filter = null)
        {
            Name = name;
            Filter = filter;

            _fields = new Dictionary<string, FieldDescriptor>();
            SuppressForeignKeys = new List<string>();
        }

        public string Name { get; private set; }

        public string Filter { get; private set; }

        public virtual IReadOnlyDictionary<string, FieldDescriptor> Fields
        {
            get
            {
                return _fields;
            }
        }

        public virtual FieldDescriptor PrimaryKey
        {
            get
            {
                return _fields.Values.SingleOrDefault(fd => fd.IsPrimaryKey);
            }
        }

        public virtual IEnumerable<FieldDescriptor> ForeignKeys
        {
            get
            {
                return _fields.Values.Where(fd => fd.ForeignKeyReference != null);
            }
        }

        public virtual List<string> SuppressForeignKeys { get; private set; }

        public override string ToString()
        {
            return string.Format("TABLE: {0}", Name);
        }

        public virtual void AddField(FieldDescriptor field)
        {
            _fields.Add(field.Name, field);

            field.ParentTable = this;
        }

        public virtual string BuildWhereExpression()
        {
            var whereClause = new StringBuilder();

            var fieldsFilters = _fields.Values.Where(fd => fd.Filter != null).Select(fd => fd.Filter).ToArray();

            if (Filter != null || fieldsFilters.Length > 0)
            {
                if (Filter != null)
                {
                    whereClause.Append(Filter);
                }

                if (fieldsFilters.Length > 0)
                {
                    if (Filter != null)
                    {
                        whereClause.Append(" AND ");
                    }

                    whereClause.Append(string.Join(" AND ", fieldsFilters));
                }
            }

            return whereClause.ToString();
        }

        public virtual string BuildSql()
        {
            var whereExpression = BuildWhereExpression();

            var sql = string.Format(
                "SELECT {0} FROM {1}{2}",
                string.Join(", ", _fields.Values.Select(f => f.BuildSql())),
                Name,
                string.IsNullOrWhiteSpace(whereExpression) ? string.Empty : string.Format(" WHERE {0}", whereExpression));

            return sql;
        }
    }
}