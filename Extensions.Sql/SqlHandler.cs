using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

#nullable disable
namespace Extensions.Sql
{
	public class SqlHandler
	{
		public static string ConnectionString { get; set; }

		protected static IDataReader ExecuteReader(string procedure, params object[] parameters)
		{ return SqlHelper.ExecuteReader(ConnectionString, procedure, parameters); }

		protected static object ExecuteScalar(string procedure, params object[] parameters)
		{ return SqlHelper.ExecuteScalar(ConnectionString, procedure, parameters); }

		protected static void ExecuteNonQuery(string procedure, params object[] parameters)
		{ SqlHelper.ExecuteNonQuery(ConnectionString, procedure, parameters); }

		protected static IEnumerable<T> ExecuteReaderCBO<T>(string procedure, params object[] parameters) where T : class, new()
		{ return SqlReflector.ReflectList<T>(ExecuteReader(procedure, parameters)); }

		protected static T ExecuteReaderRowCBO<T>(string procedure, params object[] parameters) where T : class, new()
		{
			return SqlReflector.ReflectObject<T>(ExecuteReader(procedure, parameters));
		}

		protected static DataRow ExecuteRowReader(string procedure, params object[] parameters)
		{
			var table = new DataTable();
			table.Load(ExecuteReader(procedure, parameters));

			if (table.Rows.Count > 0)
			{
				return table.Rows[0];
			}

			return null;
		}

		protected static object ExecuteQuery(string querry)
		{ return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, querry); }

		protected static object ExecuteScalarQuery(string querry)
		{ return SqlHelper.ExecuteScalar(ConnectionString, CommandType.Text, querry); }

		protected static IEnumerable<T> ExecuteQueryCBO<T>(string querry) where T : class, new()
		{ return SqlReflector.ReflectList<T>(SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, querry)); }

		#region WithConnectionStrings
		protected static IDataReader ExecuteReaderWithCnx(string connection, string procedure, params object[] parameters)
		{ return SqlHelper.ExecuteReader(connection, procedure, parameters); }

		protected static object ExecuteScalarWithCnx(string connection, string procedure, params object[] parameters)
		{ return SqlHelper.ExecuteScalar(connection, procedure, parameters); }

		protected static void ExecuteNonQueryWithCnx(string connection, string procedure, params object[] parameters)
		{ SqlHelper.ExecuteNonQuery(connection, procedure, parameters); }

		protected static IEnumerable<T> ExecuteReaderCBOWithCnx<T>(string connection, string procedure, params object[] parameters) where T : class, new()
		{ return SqlReflector.ReflectList<T>(ExecuteReader(connection, procedure, parameters)); }

		protected static T ExecuteReaderRowCBOWithCnx<T>(string connection, string procedure, params object[] parameters) where T : class, new()
		{ return SqlReflector.ReflectObject<T>(ExecuteReader(connection, procedure, parameters)); }

		protected static DataRow ExecuteRowReaderWithCnx(string connection, string procedure, params object[] parameters)
		{
			var table = new DataTable();
			table.Load(ExecuteReader(connection, procedure, parameters));

			if (table.Rows.Count > 0)
			{
				return table.Rows[0];
			}

			return null;
		}

		internal static Transaction CreateTransaction() => new();

		public class Transaction : IDisposable
		{
			private readonly SqlConnection _connection;
			private readonly SqlTransaction _transaction;

			public Transaction()
			{
				_connection = new SqlConnection(ConnectionString);

				_connection.Open();

				_transaction = _connection.BeginTransaction();
			}

			public void Dispose()
			{
				_connection?.Dispose();
				_transaction?.Dispose();
			}

			public static implicit operator SqlTransaction(Transaction transaction) => transaction._transaction;
			public static implicit operator SqlConnection(Transaction transaction) => transaction._connection;
		}
		#endregion
	}
}
#nullable enable