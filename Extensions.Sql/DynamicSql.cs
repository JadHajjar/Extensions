using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

#nullable disable
namespace Extensions.Sql;

public static class DynamicSql
{
	public static object SqlAdd<T>(this T item, bool autoUpdate = false, SqlTransaction tr = null) where T : IDynamicSql
	{
		var type = typeof(T);
		var classInf = getDynamicClass(type);
		var props = getDynamicProperties(type);
		var sb = new StringBuilder();
		var check = classInf.SingleRecord || props.Any(x => x.Value.PrimaryKey);
		autoUpdate = autoUpdate || classInf.SingleRecord;

		if (check)
		{
			sb.AppendLine(string.Format("IF (0 = (SELECT COUNT(*) FROM [{0}]", classInf.TableName));
			if (props.Any(x => x.Value.PrimaryKey || x.Value.Timestamp))
			{
				sb.AppendLine(string.Format("WHERE {0}", string.Join(" AND ", props.Where(x => x.Value.PrimaryKey || x.Value.Timestamp).Select(x => string.Format("{0} = {1}", x.ColumnName(), x.ColumnVar())).ToArray())));
			}

			sb.AppendLine("))");
			sb.AppendLine("BEGIN");
		}

		sb.AppendLine(string.Format("INSERT INTO [{0}] (", classInf.TableName));
		sb.AppendLine(string.Join(", ", props.Where(x => !x.Value.Identity && !x.Value.Timestamp).Select(x => x.ColumnName()).ToArray()));
		sb.AppendLine(") VALUES (");
		sb.AppendLine(string.Join(", ", props.Where(x => !x.Value.Identity && !x.Value.Timestamp).Select(x => x.ColumnVar()).ToArray()));
		sb.AppendLine(")");

		if (props.Any(x => x.Value.Identity))
		{
			sb.AppendLine("SELECT SCOPE_IDENTITY()");
		}
		else
		{
			sb.AppendLine("SELECT 0");
		}

		if (check)
		{
			sb.AppendLine("END");

			if (autoUpdate)
			{
				sb.AppendLine("ELSE");
				sb.AppendLine("BEGIN");
				sb.AppendLine(string.Format("UPDATE [{0}] SET", classInf.TableName));
				sb.AppendLine(string.Join(", ", props.Where(x => !x.Value.Identity && !x.Value.Timestamp).Select(x => string.Format("{0} = {1}", x.ColumnName(), x.ColumnVar())).ToArray()));
				if (props.Any(x => x.Value.PrimaryKey))
				{
					sb.AppendLine(string.Format("WHERE {0}", string.Join(" AND ", props.Where(x => x.Value.PrimaryKey).Select(x => string.Format("{0} = {1}", x.ColumnName(), x.ColumnVar())).ToArray())));
				}

				if (props.Any(x => x.Value.Identity))
				{
					sb.AppendLine(string.Format("SELECT {0} AS {1}", props.First(x => x.Value.Identity).ColumnVar(), props.First(x => x.Value.Identity).ColumnName()));
				}

				sb.AppendLine("END");
			}
			else
			{
				sb.AppendLine("ELSE");
				sb.AppendLine("BEGIN");
				sb.AppendLine("SELECT 1");
				sb.AppendLine("END");
			}
		}

		return tr == null
			? SqlHelper.ExecuteScalar(SqlHandler.ConnectionString
				, CommandType.Text
				, sb.ToString()
				, props.Select(x => x.ColumnValue(item)).ToArray())
			: SqlHelper.ExecuteScalar(tr
			, CommandType.Text
			, sb.ToString()
			, props.Select(x => x.ColumnValue(item)).ToArray());
	}

	public static object SqlUpdate<T>(this T item, SqlTransaction tr = null) where T : IDynamicSql
	{
		var type = typeof(T);
		var classInf = getDynamicClass(type);
		var props = getDynamicProperties(type);
		var sb = new StringBuilder();

		sb.AppendLine(string.Format("IF (1 = (SELECT COUNT(*) FROM [{0}]", classInf.TableName));
		if (props.Any(x => x.Value.PrimaryKey))
		{
			sb.AppendLine(string.Format("WHERE {0}))", props.Where(x => x.Value.PrimaryKey).ListStrings(x => string.Format("{0} = {1}", x.ColumnName(), x.ColumnVar()), " AND ")));
		}

