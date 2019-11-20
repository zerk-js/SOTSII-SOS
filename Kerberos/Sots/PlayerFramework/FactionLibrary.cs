// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.PlayerFramework.FactionLibrary
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kerberos.Sots.PlayerFramework
{
	internal static class FactionLibrary
	{
		public static IEnumerable<Faction> Enumerate()
		{
			HashSet<string> factionDirs = new HashSet<string>(((IEnumerable<string>)ScriptHost.FileSystem.FindDirectories("factions\\*")).Select<string, string>((Func<string, string>)(x => FileSystemHelpers.StripMountName(x))));
			foreach (string path1 in factionDirs)
			{
				string filename = Path.Combine(path1, "faction.xml");
				if (ScriptHost.FileSystem.FileExists(filename))
				{
					Faction faction = Faction.LoadXml(filename);
					yield return faction;
				}
			}
		}
	}
}
