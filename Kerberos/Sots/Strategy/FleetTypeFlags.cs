// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.FleetTypeFlags
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.Strategy
{
	public enum FleetTypeFlags
	{
		UNKNOWN = 0,
		COMBAT = 1,
		PLANETATTACK = 2,
		PATROL = 4,
		DEFEND = 8,
		COLONIZE = 16, // 0x00000010
		CONSTRUCTION = 32, // 0x00000020
		SURVEY = 64, // 0x00000040
		GATE = 128, // 0x00000080
		NPG = 256, // 0x00000100
		ANY = 511, // 0x000001FF
	}
}
