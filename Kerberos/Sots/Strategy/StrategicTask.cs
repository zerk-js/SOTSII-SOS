// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.StrategicTask
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System.Collections.Generic;

namespace Kerberos.Sots.Strategy
{
	internal class StrategicTask
	{
		public MissionType Mission;
		public List<FleetManagement> UseableFleets;
		public List<EasterEgg> EasterEggsAtSystem;
		public StationType StationType;
		public FleetTypeFlags RequiredFleetTypes;
		public int NumFleetsRequested;
		public int Score;
		public int SubScore;
		public int SystemIDTarget;
		public int PlanetIDTarget;
		public int FleetIDTarget;
		public int StationIDTarget;
		public int EnemyStrength;
		public int NumStandardPlayersAtSystem;
		public int SupportPointsAtSystem;
		public bool HostilesAtSystem;

		public StrategicTask()
		{
			this.Mission = MissionType.NO_MISSION;
			this.UseableFleets = new List<FleetManagement>();
			this.EasterEggsAtSystem = new List<EasterEgg>();
			this.StationType = StationType.INVALID_TYPE;
			this.NumFleetsRequested = 0;
			this.Score = 0;
			this.SubScore = 0;
			this.SystemIDTarget = 0;
			this.PlanetIDTarget = 0;
			this.FleetIDTarget = 0;
			this.StationIDTarget = 0;
			this.EnemyStrength = 0;
			this.NumStandardPlayersAtSystem = 0;
			this.SupportPointsAtSystem = 0;
			this.HostilesAtSystem = false;
		}
	}
}
