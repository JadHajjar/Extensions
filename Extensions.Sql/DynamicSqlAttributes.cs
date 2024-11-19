using System;
#nullable disable
namespace Extensions.Sql;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public class DynamicSqlClassAttribute : Attribute
{
	public string TableName { get; set; }
	public bool SingleRecord { get; set; }
	public bool NoChecks { get; set; }
	public string GetCondition { get; set; }
	public bool AlwaysReturn { get; set; }

	public DynamicSqlClassAttribute(string tableName)
	{ TableName = tableName; }
}

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class DynamicSqlPropertyAttribute : Attribute
{
	public string ColumnName { get; set; }
	public string Conversion { get; set; }
	public bool PrimaryKey { get; set; }
	public bool Identity { get; set; }
	public bool Indexer { get; set; }
	public bool Timestamp { get; set; }
	public int Order { get; set; }
}
#nullable enable