		sb.AppendLine("BEGIN");
		sb.AppendLine(string.Format("UPDATE [{0}] SET", classInf.TableName));
		sb.AppendLine(props.Where(x => !x.Value.Identity).ListStrings(x => string.Format("{0} = {1}", x.ColumnName(), x.ColumnVar()), ", "));
		if (props.Any(x => x.Value.PrimaryKey))
		{
			sb.AppendLine(string.Format(" WHERE {0}", props.Where(x => x.Value.PrimaryKey).ListStrings(x => string.Format("{0} = {1}", x.ColumnName(), x.ColumnVar()), " AND ")));
		}

		sb.AppendLine("END");

		return tr == null
			? SqlHelper.ExecuteScalar(SqlHandler.ConnectionString
				, CommandType.Text
				, sb.ToString()
				, props.Select(x => x.ColumnValue(item)).ToArray())
			: SqlHelper.ExecuteScalar(tr
			, CommandType.Text
			, sb.ToString()
			, props.Select(x => x.ColumnValue(item)).ToArray());
	}

	public static object SqlDeleteOne<T>(this T item, SqlTransaction tr = null) where T : IDynamicSql
	{
		var type = typeof(T);
		var classInf = getDynamicClass(type);
		var props = getDynamicProperties(type);
		var sb = new StringBuilder();

		sb.AppendLine(string.Format("DELETE FROM [{0}] WHERE {1}", classInf.TableName, props.Where(x => x.Value.PrimaryKey).ListStrings(x => string.Format("{0} = {1}", x.ColumnName(), x.ColumnVar()), " AND ")));

		return tr == null
			? SqlHelper.ExecuteScalar(SqlHandler.ConnectionString
			 , CommandType.Text
			 , sb.ToString()
			 , props.Where(x => x.Value.PrimaryKey).Select(x => x.ColumnValue(item)).ToArray())
			: SqlHelper.ExecuteScalar(tr
		 , CommandType.Text
		 , sb.ToString()
		 , props.Where(x => x.Value.PrimaryKey).Select(x => x.ColumnValue(item)).ToArray());
	}

	public static object SqlDelete<T>(string condition = null, SqlTransaction tr = null) where T : IDynamicSql
	{
		var type = typeof(T);
		var classInf = getDynamicClass(type);
		var sb = new StringBuilder();

		sb.AppendLine(string.Format("DELETE FROM [{0}]", classInf.TableName));
		if (!string.IsNullOrEmpty(condition))
		{
			sb.AppendLine(string.Format(" WHERE {0}", condition));
		}

		return tr == null
			? SqlHelper.ExecuteScalar(SqlHandler.ConnectionString
			 , CommandType.Text
			 , sb.ToString())
			: SqlHelper.ExecuteScalar(tr
		 , CommandType.Text
		 , sb.ToString());
	}

	public static object SqlDeleteByIndex<T>(this T item, string condition = null, SqlTransaction tr = null) where T : IDynamicSql
	{
		var type = typeof(T);
		var classInf = getDynamicClass(type);
		var props = getDynamicProperties(type);
		var sb = new StringBuilder();

		sb.AppendLine(string.Format("DELETE FROM [{0}]", classInf.TableName));
		sb.AppendLine(string.Format("WHERE {0}", props.Where(x => x.Value.Indexer).ListStrings(x => string.Format("{0} = {1}", x.ColumnName(), x.ColumnVar()), " AND ")));
		if (!string.IsNullOrEmpty(classInf.GetCondition))
		{
			sb.AppendLine(string.Format(" AND {0}", classInf.GetCondition));
		}

		if (!string.IsNullOrEmpty(condition))
		{
			sb.AppendLine(string.Format(" AND {0}", condition));
		}

		return tr == null
			? SqlHelper.ExecuteScalar(SqlHandler.ConnectionString
				 , CommandType.Text
				 , sb.ToString()
				 , props.Where(x => x.Value.Indexer).Select(x => x.ColumnValue(item)).ToArray())
			: SqlHelper.ExecuteScalar(tr
			 , CommandType.Text
			 , sb.ToString()
			 , props.Where(x => x.Value.Indexer).Select(x => x.ColumnValue(item)).ToArray());
	}

	public static List<T> SqlGet<T>(string condition = null, SqlTransaction tr = null) where T : IDynamicSql, new()
	{
		var type = typeof(T);
		var classInf = getDynamicClass(type);
		var props = getDynamicProperties(type);
		var sb = new StringBuilder();

		sb.AppendLine("SELECT");
		sb.AppendLine(props.ListStrings(x => x.ColumnName(true), ", "));
		sb.AppendLine(string.Format(" FROM [{0}] WITH (NOLOCK)", classInf.TableName));

		if (!string.IsNullOrEmpty(condition) || !string.IsNullOrEmpty(classInf.GetCondition))
		{
			sb.AppendLine(string.Format(" WHERE {0} {1} {2}", condition, !string.IsNullOrEmpty(condition) && !string.IsNullOrEmpty(classInf.GetCondition) ? "AND" : "", classInf.GetCondition));
		}

		return tr == null
			? SqlReflector.ReflectList<T>(SqlHelper.ExecuteReader(SqlHandler.ConnectionString
			 , CommandType.Text
			 , sb.ToString()))
			: SqlReflector.ReflectList<T>(SqlHelper.ExecuteReader(tr
		 , CommandType.Text
		 , sb.ToString()));
	}

