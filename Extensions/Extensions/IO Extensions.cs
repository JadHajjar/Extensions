using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Extensions
{
	public static partial class ExtensionClass
	{
		public static readonly char[] CharBlackList =
		{
			'\\', '/', ':', '?', '<', '>', '|'
		};

		public static string CharBlackListPattern = $"[{Regex.Escape(CharBlackList.ListStrings())}]";

		public static string AbreviatedPath(this FileInfo file, bool folder = false)
		{
			return AbreviatedPath(folder ? file.DirectoryName : file.FullName);
		}

		public static string AbreviatedPath(this DirectoryInfo folder)
		{
			return AbreviatedPath(folder.FullName);
		}

		public static void MoveFolder(string original, string target, bool overwrite)
		{
			var sourceDir = new DirectoryInfo(original);
			var targetDir = new DirectoryInfo(target);

			// Create the target directory if it doesn't exist
			if (!targetDir.Exists)
			{
				targetDir.Create();
			}

			// Move files
			foreach (var file in sourceDir.GetFiles())
			{
				var targetFilePath = Path.Combine(targetDir.FullName, file.Name);

				// Check if the file already exists in the target directory
				if (FileExists(targetFilePath))
				{
					if (overwrite)
					{
						DeleteFile(targetFilePath);
					}
					else
					{
						// Skip this file if it already exists
						continue;
					}
				}

				// Move the file to the target directory
				file.MoveTo(targetFilePath);
			}

			// Move directories recursively
			foreach (var subDir in sourceDir.GetDirectories())
			{
				var targetSubDirPath = Path.Combine(targetDir.FullName, subDir.Name);

				// Move the subdirectory recursively
				MoveFolder(subDir.FullName, targetSubDirPath, overwrite);
			}
		}

		public static bool FileExists(string path)
		{
			if (ISave.CurrentPlatform != Platform.Windows)
			{
				try
				{ return Directory.GetFiles(Path.GetDirectoryName(path).Replace('\\', '/'), Path.GetFileName(path)).Length != 0; }
				catch { }
			}

			return File.Exists(path);
		}

		public static bool IsNetwork(this DirectoryInfo folder)
		{
			if (!folder.FullName.StartsWith(@"/") && !folder.FullName.StartsWith(@"\"))
			{
				var rootPath = Path.GetPathRoot(folder.FullName); // get drive's letter
				var driveInfo = new DriveInfo(rootPath); // get info about the drive

				return driveInfo.DriveType == DriveType.Network; // return true if a network drive
			}

			return true;
		}

		public static bool DeleteFolder(string folderPath)
		{
			var res = true;
			// Delete all files inside the folder
			foreach (var file in Directory.GetFiles(folderPath))
			{
				DeleteFile(file);
			}

			// Recursively delete all subdirectories inside the folder
			foreach (var subDir in Directory.GetDirectories(folderPath))
			{
				res &= DeleteFolder(subDir);
			}

			// Finally, delete the main folder
			if (ISave.CurrentPlatform is Platform.MacOSX)
			{
				try
				{
					Directory.Delete(folderPath);
				}
				catch { }
			}
			else
			{
				Directory.Delete(folderPath);
			}

			return res && !Directory.Exists(folderPath);
		}

		public static void DeleteFile(string file, bool completely = false)
		{
			if (ISave.CurrentPlatform is Platform.MacOSX)
			{
				try
				{
					File.Delete(file);
				}
				catch { }
			}
			else if (completely || ISave.CurrentPlatform is Platform.Linux)
			{
				File.Delete(file);
			}
			else
			{
				FileOperationAPIWrapper .MoveToRecycleBin(file);
			}
		}

		public static void CopyFile(string file, string fileTo, bool overwrite)
		{
			if (ISave.CurrentPlatform is Platform.MacOSX)
			{
				try
				{
					File.Copy(file, fileTo, overwrite);
				}
				catch { }
			}
			else
			{
				File.Copy(file, fileTo, overwrite);
			}
		}

		public static bool PathEquals(this string path1, string path2)
		{
			if (path1 == path2)
			{
				return true;
			}

			var normalizedPath1 = path1.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
			var normalizedPath2 = path2.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

			return string.Equals(normalizedPath1, normalizedPath2, StringComparison.OrdinalIgnoreCase);
		}

		public static bool PathContains(this string path1, string path2)
		{
			var normalizedPath1 = path1.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
			var normalizedPath2 = path2.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

			return normalizedPath1.IndexOf(normalizedPath2, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		public static string PathReplace(this string path1, string path2, string path)
		{
			var normalizedPath1 = path1.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
			var normalizedPath2 = path2.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

			var index = normalizedPath1.IndexOf(normalizedPath2, StringComparison.OrdinalIgnoreCase);

			if (index != 0)
			{
				return path1;
			}

			return path1.Remove(index, normalizedPath2.Length).Insert(index, path);
		}

		/// <summary>
		/// Creates a windows Shortcut (.lnk)
		/// </summary>
		/// <param name="shortcut">Path of the Shortcut to create</param>
		/// <param name="targetPath">Reference Path of the Shortcut</param>
		public static void CreateShortcut(string shortcut, string targetPath, string arguments = "", string description = "")
		{
			var shell = new IWshRuntimeLibrary.WshShell();

			try
			{
				var lnk = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcut);

				try
				{
					lnk.WorkingDirectory = Directory.GetParent(targetPath).FullName;
					lnk.TargetPath = targetPath;
					lnk.Arguments = arguments;
					lnk.Description = description;
					lnk.Save();
				}
				finally
				{
					Marshal.FinalReleaseComObject(lnk);
				}
			}
			finally
			{
				Marshal.FinalReleaseComObject(shell);
			}
		}

		/// <summary>
		/// Creates a Shortcut for the file at the <paramref name="shortcutPath"/>
		/// </summary>
		public static void CreateShortcut(this FileInfo file, string shortcutPath, string arguments = "", string description = "")
		{
			CreateShortcut(shortcutPath, file.FullName, arguments, description);
		}

		public static string EscapeFileName(this string path)
		{
			return path.RegexRemove(CharBlackListPattern).Replace('"', '\'').Replace("*", " ");
		}

		/// <summary>
		/// Returns the Name of the file without its Extension
		/// </summary>
		public static string FileName(this FileInfo file)
		{
			return file.Name.Substring(0, file.Name.LastIndexOf(file.Extension, StringComparison.InvariantCultureIgnoreCase));
		}

		/// <summary>
		/// Returns all Directories in the path within the selected <paramref name="layers"/>
		/// </summary>
		public static string[] GetDirectories(string path, string pattern, int layers)
		{
			if (layers == 0)
			{
				return new string[0];
			}

			var Out = new List<string>();
			Out.AddRange(Directory.GetDirectories(path, pattern, SearchOption.TopDirectoryOnly));

			foreach (var item in Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly))
			{
				Out.AddRange(GetDirectories(item, pattern, layers - 1));
			}

			return Out.ToArray();
		}

		/// <summary>
		/// Returns all Files in the path within the selected <paramref name="layers"/>
		/// </summary>
		public static IEnumerable<FileInfo> GetFiles(this DirectoryInfo path, string pattern, int layers)
		{
			if (layers != 0)
			{
				foreach (var item in path.GetFiles(pattern, SearchOption.TopDirectoryOnly))
				{
					yield return item;
				}

				foreach (var dir in path.GetDirectories("*", SearchOption.TopDirectoryOnly))
				{
					foreach (var item in GetFiles(dir, pattern, layers - 1))
					{
						yield return item;
					}
				}
			}
		}

		/// <summary>
		/// Returns all Files in the path within the selected <paramref name="layers"/>
		/// </summary>
		public static IEnumerable<FileInfo> EnumerateFiles(this DirectoryInfo path, string pattern, int layers)
		{
			if (layers != 0)
			{
				foreach (var item in path.EnumerateFiles(pattern, SearchOption.TopDirectoryOnly))
				{
					yield return item;
				}

				foreach (var dir in path.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
				{
					foreach (var item in EnumerateFiles(dir, pattern, layers - 1))
					{
						yield return item;
					}
				}
			}
		}

		/// <summary>
		/// Returns all Files in the path within the selected <paramref name="layers"/>
		/// </summary>
		public static IEnumerable<string> EnumerateFiles(this string path, string pattern, int layers)
		{
			if (layers != 0)
			{
				foreach (var item in Directory.EnumerateFiles(path, pattern, SearchOption.TopDirectoryOnly))
				{
					yield return item;
				}

				if (layers > 1)
				{
					foreach (var dir in Directory.EnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly))
					{
						foreach (var item in EnumerateFiles(dir, pattern, layers - 1))
						{
							yield return item;
						}
					}
				}
			}
		}

		/// <summary>
		/// Returns all Files in the path within the selected <paramref name="layers"/>
		/// </summary>
		public static IEnumerable<string> GetFiles(this string path, string pattern, int layers)
		{
			if (layers != 0)
			{
				foreach (var item in Directory.GetFiles(path, pattern, SearchOption.TopDirectoryOnly))
				{
					yield return item;
				}

				if (layers > 1)
				{
					foreach (var dir in Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly))
					{
						foreach (var item in GetFiles(dir, pattern, layers - 1))
						{
							yield return item;
						}
					}
				}
			}
		}

		/// <summary>
		/// Gets all files in a directory that have any of the <paramref name="extensions"/> selected
		/// </summary>
		public static IEnumerable<FileInfo> GetFilesByExtensions(this DirectoryInfo dir, SearchOption searchOption, params string[] extensions)
		{
			if (dir.Exists)
			{
				foreach (var f in dir.EnumerateFiles("*.*", searchOption))
				{
					if (extensions.Any(x => x.Equals(f.Extension, StringComparison.InvariantCultureIgnoreCase)))
					{
						yield return f;
					}
				}
			}
		}

		/// <summary>
		/// Gets all files in a directory that have any of the <paramref name="extensions"/> selected
		/// </summary>
		public static IEnumerable<FileInfo> GetFilesByExtensions(this DirectoryInfo dir, params string[] extensions)
		{
			if (dir.Exists)
			{
				foreach (var f in dir.EnumerateFiles("*.*", SearchOption.AllDirectories))
				{
					if (extensions.Any(x => x.Equals(f.Extension, StringComparison.InvariantCultureIgnoreCase)))
					{
						yield return f;
					}
				}
			}
		}

		/// <summary>
		/// Gets the Path of the Shortcut's Target
		/// </summary>
		public static string GetShortcutPath(this string Shortcut)
		{
			if (File.Exists(Shortcut))
			{
				// Add Reference > COM > Windows Script Host Object Model > OK
				var shell = new IWshRuntimeLibrary.WshShell(); //Create a new WshShell Interface
				var link = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(Shortcut); //Link the interface to our shortcut

				return link.TargetPath; //Show the target in a MessagePrompt using IWshShortcut
			}
			return "";
		}

		/// <summary>
		/// Returns the <see cref="FileInfo"/> of the Target Path of a shortcut
		/// </summary>
		public static FileInfo GetShortcutPath(this FileInfo file)
		{
			return new FileInfo(GetShortcutPath(file.FullName));
		}

		public static bool IsFileLocked(this FileInfo file)
		{
			FileStream stream = null;

			try
			{
				stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
			}
			catch (IOException)
			{
				//the file is unavailable because it is:
				//still being written to
				//or being processed by another thread
				//or does not exist (has already been processed)
				return true;
			}
			finally
			{
				if (stream != null)
				{
					stream.Close();
				}
			}

			//file is not locked
			return false;
		}

		/// <summary>
		/// Checks if the <see cref="string"/> Path is a Folder
		/// </summary>
		/// <param name="CheckShortcuts">Checks if the Target of a shortcut is a Folder too</param>
		public static bool IsFolder(this string S, bool CheckShortcuts = true)
		{
			if (CheckShortcuts && S.EndsWith(".lnk"))
			{
				S = S.GetShortcutPath();
			}

			return Directory.Exists(S) && File.GetAttributes(S).HasFlag(FileAttributes.Directory);
		}

		/// <summary>
		/// Gets an <see cref="Array"/> containing the parents of a path
		/// </summary>
		/// <param name="depth">controls the amount of parents needed, keep null to return all parents</param>
		public static List<string> Parents(this string path, bool fullpath = false, int? depth = null)
		{
			if (!Directory.Exists(path) && !File.Exists(path))
			{
				return new List<string>();
			}

			var Out = new List<string>();
			var P = new DirectoryInfo(path);

			while ((P = P.Parent) != null && (depth == null || depth != Out.Count))
			{
				Out.Add(fullpath ? P.FullName : P.Name);
			}

			return Out;
		}

		private static string AbreviatedPath(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return string.Empty;
			}

			var items = path.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
			var selectedItems = new List<string>() { items[0] };

			selectedItems.AddRange(items.TakeLast(Math.Min(3, items.Length - 1)));

			if (items.Length != selectedItems.Count)
			{
				selectedItems.Insert(1, "..");
			}

			return selectedItems.ListStrings("\\");
		}

		public static void CopyAll(this DirectoryInfo directory, DirectoryInfo target, Func<string, bool> fileTest = null)
		{
			if (!directory.Exists)
			{
				return;
			}

			target.Create();

			//Now Create all of the directories
			foreach (var dirPath in Directory.GetDirectories(directory.FullName, "*", SearchOption.AllDirectories))
			{
				Directory.CreateDirectory(dirPath.Replace(directory.FullName, target.FullName));
			}

			//Copy all the files & Replaces any files with the same name
			foreach (var newPath in Directory.GetFiles(directory.FullName, "*.*", SearchOption.AllDirectories))
			{
				if (fileTest == null || fileTest(newPath))
				{
					File.Copy(newPath, newPath.Replace(directory.FullName, target.FullName), true);
				}
			}
		}

		public static void RemoveEmptyFolders(this DirectoryInfo folderPath)
		{
			// Check if the folder exists
			if (!folderPath.Exists)
			{
				throw new DirectoryNotFoundException($"The folder {folderPath} does not exist.");
			}

			// Remove empty subfolders
			foreach (var subdirectory in folderPath.GetDirectories())
			{
				RemoveEmptyFolders(subdirectory);
			}

			// Delete current folder if empty
			if (!folderPath.EnumerateFiles().Any() && !folderPath.EnumerateDirectories().Any())
			{
				folderPath.Delete();
			}
		}
	}
}