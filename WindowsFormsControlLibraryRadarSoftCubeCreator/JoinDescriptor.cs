namespace WindowsFormsControlLibraryRadarSoftCubeCreator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class JoinDescriptor : TableDescriptor
    {
        public JoinDescriptor(string name, TableDescriptor leftTable, TableDescriptor rightTable)
            : base(name)
        {
            if (leftTable == null)
            {
                throw new ArgumentNullException("leftTable");
            }
            if (rightTable == null)
            {
                throw new ArgumentNullException("rightTable");
            }

            LeftTable = leftTable;
            RightTable = rightTable;
        }

        public override FieldDescriptor PrimaryKey
        {
            get
            {
                var localPk = (FieldDescriptor)LeftTable.PrimaryKey.Clone();
                localPk.ParentTable = this;

                return localPk;
            }
        }

        public override IEnumerable<FieldDescriptor> ForeignKeys
        {
            get
            {
                var localFks = new List<FieldDescriptor>();

                foreach (var fd in LeftTable.ForeignKeys.Concat(RightTable.ForeignKeys))
                {
                    var fdCopy = (FieldDescriptor)fd.Clone();
                    fdCopy.ParentTable = this;

                    localFks.Add(fdCopy);
                }

                return localFks;
            }
        }

        public override List<string> SuppressForeignKeys
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override IReadOnlyDictionary<string, FieldDescriptor> Fields
        {
            get
            {
                var localFields = new Dictionary<string, FieldDescriptor>();

                foreach (var kvp in LeftTable.Fields.Concat(RightTable.Fields))
                {
                    var fdCopy = (FieldDescriptor)kvp.Value.Clone();
                    fdCopy.ParentTable = this;

                    localFields.Add(fdCopy.AliasOrName, fdCopy);
                }

                return localFields;
            }
        }

        public TableDescriptor LeftTable { get; private set; }

        public TableDescriptor RightTable { get; private set; }

        public override string ToString()
        {
            return string.Format("JOIN: {0} & {1}", LeftTable.Name, RightTable.Name);
        }

        public override void AddField(FieldDescriptor field)
        {
            throw new NotSupportedException();
        }

        public override string BuildWhereExpression()
        {
            var filter = new StringBuilder();
            
            var leftTableFilter = LeftTable.BuildWhereExpression();
            var rightTableFilter = RightTable.BuildWhereExpression();
            
            if (!string.IsNullOrWhiteSpace(leftTableFilter) || !string.IsNullOrWhiteSpace(rightTableFilter))
            {
                if (!string.IsNullOrWhiteSpace(leftTableFilter))
                {
                    filter.Append(leftTableFilter);

                    if (!string.IsNullOrWhiteSpace(rightTableFilter))
                    {
                        filter.AppendFormat(" AND {0}", rightTableFilter);
                    }
                }
                else
                {
                    filter.Append(rightTableFilter);
                }
            }

            return filter.ToString();
        }

        public override string BuildSql()
        {
            var fields = string.Join(
                ", ",
                LeftTable.Fields.Values.Select(f => f.BuildSql()).Concat(RightTable.Fields.Values.Select(f => f.BuildSql())));

            var leftJoinField = LeftTable.PrimaryKey;
            if (leftJoinField == null)
            {
                throw new InvalidOperationException("Left table does not have the primary key: " + LeftTable.Name);
            }

            // we are looking for an FK that references the 'leftJoinField' (comparison by the name/alias)
            var rightJoinField = RightTable.ForeignKeys.SingleOrDefault(fd => fd.ForeignKeyReference.AliasOrName == leftJoinField.AliasOrName);
            if (rightJoinField == null)
            {
                throw new InvalidOperationException("Right table does not have a foreign key that references the left table: " + RightTable.Name);
            }

            var joinExpression = string.Format(
                "{0} = {1}",
                leftJoinField.TableQualifiedName,
                rightJoinField.TableQualifiedName);

            var whereExpression = BuildWhereExpression();

            var sql = string.Format(
                "SELECT {0} FROM {1} LEFT OUTER JOIN {2} ON {3}{4}",
                fields,
                LeftTable.Name,
                RightTable.Name,
                joinExpression,
                string.IsNullOrWhiteSpace(whereExpression) ? string.Empty : string.Format(" WHERE {0}", whereExpression));

            return sql;
        }
    }
}