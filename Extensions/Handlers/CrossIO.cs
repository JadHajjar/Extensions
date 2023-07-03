using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extensions
{
	public static class CrossIO
	{
		public static Platform CurrentPlatform { get; set; }
		public static string PathSeparator => CurrentPlatform == Platform.Windows ? "\\" : "/";
		public static string InvalidPathSeparator => CurrentPlatform != Platform.Windows ? "\\" : "/";

		public static string FormatPath(this string path)
		{
			return path.TrimEnd('/', '\\').Replace(InvalidPathSeparator, PathSeparator);
		}

		public static string Combine(params string[] paths)
		{
			if (paths.Length == 0)
			{
				return string.Empty;
			}

			var sb = new StringBuilder(paths[0].TrimEnd('/', '\\'));

			for (var i = 1; i < paths.Length; i++)
			{
				if (!string.IsNullOrWhiteSpace(paths[i]))
				{
					sb.Append(PathSeparator);

					sb.Append(paths[i].TrimEnd('/', '\\'));
				}
			}

			return sb.ToString();
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
			if (CurrentPlatform != Platform.Windows)
			{
				try
				{ return Directory.GetFiles(Path.GetDirectoryName(path).Replace('\\', '/'), Path.GetFileName(path)).Length != 0; }
				catch { }
			}

			return File.Exists(path);
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
			if (CurrentPlatform is Platform.MacOSX)
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
			if (CurrentPlatform is Platform.MacOSX)
			{
				try
				{
					File.Delete(file);
				}
				catch { }
			}
			else if (completely || CurrentPlatform is Platform.Linux)
			{
				File.Delete(file);
			}
			else
			{
				FileOperationAPIWrapper.MoveToRecycleBin(file);
			}
		}

		public static void CopyFile(string file, string fileTo, bool overwrite)
		{
			if (CurrentPlatform is Platform.MacOSX)
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

			var normalizedPath1 = path1.Replace(InvalidPathSeparator, PathSeparator);
			var normalizedPath2 = path2.Replace(InvalidPathSeparator, PathSeparator);

			return string.Equals(normalizedPath1, normalizedPath2, StringComparison.OrdinalIgnoreCase);
		}

		public static bool PathContains(this string path1, string path2)
		{
			var normalizedPath1 = path1.Replace(InvalidPathSeparator, PathSeparator);
			var normalizedPath2 = path2.Replace(InvalidPathSeparator, PathSeparator);

			return normalizedPath1.IndexOf(normalizedPath2, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		public static string PathReplace(this string path1, string path2, string path)
		{
			var normalizedPath1 = path1.Replace(InvalidPathSeparator, PathSeparator);
			var normalizedPath2 = path2.Replace(InvalidPathSeparator, PathSeparator);

			var index = normalizedPath1.IndexOf(normalizedPath2, StringComparison.OrdinalIgnoreCase);

			if (index != 0)
			{
				return path1;
			}

			return path1.Remove(index, normalizedPath2.Length).Insert(index, path);
		}
	}
}
