// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.StarFleet.MissionEstimate
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.StarFleet
{
	internal class MissionEstimate
	{
		public int TurnsToTarget;
		public int TurnsToReturn;
		public int TurnsAtTarget;
		public int TurnsForConstruction;
		public float ConstructionCost;
		public int TripsTillSelfSufficeincy;
		public int TurnsTillPhase1Completion;

		public int TotalTurns
		{
			get
			{
				return 0 + this.TurnsForConstruction + this.TurnsToTarget + this.TurnsAtTarget + this.TurnsToReturn + this.TurnsColonySupport;
			}
		}

		public int TurnsColonySupport
		{
			get
			{
				if (this.TripsTillSelfSufficeincy > 0)
					return (this.TurnsToTarget + this.TurnsToReturn) * this.TripsTillSelfSufficeincy;
				return 0;
			}
		}

		public int TotalTravelTurns
		{
			get
			{
				return 0 + this.TurnsToTarget + this.TurnsToReturn;
			}
		}
	}
}
