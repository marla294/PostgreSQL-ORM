using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;

namespace ORM
{
    public static class Pairing
    {
        public static KeyValuePair<string, object> Of(string Column, object Value)
        {
            return new KeyValuePair<string, object>(Column, Value);
        }
    }

    public class PostgreSQLConnection : IDbConnection
    {
        private string ConnectionString { get; set; }

        private string SQL { get; set; }
        private bool IsQuery { get; set; }
        private Dictionary<int, object> Parameters { get; set; }

        public PostgreSQLConnection()
        {
            ConnectionString = "Host=127.0.0.1;Port=5433;Username=postgres; Password=Password1;Database=booklist";
            ResetFields();
        }

        private void ResetFields()
        {
            SQL = "";
            Parameters = new Dictionary<int, object>();
            IsQuery = false;
        }

        // Starting place
        public PostgreSQLConnection Take(string table)
        {
            IsQuery = true;

            var sql = $"select * from {table}";

            SQL = sql;

            return this;
        }

        public PostgreSQLConnection Where(params KeyValuePair<string, object>[] whereValues)
        {
            var parameterStart = Parameters.Count;
            var sql = SQL + $" where {whereValues[0].Key} = @parameter{(parameterStart + 1).ToString()}";
            Parameters.Add(parameterStart, whereValues[0].Value);

            for (var i = 1; i < whereValues.Length; i++)
            {
                var whereValue = whereValues[i];
                string snippet = $" and {whereValue.Key} = @parameter{(parameterStart + i + 1).ToString()}";
                sql = sql + snippet;
                Parameters.Add(parameterStart + i, whereValue.Value);
            }

            SQL = sql;

            return this;
        }

        public PostgreSQLConnection OrderBy(string orderBy, string orderByDirection = "desc")
        {
            SQL = SQL + $" order by {orderBy} {orderByDirection}";

            return this;
        }

        public PostgreSQLConnection Limit(int limit)
        {
            SQL = SQL + $" limit {limit}";

            return this;
        }

        // Starting place
        public PostgreSQLConnection Insert(string table, params KeyValuePair<string, object>[] insertValues)
        {
            var columns = $"{insertValues[0].Key}";
            var values = new List<object> { insertValues[0].Value };
            var parameters = "@parameter1";

            Parameters.Add(0, insertValues[0].Value);

            for (var i = 1; i < insertValues.Length; i++)
            {
                var insertValue = insertValues[i];

                columns = columns + $", {insertValue.Key}";
                values.Add(insertValue.Value);
                parameters = parameters + $", @parameter{(i + 1).ToString()}";

                Parameters.Add(i, insertValue.Value);
            }

            SQL = $"insert into {table} ({columns}) values ({parameters})";

            return this;
        }

        // Starting place
        public PostgreSQLConnection Update(string table, KeyValuePair<string, object> setValue)
        {
            SQL = $"update {table} set {setValue.Key} = @parameter1";
            Parameters.Add(0, setValue.Value);

            return this;
        }

        // Starting place
        public PostgreSQLConnection Delete(string table)
        {
            SQL = $"delete from {table}";

            return this;
        }

        // Starting place
        public PostgreSQLConnection CreateTable(string table, params KeyValuePair<string, object>[] columnTypes)
        {
            var result = Take("information_schema.tables").Where(Pairing.Of("table_name", $"{table}")).Execute();

            if (result[0].Count == 0)
            {
                SQL = $"create table {table}();" +
                      $" alter table {table} add column id bigserial primary key;";

                foreach (var columnType in columnTypes)
                {
                    SQL = SQL + $" alter table {table} add column {columnType.Key} {columnType.Value.ToString()};";
                }
            }

            return this;
        }

        // Starting place
        public PostgreSQLConnection DropTable(string table)
        {
            var result = Take("information_schema.tables").Where(Pairing.Of("table_name", $"{table}")).Execute();

            // Table is in the database so we can delete it
            if (result[0].Count > 0)
            {
                SQL = $"drop table {table}";
                IsQuery = false;
            }

            return this;
        }

        public List<List<string>> Execute()
        {
            var parameters = Parameters.Values.ToArray<object>();
            var results = ConnectionUtils.CreateEmptyResultSet(0);

            using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                using (var cmd = AddParameters(new NpgsqlCommand(SQL, connection), parameters))
                {
                    try
                    {
                        cmd.Prepare();

                        if (IsQuery)
                        {
                            results = ReadDBResults(cmd.ExecuteReader());
                        }
                        else
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch
                    {
                        throw new Exception("Something went wrong with your sql statement, please try again.");
                    }
                }

                connection.Close();

                ResetFields();

                return results;
            }
        }

        private NpgsqlCommand AddParameters(NpgsqlCommand cmd, params object[] parameters)
        {
            for (var i = 0; i < parameters.Length; i++)
            {
                var param = $"@parameter{(i + 1).ToString()}";

                if (parameters[i] is int)
                {
                    cmd.Parameters.AddWithValue(param, (int)parameters[i]);
                }
                else if (parameters[i] is string)
                {
                    cmd.Parameters.AddWithValue(param, (string)parameters[i]);
                }
                else if (parameters[i] is bool)
                {
                    cmd.Parameters.AddWithValue(param, (bool)parameters[i]);
                }
                else if (parameters[i] is null)
                {
                    cmd.Parameters.AddWithValue(param, DBNull.Value);
                }
            }

            return cmd;
        }

        private List<List<string>> ReadDBResults(NpgsqlDataReader dataReader)
        {
            List<List<string>> results = ConnectionUtils.CreateEmptyResultSet(dataReader.FieldCount);

            while (dataReader.Read())
            {
                for (var col = 0; col < dataReader.FieldCount; col++)
                {
                    results[col].Add(dataReader[col].ToString());
                }
            }

            return results;
        }
    }
}
