// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Steam.SteamDLCHelpers
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.PlayerFramework;
using System;

namespace Kerberos.Sots.Steam
{
	internal static class SteamDLCHelpers
	{
		public static SteamDLCIdentifiers? GetDLCIdentifierFromFaction(
		  Faction faction)
		{
			if (faction.Name == "human")
				return new SteamDLCIdentifiers?(SteamDLCIdentifiers.SolForceImmersionPack);
			if (faction.Name == "morrigi" || faction.Name == "liir_zuul")
				return new SteamDLCIdentifiers?(SteamDLCIdentifiers.LiirAndMorrigiImmersionPack);
			if (faction.Name == "hiver" || faction.Name == "tarkas")
				return new SteamDLCIdentifiers?(SteamDLCIdentifiers.HiverAndTarkasImmersionPack);
			if (faction.Name == "zuul")
				return new SteamDLCIdentifiers?(SteamDLCIdentifiers.TheHordeImmersionPack);
			return new SteamDLCIdentifiers?();
		}

		public static void LogAvailableDLC(App app)
		{
			App.Log.Trace("DLC content available:", "steam");
			foreach (SteamDLCIdentifiers steamDlcIdentifiers in (SteamDLCIdentifiers[])Enum.GetValues(typeof(SteamDLCIdentifiers)))
			{
				if (app.Steam.HasDLC((int)steamDlcIdentifiers))
					App.Log.Trace(string.Format("   {0}", (object)steamDlcIdentifiers.ToString()), "steam");
			}
			App.Log.Trace("End.", "steam");
		}
	}
}
