// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.StarFleet.StarFleet
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.StarFleet
{
	internal class StarFleet
	{
		private const int GATE_COST = 2000;
		public static bool FleetsAlwaysInRange;

		public static bool IsFleetAvailableForMission(GameSession game, int fleetID, int systemID)
		{
			if (game.GameDatabase.GetMissionByFleetID(fleetID) != null)
				return false;
			return Kerberos.Sots.StarFleet.StarFleet.IsFleetInRange(game, fleetID, systemID, new float?(), new float?(), new float?());
		}

		public static int GetRepairPointsMax(App app, DesignInfo des)
		{
			int num = 0;
			for (int index = 0; index < ((IEnumerable<DesignSectionInfo>)des.DesignSections).Count<DesignSectionInfo>(); ++index)
			{
				ShipSectionAsset shipSectionAsset = app.AssetDatabase.GetShipSectionAsset(des.DesignSections[index].FilePath);
				num += shipSectionAsset.RepairPoints;
				foreach (DesignModuleInfo module in des.DesignSections[index].Modules)
				{
					string mPath = app.GameDatabase.GetModuleAsset(module.ModuleID);
					LogicalModule logicalModule = app.AssetDatabase.Modules.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == mPath));
					num += logicalModule.RepairPointsBonus;
				}
			}
			return num;
		}

		public static bool GetIsSalavageCapable(App app, DesignInfo des)
		{
			for (int index = 0; index < ((IEnumerable<DesignSectionInfo>)des.DesignSections).Count<DesignSectionInfo>(); ++index)
			{
				ShipSectionAsset shipSectionAsset = app.AssetDatabase.GetShipSectionAsset(des.DesignSections[index].FilePath);
				if (shipSectionAsset.SectionName == "cr_mis_repair" || shipSectionAsset.SectionName == "cr_mis_repair_salvage" || shipSectionAsset.SectionName == "dn_mis_supply")
					return true;
			}
			return false;
		}

		public static int GetSalvageChance(App app, DesignInfo des)
		{
			Faction faction = app.GetPlayer(des.PlayerID).Faction;
			for (int index = 0; index < ((IEnumerable<DesignSectionInfo>)des.DesignSections).Count<DesignSectionInfo>(); ++index)
			{
				ShipSectionAsset shipSectionAsset = app.AssetDatabase.GetShipSectionAsset(des.DesignSections[index].FilePath);
				if (shipSectionAsset.SectionName == "cr_mis_repair" || shipSectionAsset.SectionName == "cr_mis_repair_salvage" || shipSectionAsset.SectionName == "dn_mis_supply")
					return faction.RepSel;
			}
			return faction.DefaultRepSel;
		}

		public static int[] GetHealthAndHealthMax(GameSession game, DesignInfo design, int shipid)
		{
			int num1 = 0;
			int num2 = 0;
			int designID = 0;
			int num3 = 0;
			List<SectionInstanceInfo> list1 = game.GameDatabase.GetShipSectionInstances(shipid).ToList<SectionInstanceInfo>();
			for (int i = 0; i < ((IEnumerable<DesignSectionInfo>)design.DesignSections).Count<DesignSectionInfo>(); ++i)
			{
				SectionInstanceInfo sectionInstanceInfo = list1.FirstOrDefault<SectionInstanceInfo>((Func<SectionInstanceInfo, bool>)(x => x.SectionID == design.DesignSections[i].ID));
				ShipSectionAsset shipSectionAsset = game.AssetDatabase.GetShipSectionAsset(design.DesignSections[i].FilePath);
				List<string> techs = new List<string>();
				if (design.DesignSections[i].Techs.Count > 0)
				{
					foreach (int tech in design.DesignSections[i].Techs)
						techs.Add(game.GameDatabase.GetTechFileID(tech));
				}
				int structureWithTech = Ship.GetStructureWithTech(game.AssetDatabase, techs, shipSectionAsset.Structure);
				num2 += structureWithTech;
				num1 += sectionInstanceInfo != null ? sectionInstanceInfo.Structure : structureWithTech;
				if (sectionInstanceInfo != null)
				{
					Dictionary<ArmorSide, DamagePattern> armorInstances = game.GameDatabase.GetArmorInstances(sectionInstanceInfo.ID);
					if (armorInstances.Count > 0)
					{
						for (int index1 = 0; index1 < 4; ++index1)
						{
							num2 += armorInstances[(ArmorSide)index1].Width * armorInstances[(ArmorSide)index1].Height * 3;
							ArmorSide index2 = (ArmorSide)index1;
							for (int x = 0; x < armorInstances[index2].Width; ++x)
							{
								for (int y = 0; y < armorInstances[index2].Height; ++y)
								{
									if (!armorInstances[index2].GetValue(x, y))
										num1 += 3;
								}
							}
						}
					}
					List<ModuleInstanceInfo> list2 = game.GameDatabase.GetModuleInstances(sectionInstanceInfo.ID).ToList<ModuleInstanceInfo>();
					List<DesignModuleInfo> module = design.DesignSections[i].Modules;
					for (int mod = 0; mod < module.Count; ++mod)
					{
						ModuleInstanceInfo moduleInstanceInfo = list2.FirstOrDefault<ModuleInstanceInfo>((Func<ModuleInstanceInfo, bool>)(x => x.ModuleNodeID == module[mod].MountNodeName));
						string modAsset = game.GameDatabase.GetModuleAsset(module[mod].ModuleID);
						LogicalModule logicalModule = game.AssetDatabase.Modules.Where<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == modAsset)).First<LogicalModule>();
						num2 += (int)logicalModule.Structure;
						num1 += moduleInstanceInfo != null ? moduleInstanceInfo.Structure : (int)Math.Ceiling((double)logicalModule.Structure);
						if (module[mod].DesignID.HasValue)
						{
							foreach (LogicalMount mount in logicalModule.Mounts)
							{
								if (WeaponEnums.IsWeaponBattleRider(mount.Bank.TurretClass))
								{
									if (designID == 0)
										designID = module[mod].DesignID.Value;
									++num3;
								}
							}
						}
					}
					foreach (WeaponInstanceInfo weaponInstanceInfo in game.GameDatabase.GetWeaponInstances(sectionInstanceInfo.ID).ToList<WeaponInstanceInfo>())
					{
						num2 += (int)Math.Ceiling((double)weaponInstanceInfo.MaxStructure);
						num1 += (int)Math.Ceiling((double)weaponInstanceInfo.Structure);
					}
					foreach (LogicalMount mount1 in shipSectionAsset.Mounts)
					{
						LogicalMount mount = mount1;
						if (WeaponEnums.IsWeaponBattleRider(mount.Bank.TurretClass))
						{
							if (designID == 0)
							{
								WeaponBankInfo weaponBankInfo = design.DesignSections[i].WeaponBanks.FirstOrDefault<WeaponBankInfo>((Func<WeaponBankInfo, bool>)(x =>
							   {
								   if (!(x.BankGUID == mount.Bank.GUID) || !x.DesignID.HasValue)
									   return false;
								   int? designId = x.DesignID;
								   if (designId.GetValueOrDefault() == 0)
									   return !designId.HasValue;
								   return true;
							   }));
								designID = weaponBankInfo == null || !weaponBankInfo.DesignID.HasValue ? 0 : weaponBankInfo.DesignID.Value;
							}
							++num3;
						}
					}
				}
			}
			List<ShipInfo> list3 = game.GameDatabase.GetBattleRidersByParentID(shipid).ToList<ShipInfo>();
			if (num3 > 0)
			{
				int num4 = num3;
				foreach (ShipInfo shipInfo in list3)
				{
					DesignInfo designInfo = game.GameDatabase.GetDesignInfo(shipInfo.DesignID);
					if (designInfo != null)
					{
						DesignSectionInfo designSectionInfo = ((IEnumerable<DesignSectionInfo>)designInfo.DesignSections).FirstOrDefault<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.ShipSectionAsset.Type == ShipSectionType.Mission));
						if (designSectionInfo != null && ShipSectionAsset.IsBattleRiderClass(designSectionInfo.ShipSectionAsset.RealClass))
							--num4;
					}
				}
				int num5 = 0;
				if (designID != 0)
				{
					foreach (DesignSectionInfo designSection in game.GameDatabase.GetDesignInfo(designID).DesignSections)
					{
						ShipSectionAsset shipSectionAsset = game.AssetDatabase.GetShipSectionAsset(designSection.FilePath);
						num5 = shipSectionAsset.Structure;
						int repairPoints = shipSectionAsset.RepairPoints;
						if (shipSectionAsset.Armor.Length > 0)
						{
							for (int index = 0; index < 4; ++index)
								num5 += shipSectionAsset.Armor[index].X * shipSectionAsset.Armor[index].Y * 3;
						}
					}
				}
				num2 += num5 * num3;
				num1 += num5 * (num3 - num4);
			}
			return new int[2] { num1, num2 };
		}

		private static void Warn(string message)
		{
			App.Log.Warn(message, "game");
		}

		public static void RepairShip(App app, ShipInfo ship, int points)
		{
			List<SectionInstanceInfo> list1 = app.GameDatabase.GetShipSectionInstances(ship.ID).ToList<SectionInstanceInfo>();
			List<DesignSectionInfo> sections = ((IEnumerable<DesignSectionInfo>)app.GameDatabase.GetShipInfo(ship.ID, true).DesignInfo.DesignSections).ToList<DesignSectionInfo>();
			List<int> source = new List<int>();
			int num1 = 0;
			int designID = 0;
			int num2 = 0;
			int num3 = list1.Count * 5;
			for (int j = 0; j < sections.Count; ++j)
			{
				SectionInstanceInfo sectionInstanceInfo = list1.First<SectionInstanceInfo>((Func<SectionInstanceInfo, bool>)(x => x.SectionID == sections[j].ID));
				List<ModuleInstanceInfo> list2 = app.GameDatabase.GetModuleInstances(sectionInstanceInfo.ID).ToList<ModuleInstanceInfo>();
				num3 = num3 + list2.Count + app.GameDatabase.GetWeaponInstances(sectionInstanceInfo.ID).ToList<WeaponInstanceInfo>().Count;
				foreach (LogicalMount mount1 in app.AssetDatabase.GetShipSectionAsset(sections[j].FilePath).Mounts)
				{
					LogicalMount mount = mount1;
					if (WeaponEnums.IsWeaponBattleRider(mount.Bank.TurretClass))
					{
						if (designID == 0)
						{
							WeaponBankInfo weaponBankInfo = sections[j].WeaponBanks.FirstOrDefault<WeaponBankInfo>((Func<WeaponBankInfo, bool>)(x =>
						   {
							   if (!(x.BankGUID == mount.Bank.GUID) || !x.DesignID.HasValue)
								   return false;
							   int? designId = x.DesignID;
							   if (designId.GetValueOrDefault() == 0)
								   return !designId.HasValue;
							   return true;
						   }));
							designID = weaponBankInfo == null || !weaponBankInfo.DesignID.HasValue ? 0 : weaponBankInfo.DesignID.Value;
						}
						++num2;
						source.Add(num1);
						++num1;
					}
					else if (WeaponEnums.IsBattleRider(mount.Bank.TurretClass))
						++num1;
				}
				if (list2.Count > 0)
				{
					foreach (ModuleInstanceInfo moduleInstanceInfo in list2)
					{
						ModuleInstanceInfo mii = moduleInstanceInfo;
						DesignModuleInfo designModuleInfo = sections[j].Modules.First<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(x => x.MountNodeName == mii.ModuleNodeID));
						if (designModuleInfo.DesignID.HasValue)
						{
							string modAsset = app.GameDatabase.GetModuleAsset(designModuleInfo.ModuleID);
							foreach (LogicalMount mount in app.AssetDatabase.Modules.Where<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == modAsset)).First<LogicalModule>().Mounts)
							{
								if (WeaponEnums.IsWeaponBattleRider(mount.Bank.TurretClass))
								{
									if (designID == 0)
										designID = designModuleInfo.DesignID.Value;
									++num2;
									source.Add(num1);
									++num1;
								}
								else
									++num1;
							}
						}
					}
				}
			}
			int num4 = num2;
			int num5 = 0;
			int num6 = 0;
			foreach (ShipInfo shipInfo in app.GameDatabase.GetBattleRidersByParentID(ship.ID).ToList<ShipInfo>())
			{
				if (shipInfo.DesignID == designID)
				{
					--num4;
					source.Remove(shipInfo.RiderIndex);
				}
			}
			DesignInfo designInfo = app.GameDatabase.GetDesignInfo(designID);
			if (designInfo != null)
			{
				foreach (DesignSectionInfo designSection in designInfo.DesignSections)
				{
					num5 += designSection.ShipSectionAsset.Structure;
					num6 += designSection.ShipSectionAsset.RepairPoints;
					if (designSection.ShipSectionAsset.Armor.Length > 0)
					{
						for (int index = 0; index < 4; ++index)
							num5 += designSection.ShipSectionAsset.Armor[index].X * designSection.ShipSectionAsset.Armor[index].Y * 3;
					}
				}
			}
			int num7 = num3 + num4;
			if (num7 <= 0)
			{
				Kerberos.Sots.StarFleet.StarFleet.Warn("StarFleet.RepairShip: thingsToRepair <= 0");
			}
			else
			{
				int num8 = points / sections.Count;
				if (num8 == 0)
					num8 = points;
				int num9 = Math.Min(num8 + (3 - num8 % 3), points);
				int num10 = 0;
				if (num8 <= 0)
				{
					Kerberos.Sots.StarFleet.StarFleet.Warn("StarFleet.RepairShip: pointsPerSection <= 0");
				}
				else
				{
					int num11 = Math.Max(50, points / num8);
					while (points > 0 && num10 != num7 && num11 > 0)
					{
						--num11;
						for (int j = 0; j < sections.Count; ++j)
						{
							SectionInstanceInfo sectionInstanceInfo = list1.First<SectionInstanceInfo>((Func<SectionInstanceInfo, bool>)(x => x.SectionID == sections[j].ID));
							List<ModuleInstanceInfo> list2 = app.GameDatabase.GetModuleInstances(sectionInstanceInfo.ID).ToList<ModuleInstanceInfo>();
							List<DesignModuleInfo> module = sections[j].Modules;
							List<WeaponInstanceInfo> list3 = app.GameDatabase.GetWeaponInstances(sectionInstanceInfo.ID).ToList<WeaponInstanceInfo>();
							ShipSectionAsset shipSectionAsset = app.AssetDatabase.GetShipSectionAsset(sections[j].FilePath);
							List<string> techs = new List<string>();
							if (sections[j].Techs.Count > 0)
							{
								foreach (int tech in sections[j].Techs)
									techs.Add(app.GameDatabase.GetTechFileID(tech));
							}
							int structureWithTech = Ship.GetStructureWithTech(app.AssetDatabase, techs, shipSectionAsset.Structure);
							int num12 = structureWithTech - sectionInstanceInfo.Structure;
							if (num12 > 0)
							{
								if (num12 > num8)
									num12 = num8;
								if (num12 > points)
									num12 = points;
								sectionInstanceInfo.Structure += num12;
								if (sectionInstanceInfo.Structure == structureWithTech)
									++num10;
								points -= num12;
							}
							Dictionary<ArmorSide, DamagePattern> armorInstances = app.GameDatabase.GetArmorInstances(sectionInstanceInfo.ID);
							if (armorInstances.Count > 0)
							{
								for (int index1 = 0; index1 < 4; ++index1)
								{
									ArmorSide index2 = (ArmorSide)index1;
									int num13 = armorInstances[index2].Width * armorInstances[index2].Height * 3;
									int num14 = armorInstances[index2].GetTotalFilled() * 3;
									int num15 = num13 - num14;
									if (num15 > 0)
									{
										if (num15 > num9)
											num15 = num9;
										if (num15 > points)
											num15 = points;
										if (num14 + num15 == num13)
											++num10;
										points -= num15;
									}
									int num16 = num15;
									if (num16 > 0)
									{
										for (int y = armorInstances[index2].Height - 1; y >= 0; --y)
										{
											for (int x = 0; x < armorInstances[index2].Width; ++x)
											{
												if (armorInstances[index2].GetValue(x, y) && num16 >= 3)
												{
													armorInstances[index2].SetValue(x, y, false);
													num16 -= 3;
												}
												if (num16 <= 0)
													break;
											}
											if (num16 <= 0)
												break;
										}
									}
								}
								app.GameDatabase.UpdateArmorInstances(sectionInstanceInfo.ID, armorInstances);
							}
							if (list3.Count > 0)
							{
								foreach (WeaponInstanceInfo weapon in list3)
								{
									int num13 = (int)((double)weapon.MaxStructure - (double)weapon.Structure);
									if (num13 > 0)
									{
										if (num13 > num8)
											num13 = num8;
										if (num13 > points)
											num13 = points;
										weapon.Structure += (float)num13;
										if ((double)weapon.Structure == (double)weapon.MaxStructure)
											++num10;
										points -= num13;
									}
									app.GameDatabase.UpdateWeaponInstance(weapon);
								}
							}
							if (list2.Count == module.Count)
							{
								for (int mod = 0; mod < module.Count; ++mod)
								{
									string modAsset = app.GameDatabase.GetModuleAsset(module[mod].ModuleID);
									LogicalModule logicalModule = app.AssetDatabase.Modules.Where<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == modAsset)).First<LogicalModule>();
									ModuleInstanceInfo module1 = list2.First<ModuleInstanceInfo>((Func<ModuleInstanceInfo, bool>)(x => x.ModuleNodeID == module[mod].MountNodeName));
									int num13 = (int)((double)logicalModule.Structure - (double)module1.Structure);
									if (num13 > 0)
									{
										if (num13 > num8)
											num13 = num8;
										if (num13 > points)
											num13 = points;
										module1.Structure += num13;
										if ((double)list2[mod].Structure == (double)logicalModule.Structure)
											++num10;
										points -= num13;
									}
									app.GameDatabase.UpdateModuleInstance(module1);
								}
							}
							if (designID != 0 && num4 > 0)
							{
								int num13 = 0;
								int num14 = num5;
								for (int index = 0; index < num4 && (num14 > 0 && num14 <= points) && source.Count != 0; ++index)
								{
									points -= num14;
									int shipID = app.GameDatabase.InsertShip(ship.FleetID, designID, null, (ShipParams)0, new int?(), 0);
									app.GameDatabase.SetShipParent(shipID, ship.ID);
									app.GameDatabase.UpdateShipRiderIndex(shipID, source.First<int>());
									source.RemoveAt(0);
									++num13;
								}
								num4 -= num13;
								num10 += num13;
							}
						}
					}
				}
			}
			foreach (SectionInstanceInfo section in list1)
				app.GameDatabase.UpdateSectionInstance(section);
		}

		public static int GetFleetEndurance(GameSession game, int fleetID)
		{
			List<ShipInfo> list = game.GameDatabase.GetShipInfoByFleetID(fleetID, false).ToList<ShipInfo>();
			if (list.Count<ShipInfo>() == 0)
				return 0;
			int num1 = 0;
			int num2 = 0;
			int num3;
			if (game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerInfo(game.GameDatabase.GetFleetInfo(fleetID).PlayerID).FactionID).Name == "loa")
			{
				num3 = 15;
			}
			else
			{
				foreach (ShipInfo shipInfo in list)
				{
					DesignInfo designInfo = game.GameDatabase.GetDesignInfo(shipInfo.DesignID);
					RealShipClasses? realShipClass = designInfo.GetRealShipClass();
					if (realShipClass.HasValue)
					{
						switch (realShipClass.Value)
						{
							case RealShipClasses.BattleRider:
							case RealShipClasses.Drone:
							case RealShipClasses.BoardingPod:
							case RealShipClasses.EscapePod:
							case RealShipClasses.AssaultShuttle:
							case RealShipClasses.Biomissile:
								continue;
						}
					}
					num1 += designInfo.GetEndurance(game);
					++num2;
				}
				if (num2 == 0)
					return 0;
				num3 = num1 / num2;
			}
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetID);
			IEnumerable<AdmiralInfo.TraitType> admiralTraits = game.GameDatabase.GetAdmiralTraits(fleetInfo.AdmiralID);
			int num4 = 0;
			if (admiralTraits.Contains<AdmiralInfo.TraitType>(AdmiralInfo.TraitType.TrueGrit))
				num4 += 2;
			if (admiralTraits.Contains<AdmiralInfo.TraitType>(AdmiralInfo.TraitType.Thrifty))
				num4 += (int)((double)num3 * 0.200000002980232);
			if (admiralTraits.Contains<AdmiralInfo.TraitType>(AdmiralInfo.TraitType.Wastrel))
				num4 -= (int)((double)num3 * 0.200000002980232);
			if (admiralTraits.Contains<AdmiralInfo.TraitType>(AdmiralInfo.TraitType.DrillSergeant))
				num4 -= (int)((double)num3 * 0.0500000007450581);
			return num3 + num4;
		}

		public static bool IsFleetExhausted(GameSession game, FleetInfo fleet)
		{
			int fleetEndurance = Kerberos.Sots.StarFleet.StarFleet.GetFleetEndurance(game, fleet.ID);
			return fleet.TurnsAway >= fleetEndurance * 2 || (double)fleet.SupplyRemaining <= 0.0;
		}

		public static float GetFleetRange(GameSession game, FleetInfo fi)
		{
			Faction faction = game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerFactionID(fi.PlayerID));
			float supportRange = GameSession.GetSupportRange(game.AssetDatabase, game.GameDatabase, fi.PlayerID);
			int num1 = Math.Max(Kerberos.Sots.StarFleet.StarFleet.GetFleetEndurance(game, fi.ID) - fi.TurnsAway, 0);
			float num2 = (!faction.CanUseGate() ? (float)(num1 / 2) : (float)Math.Max(num1 - 1, 0)) * Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(game, fi.ID, faction.CanUseNodeLine(new bool?()));
			return supportRange + num2;
		}

		public static bool IsFleetInRange(
		  GameSession game,
		  int fleetID,
		  int systemID,
		  float? fleetRange = null,
		  float? travelSpeed = null,
		  float? nodeTravelSpeed = null)
		{
			if (Kerberos.Sots.StarFleet.StarFleet.FleetsAlwaysInRange)
				return true;
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetID);
			return Kerberos.Sots.StarFleet.StarFleet.IsFleetInRange(game, fleetInfo, systemID, fleetRange, travelSpeed, nodeTravelSpeed);
		}

		public static bool IsFleetInRange(
		  GameSession game,
		  FleetInfo fleet,
		  int systemID,
		  float? fleetRange = null,
		  float? travelSpeed = null,
		  float? nodeTravelSpeed = null)
		{
			if (Kerberos.Sots.StarFleet.StarFleet.FleetsAlwaysInRange)
				return true;
			float num = fleetRange.HasValue ? fleetRange.Value : Kerberos.Sots.StarFleet.StarFleet.GetFleetRange(game, fleet);
			int tripTime;
			float tripDistance;
			Kerberos.Sots.StarFleet.StarFleet.GetBestTravelPath(game, fleet.ID, fleet.SystemID, systemID, out tripTime, out tripDistance, false, travelSpeed, nodeTravelSpeed);
			return (double)tripDistance <= (double)num;
		}

		public static List<FleetInfo> GetFleetsInRangeOfSystem(
		  GameSession game,
		  int systemID,
		  Dictionary<FleetInfo, FleetRangeData> fleetRanges,
		  float scale = 1f)
		{
			List<FleetInfo> fleetInfoList = new List<FleetInfo>();
			foreach (KeyValuePair<FleetInfo, FleetRangeData> fleetRange in fleetRanges)
			{
				int tripTime;
				float tripDistance;
				Kerberos.Sots.StarFleet.StarFleet.GetBestTravelPath(game, fleetRange.Key.ID, fleetRange.Key.SystemID, systemID, out tripTime, out tripDistance, false, fleetRange.Value.FleetTravelSpeed, fleetRange.Value.FleetNodeTravelSpeed);
				if ((double)tripDistance <= (double)fleetRange.Value.FleetRange * (double)scale)
					fleetInfoList.Add(fleetRange.Key);
			}
			return fleetInfoList;
		}

		public static bool IsSuulkaFleet(GameDatabase db, FleetInfo fleet)
		{
			return Kerberos.Sots.StarFleet.StarFleet.HasSuulkaInList(db.GetShipInfoByFleetID(fleet.ID, true).ToList<ShipInfo>());
		}

		public static bool HasSuulkaInList(List<ShipInfo> ships)
		{
			foreach (ShipInfo ship in ships)
			{
				foreach (DesignSectionInfo designSection in ship.DesignInfo.DesignSections)
				{
					if (designSection.ShipSectionAsset.IsSuulka)
						return true;
				}
			}
			return false;
		}

		public static ShipInfo GetFleetSuulkaShipInfo(GameDatabase db, FleetInfo fleet)
		{
			foreach (ShipInfo shipInfo in db.GetShipInfoByFleetID(fleet.ID, true))
			{
				foreach (DesignSectionInfo designSection in shipInfo.DesignInfo.DesignSections)
				{
					if (designSection.ShipSectionAsset.IsSuulka)
						return shipInfo;
				}
			}
			return (ShipInfo)null;
		}

		public static bool IsGardenerFleet(GameSession game, FleetInfo fleet)
		{
			if (game.ScriptModules.Gardeners == null)
				return false;
			return game.GameDatabase.GetShipInfoByFleetID(fleet.ID, false).ToList<ShipInfo>().Any<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignID == game.ScriptModules.Gardeners.GardenerDesignId));
		}

		public static bool HasBoreShip(App app, int fleetID)
		{
			return app.GameDatabase.GetShipInfoByFleetID(fleetID, true).ToList<ShipInfo>().Any<ShipInfo>((Func<ShipInfo, bool>)(x => ((IEnumerable<DesignSectionInfo>)x.DesignInfo.DesignSections).Any<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(y => y.ShipSectionAsset.IsBoreShip))));
		}

		public static bool DesignIsSuulka(App app, DesignInfo design)
		{
			foreach (DesignSectionInfo designSection in design.DesignSections)
			{
				if (designSection.ShipSectionAsset.IsSuulka)
					return true;
			}
			return false;
		}

		public static List<int> GetFactionRequiredDesignsForFleet(
		  GameSession game,
		  int fleetID,
		  int targetSystemId)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			List<int> intList = new List<int>();
			if (game.GameDatabase.GetUnixTimeCreated() != 0.0)
				return intList;
			FleetInfo fleetInfo = gameDatabase.GetFleetInfo(fleetID);
			PlayerInfo playerInfo = gameDatabase.GetPlayerInfo(fleetInfo.PlayerID);
			switch (gameDatabase.GetFactionName(playerInfo.FactionID))
			{
				case "zuul":
					Player playerObject = game.GetPlayerObject(fleetInfo.PlayerID);
					if (playerObject != null && playerObject.IsAI() && (Kerberos.Sots.StarFleet.StarFleet.GetNodeTravelPath(gameDatabase, fleetInfo.SystemID, targetSystemId, playerInfo.ID, false, true, false).Count<int>() == 0 && !GameSession.FleetHasBore(gameDatabase, fleetID)))
					{
						int num = 0;
						foreach (DesignInfo designInfo in gameDatabase.GetDesignInfosForPlayer(playerInfo.ID))
						{
							foreach (DesignSectionInfo designSection in designInfo.DesignSections)
							{
								if (gameDatabase.AssetDatabase.GetShipSectionAsset(designSection.FilePath).IsBoreShip)
								{
									num = designInfo.ID;
									break;
								}
							}
							if (num != 0)
								break;
						}
						if (num != 0)
						{
							intList.Add(num);
							break;
						}
						break;
					}
					break;
			}
			return intList;
		}

		private static void MissionTrace(string message)
		{
			App.Log.Trace(message, "game");
		}

		public static int SetColonizationMission(
		  GameSession game,
		  int fleetID,
		  int systemID,
		  bool useDirectRoute,
		  int planetID,
		  List<int> designIDs,
		  int? ReBaseTarget = null)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			if (designIDs != null)
				designIDs.AddRange((IEnumerable<int>)Kerberos.Sots.StarFleet.StarFleet.GetFactionRequiredDesignsForFleet(game, fleetID, systemID));
			else
				designIDs = Kerberos.Sots.StarFleet.StarFleet.GetFactionRequiredDesignsForFleet(game, fleetID, systemID);
			int missionID = gameDatabase.InsertMission(fleetID, MissionType.COLONIZATION, systemID, planetID, 0, 1, useDirectRoute, new int?());
			if (designIDs != null && designIDs.Count<int>() > 0)
				Kerberos.Sots.StarFleet.StarFleet.AddShipsForMission(gameDatabase, fleetID, missionID, (IList<int>)designIDs);
			Kerberos.Sots.StarFleet.StarFleet.SetWaypointsForMission(game, MissionType.COLONIZATION, missionID, fleetID, systemID, 0, ReBaseTarget);
			Kerberos.Sots.StarFleet.StarFleet.MissionTrace("Fleet " + (object)fleetID + " sent on Colonization mission to system " + (object)systemID);
			return missionID;
		}

		public static int SetEvacuationMission(
		  GameSession game,
		  int fleetID,
		  int systemID,
		  bool useDirectRoute,
		  int planetID,
		  List<int> designIDs)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			if (designIDs != null)
				designIDs.AddRange((IEnumerable<int>)Kerberos.Sots.StarFleet.StarFleet.GetFactionRequiredDesignsForFleet(game, fleetID, systemID));
			else
				designIDs = Kerberos.Sots.StarFleet.StarFleet.GetFactionRequiredDesignsForFleet(game, fleetID, systemID);
			int missionID = gameDatabase.InsertMission(fleetID, MissionType.EVACUATE, systemID, planetID, 0, 1, useDirectRoute, new int?());
			if (designIDs != null && designIDs.Count<int>() > 0)
				Kerberos.Sots.StarFleet.StarFleet.AddShipsForMission(gameDatabase, fleetID, missionID, (IList<int>)designIDs);
			Kerberos.Sots.StarFleet.StarFleet.SetWaypointsForMission(game, MissionType.EVACUATE, missionID, fleetID, systemID, 0, new int?());
			Kerberos.Sots.StarFleet.StarFleet.MissionTrace("Fleet " + (object)fleetID + " sent on Evacuation mission to system " + (object)systemID);
			return missionID;
		}

		public static int SetRelocationMission(
		  GameSession game,
		  int fleetID,
		  int systemId,
		  bool useDirectRoute,
		  List<int> designIDs)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			if (designIDs != null)
				designIDs.AddRange((IEnumerable<int>)Kerberos.Sots.StarFleet.StarFleet.GetFactionRequiredDesignsForFleet(game, fleetID, systemId));
			else
				designIDs = Kerberos.Sots.StarFleet.StarFleet.GetFactionRequiredDesignsForFleet(game, fleetID, systemId);
			int missionID = gameDatabase.InsertMission(fleetID, MissionType.RELOCATION, systemId, 0, 0, 1, useDirectRoute, new int?());
			if (designIDs != null && designIDs.Count<int>() > 0)
				Kerberos.Sots.StarFleet.StarFleet.AddShipsForMission(gameDatabase, fleetID, missionID, (IList<int>)designIDs);
			Kerberos.Sots.StarFleet.StarFleet.SetWaypointsForMission(game, MissionType.RELOCATION, missionID, fleetID, systemId, 0, new int?());
			Kerberos.Sots.StarFleet.StarFleet.MissionTrace("Fleet " + (object)fleetID + " sent on Transfer mission to system: " + gameDatabase.GetStarSystemInfo(systemId).Name);
			return missionID;
		}

		public static int SetPatrolMission(
		  GameSession game,
		  int fleetID,
		  int systemId,
		  bool useDirectRoute,
		  List<int> designIDs,
		  int? ReBaseTarget = null)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			if (designIDs != null)
				designIDs.AddRange((IEnumerable<int>)Kerberos.Sots.StarFleet.StarFleet.GetFactionRequiredDesignsForFleet(game, fleetID, systemId));
			else
				designIDs = Kerberos.Sots.StarFleet.StarFleet.GetFactionRequiredDesignsForFleet(game, fleetID, systemId);
			int missionID = gameDatabase.InsertMission(fleetID, MissionType.PATROL, systemId, 0, 0, 1, useDirectRoute, new int?());
			if (designIDs != null && designIDs.Count<int>() > 0)
				Kerberos.Sots.StarFleet.StarFleet.AddShipsForMission(gameDatabase, fleetID, missionID, (IList<int>)designIDs);
			Kerberos.Sots.StarFleet.StarFleet.SetWaypointsForMission(game, MissionType.PATROL, missionID, fleetID, systemId, 0, ReBaseTarget);
			Kerberos.Sots.StarFleet.StarFleet.MissionTrace("Fleet " + (object)fleetID + " sent on Patrol mission to system: " + (object)systemId);
			return missionID;
		}

		public static int SetNPGMission(
		  GameSession game,
		  int fleetId,
		  int systemId,
		  bool useDirectRoute,
		  List<int> gatepoints,
		  List<int> designIds,
		  int? ReBaseTarget = null)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			gatepoints.Sort();
			if (designIds != null)
				designIds.AddRange((IEnumerable<int>)Kerberos.Sots.StarFleet.StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId));
			else
				designIds = Kerberos.Sots.StarFleet.StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId);
			int missionID = gameDatabase.InsertMission(fleetId, MissionType.DEPLOY_NPG, systemId, 0, 0, 1, useDirectRoute, new int?());
			if (designIds != null && designIds.Count<int>() > 0)
				Kerberos.Sots.StarFleet.StarFleet.AddShipsForMission(gameDatabase, fleetId, missionID, (IList<int>)designIds);
			Kerberos.Sots.StarFleet.StarFleet.SetWaypointsForMission(game, MissionType.DEPLOY_NPG, missionID, fleetId, systemId, 0, ReBaseTarget);
			Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(game, fleetId);
			StarSystemInfo starSystemInfo1 = game.GameDatabase.GetStarSystemInfo(game.GameDatabase.GetFleetInfo(fleetId).SystemID);
			StarSystemInfo starSystemInfo2 = game.GameDatabase.GetStarSystemInfo(systemId);
			if (gatepoints.Any<int>())
			{
				foreach (int gatepoint1 in gatepoints)
				{
					int gatepoint = gatepoint1;
					Vector3 vector3 = starSystemInfo2.Origin - starSystemInfo1.Origin;
					Vector3 toCoords = vector3 * ((float)gatepoint / 100f) + starSystemInfo1.Origin;
					Vector3 fromCoords = gatepoint == gatepoints.First<int>() ? starSystemInfo1.Origin : vector3 * ((float)gatepoints[gatepoints.FindIndex((Predicate<int>)(x => x == gatepoint)) - 1] / 100f) + starSystemInfo1.Origin;
					game.GameDatabase.InsertMoveOrder(fleetId, gatepoint == gatepoints.First<int>() ? game.GameDatabase.GetFleetInfo(fleetId).SystemID : 0, fromCoords, gatepoint == gatepoints.Last<int>() ? 0 : 0, toCoords);
				}
				Vector3 fromCoords1 = (starSystemInfo2.Origin - starSystemInfo1.Origin) * ((float)gatepoints.Last<int>() / 100f) + starSystemInfo1.Origin;
				game.GameDatabase.InsertMoveOrder(fleetId, 0, fromCoords1, systemId, Vector3.Zero);
			}
			Kerberos.Sots.StarFleet.StarFleet.MissionTrace("Fleet " + (object)fleetId + " sent on NPG mission to system: " + (object)systemId);
			return missionID;
		}

		public static IEnumerable<Vector3> GetAccelGateSlotsBetweenSystems(
		  GameDatabase db,
		  int systemida,
		  int systemidb)
		{
			StarSystemInfo starSystemInfo1 = db.GetStarSystemInfo(systemida);
			StarSystemInfo starSystemInfo2 = db.GetStarSystemInfo(systemidb);
			List<Vector3> vector3List = new List<Vector3>();
			float length = (starSystemInfo1.Origin - starSystemInfo2.Origin).Length;
			int num = (int)Math.Floor((double)length / (double)db.AssetDatabase.LoaDistanceBetweenGates);
			for (int index = 0; index < num; ++index)
			{
				Vector3 vector3 = (starSystemInfo2.Origin - starSystemInfo1.Origin) * (db.AssetDatabase.LoaDistanceBetweenGates * (float)(index + 1) / length) + starSystemInfo1.Origin;
				if ((double)(vector3 - starSystemInfo2.Origin).Length >= (double)db.AssetDatabase.LoaGateSystemMargin)
					vector3List.Add(vector3);
			}
			return (IEnumerable<Vector3>)vector3List;
		}

		public static IEnumerable<int> GetAccelGatePercentPointsBetweenSystems(
		  GameDatabase db,
		  int systemida,
		  int systemidb)
		{
			StarSystemInfo starSystemInfo1 = db.GetStarSystemInfo(systemida);
			StarSystemInfo starSystemInfo2 = db.GetStarSystemInfo(systemidb);
			float length = (starSystemInfo1.Origin - starSystemInfo2.Origin).Length;
			List<int> intList = new List<int>();
			List<Vector3> list = Kerberos.Sots.StarFleet.StarFleet.GetAccelGateSlotsBetweenSystems(db, starSystemInfo1.ID, starSystemInfo2.ID).ToList<Vector3>();
			int count = list.Count;
			for (int index = 0; index < count; ++index)
				intList.Add((int)((double)(starSystemInfo1.Origin - list[index]).Length / (double)length * 100.0));
			return (IEnumerable<int>)intList;
		}

		public static int GetMaxLoaFleetCubeMassForTransit(GameSession game, int playerid)
		{
			if (game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerFactionID(playerid)).Name != "loa")
				return 0;
			int num = game.AssetDatabase.LoaBaseMaxMass;
			if (game.GameDatabase.GetStratModifier<bool>(StratModifiers.StandingNeutrinoWaves, playerid))
				num = game.AssetDatabase.LoaMassStandingPulseWavesMaxMass;
			if (game.GameDatabase.GetStratModifier<bool>(StratModifiers.MassInductionProjectors, playerid))
				num = game.AssetDatabase.LoaMassInductionProjectorsMaxMass;
			return num;
		}

		public static int SetGateMission(
		  GameSession game,
		  int fleetId,
		  int systemId,
		  bool useDirectRoute,
		  List<int> designIds,
		  int? ReBaseTarget = null)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			if (designIds != null)
				designIds.AddRange((IEnumerable<int>)Kerberos.Sots.StarFleet.StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId));
			else
				designIds = Kerberos.Sots.StarFleet.StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId);
			int missionID = gameDatabase.InsertMission(fleetId, MissionType.GATE, systemId, 0, 0, 1, useDirectRoute, new int?());
			if (designIds != null && designIds.Count<int>() > 0)
				Kerberos.Sots.StarFleet.StarFleet.AddShipsForMission(gameDatabase, fleetId, missionID, (IList<int>)designIds);
			Kerberos.Sots.StarFleet.StarFleet.SetWaypointsForMission(game, MissionType.GATE, missionID, fleetId, systemId, 0, ReBaseTarget);
			Kerberos.Sots.StarFleet.StarFleet.MissionTrace("Fleet " + (object)fleetId + " sent on Gate mission to system: " + (object)systemId);
			return missionID;
		}

		public static int SetSurveyMission(
		  GameSession game,
		  int fleetId,
		  int systemId,
		  bool useDirectRoute,
		  List<int> designIds,
		  int? ReBaseTarget = null)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			if (designIds != null)
				designIds.AddRange((IEnumerable<int>)Kerberos.Sots.StarFleet.StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId));
			else
				designIds = Kerberos.Sots.StarFleet.StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId);
			gameDatabase.GetFleetInfo(fleetId);
			int requiredForSystem = Kerberos.Sots.StarFleet.StarFleet.GetSurveyPointsRequiredForSystem(gameDatabase, systemId);
			int missionID = gameDatabase.InsertMission(fleetId, MissionType.SURVEY, systemId, 0, 0, requiredForSystem, useDirectRoute, new int?());
			if (designIds != null && designIds.Count<int>() > 0)
				Kerberos.Sots.StarFleet.StarFleet.AddShipsForMission(gameDatabase, fleetId, missionID, (IList<int>)designIds);
			Kerberos.Sots.StarFleet.StarFleet.SetWaypointsForMission(game, MissionType.SURVEY, missionID, fleetId, systemId, 0, ReBaseTarget);
			Kerberos.Sots.StarFleet.StarFleet.MissionTrace("Fleet " + (object)fleetId + " sent on Survey mission to system " + (object)systemId);
			return missionID;
		}

		public static int SetInterdictionMission(
		  GameSession game,
		  int fleetId,
		  int systemId,
		  bool useDirectRoute,
		  int duration,
		  List<int> designIds)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			if (designIds != null)
				designIds.AddRange((IEnumerable<int>)Kerberos.Sots.StarFleet.StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId));
			else
				designIds = Kerberos.Sots.StarFleet.StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId);
			int missionID = gameDatabase.InsertMission(fleetId, MissionType.INTERDICTION, systemId, 0, 0, duration, useDirectRoute, new int?());
			if (designIds != null && designIds.Count<int>() > 0)
				Kerberos.Sots.StarFleet.StarFleet.AddShipsForMission(gameDatabase, fleetId, missionID, (IList<int>)designIds);
			Kerberos.Sots.StarFleet.StarFleet.SetWaypointsForMission(game, MissionType.INTERDICTION, missionID, fleetId, systemId, 0, new int?());
			Kerberos.Sots.StarFleet.StarFleet.MissionTrace("Fleet " + (object)fleetId + " sent on Interdiction mission to system " + (object)systemId);
			return missionID;
		}

		public static int SetStrikeMission(
		  GameSession game,
		  int fleetId,
		  int systemId,
		  bool useDirectRoute,
		  int orbitalObjectId,
		  int targetFleetId,
		  List<int> designIds)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			if (designIds != null)
				designIds.AddRange((IEnumerable<int>)Kerberos.Sots.StarFleet.StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId));
			else
				designIds = Kerberos.Sots.StarFleet.StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId);
			int missionID = gameDatabase.InsertMission(fleetId, MissionType.STRIKE, systemId, orbitalObjectId, targetFleetId, 1, useDirectRoute, new int?());
			if (designIds != null && designIds.Count<int>() > 0)
				Kerberos.Sots.StarFleet.StarFleet.AddShipsForMission(gameDatabase, fleetId, missionID, (IList<int>)designIds);
			Kerberos.Sots.StarFleet.StarFleet.SetWaypointsForMission(game, MissionType.STRIKE, missionID, fleetId, systemId, 0, new int?());
			Kerberos.Sots.StarFleet.StarFleet.MissionTrace("Fleet " + (object)fleetId + " sent on Strike mission to system " + (object)systemId);
			return missionID;
		}

		public static int SetPiracyMission(
		  GameSession game,
		  int fleetId,
		  int systemId,
		  bool useDirectRoute,
		  int orbitalObjectId,
		  int targetFleetId,
		  List<int> designIds)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			if (designIds != null)
				designIds.AddRange((IEnumerable<int>)Kerberos.Sots.StarFleet.StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId));
			else
				designIds = Kerberos.Sots.StarFleet.StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId);
			int missionID = gameDatabase.InsertMission(fleetId, MissionType.PIRACY, systemId, orbitalObjectId, targetFleetId, 1, useDirectRoute, new int?());
			if (designIds != null && designIds.Count<int>() > 0)
				Kerberos.Sots.StarFleet.StarFleet.AddShipsForMission(gameDatabase, fleetId, missionID, (IList<int>)designIds);
			Kerberos.Sots.StarFleet.StarFleet.SetWaypointsForMission(game, MissionType.PIRACY, missionID, fleetId, systemId, 0, new int?());
			Kerberos.Sots.StarFleet.StarFleet.MissionTrace("Fleet " + (object)fleetId + " sent on Piracy mission to system " + (object)systemId);
			return missionID;
		}

		public static int SetFleetInterceptMission(
		  GameSession game,
		  int fleetId,
		  int targetFleet,
		  bool useDirectRoute,
		  List<int> designIds)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			int missionID = gameDatabase.InsertMission(fleetId, MissionType.INTERCEPT, 0, 0, targetFleet, 6, useDirectRoute, new int?());
			if (designIds != null && designIds.Count<int>() > 0)
				Kerberos.Sots.StarFleet.StarFleet.AddShipsForMission(gameDatabase, fleetId, missionID, (IList<int>)designIds);
			Kerberos.Sots.StarFleet.StarFleet.SetWaypointsForMission(game, MissionType.INTERCEPT, missionID, fleetId, targetFleet, 0, new int?());
			Kerberos.Sots.StarFleet.StarFleet.MissionTrace("Fleet " + (object)fleetId + " sent on Intercept mission to fleet " + (object)targetFleet);
			return missionID;
		}

		public static int SetInvasionMission(
		  GameSession game,
		  int fleetId,
		  int systemId,
		  bool useDirectRoute,
		  int orbitalObjectId,
		  List<int> designIds)
		{
			if (designIds != null)
				designIds.AddRange((IEnumerable<int>)Kerberos.Sots.StarFleet.StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId));
			else
				designIds = Kerberos.Sots.StarFleet.StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId);
			int missionID = game.GameDatabase.InsertMission(fleetId, MissionType.INVASION, systemId, orbitalObjectId, 0, 6, useDirectRoute, new int?());
			Kerberos.Sots.StarFleet.StarFleet.SetWaypointsForMission(game, MissionType.INVASION, missionID, fleetId, systemId, 0, new int?());
			if (designIds != null && designIds.Count<int>() > 0)
				Kerberos.Sots.StarFleet.StarFleet.AddShipsForMission(game.GameDatabase, fleetId, missionID, (IList<int>)designIds);
			Kerberos.Sots.StarFleet.StarFleet.MissionTrace("Fleet " + (object)fleetId + " sent on Invasion mission to system " + (object)systemId);
			return missionID;
		}

		public static int SetSupportMission(
		  GameSession game,
		  int fleetId,
		  int systemId,
		  bool useDirectRoute,
		  int orbitalObjectId,
		  List<int> designIds,
		  int numTrips,
		  int? ReBaseTarget = null)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			if (designIds != null)
				designIds.AddRange((IEnumerable<int>)Kerberos.Sots.StarFleet.StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId));
			else
				designIds = Kerberos.Sots.StarFleet.StarFleet.GetFactionRequiredDesignsForFleet(game, fleetId, systemId);
			int missionID = gameDatabase.InsertMission(fleetId, MissionType.SUPPORT, systemId, orbitalObjectId, 0, 0, useDirectRoute, new int?());
			if (designIds != null && designIds.Count<int>() > 0)
				Kerberos.Sots.StarFleet.StarFleet.AddShipsForMission(gameDatabase, fleetId, missionID, (IList<int>)designIds);
			Kerberos.Sots.StarFleet.StarFleet.SetWaypointsForMission(game, MissionType.SUPPORT, missionID, fleetId, systemId, numTrips, ReBaseTarget);
			Kerberos.Sots.StarFleet.StarFleet.MissionTrace("Fleet " + (object)fleetId + " sent on Invasion mission to system " + (object)systemId);
			return missionID;
		}

		public static int SetConstructionMission(
		  GameSession game,
		  int fleetID,
		  int systemID,
		  bool useDirectRoute,
		  int orbitalObjectID,
		  List<int> designIDs,
		  StationType type,
		  int? ReBaseTarget = null)
		{
			string factionName = game.GameDatabase.GetFactionName(game.GameDatabase.GetFleetFaction(fleetID));
			int constructionCost = Kerberos.Sots.StarFleet.StarFleet.GetStationConstructionCost(game, type, factionName, 1);
			MissionType type1 = MissionType.CONSTRUCT_STN;
			int missionID = game.GameDatabase.InsertMission(fleetID, type1, systemID, orbitalObjectID, 0, constructionCost, useDirectRoute, new int?((int)type));
			if (designIDs != null && designIDs.Count<int>() > 0)
				Kerberos.Sots.StarFleet.StarFleet.AddShipsForMission(game.GameDatabase, fleetID, missionID, (IList<int>)designIDs);
			Kerberos.Sots.StarFleet.StarFleet.SetWaypointsForMission(game, type1, missionID, fleetID, systemID, 0, ReBaseTarget);
			Kerberos.Sots.StarFleet.StarFleet.MissionTrace("Fleet " + (object)fleetID + " sent to " + type1.ToString() + " in system " + (object)systemID);
			return missionID;
		}

		public static int SetUpgradeStationMission(
		  GameSession game,
		  int fleetID,
		  int systemID,
		  bool useDirectRoute,
		  int orbitalObjectID,
		  List<int> designIDs,
		  StationType type,
		  int? ReBaseTarget = null)
		{
			StationInfo stationInfo = game.GameDatabase.GetStationInfo(orbitalObjectID);
			if (stationInfo == null)
				return 0;
			string factionName = game.GameDatabase.GetFactionName(game.GameDatabase.GetFleetFaction(fleetID));
			int constructionCost = Kerberos.Sots.StarFleet.StarFleet.GetStationConstructionCost(game, type, factionName, stationInfo.DesignInfo.StationLevel + 1);
			MissionType type1 = MissionType.CONSTRUCT_STN;
			if (stationInfo.DesignInfo.StationLevel > 0)
				type1 = MissionType.UPGRADE_STN;
			int missionID = game.GameDatabase.InsertMission(fleetID, type1, systemID, orbitalObjectID, 0, constructionCost, useDirectRoute, new int?());
			if (designIDs != null && designIDs.Count<int>() > 0)
				Kerberos.Sots.StarFleet.StarFleet.AddShipsForMission(game.GameDatabase, fleetID, missionID, (IList<int>)designIDs);
			Kerberos.Sots.StarFleet.StarFleet.SetWaypointsForMission(game, type1, missionID, fleetID, systemID, 0, ReBaseTarget);
			Kerberos.Sots.StarFleet.StarFleet.MissionTrace("Fleet " + (object)fleetID + " sent to " + type1.ToString() + " in system " + (object)systemID);
			return missionID;
		}

		public static int SetSpecialConstructionMission(
		  GameSession game,
		  int fleetID,
		  int targetFleetID,
		  bool useDirectRoute,
		  List<int> designIDs,
		  StationType type)
		{
			string factionName = game.GameDatabase.GetFactionName(game.GameDatabase.GetFleetFaction(fleetID));
			int constructionCost = Kerberos.Sots.StarFleet.StarFleet.GetStationConstructionCost(game, type, factionName, 1);
			MissionType type1 = MissionType.SPECIAL_CONSTRUCT_STN;
			int missionID = game.GameDatabase.InsertMission(fleetID, type1, 0, 0, targetFleetID, constructionCost, useDirectRoute, new int?((int)type));
			if (designIDs != null && designIDs.Count<int>() > 0)
				Kerberos.Sots.StarFleet.StarFleet.AddShipsForMission(game.GameDatabase, fleetID, missionID, (IList<int>)designIDs);
			Kerberos.Sots.StarFleet.StarFleet.SetWaypointsForMission(game, type1, missionID, fleetID, 0, 0, new int?());
			Kerberos.Sots.StarFleet.StarFleet.MissionTrace("Fleet " + (object)fleetID + " sent to build " + type1.ToString() + " in system of GM fleet " + (object)targetFleetID);
			return missionID;
		}

		public static void ForceReturnMission(GameDatabase game, FleetInfo fleetInfo)
		{
			int missionID = game.InsertMission(fleetInfo.ID, MissionType.RETURN, 0, 0, 0, 0, false, new int?());
			game.InsertWaypoint(missionID, WaypointType.ReturnHome, new int?());
		}

		public static void CancelMission(GameSession game, FleetInfo fleetInfo, bool removeStation = true)
		{
			if (fleetInfo == null)
				return;
			MoveOrderInfo orderInfoByFleetId = game.GameDatabase.GetMoveOrderInfoByFleetID(fleetInfo.ID);
			MissionInfo missionByFleetId = game.GameDatabase.GetMissionByFleetID(fleetInfo.ID);
			if (missionByFleetId == null || missionByFleetId.Type == MissionType.RETREAT || missionByFleetId.Type == MissionType.RETURN)
				return;
			bool flag = missionByFleetId.Type == MissionType.CONSTRUCT_STN && removeStation;
			int targetOrbitalObjectId = missionByFleetId.TargetOrbitalObjectID;
			if (orderInfoByFleetId == null && fleetInfo.SystemID == fleetInfo.SupportingSystemID)
			{
				game.GameDatabase.RemoveMission(missionByFleetId.ID);
			}
			else
			{
				if (orderInfoByFleetId != null && orderInfoByFleetId.FromSystemID != 0 && orderInfoByFleetId.ToSystemID != 0)
				{
					Faction faction = game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerFactionID(fleetInfo.PlayerID));
					int tripTime;
					float tripDistance;
					if (((game.GameDatabase.GetNodeLineBetweenSystems(fleetInfo.PlayerID, orderInfoByFleetId.FromSystemID, orderInfoByFleetId.ToSystemID, true, false) == null || !faction.CanUseNodeLine(new bool?(true))) && (game.GameDatabase.GetNodeLineBetweenSystems(fleetInfo.PlayerID, orderInfoByFleetId.FromSystemID, orderInfoByFleetId.ToSystemID, false, false) == null || !faction.CanUseNodeLine(new bool?(false))) || faction.Name == "loa") && (Kerberos.Sots.StarFleet.StarFleet.GetBestTravelPath(game, fleetInfo.ID, fleetInfo.SystemID, game.GameDatabase.GetHomeSystem(game, missionByFleetId.ID, fleetInfo), out tripTime, out tripDistance, missionByFleetId.UseDirectRoute, new float?(), new float?())[1] == orderInfoByFleetId.FromSystemID || faction.CanUseGate() && game.GameDatabase.SystemHasGate(orderInfoByFleetId.FromSystemID, fleetInfo.PlayerID) && (!game.GameDatabase.SystemHasGate(orderInfoByFleetId.ToSystemID, fleetInfo.PlayerID) && !game.GameDatabase.SystemHasGate(fleetInfo.SystemID, fleetInfo.PlayerID))))
					{
						game.GameDatabase.InsertMoveOrder(orderInfoByFleetId.FleetID, 0, game.GameDatabase.GetFleetLocation(fleetInfo.ID, false).Coords, orderInfoByFleetId.FromSystemID, Vector3.Zero);
						game.GameDatabase.RemoveMoveOrder(orderInfoByFleetId.ID);
					}
				}
				missionByFleetId.Type = MissionType.RETURN;
				missionByFleetId.TargetSystemID = 0;
				missionByFleetId.TargetOrbitalObjectID = 0;
				missionByFleetId.TargetFleetID = 0;
				game.GameDatabase.UpdateMission(missionByFleetId);
				game.GameDatabase.ClearWaypoints(missionByFleetId.ID);
				game.GameDatabase.InsertWaypoint(missionByFleetId.ID, WaypointType.ReturnHome, new int?());
			}
			if (!flag)
				return;
			StationInfo stationInfo = game.GameDatabase.GetStationInfo(targetOrbitalObjectId);
			if (stationInfo == null || stationInfo.DesignInfo.StationLevel != 0)
				return;
			game.GameDatabase.DestroyStation(game, targetOrbitalObjectId, missionByFleetId.ID);
		}

		public static ShipSectionAsset GetStationAsset(
		  GameSession game,
		  StationType type,
		  string faction,
		  int level)
		{
			return game.AssetDatabase.ShipSections.Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
		   {
			   if (x.Class == ShipClass.Station)
				   return x.Faction == faction;
			   return false;
		   })).ToList<ShipSectionAsset>().FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
	 {
		 if (x.StationType == type)
			 return x.StationLevel == level;
		 return false;
	 }));
		}

		public static int GetStationSavingsCost(
		  GameSession game,
		  StationType type,
		  string faction,
		  int level)
		{
			ShipSectionAsset stationAsset = Kerberos.Sots.StarFleet.StarFleet.GetStationAsset(game, type, faction, level);
			if (stationAsset != null)
				return stationAsset.SavingsCost;
			return 0;
		}

		public static int GetStationConstructionCost(
		  GameSession game,
		  StationType type,
		  string faction,
		  int level)
		{
			ShipSectionAsset stationAsset = Kerberos.Sots.StarFleet.StarFleet.GetStationAsset(game, type, faction, level);
			if (stationAsset != null)
				return stationAsset.ProductionCost;
			return 0;
		}

		public static void ConfigureFleetForMission(GameSession game, int fleetID)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			FleetInfo fleetInfo = gameDatabase.GetFleetInfo(fleetID);
			if (fleetInfo == null || fleetInfo.IsReserveFleet || fleetInfo.IsDefenseFleet)
				return;
			FleetInfo reserveFleetInfo = gameDatabase.InsertOrGetReserveFleetInfo(fleetInfo.SupportingSystemID, fleetInfo.PlayerID);
			MissionInfo missionByFleetId = gameDatabase.GetMissionByFleetID(fleetID);
			if (missionByFleetId == null || fleetInfo.SystemID != fleetInfo.SupportingSystemID)
				return;
			List<ShipInfo> list = gameDatabase.GetShipInfoByFleetID(fleetInfo.ID, true).ToList<ShipInfo>();
			ShipRole[] shipRoleArray = Kerberos.Sots.StarFleet.StarFleet.CollectShipRolesForMission(missionByFleetId.Type);
			int num1 = 0;
			foreach (ShipInfo shipInfo in list)
			{
				if (shipInfo.DesignInfo.Role == ShipRole.COMMAND)
				{
					int commandPointQuota = gameDatabase.GetDesignCommandPointQuota(gameDatabase.AssetDatabase, shipInfo.DesignID);
					if (commandPointQuota > num1)
						num1 = commandPointQuota;
				}
			}
			int num2 = num1 - gameDatabase.GetFleetCommandPointCost(fleetInfo.ID);
			if (num2 <= 0)
				return;
			foreach (ShipInfo shipInfo in gameDatabase.GetShipInfoByFleetID(reserveFleetInfo.ID, true).ToArray<ShipInfo>())
			{
				if (((IEnumerable<ShipRole>)shipRoleArray).Contains<ShipRole>(shipInfo.DesignInfo.Role))
				{
					int commandPointCost = gameDatabase.GetCommandPointCost(shipInfo.DesignID);
					if (commandPointCost < num2)
					{
						gameDatabase.TransferShip(shipInfo.ID, fleetInfo.ID);
						num2 -= commandPointCost;
					}
				}
			}
		}

		private static ShipRole[] CollectShipRolesForMission(MissionType mission)
		{
			ShipRole[] shipRoleArray;
			switch (mission)
			{
				case MissionType.COLONIZATION:
				case MissionType.SUPPORT:
				case MissionType.EVACUATE:
					shipRoleArray = new ShipRole[4]
					{
			ShipRole.COMMAND,
			ShipRole.SUPPLY,
			ShipRole.COLONIZER,
			ShipRole.COMBAT
					};
					break;
				case MissionType.SURVEY:
				case MissionType.PATROL:
				case MissionType.STRIKE:
				case MissionType.INVASION:
				case MissionType.PIRACY:
					shipRoleArray = new ShipRole[3]
					{
			ShipRole.COMMAND,
			ShipRole.SUPPLY,
			ShipRole.COMBAT
					};
					break;
				case MissionType.CONSTRUCT_STN:
				case MissionType.SPECIAL_CONSTRUCT_STN:
					shipRoleArray = new ShipRole[4]
					{
			ShipRole.COMMAND,
			ShipRole.SUPPLY,
			ShipRole.CONSTRUCTOR,
			ShipRole.COMBAT
					};
					break;
				case MissionType.GATE:
					shipRoleArray = new ShipRole[4]
					{
			ShipRole.COMMAND,
			ShipRole.SUPPLY,
			ShipRole.GATE,
			ShipRole.COMBAT
					};
					break;
				default:
					shipRoleArray = new ShipRole[3]
					{
			ShipRole.COMMAND,
			ShipRole.SUPPLY,
			ShipRole.COMBAT
					};
					break;
			}
			return shipRoleArray;
		}

		private static void AddShipsForMission(
		  GameDatabase db,
		  int fleetID,
		  int missionID,
		  IList<int> designIDs)
		{
			FleetInfo fleetInfo = db.GetFleetInfo(fleetID);
			db.InsertBuildOrders(fleetInfo.SystemID, (IEnumerable<int>)designIDs, 1, missionID, new int?(), new int?());
		}

		public static float GetSensorTravelDistance(
		  GameSession game,
		  int startId,
		  int endId,
		  int fleetId)
		{
			if (startId == 0 || endId == 0)
				return float.MaxValue;
			float tripDistance = 0.0f;
			int tripTime;
			List<int> bestTravelPath = Kerberos.Sots.StarFleet.StarFleet.GetBestTravelPath(game, fleetId, startId, endId, out tripTime, out tripDistance, false, new float?(), new float?());
			tripDistance = 0.0f;
			for (int index = 0; index < bestTravelPath.Count - 1; ++index)
				tripDistance += (game.GameDatabase.GetStarSystemOrigin(bestTravelPath[index]) - game.GameDatabase.GetStarSystemOrigin(bestTravelPath[index + 1])).Length;
			return tripDistance;
		}

		public static bool DoAutoPatrol(GameSession game, FleetInfo fleet, MissionInfo currentMission)
		{
			App app = game.App;
			switch (currentMission.Type)
			{
				case MissionType.COLONIZATION:
				case MissionType.SUPPORT:
				case MissionType.RELOCATION:
				case MissionType.PATROL:
				case MissionType.INTERCEPT:
				case MissionType.RETURN:
					return false;
				default:
					if (currentMission == null || currentMission.TargetSystemID == 0 || Kerberos.Sots.StarFleet.StarFleet.IsFleetExhausted(game, fleet) || game.GameDatabase.GetWaypointsByMissionID(currentMission.ID).Any<WaypointInfo>((Func<WaypointInfo, bool>)(x => x.Type == WaypointType.ReBase)))
						return false;
					app.GameDatabase.ClearWaypoints(currentMission.ID);
					app.GameDatabase.RemoveMission(currentMission.ID);
					Kerberos.Sots.StarFleet.StarFleet.SetPatrolMission(game, currentMission.FleetID, currentMission.TargetSystemID, false, (List<int>)null, new int?());
					return true;
			}
		}

		public static bool SetReturnMission(
		  GameSession game,
		  FleetInfo fleet,
		  MissionInfo currentMission)
		{
			switch (currentMission.Type)
			{
				case MissionType.COLONIZATION:
				case MissionType.SUPPORT:
				case MissionType.RELOCATION:
				case MissionType.INTERCEPT:
				case MissionType.RETURN:
					return false;
				default:
					if (game.GameDatabase.GetWaypointsByMissionID(currentMission.ID).Any<WaypointInfo>((Func<WaypointInfo, bool>)(x => x.Type == WaypointType.ReBase)) || fleet.SupportingSystemID == fleet.SystemID)
						return false;
					currentMission.Type = MissionType.RETURN;
					currentMission.TargetSystemID = 0;
					currentMission.TargetOrbitalObjectID = 0;
					currentMission.TargetFleetID = 0;
					game.GameDatabase.UpdateMission(currentMission);
					game.GameDatabase.ClearWaypoints(currentMission.ID);
					game.GameDatabase.InsertWaypoint(currentMission.ID, WaypointType.ReturnHome, new int?());
					return true;
			}
		}

		public static void SetWaypointsForMission(
		  GameSession game,
		  MissionType type,
		  int missionID,
		  int fleetID,
		  int systemID,
		  int numTrips = 0,
		  int? ReBaseTarget = null)
		{
			GameDatabase gameDatabase = game.GameDatabase;
			WaypointType type1 = WaypointType.ReturnHome;
			if (ReBaseTarget.HasValue)
				type1 = WaypointType.ReBase;
			switch (type)
			{
				case MissionType.COLONIZATION:
					gameDatabase.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(systemID));
					gameDatabase.InsertWaypoint(missionID, WaypointType.DoMission, new int?());
					gameDatabase.InsertWaypoint(missionID, type1, new int?());
					gameDatabase.InsertWaypoint(missionID, WaypointType.CheckSupportColony, new int?());
					if (!ReBaseTarget.HasValue)
						break;
					gameDatabase.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(ReBaseTarget.Value));
					gameDatabase.InsertWaypoint(missionID, type1, new int?());
					break;
				case MissionType.SUPPORT:
					for (int index = 0; index < numTrips; ++index)
					{
						if (gameDatabase.GetFleetInfo(fleetID).SupportingSystemID != systemID)
							gameDatabase.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(systemID));
						gameDatabase.InsertWaypoint(missionID, WaypointType.DoMission, new int?());
						gameDatabase.InsertWaypoint(missionID, type1, new int?());
					}
					gameDatabase.InsertWaypoint(missionID, WaypointType.CheckSupportColony, new int?());
					if (!ReBaseTarget.HasValue)
						break;
					gameDatabase.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(ReBaseTarget.Value));
					gameDatabase.InsertWaypoint(missionID, type1, new int?());
					break;
				case MissionType.SURVEY:
				case MissionType.CONSTRUCT_STN:
				case MissionType.UPGRADE_STN:
				case MissionType.PATROL:
				case MissionType.INTERDICTION:
				case MissionType.STRIKE:
				case MissionType.INVASION:
				case MissionType.GATE:
				case MissionType.PIRACY:
					gameDatabase.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(systemID));
					gameDatabase.InsertWaypoint(missionID, WaypointType.DoMission, new int?());
					if (ReBaseTarget.HasValue)
						gameDatabase.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(ReBaseTarget.Value));
					gameDatabase.InsertWaypoint(missionID, type1, new int?());
					break;
				case MissionType.RELOCATION:
					gameDatabase.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(systemID));
					gameDatabase.InsertWaypoint(missionID, WaypointType.DoMission, new int?());
					break;
				case MissionType.INTERCEPT:
					gameDatabase.InsertWaypoint(missionID, WaypointType.DoMission, new int?());
					gameDatabase.InsertWaypoint(missionID, type1, new int?());
					break;
				case MissionType.DEPLOY_NPG:
					gameDatabase.InsertWaypoint(missionID, WaypointType.DoMission, new int?(systemID));
					gameDatabase.InsertWaypoint(missionID, type1, new int?());
					break;
				case MissionType.EVACUATE:
					gameDatabase.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(systemID));
					gameDatabase.InsertWaypoint(missionID, WaypointType.DoMission, new int?());
					gameDatabase.InsertWaypoint(missionID, type1, new int?());
					gameDatabase.InsertWaypoint(missionID, WaypointType.CheckEvacuate, new int?());
					break;
				case MissionType.SPECIAL_CONSTRUCT_STN:
					gameDatabase.InsertWaypoint(missionID, WaypointType.DoMission, new int?());
					gameDatabase.InsertWaypoint(missionID, type1, new int?());
					break;
			}
		}

		public static int GetTurnsToSurveySystem(
		  GameDatabase db,
		  int systemID,
		  IEnumerable<ShipInfo> ships)
		{
			int fleetSurveyPoints = Kerberos.Sots.StarFleet.StarFleet.GetFleetSurveyPoints(db, ships);
			if (fleetSurveyPoints < 1)
				return -1;
			return Math.Max(Kerberos.Sots.StarFleet.StarFleet.GetSurveyPointsRequiredForSystem(db, systemID) / fleetSurveyPoints, 1);
		}

		public static int GetTurnsToSurveySystem(GameDatabase db, int systemID, int fleetID)
		{
			return Kerberos.Sots.StarFleet.StarFleet.GetTurnsToSurveySystem(db, systemID, db.GetShipInfoByFleetID(fleetID, true));
		}

		public static int GetFleetSurveyPoints(GameDatabase db, IEnumerable<ShipInfo> ships)
		{
			List<int> intList = new List<int>();
			int num = 0;
			foreach (ShipInfo ship in ships)
			{
				if (ship.DesignInfo.Class == ShipClass.BattleRider)
				{
					if (!intList.Contains(ship.ParentID))
						intList.Add(ship.ParentID);
					else
						continue;
				}
				bool flag = db.GetDesignAttributesForDesign(ship.DesignID).Contains<SectionEnumerations.DesignAttribute>(SectionEnumerations.DesignAttribute.Louis_And_Clark);
				num += flag ? 4 : 2;
				foreach (string designSectionName in db.GetDesignSectionNames(ship.DesignID))
				{
					if (designSectionName.Contains("deepscan"))
					{
						num += flag ? 16 : 8;
						break;
					}
				}
			}
			if (ships.Count<ShipInfo>() > 0)
			{
				List<AdmiralInfo.TraitType> list = db.GetAdmiralTraits(db.GetFleetInfo(ships.First<ShipInfo>().FleetID).AdmiralID).ToList<AdmiralInfo.TraitType>();
				if (list.Contains(AdmiralInfo.TraitType.Pathfinder))
					num = (int)Math.Floor((double)num * 1.25);
				else if (list.Contains(AdmiralInfo.TraitType.Livingstone))
					num = (int)Math.Floor((double)num * 0.75);
			}
			return num;
		}

		public static int GetFleetSurveyPoints(GameDatabase db, int fleetID)
		{
			return Kerberos.Sots.StarFleet.StarFleet.GetFleetSurveyPoints(db, db.GetShipInfoByFleetID(fleetID, true));
		}

		public static int GetSurveyPointsRequiredForSystem(GameDatabase db, int systemID)
		{
			int num = 0;
			foreach (PlanetInfo systemPlanetInfo in db.GetStarSystemPlanetInfos(systemID))
			{
				if (db.GetOrbitalObjectInfo(systemPlanetInfo.ID).ParentID.HasValue)
					num += 5;
				else if (systemPlanetInfo.Type == "gaseous")
					num += 15;
				else
					num += 10;
			}
			foreach (AsteroidBeltInfo asteroidBeltInfo in db.GetStarSystemAsteroidBeltInfos(systemID))
				num += 20;
			return num;
		}

		public static List<int> GetBestTravelPath(
		  GameSession game,
		  int fleetId,
		  int fromSystem,
		  int toSystem,
		  out int tripTime,
		  out float tripDistance,
		  bool useDirectRoute,
		  float? travelSpeed = null,
		  float? nodeTravelSpeed = null)
		{
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetId);
			Faction faction = game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerFactionID(fleetInfo.PlayerID));
			int tripTime1 = int.MaxValue;
			float tripDistance1 = float.MaxValue;
			tripDistance = float.MaxValue;
			tripTime = int.MaxValue;
			List<int> intList;
			if (faction.CanUseNodeLine(new bool?(false)) && !useDirectRoute && (fromSystem != 0 && toSystem != 0))
			{
				intList = Kerberos.Sots.StarFleet.StarFleet.GetTempNodeTravelPath(game, fleetInfo, fromSystem, toSystem, out tripTime, out tripDistance, travelSpeed, nodeTravelSpeed).ToList<int>();
				List<int> list = Kerberos.Sots.StarFleet.StarFleet.GetLinearTravelPath(game, fleetInfo, fromSystem, toSystem, out tripTime1, out tripDistance1, travelSpeed).ToList<int>();
				if (tripTime1 < tripTime)
				{
					tripTime = tripTime1;
					intList = list;
					tripDistance = tripDistance1;
				}
			}
			else
				intList = Kerberos.Sots.StarFleet.StarFleet.GetLinearTravelPath(game, fleetInfo, fromSystem, toSystem, out tripTime, out tripDistance, travelSpeed).ToList<int>();
			if (faction.CanUseNodeLine(new bool?(true)))
			{
				List<int> list = Kerberos.Sots.StarFleet.StarFleet.GetPermanentNodeTravelPath(game, fleetInfo, fromSystem, toSystem, out tripTime1, out tripDistance1, travelSpeed, nodeTravelSpeed).ToList<int>();
				if (tripTime1 < tripTime || tripTime == int.MinValue)
				{
					tripTime = tripTime1;
					intList = list;
					tripDistance = tripDistance1;
				}
			}
			if (faction.CanUseGate() && fromSystem != 0 && toSystem != 0)
			{
				List<int> list = Kerberos.Sots.StarFleet.StarFleet.GetGateTravelPath(game, fleetInfo, fromSystem, toSystem, out tripTime1, out tripDistance1, travelSpeed).ToList<int>();
				if (tripTime1 < tripTime || tripTime == int.MinValue)
				{
					tripTime = tripTime1;
					intList = list;
					tripDistance = tripDistance1;
				}
			}
			if (faction.CanUseAccelerators() && fromSystem != 0 && toSystem != 0)
			{
				List<int> list = Kerberos.Sots.StarFleet.StarFleet.GetLoaAcceleratorTravelPath(game, fleetInfo, fromSystem, toSystem, out tripTime1, out tripDistance1, travelSpeed, nodeTravelSpeed).ToList<int>();
				if (tripTime1 < tripTime)
				{
					tripTime = tripTime1;
					intList = list;
					tripDistance = tripDistance1;
				}
			}
			return intList;
		}

		public static List<int> GetLinearTravelPath(
		  GameSession game,
		  FleetInfo fleet,
		  int fromSystemID,
		  int toSystemID,
		  out int tripTime,
		  out float tripDistance,
		  float? travelSpeed = null)
		{
			float num1 = travelSpeed.HasValue ? travelSpeed.Value : Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(game, fleet.ID, false);
			MoveOrderInfo moveOrder = new MoveOrderInfo()
			{
				FleetID = fleet.ID,
				FromSystemID = fromSystemID,
				ToSystemID = toSystemID
			};
			if (fromSystemID != 0 && toSystemID != 0)
			{
				tripDistance = !game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerFactionID(fleet.PlayerID)).CanUseGravityWell() || game.GameDatabase.FleetHasCurvatureComp(fleet) ? GameSession.GetSystemToSystemDistance(game.GameDatabase, fromSystemID, toSystemID) : GameSession.GetGravityWellTravelDistance(game.GameDatabase, moveOrder);
			}
			else
			{
				Vector3 vector3_1 = moveOrder.FromSystemID == 0 ? moveOrder.FromCoords : game.GameDatabase.GetStarSystemInfo(moveOrder.FromSystemID).Origin;
				Vector3 vector3_2 = moveOrder.ToSystemID == 0 ? moveOrder.ToCoords : game.GameDatabase.GetStarSystemInfo(moveOrder.ToSystemID).Origin;
				tripDistance = (vector3_2 - vector3_1).Length;
			}
			if (game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerFactionID(fleet.PlayerID)).Name == "loa" && game.GameDatabase.GetNodeLineBetweenSystems(fleet.PlayerID, fromSystemID, toSystemID, false, true) == null)
			{
				float num2 = num1;
				float num3 = 0.0f;
				float num4 = tripDistance;
				bool flag = true;
				while ((double)num4 >= 0.0)
				{
					if (flag)
					{
						flag = false;
						num4 -= num2;
						++num3;
					}
					else
					{
						num4 -= num2;
						num2 = Math.Max(1f, num2 - 1f);
						++num3;
					}
				}
				num1 = tripDistance / num3;
			}
			tripTime = (int)Math.Ceiling((double)tripDistance / (double)num1);
			return new List<int>() { fromSystemID, toSystemID };
		}

		public static List<int> GetLoaAcceleratorTravelPath(
		  GameSession game,
		  FleetInfo fleet,
		  int fromSystemID,
		  int toSystemID,
		  out int tripTime,
		  out float tripDistance,
		  float? travelSpeed = null,
		  float? nodeTravelSpeed = null)
		{
			float num1 = nodeTravelSpeed.HasValue ? nodeTravelSpeed.Value : Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(game, fleet.ID, true);
			List<int> list = Kerberos.Sots.StarFleet.StarFleet.GetNodeTravelPath(game.GameDatabase, fromSystemID, toSystemID, fleet.PlayerID, false, false, true).ToList<int>();
			Vector3 vector3_1 = game.GameDatabase.GetStarSystemOrigin(fromSystemID);
			tripDistance = 0.0f;
			int systemA = 0;
			foreach (int systemId in list)
			{
				Vector3 starSystemOrigin = game.GameDatabase.GetStarSystemOrigin(systemId);
				Vector3 vector3_2;
				if (systemId == toSystemID && game.GameDatabase.GetNodeLineBetweenSystems(fleet.PlayerID, systemA, toSystemID, false, true) == null)
				{
					float num2 = num1;
					float num3 = tripDistance / num2;
					vector3_2 = vector3_1 - starSystemOrigin;
					float length1 = vector3_2.Length;
					bool flag = true;
					while ((double)length1 >= 0.0)
					{
						if (flag)
						{
							flag = false;
							length1 -= num2;
							++num3;
						}
						else
						{
							length1 -= num2;
							num2 = Math.Max(1f, num2 - 1f);
							++num3;
						}
					}
					ref float local = ref tripDistance;
					double num4 = (double)tripDistance;
					vector3_2 = vector3_1 - starSystemOrigin;
					double length2 = (double)vector3_2.Length;
					double num5 = num4 + length2;
					local = (float)num5;
					num1 = tripDistance / num3;
				}
				else
				{
					ref float local = ref tripDistance;
					double num2 = (double)tripDistance;
					vector3_2 = vector3_1 - starSystemOrigin;
					double length = (double)vector3_2.Length;
					double num3 = num2 + length;
					local = (float)num3;
				}
				vector3_1 = starSystemOrigin;
			}
			tripTime = (int)Math.Ceiling((double)tripDistance / (double)num1);
			if (list.Count <= 1)
				return Kerberos.Sots.StarFleet.StarFleet.GetLinearTravelPath(game, fleet, fromSystemID, toSystemID, out tripTime, out tripDistance, travelSpeed);
			return list;
		}

		public static List<int> GetTempNodeTravelPath(
		  GameSession game,
		  FleetInfo fleet,
		  int fromSystemID,
		  int toSystemID,
		  out int tripTime,
		  out float tripDistance,
		  float? travelSpeed = null,
		  float? nodeTravelSpeed = null)
		{
			float num1 = nodeTravelSpeed.HasValue ? nodeTravelSpeed.Value : Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(game, fleet.ID, true);
			List<int> list = Kerberos.Sots.StarFleet.StarFleet.GetNodeTravelPath(game.GameDatabase, fromSystemID, toSystemID, fleet.PlayerID, false, false, game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerFactionID(fleet.PlayerID)).Name == "loa").ToList<int>();
			Vector3 vector3_1 = game.GameDatabase.GetStarSystemOrigin(fromSystemID);
			tripDistance = 0.0f;
			int systemA = 0;
			foreach (int systemId in list)
			{
				Vector3 starSystemOrigin = game.GameDatabase.GetStarSystemOrigin(systemId);
				Vector3 vector3_2;
				if (game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerFactionID(fleet.PlayerID)).Name == "loa" && systemId == toSystemID && game.GameDatabase.GetNodeLineBetweenSystems(fleet.PlayerID, systemA, toSystemID, false, true) == null)
				{
					float num2 = num1;
					float num3 = tripDistance / num2;
					vector3_2 = vector3_1 - starSystemOrigin;
					float length1 = vector3_2.Length;
					bool flag = true;
					while ((double)length1 >= 0.0)
					{
						if (flag)
						{
							flag = false;
							length1 -= num2;
							++num3;
						}
						else
						{
							length1 -= num2;
							num2 = Math.Max(1f, num2 - 1f);
							++num3;
						}
					}
					ref float local = ref tripDistance;
					double num4 = (double)tripDistance;
					vector3_2 = vector3_1 - starSystemOrigin;
					double length2 = (double)vector3_2.Length;
					double num5 = num4 + length2;
					local = (float)num5;
					num1 = tripDistance / num3;
				}
				else
				{
					ref float local = ref tripDistance;
					double num2 = (double)tripDistance;
					vector3_2 = vector3_1 - starSystemOrigin;
					double length = (double)vector3_2.Length;
					double num3 = num2 + length;
					local = (float)num3;
				}
				systemA = systemId;
				vector3_1 = starSystemOrigin;
			}
			tripTime = (int)Math.Ceiling((double)tripDistance / (double)num1);
			if (list.Count <= 1)
				return Kerberos.Sots.StarFleet.StarFleet.GetLinearTravelPath(game, fleet, fromSystemID, toSystemID, out tripTime, out tripDistance, travelSpeed);
			return list;
		}

		public static List<int> GetPermanentNodeTravelPath(
		  GameSession game,
		  FleetInfo fleet,
		  int fromSystemID,
		  int toSystemID,
		  out int tripTime,
		  out float tripDistance,
		  float? travelSpeed = null,
		  float? nodeTravelSpeed = null)
		{
			float num = nodeTravelSpeed.HasValue ? nodeTravelSpeed.Value : Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(game, fleet.ID, true);
			List<int> list = Kerberos.Sots.StarFleet.StarFleet.GetNodeTravelPath(game.GameDatabase, fromSystemID, toSystemID, fleet.PlayerID, true, true, false).ToList<int>();
			Vector3 vector3 = fromSystemID != 0 ? game.GameDatabase.GetStarSystemOrigin(fromSystemID) : game.GameDatabase.GetFleetLocation(fleet.ID, false).Coords;
			tripDistance = 0.0f;
			foreach (int systemId in list)
			{
				Vector3 starSystemOrigin = game.GameDatabase.GetStarSystemOrigin(systemId);
				tripDistance += (vector3 - starSystemOrigin).Length;
				vector3 = starSystemOrigin;
			}
			tripTime = (int)Math.Ceiling((double)tripDistance / (double)num);
			if (list.Count <= 1)
				return Kerberos.Sots.StarFleet.StarFleet.GetLinearTravelPath(game, fleet, fromSystemID, toSystemID, out tripTime, out tripDistance, travelSpeed);
			return list;
		}

		public static List<int> GetGateTravelPath(
		  GameSession game,
		  FleetInfo fleet,
		  int fromSystemID,
		  int toSystemID,
		  out int tripTime,
		  out float tripDistance,
		  float? travelSpeed = null)
		{
			List<int> intList = new List<int>();
			intList.Add(fromSystemID);
			if (game.GameDatabase.SystemHasGate(fromSystemID, fleet.PlayerID))
			{
				if (game.GameDatabase.SystemHasGate(toSystemID, fleet.PlayerID))
				{
					intList.Add(toSystemID);
					tripTime = 1;
					tripDistance = 0.1f;
					return intList;
				}
				StarSystemInfo starSystemInfo1 = game.GameDatabase.GetStarSystemInfo(fromSystemID);
				StarSystemInfo starSystemInfo2 = game.GameDatabase.GetStarSystemInfo(toSystemID);
				float num1 = travelSpeed.HasValue ? travelSpeed.Value : Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(game, fleet.ID, false);
				float length = (starSystemInfo2.Origin - starSystemInfo1.Origin).Length;
				List<StarSystemInfo> closestGates = game.GetClosestGates(fleet.PlayerID, starSystemInfo2, length);
				if (closestGates.Count <= 0)
					return Kerberos.Sots.StarFleet.StarFleet.GetLinearTravelPath(game, fleet, fromSystemID, toSystemID, out tripTime, out tripDistance, new float?(num1));
				StarSystemInfo starSystemInfo3 = closestGates.First<StarSystemInfo>();
				if (starSystemInfo3.ID != fromSystemID)
					intList.Add(starSystemInfo3.ID);
				intList.Add(toSystemID);
				float num2 = Math.Max(0.0f, (starSystemInfo3.Origin - starSystemInfo2.Origin).Length - game.GameDatabase.GetStratModifier<float>(StratModifiers.GateCastDistance, fleet.PlayerID));
				tripDistance = num2;
				tripTime = Math.Max((int)Math.Ceiling((double)num2 / (double)num1), 1);
				return intList;
			}
			StarSystemInfo starSystemInfo4 = game.GameDatabase.GetStarSystemInfo(fromSystemID);
			StarSystemInfo starSystemInfo5 = game.GameDatabase.GetStarSystemInfo(toSystemID);
			float num3 = travelSpeed.HasValue ? travelSpeed.Value : Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(game, fleet.ID, false);
			float length1 = (starSystemInfo5.Origin - starSystemInfo4.Origin).Length;
			List<StarSystemInfo> closestGates1 = game.GetClosestGates(fleet.PlayerID, starSystemInfo4, length1);
			List<StarSystemInfo> closestGates2 = game.GetClosestGates(fleet.PlayerID, starSystemInfo5, length1);
			if (closestGates1.Count <= 0 || closestGates2.Count <= 0)
				return Kerberos.Sots.StarFleet.StarFleet.GetLinearTravelPath(game, fleet, fromSystemID, toSystemID, out tripTime, out tripDistance, new float?(num3));
			StarSystemInfo starSystemInfo6 = closestGates1.First<StarSystemInfo>();
			StarSystemInfo starSystemInfo7 = closestGates2.First<StarSystemInfo>();
			float num4 = (starSystemInfo4.Origin - starSystemInfo6.Origin).Length + Math.Max(0.0f, (starSystemInfo5.Origin - starSystemInfo7.Origin).Length - game.GameDatabase.GetStratModifier<float>(StratModifiers.GateCastDistance, fleet.PlayerID));
			if ((double)num4 >= (double)length1)
				return Kerberos.Sots.StarFleet.StarFleet.GetLinearTravelPath(game, fleet, fromSystemID, toSystemID, out tripTime, out tripDistance, new float?(num3));
			if (starSystemInfo6.ID != fromSystemID)
				intList.Add(starSystemInfo6.ID);
			intList.Add(starSystemInfo7.ID);
			if (starSystemInfo7.ID != starSystemInfo5.ID)
				intList.Add(starSystemInfo5.ID);
			tripDistance = num4;
			tripTime = Math.Max((int)Math.Ceiling((double)num4 / (double)num3), 1);
			return intList;
		}

		internal static IEnumerable<int> GetNodeTravelPath(
		  GameDatabase db,
		  int fromSystemID,
		  int toSystemID,
		  int playerID,
		  bool permanent,
		  bool nodeLinesOnly = false,
		  bool loalines = false)
		{
			return (IEnumerable<int>)Kerberos.Sots.StarSystemPathing.StarSystemPathing.FindClosestPath(db, playerID, fromSystemID, toSystemID, nodeLinesOnly);
		}

		public static void AddNodeToList(
		  List<Kerberos.Sots.StarFleet.StarFleet.EvaluatedNode> list,
		  Kerberos.Sots.StarFleet.StarFleet.EvaluatedNode node)
		{
			foreach (Kerberos.Sots.StarFleet.StarFleet.EvaluatedNode evaluatedNode in list)
			{
				if (evaluatedNode.SystemID == node.SystemID)
				{
					if ((double)evaluatedNode.FCost <= (double)node.FCost)
						return;
					evaluatedNode.FCost = node.FCost;
					evaluatedNode.FromNodeID = node.FromNodeID;
					return;
				}
			}
			list.Add(node);
		}

		public static List<FleetInfo> GetFleetInfosForMission(
		  GameSession game,
		  int systemId)
		{
			return game.GameDatabase.GetFleetInfosByPlayerID(game.LocalPlayer.ID, FleetType.FL_NORMAL | FleetType.FL_RESERVE).Where<FleetInfo>((Func<FleetInfo, bool>)(x => Kerberos.Sots.StarFleet.StarFleet.IsFleetAvailableForMission(game, x.ID, systemId))).ToList<FleetInfo>();
		}

		public static int GetTurnsRemainingForMissionFleet(GameSession sim, FleetInfo fleet)
		{
			MissionInfo missionByFleetId = sim.GameDatabase.GetMissionByFleetID(fleet.ID);
			int num = 0;
			if (missionByFleetId != null && fleet.PlayerID == sim.LocalPlayer.ID)
			{
				StationInfo stationInfo = sim.GameDatabase.GetStationInfo(missionByFleetId.TargetOrbitalObjectID);
				MissionEstimate missionEstimate = stationInfo == null ? Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(sim, missionByFleetId.Type, StationType.INVALID_TYPE, fleet.ID, missionByFleetId.TargetSystemID, missionByFleetId.TargetOrbitalObjectID, (List<int>)null, 1, false, new float?(), new float?()) : Kerberos.Sots.StarFleet.StarFleet.GetMissionEstimate(sim, missionByFleetId.Type, stationInfo.DesignInfo.StationType, fleet.ID, missionByFleetId.TargetSystemID, missionByFleetId.TargetOrbitalObjectID, (List<int>)null, 1, false, new float?(), new float?());
				if (missionEstimate != null)
					num = missionEstimate.TotalTurns + missionByFleetId.TurnStarted - sim.GameDatabase.GetTurnCount();
			}
			return num;
		}

		public static int? GetTravelTime(
		  GameSession game,
		  FleetInfo fleetInfo,
		  int fromSystemID,
		  int toSystemID,
		  bool restrictToPermanentNodeLines = false,
		  float? travelSpeed = null,
		  float? nodeTravelSpeed = null)
		{
			int tripTime = 0;
			float tripDistance = 0.0f;
			if (fromSystemID == 0 || toSystemID == 0)
				return new int?();
			List<int> bestTravelPath = Kerberos.Sots.StarFleet.StarFleet.GetBestTravelPath(game, fleetInfo.ID, fromSystemID, toSystemID, out tripTime, out tripDistance, false, travelSpeed, nodeTravelSpeed);
			if (restrictToPermanentNodeLines)
			{
				for (int index = 1; index < bestTravelPath.Count; ++index)
				{
					int systemA = bestTravelPath[index - 1];
					int systemB = bestTravelPath[index];
					if (game.GameDatabase.GetNodeLineBetweenSystems(fleetInfo.PlayerID, systemA, systemB, true, false) == null)
						return new int?();
				}
			}
			return new int?(tripTime);
		}

		public static int? GetTravelTime(
		  GameSession game,
		  FleetInfo fleetInfo,
		  int toSystemID,
		  bool restrictToPermanentNodeLines = false,
		  float? travelSpeed = null,
		  float? nodeTravelSpeed = null)
		{
			return Kerberos.Sots.StarFleet.StarFleet.GetTravelTime(game, fleetInfo, fleetInfo.SupportingSystemID, toSystemID, restrictToPermanentNodeLines, travelSpeed, nodeTravelSpeed);
		}

		public static MissionEstimate GetMissionEstimate(
		  GameSession sim,
		  MissionType type,
		  StationType stationType,
		  int fleetID,
		  int systemID,
		  int planetID,
		  List<int> designsToBuild = null,
		  int stationLevel = 1,
		  bool ReBase = false,
		  float? travelSpeed = null,
		  float? nodeTravelSpeed = null)
		{
			FleetInfo fleetInfo = sim.GameDatabase.GetFleetInfo(fleetID);
			if (fleetInfo == null)
				return (MissionEstimate)null;
			List<ShipInfo> list = sim.GameDatabase.GetShipInfoByFleetID(fleetID, true).ToList<ShipInfo>();
			int num1 = 0;
			double num2 = 0.0;
			if (designsToBuild != null && designsToBuild.Count<int>() > 0)
			{
				int num3 = 0;
				foreach (int designID in designsToBuild)
				{
					DesignInfo designInfo = sim.GameDatabase.GetDesignInfo(designID);
					num3 += designInfo.GetPlayerProductionCost(sim.GameDatabase, sim.LocalPlayer.ID, !designInfo.isPrototyped, new float?());
					num2 += (double)designInfo.SavingsCost;
					list.Add(new ShipInfo()
					{
						DesignID = designInfo.ID,
						DesignInfo = designInfo,
						FleetID = fleetInfo.ID,
						SerialNumber = 0,
						ShipName = string.Empty
					});
				}
				int num4 = Kerberos.Sots.StarFleet.StarFleet.PredictProductionOutputForSystem(sim, fleetInfo.SupportingSystemID, fleetInfo.PlayerID);
				num1 = num4 <= 0 ? 0 : (num3 + (num4 - 1)) / num4;
			}
			int num5 = 0;
			int? travelTime = Kerberos.Sots.StarFleet.StarFleet.GetTravelTime(sim, fleetInfo, systemID, false, travelSpeed, nodeTravelSpeed);
			if (travelTime.HasValue)
				num5 = travelTime.Value;
			Faction faction = sim.AssetDatabase.GetFaction(sim.GameDatabase.GetPlayerFactionID(fleetInfo.PlayerID));
			int num6 = 0;
			int num7 = 0;
			int num8 = 0;
			if (type == MissionType.COLONIZATION && planetID != 0)
			{
				double colSpace = Kerberos.Sots.StarFleet.StarFleet.GetColonizationSpace(sim, (IEnumerable<ShipInfo>)list, faction.Name);
				double terSpace = Kerberos.Sots.StarFleet.StarFleet.GetTerraformingSpace(sim, (IEnumerable<ShipInfo>)list);
				int num3 = Kerberos.Sots.StarFleet.StarFleet.GetNumColonizationShips(sim, (IEnumerable<ShipInfo>)list);
				if (sim.GameDatabase.GetPlayerFaction(fleetInfo.PlayerID).Name == "loa" && sim.GetPlayerObject(fleetInfo.PlayerID).IsAI())
				{
					colSpace = (double)Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(sim, fleetInfo.ID) / 2.0;
					terSpace = (double)Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(sim, fleetInfo.ID) / 2.0;
					num3 = 2;
				}
				if (colSpace > 0.0 && num3 > 0)
				{
					ColonyInfo colony = new ColonyInfo();
					colony.OrbitalObjectID = planetID;
					colony.ImperialPop = colSpace;
					colony.CivilianWeight = 0.0f;
					colony.PlayerID = fleetInfo.PlayerID;
					colony.InfraRate = 0.5f;
					colony.TerraRate = 0.5f;
					colony.DamagedLastTurn = false;
					PlanetInfo planetInfo = sim.GameDatabase.GetPlanetInfo(colony.OrbitalObjectID);
					planetInfo.Infrastructure = (float)(colSpace * 0.0001);
					num7 = Colony.SupportTripsTillSelfSufficient(sim, colony, planetInfo, colSpace, terSpace, fleetInfo);
					num8 = Colony.PredictTurnsToPhase1Completion(sim, colony, planetInfo);
					if (num8 > -1)
						num8 += (num7 + 1) * 2 * num5;
				}
			}
			else if (type == MissionType.EVACUATE && planetID != 0)
			{
				int num3 = Kerberos.Sots.StarFleet.StarFleet.GetNumColonizationShips(sim, (IEnumerable<ShipInfo>)list);
				double civilianPopulation = sim.GameDatabase.GetCivilianPopulation(planetID, 0, false);
				if (sim.GameDatabase.GetPlayerFaction(fleetInfo.PlayerID).Name == "loa" && sim.GetPlayerObject(fleetInfo.PlayerID).IsAI())
					num3 = 2;
				if (num3 > 0)
				{
					double num4 = (double)num3 * (double)sim.AssetDatabase.EvacCivPerCol;
					num7 = (int)Math.Ceiling(civilianPopulation / num4);
					num8 += (num7 + num5) * 2;
				}
			}
			else if (type == MissionType.SURVEY && systemID != 0)
				num6 = Kerberos.Sots.StarFleet.StarFleet.GetTurnsToSurveySystem(sim.GameDatabase, systemID, (IEnumerable<ShipInfo>)list);
			else if (type == MissionType.GATE && systemID != 0)
				num6 = 1;
			else if ((type == MissionType.PATROL || type == MissionType.INVASION || type == MissionType.INTERDICTION) && systemID != 0)
				num6 = !sim.IsSystemInSupplyRange(systemID, fleetInfo.PlayerID) ? (int)((double)fleetInfo.SupplyRemaining / (double)Kerberos.Sots.StarFleet.StarFleet.GetSupplyConsumption(sim, fleetInfo.ID)) : Kerberos.Sots.StarFleet.StarFleet.GetFleetEndurance(sim, fleetInfo.ID) * 2;
			else if (type == MissionType.CONSTRUCT_STN)
			{
				int num3 = (int)Math.Ceiling((double)Kerberos.Sots.StarFleet.StarFleet.GetConstructionPointsForFleet(sim, (IEnumerable<ShipInfo>)list));
				if (num3 > 0)
				{
					string factionName = sim.GameDatabase.GetFactionName(sim.GameDatabase.GetFleetFaction(fleetID));
					num6 = (Kerberos.Sots.StarFleet.StarFleet.GetStationConstructionCost(sim, stationType, factionName, stationLevel) + (num3 - 1)) / num3;
					num2 += (double)Kerberos.Sots.StarFleet.StarFleet.GetStationSavingsCost(sim, stationType, factionName, stationLevel);
				}
				else
					num6 = 0;
				if (sim.GameDatabase.GetPlayerFaction(fleetInfo.PlayerID).Name == "loa" && sim.GetPlayerObject(fleetInfo.PlayerID).IsAI())
					num6 = 1;
			}
			return new MissionEstimate()
			{
				TurnsToTarget = num5,
				TurnsToReturn = type == MissionType.RELOCATION | ReBase ? 0 : (faction.CanUseGate() ? 1 : num5),
				TurnsAtTarget = num6,
				TurnsForConstruction = num1,
				ConstructionCost = (float)num2,
				TripsTillSelfSufficeincy = num7,
				TurnsTillPhase1Completion = num8
			};
		}

		public static double GetFleetSlaves(GameSession game, int fleetID)
		{
			return game.GameDatabase.GetShipInfoByFleetID(fleetID, false).ToList<ShipInfo>().Sum<ShipInfo>((Func<ShipInfo, double>)(x => x.SlavesObtained));
		}

		public static float GetFleetTravelSpeed(
		  GameSession game,
		  int fleetID,
		  IEnumerable<ShipInfo> ships,
		  bool nodeTravel)
		{
			float val1 = 0.0f;
			float num1 = 0.0f;
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetID);
			Faction faction = game.GetPlayerObject(fleetInfo.PlayerID).Faction;
			bool flag = false;
			float val2_1 = 1f;
			float val2_2 = 1f;
			foreach (ShipInfo ship in ships)
			{
				if (ship.ParentID == 0)
				{
					foreach (DesignSectionInfo designSection in ship.DesignInfo.DesignSections)
					{
						ShipSectionAsset shipSectionAsset = designSection.ShipSectionAsset;
						if (shipSectionAsset != null)
						{
							val2_1 = Math.Max(shipSectionAsset.FleetSpeedModifier, val2_1);
							val2_2 = Math.Min(shipSectionAsset.FleetSpeedModifier, val2_2);
							if (!flag && faction.CanUseNodeLine(new bool?(false)) && shipSectionAsset.FileName.Contains("bore"))
								flag = true;
							if ((double)shipSectionAsset.NodeSpeed > 0.0 && ((double)num1 <= 0.0 || (double)num1 > (double)shipSectionAsset.NodeSpeed))
								num1 = shipSectionAsset.NodeSpeed;
							if (game.App.GetStratModifier<bool>(StratModifiers.UseFastestShipForFTLSpeed, fleetInfo.PlayerID))
								val1 = Math.Max(val1, shipSectionAsset.FtlSpeed);
							else if ((double)shipSectionAsset.FtlSpeed > 0.0 && ((double)val1 <= 0.0 || (double)val1 > (double)shipSectionAsset.FtlSpeed))
								val1 = shipSectionAsset.FtlSpeed;
						}
					}
				}
			}
			float num2 = !nodeTravel ? (!flag ? val1 : num1 * game.GameDatabase.GetStratModifier<float>(StratModifiers.BoreSpeedModifier, fleetInfo.PlayerID)) : num1;
			if (faction.CanUseFlockBonus())
				num2 += num2 * Kerberos.Sots.StarFleet.StarFleet.GetMorrigiFlockBonus(game, fleetID);
			if (faction.CanUseAccelerators())
			{
				if (fleetInfo.LastTurnAccelerated != game.GameDatabase.GetTurnCount())
				{
					if (fleetInfo.SystemID == 0 ? game.GameDatabase.IsInAccelRange(fleetInfo.ID) : game.GameDatabase.GetFleetsByPlayerAndSystem(fleetInfo.PlayerID, fleetInfo.SystemID, FleetType.FL_ACCELERATOR).Any<FleetInfo>())
					{
						game.GameDatabase.UpdateFleetAccelerated(game, fleetInfo.ID, new int?());
						fleetInfo = game.GameDatabase.GetFleetInfo(fleetInfo.ID);
					}
				}
				int num3 = Math.Max(game.GameDatabase.GetTurnCount() - fleetInfo.LastTurnAccelerated - 1, 0);
				if (Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(game, fleetInfo.ID) > Kerberos.Sots.StarFleet.StarFleet.GetMaxLoaFleetCubeMassForTransit(game, fleetInfo.PlayerID))
					num3 = 100;
				num2 = Math.Max(val1, Kerberos.Sots.StarFleet.StarFleet.LoaPlayerFleetSpeed(game, fleetInfo.PlayerID) - (float)num3);
			}
			return (float)((double)num2 + (double)num2 * ((double)val2_1 - 1.0) + (double)num2 * ((double)val2_2 - 1.0));
		}

		public static float GetFleetTravelSpeed(GameSession game, int fleetID, bool nodeTravel)
		{
			return Kerberos.Sots.StarFleet.StarFleet.GetFleetTravelSpeed(game, fleetID, game.GameDatabase.GetShipInfoByFleetID(fleetID, true), nodeTravel);
		}

		public static float LoaPlayerFleetSpeed(GameSession game, int playerid)
		{
			float num = 4f;
			if (game.GameDatabase.GetStratModifier<bool>(StratModifiers.StandingNeutrinoWaves, playerid))
				num = 6f;
			return num;
		}

		public static int PredictProductionOutputForSystem(GameSession sim, int systemID, int playerID)
		{
			int num = 0;
			foreach (int starSystemPlanet in sim.GameDatabase.GetStarSystemPlanets(systemID))
			{
				PlanetInfo planetInfo = sim.GameDatabase.GetPlanetInfo(starSystemPlanet);
				ColonyInfo colonyInfoForPlanet = sim.GameDatabase.GetColonyInfoForPlanet(planetInfo.ID);
				if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == playerID)
					num += (int)Colony.GetShipConstResources(sim, colonyInfoForPlanet, planetInfo);
			}
			return num;
		}

		private static float GetMorrigiFlockBonus(GameSession game, int fleetID)
		{
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetID);
			float val2_1 = 0.0f;
			float val2_2 = 0.0f;
			float val2_3 = 0.0f;
			float val2_4 = 0.0f;
			float stratModifier = game.GameDatabase.GetStratModifier<float>(StratModifiers.MaxFlockBonusMod, fleetInfo.PlayerID);
			int num1 = (int)((double)game.AssetDatabase.FlockBRCountBonus * (double)stratModifier);
			int num2 = (int)((double)game.AssetDatabase.FlockCRCountBonus * (double)stratModifier);
			int num3 = (int)((double)game.AssetDatabase.FlockDNCountBonus * (double)stratModifier);
			int num4 = (int)((double)game.AssetDatabase.FlockLVCountBonus * (double)stratModifier);
			float flockMaxBonus = game.AssetDatabase.FlockMaxBonus;
			foreach (ShipInfo shipInfo in game.GameDatabase.GetShipInfoByFleetID(fleetID, false))
			{
				switch (shipInfo.DesignInfo.Class)
				{
					case ShipClass.Cruiser:
						if (num2 > 0)
						{
							val2_2 += game.AssetDatabase.FlockCRBonus;
							--num2;
							continue;
						}
						continue;
					case ShipClass.Dreadnought:
						if (num3 > 0)
						{
							val2_3 += game.AssetDatabase.FlockDNBonus;
							--num3;
							continue;
						}
						continue;
					case ShipClass.Leviathan:
						if (num4 > 0)
						{
							val2_4 += game.AssetDatabase.FlockLVBonus;
							--num4;
							continue;
						}
						continue;
					case ShipClass.BattleRider:
						if (num1 > 0)
						{
							val2_1 += game.AssetDatabase.FlockBRBonus;
							--num1;
							continue;
						}
						continue;
					default:
						continue;
				}
			}
			return Math.Min(flockMaxBonus, val2_1) + Math.Min(flockMaxBonus, val2_2) + Math.Min(flockMaxBonus, val2_3) + Math.Min(flockMaxBonus, val2_4);
		}

		public static int GetDesignForColonizer(GameSession game, int playerID)
		{
			int num1 = 0;
			int num2 = 0;
			foreach (DesignInfo designInfo in game.GameDatabase.GetDesignInfosForPlayer(playerID))
			{
				foreach (string designSectionName in game.GameDatabase.GetDesignSectionNames(designInfo.ID))
				{
					string sectionName = designSectionName;
					ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == sectionName));
					if (shipSectionAsset != null && shipSectionAsset.ColonizationSpace > num2)
					{
						num1 = designInfo.ID;
						num2 = shipSectionAsset.ColonizationSpace;
					}
				}
			}
			return num1;
		}

		public static double GetTerraformingSpace(GameSession sim, IEnumerable<ShipInfo> ships)
		{
			double num = 0.0;
			foreach (ShipInfo ship in ships)
			{
				foreach (string designSectionName in sim.GameDatabase.GetDesignSectionNames(ship.DesignID))
				{
					string sectionName = designSectionName;
					ShipSectionAsset shipSectionAsset = sim.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == sectionName));
					if (shipSectionAsset != null && shipSectionAsset.TerraformingSpace > 0)
						num += (double)shipSectionAsset.TerraformingSpace;
				}
			}
			return num;
		}

		public static double GetColonizationSpace(
		  GameSession sim,
		  IEnumerable<ShipInfo> ships,
		  string FactionName)
		{
			double num = 0.0;
			if (FactionName == "loa")
			{
				if (ships.Any<ShipInfo>())
				{
					PlayerInfo playerInfo = sim.GameDatabase.GetPlayerInfo(sim.GameDatabase.GetFleetInfo(ships.First<ShipInfo>().FleetID).PlayerID);
					DesignInfo designInfo = sim.GameDatabase.GetDesignInfosForPlayer(playerInfo.ID).FirstOrDefault<DesignInfo>((Func<DesignInfo, bool>)(x => ((IEnumerable<DesignSectionInfo>)x.DesignSections).Any<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(j => j.ShipSectionAsset.ColonizationSpace > 0))));
					if (designInfo != null)
					{
						foreach (DesignSectionInfo designSection in designInfo.DesignSections)
							num += (double)designSection.ShipSectionAsset.ColonizationSpace;
					}
				}
			}
			else
			{
				foreach (ShipInfo ship in ships)
				{
					foreach (string designSectionName in sim.GameDatabase.GetDesignSectionNames(ship.DesignID))
					{
						string sectionName = designSectionName;
						ShipSectionAsset shipSectionAsset = sim.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == sectionName));
						if (shipSectionAsset != null && shipSectionAsset.ColonizationSpace > 0)
							num += (double)shipSectionAsset.ColonizationSpace;
					}
				}
			}
			return num;
		}

		public static double GetTerraformingSpace(GameSession game, int fleetID)
		{
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetID);
			float num = 1f;
			List<AdmiralInfo.TraitType> list = game.GameDatabase.GetAdmiralTraits(fleetInfo.AdmiralID).ToList<AdmiralInfo.TraitType>();
			if (list.Contains(AdmiralInfo.TraitType.GreenThumb))
				num += 0.1f;
			if (list.Contains(AdmiralInfo.TraitType.BlackThumb))
				num -= 0.1f;
			return Kerberos.Sots.StarFleet.StarFleet.GetTerraformingSpace(game, game.GameDatabase.GetShipInfoByFleetID(fleetID, false)) * (double)num;
		}

		public static double GetColonizationSpace(GameSession game, int fleetID)
		{
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetID);
			float num = 1f;
			List<AdmiralInfo.TraitType> list = game.GameDatabase.GetAdmiralTraits(fleetInfo.AdmiralID).ToList<AdmiralInfo.TraitType>();
			if (list.Contains(AdmiralInfo.TraitType.GoodShepherd))
				num += 0.1f;
			if (list.Contains(AdmiralInfo.TraitType.BadShepherd))
				num -= 0.1f;
			return Kerberos.Sots.StarFleet.StarFleet.GetColonizationSpace(game, game.GameDatabase.GetShipInfoByFleetID(fleetID, false), game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerInfo(fleetInfo.PlayerID).FactionID).Name) * (double)num;
		}

		public static bool CanDesignColonize(GameSession game, int designID)
		{
			foreach (string designSectionName in game.GameDatabase.GetDesignSectionNames(designID))
			{
				string sectionName = designSectionName;
				ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == sectionName));
				if (shipSectionAsset != null && shipSectionAsset.ColonizationSpace > 0)
					return true;
			}
			return false;
		}

		public static bool CanDesignConstruct(GameSession game, int designID)
		{
			foreach (string designSectionName in game.GameDatabase.GetDesignSectionNames(designID))
			{
				string sectionName = designSectionName;
				ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == sectionName));
				if (shipSectionAsset != null && shipSectionAsset.isConstructor)
					return true;
			}
			return false;
		}

		public static int GetNumColonizationShips(GameSession game, IEnumerable<ShipInfo> ships)
		{
			int num = 0;
			foreach (ShipInfo ship in ships)
			{
				foreach (string designSectionName in game.GameDatabase.GetDesignSectionNames(ship.DesignID))
				{
					string sectionName = designSectionName;
					ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == sectionName));
					if (shipSectionAsset != null && shipSectionAsset.ColonizationSpace > 0)
						++num;
				}
			}
			return num;
		}

		public static ShipInfo GetFirstConstructionShip(GameSession game, FleetInfo fleet)
		{
			foreach (ShipInfo shipInfo in game.GameDatabase.GetShipInfoByFleetID(fleet.ID, false))
			{
				foreach (string designSectionName in game.GameDatabase.GetDesignSectionNames(shipInfo.DesignID))
				{
					string sectionName = designSectionName;
					ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == sectionName));
					if (shipSectionAsset != null && shipSectionAsset.ColonizationSpace > 0)
						return shipInfo;
				}
			}
			return (ShipInfo)null;
		}

		public static int GetNumColonizationShips(GameSession game, int fleetID)
		{
			return Kerberos.Sots.StarFleet.StarFleet.GetNumColonizationShips(game, game.GameDatabase.GetShipInfoByFleetID(fleetID, false));
		}

		public static int GetNumConstructionShips(GameSession game, int fleetID)
		{
			int num = 0;
			foreach (ShipInfo shipInfo in game.GameDatabase.GetShipInfoByFleetID(fleetID, false))
			{
				foreach (string designSectionName in game.GameDatabase.GetDesignSectionNames(shipInfo.DesignID))
				{
					string sectionName = designSectionName;
					ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == sectionName));
					if (shipSectionAsset != null && shipSectionAsset.isConstructor)
						++num;
				}
			}
			return num;
		}

		public static int ProjectNumColonizationRuns(
		  GameSession sim,
		  int planetID,
		  int fleetID,
		  List<int> colonizersToBuild,
		  int turnTimeout)
		{
			return 1;
		}

		public static int GetTerraformingSpaceForDesign(GameSession game, int designID)
		{
			DesignInfo designInfo = game.GameDatabase.GetDesignInfo(designID);
			int num = 0;
			foreach (string designSectionName in game.GameDatabase.GetDesignSectionNames(designInfo.ID))
			{
				string sectionName = designSectionName;
				ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == sectionName));
				if (shipSectionAsset != null)
					num += shipSectionAsset.TerraformingSpace;
			}
			return num;
		}

		public static int GetColonizationSpaceForDesign(GameSession game, int designID)
		{
			DesignInfo designInfo = game.GameDatabase.GetDesignInfo(designID);
			int num = 0;
			foreach (string designSectionName in game.GameDatabase.GetDesignSectionNames(designInfo.ID))
			{
				string sectionName = designSectionName;
				ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == sectionName));
				if (shipSectionAsset != null)
					num += shipSectionAsset.ColonizationSpace;
			}
			return num;
		}

		public static float GetSupplyCapacity(GameDatabase game, int fleetID)
		{
			float num = 0.0f;
			foreach (ShipInfo shipInfo in game.GetShipInfoByFleetID(fleetID, false))
			{
				foreach (string designSectionName in game.GetDesignSectionNames(shipInfo.DesignID))
				{
					string sectionName = designSectionName;
					ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == sectionName));
					if (shipSectionAsset != null && (double)shipSectionAsset.Supply > 0.0)
						num += (float)shipSectionAsset.Supply;
				}
			}
			return num;
		}

		public static float GetSupplyConsumption(GameSession game, int fleetID)
		{
			int num = 0;
			foreach (ShipInfo shipInfo in game.GameDatabase.GetShipInfoByFleetID(fleetID, true))
				num = shipInfo.DesignInfo.SupplyRequired;
			return (float)num;
		}

		public static float GetConstructionPointsForFleet(GameSession game, IEnumerable<ShipInfo> ships)
		{
			if (GameSession.InstaBuildHackEnabled)
				return 1E+09f;
			float num = 0.0f;
			foreach (ShipInfo ship in ships)
			{
				foreach (string designSectionName in game.GameDatabase.GetDesignSectionNames(ship.DesignID))
				{
					string sectionName = designSectionName;
					ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == sectionName));
					if (shipSectionAsset != null && shipSectionAsset.isConstructor)
						num += (float)shipSectionAsset.ConstructionPoints;
				}
			}
			return num;
		}

		public static float GetConstructionPointsForFleet(GameSession game, int fleetID)
		{
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetID);
			float num = 1f;
			if (game.GameDatabase.GetAdmiralTraits(fleetInfo.AdmiralID).ToList<AdmiralInfo.TraitType>().Contains(AdmiralInfo.TraitType.Architect))
				num += 0.1f;
			return Kerberos.Sots.StarFleet.StarFleet.GetConstructionPointsForFleet(game, game.GameDatabase.GetShipInfoByFleetID(fleetID, false)) * num;
		}

		public static bool CanDoSurveyMissionToTarget(
		  GameSession game,
		  int systemID,
		  int fleetID,
		  float? fleetRange = null,
		  float? travelSpeed = null,
		  float? nodeTravelSpeed = null)
		{
			MissionInfo missionByFleetId = game.GameDatabase.GetMissionByFleetID(fleetID);
			if (missionByFleetId != null)
			{
				switch (missionByFleetId.Type)
				{
					case MissionType.PATROL:
					case MissionType.RETURN:
						break;
					default:
						return false;
				}
			}
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetID);
			if (fleetInfo.IsReserveFleet || Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(game, fleetInfo) || Kerberos.Sots.StarFleet.StarFleet.IsFleetExhausted(game, fleetInfo))
				return false;
			return Kerberos.Sots.StarFleet.StarFleet.IsFleetInRange(game, fleetID, systemID, fleetRange, travelSpeed, nodeTravelSpeed);
		}

		public static bool HasRelocatableDefResAssetsInRange(GameSession game, int systemID)
		{
			if (!game.GameDatabase.IsSurveyed(game.LocalPlayer.ID, systemID) || game.GameDatabase.GetNavalStationForSystemAndPlayer(systemID, game.LocalPlayer.ID) == null && !game.GameDatabase.GetColonyInfosForSystem(systemID).Any<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == game.LocalPlayer.ID)))
				return false;
			List<StarSystemInfo> list1 = game.GameDatabase.GetVisibleStarSystemInfos(game.LocalPlayer.ID).ToList<StarSystemInfo>().Where<StarSystemInfo>((Func<StarSystemInfo, bool>)(x => game.GameDatabase.GetFleetInfoBySystemID(x.ID, FleetType.FL_RESERVE | FleetType.FL_DEFENSE).Where<FleetInfo>((Func<FleetInfo, bool>)(j =>
		  {
			  if (j.PlayerID == game.LocalPlayer.ID)
				  return game.GameDatabase.GetShipsByFleetID(j.ID).Any<int>();
			  return false;
		  })).Any<FleetInfo>())).ToList<StarSystemInfo>();
			List<MissionInfo> list2 = game.GameDatabase.GetMissionsBySystemDest(systemID).Where<MissionInfo>((Func<MissionInfo, bool>)(x => x.Type == MissionType.INTERDICTION)).ToList<MissionInfo>();
			if (list1.Any<StarSystemInfo>())
				return !list2.Any<MissionInfo>();
			return false;
		}

		public static bool CanDoRelocationMissionToTarget(GameSession game, int systemId, int fleetID)
		{
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetID);
			if (Kerberos.Sots.StarFleet.StarFleet.IsSuulkaFleet(game.GameDatabase, fleetInfo) || Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(game, fleetInfo))
				return true;
			if (!Kerberos.Sots.StarFleet.StarFleet.CanSystemSupportFleet(game, systemId, fleetID))
				return false;
			IEnumerable<ColonyInfo> colonyInfosForSystem = game.GameDatabase.GetColonyInfosForSystem(systemId);
			StationInfo forSystemAndPlayer = game.GameDatabase.GetNavalStationForSystemAndPlayer(systemId, fleetInfo.PlayerID);
			if (colonyInfosForSystem == null && forSystemAndPlayer == null)
				return false;
			bool flag1 = colonyInfosForSystem.Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == fleetInfo.PlayerID)).Any<ColonyInfo>();
			bool flag2 = forSystemAndPlayer != null;
			return fleetInfo.SupportingSystemID != systemId && (flag1 || flag2) && (game.GameDatabase.GetMissionByFleetID(fleetInfo.ID) == null && !fleetInfo.IsReserveFleet);
		}

		public static bool CanSystemSupportFleet(GameSession sim, int systemId, int fleetId)
		{
			FleetInfo fleetInfo = sim.GameDatabase.GetFleetInfo(fleetId);
			return sim.GameDatabase.GetRemainingSupportPoints(sim, systemId, fleetInfo.PlayerID) >= sim.GameDatabase.GetFleetCruiserEquivalent(fleetId);
		}

		public static bool CanDoEvacuationMissionToTarget(
		  GameSession game,
		  int systemID,
		  int fleetID,
		  int fleetSupportingSystimID,
		  float? fleetRange = null,
		  float? travelSpeed = null,
		  float? nodeTravelSpeed = null)
		{
			if (systemID == fleetSupportingSystimID || Kerberos.Sots.StarFleet.StarFleet.GetNumColonizationShips(game, fleetID) <= 0)
				return false;
			return Kerberos.Sots.StarFleet.StarFleet.CanDoSurveyMissionToTarget(game, systemID, fleetID, fleetRange, travelSpeed, nodeTravelSpeed);
		}

		public static bool CanDoColonizeMissionToTarget(
		  GameSession game,
		  int systemID,
		  int fleetID,
		  float? fleetRange = null,
		  float? travelSpeed = null,
		  float? nodeTravelSpeed = null)
		{
			return Kerberos.Sots.StarFleet.StarFleet.CanDoSurveyMissionToTarget(game, systemID, fleetID, fleetRange, travelSpeed, nodeTravelSpeed);
		}

		public static bool CanDoTransferMissionToTarget(GameSession game, int systemID, int fleetID)
		{
			if (game.GameDatabase.GetMissionByFleetID(fleetID) != null)
				return false;
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetID);
			if (fleetInfo.IsReserveFleet || fleetInfo == null || fleetInfo.SupportingSystemID == systemID)
				return false;
			if (game.GameDatabase.GetFactionInfo(game.GameDatabase.GetFleetFaction(fleetID)).Name == "human")
			{
				foreach (int systemId in Kerberos.Sots.StarFleet.StarFleet.GetNodeTravelPath(game.GameDatabase, fleetInfo.SystemID, fleetInfo.SupportingSystemID, fleetInfo.PlayerID, true, false, false))
				{
					if (systemId == 0)
						return false;
					App.Log.Trace("Human node path: " + game.GameDatabase.GetStarSystemInfo(systemId).Name, nameof(game));
				}
			}
			return true;
		}

		public static bool CanDoPatrolMissionToTarget(
		  GameSession game,
		  int systemID,
		  int fleetID,
		  float? fleetRange = null,
		  float? travelSpeed = null,
		  float? nodeTravelSpeed = null)
		{
			return Kerberos.Sots.StarFleet.StarFleet.CanDoSurveyMissionToTarget(game, systemID, fleetID, fleetRange, travelSpeed, nodeTravelSpeed);
		}

		public static bool CanDoDeployNPGToTarget(
		  GameSession game,
		  int systemID,
		  int fleetID,
		  float? fleetRange = null,
		  float? travelSpeed = null,
		  float? nodeTravelSpeed = null)
		{
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetID);
			if (!Kerberos.Sots.StarFleet.StarFleet.CanDoSurveyMissionToTarget(game, systemID, fleetID, fleetRange, travelSpeed, nodeTravelSpeed) || fleetInfo.SystemID == 0 || !game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerInfo(fleetInfo.PlayerID).FactionID).CanUseAccelerators())
				return false;
			int productionCost = ((IEnumerable<DesignSectionInfo>)game.GameDatabase.GetDesignInfosForPlayer(fleetInfo.PlayerID).FirstOrDefault<DesignInfo>((Func<DesignInfo, bool>)(x => x.IsAccelerator())).DesignSections).First<DesignSectionInfo>().ShipSectionAsset.ProductionCost;
			return Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(game, fleetID) >= productionCost;
		}

		public static bool CanDoGateMissionToTarget(
		  GameSession sim,
		  int systemId,
		  int fleetId,
		  float? fleetRange = null,
		  float? travelSpeed = null,
		  float? nodeTravelSpeed = null)
		{
			bool flag = false;
			List<ShipInfo> list = sim.GameDatabase.GetShipInfoByFleetID(fleetId, true).ToList<ShipInfo>();
			if (list == null)
				return false;
			foreach (ShipInfo shipInfo in list)
			{
				if (shipInfo.DesignInfo.Role == ShipRole.GATE)
				{
					flag = true;
					break;
				}
			}
			if (flag)
				return Kerberos.Sots.StarFleet.StarFleet.CanDoSurveyMissionToTarget(sim, systemId, fleetId, fleetRange, travelSpeed, nodeTravelSpeed);
			return false;
		}

		public static bool CanDoInterdictionMissionToTarget(
		  GameSession game,
		  int systemID,
		  int fleetID,
		  float? fleetRange = null,
		  float? travelSpeed = null,
		  float? nodeTravelSpeed = null)
		{
			return Kerberos.Sots.StarFleet.StarFleet.CanDoSurveyMissionToTarget(game, systemID, fleetID, fleetRange, travelSpeed, nodeTravelSpeed);
		}

		public static bool CanDoStrikeMissionToTarget(
		  GameSession game,
		  int systemID,
		  int fleetID,
		  float? fleetRange = null,
		  float? travelSpeed = null,
		  float? nodeTravelSpeed = null)
		{
			return Kerberos.Sots.StarFleet.StarFleet.CanDoSurveyMissionToTarget(game, systemID, fleetID, fleetRange, travelSpeed, nodeTravelSpeed);
		}

		public static bool CanDoInvasionMissionToTarget(
		  GameSession game,
		  int systemID,
		  int fleetID,
		  float? fleetRange = null,
		  float? travelSpeed = null,
		  float? nodeTravelSpeed = null)
		{
			return Kerberos.Sots.StarFleet.StarFleet.CanDoSurveyMissionToTarget(game, systemID, fleetID, fleetRange, travelSpeed, nodeTravelSpeed);
		}

		public static bool CanDoSupportMissionToTarget(
		  GameSession game,
		  int systemID,
		  int fleetID,
		  float? fleetRange = null,
		  float? travelSpeed = null,
		  float? nodeTravelSpeed = null)
		{
			return Kerberos.Sots.StarFleet.StarFleet.CanDoColonizeMissionToTarget(game, systemID, fleetID, fleetRange, travelSpeed, nodeTravelSpeed);
		}

		public static bool CanDoPiracyMissionToTarget(
		  GameSession game,
		  int systemID,
		  int fleetID,
		  float? fleetRange = null,
		  float? travelSpeed = null,
		  float? nodeTravelSpeed = null)
		{
			if (game.GameDatabase.GetMissionByFleetID(fleetID) != null)
				return false;
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetID);
			if (fleetInfo.IsReserveFleet || Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(game, fleetInfo) || game.GameDatabase.GetShipInfoByFleetID(fleetID, false).ToList<ShipInfo>().Any<ShipInfo>((Func<ShipInfo, bool>)(x =>
		   {
			   RealShipClasses? realShipClass1 = x.DesignInfo.GetRealShipClass();
			   if ((realShipClass1.GetValueOrDefault() != RealShipClasses.Dreadnought ? 0 : (realShipClass1.HasValue ? 1 : 0)) != 0)
				   return true;
			   RealShipClasses? realShipClass2 = x.DesignInfo.GetRealShipClass();
			   if (realShipClass2.GetValueOrDefault() == RealShipClasses.Leviathan)
				   return realShipClass2.HasValue;
			   return false;
		   })))
				return false;
			return Kerberos.Sots.StarFleet.StarFleet.IsFleetInRange(game, fleetID, systemID, fleetRange, travelSpeed, nodeTravelSpeed);
		}

		public static bool CanDoConstructionMissionToTarget(
		  GameSession game,
		  int systemID,
		  int fleetID,
		  bool forUI)
		{
			if (game.GameDatabase.GetMissionByFleetID(fleetID) != null)
				return false;
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetID);
			if (fleetInfo.IsReserveFleet)
				return false;
			if (game.GameDatabase.GetFactionInfo(game.GameDatabase.GetFleetFaction(fleetID)).Name == "human")
			{
				foreach (int systemId in Kerberos.Sots.StarFleet.StarFleet.GetNodeTravelPath(game.GameDatabase, fleetInfo.SystemID, systemID, fleetInfo.PlayerID, true, false, false))
				{
					if (systemId == 0)
						return false;
					App.Log.Trace("Human node path: " + game.GameDatabase.GetStarSystemInfo(systemId).Name, nameof(game));
				}
			}
			return forUI || (double)Kerberos.Sots.StarFleet.StarFleet.GetConstructionPointsForFleet(game, fleetID) >= 1.0;
		}

		public static bool IsFleetWaitingForBuildOrders(App game, int missionID, int fleetID)
		{
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetID);
			foreach (BuildOrderInfo buildOrderInfo in game.GameDatabase.GetBuildOrdersForSystem(fleetInfo.SystemID))
			{
				if (buildOrderInfo.MissionID == missionID)
					return true;
			}
			return false;
		}

		public static List<int> GetMissionCapableShips(
		  GameSession game,
		  int fleetID,
		  MissionType missionType)
		{
			List<int> intList = new List<int>();
			switch (missionType)
			{
				case MissionType.COLONIZATION:
				case MissionType.SUPPORT:
				case MissionType.EVACUATE:
					using (IEnumerator<ShipInfo> enumerator = game.GameDatabase.GetShipInfoByFleetID(fleetID, false).GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							ShipInfo current = enumerator.Current;
							if (Kerberos.Sots.StarFleet.StarFleet.CanDesignColonize(game, current.DesignID))
								intList.Add(current.ID);
						}
						break;
					}
				case MissionType.CONSTRUCT_STN:
					using (IEnumerator<ShipInfo> enumerator = game.GameDatabase.GetShipInfoByFleetID(fleetID, false).GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							ShipInfo current = enumerator.Current;
							if (Kerberos.Sots.StarFleet.StarFleet.CanDesignConstruct(game, current.DesignID))
								intList.Add(current.ID);
						}
						break;
					}
				default:
					intList.AddRange(game.GameDatabase.GetShipsByFleetID(fleetID));
					break;
			}
			return intList;
		}

		public static string GetShipClassAbbr(ShipClass shipClass)
		{
			switch (shipClass)
			{
				case ShipClass.Cruiser:
					return "CR";
				case ShipClass.Dreadnought:
					return "DN";
				case ShipClass.Leviathan:
					return "LV";
				case ShipClass.BattleRider:
					return "BR";
				case ShipClass.Station:
					return "SN";
				default:
					throw new ArgumentOutOfRangeException(nameof(shipClass));
			}
		}

		public static StationTypeFlags MissionStringToStationType(string missionType)
		{
			return (StationTypeFlags)Enum.Parse(typeof(StationTypeFlags), missionType.Split(' ')[0]);
		}

		public static void CheckSystemCanSupportResidentFleets(App App, int systemId, int playerId)
		{
			int remainingSupportPoints = App.GameDatabase.GetRemainingSupportPoints(App.Game, systemId, playerId);
			bool flag1 = App.GameDatabase.GetColonyInfosForSystem(systemId).Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == playerId)).Any<ColonyInfo>();
			if (remainingSupportPoints >= 0)
				return;
			foreach (FleetInfo fleetInfo in App.GameDatabase.GetFleetInfoBySystemID(systemId, FleetType.FL_NORMAL).Where<FleetInfo>((Func<FleetInfo, bool>)(x => App.GameDatabase.GetMissionByFleetID(x.ID) == null)).ToList<FleetInfo>())
			{
				if (remainingSupportPoints >= 0)
					break;
				remainingSupportPoints += App.GameDatabase.GetFleetCruiserEquivalent(fleetInfo.ID);
				bool flag2 = Kerberos.Sots.StarFleet.StarFleet.IsSuulkaFleet(App.GameDatabase, fleetInfo);
				if (flag1)
				{
					if (!flag2)
					{
						int? reserveFleetId = App.GameDatabase.GetReserveFleetID(fleetInfo.PlayerID, fleetInfo.SystemID);
						if (!reserveFleetId.HasValue)
						{
							App.GameDatabase.RemoveFleet(fleetInfo.ID);
						}
						else
						{
							foreach (ShipInfo shipInfo in App.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, false).ToList<ShipInfo>())
							{
								App.GameDatabase.UpdateShipAIFleetID(shipInfo.ID, new int?());
								App.GameDatabase.TransferShip(shipInfo.ID, reserveFleetId.Value);
							}
							App.GameDatabase.RemoveFleet(fleetInfo.ID);
						}
					}
				}
				else
				{
					int newHomeSystem = App.GameDatabase.FindNewHomeSystem(fleetInfo);
					if (newHomeSystem != 0)
					{
						bool flag3 = false;
						if (!flag2 && App.GameDatabase.GetRemainingSupportPoints(App.Game, newHomeSystem, fleetInfo.PlayerID) < App.GameDatabase.GetFleetCruiserEquivalent(fleetInfo.ID))
							flag3 = true;
						int missionID = App.GameDatabase.InsertMission(fleetInfo.ID, MissionType.RELOCATION, newHomeSystem, 0, 0, 1, false, new int?());
						App.GameDatabase.InsertWaypoint(missionID, WaypointType.TravelTo, new int?(newHomeSystem));
						if (flag3)
							App.GameDatabase.InsertWaypoint(missionID, WaypointType.DisbandFleet, new int?());
						App.GameDatabase.InsertWaypoint(missionID, WaypointType.DoMission, new int?());
					}
				}
			}
		}

		public static IEnumerable<int> CollectAvailableSystemsForFleetMission(
		  GameDatabase db,
		  GameSession game,
		  int fleetid,
		  MissionType mission,
		  bool forUI)
		{
			List<StarSystemInfo> list = game.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>();
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetid);
			int playerId = fleetInfo.PlayerID;
			string factionName = game.GameDatabase.GetFactionName(game.GameDatabase.GetPlayerFactionID(fleetInfo.PlayerID));
			game.AssetDatabase.GetFaction(factionName);
			List<int> intList = new List<int>();
			foreach (int num in list.Select<StarSystemInfo, int>((Func<StarSystemInfo, int>)(x => x.ID)))
			{
				bool flag = false;
				switch (mission)
				{
					case MissionType.COLONIZATION:
						if (game.GameDatabase.CanColonize(playerId, num, game.GameDatabase.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, playerId)))
						{
							flag = Kerberos.Sots.StarFleet.StarFleet.CanDoColonizeMissionToTarget(game, num, fleetInfo.ID, new float?(), new float?(), new float?()) && (Kerberos.Sots.StarFleet.StarFleet.GetNumColonizationShips(game, fleetInfo.ID) > 0 || factionName == "loa");
							break;
						}
						break;
					case MissionType.SUPPORT:
						if (game.GameDatabase.CanSupport(playerId, num))
						{
							flag = Kerberos.Sots.StarFleet.StarFleet.CanDoSupportMissionToTarget(game, num, fleetInfo.ID, new float?(), new float?(), new float?());
							break;
						}
						break;
					case MissionType.SURVEY:
						if (game.GameDatabase.CanSurvey(playerId, num))
						{
							flag = Kerberos.Sots.StarFleet.StarFleet.CanDoSurveyMissionToTarget(game, num, fleetInfo.ID, new float?(), new float?(), new float?());
							break;
						}
						break;
					case MissionType.RELOCATION:
						if (game.GameDatabase.CanRelocate(game, playerId, num))
						{
							flag = Kerberos.Sots.StarFleet.StarFleet.CanDoRelocationMissionToTarget(game, num, fleetInfo.ID);
							break;
						}
						break;
					case MissionType.PATROL:
						if (game.GameDatabase.CanPatrol(playerId, num))
						{
							flag = Kerberos.Sots.StarFleet.StarFleet.CanDoPatrolMissionToTarget(game, num, fleetInfo.ID, new float?(), new float?(), new float?());
							break;
						}
						break;
					case MissionType.INTERDICTION:
						if (game.GameDatabase.CanInterdict(playerId, num))
						{
							flag = Kerberos.Sots.StarFleet.StarFleet.CanDoInterdictionMissionToTarget(game, num, fleetInfo.ID, new float?(), new float?(), new float?());
							break;
						}
						break;
					case MissionType.STRIKE:
						if (game.GameDatabase.CanStrike(playerId, num))
						{
							flag = Kerberos.Sots.StarFleet.StarFleet.CanDoStrikeMissionToTarget(game, num, fleetInfo.ID, new float?(), new float?(), new float?());
							break;
						}
						break;
					case MissionType.INVASION:
						if (game.GameDatabase.CanInvade(playerId, num))
						{
							flag = Kerberos.Sots.StarFleet.StarFleet.CanDoInvasionMissionToTarget(game, num, fleetInfo.ID, new float?(), new float?(), new float?());
							break;
						}
						break;
					case MissionType.PIRACY:
						if (game.GameDatabase.CanPirate(playerId, num))
						{
							flag = Kerberos.Sots.StarFleet.StarFleet.CanDoPiracyMissionToTarget(game, num, fleetInfo.ID, new float?(), new float?(), new float?());
							break;
						}
						break;
					case MissionType.EVACUATE:
						flag = Kerberos.Sots.StarFleet.StarFleet.CanDoEvacuationMissionToTarget(game, num, fleetInfo.ID, fleetInfo.SupportingSystemID, new float?(), new float?(), new float?()) || factionName == "loa";
						break;
				}
				if (flag)
				{
					if (!intList.Contains(num))
						intList.Add(num);
					if (forUI)
						break;
				}
			}
			return (IEnumerable<int>)intList;
		}

		public static IEnumerable<FleetInfo> CollectAvailableFleets(
		  GameSession game,
		  int playerId,
		  int systemId,
		  MissionType missionType,
		  bool forUI)
		{
			IEnumerable<FleetInfo> fleetInfosByPlayerId = game.GameDatabase.GetFleetInfosByPlayerID(playerId, FleetType.FL_NORMAL);
			string factionName = game.GameDatabase.GetFactionName(game.GameDatabase.GetPlayerFactionID(playerId));
			Faction faction = game.AssetDatabase.GetFaction(factionName);
			switch (missionType)
			{
				case MissionType.COLONIZATION:
					if (game.GameDatabase.CanColonize(playerId, systemId, game.GameDatabase.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, playerId)))
						return fleetInfosByPlayerId.Where<FleetInfo>((Func<FleetInfo, bool>)(x =>
					   {
						   if (!Kerberos.Sots.StarFleet.StarFleet.CanDoColonizeMissionToTarget(game, systemId, x.ID, new float?(), new float?(), new float?()))
							   return false;
						   if (Kerberos.Sots.StarFleet.StarFleet.GetNumColonizationShips(game, x.ID) <= 0)
							   return factionName == "loa";
						   return true;
					   }));
					break;
				case MissionType.SUPPORT:
					if (game.GameDatabase.CanSupport(playerId, systemId))
						return fleetInfosByPlayerId.Where<FleetInfo>((Func<FleetInfo, bool>)(x => Kerberos.Sots.StarFleet.StarFleet.CanDoSupportMissionToTarget(game, systemId, x.ID, new float?(), new float?(), new float?())));
					break;
				case MissionType.SURVEY:
					if (game.GameDatabase.CanSurvey(playerId, systemId))
						return fleetInfosByPlayerId.Where<FleetInfo>((Func<FleetInfo, bool>)(x => Kerberos.Sots.StarFleet.StarFleet.CanDoSurveyMissionToTarget(game, systemId, x.ID, new float?(), new float?(), new float?())));
					break;
				case MissionType.RELOCATION:
					if (game.GameDatabase.CanRelocate(game, playerId, systemId))
						return fleetInfosByPlayerId.Where<FleetInfo>((Func<FleetInfo, bool>)(x => Kerberos.Sots.StarFleet.StarFleet.CanDoRelocationMissionToTarget(game, systemId, x.ID)));
					break;
				case MissionType.CONSTRUCT_STN:
					if (game.GameDatabase.CanConstructStation(game.App, playerId, systemId, faction.CanUseGate()))
						return fleetInfosByPlayerId.Where<FleetInfo>((Func<FleetInfo, bool>)(x => Kerberos.Sots.StarFleet.StarFleet.CanDoConstructionMissionToTarget(game, systemId, x.ID, forUI)));
					break;
				case MissionType.PATROL:
					if (game.GameDatabase.CanPatrol(playerId, systemId))
						return fleetInfosByPlayerId.Where<FleetInfo>((Func<FleetInfo, bool>)(x => Kerberos.Sots.StarFleet.StarFleet.CanDoPatrolMissionToTarget(game, systemId, x.ID, new float?(), new float?(), new float?())));
					break;
				case MissionType.INTERDICTION:
					if (game.GameDatabase.CanInterdict(playerId, systemId))
						return fleetInfosByPlayerId.Where<FleetInfo>((Func<FleetInfo, bool>)(x => Kerberos.Sots.StarFleet.StarFleet.CanDoInterdictionMissionToTarget(game, systemId, x.ID, new float?(), new float?(), new float?())));
					break;
				case MissionType.STRIKE:
					if (game.GameDatabase.CanStrike(playerId, systemId))
						return fleetInfosByPlayerId.Where<FleetInfo>((Func<FleetInfo, bool>)(x => Kerberos.Sots.StarFleet.StarFleet.CanDoStrikeMissionToTarget(game, systemId, x.ID, new float?(), new float?(), new float?())));
					break;
				case MissionType.INVASION:
					if (game.GameDatabase.CanInvade(playerId, systemId))
						return fleetInfosByPlayerId.Where<FleetInfo>((Func<FleetInfo, bool>)(x => Kerberos.Sots.StarFleet.StarFleet.CanDoInvasionMissionToTarget(game, systemId, x.ID, new float?(), new float?(), new float?())));
					break;
				case MissionType.GATE:
					if (game.GameDatabase.CanGate(playerId, systemId))
						return fleetInfosByPlayerId.Where<FleetInfo>((Func<FleetInfo, bool>)(x => Kerberos.Sots.StarFleet.StarFleet.CanDoGateMissionToTarget(game, systemId, x.ID, new float?(), new float?(), new float?())));
					break;
				case MissionType.PIRACY:
					if (game.GameDatabase.CanPirate(playerId, systemId))
						return fleetInfosByPlayerId.Where<FleetInfo>((Func<FleetInfo, bool>)(x => Kerberos.Sots.StarFleet.StarFleet.CanDoPiracyMissionToTarget(game, systemId, x.ID, new float?(), new float?(), new float?())));
					break;
				case MissionType.DEPLOY_NPG:
					if (faction.CanUseAccelerators())
						return fleetInfosByPlayerId.Where<FleetInfo>((Func<FleetInfo, bool>)(x => Kerberos.Sots.StarFleet.StarFleet.CanDoDeployNPGToTarget(game, systemId, x.ID, new float?(), new float?(), new float?())));
					break;
				case MissionType.EVACUATE:
					return fleetInfosByPlayerId.Where<FleetInfo>((Func<FleetInfo, bool>)(x =>
				   {
					   if (!Kerberos.Sots.StarFleet.StarFleet.CanDoEvacuationMissionToTarget(game, systemId, x.ID, x.SupportingSystemID, new float?(), new float?(), new float?()))
						   return factionName == "loa";
					   return true;
				   }));
				case MissionType.SPECIAL_CONSTRUCT_STN:
					return fleetInfosByPlayerId.Where<FleetInfo>((Func<FleetInfo, bool>)(x => (double)Kerberos.Sots.StarFleet.StarFleet.GetConstructionPointsForFleet(game, x.ID) > 0.0));
				case MissionType.REACTION:
					return Enumerable.Empty<FleetInfo>();
				default:
					return fleetInfosByPlayerId.Where<FleetInfo>((Func<FleetInfo, bool>)(x => game.GameDatabase.GetMissionByFleetID(x.ID) == null));
			}
			return Enumerable.Empty<FleetInfo>();
		}

		public static int CalculateRetrofitCost(
		  App App,
		  DesignInfo olddesign,
		  DesignInfo RetrofitDesign)
		{
			IEnumerable<DesignInfo> designInfosForPlayer = App.GameDatabase.GetVisibleDesignInfosForPlayer(App.LocalPlayer.ID);
			int num1 = RetrofitDesign.RetrofitBaseID == 0 ? Kerberos.Sots.StarFleet.StarFleet.RetrofitCostMultiplier(olddesign, designInfosForPlayer) + 1 : Kerberos.Sots.StarFleet.StarFleet.RetrofitCostMultiplier(RetrofitDesign, designInfosForPlayer);
			int num2 = 0;
			foreach (DesignSectionInfo designSection in RetrofitDesign.DesignSections)
			{
				DesignSectionInfo desi = designSection;
				DesignSectionInfo designSectionInfo = ((IEnumerable<DesignSectionInfo>)olddesign.DesignSections).FirstOrDefault<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.FilePath == desi.FilePath));
				foreach (WeaponBankInfo weaponBank in desi.WeaponBanks)
				{
					WeaponBankInfo wbi = weaponBank;
					if (designSectionInfo.WeaponBanks.Any<WeaponBankInfo>((Func<WeaponBankInfo, bool>)(j =>
				   {
					   if (!(j.BankGUID == wbi.BankGUID))
						   return false;
					   int? weaponId1 = j.WeaponID;
					   int? weaponId2 = wbi.WeaponID;
					   if ((weaponId1.GetValueOrDefault() != weaponId2.GetValueOrDefault() ? 1 : (weaponId1.HasValue != weaponId2.HasValue ? 1 : 0)) != 0 && !j.DesignID.HasValue)
						   return true;
					   int? designId1 = j.DesignID;
					   int? designId2 = wbi.DesignID;
					   if (designId1.GetValueOrDefault() == designId2.GetValueOrDefault())
						   return designId1.HasValue != designId2.HasValue;
					   return true;
				   })))
					{
						string weaponFile = App.GameDatabase.GetWeaponAsset(wbi.WeaponID.Value);
						LogicalWeapon logicalWeapon = App.Game.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.FileName == weaponFile));
						if (logicalWeapon != null)
							num2 += logicalWeapon.Cost * 2 * num1;
						if (wbi.DesignID.HasValue)
						{
							WeaponBankInfo weaponBankInfo = designSectionInfo.WeaponBanks.FirstOrDefault<WeaponBankInfo>((Func<WeaponBankInfo, bool>)(x => x.BankGUID == wbi.BankGUID));
							LogicalBank lb = ((IEnumerable<LogicalBank>)desi.ShipSectionAsset.Banks).FirstOrDefault<LogicalBank>((Func<LogicalBank, bool>)(x => x.GUID == wbi.BankGUID));
							int num3 = ((IEnumerable<LogicalMount>)desi.ShipSectionAsset.Mounts).Where<LogicalMount>((Func<LogicalMount, bool>)(x => x.Bank == lb)).Count<LogicalMount>();
							int num4 = num3 > 0 ? num3 : 1;
							DesignInfo designInfo1 = App.GameDatabase.GetDesignInfo(wbi.DesignID.Value);
							DesignInfo designInfo2 = App.GameDatabase.GetDesignInfo(weaponBankInfo.DesignID.Value);
							int num5 = designInfo1.SavingsCost - designInfo2.SavingsCost;
							int num6 = num5 >= 0 ? num5 : 0;
							if (designInfo1 != null)
								num2 += num6 * num1 * num4;
						}
					}
					else if (!designSectionInfo.WeaponBanks.Any<WeaponBankInfo>((Func<WeaponBankInfo, bool>)(j => j.BankGUID == wbi.BankGUID)))
					{
						string weaponFile = App.GameDatabase.GetWeaponAsset(wbi.WeaponID.Value);
						LogicalWeapon logicalWeapon = App.Game.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.FileName == weaponFile));
						if (logicalWeapon != null)
							num2 += logicalWeapon.Cost * 2 * num1;
						if (wbi.DesignID.HasValue)
						{
							DesignInfo designInfo = App.GameDatabase.GetDesignInfo(wbi.DesignID.Value);
							if (designInfo != null)
								num2 += designInfo.ProductionCost * num1;
						}
					}
				}
				foreach (DesignModuleInfo module in desi.Modules)
				{
					DesignModuleInfo dmi = module;
					if (designSectionInfo.Modules.Any<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(j =>
				   {
					   if (j.MountNodeName == dmi.MountNodeName)
						   return j.ModuleID != dmi.ModuleID;
					   return false;
				   })))
					{
						string moduleAsset = App.GameDatabase.GetModuleAsset(dmi.ModuleID);
						LogicalModule logicalModule = App.AssetDatabase.Modules.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == moduleAsset));
						if (logicalModule != null)
							num2 += logicalModule.SavingsCost * 2 * num1;
					}
					else if (!designSectionInfo.Modules.Any<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(j => j.MountNodeName == dmi.MountNodeName)))
					{
						string moduleAsset = App.GameDatabase.GetModuleAsset(dmi.ModuleID);
						LogicalModule logicalModule = App.AssetDatabase.Modules.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == moduleAsset));
						if (logicalModule != null)
							num2 += logicalModule.SavingsCost * 2 * num1;
					}
				}
			}
			return num2;
		}

		public static int CalculateStationRetrofitCost(
		  App App,
		  DesignInfo olddesign,
		  DesignInfo RetrofitDesign)
		{
			int num = 0;
			foreach (DesignModuleInfo module in RetrofitDesign.DesignSections[0].Modules)
			{
				DesignModuleInfo dmi = module;
				DesignModuleInfo designModuleInfo = olddesign.DesignSections[0].Modules.FirstOrDefault<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(x => x.MountNodeName == dmi.MountNodeName));
				if (designModuleInfo != null)
				{
					int? weaponId1 = designModuleInfo.WeaponID;
					int? weaponId2 = dmi.WeaponID;
					if ((weaponId1.GetValueOrDefault() != weaponId2.GetValueOrDefault() ? 1 : (weaponId1.HasValue != weaponId2.HasValue ? 1 : 0)) != 0)
					{
						string weaponFile = App.GameDatabase.GetWeaponAsset(dmi.WeaponID.Value);
						LogicalWeapon logicalWeapon = App.Game.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.FileName == weaponFile));
						if (logicalWeapon != null)
							num += logicalWeapon.Cost;
					}
				}
			}
			return num;
		}

		public static bool CanRetrofitStation(App App, int shipid)
		{
			if (App.GameDatabase.GetStationRetrofitOrders().ToList<StationRetrofitOrderInfo>().Any<StationRetrofitOrderInfo>((Func<StationRetrofitOrderInfo, bool>)(x => x.ShipID == shipid)))
				return false;
			ShipInfo shipInfo = App.GameDatabase.GetShipInfo(shipid, true);
			if (App.GameDatabase.GetQueuedStationModules(shipInfo.DesignInfo.DesignSections[0]).ToList<DesignModuleInfo>().Any<DesignModuleInfo>() || shipInfo.DesignInfo.StationType == StationType.INVALID_TYPE)
				return false;
			foreach (DesignModuleInfo module in shipInfo.DesignInfo.DesignSections[0].Modules)
			{
				string moduleass = App.GameDatabase.GetModuleAsset(module.ModuleID);
				if (((IEnumerable<LogicalBank>)App.AssetDatabase.Modules.First<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == moduleass)).Banks).Count<LogicalBank>() > 0)
					return true;
			}
			return false;
		}

		public static int RetrofitCostMultiplier(
		  DesignInfo RetrofitDesign,
		  IEnumerable<DesignInfo> designs)
		{
			DesignInfo NewestDesign = Kerberos.Sots.StarFleet.StarFleet.GetNewestRetrofitDesign(RetrofitDesign, designs);
			int num = 0;
			while (NewestDesign != null)
			{
				NewestDesign = designs.FirstOrDefault<DesignInfo>((Func<DesignInfo, bool>)(x => x.ID == NewestDesign.RetrofitBaseID));
				++num;
			}
			return num;
		}

		public static bool IsNewestRetrofit(DesignInfo design, IEnumerable<DesignInfo> designs)
		{
			return designs.Where<DesignInfo>((Func<DesignInfo, bool>)(x => x.RetrofitBaseID == design.ID)).Count<DesignInfo>() == 0;
		}

		public static DesignInfo GetNewestRetrofitDesign(
		  DesignInfo design,
		  IEnumerable<DesignInfo> designs)
		{
			if (design.ID != 0 && designs.Where<DesignInfo>((Func<DesignInfo, bool>)(x => x.RetrofitBaseID == design.ID)).Count<DesignInfo>() != 0)
				return Kerberos.Sots.StarFleet.StarFleet.GetNewestRetrofitDesign(designs.Where<DesignInfo>((Func<DesignInfo, bool>)(x => x.RetrofitBaseID == design.ID)).First<DesignInfo>(), designs);
			return design;
		}

		public static DesignInfo GetRetrofitBaseDesign(
		  DesignInfo design,
		  IEnumerable<DesignInfo> designs)
		{
			if (design.RetrofitBaseID == 0)
				return design;
			return Kerberos.Sots.StarFleet.StarFleet.GetRetrofitBaseDesign(designs.Where<DesignInfo>((Func<DesignInfo, bool>)(x => x.RetrofitBaseID == design.ID)).First<DesignInfo>(), designs);
		}

		public static double GetTimeRequiredToRetrofit(App App, ShipInfo shipinfo, int numships)
		{
			if (numships == 0)
				return 0.0;
			FleetInfo fleetInfo = App.GameDatabase.GetFleetInfo(shipinfo.FleetID);
			if (fleetInfo == null)
				return 0.0;
			StationInfo forSystemAndPlayer = App.GameDatabase.GetNavalStationForSystemAndPlayer(fleetInfo.SystemID, App.LocalPlayer.ID);
			if (forSystemAndPlayer == null)
				return 0.0;
			int num = 0;
			foreach (DesignSectionInfo designSection in forSystemAndPlayer.DesignInfo.DesignSections)
				num += designSection.Modules.Where<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(x =>
			   {
				   ModuleEnums.StationModuleType? stationModuleType = x.StationModuleType;
				   if (stationModuleType.GetValueOrDefault() == ModuleEnums.StationModuleType.Dock)
					   return stationModuleType.HasValue;
				   return false;
			   })).Count<DesignModuleInfo>();
			return Math.Ceiling((double)numships / (double)num);
		}

		public static bool SystemSupportsRetrofitting(App App, int systemID, int playerid)
		{
			if (!App.GameDatabase.GetColonyInfosForSystem(systemID).ToList<ColonyInfo>().Any<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == playerid)))
				return false;
			StationInfo forSystemAndPlayer = App.GameDatabase.GetNavalStationForSystemAndPlayer(systemID, playerid);
			if (forSystemAndPlayer == null)
				return false;
			int num = 0;
			foreach (DesignSectionInfo designSection in forSystemAndPlayer.DesignInfo.DesignSections)
				num += designSection.Modules.Where<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(x =>
			   {
				   ModuleEnums.StationModuleType? stationModuleType = x.StationModuleType;
				   if (stationModuleType.GetValueOrDefault() == ModuleEnums.StationModuleType.Dock)
					   return stationModuleType.HasValue;
				   return false;
			   })).Count<DesignModuleInfo>();
			if (num > 0)
				return forSystemAndPlayer.DesignInfo.StationLevel >= 3;
			return false;
		}

		public static int GetSystemRetrofitCapacity(App App, int systemID, int playerId)
		{
			StationInfo forSystemAndPlayer = App.GameDatabase.GetNavalStationForSystemAndPlayer(systemID, playerId);
			if (forSystemAndPlayer == null)
				return 0;
			int num = 0;
			foreach (DesignSectionInfo designSection in forSystemAndPlayer.DesignInfo.DesignSections)
				num += designSection.Modules.Where<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(x =>
			   {
				   ModuleEnums.StationModuleType? stationModuleType = x.StationModuleType;
				   if (stationModuleType.GetValueOrDefault() == ModuleEnums.StationModuleType.Dock)
					   return stationModuleType.HasValue;
				   return false;
			   })).Count<DesignModuleInfo>();
			return num;
		}

		public static int GetFleetCommandPoints(App App, int fleetid, IEnumerable<int> excludeShips = null)
		{
			int num = 0;
			foreach (ShipInfo shipInfo in App.GameDatabase.GetShipInfoByFleetID(fleetid, true).ToList<ShipInfo>())
			{
				ShipInfo inf = shipInfo;
				if (excludeShips == null || !excludeShips.Any<int>((Func<int, bool>)(x => x == inf.ID)))
					num += inf.DesignInfo.GetCommandPoints();
			}
			return num;
		}

		public static int GetFleetCommandCost(App App, int fleetid, IEnumerable<int> excludeShips = null)
		{
			int num = 0;
			foreach (ShipInfo shipInfo in App.GameDatabase.GetShipInfoByFleetID(fleetid, true).ToList<ShipInfo>())
			{
				ShipInfo inf = shipInfo;
				if (excludeShips == null || !excludeShips.Any<int>((Func<int, bool>)(x => x == inf.ID)))
					num += inf.DesignInfo.CommandPointCost;
			}
			return num;
		}

		public static bool FleetCanFunctionWithoutShips(
		  App App,
		  int fleetid,
		  IEnumerable<int> excludeShips)
		{
			FleetInfo fleetInfo = App.GameDatabase.GetFleetInfo(fleetid);
			if (fleetInfo == null)
				return false;
			if (fleetInfo.Type == FleetType.FL_RESERVE)
				return true;
			App.GameDatabase.GetShipInfoByFleetID(fleetid, true).ToList<ShipInfo>();
			int fleetCommandPoints = Kerberos.Sots.StarFleet.StarFleet.GetFleetCommandPoints(App, fleetid, excludeShips);
			if (Kerberos.Sots.StarFleet.StarFleet.GetFleetCommandPoints(App, fleetid, (IEnumerable<int>)null) <= fleetCommandPoints)
				return true;
			int fleetCommandCost = Kerberos.Sots.StarFleet.StarFleet.GetFleetCommandCost(App, fleetid, excludeShips);
			return fleetCommandPoints >= fleetCommandCost;
		}

		public static bool FleetCanFunctionWithoutShip(App App, int fleetid, int shipid)
		{
			return Kerberos.Sots.StarFleet.StarFleet.FleetCanFunctionWithoutShips(App, fleetid, (IEnumerable<int>)new List<int>()
	  {
		shipid
	  });
		}

		private static CombatZonePositionInfo GetZoneFromPosition(
		  App App,
		  Vector3 Position,
		  Vector3 Origin,
		  List<CombatZonePositionInfo> cz)
		{
			float num = (float)Math.Abs((Math.Atan2((double)Position.Z, (double)Position.X) + 4.0 * Math.PI) % (2.0 * Math.PI));
			float length = (Position - Origin).Length;
			foreach (CombatZonePositionInfo zonePositionInfo in cz)
			{
				if ((double)length >= (double)zonePositionInfo.RadiusLower && (double)length <= (double)zonePositionInfo.RadiusUpper && ((double)num >= (double)zonePositionInfo.AngleLeft && (double)num <= (double)zonePositionInfo.AngleRight))
					return zonePositionInfo;
			}
			return (CombatZonePositionInfo)null;
		}

		private static Vector3 PickRandomPositionAroundOrigin(
		  App App,
		  Vector3 Origin,
		  int distance)
		{
			float num = (float)new Random().Next(360);
			Vector3 vector3 = new Vector3((float)Math.Cos((double)num), 0.0f, (float)Math.Sin((double)num));
			return (Origin - vector3) * (float)distance;
		}

		private static bool CanPlacePlatformInZone(
		  App App,
		  List<ShipInfo> PlacedDefShips,
		  List<CombatZonePositionInfo> combatzones,
		  ShipInfo Ship,
		  Matrix Position)
		{
			CombatZonePositionInfo zoneFromPosition1 = Kerberos.Sots.StarFleet.StarFleet.GetZoneFromPosition(App, Position.Position, new Vector3(0.0f, 0.0f, 0.0f), combatzones);
			if (zoneFromPosition1 == null)
				return false;
			int num = zoneFromPosition1.RingIndex == 0 ? 1 : (zoneFromPosition1.RingIndex == 2 ? 3 : 2);
			foreach (ShipInfo placedDefShip in PlacedDefShips)
			{
				if (placedDefShip != Ship && placedDefShip.ShipSystemPosition.HasValue)
				{
					CombatZonePositionInfo zoneFromPosition2 = Kerberos.Sots.StarFleet.StarFleet.GetZoneFromPosition(App, placedDefShip.ShipSystemPosition.Value.Position, new Vector3(0.0f, 0.0f, 0.0f), combatzones);
					if (zoneFromPosition2 != null)
					{
						if (zoneFromPosition2.RingIndex == zoneFromPosition1.RingIndex && zoneFromPosition2.ZoneIndex == zoneFromPosition1.ZoneIndex)
							--num;
						if (num == 0)
							return false;
					}
				}
			}
			return true;
		}

		private static bool CanPlaceAsset(
		  App App,
		  List<ShipInfo> PlacedDefShips,
		  List<PlanetInfo> planets,
		  ShipInfo Ship,
		  Matrix Position)
		{
			foreach (PlanetInfo planet in planets)
			{
				Matrix orbitalTransform = App.GameDatabase.GetOrbitalTransform(planet.ID);
				StellarBody.Params stellarBodyParams = App.AssetDatabase.PlanetGenerationRules.GetStellarBodyParams(App.Game, planet.ID);
				if ((double)(Position.Position - orbitalTransform.Position).Length < (double)stellarBodyParams.Radius + 6000.0)
					return false;
			}
			foreach (ShipInfo placedDefShip in PlacedDefShips)
			{
				if (placedDefShip != Ship && placedDefShip.ShipSystemPosition.HasValue && (double)(placedDefShip.ShipSystemPosition.Value.Position - Position.Position).Length < 5000.0)
					return false;
			}
			return true;
		}

		private static bool AutoPlaceSDB(App App, ShipInfo DefAsset, StarSystemInfo ssi)
		{
			FleetInfo fi = App.GameDatabase.GetFleetInfo(DefAsset.FleetID);
			if (fi == null)
				return false;
			List<CombatZonePositionInfo> combatZonesForSystem = Kerberos.Sots.GameStates.StarSystem.GetCombatZonesForSystem(App.Game, ssi.ID, 1f);
			if (combatZonesForSystem == null || combatZonesForSystem.Count == 0)
				return false;
			List<PlanetInfo> list1 = ((IEnumerable<PlanetInfo>)App.GameDatabase.GetStarSystemPlanetInfos(ssi.ID)).ToList<PlanetInfo>();
			List<ColonyInfo> colonies = App.GameDatabase.GetColonyInfosForSystem(ssi.ID).Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == fi.PlayerID)).ToList<ColonyInfo>();
			List<ColonyInfo> list2 = colonies.Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == fi.PlayerID)).ToList<ColonyInfo>().OrderBy<ColonyInfo, double>((Func<ColonyInfo, double>)(x => App.GameDatabase.GetTotalPopulation(x))).Reverse<ColonyInfo>().ToList<ColonyInfo>();
			Random random = new Random();
			List<ShipInfo> shipInfoList = new List<ShipInfo>();
			int? defenseFleetId = App.GameDatabase.GetDefenseFleetID(ssi.ID, fi.PlayerID);
			if (defenseFleetId.HasValue)
				App.GameDatabase.GetShipInfoByFleetID(defenseFleetId.Value, false).Where<ShipInfo>((Func<ShipInfo, bool>)(x => x.ShipSystemPosition.HasValue)).ToList<ShipInfo>();
			int num1 = 5;
			if (list2.Count > 0)
			{
				for (int index = 0; index < num1 * 2; ++index)
				{
					ColonyInfo colonyInfo = list2[random.Next(list2.Count)];
					if (colonyInfo != null)
					{
						Matrix orbitalTransform = App.GameDatabase.GetOrbitalTransform(colonyInfo.OrbitalObjectID);
						Vector3 position = orbitalTransform.Position;
						StellarBody.Params stellarBodyParams = App.AssetDatabase.PlanetGenerationRules.GetStellarBodyParams(App.Game, colonyInfo.OrbitalObjectID);
						Matrix matrix = Matrix.CreateTranslation(Kerberos.Sots.StarFleet.StarFleet.PickRandomPositionAroundOrigin(App, new Vector3(0.0f, 0.0f, 0.0f), (int)((double)stellarBodyParams.Radius + 6000.0))) * orbitalTransform;
						PlanetInfo planetInfo = App.GameDatabase.GetPlanetInfo(colonyInfo.OrbitalObjectID);
						int num2 = 1;
						if (App.AssetDatabase.IsGasGiant(planetInfo.Type))
							num2 = 3;
						else if (App.AssetDatabase.IsPotentialyHabitable(planetInfo.Type))
							num2 = 2;
						List<SDBInfo> list3 = App.GameDatabase.GetSDBInfoFromOrbital(planetInfo.ID).ToList<SDBInfo>();
						if (num2 - list3.Count > 0)
						{
							App.GameDatabase.UpdateShipSystemPosition(DefAsset.ID, new Matrix?(matrix));
							App.GameDatabase.InsertSDB(planetInfo.ID, DefAsset.ID);
							return true;
						}
					}
				}
			}
			List<PlanetInfo> list4 = list1.Where<PlanetInfo>((Func<PlanetInfo, bool>)(x => !colonies.Any<ColonyInfo>((Func<ColonyInfo, bool>)(j => j.OrbitalObjectID == x.ID)))).ToList<PlanetInfo>();
			if (list4.Count > 0)
			{
				for (int index = 0; index < num1; ++index)
				{
					PlanetInfo planetInfo = list4[random.Next(list4.Count)];
					if (planetInfo != null)
					{
						StellarBody.Params stellarBodyParams = App.AssetDatabase.PlanetGenerationRules.GetStellarBodyParams(App.Game, planetInfo.ID);
						Matrix orbitalTransform = App.GameDatabase.GetOrbitalTransform(planetInfo.ID);
						Vector3 position = orbitalTransform.Position;
						Matrix matrix = Matrix.CreateTranslation(Kerberos.Sots.StarFleet.StarFleet.PickRandomPositionAroundOrigin(App, new Vector3(0.0f, 0.0f, 0.0f), (int)((double)stellarBodyParams.Radius + 6000.0))) * orbitalTransform;
						int num2 = 1;
						if (App.AssetDatabase.IsGasGiant(planetInfo.Type))
							num2 = 3;
						else if (App.AssetDatabase.IsPotentialyHabitable(planetInfo.Type))
							num2 = 2;
						List<SDBInfo> list3 = App.GameDatabase.GetSDBInfoFromOrbital(planetInfo.ID).ToList<SDBInfo>();
						if (num2 - list3.Count > 0)
						{
							App.GameDatabase.UpdateShipSystemPosition(DefAsset.ID, new Matrix?(matrix));
							App.GameDatabase.InsertSDB(planetInfo.ID, DefAsset.ID);
							return true;
						}
					}
				}
			}
			return false;
		}

		private static bool AutoPlacePoliceShip(App App, ShipInfo DefAsset, StarSystemInfo ssi)
		{
			FleetInfo fi = App.GameDatabase.GetFleetInfo(DefAsset.FleetID);
			if (fi == null)
				return false;
			List<CombatZonePositionInfo> combatZonesForSystem = Kerberos.Sots.GameStates.StarSystem.GetCombatZonesForSystem(App.Game, ssi.ID, 1f);
			if (combatZonesForSystem == null || combatZonesForSystem.Count == 0)
				return false;
			List<PlanetInfo> list1 = ((IEnumerable<PlanetInfo>)App.GameDatabase.GetStarSystemPlanetInfos(ssi.ID)).ToList<PlanetInfo>();
			List<ColonyInfo> colonies = App.GameDatabase.GetColonyInfosForSystem(ssi.ID).Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == fi.PlayerID)).ToList<ColonyInfo>();
			List<ColonyInfo> list2 = colonies.Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == fi.PlayerID)).ToList<ColonyInfo>().OrderBy<ColonyInfo, double>((Func<ColonyInfo, double>)(x => App.GameDatabase.GetTotalPopulation(x))).Reverse<ColonyInfo>().ToList<ColonyInfo>();
			Random random = new Random();
			List<ShipInfo> PlacedDefShips = new List<ShipInfo>();
			int? defenseFleetId = App.GameDatabase.GetDefenseFleetID(ssi.ID, fi.PlayerID);
			if (defenseFleetId.HasValue)
				PlacedDefShips = App.GameDatabase.GetShipInfoByFleetID(defenseFleetId.Value, false).Where<ShipInfo>((Func<ShipInfo, bool>)(x => x.ShipSystemPosition.HasValue)).ToList<ShipInfo>();
			int num1 = 5;
			if (list2.Count > 0)
			{
				for (int index = 0; index < num1 * 2; ++index)
				{
					ColonyInfo colonyInfo = list2[random.Next(list2.Count)];
					if (colonyInfo != null)
					{
						Matrix orbitalTransform = App.GameDatabase.GetOrbitalTransform(colonyInfo.OrbitalObjectID);
						Vector3 position = orbitalTransform.Position;
						int num2 = random.Next(9000);
						StellarBody.Params stellarBodyParams = App.AssetDatabase.PlanetGenerationRules.GetStellarBodyParams(App.Game, colonyInfo.OrbitalObjectID);
						Matrix Position = Matrix.CreateTranslation(Kerberos.Sots.StarFleet.StarFleet.PickRandomPositionAroundOrigin(App, new Vector3(0.0f, 0.0f, 0.0f), (int)((double)stellarBodyParams.Radius + 6000.0 + (double)num2))) * orbitalTransform;
						if (Kerberos.Sots.StarFleet.StarFleet.CanPlaceAsset(App, PlacedDefShips, list1, DefAsset, Position))
						{
							App.GameDatabase.UpdateShipSystemPosition(DefAsset.ID, new Matrix?(Position));
							return true;
						}
					}
				}
			}
			List<PlanetInfo> list3 = list1.Where<PlanetInfo>((Func<PlanetInfo, bool>)(x => !colonies.Any<ColonyInfo>((Func<ColonyInfo, bool>)(j => j.OrbitalObjectID == x.ID)))).ToList<PlanetInfo>();
			if (list3.Count > 0)
			{
				for (int index = 0; index < num1; ++index)
				{
					PlanetInfo planetInfo = list3[random.Next(list3.Count)];
					if (planetInfo != null)
					{
						StellarBody.Params stellarBodyParams = App.AssetDatabase.PlanetGenerationRules.GetStellarBodyParams(App.Game, planetInfo.ID);
						Matrix orbitalTransform = App.GameDatabase.GetOrbitalTransform(planetInfo.ID);
						Vector3 position = orbitalTransform.Position;
						int num2 = random.Next(9000);
						Matrix Position = Matrix.CreateTranslation(Kerberos.Sots.StarFleet.StarFleet.PickRandomPositionAroundOrigin(App, new Vector3(0.0f, 0.0f, 0.0f), (int)((double)stellarBodyParams.Radius + 6000.0 + (double)num2))) * orbitalTransform;
						if (Kerberos.Sots.StarFleet.StarFleet.CanPlaceAsset(App, PlacedDefShips, list1, DefAsset, Position))
						{
							App.GameDatabase.UpdateShipSystemPosition(DefAsset.ID, new Matrix?(Position));
							return true;
						}
					}
				}
			}
			return false;
		}

		private static bool AutoPlaceMinefield(App App, ShipInfo DefAsset, StarSystemInfo ssi)
		{
			FleetInfo fi = App.GameDatabase.GetFleetInfo(DefAsset.FleetID);
			if (fi == null)
				return false;
			List<CombatZonePositionInfo> combatZonesForSystem = Kerberos.Sots.GameStates.StarSystem.GetCombatZonesForSystem(App.Game, ssi.ID, 1f);
			if (combatZonesForSystem == null || combatZonesForSystem.Count == 0)
				return false;
			List<PlanetInfo> list1 = ((IEnumerable<PlanetInfo>)App.GameDatabase.GetStarSystemPlanetInfos(ssi.ID)).ToList<PlanetInfo>();
			List<ColonyInfo> colonies = App.GameDatabase.GetColonyInfosForSystem(ssi.ID).Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == fi.PlayerID)).ToList<ColonyInfo>();
			List<ColonyInfo> list2 = colonies.Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == fi.PlayerID)).ToList<ColonyInfo>().OrderBy<ColonyInfo, double>((Func<ColonyInfo, double>)(x => App.GameDatabase.GetTotalPopulation(x))).Reverse<ColonyInfo>().ToList<ColonyInfo>();
			Random random = new Random();
			List<ShipInfo> PlacedDefShips = new List<ShipInfo>();
			int? defenseFleetId = App.GameDatabase.GetDefenseFleetID(ssi.ID, fi.PlayerID);
			if (defenseFleetId.HasValue)
				PlacedDefShips = App.GameDatabase.GetShipInfoByFleetID(defenseFleetId.Value, false).Where<ShipInfo>((Func<ShipInfo, bool>)(x => x.ShipSystemPosition.HasValue)).ToList<ShipInfo>();
			int num1 = 5;
			if (list2.Count > 0)
			{
				for (int index = 0; index < num1 * 2; ++index)
				{
					ColonyInfo colonyInfo = list2[random.Next(list2.Count)];
					if (colonyInfo != null)
					{
						Matrix orbitalTransform = App.GameDatabase.GetOrbitalTransform(colonyInfo.OrbitalObjectID);
						Vector3 position = orbitalTransform.Position;
						int num2 = random.Next(8000);
						StellarBody.Params stellarBodyParams = App.AssetDatabase.PlanetGenerationRules.GetStellarBodyParams(App.Game, colonyInfo.OrbitalObjectID);
						Matrix Position = Matrix.CreateTranslation(Kerberos.Sots.StarFleet.StarFleet.PickRandomPositionAroundOrigin(App, new Vector3(0.0f, 0.0f, 0.0f), (int)((double)stellarBodyParams.Radius + 6000.0 + (double)num2))) * orbitalTransform;
						if (Kerberos.Sots.StarFleet.StarFleet.CanPlaceAsset(App, PlacedDefShips, list1, DefAsset, Position))
						{
							App.GameDatabase.UpdateShipSystemPosition(DefAsset.ID, new Matrix?(Position));
							return true;
						}
					}
				}
			}
			List<PlanetInfo> list3 = list1.Where<PlanetInfo>((Func<PlanetInfo, bool>)(x => !colonies.Any<ColonyInfo>((Func<ColonyInfo, bool>)(j => j.OrbitalObjectID == x.ID)))).ToList<PlanetInfo>();
			if (list3.Count > 0)
			{
				for (int index = 0; index < num1; ++index)
				{
					PlanetInfo planetInfo = list3[random.Next(list3.Count)];
					if (planetInfo != null)
					{
						StellarBody.Params stellarBodyParams = App.AssetDatabase.PlanetGenerationRules.GetStellarBodyParams(App.Game, planetInfo.ID);
						Matrix orbitalTransform = App.GameDatabase.GetOrbitalTransform(planetInfo.ID);
						Vector3 position = orbitalTransform.Position;
						int num2 = random.Next(5000);
						Matrix Position = Matrix.CreateTranslation(Kerberos.Sots.StarFleet.StarFleet.PickRandomPositionAroundOrigin(App, new Vector3(0.0f, 0.0f, 0.0f), (int)((double)stellarBodyParams.Radius + 6000.0 + (double)num2))) * orbitalTransform;
						if (Kerberos.Sots.StarFleet.StarFleet.CanPlaceAsset(App, PlacedDefShips, list1, DefAsset, Position))
						{
							App.GameDatabase.UpdateShipSystemPosition(DefAsset.ID, new Matrix?(Position));
							return true;
						}
					}
				}
			}
			return false;
		}

		private static bool AutoPlacePlatform(App App, ShipInfo DefAsset, StarSystemInfo ssi)
		{
			FleetInfo fi = App.GameDatabase.GetFleetInfo(DefAsset.FleetID);
			if (fi == null)
				return false;
			List<CombatZonePositionInfo> combatZonesForSystem = Kerberos.Sots.GameStates.StarSystem.GetCombatZonesForSystem(App.Game, ssi.ID, 1f);
			if (combatZonesForSystem == null || combatZonesForSystem.Count == 0)
				return false;
			List<PlanetInfo> list1 = ((IEnumerable<PlanetInfo>)App.GameDatabase.GetStarSystemPlanetInfos(ssi.ID)).ToList<PlanetInfo>();
			List<ColonyInfo> colonies = App.GameDatabase.GetColonyInfosForSystem(ssi.ID).Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == fi.PlayerID)).ToList<ColonyInfo>();
			List<ColonyInfo> list2 = colonies.Where<ColonyInfo>((Func<ColonyInfo, bool>)(x => x.PlayerID == fi.PlayerID)).ToList<ColonyInfo>().OrderBy<ColonyInfo, double>((Func<ColonyInfo, double>)(x => App.GameDatabase.GetTotalPopulation(x))).Reverse<ColonyInfo>().ToList<ColonyInfo>();
			Random random = new Random();
			List<ShipInfo> PlacedDefShips = new List<ShipInfo>();
			int? defenseFleetId = App.GameDatabase.GetDefenseFleetID(ssi.ID, fi.PlayerID);
			if (defenseFleetId.HasValue)
				PlacedDefShips = App.GameDatabase.GetShipInfoByFleetID(defenseFleetId.Value, false).Where<ShipInfo>((Func<ShipInfo, bool>)(x => x.ShipSystemPosition.HasValue)).ToList<ShipInfo>();
			int num1 = 5;
			if (list2.Count > 0)
			{
				for (int index = 0; index < num1 * 2; ++index)
				{
					ColonyInfo colonyInfo = list2[random.Next(list2.Count)];
					if (colonyInfo != null)
					{
						Matrix orbitalTransform = App.GameDatabase.GetOrbitalTransform(colonyInfo.OrbitalObjectID);
						Vector3 position = orbitalTransform.Position;
						int num2 = random.Next(5000);
						StellarBody.Params stellarBodyParams = App.AssetDatabase.PlanetGenerationRules.GetStellarBodyParams(App.Game, colonyInfo.OrbitalObjectID);
						Matrix Position = Matrix.CreateTranslation(Kerberos.Sots.StarFleet.StarFleet.PickRandomPositionAroundOrigin(App, new Vector3(0.0f, 0.0f, 0.0f), (int)((double)stellarBodyParams.Radius + 6000.0 + (double)num2))) * orbitalTransform;
						if (Kerberos.Sots.StarFleet.StarFleet.CanPlacePlatformInZone(App, PlacedDefShips, combatZonesForSystem, DefAsset, Position) && Kerberos.Sots.StarFleet.StarFleet.CanPlaceAsset(App, PlacedDefShips, list1, DefAsset, Position))
						{
							App.GameDatabase.UpdateShipSystemPosition(DefAsset.ID, new Matrix?(Position));
							return true;
						}
					}
				}
			}
			List<PlanetInfo> list3 = list1.Where<PlanetInfo>((Func<PlanetInfo, bool>)(x => !colonies.Any<ColonyInfo>((Func<ColonyInfo, bool>)(j => j.OrbitalObjectID == x.ID)))).ToList<PlanetInfo>();
			if (list3.Count > 0)
			{
				for (int index = 0; index < num1; ++index)
				{
					PlanetInfo planetInfo = list3[random.Next(list3.Count)];
					if (planetInfo != null)
					{
						StellarBody.Params stellarBodyParams = App.AssetDatabase.PlanetGenerationRules.GetStellarBodyParams(App.Game, planetInfo.ID);
						Matrix orbitalTransform = App.GameDatabase.GetOrbitalTransform(planetInfo.ID);
						Vector3 position = orbitalTransform.Position;
						int num2 = random.Next(5000);
						Matrix Position = Matrix.CreateTranslation(Kerberos.Sots.StarFleet.StarFleet.PickRandomPositionAroundOrigin(App, new Vector3(0.0f, 0.0f, 0.0f), (int)((double)stellarBodyParams.Radius + 6000.0 + (double)num2))) * orbitalTransform;
						if (Kerberos.Sots.StarFleet.StarFleet.CanPlacePlatformInZone(App, PlacedDefShips, combatZonesForSystem, DefAsset, Position) && Kerberos.Sots.StarFleet.StarFleet.CanPlaceAsset(App, PlacedDefShips, list1, DefAsset, Position))
						{
							App.GameDatabase.UpdateShipSystemPosition(DefAsset.ID, new Matrix?(Position));
							return true;
						}
					}
				}
			}
			return false;
		}

		public static bool AutoPlaceDefenseAsset(App App, int shipid, int systemid)
		{
			ShipInfo shipInfo = App.GameDatabase.GetShipInfo(shipid, false);
			StarSystemInfo starSystemInfo = App.GameDatabase.GetStarSystemInfo(systemid);
			if (shipInfo == null || starSystemInfo == (StarSystemInfo)null)
				return false;
			if (shipInfo.IsPlatform())
				return Kerberos.Sots.StarFleet.StarFleet.AutoPlacePlatform(App, shipInfo, starSystemInfo);
			if (shipInfo.IsMinelayer())
				return Kerberos.Sots.StarFleet.StarFleet.AutoPlaceMinefield(App, shipInfo, starSystemInfo);
			if (shipInfo.IsPoliceShip())
				return Kerberos.Sots.StarFleet.StarFleet.AutoPlacePoliceShip(App, shipInfo, starSystemInfo);
			if (shipInfo.IsSDB())
				return Kerberos.Sots.StarFleet.StarFleet.AutoPlaceSDB(App, shipInfo, starSystemInfo);
			return false;
		}

		public static string GetAdmiralAvatar(App App, int admiralid)
		{
			string str = "";
			AdmiralInfo admiralInfo = App.GameDatabase.GetAdmiralInfo(admiralid);
			if (admiralInfo != null)
			{
				str = string.Format("admiral_{0}", (object)admiralInfo.Race);
				if (admiralInfo.Gender == "female" && (admiralInfo.Race == "tarka" || admiralInfo.Race == "human" || admiralInfo.Race == "morrigi"))
					str += "2";
				if (admiralInfo.Engram)
					str = "admiral_robot";
			}
			return str;
		}

		public static int GetShipLoaCubeValue(GameSession game, int shipid)
		{
			ShipInfo shipInfo = game.GameDatabase.GetShipInfo(shipid, true);
			if (shipInfo != null)
			{
				RealShipClasses? realShipClass1 = shipInfo.DesignInfo.GetRealShipClass();
				if ((realShipClass1.GetValueOrDefault() != RealShipClasses.BoardingPod ? 0 : (realShipClass1.HasValue ? 1 : 0)) == 0)
				{
					RealShipClasses? realShipClass2 = shipInfo.DesignInfo.GetRealShipClass();
					if ((realShipClass2.GetValueOrDefault() != RealShipClasses.Drone ? 0 : (realShipClass2.HasValue ? 1 : 0)) == 0)
					{
						RealShipClasses? realShipClass3 = shipInfo.DesignInfo.GetRealShipClass();
						if ((realShipClass3.GetValueOrDefault() != RealShipClasses.AssaultShuttle ? 0 : (realShipClass3.HasValue ? 1 : 0)) == 0)
						{
							if (shipInfo.DesignInfo.IsLoaCube())
								return shipInfo.LoaCubes;
							int[] healthAndHealthMax = Kerberos.Sots.StarFleet.StarFleet.GetHealthAndHealthMax(game, shipInfo.DesignInfo, shipInfo.ID);
							float val1 = (float)healthAndHealthMax[0] / (float)healthAndHealthMax[1];
							return (int)((double)shipInfo.DesignInfo.GetPlayerProductionCost(game.GameDatabase, shipInfo.DesignInfo.PlayerID, !shipInfo.DesignInfo.isPrototyped, new float?()) * (double)Math.Min(Math.Max(val1, 0.0f), 1f));
						}
					}
				}
			}
			return 0;
		}

		public static int GetShipLoaCubeValue(GameSession game, DesignInfo design)
		{
			if (design != null)
			{
				RealShipClasses? realShipClass1 = design.GetRealShipClass();
				if ((realShipClass1.GetValueOrDefault() != RealShipClasses.BoardingPod ? 0 : (realShipClass1.HasValue ? 1 : 0)) == 0)
				{
					RealShipClasses? realShipClass2 = design.GetRealShipClass();
					if ((realShipClass2.GetValueOrDefault() != RealShipClasses.Drone ? 0 : (realShipClass2.HasValue ? 1 : 0)) == 0)
					{
						RealShipClasses? realShipClass3 = design.GetRealShipClass();
						if ((realShipClass3.GetValueOrDefault() != RealShipClasses.AssaultShuttle ? 0 : (realShipClass3.HasValue ? 1 : 0)) == 0 && !design.IsLoaCube())
							return design.GetPlayerProductionCost(game.GameDatabase, design.PlayerID, !design.isPrototyped, new float?());
					}
				}
			}
			return 0;
		}

		public static int GetFleetLoaCubeValue(GameSession game, int fleetid)
		{
			int num = 0;
			foreach (int shipid in game.GameDatabase.GetShipsByFleetID(fleetid))
				num += Kerberos.Sots.StarFleet.StarFleet.GetShipLoaCubeValue(game, shipid);
			return num;
		}

		public static int ConvertFleetIntoLoaCubes(GameSession game, int fleetid)
		{
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetid);
			if (Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(game, fleetInfo) || !game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerInfo(fleetInfo.PlayerID).FactionID).CanUseAccelerators())
				return 0;
			List<ShipInfo> list = game.GameDatabase.GetShipInfoByFleetID(fleetid, true).ToList<ShipInfo>();
			if (!list.Any<ShipInfo>())
				return 0;
			ShipInfo shipInfo1 = list.FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignInfo.IsLoaCube()));
			if (shipInfo1 == null)
			{
				int shipID = game.GameDatabase.InsertShip(fleetid, game.GameDatabase.GetDesignInfosForPlayer(fleetInfo.PlayerID).FirstOrDefault<DesignInfo>((Func<DesignInfo, bool>)(x => x.IsLoaCube())).ID, "Cube", (ShipParams)0, new int?(), 0);
				shipInfo1 = game.GameDatabase.GetShipInfo(shipID, false);
			}
			foreach (ShipInfo shipInfo2 in list)
			{
				if (shipInfo2.ID != shipInfo1.ID)
				{
					RealShipClasses? realShipClass1 = shipInfo2.DesignInfo.GetRealShipClass();
					if ((realShipClass1.GetValueOrDefault() != RealShipClasses.Drone ? 0 : (realShipClass1.HasValue ? 1 : 0)) == 0)
					{
						RealShipClasses? realShipClass2 = shipInfo2.DesignInfo.GetRealShipClass();
						if ((realShipClass2.GetValueOrDefault() != RealShipClasses.BoardingPod ? 0 : (realShipClass2.HasValue ? 1 : 0)) == 0)
						{
							RealShipClasses? realShipClass3 = shipInfo2.DesignInfo.GetRealShipClass();
							if ((realShipClass3.GetValueOrDefault() != RealShipClasses.AssaultShuttle ? 0 : (realShipClass3.HasValue ? 1 : 0)) == 0)
								shipInfo1.LoaCubes += Kerberos.Sots.StarFleet.StarFleet.GetShipLoaCubeValue(game, shipInfo2.ID);
						}
					}
				}
			}
			foreach (ShipInfo shipInfo2 in list)
			{
				if (shipInfo2.ID != shipInfo1.ID)
					game.GameDatabase.RemoveShip(shipInfo2.ID);
			}
			game.GameDatabase.UpdateShipLoaCubes(shipInfo1.ID, shipInfo1.LoaCubes);
			return shipInfo1.ID;
		}

		public static IEnumerable<DesignInfo> GetDesignBuildOrderForComposition(
		  GameSession game,
		  int fleetid,
		  LoaFleetComposition composition,
		  MissionType mission_type = MissionType.NO_MISSION)
		{
			MissionInfo missionByFleetId = game.GameDatabase.GetMissionByFleetID(fleetid);
			if (missionByFleetId != null)
				mission_type = missionByFleetId.Type;
			List<DesignInfo> source = new List<DesignInfo>();
			foreach (LoaFleetShipDef design in composition.designs)
			{
				DesignInfo designInfo = game.GameDatabase.GetDesignInfo(design.DesignID);
				if (designInfo != null)
					source.Add(designInfo);
			}
			DesignInfo designInfo1 = (DesignInfo)null;
			bool flag = false;
			foreach (DesignInfo designInfo2 in source)
			{
				if (designInfo2.GetCommandPoints() > 0 && (designInfo1 == null || designInfo2.GetCommandPoints() > designInfo1.GetCommandPoints() || !flag && designInfo2.isPrototyped))
				{
					designInfo1 = designInfo2;
					flag = designInfo2.isPrototyped;
				}
			}
			if (designInfo1 != null)
			{
				source.Remove(designInfo1);
				source.Insert(0, designInfo1);
			}
			if (mission_type != MissionType.NO_MISSION)
			{
				List<DesignInfo> designInfoList = new List<DesignInfo>();
				if (mission_type == MissionType.COLONIZATION || mission_type == MissionType.SUPPORT || mission_type == MissionType.EVACUATE)
					designInfoList = source.Where<DesignInfo>((Func<DesignInfo, bool>)(x => ((IEnumerable<DesignSectionInfo>)x.DesignSections).Any<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(j => j.ShipSectionAsset.ColonizationSpace > 0)))).ToList<DesignInfo>();
				if (mission_type == MissionType.CONSTRUCT_STN || mission_type == MissionType.UPGRADE_STN || mission_type == MissionType.SPECIAL_CONSTRUCT_STN)
					designInfoList = source.Where<DesignInfo>((Func<DesignInfo, bool>)(x => ((IEnumerable<DesignSectionInfo>)x.DesignSections).Any<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(j => j.ShipSectionAsset.isConstructor)))).ToList<DesignInfo>();
				foreach (DesignInfo designInfo2 in designInfoList)
					source.Remove(designInfo2);
				foreach (DesignInfo designInfo2 in designInfoList)
					source.Insert(1, designInfo2);
			}
			return (IEnumerable<DesignInfo>)source;
		}

		public static LoaFleetComposition ObtainFleetComposition(
		  GameSession game,
		  FleetInfo fleetInfo,
		  int? compositionid)
		{
			LoaFleetComposition fleetComposition = (LoaFleetComposition)null;
			if (compositionid.HasValue)
				fleetComposition = game.GameDatabase.GetLoaFleetCompositions().FirstOrDefault<LoaFleetComposition>((Func<LoaFleetComposition, bool>)(x => x.ID == compositionid.Value));
			if (!compositionid.HasValue)
			{
				PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(fleetInfo.PlayerID);
				if (game.GetPlayerObject(playerInfo.ID).IsAI())
				{
					AIFleetInfo aifi = game.GameDatabase.GetAIFleetInfos(playerInfo.ID).FirstOrDefault<AIFleetInfo>((Func<AIFleetInfo, bool>)(x =>
				   {
					   int? fleetId = x.FleetID;
					   int id = fleetInfo.ID;
					   if (fleetId.GetValueOrDefault() == id)
						   return fleetId.HasValue;
					   return false;
				   }));
					if (aifi != null)
					{
						List<LoaFleetShipDef> loaFleetShipDefList = new List<LoaFleetShipDef>();
						Dictionary<int, int> designsFromTemplate = game.GetFleetDesignsFromTemplate(game.GetPlayerObject(playerInfo.ID), aifi.FleetTemplate);
						foreach (int key in designsFromTemplate.Keys)
						{
							for (int index = 0; index < designsFromTemplate[key]; ++index)
								loaFleetShipDefList.Add(new LoaFleetShipDef()
								{
									DesignID = key
								});
						}
						FleetTemplate fleetTemplate = game.AssetDatabase.FleetTemplates.First<FleetTemplate>((Func<FleetTemplate, bool>)(x => x.Name == aifi.FleetTemplate));
						fleetComposition = new LoaFleetComposition();
						fleetComposition.Name = fleetTemplate.Name;
						fleetComposition.PlayerID = playerInfo.ID;
						fleetComposition.designs = loaFleetShipDefList;
					}
				}
			}
			return fleetComposition;
		}

		public static void BuildFleetFromCompositionID(
		  GameSession game,
		  int fleetid,
		  int? compositionid,
		  MissionType missionType = MissionType.NO_MISSION)
		{
			// ISSUE: object of a compiler-generated type is created
			// ISSUE: variable of a compiler-generated type
			//Kerberos.Sots.StarFleet.StarFleet.<> c__DisplayClass15d cDisplayClass15d1 = new Kerberos.Sots.StarFleet.StarFleet.<> c__DisplayClass15d();
			// ISSUE: reference to a compiler-generated field
			//compositionid = compositionid;
			// ISSUE: reference to a compiler-generated field
			FleetInfo fi = game.GameDatabase.GetFleetInfo(fleetid);
			// ISSUE: reference to a compiler-generated field
			if (Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(game, fi))
				return;
			LoaFleetComposition composition = (LoaFleetComposition)null;
			// ISSUE: reference to a compiler-generated field
			PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(fi.PlayerID);
			// ISSUE: reference to a compiler-generated field
			if (!compositionid.HasValue)
			{
				if (!game.GetPlayerObject(playerInfo.ID).IsAI())
					return;
				// ISSUE: reference to a compiler-generated method
				AIFleetInfo aifi = game.GameDatabase.GetAIFleetInfos(playerInfo.ID).FirstOrDefault<AIFleetInfo>((AIFleetInfo x) => x.FleetID == fi.ID);
				if (aifi != null)
				{
					List<LoaFleetShipDef> loaFleetShipDefList = new List<LoaFleetShipDef>();
					Dictionary<int, int> designsFromTemplate = game.GetFleetDesignsFromTemplate(game.GetPlayerObject(playerInfo.ID), aifi.FleetTemplate);
					foreach (int key in designsFromTemplate.Keys)
					{
						for (int index = 0; index < designsFromTemplate[key]; ++index)
							loaFleetShipDefList.Add(new LoaFleetShipDef()
							{
								DesignID = key
							});
					}
					FleetTemplate fleetTemplate = game.AssetDatabase.FleetTemplates.First<FleetTemplate>((Func<FleetTemplate, bool>)(x => x.Name == aifi.FleetTemplate));
					composition = new LoaFleetComposition();
					composition.Name = fleetTemplate.Name;
					composition.PlayerID = playerInfo.ID;
					composition.designs = loaFleetShipDefList;
				}
			}
			Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(game, fleetid);
			game.GameDatabase.GetShipInfoByFleetID(fleetid, true).ToList<ShipInfo>();
			ShipInfo shipInfo = game.GameDatabase.GetShipInfoByFleetID(fleetid, true).FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignInfo.IsLoaCube())) ?? game.GameDatabase.GetShipInfo(Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(game, fleetid), false);
			if (shipInfo == null)
				return;
			if (composition == null)
			{
				List<LoaFleetComposition> list = game.GameDatabase.GetLoaFleetCompositions().ToList<LoaFleetComposition>();
				if (!list.Any<LoaFleetComposition>())
					return;
				// ISSUE: reference to a compiler-generated method
				composition = list.FirstOrDefault((LoaFleetComposition x) => x.ID == compositionid);
			}
			if (composition == null)
				return;
			float fleetLoaCubeValue = (float)Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(game, fleetid);
			List<DesignInfo> list1 = Kerberos.Sots.StarFleet.StarFleet.GetDesignBuildOrderForComposition(game, fleetid, composition, missionType).ToList<DesignInfo>();
			int num1 = 0;
			List<DesignInfo> list2 = list1.Where<DesignInfo>((Func<DesignInfo, bool>)(X => X.Class == ShipClass.BattleRider)).ToList<DesignInfo>();
			DesignInfo des1 = list1.FirstOrDefault<DesignInfo>((Func<DesignInfo, bool>)(x => x.GetCommandPoints() > 0));
			if (des1 != null)
			{
				if ((double)fleetLoaCubeValue < (double)des1.GetPlayerProductionCost(game.GameDatabase, playerInfo.ID, !des1.isPrototyped, new float?()))
					return;
				num1 += des1.GetPlayerProductionCost(game.GameDatabase, playerInfo.ID, !des1.isPrototyped, new float?());
				int num2 = game.GameDatabase.InsertShip(fleetid, des1.ID, des1.Name, (ShipParams)0, new int?(), 0);
				game.AddDefaultStartingRiders(fleetid, des1.ID, num2);
				list1.Remove(des1);
				foreach (CarrierWingData carrierWingData in RiderManager.GetDesignBattleriderWingData(game.App, des1).ToList<CarrierWingData>())
				{
					// ISSUE: object of a compiler-generated type is created
					// ISSUE: variable of a compiler-generated type
					// Kerberos.Sots.StarFleet.StarFleet.<> c__DisplayClass163 cDisplayClass163_1 = new Kerberos.Sots.StarFleet.StarFleet.<> c__DisplayClass163();
					// ISSUE: reference to a compiler-generated field
					//cDisplayClass163_1.CS$<> 8__locals15e = cDisplayClass15d1;
					// ISSUE: reference to a compiler-generated field
					CarrierWingData wd = carrierWingData;
					// ISSUE: object of a compiler-generated type is created
					// ISSUE: variable of a compiler-generated type
					//Kerberos.Sots.StarFleet.StarFleet.<> c__DisplayClass166 cDisplayClass166_1 = new Kerberos.Sots.StarFleet.StarFleet.<> c__DisplayClass166();
					// ISSUE: reference to a compiler-generated field
					//cDisplayClass166_1.CS$<> 8__locals164 = cDisplayClass163_1;
					// ISSUE: reference to a compiler-generated field
					//cDisplayClass166_1.CS$<> 8__locals15e = cDisplayClass15d1;
					// ISSUE: reference to a compiler-generated field
					// ISSUE: reference to a compiler-generated method
					List<DesignInfo> classriders = (from x in list2
													where StrategicAI.BattleRiderMountSet.GetMatchingTurretClass(x) == wd.Class
													select x).ToList<DesignInfo>();
					// ISSUE: reference to a compiler-generated field
					// ISSUE: reference to a compiler-generated field
					if (classriders.Any<DesignInfo>() && wd.SlotIndexes.Any<int>())
					{
						// ISSUE: reference to a compiler-generated field
						// ISSUE: reference to a compiler-generated method
						// BattleRiderTypes SelectedType = classriders.Where<DesignInfo>(new Func<DesignInfo, bool>(cDisplayClass166_1.< BuildFleetFromCompositionID > b__153)).First<DesignInfo>().GetMissionSectionAsset().BattleRiderType;
						// ISSUE: reference to a compiler-generated field
						/*DesignInfo designInfo = classriders.FirstOrDefault<DesignInfo>((Func<DesignInfo, bool>)(x =>
					   {
						   // ISSUE: variable of a compiler-generated type
						   Kerberos.Sots.StarFleet.StarFleet.<> c__DisplayClass166 cDisplayClass166 = cDisplayClass166_1;
						   // ISSUE: variable of a compiler-generated type
						   Kerberos.Sots.StarFleet.StarFleet.<> c__DisplayClass163 cDisplayClass163 = cDisplayClass163_1;
						   // ISSUE: variable of a compiler-generated type
						   Kerberos.Sots.StarFleet.StarFleet.<> c__DisplayClass15d cDisplayClass15d = cDisplayClass15d1;
						   DesignInfo x1 = x;
						   if (x1.GetMissionSectionAsset().BattleRiderType == SelectedType)
							   return classriders.Count<DesignInfo>((Func<DesignInfo, bool>)(j => j.ID == x1.ID)) >= wd.SlotIndexes.Count;
						   return false;
					   }));*/
						BattleRiderTypes SelectedType = (from x in classriders
														 where classriders.Count((DesignInfo j) => j.ID == x.ID) >= wd.SlotIndexes.Count
														 select x).First<DesignInfo>().GetMissionSectionAsset().BattleRiderType;
						DesignInfo designInfo = classriders.FirstOrDefault((DesignInfo x) => x.GetMissionSectionAsset().BattleRiderType == SelectedType && classriders.Count((DesignInfo j) => j.ID == x.ID) >= wd.SlotIndexes.Count);
						// ISSUE: reference to a compiler-generated field
						foreach (int slotIndex in wd.SlotIndexes)
						{
							if (designInfo != null && (double)fleetLoaCubeValue >= (double)(designInfo.GetPlayerProductionCost(game.GameDatabase, playerInfo.ID, !designInfo.isPrototyped, new float?()) + num1))
							{
								num1 += designInfo.GetPlayerProductionCost(game.GameDatabase, playerInfo.ID, !designInfo.isPrototyped, new float?());
								int num3 = game.GameDatabase.InsertShip(fleetid, designInfo.ID, designInfo.Name, (ShipParams)0, new int?(), 0);
								game.AddDefaultStartingRiders(fleetid, designInfo.ID, num3);
								game.GameDatabase.SetShipParent(num3, num2);
								game.GameDatabase.UpdateShipRiderIndex(num3, slotIndex);
								list2.Remove(designInfo);
							}
						}
					}
				}
			}
			foreach (DesignInfo des2 in list1)
			{
				if (des2.Class != ShipClass.BattleRider)
				{
					RealShipClasses? realShipClass1 = des2.GetRealShipClass();
					if ((realShipClass1.GetValueOrDefault() != RealShipClasses.AssaultShuttle ? 0 : (realShipClass1.HasValue ? 1 : 0)) == 0)
					{
						RealShipClasses? realShipClass2 = des2.GetRealShipClass();
						if ((realShipClass2.GetValueOrDefault() != RealShipClasses.Drone ? 0 : (realShipClass2.HasValue ? 1 : 0)) == 0)
						{
							RealShipClasses? realShipClass3 = des2.GetRealShipClass();
							if ((realShipClass3.GetValueOrDefault() != RealShipClasses.EscapePod ? 0 : (realShipClass3.HasValue ? 1 : 0)) == 0 && (double)fleetLoaCubeValue >= (double)(des2.GetPlayerProductionCost(game.GameDatabase, playerInfo.ID, !des2.isPrototyped, new float?()) + num1))
							{
								num1 += des2.GetPlayerProductionCost(game.GameDatabase, playerInfo.ID, !des2.isPrototyped, new float?());
								int num2 = game.GameDatabase.InsertShip(fleetid, des2.ID, des2.Name, (ShipParams)0, new int?(), 0);
								game.AddDefaultStartingRiders(fleetid, des2.ID, num2);
								foreach (CarrierWingData carrierWingData in RiderManager.GetDesignBattleriderWingData(game.App, des2).ToList<CarrierWingData>())
								{
									CarrierWingData wd = carrierWingData;
									List<DesignInfo> list3 = list2.Where<DesignInfo>((Func<DesignInfo, bool>)(x =>
								   {
									   WeaponEnums.TurretClasses? matchingTurretClass = StrategicAI.BattleRiderMountSet.GetMatchingTurretClass(x);
									   WeaponEnums.TurretClasses turretClasses = wd.Class;
									   if (matchingTurretClass.GetValueOrDefault() == turretClasses)
										   return matchingTurretClass.HasValue;
									   return false;
								   })).ToList<DesignInfo>();
									if (list3.Any<DesignInfo>() && wd.SlotIndexes.Any<int>())
									{
										DesignInfo designInfo = App.GetSafeRandom().Choose<DesignInfo>((IList<DesignInfo>)list3);
										foreach (int slotIndex in wd.SlotIndexes)
										{
											if (designInfo != null && (double)fleetLoaCubeValue >= (double)(designInfo.GetPlayerProductionCost(game.GameDatabase, playerInfo.ID, !designInfo.isPrototyped, new float?()) + num1))
											{
												num1 += designInfo.GetPlayerProductionCost(game.GameDatabase, playerInfo.ID, !designInfo.isPrototyped, new float?());
												int num3 = game.GameDatabase.InsertShip(fleetid, designInfo.ID, designInfo.Name, (ShipParams)0, new int?(), 0);
												game.AddDefaultStartingRiders(fleetid, designInfo.ID, num3);
												game.GameDatabase.SetShipParent(num3, num2);
												game.GameDatabase.UpdateShipRiderIndex(num3, slotIndex);
												list2.Remove(designInfo);
											}
										}
									}
								}
							}
						}
					}
				}
			}
			shipInfo.LoaCubes = (int)fleetLoaCubeValue - num1;
			if (shipInfo.LoaCubes <= 0)
				game.GameDatabase.RemoveShip(shipInfo.ID);
			else
				game.GameDatabase.UpdateShipLoaCubes(shipInfo.ID, shipInfo.LoaCubes);
		}

		public static void BuildFleetFromComposition(
		  GameSession game,
		  int fleetid,
		  MissionType missionType = MissionType.NO_MISSION)
		{
			FleetInfo fleetInfo = game.GameDatabase.GetFleetInfo(fleetid);
			if (fleetInfo == null || Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(game, fleetInfo))
				return;
			Kerberos.Sots.StarFleet.StarFleet.BuildFleetFromCompositionID(game, fleetid, fleetInfo.FleetConfigID, missionType);
		}

		public class EvaluatedNode
		{
			public int SystemID;
			public int FromNodeID;
			public float FCost;
			public float HCost;
			public bool Evaluated;

			public EvaluatedNode(int system, int from, float fCost, float hCost)
			{
				this.SystemID = system;
				this.FromNodeID = from;
				this.FCost = fCost;
				this.HCost = hCost;
				this.Evaluated = false;
			}
		}
	}
}
