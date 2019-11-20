// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.FleetType
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;

namespace Kerberos.Sots.Data
{
	[Flags]
	public enum FleetType
	{
		FL_NORMAL = 1,
		FL_RESERVE = 2,
		FL_DEFENSE = 4,
		FL_GATE = 8,
		FL_LIMBOFLEET = 16, // 0x00000010
		FL_STATION = 32, // 0x00000020
		FL_CARAVAN = 64, // 0x00000040
		FL_TRAP = 128, // 0x00000080
		FL_ACCELERATOR = 256, // 0x00000100
		FL_ALL = FL_ACCELERATOR | FL_CARAVAN | FL_GATE | FL_DEFENSE | FL_RESERVE | FL_NORMAL, // 0x0000014F
		FL_ALL_COMBAT = FL_ACCELERATOR | FL_GATE | FL_DEFENSE, // 0x0000010C
	}
}
