// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Encounters.AsteroidMonitor
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Encounters
{
	internal class AsteroidMonitor
	{
		private int PlayerId = -1;
		private const string FactionName = "morrigirelics";
		private const string PlayerName = "Asteroid Monitor";
		private const string PlayerAvatar = "\\base\\factions\\morrigirelics\\avatars\\Morrigirelics_Avatar.tga";
		private const string FleetName = "Asteroid Monitor";
		private const string MonitorDesignFile = "sn_monitor.section";
		private const string MonitorCommandDesignFile = "sn_monitorcommander.section";
		private int _monitorDesignId;
		private int _monitorCommandDesignId;

		public int PlayerID
		{
			get
			{
				return this.PlayerId;
			}
		}

		public int MonitorDesignId
		{
			get
			{
				return this._monitorDesignId;
			}
		}

		public int MonitorCommandDesignId
		{
			get
			{
				return this._monitorCommandDesignId;
			}
		}

		public static AsteroidMonitor InitializeEncounter(
		  GameDatabase gamedb,
		  AssetDatabase assetdb)
		{
			AsteroidMonitor asteroidMonitor = new AsteroidMonitor();
			PlayerInfo playerInfo = gamedb.GetPlayerInfos().ToList<PlayerInfo>().FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x =>
		   {
			   if (!x.isStandardPlayer)
				   return x.Name.Contains("Asteroid Monitor");
			   return false;
		   }));
			asteroidMonitor.PlayerId = playerInfo != null ? playerInfo.ID : gamedb.InsertPlayer("Asteroid Monitor", "morrigirelics", new int?(), assetdb.RandomEncounterPrimaryColor, new Vector3(0.0f), "", "\\base\\factions\\morrigirelics\\avatars\\Morrigirelics_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			List<DesignInfo> list = gamedb.GetDesignInfosForPlayer(asteroidMonitor.PlayerId).ToList<DesignInfo>();
			asteroidMonitor._monitorDesignId = gamedb.AddOrGetEncounterDesignInfo((IEnumerable<DesignInfo>)list, asteroidMonitor.PlayerId, "Monitor", "morrigirelics", "sn_monitor.section");
			asteroidMonitor._monitorCommandDesignId = gamedb.AddOrGetEncounterDesignInfo((IEnumerable<DesignInfo>)list, asteroidMonitor.PlayerId, "Monitor Command", "morrigirelics", "sn_monitorcommander.section");
			return asteroidMonitor;
		}

		public static AsteroidMonitor ResumeEncounter(GameDatabase gamedb)
		{
			AsteroidMonitor asteroidMonitor = new AsteroidMonitor();
			PlayerInfo playerInfo = gamedb.GetPlayerInfos().ToList<PlayerInfo>().FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x =>
		   {
			   if (!x.isStandardPlayer)
				   return x.Name.Contains("Asteroid Monitor");
			   return false;
		   }));
			asteroidMonitor.PlayerId = playerInfo != null ? playerInfo.ID : gamedb.InsertPlayer("Asteroid Monitor", "morrigirelics", new int?(), new Vector3(0.0f), new Vector3(0.0f), "", "\\base\\factions\\morrigirelics\\avatars\\Morrigirelics_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			List<DesignInfo> list = gamedb.GetDesignInfosForPlayer(asteroidMonitor.PlayerId).ToList<DesignInfo>();
			asteroidMonitor._monitorDesignId = gamedb.AddOrGetEncounterDesignInfo((IEnumerable<DesignInfo>)list, asteroidMonitor.PlayerId, "Monitor", "morrigirelics", "sn_monitor.section");
			asteroidMonitor._monitorCommandDesignId = gamedb.AddOrGetEncounterDesignInfo((IEnumerable<DesignInfo>)list, asteroidMonitor.PlayerId, "Monitor Command", "morrigirelics", "sn_monitorcommander.section");
			return asteroidMonitor;
		}

		public void AddInstance(GameDatabase gamedb, AssetDatabase assetdb, int SystemId, int OrbitId)
		{
			OrbitalObjectInfo orbitalObjectInfo = gamedb.GetOrbitalObjectInfo(OrbitId);
			gamedb.RemoveOrbitalObject(orbitalObjectInfo.ID);
			int orbitalId = gamedb.InsertAsteroidBelt(orbitalObjectInfo.ParentID, orbitalObjectInfo.StarSystemID, orbitalObjectInfo.OrbitalPath, "Asteroid Monitor Belt", App.GetSafeRandom().Next());
			gamedb.InsertAsteroidMonitorInfo(new AsteroidMonitorInfo()
			{
				SystemId = SystemId,
				OrbitalId = orbitalId,
				IsAggressive = true
			});
			Matrix orbitalTransform = gamedb.GetOrbitalTransform(orbitalId);
			int fleetID = gamedb.InsertFleet(this.PlayerId, 0, SystemId, SystemId, "Asteroid Monitor", FleetType.FL_NORMAL);
			int shipId1 = gamedb.InsertShip(fleetID, this._monitorCommandDesignId, null, (ShipParams)0, new int?(), 0);
			this.SetMonitorPosition(gamedb, shipId1, 0, assetdb.GlobalAsteroidMonitorData.NumMonitors, true, orbitalTransform);
			for (int shipIndex = 0; shipIndex < assetdb.GlobalAsteroidMonitorData.NumMonitors; ++shipIndex)
			{
				int shipId2 = gamedb.InsertShip(fleetID, this._monitorDesignId, null, (ShipParams)0, new int?(), 0);
				this.SetMonitorPosition(gamedb, shipId2, shipIndex, assetdb.GlobalAsteroidMonitorData.NumMonitors, false, orbitalTransform);
			}
		}

		public void UpdateTurn(GameSession game, int id)
		{
			List<AsteroidMonitorInfo> list = game.GameDatabase.GetAllAsteroidMonitorInfos().ToList<AsteroidMonitorInfo>();
			foreach (FleetInfo fleetInfo in game.GameDatabase.GetFleetInfosByPlayerID(this.PlayerId, FleetType.FL_NORMAL).ToList<FleetInfo>())
			{
				FleetInfo fi = fleetInfo;
				if (!list.Any<AsteroidMonitorInfo>((Func<AsteroidMonitorInfo, bool>)(x => x.SystemId == fi.SystemID)))
					game.GameDatabase.RemoveFleet(fi.ID);
			}
			if (game.GameDatabase.GetAsteroidMonitorInfo(id) != null)
				return;
			game.GameDatabase.RemoveEncounter(id);
		}

		private void SetMonitorPosition(
		  GameDatabase db,
		  int shipId,
		  int shipIndex,
		  int numShips,
		  bool isCommand,
		  Matrix beltTransform)
		{
			Random random = new Random();
			float length = beltTransform.Position.Length;
			float num1 = 360f / (float)numShips;
			float degrees;
			if (isCommand)
			{
				int num2 = random.NextInclusive(0, numShips);
				degrees = num2 != numShips ? (float)((double)num1 * (double)num2 + (double)num1 * 0.5) : (float)-((double)num1 * 0.5);
				length -= 2500f;
			}
			else
				degrees = num1 * (float)(shipIndex - 1);
			Matrix rotationYpr = Matrix.CreateRotationYPR(MathHelper.DegreesToRadians(degrees), 0.0f, 0.0f);
			rotationYpr.Position = rotationYpr.Forward * length;
			db.UpdateShipSystemPosition(shipId, new Matrix?(rotationYpr));
		}

		public static Matrix GetBaseEnemyFleetTrans(
		  App app,
		  List<ShipInfo> shipIDs,
		  int systemID)
		{
			bool flag = false;
			Matrix matrix = Matrix.Identity;
			if (shipIDs.Count > 0)
			{
				int index = Math.Abs(App.GetSafeRandom().Next()) % shipIDs.Count;
				Matrix? shipSystemPosition = app.GameDatabase.GetShipSystemPosition(shipIDs[index].ID);
				if (shipSystemPosition.HasValue)
				{
					matrix = shipSystemPosition.Value;
					flag = true;
				}
			}
			if (!flag)
			{
				int encounterOrbitalId = app.GameDatabase.GetEncounterOrbitalId(EasterEgg.EE_ASTEROID_MONITOR, systemID);
				float length = app.GameDatabase.GetOrbitalTransform(encounterOrbitalId).Position.Length;
				matrix = Matrix.CreateRotationYPR(MathHelper.DegreesToRadians(0.0f), 0.0f, 0.0f);
				matrix.Position = -matrix.Forward * length;
			}
			return matrix;
		}

		public static Matrix GetSpawnTransform(
		  App app,
		  int designId,
		  int shipIndex,
		  int fleetCount,
		  int systemID)
		{
			int encounterOrbitalId = app.GameDatabase.GetEncounterOrbitalId(EasterEgg.EE_ASTEROID_MONITOR, systemID);
			float length = app.GameDatabase.GetOrbitalTransform(encounterOrbitalId).Position.Length;
			float num1 = 36f;
			float degrees;
			if (app.Game.ScriptModules.AsteroidMonitor != null && designId == app.Game.ScriptModules.AsteroidMonitor.MonitorCommandDesignId)
			{
				int num2 = Math.Abs(App.GetSafeRandom().Next()) % (fleetCount + 1);
				degrees = num2 != fleetCount ? (float)((double)num1 * (double)num2 + (double)num1 * 0.5) : (float)-((double)num1 * 0.5);
				length -= 2500f;
			}
			else
				degrees = num1 * (float)(shipIndex - 1);
			Matrix rotationYpr = Matrix.CreateRotationYPR(MathHelper.DegreesToRadians(degrees), 0.0f, 0.0f);
			rotationYpr.Position = rotationYpr.Forward * length;
			return rotationYpr;
		}
	}
}
