// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.CommonStrings
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Kerberos.Sots.Data
{
	public class CommonStrings : Dictionary<string, string>
	{
		private string _gameRoot;
		private string _forcedLanguage;

		public string TwoLetterISOLanguageName { get; private set; }

		public string Directory { get; private set; }

		public string UnrootedDirectory { get; private set; }

		public void Reload()
		{
			this.Clear();
			string twoLetterISOLanguageName = this._forcedLanguage ?? Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
			if (App.Log != null)
				App.Log.Trace("Reloading common strings for locale: " + twoLetterISOLanguageName, "data");
			string localeDirectory = CommonStrings.GetLocaleDirectory(twoLetterISOLanguageName);
			string str1 = Path.Combine(localeDirectory, "strings.csv");
			string str2 = this._gameRoot != null ? Path.Combine(this._gameRoot, str1) : str1;
			if ((this._gameRoot != null ? (!File.Exists(str2) ? 1 : 0) : (!ScriptHost.FileSystem.FileExists(str1) ? 1 : 0)) != 0)
			{
				if (App.Log != null)
					App.Log.Warn("No locale strings.csv found for language '" + twoLetterISOLanguageName + "'. Defaulting to 'en'.", "data");
				twoLetterISOLanguageName = "en";
				localeDirectory = CommonStrings.GetLocaleDirectory(twoLetterISOLanguageName);
				string path2 = Path.Combine(localeDirectory, "strings.csv");
				str2 = this._gameRoot != null ? Path.Combine(this._gameRoot, path2) : path2;
			}
			this.TwoLetterISOLanguageName = twoLetterISOLanguageName;
			this.UnrootedDirectory = localeDirectory;
			this.Directory = this._gameRoot != null ? Path.Combine(this._gameRoot, localeDirectory) : localeDirectory;
			this.MergeCsv(str2);
			this.MergeCsv(Path.Combine(this.Directory, "speech.csv"));
		}

		private void Construct(string twoLetterISOLanguageName)
		{
			this._forcedLanguage = twoLetterISOLanguageName;
			this.Reload();
		}

		public CommonStrings(string twoLetterISOLanguageName, string gameRoot)
		{
			this._gameRoot = gameRoot;
			this.Construct(twoLetterISOLanguageName);
		}

		public CommonStrings(string twoLetterISOLanguageName)
		{
			this.Construct(twoLetterISOLanguageName);
		}

		public bool Localize(string id, out string localized)
		{
			localized = string.Empty;
			if (string.IsNullOrEmpty(id))
				return false;
			lock (this)
			{
				string str;
				if (id[0] == '@' && this.TryGetValue(id.Substring(1), out str))
				{
					localized = str;
					return true;
				}
				localized = "*" + id;
				return false;
			}
		}

		public string Localize(string id)
		{
			string localized;
			this.Localize(id, out localized);
			return localized;
		}

		private void MergeCsv(string filename)
		{
			foreach (string[] strArray in this._gameRoot == null ? CsvOperations.Read(ScriptHost.FileSystem, filename, '"', ',', 0, 2) : CsvOperations.Read(filename, '"', ',', 0, 2))
			{
				if (strArray.Length != 0)
				{
					string key = strArray[0].Trim();
					if (key.Length != 0)
					{
						if (this.ContainsKey(key))
							throw new InvalidDataException(string.Format("Duplicate ID '{0}' found in '{1}'.", (object)key, (object)filename));
						string str = strArray.Length <= 1 ? string.Empty : strArray[1].Trim();
						this[key] = str;
					}
				}
			}
		}

		private static string GetLocaleDirectory(string twoLetterISOLanguageName)
		{
			return Path.Combine("locale\\" + twoLetterISOLanguageName);
		}

		public static string GetRootLocaleDirectory(string gameRoot)
		{
			return Path.Combine(gameRoot, "locale");
		}
	}
}
