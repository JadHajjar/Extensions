using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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
		var classInf = GetDynamicClass(type);
		var props = GetDynamicProperties(type);
		var sb = new StringBuilder();
		var check = !classInf.NoChecks && (classInf.SingleRecord || props.Any(x => x.Value.PrimaryKey));
		
		autoUpdate |= classInf.SingleRecord;

		if (check)
		{
			sb.AppendLine(string.Format("IF (0 = (SELECT COUNT(*) FROM [{0}]", classInf.TableName));
			if (props.Any(x => x.Value.PrimaryKey || x.Value.Timestamp))
			{
				sb.AppendLine(string.Format("WHERE ({0})", string.Join(" AND ", props.Where(x => x.Value.PrimaryKey || x.Value.Timestamp).Select(x => string.Format("{0} = {1}", x.ColumnName(), x.ColumnVar())).ToArray())));
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
					sb.AppendLine(string.Format("WHERE ({0})", string.Join(" AND ", props.Where(x => x.Value.PrimaryKey).Select(x => string.Format("{0} = {1}", x.ColumnName(), x.ColumnVar())).ToArray())));
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

		var commandText = sb.ToString();
		var parameters = props.Select(x => x.ColumnValue(item)).ToArray();

		if (tr == null)
		{
			return SqlHelper.ExecuteScalar(SqlHandler.ConnectionString, CommandType.Text, commandText, parameters);
		}

		return SqlHelper.ExecuteScalar(tr, CommandType.Text, commandText, parameters);
	}

	public static object SqlUpdate<T>(this T item, SqlTransaction tr = null) where T : IDynamicSql
	{
		var type = typeof(T);
		var classInf = GetDynamicClass(type);
		var props = GetDynamicProperties(type);
		var sb = new StringBuilder();

		sb.AppendLine(string.Format("IF (1 = (SELECT COUNT(*) FROM [{0}]", classInf.TableName));
		if (props.Any(x => x.Value.PrimaryKey))
		{
			sb.AppendLine(string.Format("WHERE ({0})))", props.Where(x => x.Value.PrimaryKey).ListStrings(x => string.Format("{0} = {1}", x.ColumnName(), x.ColumnVar()), " AND ")));
		}

		sb.AppendLine("BEGIN");
		sb.AppendLine(string.Format("UPDATE [{0}] SET", classInf.TableName));
		sb.AppendLine(props.Where(x => !x.Value.Identity).ListStrings(x => string.Format("{0} = {1}", x.ColumnName(), x.ColumnVar()), ", "));
		if (props.Any(x => x.Value.PrimaryKey))
		{
			sb.AppendLine(string.Format(" WHERE ({0})", props.Where(x => x.Value.PrimaryKey).ListStrings(x => string.Format("{0} = {1}", x.ColumnName(), x.ColumnVar()), " AND ")));
		}

		sb.AppendLine("END");

		var commandText = sb.ToString();
		var parameters = props.Select(x => x.ColumnValue(item)).ToArray();

		if (tr == null)
		{
			return SqlHelper.ExecuteScalar(SqlHandler.ConnectionString, CommandType.Text, commandText, parameters);
		}

		return SqlHelper.ExecuteScalar(tr, CommandType.Text, commandText, parameters);
	}

	public static object SqlDeleteOne<T>(this T item, SqlTransaction tr = null) where T : IDynamicSql
	{
		var type = typeof(T);
		var classInf = GetDynamicClass(type);
		var props = GetDynamicProperties(type);
		var sb = new StringBuilder();

		sb.AppendLine(string.Format("DELETE FROM [{0}] WHERE {1}", classInf.TableName, props.Where(x => x.Value.PrimaryKey).ListStrings(x => string.Format("{0} = {1}", x.ColumnName(), x.ColumnVar()), " AND ")));

		var commandText = sb.ToString();
		var parameters = props.Where(x => x.Value.PrimaryKey).Select(x => x.ColumnValue(item)).ToArray();

		if (tr == null)
		{
			return SqlHelper.ExecuteScalar(SqlHandler.ConnectionString, CommandType.Text, commandText, parameters);
		}

		return SqlHelper.ExecuteScalar(tr, CommandType.Text, commandText, parameters);
	}

	public static object SqlDelete<T>(string condition = null, SqlTransaction tr = null) where T : IDynamicSql
	{
		var type = typeof(T);
		var classInf = GetDynamicClass(type);
		var sb = new StringBuilder();

		sb.AppendLine(string.Format("DELETE FROM [{0}]", classInf.TableName));
		if (!string.IsNullOrEmpty(condition))
		{
			sb.AppendLine(string.Format(" WHERE ({0})", condition));
		}

		var commandText = sb.ToString();

		if (tr == null)
		{
			return SqlHelper.ExecuteScalar(SqlHandler.ConnectionString, CommandType.Text, commandText);
		}

		return SqlHelper.ExecuteScalar(tr, CommandType.Text, commandText);
	}

	public static object SqlDeleteByIndex<T>(this T item, string condition = null, SqlTransaction tr = null) where T : IDynamicSql
	{
		var type = typeof(T);
		var classInf = GetDynamicClass(type);
		var props = GetDynamicProperties(type);
		var sb = new StringBuilder();

		sb.AppendLine(string.Format("DELETE FROM [{0}]", classInf.TableName));
		sb.AppendLine(string.Format("WHERE ({0})", props.Where(x => x.Value.Indexer).ListStrings(x => string.Format("{0} = {1}", x.ColumnName(), x.ColumnVar()), " AND ")));
		if (!string.IsNullOrEmpty(classInf.GetCondition))
		{
			sb.AppendLine(string.Format(" AND ({0})", classInf.GetCondition));
		}

		if (!string.IsNullOrEmpty(condition))
		{
			sb.AppendLine(string.Format(" AND ({0})", condition));
		}

		var parameters = props.Where(x => x.Value.Indexer).Select(x => x.ColumnValue(item)).ToArray();
		var commandText = sb.ToString();

		if (tr == null)
		{
			return SqlHelper.ExecuteScalar(SqlHandler.ConnectionString, CommandType.Text, commandText, parameters);
		}

		return SqlHelper.ExecuteScalar(tr, CommandType.Text, commandText, parameters);
	}

	public static int SqlCount<T>(string condition = null, SqlTransaction tr = null) where T : IDynamicSql, new()
	{
		var type = typeof(T);
		var classInf = GetDynamicClass(type);
		var props = GetDynamicProperties(type);
		var sb = new StringBuilder();

		sb.AppendLine("SELECT COUNT(*)");
		sb.AppendLine(string.Format(" FROM [{0}] WITH (NOLOCK)", classInf.TableName));

		if (!string.IsNullOrEmpty(condition) || !string.IsNullOrEmpty(classInf.GetCondition))
		{
			sb.AppendLine(string.Format(" WHERE ({0} {1} {2})", condition, !string.IsNullOrEmpty(condition) && !string.IsNullOrEmpty(classInf.GetCondition) ? "AND" : "", classInf.GetCondition));
		}

		var commandText = sb.ToString();
		object result;

		if (tr == null)
		{
			result = SqlHelper.ExecuteScalar(SqlHandler.ConnectionString, CommandType.Text, commandText);
		}
		else
		{
			result = SqlHelper.ExecuteScalar(tr, CommandType.Text, commandText);
		}

		return int.Parse(result?.ToString() ?? "0");
	}

	public static List<T> SqlGet<T>(string condition = null, SqlTransaction tr = null, Pagination? pagination = null) where T : IDynamicSql, new()
	{
		var type = typeof(T);
		var classInf = GetDynamicClass(type);
		var props = GetDynamicProperties(type);
		var sb = new StringBuilder();

		sb.AppendLine("SELECT");
		sb.AppendLine(props.ListStrings(x => x.ColumnName(true), ", "));
		sb.AppendLine(string.Format(" FROM [{0}] WITH (NOLOCK)", classInf.TableName));

		if (!string.IsNullOrEmpty(condition) || !string.IsNullOrEmpty(classInf.GetCondition))
		{
			sb.AppendLine(string.Format(" WHERE ({0} {1} {2})", condition, !string.IsNullOrEmpty(condition) && !string.IsNullOrEmpty(classInf.GetCondition) ? "AND" : "", classInf.GetCondition));
		}

		if (pagination.HasValue)
		{
			sb.AppendLine(string.Format(" ORDER BY {0}", pagination.Value.OrderBy));
			sb.AppendLine(string.Format(" OFFSET {0} * ({1} - 1) ROWS", pagination.Value.PageSize, pagination.Value.PageNumber));
			sb.AppendLine(string.Format(" FETCH NEXT {0} ROWS ONLY", pagination.Value.PageSize));
		}
		else if (props.Any(x => x.Value.Order != 0))
		{
			sb.Append(" ORDER BY");

			foreach (var column in props.Where(x => x.Value.Order != 0).OrderBy(x => Math.Abs(x.Value.Order)))
			{
				sb.AppendFormat(" {0} {1},", column.ColumnName(true), column.Value.Order > 0 ? "ASC" : "DESC");
			}

			sb.Remove(sb.Length - 1, 1);
			sb.AppendLine();
		}

		var commandText = sb.ToString();
		IDataReader reader;

		if (tr == null)
		{
			reader = SqlHelper.ExecuteReader(SqlHandler.ConnectionString, CommandType.Text, commandText);
		}
		else
		{
			reader = SqlHelper.ExecuteReader(tr, CommandType.Text, commandText);
		}

		return SqlReflector.ReflectList<T>(reader);
	}

	public static T SqlGetOne<T>(string condition = null, SqlTransaction tr = null) where T : IDynamicSql, new()
	{
		var type = typeof(T);
		var classInf = GetDynamicClass(type);
		var props = GetDynamicProperties(type);
		var sb = new StringBuilder();

		sb.AppendLine("SELECT TOP 1 ");
		sb.AppendLine(props.ListStrings(x => x.ColumnName(true), ", "));
		sb.AppendLine(string.Format(" FROM [{0}] WITH (NOLOCK)", classInf.TableName));

		if (!string.IsNullOrEmpty(condition) || !string.IsNullOrEmpty(classInf.GetCondition))
		{
			sb.AppendLine(string.Format(" WHERE ({0} {1} {2})", condition, !string.IsNullOrEmpty(condition) && !string.IsNullOrEmpty(classInf.GetCondition) ? "AND" : "", classInf.GetCondition));
		}

		var commandText = sb.ToString();
		IDataReader reader;

		if (tr == null)
		{
			reader = SqlHelper.ExecuteReader(SqlHandler.ConnectionString, CommandType.Text, commandText);
		}
		else
		{
			reader = SqlHelper.ExecuteReader(tr, CommandType.Text, commandText);
		}

		return SqlReflector.ReflectObject<T>(reader);
	}

	public static T SqlGetById<T>(this T item, bool andIndex = false, SqlTransaction tr = null) where T : IDynamicSql, new()
	{
		var type = typeof(T);
		var classInf = GetDynamicClass(type);
		var props = GetDynamicProperties(type);
		var sb = new StringBuilder();

		sb.AppendLine(string.Format("SELECT * FROM [{0}] WITH (NOLOCK)", classInf.TableName));
		sb.AppendLine(string.Format("WHERE ({0})", props.Where(x => x.Value.PrimaryKey || (andIndex && x.Value.Indexer)).ListStrings(x => string.Format("{0} = {1}", x.ColumnName(), x.ColumnVar()), " AND ")));
		if (!string.IsNullOrEmpty(classInf.GetCondition))
		{
			sb.AppendLine(string.Format(" AND ({0})", classInf.GetCondition));
		}

		var commandText = sb.ToString();
		var parameters = props
			.Where(x => x.Value.PrimaryKey || (andIndex && x.Value.Indexer))
			.Select(x => x.ColumnValue(item))
			.ToArray();

		var returnValue = classInf.AlwaysReturn ? item : default;

		IDataReader reader;

		if (tr == null)
		{
			reader = SqlHelper.ExecuteReader(SqlHandler.ConnectionString, CommandType.Text, commandText, parameters);
		}
		else
		{
			reader = SqlHelper.ExecuteReader(tr, CommandType.Text, commandText, parameters);
		}

		return SqlReflector.ReflectObject<T>(reader, returnValue);
	}

	public static List<T> SqlGetByIdList<T>(this T item, string condition = null, SqlTransaction tr = null) where T : IDynamicSql, new()
	{
		var type = typeof(T);
		var classInf = GetDynamicClass(type);
		var props = GetDynamicProperties(type);
		var sb = new StringBuilder();

		sb.AppendLine(string.Format("SELECT * FROM [{0}] WITH (NOLOCK)", classInf.TableName));
		sb.AppendLine(string.Format("WHERE ({0})", props.Where(x => x.Value.PrimaryKey).ListStrings(x => string.Format("{0} = {1}", x.ColumnName(), x.ColumnVar()), " AND ")));
		if (!string.IsNullOrEmpty(classInf.GetCondition))
		{
			sb.AppendLine(string.Format(" AND ({0})", classInf.GetCondition));
		}

		if (!string.IsNullOrEmpty(condition))
		{
			sb.AppendLine(string.Format(" AND ({0})", condition));
		}

		if (props.Any(x => x.Value.Order != 0))
		{
			sb.Append(" ORDER BY");

			foreach (var column in props.Where(x => x.Value.Order != 0).OrderBy(x => Math.Abs(x.Value.Order)))
			{
				sb.AppendFormat(" {0} {1},", column.ColumnName(true), column.Value.Order > 0 ? "ASC" : "DESC");
			}

			sb.Remove(sb.Length - 1, 1);
			sb.AppendLine();
		}

		var commandText = sb.ToString();
		var parameters = props
			.Where(x => x.Value.PrimaryKey)
			.Select(x => x.ColumnValue(item))
			.ToArray();

		IDataReader reader;

		if (tr == null)
		{
			reader = SqlHelper.ExecuteReader(SqlHandler.ConnectionString, CommandType.Text, commandText, parameters);
		}
		else
		{
			reader = SqlHelper.ExecuteReader(tr, CommandType.Text, commandText, parameters);
		}

		return SqlReflector.ReflectList<T>(reader);
	}

	public static List<T> SqlGetByIndex<T>(this T item, string condition = null, SqlTransaction tr = null) where T : IDynamicSql, new()
	{
		var type = typeof(T);
		var classInf = GetDynamicClass(type);
		var props = GetDynamicProperties(type);
		var sb = new StringBuilder();

		sb.AppendLine(string.Format("SELECT * FROM [{0}] WITH (NOLOCK)", classInf.TableName));
		sb.AppendLine(string.Format("WHERE ({0})", props.Where(x => x.Value.Indexer).ListStrings(x => string.Format("{0} = {1}", x.ColumnName(), x.ColumnVar()), " AND ")));
		if (!string.IsNullOrEmpty(classInf.GetCondition))
		{
			sb.AppendLine(string.Format(" AND ({0})", classInf.GetCondition));
		}

		if (!string.IsNullOrEmpty(condition))
		{
			sb.AppendLine(string.Format(" AND ({0})", condition));
		}

		if (props.Any(x => x.Value.Order != 0))
		{
			sb.Append(" ORDER BY");

			foreach (var column in props.Where(x => x.Value.Order != 0).OrderBy(x => Math.Abs(x.Value.Order)))
			{
				sb.AppendFormat(" {0} {1},", column.ColumnName(true), column.Value.Order > 0 ? "ASC" : "DESC");
			}

			sb.Remove(sb.Length - 1, 1);
			sb.AppendLine();
		}

		var commandText = sb.ToString();
		var parameters = props
			.Where(x => x.Value.Indexer)
			.Select(x => x.ColumnValue(item))
			.ToArray();

		IDataReader reader;

		if (tr == null)
		{
			reader = SqlHelper.ExecuteReader(SqlHandler.ConnectionString, CommandType.Text, commandText, parameters);
		}
		else
		{
			reader = SqlHelper.ExecuteReader(tr, CommandType.Text, commandText, parameters);
		}

		return SqlReflector.ReflectList<T>(reader);
	}

	public static IEnumerable<object> SqlAddMultiple<T>(this IEnumerable<T> list, bool autoUpdate = false, SqlTransaction tr = null) where T : IDynamicSql
	{
		foreach (var item in list)
		{
			yield return item.SqlAdd(autoUpdate, tr);
		}
	}

	#region Private Methods
	private static Dictionary<PropertyInfo, DynamicSqlPropertyAttribute> GetDynamicProperties(Type type)
	{
		var properties = type.GetProperties();
		var dynamicProperties = properties
			.Where(prop => prop.GetCustomAttributes(typeof(DynamicSqlPropertyAttribute), false).Any())
			.ToDictionary(prop => prop, prop =>
				(DynamicSqlPropertyAttribute)prop.GetCustomAttributes(typeof(DynamicSqlPropertyAttribute), false).FirstOrDefault());

		return dynamicProperties;
	}

	private static DynamicSqlClassAttribute GetDynamicClass(Type type)
	{
		return (DynamicSqlClassAttribute)type.GetCustomAttributes(typeof(DynamicSqlClassAttribute), false).FirstOrDefault();
	}

	private static string ColumnName(this KeyValuePair<PropertyInfo, DynamicSqlPropertyAttribute> kvp, bool asSelect = false)
	{
		var columnName = string.IsNullOrEmpty(kvp.Value.ColumnName) ? kvp.Key.Name : kvp.Value.ColumnName;

		if (string.IsNullOrEmpty(kvp.Value.Conversion))
		{
			return string.Format("[{0}]", columnName);
		}

		var conversion = kvp.Value.Conversion;

		return string.Format("CONVERT({0}, [{1}])" + (asSelect ? " as {1}" : ""), conversion, columnName);
	}

	private static string ColumnVar(this KeyValuePair<PropertyInfo, DynamicSqlPropertyAttribute> kvp)
	{
		var columnName = string.IsNullOrEmpty(kvp.Value.ColumnName) ? kvp.Key.Name : kvp.Value.ColumnName;

		return string.Format("@{0}", columnName.ToLower().Replace(" ", "_"));
	}

	private static string ListStrings<T>(this IEnumerable<T> list, Func<T, string> conversion, string separator)
	{
		return string.Join(separator, list.Select(conversion).ToArray());
	}

	private static SqlParameter ColumnValue(this KeyValuePair<PropertyInfo, DynamicSqlPropertyAttribute> kvp, object item)
	{
		var value = kvp.Key.GetValue(item, null);

		if (value == null)
		{
			return new SqlParameter(ColumnVar(kvp), null);
		}

		return value switch
		{
			int intValue => new SqlParameter(ColumnVar(kvp), SqlDbType.Int) { Direction = ParameterDirection.Input, Value = intValue },

			double numericValue => new SqlParameter(ColumnVar(kvp), SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = numericValue },

			float numericValue => new SqlParameter(ColumnVar(kvp), SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = numericValue },

			decimal numericValue => new SqlParameter(ColumnVar(kvp), SqlDbType.Decimal) { Direction = ParameterDirection.Input, Value = numericValue },

			DateTime timeValue => new SqlParameter(ColumnVar(kvp), SqlDbType.DateTime)
			{
				Direction = ParameterDirection.Input,
				Value = timeValue.Year < 1900 ? new DateTime(1900, 1, 1) : timeValue
			},

			bool boolValue => new SqlParameter(ColumnVar(kvp), SqlDbType.Bit) { Direction = ParameterDirection.Input, Value = boolValue },

			Enum enumValue => new SqlParameter(ColumnVar(kvp), SqlDbType.Int) { Direction = ParameterDirection.Input, Value = enumValue },

			byte[] byteArray => new SqlParameter(ColumnVar(kvp), SqlDbType.VarBinary)
			{
				Direction = ParameterDirection.Input,
				Size = byteArray.Length,
				Value = byteArray
			},

			Guid guidValue => new SqlParameter(ColumnVar(kvp), SqlDbType.UniqueIdentifier)
			{
				Direction = ParameterDirection.Input,
				Size = guidValue.ToString().Length,
				Value = guidValue
			},

			_ => new SqlParameter(ColumnVar(kvp), SqlDbType.NVarChar)
			{
				Direction = ParameterDirection.Input,
				Size = value.ToString().Length,
				Value = value
			}
		};
	}

	#endregion Private Methods
}
#nullable enable
