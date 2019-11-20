// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.DiplomacyActionHistoryEntryInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Strategy;

namespace Kerberos.Sots.Data
{
	internal class DiplomacyActionHistoryEntryInfo
	{
		public int PlayerId;
		public int TowardsPlayerId;
		public int? TurnCount;
		public DiplomacyAction Action;
		public int? ActionSubType;
		public float? ActionData;
		public int? Duration;
		public int? ConsequenceType;
		public float? ConsequenceData;

		public int ID { get; set; }
	}
}
