﻿// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.DiplomacyReactionHistoryEntryInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.Data
{
	internal class DiplomacyReactionHistoryEntryInfo
	{
		public int PlayerId;
		public int TowardsPlayerId;
		public int? TurnCount;
		public int Difference;
		public StratModifiers Reaction;

		public int ID { get; set; }
	}
}
