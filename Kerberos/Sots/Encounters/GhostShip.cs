// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Encounters.GhostShip
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Encounters
{
	internal class GhostShip
	{
		private int PlayerId = -1;
		private const string FactionName = "ghostship";
		private const string PlayerName = "Ghost Ship";
		private const string PlayerAvatar = "\\base\\factions\\ghostship\\avatars\\Ghostship_Avatar.tga";
		private const string FleetName = "Ghost Ship";
		private const string designFile = "lv_SFS.section";
		public static bool ForceEncounter;
		private int _designId;

		public int PlayerID
		{
			get
			{
				return this.PlayerId;
			}
		}

		public int DesignId
		{
			get
			{
				return this._designId;
			}
		}

		public static GhostShip InitializeEncounter(GameDatabase gamedb, AssetDatabase assetdb)
		{
			GhostShip ghostShip = new GhostShip();
			ghostShip.PlayerId = gamedb.InsertPlayer("Ghost Ship", "ghostship", new int?(), assetdb.RandomEncounterPrimaryColor, new Vector3(0.0f), "", "\\base\\factions\\ghostship\\avatars\\Ghostship_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			List<DesignInfo> list = gamedb.GetDesignInfosForPlayer(ghostShip.PlayerId).ToList<DesignInfo>();
			ghostShip._designId = gamedb.AddOrGetEncounterDesignInfo((IEnumerable<DesignInfo>)list, ghostShip.PlayerId, "The Flying Dutchman", "ghostship", "lv_SFS.section");
			int fleetID = gamedb.InsertFleet(ghostShip.PlayerId, 0, 0, 0, "Ghost Ship", FleetType.FL_NORMAL);
			gamedb.InsertShip(fleetID, ghostShip._designId, null, (ShipParams)0, new int?(), 0);
			return ghostShip;
		}

		public static GhostShip ResumeEncounter(GameDatabase gamedb)
		{
			GhostShip ghostShip = new GhostShip();
			PlayerInfo playerInfo = gamedb.GetPlayerInfos().ToList<PlayerInfo>().FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x =>
		   {
			   if (!x.isStandardPlayer)
				   return x.Name.Contains("Ghost Ship");
			   return false;
		   }));
			ghostShip.PlayerId = playerInfo == null ? gamedb.InsertPlayer("Ghost Ship", "ghostship", new int?(), new Vector3(0.0f), new Vector3(0.0f), "", "\\base\\factions\\ghostship\\avatars\\Ghostship_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal) : playerInfo.ID;
			List<DesignInfo> list = gamedb.GetDesignInfosForPlayer(ghostShip.PlayerId).ToList<DesignInfo>();
			ghostShip._designId = gamedb.AddOrGetEncounterDesignInfo((IEnumerable<DesignInfo>)list, ghostShip.PlayerId, "The Flying Dutchman", "ghostship", "lv_SFS.section");
			return ghostShip;
		}

		public void UpdateTurn(GameSession game)
		{
			foreach (FleetInfo fleet in game.GameDatabase.GetFleetInfosByPlayerID(this.PlayerId, FleetType.FL_NORMAL).ToList<FleetInfo>())
			{
				if (game.GameDatabase.GetShipInfoByFleetID(fleet.ID, false).ToList<ShipInfo>().Count > 0)
				{
					fleet.SystemID = 0;
					game.GameDatabase.UpdateFleetInfo(fleet);
				}
				else
					game.GameDatabase.RemoveFleet(fleet.ID);
			}
		}

		public void ExecuteEncounter(GameSession game, PlayerInfo targetPlayer, int targetSystem)
		{
			List<FleetInfo> list = game.GameDatabase.GetFleetInfosByPlayerID(this.PlayerId, FleetType.FL_NORMAL).ToList<FleetInfo>();
			if (list.Count <= 0)
				return;
			FleetInfo fleet = list.FirstOrDefault<FleetInfo>();
			fleet.SystemID = targetSystem;
			game.GameDatabase.UpdateFleetInfo(fleet);
		}

		public void AddEncounter(GameSession game, PlayerInfo targetPlayer)
		{
			Random safeRandom = App.GetSafeRandom();
			List<FleetInfo> list1 = game.GameDatabase.GetFleetInfosByPlayerID(this.PlayerId, FleetType.FL_NORMAL).ToList<FleetInfo>();
			List<int> list2 = game.GameDatabase.GetPlayerColonySystemIDs(targetPlayer.ID).ToList<int>();
			if (list2.Count <= 0 || list1.Count <= 0)
				return;
			int systemid = safeRandom.Choose<int>((IList<int>)list2);
			game.GameDatabase.InsertIncomingRandom(targetPlayer.ID, systemid, RandomEncounter.RE_GHOST_SHIP, game.GameDatabase.GetTurnCount() + 1);
			if (!game.DetectedIncomingRandom(targetPlayer.ID, systemid))
				return;
			game.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_INCOMING_GHOST_SHIP,
				EventMessage = TurnEventMessage.EM_INCOMING_GHOST_SHIP,
				PlayerID = targetPlayer.ID,
				SystemID = systemid,
				TurnNumber = game.GameDatabase.GetTurnCount()
			});
		}

		public static Matrix GetBaseEnemyFleetTrans(App app, Kerberos.Sots.GameStates.StarSystem starSystem)
		{
			return GhostShip.GetSpawnTransform(app, starSystem);
		}

		public static Matrix GetSpawnTransform(App app, Kerberos.Sots.GameStates.StarSystem starSystem)
		{
			float num1 = 0.0f;
			float num2 = 5000f;
			Vector3 vector3 = Vector3.Zero;
			Vector3 forward = -Vector3.UnitZ;
			List<StellarBody> stellarBodyList = new List<StellarBody>();
			foreach (StellarBody stellarBody in starSystem.GetPlanetsInSystem())
			{
				List<Ship> stationsAroundPlanet = starSystem.GetStationsAroundPlanet(stellarBody.Parameters.OrbitalID);
				stellarBodyList.Add(stellarBody);
				foreach (Ship ship in stationsAroundPlanet)
				{
					float lengthSquared = ship.Position.LengthSquared;
					if ((double)lengthSquared > (double)num1)
					{
						num1 = lengthSquared;
						vector3 = ship.Position;
						num2 = ship.ShipSphere.radius;
						forward = stellarBody.Parameters.Position - ship.Position;
					}
				}
			}
			if ((double)num1 <= 0.0)
			{
				foreach (StellarBody stellarBody in stellarBodyList)
				{
					if (stellarBody.Population > 0.0)
					{
						float lengthSquared = stellarBody.Parameters.Position.LengthSquared;
						if ((double)lengthSquared > (double)num1)
						{
							num1 = lengthSquared;
							vector3 = stellarBody.Parameters.Position;
							num2 = stellarBody.Parameters.Radius;
							int orbitalId = stellarBody.Parameters.OrbitalID;
							forward = -vector3;
						}
					}
				}
			}
			double num3 = (double)forward.Normalize();
			return Matrix.CreateWorld(vector3 - forward * (num2 + 20000f), forward, Vector3.UnitY);
		}
	}
}
