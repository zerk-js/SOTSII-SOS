// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Encounters.Comet
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Encounters
{
	internal class Comet
	{
		private int PlayerId = -1;
		private const string FactionName = "grandmenaces";
		private const string PlayerName = "Comet";
		private const string PlayerAvatar = "\\base\\factions\\grandmenaces\\avatars\\Comet_Avatar.tga";
		private const string FleetName = "Comet";
		private const string CometDesignFile = "Comet.section";
		public static bool ForceEncounter;
		private int _cometDesignId;

		public int PlayerID
		{
			get
			{
				return this.PlayerId;
			}
		}

		public int CometDesignId
		{
			get
			{
				return this._cometDesignId;
			}
		}

		public static Comet InitializeEncounter(GameDatabase gamedb, AssetDatabase assetdb)
		{
			if (assetdb.GetFaction("grandmenaces") == null)
				return (Comet)null;
			Comet comet = new Comet();
			comet.PlayerId = gamedb.InsertPlayer(nameof(Comet), "grandmenaces", new int?(), assetdb.RandomEncounterPrimaryColor, new Vector3(0.0f), "", "\\base\\factions\\grandmenaces\\avatars\\Comet_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			DesignInfo design = new DesignInfo(comet.PlayerId, nameof(Comet), new string[1]
			{
		string.Format("factions\\{0}\\sections\\{1}", (object) "grandmenaces", (object) "Comet.section")
			});
			comet._cometDesignId = gamedb.InsertDesignByDesignInfo(design);
			return comet;
		}

		public static Comet ResumeEncounter(GameDatabase gamedb)
		{
			if (gamedb.AssetDatabase.GetFaction("grandmenaces") == null)
				return (Comet)null;
			Comet comet = new Comet();
			PlayerInfo playerInfo = gamedb.GetPlayerInfos().ToList<PlayerInfo>().FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x =>
		   {
			   if (!x.isStandardPlayer)
				   return x.Name.Contains(nameof(Comet));
			   return false;
		   }));
			comet.PlayerId = playerInfo == null ? gamedb.InsertPlayer(nameof(Comet), "grandmenaces", new int?(), new Vector3(0.0f), new Vector3(0.0f), "", "\\base\\factions\\grandmenaces\\avatars\\Comet_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal) : playerInfo.ID;
			DesignInfo designInfo = gamedb.GetDesignInfosForPlayer(comet.PlayerId).ToList<DesignInfo>().FirstOrDefault<DesignInfo>((Func<DesignInfo, bool>)(x => x.DesignSections[0].FilePath.EndsWith("Comet.section")));
			if (designInfo == null)
			{
				DesignInfo design = new DesignInfo(comet.PlayerId, nameof(Comet), new string[1]
				{
		  string.Format("factions\\{0}\\sections\\{1}", (object) "grandmenaces", (object) "Comet.section")
				});
				comet._cometDesignId = gamedb.InsertDesignByDesignInfo(design);
			}
			else
				comet._cometDesignId = designInfo.ID;
			return comet;
		}

		public void UpdateTurn(GameSession game)
		{
			foreach (FleetInfo fleetInfo in game.GameDatabase.GetFleetInfosByPlayerID(this.PlayerId, FleetType.FL_NORMAL).ToList<FleetInfo>())
				game.GameDatabase.RemoveFleet(fleetInfo.ID);
		}

		public void ExecuteInstance(GameDatabase gamedb, AssetDatabase assetdb, int systemid)
		{
			int fleetID = gamedb.InsertFleet(this.PlayerId, 0, systemid, systemid, nameof(Comet), FleetType.FL_NORMAL);
			gamedb.InsertShip(fleetID, this.CometDesignId, null, (ShipParams)0, new int?(), 0);
		}

		public void AddInstance(GameDatabase gamedb, AssetDatabase assetdb, int? targetSystem = null)
		{
			Random safeRandom = App.GetSafeRandom();
			if (!targetSystem.HasValue)
			{
				List<ColonyInfo> list = gamedb.GetColonyInfos().ToList<ColonyInfo>();
				ColonyInfo colonyInfo = safeRandom.Choose<ColonyInfo>((IList<ColonyInfo>)list);
				targetSystem = new int?(gamedb.GetOrbitalObjectInfo(colonyInfo.OrbitalObjectID).StarSystemID);
			}
			gamedb.InsertIncomingGM(targetSystem.Value, EasterEgg.GM_COMET, gamedb.GetTurnCount() + 1);
			foreach (PlayerInfo playerInfo in gamedb.GetStandardPlayerInfos().ToList<PlayerInfo>())
			{
				if (gamedb.GetStratModifier<int>(StratModifiers.GrandMenaceWarningTime, playerInfo.ID) > 0)
					gamedb.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_INCOMING_COMET,
						EventMessage = TurnEventMessage.EM_INCOMING_COMET,
						PlayerID = playerInfo.ID,
						TurnNumber = gamedb.GetTurnCount()
					});
			}
		}

		public static Matrix GetBaseEnemyFleetTrans(
		  App app,
		  Kerberos.Sots.GameStates.StarSystem starSystem,
		  OrbitalObjectInfo[] orbitalObjects)
		{
			return Comet.GetSpawnTransform(app, starSystem, orbitalObjects);
		}

		public static Matrix GetSpawnTransform(
		  App app,
		  Kerberos.Sots.GameStates.StarSystem starSystem,
		  OrbitalObjectInfo[] orbitalObjects)
		{
			int planetID = 0;
			float num1 = 0.0f;
			float num2 = 5000f;
			Vector3 vector3_1 = Vector3.Zero;
			foreach (OrbitalObjectInfo orbitalObject in orbitalObjects)
			{
				PlanetInfo planetInfo = app.GameDatabase.GetPlanetInfo(orbitalObject.ID);
				if (planetInfo != null && app.GameDatabase.GetColonyInfoForPlanet(orbitalObject.ID) != null)
				{
					Matrix orbitalTransform = app.GameDatabase.GetOrbitalTransform(orbitalObject.ID);
					float lengthSquared = orbitalTransform.Position.LengthSquared;
					if ((double)lengthSquared > (double)num1)
					{
						num1 = lengthSquared;
						vector3_1 = orbitalTransform.Position;
						num2 = StarSystemVars.Instance.SizeToRadius(planetInfo.Size) + 15000f;
						planetID = planetInfo.ID;
					}
				}
			}
			if ((double)num1 <= 0.0)
				return Matrix.Identity;
			Vector3 vector3_2 = -vector3_1;
			vector3_2.Y = 0.0f;
			double num3 = (double)vector3_2.Normalize();
			Vector3 position = vector3_1 - vector3_2 * num2;
			if (starSystem != null)
			{
				Vector3 forward = -Vector3.UnitZ;
				float num4 = -1f;
				foreach (Ship ship in starSystem.GetStationsAroundPlanet(planetID))
				{
					Vector3 v0 = vector3_1 - ship.Maneuvering.Position;
					double num5 = (double)v0.Normalize();
					float num6 = Vector3.Dot(v0, vector3_2);
					if ((double)num6 > 0.75 && (double)num6 > (double)num4)
					{
						num4 = num6;
						forward = v0;
					}
				}
				if ((double)num4 > 0.0)
				{
					Matrix world = Matrix.CreateWorld(Vector3.Zero, forward, Vector3.UnitY);
					Vector3 vector3_3 = vector3_1 + world.Right * num2;
					Vector3 vector3_4 = vector3_1 - world.Right * num2;
					vector3_2 = (double)(position - vector3_3).LengthSquared >= (double)(position - vector3_4).LengthSquared ? (forward - world.Right) * 0.5f : (forward + world.Right) * 0.5f;
					double num5 = (double)vector3_2.Normalize();
					position = vector3_1 - vector3_2 * num2;
				}
			}
			return Matrix.CreateWorld(position, vector3_2, Vector3.UnitY);
		}
	}
}
