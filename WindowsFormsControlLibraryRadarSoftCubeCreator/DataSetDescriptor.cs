namespace WindowsFormsControlLibraryRadarSoftCubeCreator
{
    using System.Collections.Generic;
    using System.Linq;

    public class DataSetDescriptor
    {
        private readonly Dictionary<string, TableDescriptor> _tables;

        public DataSetDescriptor()
        {
            _tables = new Dictionary<string, TableDescriptor>();
        }

        public IReadOnlyDictionary<string, TableDescriptor> Tables
        {
            get
            {
                return _tables;
            }
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