namespace WindowsFormsControlLibraryRadarSoftCubeCreator
{
    using RadarSoft.Common;
    using RadarSoft.WinForms;
    using RadarSoft.WinForms.Desktop;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using System.Xml.Linq;

    public partial class UserControlRadarSoftCubeCreator : UserControl
    {
        public event EventHandler CubeCreated;

        private readonly IDatabaseBridge _dbBridge;

        public UserControlRadarSoftCubeCreator()
        {
            InitializeComponent();

            _dbBridge = new OdbcDatabaseBridge();
        }

        public TOLAPCube Cube { get; private set; }

        protected virtual void OnCubeCreated()
        {
            var handler = CubeCreated;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                textBoxCubeFilePath.Text = openFileDialog.FileName;
            }
        }

        private void buttonCreate_Click(object sender, EventArgs e)
        {
            if (Cube != null)
            {
                Cube.Dispose();
            }

            var cubeFilePath = textBoxCubeFilePath.Text;

            try
            {
                CreateCube(cubeFilePath);

                OnCubeCreated();
            }
            catch (Exception exc)
            {
                MessageBox.Show(this, exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            if (Cube != null)
            {
                Cube.ShowEditor();

                OnCubeCreated();
            }
        }

        private void CreateCube(string cubeFilePath)
        {
            var contents = File.ReadAllText(cubeFilePath);

            var dataSetDesc = DataSetDescriptorBuilder.Build(contents);

            var doc = XElement.Parse(contents);

            InjectPrimaryAndForeignKeys(
                dataSetDesc,
                (string)doc.Attribute("connectionString"),
                (string)doc.Attribute("userName"));

            var dataSet = FillTables(
                dataSetDesc,
                (string)doc.Attribute("connectionString"),
                (string)doc.Attribute("userName"));

            InjectRelations(dataSetDesc, dataSet);

            Cube = new TOLAPCube();
            Cube.DataSet = dataSet;

            if (checkBoxUseEditor.Checked)
            {
                Cube.ShowEditor();
                return;
            }

            var ns = doc.Name.Namespace;

            // add measures
            foreach (var m in doc.Descendants(ns + "Measure"))
            {
                var measure = Cube.AddMeasure(
                    dataSet.Tables[(string)m.Attribute("sourceTable")],
                    (string)m.Attribute("sourceField"),
                    (string)m.Attribute("displayName"));

                var aggregateFunction = (string)m.Attribute("aggregateFunction");
                if (aggregateFunction != null)
                {
                    measure.AggregateFunction = (TFunction)Enum.Parse(typeof(TFunction), aggregateFunction, true);
                }

                var format = (string)m.Attribute("format");
                if (format != null)
                {
                    measure.DefaultFormat = format;
                }
            }

            // add dimentions
            foreach (var d in doc.Descendants(ns + "Dimention"))
            {
                var dimentionName = (string)d.Attribute("displayName");

                var dimention = Cube.AddDimension(dimentionName);

                // add flat hierarchies
                foreach (var h in d.Elements(ns + "Hierarchy"))
                {
                    CreateHierarchy(dataSetDesc, dataSet, dimentionName, dimention, h);
                }

                // add multi-level hierarchies
                foreach (var m in d.Elements(ns + "Multilevel"))
                {
                    // add sub-flat hierarchies
                    foreach (var h in m.Elements(ns + "Hierarchy"))
                    {
                        var hierarchy = CreateHierarchy(dataSetDesc, dataSet, dimentionName, dimention, h);

                        foreach (var a in h.Elements(ns + "Attribute"))
                        {
                            var displayName = (string)a.Attribute("displayName");
                            var sourceField = (string)a.Attribute("sourceField");

                            var att = new TInfoAttribute
                                    {
                                        DisplayMode = AttributeDispalyMode.AsColumn,
                                        DisplayName = displayName,
                                        SourceField = sourceField,
                                        SourceFieldType = dataSet.Tables[(string)h.Attribute("sourceTable")].Columns[sourceField].DataType
                                    };
                            hierarchy.InfoAttributes.Add(att);
                        }
                        
                        Cube.MakeUpCompositeHierarchy(dimentionName, (string)m.Attribute("name"), hierarchy);
                    }
                }
            }
        }

        private TCubeHierarchy CreateHierarchy(
            DataSetDescriptor dataSetDesc,
            DataSet dataSet,
            string dimentionName,
            TCubeDimension dimention,
            XElement h)
        {
            TCubeHierarchy hierarchy;

            var sourceTable = (string)h.Attribute("sourceTable");
            var sourceField = (string)h.Attribute("sourceField");
            var displayName = (string)h.Attribute("displayName");

            if (dataSet.Tables[sourceTable].Columns[sourceField].DataType == typeof(DateTime))
            {
                #region Add a BI hierarchy

                var hierarchyYear = Cube.AddBIHierarchy(
                                    dimention.DisplayName,
                                    dataSet.Tables[sourceTable],
                                    displayName + ": Year",
                                    sourceField,
                                    TBIMembersType.ltTimeYear);
                var hierarchyQuarter = Cube.AddBIHierarchy(
                    dimention.DisplayName,
                    dataSet.Tables[sourceTable],
                    displayName + ": Quarter",
                    sourceField,
                    TBIMembersType.ltTimeQuarter);
                var hierarchyMonth = Cube.AddBIHierarchy(
                    dimention.DisplayName,
                    dataSet.Tables[sourceTable],
                    displayName + ": Month",
                    sourceField,
                    TBIMembersType.ltTimeMonthLong);
                var hierarchyDay = Cube.AddBIHierarchy(
                    dimention.DisplayName,
                    dataSet.Tables[sourceTable],
                    displayName + ": Day",
                    sourceField,
                    TBIMembersType.ltTimeDayOfMonth);

                hierarchy = Cube.MakeUpCompositeHierarchy(
                    dimentionName,
                    displayName + ": Year-Quarter-Month-Day",
                    new List<TCubeHierarchy> { hierarchyYear, hierarchyQuarter, hierarchyMonth, hierarchyDay });

                #endregion
            }
            else
            {
                #region Add either a Parent-Child or a simple hierarchy

                var isSelfReference = (string)h.Attribute("selfReference") == "true";

                if (isSelfReference)
                {
                    var parentField = dataSetDesc.Tables[sourceTable].ForeignKeys.Single(
                        fd => fd.ForeignKeyReference.ParentTable.Name == sourceTable).AliasOrName;

                    hierarchy = Cube.AddHierarchy(
                        dimention.DisplayName,
                        dataSet.Tables[sourceTable],
                        sourceField,
                        parentField,
                        displayName);

                    hierarchy.IDField = dataSetDesc.Tables[sourceTable].PrimaryKey.AliasOrName;
                    hierarchy.IDFieldType = dataSet.Tables[sourceTable].Columns[hierarchy.IDField].DataType;
                }
                else
                {
                    hierarchy = Cube.AddHierarchy(
                        dimention.DisplayName,
                        dataSet.Tables[sourceTable],
                        sourceField,
                        null,
                        displayName);
                }

                #endregion
            }

            return hierarchy;
        }

        private void InjectPrimaryAndForeignKeys(DataSetDescriptor dataSetDesc, string connectionString, string userName)
        {
            using (var conn = _dbBridge.CreateConnection(connectionString))
            {
                conn.Open();

                // add the primary key fields into all the tables
                foreach (var kvp in dataSetDesc.Tables)
                {
                    TryInjectPrimaryKey(userName, conn, kvp.Value);
                }

                // add the foreign key fields into all the tables
                foreach (var childKvp in dataSetDesc.Tables)
                {
                    foreach (var parentKvp in dataSetDesc.Tables)
                    {
                        // include self-referencing tables...
                        if (/*parentKvp.Key == childKvp.Key || */parentKvp.Value.PrimaryKey == null)
                        {
                            continue;
                        }

                        TryInjectForeignKey(userName, conn, childKvp.Value, parentKvp.Value);
                    }
                }
            }
        }

        private DataSet FillTables(DataSetDescriptor dataSetDesc, string connectionString, string userName)
        {
            var dataSet = new DataSet();

            using (var conn = _dbBridge.CreateConnection(connectionString))
            {
                conn.Open();

                // read the data from all the tables
                foreach (var kvp in dataSetDesc.Tables)
                {
                    using (var adapter = _dbBridge.CreateAdapter(conn, kvp.Value.BuildSql()))
                    {
                        var rows = adapter.Fill(dataSet, kvp.Key);
                    }
                }
            }

            //dataSet.ReadXmlSchema("c:\\work\\schema_Dimon.xml");
            //dataSet.ReadXml("c:\\work\\dataSet_Dimon.xml");
            //dataSet.WriteXml("c:\\work\\dataSet_my.xml");
            //dataSet.WriteXmlSchema("c:\\work\\schema_my.xml");

            return dataSet;
        }

        private static void InjectRelations(DataSetDescriptor dataSetDesc, DataSet dataSet)
        {
            // add the relations between the tables
            foreach (var kvp in dataSetDesc.Tables)
            {
                var joinDesc = kvp.Value as JoinDescriptor;

                if (joinDesc != null)
                {
                    foreach (var fkFd in joinDesc.ForeignKeys)
                    {
                        var rel = dataSet.Relations.Add(
                            dataSet.Tables[fkFd.ForeignKeyReference.ParentTable.Name].Columns[fkFd.ForeignKeyReference.AliasOrName],
                            dataSet.Tables[joinDesc.Name].Columns[fkFd.AliasOrName]);
                    }
                }
                else
                {
                    foreach (var fkFd in kvp.Value.ForeignKeys)
                    {
                        var rel = dataSet.Relations.Add(
                            dataSet.Tables[fkFd.ForeignKeyReference.ParentTable.Name].Columns[fkFd.ForeignKeyReference.AliasOrName],
                            dataSet.Tables[fkFd.ParentTable.Name].Columns[fkFd.AliasOrName]);
                    }
                }
            }
        }

        private void TryInjectPrimaryKey(string userName, IDbConnection conn, TableDescriptor table)
        {
            var joinDesc = table as JoinDescriptor;

            // joins use the left table as the source of PKs...
            var primaryKey = GetPrimaryKey(conn, userName, joinDesc != null ? joinDesc.LeftTable.Name : table.Name);

            if (primaryKey != null)
            {
                var fd = new FieldDescriptor(primaryKey, isPrimaryKey: true);

                if (joinDesc != null)
                {
                    joinDesc.LeftTable.AddField(fd);
                }
                else
                {
                    table.AddField(fd);
                }
            }
        }

        private void TryInjectForeignKey(string userName, IDbConnection conn, TableDescriptor childTable, TableDescriptor parentTable)
        {
            // Right tables in joins have NO primary keys (PKs in such tables are not taken into accout)!!!

            var childJoin = childTable as JoinDescriptor;
            var parentJoin = parentTable as JoinDescriptor;

            if (childJoin == null && parentJoin == null)
            {
                // a relation from a table into a table...

                var fkFromTableToTable = GetForeignKey(
                    conn,
                    userName,
                    childTable.Name,
                    parentTable.Name,
                    childTable.SuppressForeignKeys);

                if (fkFromTableToTable != null)
                {
                    var fd = new FieldDescriptor(
                        fkFromTableToTable,
                        alias: string.Format("{0}_FK", fkFromTableToTable),
                        foreignKeyReference: parentTable.PrimaryKey);

                    childTable.AddField(fd);
                }
            }
            else if (childJoin == null)
            {
                // a relation from a table into a join (into its LEFT table!)...

                var fkFromTableToJoinLeftTable = GetForeignKey(
                    conn,
                    userName,
                    childTable.Name,
                    parentJoin.LeftTable.Name,
                    childTable.SuppressForeignKeys);

                if (fkFromTableToJoinLeftTable != null)
                {
                    var fd = new FieldDescriptor(
                        fkFromTableToJoinLeftTable,
                        alias: string.Format("{0}_FK", fkFromTableToJoinLeftTable),
                        foreignKeyReference: parentJoin.PrimaryKey);

                    childTable.AddField(fd);
                }

                // NO FKs to the JOIN RIGHT TABLE because it won't have its PK in the result table (as already mentioned)!
            }
            else if (parentJoin == null)
            {
                // a relation from a join (from both LEFT and RIGHT tables) into a table...

                var fkFromJoinLeftTableToTable = GetForeignKey(
                    conn,
                    userName,
                    childJoin.LeftTable.Name,
                    parentTable.Name,
                    childJoin.LeftTable.SuppressForeignKeys);

                if (fkFromJoinLeftTableToTable != null)
                {
                    var fd = new FieldDescriptor(
                        fkFromJoinLeftTableToTable,
                        alias: string.Format("{0}_FK", fkFromJoinLeftTableToTable),
                        foreignKeyReference: parentTable.PrimaryKey);

                    childJoin.LeftTable.AddField(fd);
                }

                var fkFromJoinRightTableToTable = GetForeignKey(
                    conn,
                    userName,
                    childJoin.RightTable.Name,
                    parentTable.Name,
                    childJoin.RightTable.SuppressForeignKeys);

                if (fkFromJoinRightTableToTable != null)
                {
                    var fd = new FieldDescriptor(
                        fkFromJoinRightTableToTable,
                        alias: string.Format("{0}_FK", fkFromJoinRightTableToTable),
                        foreignKeyReference: parentTable.PrimaryKey);

                    childJoin.RightTable.AddField(fd);
                }
            }
            else
            {
                // a relation from a join (from both LEFT and RIGHT tables) into a join (into its LEFT table!)...

                var fkFromJoinLeftTableToJoinLeftTable = GetForeignKey(
                    conn,
                    userName,
                    childJoin.LeftTable.Name,
                    parentJoin.LeftTable.Name,
                    childJoin.LeftTable.SuppressForeignKeys);

                if (fkFromJoinLeftTableToJoinLeftTable != null)
                {
                    var fd = new FieldDescriptor(
                        fkFromJoinLeftTableToJoinLeftTable,
                        alias: string.Format("{0}_FK", fkFromJoinLeftTableToJoinLeftTable),
                        foreignKeyReference: parentJoin.PrimaryKey);

                    childJoin.LeftTable.AddField(fd);
                }

                var fkFromJoinRightTableToJoinLeftTable = GetForeignKey(
                    conn,
                    userName,
                    childJoin.RightTable.Name,
                    parentJoin.LeftTable.Name,
                    childJoin.RightTable.SuppressForeignKeys);

                if (fkFromJoinRightTableToJoinLeftTable != null)
                {
                    var fd = new FieldDescriptor(
                        fkFromJoinRightTableToJoinLeftTable,
                        alias: string.Format("{0}_FK", fkFromJoinRightTableToJoinLeftTable),
                        foreignKeyReference: parentJoin.PrimaryKey);

                    childJoin.RightTable.AddField(fd);
                }

                // NO FKs to the JOIN RIGHT TABLE because it won't have its PK in the result table (as already mentioned)!
            }
        }

        private string GetPrimaryKey(IDbConnection connection, string userName, string tableName)
        {
            string primaryKey = null;

            var selectCmdText = string.Format(
                "SELECT NAME FROM SYSIBM.SYSCOLUMNS WHERE TBCREATOR = '{0}' AND TBNAME = '{1}' AND KEYSEQ > 0",
                userName,
                tableName);
            using (var selectCmd = _dbBridge.CreateCommand(connection, selectCmdText))
            {
                primaryKey = selectCmd.ExecuteScalar() as string;

                if (primaryKey != null)
                {
                    primaryKey = primaryKey.Trim(' ');
                }
            }

            return primaryKey;
        }

        private string GetForeignKey(
            IDbConnection connection,
            string userName,
            string childTableName,
            string parentTableName,
            IEnumerable<string> suppressedForeignKeys)
        {
            var allForeignKeys = new List<string>();

            var selectCmdText = string.Format(
                "SELECT FKCOLNAMES FROM SYSIBM.SYSRELS WHERE CREATOR = '{0}' AND TBNAME = '{1}' AND REFTBNAME = '{2}'",
                userName,
                childTableName,
                parentTableName);
            using (var selectCmd = _dbBridge.CreateCommand(connection, selectCmdText))
            using (var reader = selectCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var foreignKey = (string)reader["FKCOLNAMES"];

                    if (foreignKey != null)
                    {
                        foreignKey = foreignKey.Trim(' ');
                        allForeignKeys.Add(foreignKey);
                    }
                }
            }

            var resultKeys = allForeignKeys.Except(suppressedForeignKeys).ToArray();

            if (resultKeys.Length == 0)
            {
                return null;
            }

            if (resultKeys.Length == 1)
            {
                return resultKeys[0];
            }

            throw new InvalidOperationException(
                string.Format("Multiple foreign keys found after a query: {0}", selectCmdText));
        }
    }
}