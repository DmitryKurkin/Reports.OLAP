namespace WindowsFormsControlLibraryRadarSoftCubeCreator
{
    using System;
    using System.Collections.Generic;

    public class FieldDescriptor : ICloneable
    {
        public FieldDescriptor(
            string name,
            string alias = null,
            string function = null,
            string filter = null,
            bool isPrimaryKey = false,
            FieldDescriptor foreignKeyReference = null)
        {
            Name = name;
            Alias = alias;
            Function = function;
            Filter = filter;
            IsPrimaryKey = isPrimaryKey;
            ForeignKeyReference = foreignKeyReference;
        }

        public string Name { get; private set; }

        public string Alias { get; private set; }

        public string Function { get; private set; }

        public string Filter { get; private set; }

        public bool IsPrimaryKey { get; private set; }

        public FieldDescriptor ForeignKeyReference { get; private set; }

        public string TableQualifiedName
        {
            get
            {
                return string.Format("{0}.{1}", ParentTable.Name, Name);
            }
        }

        public string AliasOrName
        {
            get
            {
                return Alias ?? Name;
            }
        }

        public TableDescriptor ParentTable { get; internal set; }

        public override string ToString()
        {
            return BuildSql();
        }

        public object Clone()
        {
            var descriptor = new FieldDescriptor(
                Name,
                Alias,
                Function,
                Filter,
                IsPrimaryKey,
                ForeignKeyReference);

            return descriptor;
        }

        public string BuildSql()
        {
            if (Function != null && Alias == null)
            {
                throw new InvalidOperationException("A function is defined but no alias found");
            }

            var aliasSuffix = Alias == null ? string.Empty : string.Format(" AS {0}", Alias);

            return string.Format(
                "{0}{1}",
                Function != null ? Function.Replace("#", TableQualifiedName) : TableQualifiedName, aliasSuffix);
        }
    }
}