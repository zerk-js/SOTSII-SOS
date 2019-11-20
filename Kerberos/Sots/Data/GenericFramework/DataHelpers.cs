// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.GenericFramework.DataHelpers
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System.Collections.Generic;
using System.IO;

namespace Kerberos.Sots.Data.GenericFramework
{
	public static class DataHelpers
	{
		public static IEnumerable<string> EnumerateFactionFileNamesForTools(
		  string gameRoot,
		  string factionName)
		{
			foreach (string enumerateDirectory in Directory.EnumerateDirectories(Path.Combine(gameRoot, "factions")))
			{
				if (factionName == null || Path.GetFileName(enumerateDirectory) == factionName)
				{
					string filename = Path.Combine(enumerateDirectory, "faction.xml");
					if (File.Exists(filename))
						yield return filename;
				}
			}
		}

		public static IEnumerable<string> EnumerateRaceFileNames()
		{
			foreach (string directory in ScriptHost.FileSystem.FindDirectories("races\\*"))
			{
				string filename = Path.Combine(directory, "race.xml");
				if (ScriptHost.FileSystem.FileExists(filename))
					yield return filename;
			}
		}
	}
}