	public static T SqlGetOne<T>(string condition = null, SqlTransaction tr = null) where T : IDynamicSql, new()
	{
		var type = typeof(T);
		var classInf = getDynamicClass(type);
		var props = getDynamicProperties(type);
		var sb = new StringBuilder();

		sb.AppendLine("SELECT TOP 1 ");
		sb.AppendLine(props.ListStrings(x => x.ColumnName(true), ", "));
		sb.AppendLine(string.Format(" FROM [{0}] WITH (NOLOCK)", classInf.TableName));

		if (!string.IsNullOrEmpty(condition) || !string.IsNullOrEmpty(classInf.GetCondition))
		{
			sb.AppendLine(string.Format(" WHERE {0} {1} {2}", condition, !string.IsNullOrEmpty(condition) && !string.IsNullOrEmpty(classInf.GetCondition) ? "AND" : "", classInf.GetCondition));
		}

		return tr == null
			? SqlReflector.ReflectObject<T>(SqlHelper.ExecuteReader(SqlHandler.ConnectionString
			 , CommandType.Text
			 , sb.ToString()))
			: SqlReflector.ReflectObject<T>(SqlHelper.ExecuteReader(tr
		 , CommandType.Text
		 , sb.ToString()));
	}

	public static T SqlGetById<T>(this T item, bool andIndex = false, SqlTransaction tr = null) where T : IDynamicSql, new()
	{
		var type = typeof(T);
		var classInf = getDynamicClass(type);
		var props = getDynamicProperties(type);
		var sb = new StringBuilder();

		sb.AppendLine(string.Format("SELECT * FROM [{0}] WITH (NOLOCK)", classInf.TableName));
		sb.AppendLine(string.Format("WHERE {0}", props.Where(x => x.Value.PrimaryKey || (andIndex && x.Value.Indexer)).ListStrings(x => string.Format("{0} = {1}", x.ColumnName(), x.ColumnVar()), " AND ")));
		if (!string.IsNullOrEmpty(classInf.GetCondition))
		{
			sb.AppendLine(string.Format(" AND {0}", classInf.GetCondition));
		}

		return tr == null
			? SqlReflector.ReflectObject<T>(SqlHelper.ExecuteReader(SqlHandler.ConnectionString
			, CommandType.Text
			, sb.ToString()
			, props.Where(x => x.Value.PrimaryKey || (andIndex && x.Value.Indexer)).Select(x => x.ColumnValue(item)).ToArray()), classInf.AlwaysReturn ? item : default)
			: SqlReflector.ReflectObject<T>(SqlHelper.ExecuteReader(tr
			, CommandType.Text
			, sb.ToString()
			, props.Where(x => x.Value.PrimaryKey || (andIndex && x.Value.Indexer)).Select(x => x.ColumnValue(item)).ToArray()), classInf.AlwaysReturn ? item : default);
	}

	public static List<T> SqlGetByIndex<T>(this T item, string condition = null, SqlTransaction tr = null) where T : IDynamicSql, new()
	{
		var type = typeof(T);
		var classInf = getDynamicClass(type);
		var props = getDynamicProperties(type);
		var sb = new StringBuilder();

		sb.AppendLine(string.Format("SELECT * FROM [{0}] WITH (NOLOCK)", classInf.TableName));
		sb.AppendLine(string.Format("WHERE {0}", props.Where(x => x.Value.Indexer).ListStrings(x => string.Format("{0} = {1}", x.ColumnName(), x.ColumnVar()), " AND ")));
		if (!string.IsNullOrEmpty(classInf.GetCondition))
		{
			sb.AppendLine(string.Format(" AND {0}", classInf.GetCondition));
		}

		if (!string.IsNullOrEmpty(condition))
		{
			sb.AppendLine(string.Format(" AND {0}", condition));
		}

		return tr == null
			? SqlReflector.ReflectList<T>(SqlHelper.ExecuteReader(SqlHandler.ConnectionString
			, CommandType.Text
			, sb.ToString()
			, props.Where(x => x.Value.Indexer).Select(x => x.ColumnValue(item)).ToArray()))
			: SqlReflector.ReflectList<T>(SqlHelper.ExecuteReader(tr
		, CommandType.Text
		, sb.ToString()
		, props.Where(x => x.Value.Indexer).Select(x => x.ColumnValue(item)).ToArray()));
	}

