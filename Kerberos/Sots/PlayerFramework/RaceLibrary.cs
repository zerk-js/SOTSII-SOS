// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.PlayerFramework.RaceLibrary
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.GenericFramework;
using System.Collections.Generic;
using System.IO;

namespace Kerberos.Sots.PlayerFramework
{
	internal static class RaceLibrary
	{
		public static IEnumerable<Race> Enumerate()
		{
			foreach (string enumerateRaceFileName in DataHelpers.EnumerateRaceFileNames())
			{
				Race race = new Race();
				race.LoadXml(enumerateRaceFileName, Path.GetFileNameWithoutExtension(Path.GetDirectoryName(enumerateRaceFileName)));
				yield return race;
			}
		}
	}
}
