using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using static System.Environment;

namespace Extensions;
public class SaveHandler
{
	private readonly JsonSerializerSettings _jsonSettings;
	private readonly JsonSerializerSettings _jsonSettingsAllowErrors;

	public static SaveHandler Instance { get; set; } = new();
#if SIMPLE
	public static string AppName { get; set; }
#else
	public static string AppName { get; set; } = System.Windows.Forms.Application.ProductName;
#endif
	public string SaveDirectory { get; }

	public SaveHandler(string saveDirectory = null)
	{
		SaveDirectory = saveDirectory ?? GetFolderPath(SpecialFolder.LocalApplicationData);

		_jsonSettings = new JsonSerializerSettings();
		_jsonSettings.Error += (o, args) => args.ErrorContext.Handled = true;
		_jsonSettings.ContractResolver = new SaveHandlerContractResolver();
		
		_jsonSettingsAllowErrors = new JsonSerializerSettings();
		_jsonSettingsAllowErrors.ContractResolver = new SaveHandlerContractResolver();
	}

	#region Loading
	public T Load<T>() where T : ISaveObject, new()
	{
#if NET47
		var saveName = typeof(T).GetCustomAttribute<SaveNameAttribute>(false);
#else
		var saveName = typeof(T).GetCustomAttributes(typeof(SaveNameAttribute), false).FirstOrDefault() as SaveNameAttribute;
#endif
		var filePath = GetPath(saveName.FileName, saveName.AppName, saveName.Local);
		var fileContents = Read(filePath);

		var obj = string.IsNullOrWhiteSpace(fileContents) ? new() : JsonConvert.DeserializeObject<T>(fileContents, _jsonSettings) ?? new();

		obj.Handler = this;

		if (obj is IExtendedSaveObject extendedSaveObject)
		{
			extendedSaveObject.OnLoad(filePath);
		}

		return obj;
	}

	public T Load<T>(T obj, string fileName, string appName = null, bool local = false)
	{
		var filePath = GetPath(fileName, appName, local);
		var fileContents = Read(filePath);

		if (!string.IsNullOrWhiteSpace(fileContents))
		{
			if (obj == null)
			{
				obj = JsonConvert.DeserializeObject<T>(fileContents, _jsonSettings);
			}
			else
			{
				JsonConvert.PopulateObject(fileContents, obj, _jsonSettings);
			}
		}

		if (obj is ISaveObject saveObject)
		{
			saveObject.Handler = this;
		}

		if (obj is IExtendedSaveObject extendedSaveObject)
		{
			extendedSaveObject.OnLoad(filePath);
		}

		return obj;
	}

	public T Load<T>(out T obj, string fileName, string appName = null, bool local = false)
	{
		var filePath = GetPath(fileName, appName, local);
		var fileContents = Read(filePath);

		if (!string.IsNullOrWhiteSpace(fileContents))
		{
			obj = JsonConvert.DeserializeObject<T>(fileContents, _jsonSettings);
		}
		else
		{
			obj = default;
		}

		if (obj is ISaveObject saveObject)
		{
			saveObject.Handler = this;
		}

		if (obj is IExtendedSaveObject extendedSaveObject)
		{
			extendedSaveObject.OnLoad(filePath);
		}

		return obj;
	}

	public dynamic LoadRaw(string fileName, string appName = null, bool local = false)
	{
		var filePath = GetPath(fileName, appName, local);
		var fileContents = Read(filePath);

		var settings = new JsonSerializerSettings();
		settings.Error += (o, args) => args.ErrorContext.Handled = true;

		if (!string.IsNullOrWhiteSpace(fileContents))
		{
			return JsonConvert.DeserializeObject<dynamic>(fileContents, settings);
		}

		return null;
	}
	#endregion

	#region Saving
	public void Save<T>(T saveObject) where T : ISaveObject
	{
#if NET47
		var saveName = typeof(T).GetCustomAttribute<SaveNameAttribute>(false);
#else
		var saveName = typeof(T).GetCustomAttributes(typeof(SaveNameAttribute), false).FirstOrDefault() as SaveNameAttribute;
#endif
		var filePath = GetPath(saveName.FileName, saveName.AppName, saveName.Local);
		var extendedSaveObject = saveObject as IExtendedSaveObject;

		extendedSaveObject?.OnPreSave(filePath);

		Write(filePath
			, JsonConvert.SerializeObject(saveObject, Formatting.Indented, saveName.SuppressErrors ? _jsonSettings : _jsonSettingsAllowErrors)
			, saveName.NoBackup);

		extendedSaveObject?.OnPostSave(filePath);
	}

	public void Save(object saveObject, string fileName, bool suppressErrors = false, string appName = null, bool local = false, bool noBackup = false)
	{
		if (string.IsNullOrWhiteSpace(fileName))
		{
			throw new MissingFieldException("FileName must be provided");
		}

		var filePath = GetPath(fileName, appName, local);
		var extendedSaveObject = saveObject as IExtendedSaveObject;

		extendedSaveObject?.OnPreSave(filePath);

		Write(filePath
			, JsonConvert.SerializeObject(saveObject, Formatting.Indented, suppressErrors ? _jsonSettings : _jsonSettingsAllowErrors)
			, noBackup);

		extendedSaveObject?.OnPostSave(filePath);
	}