	public static IEnumerable<object> SqlAddMultiple<T>(this IEnumerable<T> list, bool autoUpdate = false, SqlTransaction tr = null) where T : IDynamicSql
	{
		foreach (var item in list)
		{
			yield return item.SqlAdd(autoUpdate, tr);
		}
	}

	#region Private Methods

	private static Dictionary<PropertyInfo, DynamicSqlPropertyAttribute> getDynamicProperties(Type type)
	{ return type.GetProperties().Where(x => x.GetCustomAttributes(typeof(DynamicSqlPropertyAttribute), false).Any()).ToDictionary(x => x, x => (DynamicSqlPropertyAttribute)x.GetCustomAttributes(typeof(DynamicSqlPropertyAttribute), false).FirstOrDefault()); }

	private static DynamicSqlClassAttribute getDynamicClass(Type type)
	{ return (DynamicSqlClassAttribute)type.GetCustomAttributes(typeof(DynamicSqlClassAttribute), false).FirstOrDefault(); }

	private static string ColumnName(this KeyValuePair<PropertyInfo, DynamicSqlPropertyAttribute> kvp, bool asSelect = false)
	{
		return string.IsNullOrEmpty(kvp.Value.Conversion)
			? string.Format("[{0}]", string.IsNullOrEmpty(kvp.Value.ColumnName) ? kvp.Key.Name : kvp.Value.ColumnName)
			: string.Format("CONVERT({0}, [{1}])" + (asSelect ? " as {1}" : ""), kvp.Value.Conversion, string.IsNullOrEmpty(kvp.Value.ColumnName) ? kvp.Key.Name : kvp.Value.ColumnName);
	}

	private static string ColumnVar(this KeyValuePair<PropertyInfo, DynamicSqlPropertyAttribute> kvp)
	{ return string.Format("@{0}", (string.IsNullOrEmpty(kvp.Value.ColumnName) ? kvp.Key.Name : kvp.Value.ColumnName).ToLower().Replace(" ", "_")); }

	private static string ListStrings<T>(this IEnumerable<T> list, Func<T, string> conversion, string seperator)
	{ return string.Join(seperator, list.Select(conversion).ToArray()); }

	private static SqlParameter ColumnValue(this KeyValuePair<PropertyInfo, DynamicSqlPropertyAttribute> kvp, object item)
	{
		var val = kvp.Key.GetValue(item, null);

		if (val == null)
		{
			return new SqlParameter(ColumnVar(kvp), null);
		}

		if (val is int)
		{
			return new SqlParameter(ColumnVar(kvp), SqlDbType.Int)
			{
				Direction = ParameterDirection.Input,
				Value = val
			};
		}

		if (val is double or float or decimal)
		{
			return new SqlParameter(ColumnVar(kvp), SqlDbType.Decimal)
			{
				Direction = ParameterDirection.Input,
				Value = val
			};
		}

		if (val is DateTime)
		{
			return new SqlParameter(ColumnVar(kvp), SqlDbType.DateTime)
			{
				Direction = ParameterDirection.Input,
				Value = ((DateTime)val).Year < 1900 ? new DateTime(1900, 1, 1) : (DateTime)val
			};
		}

		if (val is bool)
		{
			return new SqlParameter(ColumnVar(kvp), SqlDbType.Bit)
			{
				Direction = ParameterDirection.Input,
				Value = val
			};
		}

		if (val is Enum)
		{
			return new SqlParameter(ColumnVar(kvp), SqlDbType.Int)
			{
				Direction = ParameterDirection.Input,
				Value = val
			};
		}

		return val is byte[]? new SqlParameter(ColumnVar(kvp), SqlDbType.VarBinary)
		{
			Direction = ParameterDirection.Input,
			Size = ((byte[])val).Length,
			Value = val
		}
			: val is Guid
			? new SqlParameter(ColumnVar(kvp), SqlDbType.UniqueIdentifier)
			{
				Direction = ParameterDirection.Input,
				Size = val == null ? 0 : val.ToString().Length,
				Value = val
			}
			: new SqlParameter(ColumnVar(kvp), SqlDbType.NVarChar)
			{
				Direction = ParameterDirection.Input,
				Size = val == null ? 0 : val.ToString().Length,
				Value = val
			};
	}

	#endregion Private Methods
}
#nullable enable
