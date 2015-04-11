namespace WindowsFormsControlLibraryRadarSoftCubeCreator
{
    using System.Data;
    using System.Data.Common;

    public interface IDatabaseBridge
    {
        IDbConnection CreateConnection(string connectionString);

        IDbCommand CreateCommand(IDbConnection connection, string commandText);

        DbDataAdapter CreateAdapter(IDbConnection connection, string selectCommandText);
    }
}