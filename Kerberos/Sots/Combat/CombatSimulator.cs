// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.CombatSimulator
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameTriggers;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal static class CombatSimulator
	{
		private static Random _rand = new Random();

		private static void ApplyWeaponStats(ShipCombatInfo sci, LogicalWeapon lw, int totalMounts)
		{
			if (lw == null || (double)lw.RechargeTime <= 0.0)
				return;
			float num = (float)(((double)lw.Duration > 0.0 ? (double)lw.Duration : 1.0) / (double)lw.RechargeTime * 60.0) * (float)totalMounts;
			if (lw.PayloadType == WeaponEnums.PayloadTypes.Missile || lw.PayloadType == WeaponEnums.PayloadTypes.Torpedo)
				sci.trackingFireFactor += lw.RangeTable.Effective.Damage * num;
			if (((IEnumerable<WeaponEnums.WeaponTraits>)lw.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.PointDefence))
				sci.pdFactor += lw.RangeTable.Effective.Damage * num;
			sci.directFireFactor = lw.RangeTable.PointBlank.Damage * num;
			sci.bombFactorPopulation += lw.PopDamage * num;
			sci.bombFactorInfrastructure += lw.InfraDamage * num;
			sci.bombFactorHazard += lw.TerraDamage * num;
		}

		private static void ApplyDeaths(
		  Dictionary<FleetInfo, List<ShipCombatInfo>> shipCombatInfo)
		{
			foreach (KeyValuePair<FleetInfo, List<ShipCombatInfo>> keyValuePair in shipCombatInfo)
			{
				foreach (ShipCombatInfo shipCombatInfo1 in keyValuePair.Value)
				{
					if ((double)shipCombatInfo1.structureFactor <= 0.0)
						shipCombatInfo1.shipDead = true;
				}
			}
		}

		public static void Simulate(GameSession game, int systemId, List<FleetInfo> fleets)
		{
			if (ScriptHost.AllowConsole)
				App.Log.Trace(string.Format("Simulating AI combat at: {0}", (object)systemId), "combat");
			List<PlanetCombatInfo> planets = new List<PlanetCombatInfo>();
			PlanetInfo[] systemPlanetInfos = game.GameDatabase.GetStarSystemPlanetInfos(systemId);
			if (systemPlanetInfos != null)
			{
				foreach (PlanetInfo planetInfo in systemPlanetInfos)
					planets.Add(new PlanetCombatInfo()
					{
						planetInfo = planetInfo,
						colonyInfo = game.GameDatabase.GetColonyInfoForPlanet(planetInfo.ID)
					});
			}
			Dictionary<FleetInfo, List<ShipCombatInfo>> shipCombatInfo = new Dictionary<FleetInfo, List<ShipCombatInfo>>();
			foreach (FleetInfo fleet in fleets)
			{
				List<ShipCombatInfo> shipCombatInfoList = new List<ShipCombatInfo>();
				foreach (ShipInfo shipInfo in game.GameDatabase.GetShipInfoByFleetID(fleet.ID, true).ToList<ShipInfo>())
				{
					if (shipInfo.DesignInfo.Class != ShipClass.BattleRider)
					{
						ShipCombatInfo sci = new ShipCombatInfo();
						sci.shipInfo = shipInfo;
						float num = 1f;
						if (shipInfo.DesignInfo.Class == ShipClass.Cruiser || shipInfo.DesignInfo.Class == ShipClass.Dreadnought)
							num = 3f;
						sci.armorFactor = (float)shipInfo.DesignInfo.Armour / num;
						sci.structureFactor = shipInfo.DesignInfo.Structure / num;
						foreach (DesignSectionInfo designSection in shipInfo.DesignInfo.DesignSections)
						{
							ShipSectionAsset shipSectionAsset = game.AssetDatabase.GetShipSectionAsset(designSection.FilePath);
							foreach (WeaponBankInfo weaponBank in designSection.WeaponBanks)
							{
								WeaponBankInfo wbi = weaponBank;
								if (wbi.WeaponID.HasValue)
								{
									string weaponName = Path.GetFileNameWithoutExtension(game.GameDatabase.GetWeaponAsset(wbi.WeaponID.Value));
									LogicalWeapon lw = game.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(weapon => string.Equals(weapon.WeaponName, weaponName, StringComparison.InvariantCultureIgnoreCase)));
									List<LogicalMount> list = ((IEnumerable<LogicalMount>)shipSectionAsset.Mounts).Where<LogicalMount>((Func<LogicalMount, bool>)(x => x.Bank.GUID == wbi.BankGUID)).ToList<LogicalMount>();
									int totalMounts = list.Count<LogicalMount>() <= 0 ? 1 : list.Count<LogicalMount>();
									foreach (LogicalMount logicalMount in list)
									{
										switch (logicalMount.Bank.TurretClass)
										{
											case WeaponEnums.TurretClasses.Drone:
												++sci.drones;
												continue;
											case WeaponEnums.TurretClasses.DestroyerRider:
											case WeaponEnums.TurretClasses.CruiserRider:
											case WeaponEnums.TurretClasses.DreadnoughtRider:
												++sci.battleRiders;
												continue;
											default:
												CombatSimulator.ApplyWeaponStats(sci, lw, totalMounts);
												continue;
										}
									}
								}
							}
							foreach (DesignModuleInfo module in designSection.Modules)
							{
								DesignModuleInfo mod = module;
								LogicalModule logicalModule = game.AssetDatabase.Modules.First<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == game.GameDatabase.GetModuleAsset(mod.ModuleID)));
								if (logicalModule != null && mod.WeaponID.HasValue)
								{
									foreach (LogicalBank bank in logicalModule.Banks)
									{
										LogicalBank lb = bank;
										string weaponName2 = Path.GetFileNameWithoutExtension(game.GameDatabase.GetWeaponAsset(mod.WeaponID.Value));
										LogicalWeapon lw = game.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(weapon => string.Equals(weapon.WeaponName, weaponName2, StringComparison.InvariantCultureIgnoreCase)));
										List<LogicalMount> list = ((IEnumerable<LogicalMount>)shipSectionAsset.Mounts).Where<LogicalMount>((Func<LogicalMount, bool>)(x => x.Bank.GUID == lb.GUID)).ToList<LogicalMount>();
										int totalMounts = list.Count<LogicalMount>() <= 0 ? 1 : list.Count<LogicalMount>();
										foreach (LogicalMount logicalMount in list)
											CombatSimulator.ApplyWeaponStats(sci, lw, totalMounts);
									}
								}
							}
						}
						shipCombatInfoList.Add(sci);
					}
				}
				shipCombatInfo.Add(fleet, shipCombatInfoList);
			}
			if (fleets.Count<FleetInfo>() > 1)
			{
				CombatSimulator.TrackingPhase(shipCombatInfo, 4f);
				CombatSimulator.DirectPhase(shipCombatInfo, 4f);
				CombatSimulator.TrackingPhase(shipCombatInfo, 1f);
				CombatSimulator.DirectPhase(shipCombatInfo, 2f);
				CombatSimulator.BombardmentPhase(game.GameDatabase, shipCombatInfo, planets, 1f);
			}
			else
				CombatSimulator.BombardmentPhase(game.GameDatabase, shipCombatInfo, planets, 2f);
			CombatSimulator.CompleteSimulation(game, systemId, shipCombatInfo, planets);
		}

		private static void CompleteSimulation(
		  GameSession game,
		  int systemId,
		  Dictionary<FleetInfo, List<ShipCombatInfo>> shipCombatInfo,
		  List<PlanetCombatInfo> planets)
		{
			CombatData combatData = game.CombatData.AddCombat(GameSession.GetNextUniqueCombatID(), systemId, game.GameDatabase.GetTurnCount());
			foreach (KeyValuePair<FleetInfo, List<ShipCombatInfo>> keyValuePair1 in shipCombatInfo)
			{
				PlayerCombatData orAddPlayer = combatData.GetOrAddPlayer(keyValuePair1.Key.PlayerID);
				orAddPlayer.VictoryStatus = GameSession.VictoryStatus.Draw;
				foreach (ShipCombatInfo shipCombatInfo1 in keyValuePair1.Value)
				{
					ShipCombatInfo sci = shipCombatInfo1;
					if ((double)sci.structureFactor == 0.0)
					{
						if (sci.shipInfo.DesignInfo.IsSuulka())
						{
							TurnEvent turnEvent = game.GameDatabase.GetTurnEventsByTurnNumber(game.GameDatabase.GetTurnCount(), orAddPlayer.PlayerID).FirstOrDefault<TurnEvent>((Func<TurnEvent, bool>)(x => x.ShipID == sci.shipInfo.ID));
							if (turnEvent != null)
								game.GameDatabase.RemoveTurnEvent(turnEvent.ID);
							List<int> intList1 = new List<int>();
							List<int> intList2 = new List<int>();
							foreach (KeyValuePair<FleetInfo, List<ShipCombatInfo>> keyValuePair2 in shipCombatInfo)
							{
								if (orAddPlayer.PlayerID != keyValuePair2.Key.PlayerID && !intList1.Contains(keyValuePair2.Key.PlayerID) && !intList2.Contains(keyValuePair2.Key.PlayerID))
								{
									switch (game.GameDatabase.GetDiplomacyStateBetweenPlayers(orAddPlayer.PlayerID, keyValuePair2.Key.PlayerID))
									{
										case DiplomacyState.WAR:
											intList1.Add(keyValuePair2.Key.PlayerID);
											continue;
										case DiplomacyState.NEUTRAL:
											intList2.Add(keyValuePair2.Key.PlayerID);
											continue;
										default:
											continue;
									}
								}
							}
							int num = 0;
							if (intList1.Count > 0)
								num = App.GetSafeRandom().Choose<int>((IList<int>)intList1);
							else if (intList2.Count > 0)
								num = App.GetSafeRandom().Choose<int>((IList<int>)intList2);
							game.GameDatabase.InsertTurnEvent(new TurnEvent()
							{
								EventType = TurnEventType.EV_SUULKA_DIES,
								EventMessage = TurnEventMessage.EM_SUULKA_DIES,
								PlayerID = num,
								SystemID = systemId,
								ShipID = sci.shipInfo.ID,
								DesignID = sci.shipInfo.DesignID,
								TurnNumber = game.GameDatabase.GetTurnCount(),
								ShowsDialog = false
							});
							SuulkaInfo suulkaByShipId = game.GameDatabase.GetSuulkaByShipID(sci.shipInfo.ID);
							if (suulkaByShipId != null)
								game.GameDatabase.RemoveSuulka(suulkaByShipId.ID);
						}
						game.GameDatabase.RemoveShip(sci.shipInfo.ID);
						GameTrigger.PushEvent(EventType.EVNT_SHIPDIED, (object)sci.shipInfo.DesignInfo.Class, game);
						orAddPlayer.AddShipData(sci.shipInfo.DesignID, 0.0f, 0.0f, 0, true);
						if (ScriptHost.AllowConsole)
							App.Log.Trace(string.Format("Ship destroyed: {0} ({1})", (object)sci.shipInfo.ID, (object)sci.shipInfo.ShipName), "combat");
					}
					else
					{
						if (sci.shipInfo.DesignInfo == null)
							sci.shipInfo.DesignInfo = game.GameDatabase.GetDesignInfo(sci.shipInfo.DesignID);
						foreach (SectionInstanceInfo sectionInstanceInfo in game.GameDatabase.GetShipSectionInstances(sci.shipInfo.ID).ToList<SectionInstanceInfo>())
						{
							SectionInstanceInfo sii = sectionInstanceInfo;
							int minStructure = ((IEnumerable<DesignSectionInfo>)sci.shipInfo.DesignInfo.DesignSections).First<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.ID == sii.SectionID)).GetMinStructure(game.GameDatabase, game.AssetDatabase);
							sii.Structure -= sii.Structure - (int)Math.Round((double)sci.structureFactor);
							sii.Structure = Math.Max(sii.Structure, minStructure);
							game.GameDatabase.UpdateSectionInstance(sii);
							if (sii.Structure == minStructure)
							{
								foreach (ModuleInstanceInfo module in game.GameDatabase.GetModuleInstances(sii.ID).ToList<ModuleInstanceInfo>())
								{
									module.Structure = 0;
									game.GameDatabase.UpdateModuleInstance(module);
								}
								foreach (WeaponInstanceInfo weapon in game.GameDatabase.GetWeaponInstances(sii.ID).ToList<WeaponInstanceInfo>())
								{
									weapon.Structure = 0.0f;
									game.GameDatabase.UpdateWeaponInstance(weapon);
								}
							}
						}
					}
				}
				if (!CombatSimulator.IsFleetAlive(keyValuePair1.Value))
				{
					game.GameDatabase.RemoveFleet(keyValuePair1.Key.ID);
					GameTrigger.PushEvent(EventType.EVNT_FLEETDIED, (object)keyValuePair1.Key.Name, game);
					if (ScriptHost.AllowConsole)
						App.Log.Trace(string.Format("Fleet destroyed: {0} ({1})", (object)keyValuePair1.Key.ID, (object)keyValuePair1.Key.Name), "combat");
				}
				else
					CombatSimulator.CheckFleetCommandPoints(game, keyValuePair1.Key);
			}
			bool flag = true;
			foreach (PlanetCombatInfo planet in planets)
			{
				game.GameDatabase.UpdatePlanet(planet.planetInfo);
				if (planet.colonyInfo != null)
				{
					if (planet.colonyInfo.ImperialPop <= 0.0)
					{
						game.GameDatabase.RemoveColonyOnPlanet(planet.planetInfo.ID);
						if (ScriptHost.AllowConsole)
							App.Log.Trace(string.Format("Colony defeated: planetid={0}", (object)planet.planetInfo.ID), "combat");
					}
					else
					{
						flag = false;
						planet.colonyInfo.DamagedLastTurn = true;
						game.GameDatabase.UpdateColony(planet.colonyInfo);
						foreach (ColonyFactionInfo faction in planet.colonyInfo.Factions)
							game.GameDatabase.UpdateCivilianPopulation(faction);
					}
				}
			}
			if (flag)
			{
				foreach (StationInfo stationInfo in game.GameDatabase.GetStationForSystem(systemId).ToList<StationInfo>())
					game.GameDatabase.DestroyStation(game, stationInfo.ID, 0);
			}
			game.GameDatabase.InsertCombatData(systemId, combatData.CombatID, combatData.Turn, combatData.ToByteArray());
		}

		private static bool IsFleetAlive(List<ShipCombatInfo> ships)
		{
			bool flag = false;
			foreach (ShipCombatInfo ship in ships)
			{
				if ((double)ship.structureFactor > 0.0)
				{
					flag = true;
					break;
				}
			}
			return flag;
		}

		private static void CheckFleetCommandPoints(GameSession game, FleetInfo fleet)
		{
			if (game.GameDatabase.GetFactionName(game.GameDatabase.GetPlayerFactionID(fleet.PlayerID)) == "loa")
				return;
			List<ShipInfo> list = game.GameDatabase.GetShipInfoByFleetID(fleet.ID, false).ToList<ShipInfo>();
			if (list.Count == 0 || fleet.Type != FleetType.FL_NORMAL || list.Max<ShipInfo>((Func<ShipInfo, int>)(x => game.GameDatabase.GetShipCommandPointQuota(x.ID))) != 0)
				return;
			int num = game.GameDatabase.InsertFleet(fleet.PlayerID, 0, fleet.SystemID, fleet.SupportingSystemID, App.Localize("@FLEET_RETREAT"), FleetType.FL_NORMAL);
			int missionID = game.GameDatabase.InsertMission(num, MissionType.RETREAT, 0, 0, 0, 0, false, new int?());
			game.GameDatabase.InsertWaypoint(missionID, WaypointType.ReturnHome, new int?());
			game.GameDatabase.InsertWaypoint(missionID, WaypointType.DisbandFleet, new int?());
			foreach (ShipInfo shipInfo in list)
				game.GameDatabase.TransferShip(shipInfo.ID, num);
			game.GameDatabase.RemoveFleet(fleet.ID);
		}

		private static ShipCombatInfo SelectTargetShip(
		  FleetInfo currentFleet,
		  Dictionary<FleetInfo, List<ShipCombatInfo>> shipCombatInfo)
		{
			IEnumerable<KeyValuePair<FleetInfo, List<ShipCombatInfo>>> source = shipCombatInfo.Where<KeyValuePair<FleetInfo, List<ShipCombatInfo>>>((Func<KeyValuePair<FleetInfo, List<ShipCombatInfo>>, bool>)(x =>
		   {
			   if (x.Key != currentFleet)
				   return CombatSimulator.IsFleetAlive(x.Value);
			   return false;
		   }));
			if (source.Count<KeyValuePair<FleetInfo, List<ShipCombatInfo>>>() == 0)
				return (ShipCombatInfo)null;
			int index = CombatSimulator._rand.Next(0, source.Count<KeyValuePair<FleetInfo, List<ShipCombatInfo>>>() - 1);
			KeyValuePair<FleetInfo, List<ShipCombatInfo>> keyValuePair = source.ElementAt<KeyValuePair<FleetInfo, List<ShipCombatInfo>>>(index);
			ShipCombatInfo shipCombatInfo1 = (ShipCombatInfo)null;
			foreach (ShipCombatInfo shipCombatInfo2 in keyValuePair.Value)
			{
				if (shipCombatInfo2.shipInfo.DesignInfo.Role == ShipRole.COMMAND && (double)shipCombatInfo2.structureFactor > 0.0)
				{
					shipCombatInfo1 = shipCombatInfo2;
					break;
				}
			}
			if (shipCombatInfo1 == null)
			{
				foreach (ShipCombatInfo shipCombatInfo2 in keyValuePair.Value)
				{
					if ((double)shipCombatInfo2.structureFactor > 0.0)
					{
						shipCombatInfo1 = shipCombatInfo2;
						break;
					}
				}
			}
			return shipCombatInfo1;
		}

		private static void ApplyShipDamage(ShipCombatInfo targetShip, float damage)
		{
			if ((double)damage <= 0.0)
				return;
			if ((double)targetShip.armorFactor > (double)damage)
			{
				float num = damage * 0.25f;
				damage -= damage * 0.25f;
				targetShip.armorFactor -= damage;
				targetShip.structureFactor -= num;
				damage = 0.0f;
			}
			else
			{
				float armorFactor = targetShip.armorFactor;
				targetShip.armorFactor = 0.0f;
				damage -= armorFactor;
			}
			if ((double)damage <= 0.0)
				return;
			if ((double)targetShip.structureFactor > (double)damage)
			{
				targetShip.structureFactor -= damage;
				damage = 0.0f;
			}
			else
				targetShip.structureFactor = 0.0f;
		}

		private static bool TrackingPhase(
		  Dictionary<FleetInfo, List<ShipCombatInfo>> shipCombatInfo,
		  float damageMultiplier = 1f)
		{
			foreach (KeyValuePair<FleetInfo, List<ShipCombatInfo>> keyValuePair in shipCombatInfo)
			{
				ShipCombatInfo targetShip = CombatSimulator.SelectTargetShip(keyValuePair.Key, shipCombatInfo);
				if (targetShip != null)
				{
					foreach (ShipCombatInfo shipCombatInfo1 in keyValuePair.Value)
					{
						if (!shipCombatInfo1.shipDead)
						{
							if ((double)targetShip.structureFactor <= 0.0)
								targetShip = CombatSimulator.SelectTargetShip(keyValuePair.Key, shipCombatInfo);
							if (targetShip != null)
							{
								float damage = shipCombatInfo1.trackingFireFactor * (float)(0.75 + CombatSimulator._rand.NextDouble() * 1.25) * damageMultiplier - targetShip.pdFactor;
								CombatSimulator.ApplyShipDamage(targetShip, damage);
							}
							else
								break;
						}
					}
				}
			}
			CombatSimulator.ApplyDeaths(shipCombatInfo);
			return true;
		}

		private static bool DirectPhase(
		  Dictionary<FleetInfo, List<ShipCombatInfo>> shipCombatInfo,
		  float damageMultiplier = 1f)
		{
			foreach (KeyValuePair<FleetInfo, List<ShipCombatInfo>> keyValuePair in shipCombatInfo)
			{
				ShipCombatInfo targetShip = CombatSimulator.SelectTargetShip(keyValuePair.Key, shipCombatInfo);
				if (targetShip != null)
				{
					foreach (ShipCombatInfo shipCombatInfo1 in keyValuePair.Value)
					{
						if (!shipCombatInfo1.shipDead)
						{
							if ((double)targetShip.structureFactor <= 0.0)
							{
								targetShip = CombatSimulator.SelectTargetShip(keyValuePair.Key, shipCombatInfo);
								if (targetShip == null)
									break;
							}
							float damage = (float)((double)shipCombatInfo1.directFireFactor * (0.5 + CombatSimulator._rand.NextDouble() * 1.5) * (double)damageMultiplier * (1.0 + (double)shipCombatInfo1.battleRiders * 0.150000005960464) * (1.0 + (double)shipCombatInfo1.drones * 0.0500000007450581));
							CombatSimulator.ApplyShipDamage(targetShip, damage);
						}
					}
				}
			}
			CombatSimulator.ApplyDeaths(shipCombatInfo);
			return true;
		}

		private static bool BombardmentPhase(
		  GameDatabase db,
		  Dictionary<FleetInfo, List<ShipCombatInfo>> shipCombatInfo,
		  List<PlanetCombatInfo> planets,
		  float damageMultiplier = 1f)
		{
			foreach (PlanetCombatInfo planet in planets)
			{
				PlanetCombatInfo pci = planet;
				if (pci.colonyInfo != null && pci.colonyInfo.ImperialPop != 0.0)
				{
					IEnumerable<KeyValuePair<FleetInfo, List<ShipCombatInfo>>> keyValuePairs = shipCombatInfo.Where<KeyValuePair<FleetInfo, List<ShipCombatInfo>>>((Func<KeyValuePair<FleetInfo, List<ShipCombatInfo>>, bool>)(x => x.Key.PlayerID != pci.colonyInfo.PlayerID));
					float num1 = CombatSimulator._rand.CoinToss(50) ? 1f : -1f;
					foreach (KeyValuePair<FleetInfo, List<ShipCombatInfo>> keyValuePair in keyValuePairs)
					{
						foreach (ShipCombatInfo shipCombatInfo1 in keyValuePair.Value)
						{
							pci.planetInfo.Infrastructure -= (float)((double)shipCombatInfo1.bombFactorInfrastructure * (double)damageMultiplier * 0.0500000007450581);
							pci.planetInfo.Suitability += (float)((double)num1 * (double)shipCombatInfo1.bombFactorHazard * (double)damageMultiplier * 0.5);
							pci.planetInfo.Infrastructure = Math.Max(0.0f, pci.planetInfo.Infrastructure);
							pci.planetInfo.Suitability = Math.Max(Math.Min(Constants.MaxSuitability, pci.planetInfo.Suitability), Constants.MinSuitability);
							float num2 = shipCombatInfo1.bombFactorPopulation / ((float)((IEnumerable<ColonyFactionInfo>)pci.colonyInfo.Factions).Count<ColonyFactionInfo>() + 1f) * damageMultiplier;
							pci.colonyInfo.ImperialPop = Math.Max(0.0, pci.colonyInfo.ImperialPop - Math.Ceiling((double)num2));
							foreach (ColonyFactionInfo faction in pci.colonyInfo.Factions)
								faction.CivilianPop = Math.Max(0.0, faction.CivilianPop - Math.Ceiling((double)num2));
						}
					}
				}
			}
			bool flag = false;
			foreach (PlanetCombatInfo planet in planets)
			{
				if (planet.colonyInfo != null && planet.colonyInfo.ImperialPop > 0.0)
				{
					flag = true;
					break;
				}
			}
			return flag;
		}
	}
}
