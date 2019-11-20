// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Encounters.Spectre
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
	internal class Spectre
	{
		private int PlayerId = -1;
		private const string FactionName = "specters";
		private const string PlayerName = "Spectre";
		private const string PlayerAvatar = "\\base\\factions\\specters\\avatars\\Specters_Avatar.tga";
		private const string FleetName = "Haunt of Spectres";
		private const string smallDesignFile = "small_specter.section";
		private const string mediumDesignFile = "medium_specter.section";
		private const string bigDesignFile = "big_specter.section";
		public static bool ForceEncounter;
		private int _smallDesignId;
		private int _mediumDesignId;
		private int _bigDesignId;
		private bool _force;

		public int PlayerID
		{
			get
			{
				return this.PlayerId;
			}
		}

		public int SmallDesignId
		{
			get
			{
				return this._smallDesignId;
			}
		}

		public int MediumDesignId
		{
			get
			{
				return this._mediumDesignId;
			}
		}

		public int BigDesignId
		{
			get
			{
				return this._bigDesignId;
			}
		}

		public static Spectre InitializeEncounter(GameDatabase gamedb, AssetDatabase assetdb)
		{
			Spectre spectre = new Spectre();
			spectre.PlayerId = gamedb.InsertPlayer(nameof(Spectre), "specters", new int?(), assetdb.RandomEncounterPrimaryColor, new Vector3(0.0f), "", "\\base\\factions\\specters\\avatars\\Specters_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			DesignInfo design1 = new DesignInfo(spectre.PlayerId, "Small Spectre", new string[1]
			{
		string.Format("factions\\{0}\\sections\\{1}", (object) "specters", (object) "small_specter.section")
			});
			DesignInfo design2 = new DesignInfo(spectre.PlayerId, "Medium Spectre", new string[1]
			{
		string.Format("factions\\{0}\\sections\\{1}", (object) "specters", (object) "medium_specter.section")
			});
			DesignInfo design3 = new DesignInfo(spectre.PlayerId, "Big Spectre", new string[1]
			{
		string.Format("factions\\{0}\\sections\\{1}", (object) "specters", (object) "big_specter.section")
			});
			spectre._smallDesignId = gamedb.InsertDesignByDesignInfo(design1);
			spectre._mediumDesignId = gamedb.InsertDesignByDesignInfo(design2);
			spectre._bigDesignId = gamedb.InsertDesignByDesignInfo(design3);
			return spectre;
		}

		public static Spectre ResumeEncounter(GameDatabase gamedb)
		{
			Spectre spectre = new Spectre();
			PlayerInfo playerInfo = gamedb.GetPlayerInfos().ToList<PlayerInfo>().FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x =>
		   {
			   if (!x.isStandardPlayer)
				   return x.Name.Contains(nameof(Spectre));
			   return false;
		   }));
			spectre.PlayerId = playerInfo.ID;
			List<DesignInfo> list = gamedb.GetDesignInfosForPlayer(spectre.PlayerId).ToList<DesignInfo>();
			spectre._smallDesignId = list.First<DesignInfo>((Func<DesignInfo, bool>)(x => x.DesignSections[0].FilePath.EndsWith("small_specter.section"))).ID;
			spectre._mediumDesignId = list.First<DesignInfo>((Func<DesignInfo, bool>)(x => x.DesignSections[0].FilePath.EndsWith("medium_specter.section"))).ID;
			spectre._bigDesignId = list.First<DesignInfo>((Func<DesignInfo, bool>)(x => x.DesignSections[0].FilePath.EndsWith("big_specter.section"))).ID;
			return spectre;
		}

		public void UpdateTurn(GameSession game)
		{
			if (this._force)
			{
				this._force = false;
			}
			else
			{
				foreach (FleetInfo fleetInfo in game.GameDatabase.GetFleetInfosByPlayerID(this.PlayerId, FleetType.FL_NORMAL).ToList<FleetInfo>())
					game.GameDatabase.RemoveFleet(fleetInfo.ID);
			}
		}

		public void ExecuteEncounter(
		  GameSession game,
		  PlayerInfo targetPlayer,
		  int targetSystem,
		  bool force = false)
		{
			this._force = force;
			Random safeRandom = App.GetSafeRandom();
			int num1 = safeRandom.Next(game.AssetDatabase.GlobalSpectreData.MinSpectres, game.AssetDatabase.GlobalSpectreData.MaxSpectres);
			int fleetID = game.GameDatabase.InsertFleet(this.PlayerId, 0, targetSystem, targetSystem, "Haunt of Spectres", FleetType.FL_NORMAL);
			for (int index = 0; index < num1; ++index)
			{
				int num2 = safeRandom.Next(100);
				if (num2 > 80)
					game.GameDatabase.InsertShip(fleetID, this._bigDesignId, null, (ShipParams)0, new int?(), 0);
				else if (num2 > 50)
					game.GameDatabase.InsertShip(fleetID, this._mediumDesignId, null, (ShipParams)0, new int?(), 0);
				else
					game.GameDatabase.InsertShip(fleetID, this._smallDesignId, null, (ShipParams)0, new int?(), 0);
			}
		}

		public void AddEncounter(GameSession game, PlayerInfo targetPlayer, int? forceSystem = null)
		{
			if (game.GameDatabase.GetStratModifier<bool>(StratModifiers.ImmuneToSpectre, targetPlayer.ID))
				return;
			Random safeRandom = App.GetSafeRandom();
			List<int> list = game.GameDatabase.GetPlayerColonySystemIDs(targetPlayer.ID).ToList<int>();
			if (list.Count <= 0 && !forceSystem.HasValue)
				return;
			int systemid = forceSystem.HasValue ? forceSystem.Value : safeRandom.Choose<int>((IList<int>)list);
			game.GameDatabase.InsertIncomingRandom(targetPlayer.ID, systemid, RandomEncounter.RE_SPECTORS, game.GameDatabase.GetTurnCount() + 1);
			if (!game.DetectedIncomingRandom(targetPlayer.ID, systemid))
				return;
			game.GameDatabase.InsertTurnEvent(new TurnEvent()
			{
				EventType = TurnEventType.EV_INCOMING_SPECTORS,
				EventMessage = TurnEventMessage.EM_INCOMING_SPECTORS,
				PlayerID = targetPlayer.ID,
				SystemID = systemid,
				TurnNumber = game.GameDatabase.GetTurnCount()
			});
		}

		public static Matrix GetBaseEnemyFleetTrans(App app, int systemID)
		{
			return Spectre.GetSpawnTransform(app, systemID);
		}

		public static Matrix GetSpawnTransform(App app, int systemId)
		{
			bool flag = false;
			float num1 = 0.0f;
			float num2 = 0.0f;
			OrbitalObjectInfo orbitalObjectInfo1 = (OrbitalObjectInfo)null;
			Vector3 vector3 = Vector3.Zero;
			foreach (OrbitalObjectInfo orbitalObjectInfo2 in app.GameDatabase.GetStarSystemOrbitalObjectInfos(systemId))
			{
				ColonyInfo colonyInfoForPlanet = app.GameDatabase.GetColonyInfoForPlanet(orbitalObjectInfo2.ID);
				if (!flag || colonyInfoForPlanet != null)
				{
					PlanetInfo planetInfo = app.GameDatabase.GetPlanetInfo(orbitalObjectInfo2.ID);
					float num3 = 1000f;
					if (planetInfo != null)
						num3 = StarSystemVars.Instance.SizeToRadius(planetInfo.Size);
					Vector3 position = app.GameDatabase.GetOrbitalTransform(orbitalObjectInfo2.ID).Position;
					float num4 = position.Length + num3;
					if ((double)num4 > (double)num1 || !flag && colonyInfoForPlanet != null)
					{
						orbitalObjectInfo1 = orbitalObjectInfo2;
						num1 = num4;
						flag = colonyInfoForPlanet != null;
						vector3 = position;
						num2 = num3 + 10000f;
						if (flag)
							break;
					}
				}
			}
			if (orbitalObjectInfo1 == null)
				return Matrix.Identity;
			Vector3 forward = -vector3;
			double num5 = (double)forward.Normalize();
			return Matrix.CreateWorld(vector3 - forward * num2, forward, Vector3.UnitY);
		}
	}
}
