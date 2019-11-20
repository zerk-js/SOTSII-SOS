// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.MissionInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.StarFleet;

namespace Kerberos.Sots.Data
{
	internal class MissionInfo : IIDProvider
	{
		public int FleetID;
		public MissionType Type;
		public int TargetSystemID;
		public int TargetOrbitalObjectID;
		public int TargetFleetID;
		public int Duration;
		public bool UseDirectRoute;
		public int TurnStarted;
		public int StartingSystem;
		public int? StationType;

		public int ID { get; set; }
	}
}