	public void Save(object saveObject, Type[] specificType, string fileName, bool suppressErrors = false, string appName = null, bool local = false, bool noBackup = false)
	{
		if (string.IsNullOrWhiteSpace(fileName))
		{
			throw new MissingFieldException("FileName must be provided");
		}

		var filePath = GetPath(fileName, appName, local);
		var extendedSaveObject = saveObject as IExtendedSaveObject;
		var settings = new JsonSerializerSettings
		{
			ContractResolver = new InterfaceContractResolver(specificType)
		};

		if (suppressErrors)
		{
			settings.Error += (o, args) => args.ErrorContext.Handled = true;
		}

		extendedSaveObject?.OnPreSave(filePath);

		Write(filePath
			, JsonConvert.SerializeObject(saveObject, Formatting.Indented, settings)
			, noBackup);

		extendedSaveObject?.OnPostSave(filePath);
	}
	#endregion

	#region Other
	public void Delete<T>() where T : ISaveObject
	{
#if NET47
		var saveName = typeof(T).GetCustomAttribute<SaveNameAttribute>(false);
#else
		var saveName = typeof(T).GetCustomAttributes(typeof(SaveNameAttribute), false).FirstOrDefault() as SaveNameAttribute;
#endif
		var filePath = GetPath(saveName.FileName, saveName.AppName, saveName.Local);

		if (CrossIO.FileExists(filePath))
		{
			CrossIO.DeleteFile(filePath);
		}

		if (CrossIO.FileExists(filePath + ".bak"))
		{
			CrossIO.DeleteFile(filePath + ".bak");
		}
	}

	public void Delete(string fileName, string appName = null, bool local = false)
	{
		var filePath = GetPath(fileName, appName, local);

		if (CrossIO.FileExists(filePath))
		{
			CrossIO.DeleteFile(filePath);
		}

		if (CrossIO.FileExists(filePath + ".bak"))
		{
			CrossIO.DeleteFile(filePath + ".bak");
		}
	}
	#endregion

	#region IO Implementation
	private static readonly Dictionary<string, object> _lockObjects = [];

	private static object LockObj(string path)
	{
		lock (_lockObjects)
		{
			if (!_lockObjects.ContainsKey(path))
			{
				_lockObjects.Add(path, new object());
			}

			return _lockObjects[path];
		}
	}

	public string Read(string path)
	{
		lock (LockObj(path))
		{
			var pathExists = CrossIO.FileExists(path);
			if (!pathExists && !CrossIO.FileExists($"{path}.bak"))
			{
				return null;
			}

			var tries = 3;

			retry:
			try
			{
				return File.ReadAllText(tries <= 1 || !pathExists ? $"{path}.bak" : path);
			}
			catch
			{
				if (--tries > 0)
				{
					Thread.Sleep(1500);
					goto retry;
				}

				throw;
			}
		}
	}

	public void Write(string path, string content, bool noBackup)
	{
		lock (LockObj(path))
		{
			var guid = Guid.NewGuid();
			var tries = 3;
			var parent = Directory.GetParent(path);
			var temp = CrossIO.Combine(parent.FullName, $"{guid}.tmp");

			noBackup |= CrossIO.CurrentPlatform == Platform.MacOSX;

			retry:
			try
			{
				parent.Create();

				File.WriteAllText(noBackup ? path : temp, content);

				if (!noBackup)
				{
					if (CrossIO.FileExists(path))
					{
						File.Replace(temp, path, $"{path}.bak");
						File.SetAttributes($"{path}.bak", FileAttributes.System | FileAttributes.Hidden);
					}
					else
					{
						File.Move(temp, path);
					}
				}
			}
			catch
			{
				if (--tries > 0)
				{
					Thread.Sleep(1500);
					goto retry;
				}

				throw;
			}
			finally
			{
				if (!noBackup && CrossIO.FileExists(temp))
				{
					CrossIO.DeleteFile(temp, true);
				}
			}
		}
	}

	public string GetPath(string name, string appName = null, bool local = false)
	{
#if SIMPLE
		var basePath =  SaveDirectory;
#else
		var basePath = local ? Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) : SaveDirectory;
#endif

		return CrossIO.Combine(basePath, appName ?? AppName, name);
	}
#endregion Other
}

public static class SaveHandlerExtensions
{
	public static void Save<T>(this T saveObject) where T : ISaveObject
	{
		saveObject.Handler.Save(saveObject);
	}
}

public interface ISaveObject
{
	SaveHandler Handler { get; set; }
}

public interface IExtendedSaveObject : ISaveObject
{
	void OnPreSave(string filePath);
	void OnPostSave(string filePath);
	void OnLoad(string filePath);
	void Reset();
	void Reload();
}

public class SaveNameAttribute(string fileName, string appName = null, bool noBackup = false, bool local = false, bool suppressErrors = false) : Attribute
{
	public string FileName { get; } = fileName is null or "" ? throw new MissingFieldException("FileName must be provided") : fileName;
	public string AppName { get; } = appName;
	public bool NoBackup { get; } = noBackup;
	public bool Local { get; } = local;
	public bool SuppressErrors { get; } = suppressErrors;
}

public class SaveHandlerContractResolver : DefaultContractResolver
{
	protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
	{
		var property = base.CreateProperty(member, memberSerialization);

		if (property.PropertyType == typeof(SaveHandler))
		{
			property.Ignored = true;
			property.ShouldDeserialize = (_) => false;
			property.ShouldSerialize = (_) => false;
		}

		return property;
	}
}
