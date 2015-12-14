namespace Reporting.BusinessLogic
{
    using System;
    using System.Data;

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
            FillDataSet();

            InjectPrimaryKeys();

            InjectRelations();

            return _dataSet;
        }

        /// <summary>
        /// Fills the data set
        /// </summary>
        private void FillDataSet()
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
                        adapter.Fill(_dataSet, kvp.Key);
                    }
                }
            }
        }

        /// <summary>
        /// Injects PKs into the tables
        /// </summary>
        private void InjectPrimaryKeys()
        {
            foreach (var kvp in _dataSetDescriptor.Tables)
            {
                var currTable = _dataSet.Tables[kvp.Key];

                var multijoin = kvp.Value as MultiJoinDescriptor;

                if (multijoin != null)
                {
                    currTable.PrimaryKey = new[] {currTable.Columns[multijoin.PrimaryKey]};
                }
                else
                {
                    var pkDef = kvp.Value.GetPrimaryKey();

                    if (pkDef != null)
                    {
                        currTable.PrimaryKey = new[] {currTable.Columns[pkDef.Name]};
                    }
                }
            }
        }

        /// <summary>
        /// Injects FK-to-PK relations into the data set
        /// </summary>
        private void InjectRelations()
        {
            foreach (var kvp in _dataSetDescriptor.Tables)
            {
                var multijoin = kvp.Value as MultiJoinDescriptor;

                if (multijoin != null)
                {
                    foreach (var td in multijoin.Tables)
                    {
                        foreach (var fkFd in td.GetForeignKeys())
                        {
                            _dataSet.Relations.Add(
                                _dataSet.Tables[fkFd.References].PrimaryKey[0],
                                _dataSet.Tables[multijoin.Name].Columns[fkFd.AliasOrName]);
                        }
                    }
                }
                else
                {
                    foreach (var fkFd in kvp.Value.GetForeignKeys())
                    {
                        _dataSet.Relations.Add(
                            _dataSet.Tables[fkFd.References].PrimaryKey[0],
                            _dataSet.Tables[kvp.Key].Columns[fkFd.AliasOrName]);
                    }
                }
            }
        }
    }
}