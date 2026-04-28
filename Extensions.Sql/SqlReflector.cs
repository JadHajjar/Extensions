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
	private static readonly DateTime MinimumDate = new(1900, 1, 1);

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
            else if (Nullable.GetUnderlyingType(pi.Key.PropertyType) == typeof(DateTime))
            {
                pi.Key.SetValue(entity, null, null); // DateTime? with no value → null, not 1900
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
            else if (Nullable.GetUnderlyingType(pi.Key.PropertyType) == typeof(DateTime))
            {
                pi.Key.SetValue(entity, null, null); // DateTime? with no value → null, not 1900
            }
        }

		return entity;
	}

    public static void ConvertDataValue(ref object dbValue, Type type)
    {
        // Unwrap nullable once — all branches work against the real underlying type
        var underlyingType = Nullable.GetUnderlyingType(type);
        var isNullable = underlyingType != null;
        var targetType = underlyingType ?? type;

        // Empty string into a nullable field → null, we're done
        if (isNullable && dbValue is string s && string.IsNullOrWhiteSpace(s))
        {
            dbValue = null;
            return;
        }

        if (targetType.IsEnum)
        {
            dbValue = Enum.ToObject(targetType, dbValue);
            return;
        }

        if (targetType == typeof(bool) && dbValue is not bool)
        {
            var valStr = dbValue.ToString();
            dbValue = valStr.Equals("1", StringComparison.CurrentCultureIgnoreCase)
                   || valStr.Equals("true", StringComparison.CurrentCultureIgnoreCase)
                   || valStr.Equals("yes", StringComparison.CurrentCultureIgnoreCase);
            return;
        }

        if (targetType == typeof(DateTime) && (dbValue is not DateTime time || time < MinimumDate))
        {
            if (int.TryParse(dbValue.ToString(), out var days))
            {
                dbValue = DateTime.FromOADate(days);
            }
            else
            {
                dbValue = dbValue is string dbStr && DateTime.TryParseExact(dbStr, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var edt) ? edt
                        : dbValue is string dbStr2 && DateTime.TryParseExact(dbStr2, "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var edt2) ? edt2
                        : dbValue is string dbStr3 && DateTime.TryParse(dbStr3, out var cdt) ? cdt
                        : isNullable ? null : (object)MinimumDate;
            }
            return;
        }

        if (targetType == typeof(long) && dbValue is byte[] bytes)
        {
            dbValue = BitConverter.ToInt64((bytes as IEnumerable<byte>).Reverse().ToArray(), 0);
            return;
        }

        if (targetType != dbValue.GetType())
        {
            if (dbValue is string str && string.IsNullOrEmpty(str))
            {
                dbValue = isNullable ? null : Activator.CreateInstance(targetType);
            }
            else
            {
                // Convert to the unwrapped type — Convert.ChangeType can't handle Nullable<T>
                dbValue = Convert.ChangeType(dbValue, targetType, CultureInfo.InvariantCulture);
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