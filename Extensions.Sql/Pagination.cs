#nullable disable
namespace Extensions.Sql;

public struct Pagination
{
	public string OrderBy { get; set; }
	public int PageSize { get; set; }
	public int PageNumber { get; set; }
}
#nullable enable