// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.InhabitedPlanet.StationTypeExtensions
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.Strategy.InhabitedPlanet
{
	public static class StationTypeExtensions
	{
		public static string ToDisplayText(this StationType stationType, string faction)
		{
			switch (stationType)
			{
				case StationType.NAVAL:
					return "Naval";
				case StationType.SCIENCE:
					return "Science";
				case StationType.CIVILIAN:
					return !(faction == "zuul") ? "Civilian" : "Slave";
				case StationType.DIPLOMATIC:
					return !(faction == "zuul") ? "Diplomatic" : "Tribute";
				case StationType.GATE:
					return "Gate";
				case StationType.MINING:
					return "Mining";
				case StationType.DEFENCE:
					return "Defence";
				default:
					return "Unknown";
			}
		}
	}
}
