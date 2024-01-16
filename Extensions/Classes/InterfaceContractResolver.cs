using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Extensions;

public class InterfaceContractResolver : DefaultContractResolver
{
	private readonly Type[] Types;

	public InterfaceContractResolver(params Type[] InterfaceType)
	{
		Types = InterfaceType;
	}

	protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
	{
		return base.CreateProperties(Types.FirstOrDefault(x => x.IsAssignableFrom(type)) ?? type, memberSerialization);
	}
}