namespace Reporting.BusinessLogic
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// Contains a method that allows to build a <see cref="DataSetDescriptor"/>
    /// </summary>
    public static class DataSetDescriptorBuilder
    {
        /// <summary>
        /// The major version of the XML format
        /// </summary>
        private const int MajorVersion = 0;

        /// <summary>
        /// The minor version of the XML format
        /// </summary>
        private const int MinorVersion = 17;

        /// <summary>
        /// Gets or sets an <see cref="IExternalFilterProvider"/> to use when building
        /// </summary>
        public static IExternalFilterProvider FilterProvider { get; set; }

        /// <summary>
        /// Returns a new <see cref="DataSetDescriptor"/>
        /// </summary>
        /// <param name="contents">The contents to use to build</param>
        /// <returns>A new <see cref="DataSetDescriptor"/></returns>
        public static DataSetDescriptor Build(string contents)
        {
            if (contents == null) throw new ArgumentNullException(nameof(contents));
            if (string.IsNullOrWhiteSpace(contents))
                throw new ArgumentException("The contents cannot be empty", nameof(contents));

            // TODO: move the property to the signature
            if (FilterProvider == null)
            {
                throw new InvalidOperationException("The filter provider is not set");
            }

            var doc = XElement.Parse(contents);

            ValidateVersion(doc);

            var dataSetDesc = CreateDataSetDescriptor(doc);

            return dataSetDesc;
        }

        /// <summary>
        /// Validates the version of the specified XML document
        /// </summary>
        /// <param name="doc">The XML document to validate</param>
        private static void ValidateVersion(XElement doc)
        {
            var versionMajorAttr = (string) doc.Attribute("versionMajor");
            if (versionMajorAttr == null)
                throw new InvalidOperationException("Attribute 'versionMajor' is missing in element 'Cube'");

            var versionMajor = int.Parse(versionMajorAttr, new CultureInfo("en-US"));
            if (versionMajor != MajorVersion)
                throw new InvalidDataException($"The major version ({versionMajor}) is invalid");

            var versionMinorAttr = (string) doc.Attribute("versionMinor");
            if (versionMinorAttr == null)
                throw new InvalidOperationException("Attribute 'versionMinor' is missing in element 'Cube'");

            var versionMinor = int.Parse(versionMinorAttr, new CultureInfo("en-US"));
            if (versionMinor != MinorVersion)
                throw new InvalidDataException($"The minor version ({versionMinor}) is invalid");
        }

        /// <summary>
        /// Returns a <see cref="DataSetDescriptor"/> read from the specified XML document
        /// </summary>
        /// <param name="doc">The XML document to read a <see cref="DataSetDescriptor"/> from</param>
        /// <returns>A <see cref="DataSetDescriptor"/> read from the specified XML document</returns>
        private static DataSetDescriptor CreateDataSetDescriptor(XElement doc)
        {
            var connectionString = (string) doc.Attribute("connectionString");
            if (connectionString == null)
                throw new InvalidOperationException("Attribute 'connectionString' is missing in element 'Cube'");

            var userName = (string) doc.Attribute("userName");
            if (userName == null)
                throw new InvalidOperationException("Attribute 'userName' is missing in element 'Cube'");

            var dataSetDesc = new DataSetDescriptor(connectionString, userName);

            var ns = doc.Name.Namespace;

            var dataSetTag = doc.Element(ns + "DataSet");
            if (dataSetTag == null)
                throw new InvalidOperationException("Element 'DataSet' is missing in element 'Cube'");

            foreach (var mj in dataSetTag.Elements(ns + "MultiJoin"))
            {
                var multiJoin = ReadMultiJoinDescriptor(ns, mj);

                dataSetDesc.AddTable(multiJoin);
            }

            foreach (var t in dataSetTag.Elements(ns + "Table"))
            {
                var table = ReadTableDescriptor(ns, t);

                dataSetDesc.AddTable(table);
            }

            ValidateReferences(dataSetDesc);

            return dataSetDesc;
        }

        /// <summary>
        /// Validates the 'references' attributes of the specified <see cref="DataSetDescriptor"/>
        /// </summary>
        /// <param name="dataSetDescriptor">The <see cref="DataSetDescriptor"/> to validate</param>
        private static void ValidateReferences(DataSetDescriptor dataSetDescriptor)
        {
            foreach (var kvp in dataSetDescriptor.Tables)
            {
                var multijoin = kvp.Value as MultiJoinDescriptor;
                if (multijoin != null)
                {
                    foreach (var td in multijoin.Tables)
                    {
                        foreach (var fd in td.GetForeignKeys())
                        {
                            if (!dataSetDescriptor.Tables.ContainsKey(fd.References))
                                throw new InvalidOperationException(
                                    $"Field '{fd}' of table '{td}' in multijoin '{multijoin.Name}' references table '{fd.References}' which does not exist");

                            if (dataSetDescriptor.Tables[fd.References].GetPrimaryKey() == null)
                                throw new InvalidOperationException(
                                    $"Field '{fd}' of table '{td}' in multijoin '{multijoin.Name}' references table '{fd.References}' which does not have the PK");
                        }
                    }

                    //var mainTable = multijoin
                    //    .Tables
                    //    .SingleOrDefault(
                    //        td => td.Name.Equals(multijoin.PrimaryKey, StringComparison.OrdinalIgnoreCase));
                    //if (mainTable == null)
                    //    throw new InvalidOperationException(
                    //        $"Table '{multijoin.PrimaryKey}' does not exist in multijoin '{multijoin.Name}' to use its PK");

                    //if (mainTable.GetPrimaryKey() == null)
                    //    throw new InvalidOperationException(
                    //        $"Table '{mainTable}' does not have the PK to use it in multijoin '{multijoin.Name}'");
                    if (multijoin
                        .Tables
                        .SelectMany(td => td.Fields.Values)
                        .Any(fd => fd.AliasOrName.Equals(multijoin.PrimaryKey, StringComparison.OrdinalIgnoreCase)))
                        throw new InvalidOperationException(
                            $"PK column name '{multijoin.PrimaryKey}' in multijoin '{multijoin.Name}' causes a name conflict");
                }
                else
                {
                    foreach (var fd in kvp.Value.GetForeignKeys())
                    {
                        if (!dataSetDescriptor.Tables.ContainsKey(fd.References))
                            throw new InvalidOperationException(
                                $"Field '{fd}' of table '{kvp.Value.Name}' references table '{fd.References}' which does not exist");

                        if (dataSetDescriptor.Tables[fd.References].GetPrimaryKey() == null)
                            throw new InvalidOperationException(
                                $"Field '{fd}' of table '{kvp.Value.Name}' references table '{fd.References}' which does not have the PK");
                    }
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="TableDescriptor"/> read from the specified XML element
        /// </summary>
        /// <param name="ns">The XML namespace to use when reading</param>
        /// <param name="e">The XML element to read a <see cref="TableDescriptor"/> from</param>
        /// <returns>A <see cref="TableDescriptor"/> read from the specified XML element</returns>
        private static TableDescriptor ReadTableDescriptor(XNamespace ns, XElement e)
        {
            var tableName = (string) e.Attribute("name");
            if (tableName == null)
                throw new InvalidOperationException("Attribute 'name' is missing in the table definition");

            var table = new TableDescriptor(tableName, (string) e.Attribute("filter"));

            var primaryKeyName = (string) e.Attribute("primaryKey");
            if (primaryKeyName != null)
            {
                table.AddField(new FieldDescriptor(primaryKeyName));
            }

            foreach (var f in e.Descendants(ns + "Field"))
            {
                var fieldName = (string) f.Attribute("name");
                if (fieldName == null)
                    throw new InvalidOperationException(
                        $"Attribute 'name' is missing in the field definition of table {tableName}");

                var fieldAlias = (string) f.Attribute("alias");

                string filter = null;

                var externalFilter = (string) f.Attribute("externalFilter");
                if (externalFilter != null && externalFilter == "true")
                {
                    filter = FilterProvider.GetFilter(table.Name, fieldName, fieldAlias);
                }

                var field = new FieldDescriptor(
                    fieldName,
                    fieldAlias,
                    (string) f.Attribute("function"),
                    filter,
                    (string) f.Attribute("references"));

                table.AddField(field);
            }

            return table;
        }

        /// <summary>
        /// Returns a <see cref="MultiJoinDescriptor"/> read from the specified XML element
        /// </summary>
        /// <param name="ns">The XML namespace to use when reading</param>
        /// <param name="e">The XML element to read a <see cref="MultiJoinDescriptor"/> from</param>
        /// <returns>A <see cref="MultiJoinDescriptor"/> read from the specified XML element</returns>
        private static MultiJoinDescriptor ReadMultiJoinDescriptor(XNamespace ns, XElement e)
        {
            var tablesTag = e.Element(ns + "Tables");
            if (tablesTag == null)
                throw new InvalidOperationException("Element 'Tables' is missing in the multijoin definition");

            var linksTag = e.Element(ns + "Links");
            if (linksTag == null)
                throw new InvalidOperationException("Element 'Links' is missing in the multijoin definition");

            var tables = tablesTag.Elements(ns + "Table").Select(t => ReadTableDescriptor(ns, t));
            var links = linksTag.Elements(ns + "Link").Select(ReadLinkDescriptor);

            var alias = (string) e.Attribute("alias");
            if (alias == null)
                throw new InvalidOperationException("Attribute 'alias' is missing in the multijoin definition");

            var primaryKey = (string) e.Attribute("primaryKey");
            if (primaryKey == null)
                throw new InvalidOperationException("Attribute 'primaryKey' is missing in the multijoin definition");

            var maxRows = 0;
            var maxRowsStr = (string) e.Attribute("maxRows");
            if (maxRowsStr != null)
            {
                int.TryParse(maxRowsStr, out maxRows);
            }

            var multiJoin = new MultiJoinDescriptor(alias, primaryKey, maxRows, tables, links);

            return multiJoin;
        }

        /// <summary>
        /// Returns a <see cref="LinkDescriptor"/> read from the specified XML element
        /// </summary>
        /// <param name="e">The XML element to read a <see cref="LinkDescriptor"/> from</param>
        /// <returns>A <see cref="LinkDescriptor"/> read from the specified XML element</returns>
        private static LinkDescriptor ReadLinkDescriptor(XElement e)
        {
            var fromKey = (string) e.Attribute("fromKey");
            var toKey = (string) e.Attribute("toKey");

            var link = fromKey != null && toKey != null
                ? new LinkDescriptor(
                    (string) e.Attribute("table"),
                    fromKey,
                    toKey)
                : new LinkDescriptor((string) e.Attribute("table"));

            return link;
        }
    }
}