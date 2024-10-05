using System;
using System.Data;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;

namespace DatabaseOperations
{
    public class DbParameter
    {
        public string ParameterName { get; set; }
        public object Value { get; set; }
        public MySqlDbType DbType { get; set; }
        public ParameterDirection Direction { get; set; }
        public int? Size { get; set; }

        public DbParameter(string parameterName, object value, MySqlDbType dbType, 
            ParameterDirection direction = ParameterDirection.Input, int? size = null)
        {
            ParameterName = parameterName;
            Value = value;
            DbType = dbType;
            Direction = direction;
            Size = size;
        }
    }

    public static class DbParameterExtensions
    {
        public static DbParameter CreateParameter(this string parameterName, object value, MySqlDbType dbType,
            ParameterDirection direction = ParameterDirection.Input, int? size = null)
        {
            return new DbParameter(parameterName, value, dbType, direction, size);
        }
    }

    public class DbOperations : IDisposable
    {
        private readonly string _connectionString;
        private MySqlConnection _connection;
        private MySqlTransaction _transaction;
        private bool _disposed = false;

        public DbOperations(string connectionString)
        {
            _connectionString = connectionString;
        }

        private void CreateConnection()
        {
            try
            {
                if (_connection == null)
                {
                    _connection = new MySqlConnection(_connectionString);
                }
                
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create database connection.", ex);
            }
        }

        private void AddParametersToCommand(MySqlCommand cmd, Dictionary<string, DbParameter> parameters)
        {
            if (parameters == null) return;

            foreach (var param in parameters)
            {
                MySqlParameter sqlParam = cmd.Parameters.Add(param.Key, param.Value.DbType);
                sqlParam.Value = param.Value.Value ?? DBNull.Value;
                sqlParam.Direction = param.Value.Direction;
                
                if (param.Value.Size.HasValue)
                {
                    sqlParam.Size = param.Value.Size.Value;
                }
            }
        }

        public void BeginTransaction()
        {
            try
            {
                CreateConnection();
                _transaction = _connection.BeginTransaction();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to begin transaction.", ex);
            }
        }

        public void CommitTransaction()
        {
            try
            {
                _transaction?.Commit();
                _transaction = null;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to commit transaction.", ex);
            }
        }

        public void RollbackTransaction()
        {
            try
            {
                _transaction?.Rollback();
                _transaction = null;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to rollback transaction.", ex);
            }
        }

        public DataSet ExecuteStoredProcedureDataSet(string procedureName, Dictionary<string, DbParameter> parameters = null)
        {
            try
            {
                CreateConnection();
                using (MySqlCommand cmd = new MySqlCommand(procedureName, _connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (_transaction != null)
                        cmd.Transaction = _transaction;

                    AddParametersToCommand(cmd, parameters);

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapter.Fill(ds);

                        // Handle output parameters if any
                        if (parameters != null)
                        {
                            foreach (var param in parameters.Values)
                            {
                                if (param.Direction == ParameterDirection.Output || 
                                    param.Direction == ParameterDirection.InputOutput)
                                {
                                    param.Value = cmd.Parameters[param.ParameterName].Value;
                                }
                            }
                        }

                        return ds;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to execute stored procedure: {procedureName}", ex);
            }
        }

        public DataTable ExecuteStoredProcedureDataTable(string procedureName, Dictionary<string, DbParameter> parameters = null)
        {
            try
            {
                DataSet ds = ExecuteStoredProcedureDataSet(procedureName, parameters);
                return ds.Tables.Count > 0 ? ds.Tables[0] : new DataTable();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to execute stored procedure: {procedureName}", ex);
            }
        }

        public string ExecuteStoredProcedureScalar(string procedureName, Dictionary<string, DbParameter> parameters = null)
        {
            try
            {
                CreateConnection();
                using (MySqlCommand cmd = new MySqlCommand(procedureName, _connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (_transaction != null)
                        cmd.Transaction = _transaction;

                    AddParametersToCommand(cmd, parameters);

                    object result = cmd.ExecuteScalar();

                    // Handle output parameters if any
                    if (parameters != null)
                    {
                        foreach (var param in parameters.Values)
                        {
                            if (param.Direction == ParameterDirection.Output || 
                                param.Direction == ParameterDirection.InputOutput)
                            {
                                param.Value = cmd.Parameters[param.ParameterName].Value;
                            }
                        }
                    }

                    return result?.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to execute stored procedure: {procedureName}", ex);
            }
        }

        // ... Rest of the class implementation remains the same ...

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    if (_connection?.State == ConnectionState.Open)
                    {
                        _connection.Close();
                    }
                    _connection?.Dispose();
                }
                _disposed = true;
            }
        }

        ~DbOperations()
        {
            Dispose(false);
        }
    }
}