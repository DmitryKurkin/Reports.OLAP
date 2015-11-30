namespace Reporting.BusinessLogic
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    public static class DataSetDescriptorBuilder
    {
        private const int MajorVersion = 0;

        private const int MinorVersion = 16;

        public static IExternalFilterProvider FilterProvider { get; set; }

        public static DataSetDescriptor Build(string contents)
        {
            if (FilterProvider == null)
            {
                throw new InvalidOperationException("The filter provider is not set");
            }

            var doc = XElement.Parse(contents);

            ValidateVersion(doc);

            var dataSetDesc = CreateDataSetDescriptor(doc);

            return dataSetDesc;
        }

        private static void ValidateVersion(XElement doc)
        {
            var versionMajor = int.Parse((string) doc.Attribute("versionMajor"), new CultureInfo("en-US"));
            if (versionMajor != MajorVersion)
            {
                throw new InvalidDataException("The major version is invalid");
            }

            var versionMinor = int.Parse((string) doc.Attribute("versionMinor"), new CultureInfo("en-US"));
            if (versionMinor != MinorVersion)
            {
                throw new InvalidDataException("The minor version is invalid");
            }
        }

        private static DataSetDescriptor CreateDataSetDescriptor(XElement doc)
        {
            var dataSetDesc = new DataSetDescriptor(
                (string)doc.Attribute("connectionString"),
                (string)doc.Attribute("userName"));

            var ns = doc.Name.Namespace;

            foreach (var t in doc.Element(ns + "DataSet").Elements(ns + "Table"))
            {
                var table = ReadTableDescriptor(ns, t);

                dataSetDesc.AddTable(table);
            }

            foreach (var j in doc.Element(ns + "DataSet").Elements(ns + "Join"))
            {
                var joinTables = new List<TableDescriptor>();

                foreach (var t in j.Elements(ns + "Table"))
                {
                    var table = ReadTableDescriptor(ns, t);

                    joinTables.Add(table);
                }

                var alias = (string) j.Attribute("alias");
                var leftName = (string) j.Attribute("left");
                var rightName = (string) j.Attribute("right");
                if (alias == null)
                {
                    throw new InvalidOperationException("Alias attribute for a join is not found");
                }
                if (leftName == null)
                {
                    throw new InvalidOperationException("Left table attribute for a join is not found");
                }
                if (rightName == null)
                {
                    throw new InvalidOperationException("Right table attribute for a join is not found");
                }

                var leftTable = joinTables.SingleOrDefault(t => t.Name == leftName);
                var rightTable = joinTables.SingleOrDefault(t => t.Name == rightName);
                if (leftTable == null)
                {
                    throw new InvalidOperationException("Left table for a join with the name specified is not found: " +
                                                        leftName);
                }
                if (rightTable == null)
                {
                    throw new InvalidOperationException(
                        "Right table for a join with the name specified is not found: " + rightName);
                }

                var joinDesc = new JoinDescriptor(alias, leftTable, rightTable);

                dataSetDesc.AddTable(joinDesc);
            }

            return dataSetDesc;
        }

        private static TableDescriptor ReadTableDescriptor(XNamespace ns, XElement e)
        {
            var table = new TableDescriptor((string) e.Attribute("name"), (string) e.Attribute("filter"));

            var suppressForeignKeys = (string) e.Attribute("suppressForeignKeys");
            if (suppressForeignKeys != null)
            {
                table.SuppressForeignKeys.AddRange(
                    suppressForeignKeys.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(fk => fk.Trim(' ')));
            }

            foreach (var f in e.Descendants(ns + "Field"))
            {
                var fieldName = (string) f.Attribute("name");
                var fieldAlias = (string) f.Attribute("alias");

                string filter = null;

                var externalFilter = (string) f.Attribute("externalFilter");
                if (externalFilter != null && externalFilter == "true")
                {
                    filter = FilterProvider.GetFilter(table.Name, fieldName, fieldAlias);
                }

                var field = new FieldDescriptor(
                    fieldName,
                    alias: fieldAlias,
                    function: (string) f.Attribute("function"),
                    filter: filter);

                table.AddField(field);
            }

            return table;
        }
    }
}