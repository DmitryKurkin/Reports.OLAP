namespace Reporting.BusinessLogic
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    /// <summary>
    /// Contains a method that allows to build a <see cref="DataSet"/> based on a <see cref="DataSetDescriptor"/>
    /// </summary>
    public class DataSetBuilder
    {
        /// <summary>
        /// Returns a <see cref="DataSet"/> built based on the specified <see cref="DataSetDescriptor"/>
        /// </summary>
        /// <param name="dataSetDescriptor">The <see cref="DataSetDescriptor"/> to use</param>
        /// <param name="databaseBridge">The <see cref="IDatabaseBridge"/> to use</param>
        /// <returns>A <see cref="DataSet"/> built based on the specified <see cref="DataSetDescriptor"/></returns>
        public static DataSet BuilDataSet(DataSetDescriptor dataSetDescriptor, IDatabaseBridge databaseBridge)
        {
            if (dataSetDescriptor == null) throw new ArgumentNullException(nameof(dataSetDescriptor));
            if (databaseBridge == null) throw new ArgumentNullException(nameof(databaseBridge));

            return new DataSetBuilder(dataSetDescriptor, databaseBridge).Build();
        }

        /// <summary>
        /// The data set descriptor
        /// </summary>
        private readonly DataSetDescriptor _dataSetDescriptor;

        /// <summary>
        /// The database bridge
        /// </summary>
        private readonly IDatabaseBridge _databaseBridge;

        /// <summary>
        /// The data set being built
        /// </summary>
        private DataSet _dataSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSetBuilder"/> class
        /// </summary>
        /// <param name="dataSetDescriptor">The data set descriptor</param>
        /// <param name="databaseBridge">The database bridge</param>
        private DataSetBuilder(DataSetDescriptor dataSetDescriptor, IDatabaseBridge databaseBridge)
        {
            if (dataSetDescriptor == null) throw new ArgumentNullException(nameof(dataSetDescriptor));
            if (databaseBridge == null) throw new ArgumentNullException(nameof(databaseBridge));

            _dataSetDescriptor = dataSetDescriptor;
            _databaseBridge = databaseBridge;
        }

        /// <summary>
        /// Returns a new <see cref="DataSet"/>
        /// </summary>
        /// <returns>A new <see cref="DataSet"/></returns>
        public DataSet Build()
        {
            InjectPrimaryAndForeignKeys();

            FillTables();

            InjectRelations();

            return _dataSet;
        }

        private void InjectPrimaryAndForeignKeys()
        {
            using (var conn = _databaseBridge.CreateConnection(_dataSetDescriptor.ConnectionString))
            {
                conn.Open();

                // add the primary key fields into all the tables
                foreach (var kvp in _dataSetDescriptor.Tables)
                {
                    TryInjectPrimaryKey(_dataSetDescriptor.UserName, conn, kvp.Value);
                }

                // add the foreign key fields into all the tables
                foreach (var childKvp in _dataSetDescriptor.Tables)
                {
                    foreach (var parentKvp in _dataSetDescriptor.Tables)
                    {
                        // include self-referencing tables...
                        if ( /*parentKvp.Key == childKvp.Key || */parentKvp.Value.PrimaryKey == null)
                        {
                            continue;
                        }

                        TryInjectForeignKey(_dataSetDescriptor.UserName, conn, childKvp.Value, parentKvp.Value);
                    }
                }
            }
        }

        private void FillTables()
        {
            _dataSet = new DataSet();

            using (var conn = _databaseBridge.CreateConnection(_dataSetDescriptor.ConnectionString))
            {
                conn.Open();

                // read the data from all the tables
                foreach (var kvp in _dataSetDescriptor.Tables)
                {
                    using (var adapter = _databaseBridge.CreateAdapter(conn, kvp.Value.BuildSql()))
                    {
                        var rows = adapter.Fill(_dataSet, kvp.Key);
                    }
                }
            }

            //dataSet.ReadXmlSchema("c:\\work\\schema_Dimon.xml");
            //dataSet.ReadXml("c:\\work\\dataSet_Dimon.xml");
            //dataSet.WriteXml("c:\\work\\dataSet_my.xml");
            //dataSet.WriteXmlSchema("c:\\work\\schema_my.xml");
        }

        private void InjectRelations()
        {
            // add the relations between the tables
            foreach (var kvp in _dataSetDescriptor.Tables)
            {
                var joinDesc = kvp.Value as JoinDescriptor;

                if (joinDesc != null)
                {
                    foreach (var fkFd in joinDesc.ForeignKeys)
                    {
                        var rel = _dataSet.Relations.Add(
                            _dataSet.Tables[fkFd.ForeignKeyReference.ParentTable.Name].Columns[
                                fkFd.ForeignKeyReference.AliasOrName],
                            _dataSet.Tables[joinDesc.Name].Columns[fkFd.AliasOrName]);
                    }
                }
                else
                {
                    foreach (var fkFd in kvp.Value.ForeignKeys)
                    {
                        var rel = _dataSet.Relations.Add(
                            _dataSet.Tables[fkFd.ForeignKeyReference.ParentTable.Name].Columns[
                                fkFd.ForeignKeyReference.AliasOrName],
                            _dataSet.Tables[fkFd.ParentTable.Name].Columns[fkFd.AliasOrName]);
                    }
                }
            }
        }

        private void TryInjectPrimaryKey(
            string userName,
            IDbConnection conn,
            TableDescriptor table)
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

        private void TryInjectForeignKey(
            string userName,
            IDbConnection conn,
            TableDescriptor childTable,
            TableDescriptor parentTable)
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

        private string GetPrimaryKey(
            IDbConnection connection,
            string userName,
            string tableName)
        {
            string primaryKey = null;

            var selectCmdText = string.Format(
                "SELECT NAME FROM SYSIBM.SYSCOLUMNS WHERE TBCREATOR = '{0}' AND TBNAME = '{1}' AND KEYSEQ > 0",
                userName,
                tableName);
            using (var selectCmd = _databaseBridge.CreateCommand(connection, selectCmdText))
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
            using (var selectCmd = _databaseBridge.CreateCommand(connection, selectCmdText))
            using (var reader = selectCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var foreignKey = (string) reader["FKCOLNAMES"];

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