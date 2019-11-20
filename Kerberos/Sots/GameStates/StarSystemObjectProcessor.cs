// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.StarSystemObjectProcessor
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.GameStates
{
	internal class StarSystemObjectProcessor
	{
		public event StarSystemObjectProcessor.ProcessStar OnStar;

		public event StarSystemObjectProcessor.ProcessPlanet OnPlanet;

		public event StarSystemObjectProcessor.ProcessFleet OnFleet;

		public event StarSystemObjectProcessor.ProcessShip OnShip;

		public void Process(App game, int systemId)
		{
			if (systemId == 0)
				return;
			GameDatabase gameDatabase = game.GameDatabase;
			StarSystemInfo starSystemInfo = gameDatabase.GetStarSystemInfo(systemId);
			gameDatabase.GetStarSystemOrbitalObjectInfos(systemId);
			if (this.OnStar != null)
				this.OnStar(starSystemInfo);
			if (this.OnPlanet != null)
			{
				foreach (PlanetInfo systemPlanetInfo in gameDatabase.GetStarSystemPlanetInfos(systemId))
				{
					Matrix orbitalTransform = gameDatabase.GetOrbitalTransform(systemPlanetInfo.ID);
					this.OnPlanet(systemPlanetInfo, orbitalTransform);
				}
			}
			if (this.OnFleet == null && this.OnShip == null)
				return;
			IEnumerable<FleetInfo> fleetInfoBySystemId = gameDatabase.GetFleetInfoBySystemID(systemId, FleetType.FL_NORMAL);
			if (this.OnFleet != null)
			{
				foreach (FleetInfo fleetInfo in fleetInfoBySystemId)
				{
					Matrix translation = Matrix.CreateTranslation(this.GetSpawnPointForPlayer(fleetInfo.PlayerID, systemId));
					this.OnFleet(fleetInfo, translation);
				}
			}
			if (this.OnShip == null)
				return;
			Vector3 vector3_1 = new Vector3(200f, 0.0f, 0.0f);
			foreach (FleetInfo fleetInfo in fleetInfoBySystemId)
			{
				Vector3 vector3_2 = this.GetSpawnPointForPlayer(fleetInfo.PlayerID, systemId) + vector3_1 * (float)fleetInfo.ID;
				foreach (ShipInfo shipInfo in gameDatabase.GetShipInfoByFleetID(fleetInfo.ID, false).ToList<ShipInfo>())
				{
					Vector3 trans = vector3_2;
					vector3_2 += vector3_1;
					Matrix translation = Matrix.CreateTranslation(trans);
					this.OnShip(fleetInfo, shipInfo, translation);
				}
			}
		}

		public Vector3 GetSpawnPointForPlayer(int playerID, int systemID)
		{
			return new Vector3(0.0f, 0.0f, 10000f * (float)(playerID - 1));
		}

		public delegate void ProcessStar(StarSystemInfo starInfo);

		public delegate void ProcessPlanet(PlanetInfo planetInfo, Matrix world);

		public delegate void ProcessFleet(FleetInfo fleetInfo, Matrix world);

		public delegate void ProcessShip(FleetInfo fleetInfo, ShipInfo shipInfo, Matrix world);
	}
}
