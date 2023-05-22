using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;

#nullable disable
namespace Extensions.Sql;

public static class SqlReflector
{
	private static readonly DateTime MinimumDate = new DateTime(1900, 1, 1);

	public static TEntity ReflectObject<TEntity>(this IDataReader dr, TEntity @base = default) where TEntity : new()
	{
		if (dr != null)
		{
			var propertyInfos = getDynamicProperties(typeof(TEntity));

			var dt = new DataTable();

			dt.Load(dr);

			if (dt.Rows.Count > 0)
			{
				return populateEntity(new TEntity(), propertyInfos, dt, dt.Rows[0]);
			}
		}

		return @base;
	}

	public static TEntity ReflectObject<TEntity>(this DataTable dt, TEntity @base = default) where TEntity : new()
	{
		if (dt != null)
		{
			var propertyInfos = getDynamicProperties(typeof(TEntity));

			if (dt.Rows.Count > 0)
			{
				return populateEntity(new TEntity(), propertyInfos, dt, dt.Rows[0]);
			}
		}

		return @base;
	}

	public static TEntity ReflectObject<TEntity>(this DataRow dt, TEntity @base = default) where TEntity : new()
	{
		if (dt != null)
		{
			var propertyInfos = getDynamicProperties(typeof(TEntity));

			if (dt != null)
			{
				return populateEntity(new TEntity(), propertyInfos, dt.Table, dt);
			}
		}

		return @base;
	}

	public static TEntity ReflectObject<TEntity>(this TEntity @base, DataRow dt) where TEntity : class
	{
		if (dt != null)
		{
			var propertyInfos = getDynamicProperties(@base.GetType());

			if (dt != null)
			{
				return populateEntity(@base, propertyInfos, dt.Table, dt);
			}
		}

		return @base;
	}

	public static object ReflectObject(this object @base, DataRow dt, Type type)
	{
		if (dt != null)
		{
			var propertyInfos = getDynamicProperties(type);

			if (dt != null)
			{
				return populateEntity(@base, propertyInfos, dt.Table, dt);
			}
		}

		return @base;
	}

	public static List<TEntity> ReflectList<TEntity>(this IDataReader dr) where TEntity : new()
	{
		var listToPopulate = new List<TEntity>();

		if (dr != null)
		{
			var propertyInfos = getDynamicProperties(typeof(TEntity));

			var dt = new DataTable();

			dt.Load(dr);

			foreach (DataRow row in dt.Rows)
			{
				listToPopulate.Add(populateEntity(new TEntity(), propertyInfos, dt, row));
			}
		}

		return listToPopulate;
	}

	public static List<TEntity> ReflectList<TEntity>(this DataTable dt) where TEntity : new()
	{
		var listToPopulate = new List<TEntity>();

		if (dt != null)
		{
			var propertyInfos = getDynamicProperties(typeof(TEntity));

			foreach (DataRow row in dt.Rows)
			{
				listToPopulate.Add(populateEntity(new TEntity(), propertyInfos, dt, row));
			}
		}

		return listToPopulate;
	}

	private static TEntity populateEntity<TEntity>(TEntity entity, Dictionary<PropertyInfo, DynamicSqlPropertyAttribute> propertyInfos, DataTable dt, DataRow row)
	{
		foreach (var pi in propertyInfos)
		{
			var column = pi.Value?.ColumnName ?? pi.Key.Name;

			if (dt.Columns.Contains(column) && (pi.Value != null || !propertyInfos.Any(x => x.Key != pi.Key && x.Value != null && x.Value.ColumnName == column)))
			{
				var dbValue = row[column];

				if (dbValue != null && dbValue != DBNull.Value)
				{
					ConvertDataValue(ref dbValue, pi.Key.PropertyType);

					pi.Key.SetValue(entity, dbValue, null);

					continue;
				}
			}

			if (pi.Key.PropertyType == typeof(string))
			{
				pi.Key.SetValue(entity, string.Empty, null);
			}
			else if (pi.Key.PropertyType == typeof(DateTime))
			{
				pi.Key.SetValue(entity, MinimumDate, null);
			}
		}

		return entity;
	}

	public static TEntity Clone<TEntity, TSource>(TSource source) where TEntity : class, TSource, new() where TSource : class
	{
		if (source == null)
		{
			return null;
		}

		var entity = new TEntity();
		var propertyInfos = getDynamicProperties(typeof(TSource));

		foreach (var pi in propertyInfos)
		{
			var dbValue = pi.Key.GetValue(source, null);

			if (dbValue != null && dbValue != DBNull.Value)
			{
				ConvertDataValue(ref dbValue, pi.Key.PropertyType);

				pi.Key.SetValue(entity, dbValue, null);

				continue;
			}

			if (pi.Key.PropertyType == typeof(string))
			{
				pi.Key.SetValue(entity, string.Empty, null);
			}
			else if (pi.Key.PropertyType == typeof(DateTime))
			{
				pi.Key.SetValue(entity, MinimumDate, null);
			}
		}

		return entity;
	}

	public static void ConvertDataValue(ref object dbValue, Type type)
	{
		if (type.IsEnum)
		{
			dbValue = Enum.ToObject(type, dbValue);
		}

		if (type == typeof(bool) && dbValue is not bool)
		{
			var valStr = dbValue.ToString();

			dbValue = valStr.Equals("1", StringComparison.CurrentCultureIgnoreCase)
				|| valStr.Equals("true", StringComparison.CurrentCultureIgnoreCase)
				|| valStr.Equals("yes", StringComparison.CurrentCultureIgnoreCase);
		}

		if (type == typeof(DateTime) && (dbValue is not DateTime time || time < MinimumDate))
		{
			if (int.TryParse(dbValue.ToString(), out var days))
			{
				dbValue = DateTime.FromOADate(days);
			}
			else if (dbValue is string dbStr && DateTime.TryParseExact(dbStr, "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var edt))
			{
				dbValue = edt;
			}
			else if (dbValue is string dbStr2 && DateTime.TryParse(dbStr2, out var cdt))
			{
				dbValue = cdt;
			}
			else
			{
				dbValue = MinimumDate;
			}
		}

		if (type == typeof(long) && dbValue is byte[] bytes)
		{
			dbValue = BitConverter.ToInt64(bytes.Reverse().ToArray(), 0);
		}

		if (type != dbValue.GetType())
		{
			if (dbValue is string str && string.IsNullOrEmpty(str))
			{
				dbValue = Activator.CreateInstance(type);
			}
			else
			{
				dbValue = Convert.ChangeType(dbValue, type, CultureInfo.InvariantCulture);
			}
		}
	}

	private static Dictionary<PropertyInfo, DynamicSqlPropertyAttribute> getDynamicProperties(Type type)
	{
		return type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanWrite)
				.ToDictionary(x => x, x => (DynamicSqlPropertyAttribute)x.GetCustomAttributes(typeof(DynamicSqlPropertyAttribute), false).FirstOrDefault());
	}
}
#nullable enable