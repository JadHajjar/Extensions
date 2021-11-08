using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using static System.Environment;

namespace Extensions
{
	public class ISave
	{
		public static string AppName { get; set; } = Application.ProductName;

		public static string DocsFolder => Path.Combine(CustomSaveDirectory.IfEmpty(GetFolderPath(SpecialFolder.MyDocuments)), AppName);

		public static string CustomSaveDirectory { get; set; }

		public virtual string Name { get; set; }

		public virtual void OnLoad()
		{ }

		#region Load

		public static T Load<T>(string name, string appName = null, bool local = false) where T : ISave, new()
			=> Load<T>(null, name, appName, local);

		public static T Load<T>(T obj, string name, string appName = null, bool local = false) where T : ISave, new()
		{
			var doc = GetPath(name, appName, local);
			var settings = new JsonSerializerSettings();
			settings.Error += (o, args) =>
			{
				args.ErrorContext.Handled = true;
			};

			var text = Read(doc);

			if (!string.IsNullOrWhiteSpace(text))
			{
				if (obj == null)
					obj = JsonConvert.DeserializeObject<T>(text, settings);
				else
					JsonConvert.PopulateObject(text, obj, settings);

				obj.OnLoad();

				return obj;
			}

			return obj ?? new T() { Name = name };
		}

		public static void Load<T>(out T obj, string name, string appName = null, bool local = false)
		{
			var doc = GetPath(name, appName, local);

			if (File.Exists(doc) && new FileInfo(doc).Length > 0)
				obj = JsonConvert.DeserializeObject<T>(Read(doc));
			else
				obj = default;
		}

		public static dynamic LoadRaw(string name, string appName = null, bool local = false)
		{
			var doc = GetPath(name, appName, local);

			if (File.Exists(doc))
				return JsonConvert.DeserializeObject<dynamic>(Read(doc));
			else
				return null;
		}

		#endregion Load

		#region Save

		public void Save(string name = null, bool supressErrors = false, string appName = null, bool local = false, bool noBackup = false)
			=> Save(null, name, supressErrors, appName, local, noBackup);

		public void Save(Type[] specificType, string name = null, bool supressErrors = false, string appName = null, bool local = false, bool noBackup = false)
		{
			if (string.IsNullOrWhiteSpace(name.IfEmpty(Name)))
				throw new MissingFieldException("ISave", "Name");

			var doc = GetPath(name.IfEmpty(Name), appName, local);
			var settings = new JsonSerializerSettings();

			if (supressErrors)
				settings.Error += (o, args) => args.ErrorContext.Handled = true;

			if (specificType != null)
				settings.ContractResolver = new InterfaceContractResolver(specificType);

			Write(doc, JsonConvert.SerializeObject(this, Formatting.Indented, settings), noBackup);
		}

		public static void Save(object obj, string name, bool supressErrors = false, string appName = null, bool local = false, bool noBackup = false)
			=> Save(obj, null, name, supressErrors, appName, local, noBackup);

		public static void Save(object obj, Type[] specificType, string name, bool supressErrors = false, string appName = null, bool local = false, bool noBackup = false)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new MissingFieldException("ISave", "Name");

			var doc = GetPath(name, appName, local);
			var settings = new JsonSerializerSettings();

			if (supressErrors)
				settings.Error += (o, args) => args.ErrorContext.Handled = true;

			if (specificType != null)
				settings.ContractResolver = new InterfaceContractResolver(specificType);

			Write(doc, JsonConvert.SerializeObject(obj, Formatting.Indented, settings), noBackup);
		}

		#endregion Save

		#region Other

		private static readonly Dictionary<string, object> lockObjects = new Dictionary<string, object>();

		private static object lockObj(string path)
		{
			lock (lockObjects)
			{
				if (!lockObjects.ContainsKey(path))
					lockObjects.Add(path, new object());

				return lockObjects[path];
			}
		}

		public void Delete(string name = null, string appName = null, bool local = false)
		{
			if (string.IsNullOrWhiteSpace(name.IfEmpty(Name)))
				throw new MissingFieldException("ISave", "Name");

			var doc = GetPath(name.IfEmpty(Name), appName, local);

			File.Delete(doc);
		}

		private static string Read(string path)
		{
			lock (lockObj(path))
			{
				var pathExists = File.Exists(path);
				if (!pathExists && !File.Exists($"{path}.bak")) return null;

				var tries = 3;

			retry: try
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

		private static void Write(string path, string content, bool noBackup)
		{
			lock (lockObj(path))
			{
				var guid = Guid.NewGuid();
				var tries = 3;
				var parent = Directory.GetParent(path);
				var temp = Path.Combine(parent.FullName, $"{guid}.tmp");

			retry: try
				{
					parent.Create();

					File.WriteAllText(noBackup ? path : temp, content);

					if (!noBackup)
					{
						if (File.Exists(path))
						{
							File.Replace(temp, path, $"{path}.bak");
							File.SetAttributes($"{path}.bak", FileAttributes.System | FileAttributes.Hidden);
						}
						else
							File.Move(temp, path);
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
					if (File.Exists(temp))
						File.Delete(temp);
				}
			}
		}

		public static string GetPath(string name, string appName = null, bool local = false)
			=> Path.Combine(
					local
					? Directory.GetParent(Application.ExecutablePath).FullName
					: (appName != null ? null : CustomSaveDirectory).IfEmpty(GetFolderPath(SpecialFolder.MyDocuments))
				, appName.IfEmpty(AppName), name);

		#endregion Other
	}
}