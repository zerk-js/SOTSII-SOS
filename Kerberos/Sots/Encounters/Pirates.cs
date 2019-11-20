// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Encounters.Pirates
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Encounters
{
	internal class Pirates
	{
		private static readonly string[] availableCommandSections = new string[5]
		{
	  "cr_cmd.section",
	  "cr_cmd_cloaking.section",
	  "cr_cmd_hammerhead.section",
	  "cr_cmd_strafe.section",
	  "cr_cmd_deepscan.section"
		};
		private static readonly string[] availableMissionSections = new string[4]
		{
	  "cr_mis_armor.section",
	  "cr_mis_boarding.section",
	  "cr_mis_dronecarrier.section",
	  "cr_mis_supply.section"
		};
		private static readonly string[] availableEngineSections = new string[2]
		{
	  "cr_eng_fusion.section",
	  "cr_eng_antimatter.section"
		};
		public static bool ForceEncounter = false;
		private int PlayerId = -1;
		private const string FactionName = "slavers";
		private const string PlayerName = "Pirate";
		private const string PlayerAvatar = "\\base\\factions\\slavers\\avatars\\Slavers_Avatar.tga";
		private const string FleetName = "Pirate Raiders";
		private const string droneSection = "br_drone.section";
		private const string boardingPodSection = "br_boardingpod.section";
		private const string PirateBaseShipName = "Pirate Base";
		private const string PirateBaseSectionName = "sn_piratebase.section";
		private int _pirateBaseDesignId;

		public int PirateBaseDesignId
		{
			get
			{
				return this._pirateBaseDesignId;
			}
		}

		public int PlayerID
		{
			get
			{
				return this.PlayerId;
			}
		}

		public static Pirates InitializeEncounter(GameDatabase gamedb, AssetDatabase assetdb)
		{
			Pirates pirates = new Pirates();
			pirates.PlayerId = gamedb.InsertPlayer("Pirate", "slavers", new int?(), assetdb.RandomEncounterPrimaryColor, new Vector3(0.0f), "", "\\base\\factions\\slavers\\avatars\\Slavers_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal);
			foreach (LogicalWeapon weapon in assetdb.Weapons)
				gamedb.InsertWeapon(weapon, pirates.PlayerId);
			if (gamedb.HasEndOfFleshExpansion())
				pirates._pirateBaseDesignId = gamedb.InsertDesignByDesignInfo(new DesignInfo(pirates.PlayerId, "Pirate Base", new string[1]
				{
		  string.Format("factions\\{0}\\sections\\{1}", (object) "slavers", (object) "sn_piratebase.section")
				})
				{
					StationType = StationType.NAVAL
				});
			return pirates;
		}

		public static Pirates ResumeEncounter(GameDatabase gamedb)
		{
			Pirates pirates = new Pirates();
			PlayerInfo playerInfo = gamedb.GetPlayerInfos().ToList<PlayerInfo>().FirstOrDefault<PlayerInfo>((Func<PlayerInfo, bool>)(x =>
		   {
			   if (!x.isStandardPlayer)
				   return x.Name.Contains("Pirate");
			   return false;
		   }));
			pirates.PlayerId = playerInfo == null ? gamedb.InsertPlayer("Pirate", "slavers", new int?(), new Vector3(0.0f), new Vector3(0.0f), "", "\\base\\factions\\slavers\\avatars\\Slavers_Avatar.tga", 0.0, 0, false, false, false, 0, AIDifficulty.Normal) : playerInfo.ID;
			DesignInfo designInfo = gamedb.GetDesignInfosForPlayer(pirates.PlayerId).FirstOrDefault<DesignInfo>((Func<DesignInfo, bool>)(x => x.Name == "Pirate Base"));
			pirates._pirateBaseDesignId = designInfo != null ? designInfo.ID : -1;
			return pirates;
		}

		public void AddInstance(
		  GameDatabase gamedb,
		  AssetDatabase assetdb,
		  GameSession game,
		  int SystemId,
		  int OrbitId)
		{
			Random random = new Random();
			PirateBaseInfo pbi = new PirateBaseInfo();
			pbi.SystemId = SystemId;
			pbi.NumShips = assetdb.GlobalPiracyData.PiracyMinBaseShips;
			pbi.TurnsUntilAddShip = assetdb.GlobalPiracyData.PiracyBaseTurnsPerUpdate;
			float num = StarSystemVars.Instance.SizeToRadius(game.GameDatabase.GetPlanetInfo(OrbitId).Size) + (float)StarSystemVars.Instance.StationOrbitDistance;
			OrbitalObjectInfo orbitalObjectInfo = gamedb.GetOrbitalObjectInfo(OrbitId);
			OrbitalPath path = new OrbitalPath();
			path.Scale = new Vector2(num, num);
			path.Rotation = new Vector3(0.0f, 0.0f, 0.0f);
			path.DeltaAngle = random.NextInclusive(0.0f, 360f);
			path.InitialAngle = 0.0f;
			DesignInfo designInfo = gamedb.GetDesignInfo(this.PirateBaseDesignId);
			pbi.BaseStationId = gamedb.InsertStation(OrbitId, orbitalObjectInfo.StarSystemID, path, designInfo.Name, this.PlayerId, designInfo);
			gamedb.InsertPirateBaseInfo(pbi);
		}

		public void UpdateTurn(GameSession game)
		{
			foreach (FleetInfo fleetInfo in game.GameDatabase.GetFleetInfosByPlayerID(this.PlayerId, FleetType.FL_NORMAL).ToList<FleetInfo>())
				game.GameDatabase.RemoveFleet(fleetInfo.ID);
			foreach (DesignInfo designInfo in game.GameDatabase.GetDesignInfosForPlayer(this.PlayerId).ToList<DesignInfo>())
			{
				if (designInfo.ID != this.PirateBaseDesignId)
					game.GameDatabase.RemovePlayerDesign(designInfo.ID);
			}
			List<PirateBaseInfo> pirateBases = game.GameDatabase.GetPirateBaseInfos().ToList<PirateBaseInfo>();
			foreach (PirateBaseInfo pbi in pirateBases)
			{
				if (game.GameDatabase.GetStationInfo(pbi.BaseStationId) == null)
				{
					game.GameDatabase.RemoveEncounter(pbi.Id);
				}
				else
				{
					--pbi.TurnsUntilAddShip;
					if (pbi.TurnsUntilAddShip <= 0)
					{
						pbi.TurnsUntilAddShip = game.AssetDatabase.GlobalPiracyData.PiracyBaseTurnsPerUpdate;
						pbi.NumShips = Math.Min(pbi.NumShips + 1, game.AssetDatabase.GlobalPiracyData.PiracyTotalMaxShips);
					}
					game.GameDatabase.UpdatePirateBaseInfo(pbi);
				}
			}
			Random safeRandom = App.GetSafeRandom();
			float piracyBaseOdds1 = game.AssetDatabase.GlobalPiracyData.PiracyBaseOdds;
			TradeResultsTable tradeResultsTable = game.GameDatabase.GetTradeResultsTable();
			List<PlayerInfo> list = game.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
			List<int> intList = new List<int>();
			foreach (PlayerInfo playerInfo in list)
			{
				if (game.AssetDatabase.GetFaction(playerInfo.FactionID).HasSlaves())
				{
					IEnumerable<int> playerColonySystemIds = game.GameDatabase.GetPlayerColonySystemIDs(playerInfo.ID);
					intList.AddRange(playerColonySystemIds);
				}
			}
			foreach (KeyValuePair<int, TradeNode> tradeNode in tradeResultsTable.TradeNodes)
			{
				float piracyBaseOdds2 = game.AssetDatabase.GlobalPiracyData.PiracyBaseOdds;
				int? p = game.GameDatabase.GetSystemOwningPlayer(tradeNode.Key);
				if (p.HasValue && list.Any<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.ID == p.Value)) && (tradeNode.Value.Freighters != 0 && !intList.Contains(tradeNode.Key)))
				{
					Player playerObject = game.GetPlayerObject(p.Value);
					if (playerObject == null || !playerObject.IsAI())
					{
						foreach (FleetInfo fleetInfo in game.GameDatabase.GetFleetInfoBySystemID(tradeNode.Key, FleetType.FL_DEFENSE).ToList<FleetInfo>())
						{
							foreach (ShipInfo shipInfo in game.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, false).ToList<ShipInfo>())
							{
								if (shipInfo.IsPoliceShip() && game.GameDatabase.GetShipSystemPosition(shipInfo.ID).HasValue)
									piracyBaseOdds2 += game.AssetDatabase.GlobalPiracyData.PiracyModPolice;
							}
						}
						float val1 = game.AssetDatabase.GlobalPiracyData.PiracyModNoNavalBase;
						foreach (StationInfo stationInfo in game.GameDatabase.GetStationForSystem(tradeNode.Key).ToList<StationInfo>())
						{
							if (stationInfo.DesignInfo.StationType == StationType.NAVAL)
								val1 = Math.Min(val1, (float)stationInfo.DesignInfo.StationLevel * game.AssetDatabase.GlobalPiracyData.PiracyModNavalBase);
						}
						float num1 = piracyBaseOdds2 + ((double)val1 < 0.0 ? val1 : game.AssetDatabase.GlobalPiracyData.PiracyModNoNavalBase);
						Vector3 starSystemOrigin = game.GameDatabase.GetStarSystemOrigin(tradeNode.Key);
						foreach (int systemId in intList)
						{
							if ((double)(game.GameDatabase.GetStarSystemOrigin(systemId) - starSystemOrigin).Length < (double)game.AssetDatabase.GlobalPiracyData.PiracyMinZuulProximity)
								num1 += game.AssetDatabase.GlobalPiracyData.PiracyModZuulProximity;
						}
						if (game.GameDatabase.GetStarSystemInfo(tradeNode.Key).IsOpen)
							num1 += 0.02f;
						int num2 = 0;
						if (pirateBases.Count > 0)
							num2 = game.GameDatabase.GetSystemsInRange(starSystemOrigin, (float)game.AssetDatabase.GlobalPiracyData.PiracyBaseRange).ToList<StarSystemInfo>().Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x => pirateBases.Any<PirateBaseInfo>((Func<PirateBaseInfo, bool>)(y => y.SystemId == x.ID)))).Count<StarSystemInfo>();
						if (num2 > 0)
							num1 += game.AssetDatabase.GlobalPiracyData.PiracyBaseMod;
						float num3 = num1 * game.GameDatabase.GetStratModifier<float>(StratModifiers.ChanceOfPirates, p.Value);
						if (safeRandom.CoinToss((double)num3) || Pirates.ForceEncounter)
						{
							int num4 = (game.GameDatabase.GetStarSystemInfo(tradeNode.Key).ProvinceID.HasValue ? 0 : 1) + game.AssetDatabase.GlobalPiracyData.PiracyBaseShipBonus * num2;
							int numShips = safeRandom.Next(game.AssetDatabase.GlobalPiracyData.PiracyMinShips + num4, game.AssetDatabase.GlobalPiracyData.PiracyMaxShips + num4 + 1);
							this.SpawnPirateFleet(game, tradeNode.Key, numShips);
						}
					}
				}
			}
		}

		public int SpawnPirateFleet(GameSession game, int targetSystem, int numShips)
		{
			Random random = new Random();
			int fleetID = game.GameDatabase.InsertFleet(this.PlayerID, 0, targetSystem, 0, "Pirate Raiders", FleetType.FL_NORMAL);
			Dictionary<LogicalWeapon, int> dictionary1 = new Dictionary<LogicalWeapon, int>();
			List<PlayerInfo> list1 = game.GameDatabase.GetStandardPlayerInfos().ToList<PlayerInfo>();
			List<PlayerInfo> playerInfoList = new List<PlayerInfo>();
			foreach (PlayerInfo playerInfo in list1)
			{
				Faction faction = game.AssetDatabase.GetFaction(playerInfo.FactionID);
				if (faction.Name != "liir_zuul" && faction.Name != "hiver" && faction.Name != "loa")
					playerInfoList.Add(playerInfo);
				foreach (LogicalWeapon key in game.GameDatabase.GetAvailableWeapons(game.AssetDatabase, playerInfo.ID).ToList<LogicalWeapon>())
				{
					if (!dictionary1.ContainsKey(key))
					{
						dictionary1.Add(key, 1);
					}
					else
					{
						Dictionary<LogicalWeapon, int> dictionary2;
						LogicalWeapon index;
						(dictionary2 = dictionary1)[index = key] = dictionary2[index] + 1;
					}
				}
			}
			if (playerInfoList.Count > 0)
			{
				List<LogicalWeapon> availableWeapons = new List<LogicalWeapon>();
				foreach (LogicalWeapon logicalWeapon in game.AssetDatabase.Weapons.ToList<LogicalWeapon>())
				{
					if (((IEnumerable<LogicalTurretClass>)logicalWeapon.TurretClasses).Count<LogicalTurretClass>() > 0 && WeaponEnums.IsBattleRider(logicalWeapon.DefaultWeaponClass))
						availableWeapons.Add(logicalWeapon);
				}
				foreach (KeyValuePair<LogicalWeapon, int> keyValuePair in dictionary1)
				{
					if (keyValuePair.Value > 1 && !availableWeapons.Contains(keyValuePair.Key))
						availableWeapons.Add(keyValuePair.Key);
				}
				for (int index1 = 0; index1 < numShips + 1; ++index1)
				{
					PlayerInfo playerInfo = random.Choose<PlayerInfo>((IList<PlayerInfo>)playerInfoList);
					Faction faction = game.AssetDatabase.GetFaction(playerInfo.FactionID);
					DesignInfo design1 = DesignLab.DesignShip(game, ShipClass.BattleRider, ShipRole.BOARDINGPOD, WeaponRole.PLANET_ATTACK, playerInfo.ID);
					DesignInfo design2 = DesignLab.DesignShip(game, ShipClass.BattleRider, ShipRole.DRONE, WeaponRole.UNDEFINED, playerInfo.ID);
					design1.PlayerID = this.PlayerID;
					design2.PlayerID = this.PlayerID;
					game.GameDatabase.InsertDesignByDesignInfo(design1);
					game.GameDatabase.InsertDesignByDesignInfo(design2);
					DesignInfo designInfo1;
					if (index1 == 0)
					{
						DesignInfo designInfo2 = DesignLab.DesignShip(game, ShipClass.Cruiser, ShipRole.COMMAND, WeaponRole.UNDEFINED, playerInfo.ID);
						designInfo1 = new DesignInfo(playerInfo.ID, "", new string[3]
						{
			  ((IEnumerable<DesignSectionInfo>) designInfo2.DesignSections).First<DesignSectionInfo>((Func<DesignSectionInfo, bool>) (x => x.ShipSectionAsset.Type == ShipSectionType.Command)).FilePath,
			  ((IEnumerable<DesignSectionInfo>) designInfo2.DesignSections).First<DesignSectionInfo>((Func<DesignSectionInfo, bool>) (x => x.ShipSectionAsset.Type == ShipSectionType.Mission)).FilePath,
			  ((IEnumerable<DesignSectionInfo>) designInfo2.DesignSections).First<DesignSectionInfo>((Func<DesignSectionInfo, bool>) (x => x.ShipSectionAsset.Type == ShipSectionType.Engine)).FilePath
						});
					}
					else
					{
						List<ShipSectionAsset> availableSections = game.GetAvailableShipSections(playerInfo.ID).ToList<ShipSectionAsset>();
						List<string> list2 = ((IEnumerable<string>)Pirates.availableCommandSections).ToList<string>();
						List<string> list3 = ((IEnumerable<string>)Pirates.availableMissionSections).ToList<string>();
						List<string> list4 = ((IEnumerable<string>)Pirates.availableEngineSections).ToList<string>();
						for (int index2 = 0; index2 < list2.Count<string>(); ++index2)
							list2[index2] = string.Format("factions\\{0}\\sections\\{1}", (object)faction.Name, (object)list2[index2]);
						for (int index2 = 0; index2 < list3.Count<string>(); ++index2)
							list3[index2] = string.Format("factions\\{0}\\sections\\{1}", (object)faction.Name, (object)list3[index2]);
						for (int index2 = 0; index2 < list4.Count<string>(); ++index2)
							list4[index2] = string.Format("factions\\{0}\\sections\\{1}", (object)faction.Name, (object)list4[index2]);
						list2.RemoveAll((Predicate<string>)(x => !availableSections.Any<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(y => y.FileName == x))));
						list3.RemoveAll((Predicate<string>)(x => !availableSections.Any<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(y => y.FileName == x))));
						list4.RemoveAll((Predicate<string>)(x => !availableSections.Any<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(y => y.FileName == x))));
						designInfo1 = new DesignInfo(playerInfo.ID, "", new string[3]
						{
			  random.Choose<string>((IList<string>) list2),
			  random.Choose<string>((IList<string>) list3),
			  random.Choose<string>((IList<string>) list4)
						});
					}
					designInfo1.Name = DesignLab.GenerateDesignName(game.AssetDatabase, game.GameDatabase, (DesignInfo)null, designInfo1, DesignLab.NameGenerators.FactionRandom);
					DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, designInfo1);
					DesignInfo design3 = DesignLab.AssignWeaponsToDesign(game, designInfo1, availableWeapons, this.PlayerId, WeaponRole.BRAWLER, (AITechStyles)null);
					design3.PlayerID = this.PlayerID;
					int designID = game.GameDatabase.InsertDesignByDesignInfo(design3);
					int carrierID = game.GameDatabase.InsertShip(fleetID, designID, null, (ShipParams)0, new int?(), 0);
					game.AddDefaultStartingRiders(fleetID, designID, carrierID);
				}
			}
			return fleetID;
		}

		public void AddEncounter(GameSession game, PlayerInfo targetPlayer)
		{
		}

		public static Matrix GetBaseEnemyFleetTrans(App app, PirateBaseInfo pbi)
		{
			if (pbi == null)
				return Matrix.Identity;
			StationInfo stationInfo = app.GameDatabase.GetStationInfo(pbi.BaseStationId);
			OrbitalObjectInfo orbitalObjectInfo = app.GameDatabase.GetOrbitalObjectInfo(stationInfo.OrbitalObjectID);
			float radius = StarSystemVars.Instance.SizeToRadius(app.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ParentID.Value).Size);
			Matrix orbitalTransform = app.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ParentID.Value);
			Vector3 vector3 = Vector3.Normalize(orbitalTransform.Position);
			return Matrix.CreateWorld(orbitalTransform.Position + vector3 * (float)((double)radius + 750.0 + 1000.0), -vector3, Vector3.UnitY);
		}

		public static Matrix GetSpawnTransform(App app, PirateBaseInfo pbi)
		{
			if (pbi == null)
				return Matrix.Identity;
			StationInfo stationInfo = app.GameDatabase.GetStationInfo(pbi.BaseStationId);
			OrbitalObjectInfo orbitalObjectInfo = app.GameDatabase.GetOrbitalObjectInfo(stationInfo.OrbitalObjectID);
			Matrix orbitalTransform1 = app.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ID);
			Matrix orbitalTransform2 = app.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ParentID.Value);
			Vector3 vector3 = Vector3.Normalize(orbitalTransform1.Position - orbitalTransform2.Position);
			return Matrix.CreateWorld(orbitalTransform1.Position + vector3 * 2000f, -vector3, Vector3.UnitY);
		}
	}
}
