// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Encounters.MorrigiRelic
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Combat;
using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Encounters
{
	internal class MorrigiRelic
	{
		private static string[] designFiles = new string[11]
		{
	  "sn_tholostombstationpristinelevelone.section",
	  "sn_tholostombstationpristineleveltwo.section",
	  "sn_tholostombstationpristinelevelthree.section",
	  "sn_tholostombstationpristinelevelfour.section",
	  "sn_tholostombstationpristinelevelfive.section",
	  "sn_tholostombstationstealthlevelone.section",
	  "sn_tholostombstationstealthleveltwo.section",
	  "sn_tholostombstationstealthlevelthree.section",
	  "sn_tholostombstationstealthlevelfour.section",
	  "sn_tholostombstationstealthlevelfive.section",
	  "br_drone.section"
		};
		private static List<int> _designIds = new List<int>();
		private int PlayerId = -1;
		private const string FactionName = "morrigirelics";
		private const string PlayerName = "Morrigi Relic";
		private const string PlayerAvatar = "\\base\\factions\\morrigirelics\\avatars\\Morrigirelics_Avatar.tga";
		private const string FleetName = "Morrigi Relic";

		public int PlayerID
		{
			get
			{
				return this.PlayerId;
			}
		}

		public static List<int> DesignIds
		{
			get
			{
				return MorrigiRelic._designIds;
			}
		}

		public static MorrigiRelic InitializeEncounter(
		  GameDatabase gamedb,
		  AssetDatabase assetdb)
		{
			MorrigiRelic morrigiRelic = new MorrigiRelic();
			PlayerInfo playerInfo = gamedb.GetPlayerInfos().ToList<PlayerInfo>().FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x =>
		   {
			   if (!x.isStandardPlayer)
				   return x.Name.Contains("Morrigi Relic");
			   return false;
		   }));
			morrigiRelic.PlayerId = playerInfo != null ? playerInfo.ID : gamedb.InsertPlayer("Morrigi Relic", "morrigirelics", new int?(), assetdb.RandomEncounterPrimaryColor, new Vector3(0.0f), "", "\\base\\factions\\morrigirelics\\avatars\\Morrigirelics_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			MorrigiRelic._designIds.Clear();
			foreach (string designFile in MorrigiRelic.designFiles)
			{
				DesignInfo design = new DesignInfo(morrigiRelic.PlayerId, "Tholos Tomb", new string[1]
				{
		  string.Format("factions\\{0}\\sections\\{1}", (object) "morrigirelics", (object) designFile)
				});
				MorrigiRelic._designIds.Add(gamedb.InsertDesignByDesignInfo(design));
			}
			return morrigiRelic;
		}

		public static MorrigiRelic ResumeEncounter(GameDatabase gamedb)
		{
			MorrigiRelic morrigiRelic = new MorrigiRelic();
			PlayerInfo playerInfo = gamedb.GetPlayerInfos().ToList<PlayerInfo>().FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x =>
		   {
			   if (!x.isStandardPlayer)
				   return x.Name.Contains("Morrigi Relic");
			   return false;
		   }));
			morrigiRelic.PlayerId = playerInfo != null ? playerInfo.ID : gamedb.InsertPlayer("Morrigi Relic", "morrigirelics", new int?(), new Vector3(0.0f), new Vector3(0.0f), "", "\\base\\factions\\morrigirelics\\avatars\\Morrigirelics_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			MorrigiRelic._designIds.Clear();
			List<DesignInfo> list = gamedb.GetDesignInfosForPlayer(morrigiRelic.PlayerId).ToList<DesignInfo>();
			foreach (string designFile in MorrigiRelic.designFiles)
			{
				string s = designFile;
				if (list.Any<DesignInfo>((Func<DesignInfo, bool>)(x => x.DesignSections[0].FilePath.EndsWith(s))))
				{
					DesignInfo designInfo = list.First<DesignInfo>((Func<DesignInfo, bool>)(x => x.DesignSections[0].FilePath.EndsWith(s)));
					MorrigiRelic._designIds.Add(designInfo.ID);
					list.Remove(designInfo);
				}
				else
				{
					DesignInfo design = new DesignInfo(morrigiRelic.PlayerId, "Tholos Tomb", new string[1]
					{
			string.Format("factions\\{0}\\sections\\{1}", (object) "morrigirelics", (object) s)
					});
					MorrigiRelic._designIds.Add(gamedb.InsertDesignByDesignInfo(design));
				}
			}
			return morrigiRelic;
		}

		public void AddInstance(GameDatabase gamedb, AssetDatabase assetdb, int SystemId, int OrbitId)
		{
			OrbitalObjectInfo orbitalObjectInfo = gamedb.GetOrbitalObjectInfo(OrbitId);
			gamedb.RemoveOrbitalObject(orbitalObjectInfo.ID);
			int orbitalId = gamedb.InsertAsteroidBelt(orbitalObjectInfo.ParentID, orbitalObjectInfo.StarSystemID, orbitalObjectInfo.OrbitalPath, "Morrigi Relic Belt", App.GetSafeRandom().Next());
			int fleetID = gamedb.InsertFleet(this.PlayerId, 0, SystemId, SystemId, "Morrigi Relic", FleetType.FL_NORMAL);
			gamedb.InsertMorrigiRelicInfo(new MorrigiRelicInfo()
			{
				SystemId = SystemId,
				FleetId = fleetID,
				IsAggressive = true
			});
			Random safeRandom = App.GetSafeRandom();
			Matrix orbitalTransform = gamedb.GetOrbitalTransform(orbitalId);
			List<int> list = MorrigiRelic._designIds.Where<int>((Func<int, bool>)(x => x != MorrigiRelic._designIds.Last<int>())).ToList<int>();
			for (int shipIndex = 0; shipIndex < safeRandom.Next(1, assetdb.GlobalMorrigiRelicData.NumTombs + 1); ++shipIndex)
			{
				int num = gamedb.InsertShip(fleetID, safeRandom.Choose<int>((IList<int>)list), null, (ShipParams)0, new int?(), 0);
				this.SetRelicPosition(safeRandom, gamedb, assetdb, num, shipIndex, orbitalTransform);
				for (int index = 0; index < assetdb.GlobalMorrigiRelicData.NumFighters; ++index)
				{
					int shipID = gamedb.InsertShip(fleetID, MorrigiRelic._designIds.Last<int>(), null, (ShipParams)0, new int?(), 0);
					gamedb.SetShipParent(shipID, num);
				}
			}
		}

		public void UpdateTurn(GameSession game, int id)
		{
			if (game.GameDatabase.GetMorrigiRelicInfo(id) != null)
				return;
			game.GameDatabase.RemoveEncounter(id);
		}

		public void ApplyRewardsToPlayers(
		  App game,
		  MorrigiRelicInfo relicInfo,
		  List<ShipInfo> aliveRelicShips,
		  List<Player> rewardedPlayers)
		{
			if (relicInfo == null)
				return;
			relicInfo.IsAggressive = false;
			int num1 = 0;
			foreach (ShipInfo aliveRelicShip in aliveRelicShips)
			{
				if (aliveRelicShip.DesignID != MorrigiRelic._designIds.Last<int>() && aliveRelicShip.DesignInfo != null)
				{
					string sectionName = ((IEnumerable<DesignSectionInfo>)aliveRelicShip.DesignInfo.DesignSections).Select<DesignSectionInfo, ShipSectionAsset>((Func<DesignSectionInfo, ShipSectionAsset>)(x => x.ShipSectionAsset)).FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.Type == ShipSectionType.Mission)).SectionName;
					int reward = game.AssetDatabase.GlobalMorrigiRelicData.Rewards[(int)MorrigiRelicControl.GetMorrigiRelicTypeFromName(sectionName)];
					num1 += reward;
				}
			}
			game.GameDatabase.RemoveFleet(relicInfo.FleetId);
			game.GameDatabase.RemoveEncounter(relicInfo.Id);
			if (rewardedPlayers.Count == 0)
				return;
			int num2 = num1 / rewardedPlayers.Count;
			if (num2 <= 0)
				return;
			foreach (Player rewardedPlayer in rewardedPlayers)
			{
				PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(rewardedPlayer.ID);
				if (playerInfo != null)
				{
					game.GameDatabase.UpdatePlayerSavings(playerInfo.ID, playerInfo.Savings + (double)num2);
					game.GameDatabase.InsertTurnEvent(new TurnEvent()
					{
						EventType = TurnEventType.EV_TOMB_DEFENDERS_DESTROYED,
						EventMessage = TurnEventMessage.EM_TOMB_DEFENDERS_DESTROYED,
						PlayerID = playerInfo.ID,
						SystemID = relicInfo.SystemId,
						Savings = (double)num2,
						TurnNumber = game.GameDatabase.GetTurnCount()
					});
				}
			}
		}

		private void SetRelicPosition(
		  Random r,
		  GameDatabase db,
		  AssetDatabase ad,
		  int shipId,
		  int shipIndex,
		  Matrix beltTransform)
		{
			float length = beltTransform.Position.Length;
			float val2 = (float)Math.Tan(((double)ad.DefaultTacSensorRange + 5000.0) / (double)length);
			float num1 = Math.Min(6.283185f / (float)ad.GlobalMorrigiRelicData.NumTombs, val2);
			float maxValue = num1 / 4f;
			float yawRadians = num1 * (float)shipIndex + r.NextInclusive(-maxValue, maxValue);
			float num2 = r.NextInclusive(-2500f, 2500f);
			Matrix rotationYpr = Matrix.CreateRotationYPR(yawRadians, 0.0f, 0.0f);
			rotationYpr.Position = rotationYpr.Forward * (length + num2);
			db.UpdateShipSystemPosition(shipId, new Matrix?(rotationYpr));
		}

		public static Matrix GetBaseEnemyFleetTrans(
		  App app,
		  List<ShipInfo> shipIDs,
		  OrbitalObjectInfo[] objects)
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
				int orbitalId = 0;
				foreach (OrbitalObjectInfo orbitalObjectInfo in objects)
				{
					if (app.GameDatabase.GetAsteroidBeltInfo(orbitalObjectInfo.ID) != null)
						orbitalId = orbitalObjectInfo.ID;
				}
				float length = app.GameDatabase.GetOrbitalTransform(orbitalId).Position.Length;
				matrix = Matrix.CreateRotationYPR(0.0f, 0.0f, 0.0f);
				matrix.Position = -matrix.Forward * length;
			}
			return matrix;
		}

		public static Matrix GetSpawnTransform(
		  App app,
		  Random rand,
		  int shipIndex,
		  OrbitalObjectInfo[] objects)
		{
			int orbitalId = 0;
			foreach (OrbitalObjectInfo orbitalObjectInfo in objects)
			{
				if (app.GameDatabase.GetAsteroidBeltInfo(orbitalObjectInfo.ID) != null)
					orbitalId = orbitalObjectInfo.ID;
			}
			float length = app.GameDatabase.GetOrbitalTransform(orbitalId).Position.Length;
			float val2 = (float)Math.Tan(((double)app.AssetDatabase.DefaultTacSensorRange + 1000.0) / (double)length);
			float num1 = Math.Min(6.283185f / (float)app.AssetDatabase.GlobalMorrigiRelicData.NumTombs, val2);
			float maxValue = num1 / 4f;
			float yawRadians = num1 * (float)shipIndex + rand.NextInclusive(-maxValue, maxValue);
			float num2 = rand.NextInclusive(-2500f, 2500f);
			Matrix rotationYpr = Matrix.CreateRotationYPR(yawRadians, 0.0f, 0.0f);
			rotationYpr.Position = rotationYpr.Forward * (length + num2);
			return rotationYpr;
		}
	}
}
