using Contracts.Contracts.Infraestructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace Infraestructure.Manager
{
    public class SqlManagerNested : ISqlManager
    {
        private String _query;

        private String _connString;

        public SqlManagerNested(String connString)
        {
            _connString = connString;
        }

        private T ExecuteQuery<T>(Func<DbCommand, T> callBack, DbParameter[] parameters = default(SqlParameter[]))
        {
            T returnValue = default(T);

            using (var connection = new SqlConnection(_connString))
            {
                connection.Open();

                using (var cmd = new SqlCommand(_query, connection))
                {
                    if (parameters != default(SqlParameter[]))
                        cmd.Parameters.AddRange(parameters);

                    returnValue = callBack.Invoke(cmd);
                }
                connection.Close();
            }

            return returnValue;
        }


        public IEnumerable<T> ExecuteReader<T>(DbParameter[] parameters, String query = default(String)) where T : new()
        {
            Initialize(query);

            List<T> list = ExecuteQuery((command) =>
            {
                var reader = command.ExecuteReader();

                var nestedLines = new NestedQuery().BuildEntities(reader);
                var firstEntityName = nestedLines.Lines.First().LevelName(1);

                var nested = new Nested();
                var nestedEntities = nested.BuildNestedEntities(nestedLines, firstEntityName, null, 1);

                 return nested.BuildEntities(nestedEntities).Cast<T>().ToList();

            }, parameters);

            return list;
        }

        public IEnumerable<T> ExecuteReader<T>(String query = default(String)) where T : new()
        {
            return ExecuteReader<T>(default(SqlParameter[]), query);
        }

        public Int32 ExecuteNonQuery(String query = default(String))
        {
            Initialize(query);

            return ExecuteQuery((comm) => { return comm.ExecuteNonQuery(); });
        }

        public Int32 ExecuteNonQuery(DbParameter[] parameters, String query = default(String))
        {
            Initialize(query);

            return ExecuteQuery((comm) => { return comm.ExecuteNonQuery(); }, parameters);
        }

        public T ExecuteScalar<T>(String query = default(String))
        {
            return ExecuteScalar<T>(null, query);
        }

        public T ExecuteScalar<T>(DbParameter[] parameters, String query = default(String))
        {
            Initialize(query);

            return ExecuteQuery((comm) =>
            {
                var returnValue = comm.ExecuteScalar();

                if (returnValue == DBNull.Value || returnValue == null) return default(T);

                try
                {
                    return (T)returnValue;
                }
                catch
                {
                    return (T)Convert.ChangeType(returnValue, typeof(T));
                }
            }, parameters);
        }

        private void Initialize(String query)
        {
            if (query != default(String))
                _query = query;
        }

        public ISqlManager WithConnString(String connString)
        {
            _connString = connString;

            return this;
        }

        public ISqlManager WithQuery(String query)
        {
            _query = query;

            return this;
        }

        public void ExecuteTransaction(String[] querys, DbParameter[] parameters = null)
        {
            SqlTransaction trans = null;

            try
            {
                SqlConnection connection = new SqlConnection(_connString);

                connection.Open();

                trans = connection.BeginTransaction();

                foreach (var query in querys)
                {
                    SqlCommand command = new SqlCommand(query, connection, trans);

                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    command.ExecuteNonQuery();

                    command.Parameters.Clear();
                }

                trans.Commit();
            }
            catch (Exception ex)
            {
                try
                {
                    if (trans != null) trans.Rollback();
                }
                catch (Exception)
                {

                }

                throw ex;
            }
        }

        public void ExecuteTransaction(Dictionary<String, DbParameter[]> commands)
        {
            SqlTransaction trans = null;

            try
            {
                SqlConnection connection = new SqlConnection(_connString);

                connection.Open();

                trans = connection.BeginTransaction();

                foreach (var command in commands)
                {
                    SqlCommand sqlCommand = new SqlCommand(command.Key, connection, trans);

                    if (commands.Values != null)
                        sqlCommand.Parameters.AddRange(command.Value);

                    sqlCommand.ExecuteNonQuery();

                    sqlCommand.Parameters.Clear();
                }

                trans.Commit();
            }
            catch (Exception ex)
            {
                try
                {
                    if (trans != null) trans.Rollback();
                }
                catch (Exception)
                {

                }

                throw ex;
            }
        }

        public DataTable ExecuteDataTable(DbParameter[] parameters = default(DbParameter[]))
        {
            SqlConnection conn = new SqlConnection(_connString);

            SqlCommand cmd = new SqlCommand(_query, conn);

            if (parameters != default(DbParameter[]))
                cmd.Parameters.AddRange(parameters);

            conn.Open();

            SqlDataAdapter da = new SqlDataAdapter(cmd);

            DataTable table = new DataTable();

            da.Fill(table);

            conn.Close();
            da.Dispose();

            return table;
        }
    }
}
