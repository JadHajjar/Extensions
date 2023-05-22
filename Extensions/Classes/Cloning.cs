using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Extensions
{
	public static class Cloning
	{
		public static T Clone<T>(this T obj)
		{
			// Check if the object is null
			if (obj == null)
			{
				return default;
			}

			// Get the type of the object
			var type = obj.GetType();

			// If the object is a value type or a string, return a copy of it
			if (type.IsValueType || type == typeof(string))
			{
				return obj;
			}

			// If the object is a list, create a new list and copy its elements
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
			{
				var list = (IList)obj;
				var newList = (IList)Activator.CreateInstance(type);
				foreach (var element in list)
				{
					newList.Add(Clone(element));
				}
				return (T)newList;
			}

			// If the object is a list, create a new list and copy its elements
			if (type.IsArray)
			{
				var elementType = type.GetElementType();
				var array = (obj as Array);
				var copiedArray = Array.CreateInstance(elementType, array.Length);
				for (var i = 0; i < array.Length; i++)
				{
					copiedArray.SetValue(Clone(array.GetValue(i)), i);
				}
				return (T)Convert.ChangeType(copiedArray, type);
			}

			// If the object is a class, create a new instance and copy its properties
			var newObject = Activator.CreateInstance(type);
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			foreach (var property in properties)
			{
				if (property.CanWrite && property.CanRead && property.GetCustomAttributes(typeof(CloneIgnoreAttribute), false).Length == 0)
				{
					var value = property.GetValue(obj, null);
					if (value != null)
					{
						var newValue = Clone(value);
						property.SetValue(newObject, newValue, null);
					}
				}
			}

			return (T)newObject;
		}

		public static T2 CloneTo<T, T2>(this T obj) where T2 : class, T where T : class
		{
			// Check if the object is null
			if (obj == null)
			{
				return default;
			}

			// Get the type of the object
			var type = obj.GetType();
			var newObject = Activator.CreateInstance(typeof(T2));
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			foreach (var property in properties)
			{
				if (property.CanWrite && property.CanRead && property.GetCustomAttributes(typeof(CloneIgnoreAttribute), false).Length == 0)
				{
					var value = property.GetValue(obj, null);
					if (value != null)
					{
						var newValue = Clone(value);
						property.SetValue(newObject, newValue, null);
					}
				}
			}

			return (T2)newObject;
		}
	}

	public class CloneIgnoreAttribute : Attribute { }
}
