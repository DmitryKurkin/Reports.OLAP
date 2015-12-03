namespace Reporting.PresentationLogic.WpfApplication
{
    using System.Data;
    using System.Data.Common;
    using System.Data.Odbc;

    using Reporting.BusinessLogic;

    public class OdbcDatabaseBridge : IDatabaseBridge
    {
        public IDbConnection CreateConnection(string connectionString)
        {
            return new OdbcConnection(connectionString);
        }

        public IDbCommand CreateCommand(IDbConnection connection, string commandText)
        {
            return new OdbcCommand(commandText, (OdbcConnection)connection);
        }

        public DbDataAdapter CreateAdapter(IDbConnection connection, string selectCommandText)
        {
            return new OdbcDataAdapter(selectCommandText, (OdbcConnection)connection);
        }
    }
}
