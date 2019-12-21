using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Windows.Controls;

namespace Audio_Book_Player.Code
{
	class CommonFunctions
	{
		public static string[] GetFilesInDirectory(string _dir)
		{
			return Directory.GetFiles(_dir, "*.mp3");
		}
		public static int GetStringIndex(string[] _strings, string _string)
		{
			for (int i = 0; i < _strings.Length; i++)
			{
				if (_strings[i] == _string)
					return i;
			}
			return -1;
		}
		public static string GetNextFile(string _dir, string _currentFile)
		{
			string[] files = GetFilesInDirectory(_dir);
			int index = GetStringIndex(files, _currentFile);
			if (files.Length - 1 == index)
				return files[0];
			else
				return files[index + 1];
		}
		public static string GetFormerFile(string _dir, string _currentFile)
		{
			string[] files = GetFilesInDirectory(_dir);
			int index = GetStringIndex(files, _currentFile);
			if (index == 0)
				return files[0];
			else
				return files[index - 1];
		}
		public static void AddOrUpdateAppSettings(string key, string value)
		{
			try
			{
				var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				var settings = configFile.AppSettings.Settings;
				if (settings[key] == null)
				{
					settings.Add(key, value);
				}
				else
				{
					settings[key].Value = value;
				}
				configFile.Save(ConfigurationSaveMode.Modified);
				ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
			}
			catch (ConfigurationErrorsException)
			{
				Console.WriteLine("Error writing app settings");
			}
		}

	}
}
