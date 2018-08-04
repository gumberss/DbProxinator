using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Contracts.Contracts.Infraestructure
{
    public interface ISqlManager
    {
        IEnumerable<T> ExecuteReader<T>(String query = default(String)) where T : new();

        IEnumerable<T> ExecuteReader<T>(DbParameter[] parameters, String query = default(String)) where T : new();

         DataTable ExecuteDataTable(DbParameter[] parameters = default(DbParameter[]));

        Int32 ExecuteNonQuery(String query = default(String));

        Int32 ExecuteNonQuery(DbParameter[] parameters, String query = default(String));

        T ExecuteScalar<T>(String query = default(String));

        T ExecuteScalar<T>(DbParameter[] parameters, String query = default(String));

        ISqlManager WithConnString(String connString);

        ISqlManager WithQuery(String query);

        void ExecuteTransaction(String[] querys, DbParameter[] parameters = null);

        void ExecuteTransaction(Dictionary<String, DbParameter[]> commands);
    }
}
