using System.Collections.Generic;

namespace ORM
{
    public interface IDbConnection
    {
        PostgreSQLConnection Take(string table);
        PostgreSQLConnection Where(KeyValuePair<string, object>[] whereValues);
        PostgreSQLConnection OrderBy(string orderBy, string orderByDirection = "desc");
        PostgreSQLConnection Limit(int limit);
        PostgreSQLConnection Insert(string table, KeyValuePair<string, object>[] insertValues);
        PostgreSQLConnection Update(string table, KeyValuePair<string, object> setValue);
        PostgreSQLConnection Delete(string table);
        PostgreSQLConnection DropTable(string table);
        PostgreSQLConnection CreateTable(string table, params KeyValuePair<string, object>[] columnTypes);
    }
}

