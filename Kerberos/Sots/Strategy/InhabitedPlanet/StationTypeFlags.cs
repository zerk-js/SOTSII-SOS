// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.InhabitedPlanet.StationTypeFlags
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.Strategy.InhabitedPlanet
{
	[System.Flags]
	public enum StationTypeFlags
	{
		NAVAL = 2,
		SCIENCE = 4,
		CIVILIAN = 8,
		DIPLOMATIC = 16, // 0x00000010
		GATE = 32, // 0x00000020
		MINING = 64, // 0x00000040
		DEFENCE = 128, // 0x00000080
	}
}
