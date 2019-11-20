// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.DesignLab
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kerberos.Sots.Strategy
{
	internal class DesignLab
	{
		private static readonly ShipClass[] _designShipClassFallback = new ShipClass[4]
		{
	  ShipClass.Leviathan,
	  ShipClass.Dreadnought,
	  ShipClass.Cruiser,
	  ShipClass.BattleRider
		};
		private static readonly Dictionary<ShipRole, WeaponRole> DefaultShipWeaponRoles = new Dictionary<ShipRole, WeaponRole>();
		public static Rectangle DEFAULT_CRUISER_SIZE = new Rectangle(0.0f, 0.0f, 9f, 9f);
		public static Rectangle DEFAULT_DREADNAUGHT_SIZE = new Rectangle(0.0f, 0.0f, 27f, 12f);
		public static Rectangle DEFAULT_LEVIATHAN_SIZE = new Rectangle(0.0f, 0.0f, 81f, 27f);
		private static ShipPreference[] _preferences;

		private static void Warn(string message)
		{
			App.Log.Warn(message, "design");
		}

		private static void Trace(string message)
		{
			App.Log.Trace(message, "design");
		}

		private static void TraceVerbose(string message)
		{
			App.Log.Trace(message, "design", Kerberos.Sots.Engine.LogLevel.Verbose);
		}

		public static void Init(GameSession game)
		{
			DesignLab.TraceVerbose("  First-time call: loading section preferences...");
			DesignLab._preferences = DesignLab.LoadSectionPreferences(game);
		}

		private static void InitDefaultShipWeaponRoles()
		{
			foreach (ShipRole index in (ShipRole[])Enum.GetValues(typeof(ShipRole)))
			{
				switch (index)
				{
					case ShipRole.UNDEFINED:
						continue;
					case ShipRole.COMBAT:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.BRAWLER;
						continue;
					case ShipRole.CARRIER:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.STAND_OFF;
						continue;
					case ShipRole.COMMAND:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.DISABLING;
						continue;
					case ShipRole.COLONIZER:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.STAND_OFF;
						continue;
					case ShipRole.CONSTRUCTOR:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.STAND_OFF;
						continue;
					case ShipRole.SCOUT:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.STAND_OFF;
						continue;
					case ShipRole.SUPPLY:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.STAND_OFF;
						continue;
					case ShipRole.E_WARFARE:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.DISABLING;
						continue;
					case ShipRole.GATE:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.STAND_OFF;
						continue;
					case ShipRole.BORE:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.STAND_OFF;
						continue;
					case ShipRole.FREIGHTER:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.STAND_OFF;
						continue;
					case ShipRole.SCAVENGER:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.PLANET_ATTACK;
						continue;
					case ShipRole.DRONE:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.BRAWLER;
						continue;
					case ShipRole.ASSAULTSHUTTLE:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.STAND_OFF;
						continue;
					case ShipRole.SLAVEDISK:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.STAND_OFF;
						continue;
					case ShipRole.BOARDINGPOD:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.STAND_OFF;
						continue;
					case ShipRole.BIOMISSILE:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.STAND_OFF;
						continue;
					case ShipRole.TRAPDRONE:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.BRAWLER;
						continue;
					case ShipRole.POLICE:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.DISABLING;
						continue;
					case ShipRole.PLATFORM:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.STAND_OFF;
						continue;
					case ShipRole.BR_PATROL:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.BRAWLER;
						continue;
					case ShipRole.BR_SCOUT:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.STAND_OFF;
						continue;
					case ShipRole.BR_SPINAL:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.BRAWLER;
						continue;
					case ShipRole.BR_ESCORT:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.POINT_DEFENSE;
						continue;
					case ShipRole.BR_INTERCEPTOR:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.BRAWLER;
						continue;
					case ShipRole.BR_TORPEDO:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.STAND_OFF;
						continue;
					case ShipRole.BATTLECRUISER:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.BRAWLER;
						continue;
					case ShipRole.BATTLESHIP:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.BRAWLER;
						continue;
					case ShipRole.ACCELERATOR_GATE:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.STAND_OFF;
						continue;
					case ShipRole.LOA_CUBE:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.STAND_OFF;
						continue;
					case ShipRole.CARRIER_ASSAULT:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.PLANET_ATTACK;
						continue;
					case ShipRole.CARRIER_DRONE:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.STAND_OFF;
						continue;
					case ShipRole.CARRIER_BIO:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.PLANET_ATTACK;
						continue;
					case ShipRole.CARRIER_BOARDING:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.STAND_OFF;
						continue;
					case ShipRole.GRAVBOAT:
						DesignLab.DefaultShipWeaponRoles[index] = WeaponRole.DISABLING;
						continue;
					default:
						throw new ArgumentOutOfRangeException("role");
				}
			}
		}

		static DesignLab()
		{
			DesignLab.InitDefaultShipWeaponRoles();
		}

		public static DesignInfo SetDefaultDesign(
		  GameSession game,
		  ShipRole role,
		  WeaponRole? weaponRole,
		  int playerID,
		  string optionalName,
		  bool? startsPrototyped,
		  AITechStyles optionalTechStyles,
		  AIStance? optionalStance)
		{
			List<DesignInfo> list = game.GameDatabase.GetVisibleDesignInfosForPlayerAndRole(playerID, role, weaponRole).ToList<DesignInfo>();
			list.RemoveAll((Predicate<DesignInfo>)(x => x.IsSuulka()));
			if (!list.Any<DesignInfo>())
			{
				DesignLab.TraceVerbose(string.Format("  Creating default design {0},{1} for player {2}...", (object)role, weaponRole.HasValue ? (object)weaponRole.Value.ToString() : (object)"(unspecified)", (object)playerID));
				foreach (ShipClass shipClass in DesignLab._designShipClassFallback)
				{
					WeaponRole wpnRole = !weaponRole.HasValue ? (!optionalStance.HasValue ? (!DesignLab.DefaultShipWeaponRoles.ContainsKey(role) ? WeaponRole.BRAWLER : DesignLab.DefaultShipWeaponRoles[role]) : DesignLab.SuggestWeaponRoleForNewDesign(optionalStance.Value, role, shipClass)) : weaponRole.Value;
					DesignInfo design = DesignLab.DesignShip(game, shipClass, role, wpnRole, playerID);
					if (design != null)
					{
						design.isPrototyped = !startsPrototyped.HasValue ? shipClass == ShipClass.BattleRider || shipClass == ShipClass.Station : startsPrototyped.Value;
						design.Name = optionalName;
						int designID = game.GameDatabase.InsertDesignByDesignInfo(design);
						return game.GameDatabase.GetDesignInfo(designID);
					}
				}
			}
			return list.OrderByDescending<DesignInfo, int>((Func<DesignInfo, int>)(x => x.DesignDate)).FirstOrDefault<DesignInfo>();
		}

		public static DesignInfo DesignShip(
		  GameSession game,
		  ShipClass shipClass,
		  ShipRole role,
		  WeaponRole wpnRole,
		  int playerID)
		{
			return DesignLab.DesignShip(game, shipClass, role, wpnRole, playerID, (AITechStyles)null);
		}

		private static IEnumerable<int> SelectShipOptions(
		  GameDatabase db,
		  ShipSectionAsset section,
		  int playerId,
		  ShipRole role,
		  WeaponRole wpnRole)
		{
			foreach (string[] shipOption in section.ShipOptions)
			{
				for (int index = shipOption.Length - 1; index >= 0; --index)
				{
					string techFileId = shipOption[index];
					if ((role == ShipRole.SCOUT || !(techFileId == "CCC_Advanced_Sensors")) && section.Faction != "loa")
					{
						int techId = db.GetTechID(techFileId);
						PlayerTechInfo playerTechInfo = db.GetPlayerTechInfo(playerId, techId);
						if (playerTechInfo != null && playerTechInfo.State == TechStates.Researched)
						{
							yield return techId;
							break;
						}
					}
				}
			}
		}

		public static DesignInfo CreateInitialShipDesign(
		  GameSession game,
		  string name,
		  IEnumerable<ShipSectionAsset> sections,
		  int playerID,
		  AITechStyles optionalAITechStyles)
		{
			DesignInfo designInfo = DesignLab.DesignShipCore(game, sections, new ShipClass?(), new ShipRole?(), new WeaponRole?(), playerID, optionalAITechStyles);
			designInfo.Name = name;
			designInfo.isPrototyped = true;
			return designInfo;
		}

		public static DesignInfo DesignShip(
		  GameSession game,
		  ShipClass shipClass,
		  ShipRole role,
		  WeaponRole wpnRole,
		  int playerID,
		  AITechStyles optionalAITechStyles)
		{
			return DesignLab.DesignShipCore(game, (IEnumerable<ShipSectionAsset>)new ShipSectionAsset[0], new ShipClass?(shipClass), new ShipRole?(role), new WeaponRole?(wpnRole), playerID, optionalAITechStyles);
		}

		private static DesignInfo DesignShipCore(
		  GameSession game,
		  IEnumerable<ShipSectionAsset> explicitSections,
		  ShipClass? nshipClass,
		  ShipRole? nrole,
		  WeaponRole? nwpnRole,
		  int playerID,
		  AITechStyles optionalAITechStyles)
		{
			DesignLab.TraceVerbose(string.Format("Creating a new design for player {0} to satisfy {1}, {2}, {3}...", (object)playerID, nshipClass.HasValue ? (object)nshipClass.Value.ToString() : (object)"(null)", nrole.HasValue ? (object)nrole.Value.ToString() : (object)"(null)", nwpnRole.HasValue ? (object)nwpnRole.Value.ToString() : (object)"(null)"));
			if (DesignLab._preferences == null)
				DesignLab.Init(game);
			List<ShipSectionAsset> sections = new List<ShipSectionAsset>();
			ShipSectionAsset missionsection = explicitSections.FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.Type == ShipSectionType.Mission));
			ShipClass shipClass;
			ShipRole role;
			WeaponRole wpnRole;
			if (missionsection == null)
			{
				if (!nshipClass.HasValue || !nrole.HasValue || !nwpnRole.HasValue)
					throw new ArgumentException("If there is no explicit mission section then nshipClass, nrole and nwpnRole must all have values.");
				shipClass = nshipClass.Value;
				role = nrole.Value;
				wpnRole = nwpnRole.Value;
				missionsection = DesignLab.ChooseMissionSection(game, shipClass, role, wpnRole, playerID);
			}
			else
			{
				if (nshipClass.HasValue || nrole.HasValue || nwpnRole.HasValue)
					throw new ArgumentException("If there is an explicit mission section then none of nshipClass, nrole nor nwpnRole can have values.");
				shipClass = missionsection.Class;
				role = DesignLab.GetRole(missionsection);
				wpnRole = !DesignLab.DefaultShipWeaponRoles.ContainsKey(role) ? WeaponRole.BRAWLER : DesignLab.DefaultShipWeaponRoles[role];
			}
			if (missionsection == null)
			{
				DesignLab.TraceVerbose("  Failed: No mission section available. It is possible that the player has not met prerequisites yet.");
				return (DesignInfo)null;
			}
			DesignLab.TraceVerbose(string.Format("  Mission: {0}", (object)missionsection.FileName));
			sections.Add(missionsection);
			ShipSectionAsset shipSectionAsset1 = (ShipSectionAsset)null;
			if (!((IEnumerable<ShipSectionType>)missionsection.ExcludeSectionTypes).Contains<ShipSectionType>(ShipSectionType.Engine))
			{
				shipSectionAsset1 = explicitSections.FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.Type == ShipSectionType.Engine)) ?? DesignLab.ChooseDriveSection(game, shipClass, playerID, sections);
				if (shipSectionAsset1 != null)
				{
					sections.Add(shipSectionAsset1);
					DesignLab.TraceVerbose(string.Format("  Engine: {0}", (object)shipSectionAsset1.FileName));
				}
				else
					DesignLab.TraceVerbose("  Engine: n/a");
			}
			else
				DesignLab.TraceVerbose("  Engine: n/a");
			if (!((IEnumerable<ShipSectionType>)missionsection.ExcludeSectionTypes).Contains<ShipSectionType>(ShipSectionType.Command) && shipSectionAsset1 != null && !((IEnumerable<ShipSectionType>)shipSectionAsset1.ExcludeSectionTypes).Contains<ShipSectionType>(ShipSectionType.Command))
			{
				ShipSectionAsset shipSectionAsset2 = explicitSections.FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.Type == ShipSectionType.Command)) ?? DesignLab.ChooseCommandSection(game, shipClass, missionsection.RealClass, role, wpnRole, playerID, sections);
				if (shipSectionAsset2 != null)
				{
					sections.Add(shipSectionAsset2);
					DesignLab.TraceVerbose(string.Format("  Command: {0}", (object)shipSectionAsset2.FileName));
				}
				else
					DesignLab.TraceVerbose("  Command: n/a");
			}
			else
				DesignLab.TraceVerbose("  Command: n/a");
			DesignInfo design = new DesignInfo();
			List<DesignSectionInfo> designSectionInfoList = new List<DesignSectionInfo>();
			List<LogicalWeapon> list1 = game.GameDatabase.GetAvailableWeapons(game.AssetDatabase, playerID).ToList<LogicalWeapon>();
			List<LogicalPsionic> list2 = game.AssetDatabase.Psionics.Where<LogicalPsionic>((Func<LogicalPsionic, bool>)(x => x.IsAvailable(game.GameDatabase, playerID, false))).ToList<LogicalPsionic>();
			foreach (ShipSectionAsset shipSectionAsset2 in sections)
			{
				DesignLab.TraceVerbose(string.Format("  Designing details for {0}...", (object)shipSectionAsset2.FileName));
				DesignSectionInfo designSectionInfo = new DesignSectionInfo()
				{
					DesignInfo = design
				};
				designSectionInfo.FilePath = shipSectionAsset2.FileName;
				designSectionInfo.ShipSectionAsset = shipSectionAsset2;
				designSectionInfo.Modules = DesignLab.ChooseModules(game, (IList<LogicalWeapon>)list1, shipClass, role, wpnRole, shipSectionAsset2, playerID, optionalAITechStyles, list2);
				designSectionInfo.WeaponBanks = DesignLab.ChooseWeapons(game, (IList<LogicalWeapon>)list1, role, wpnRole, shipSectionAsset2, playerID, optionalAITechStyles);
				designSectionInfo.Techs = new List<int>();
				designSectionInfo.Techs.AddRange(DesignLab.SelectShipOptions(game.GameDatabase, shipSectionAsset2, playerID, role, wpnRole));
				designSectionInfoList.Add(designSectionInfo);
			}
			design.Name = null;
			design.PlayerID = playerID;
			design.DesignSections = designSectionInfoList.ToArray();
			design.Role = role;
			design.WeaponRole = wpnRole;
			if (design.Role == ShipRole.ASSAULTSHUTTLE || design.Role == ShipRole.DRONE)
				design.isPrototyped = true;
			else
				DesignLab.TraceVerbose("  Requires prototyping.");
			DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, design);
			return design;
		}

		public static DesignInfo CreateCounterDesign(
		  GameSession game,
		  ShipClass shipClass,
		  int playerId,
		  StrategicAI.DesignConfigurationInfo enemyInfo)
		{
			Dictionary<DesignLab.DefenseStrat, float> source1 = new Dictionary<DesignLab.DefenseStrat, float>();
			Dictionary<WeaponRole, float> source2 = new Dictionary<WeaponRole, float>();
			Dictionary<ShipSectionType, ShipSectionAsset> shipSections = new Dictionary<ShipSectionType, ShipSectionAsset>();
			Random random = new Random();
			source1[DesignLab.DefenseStrat.Energy] = enemyInfo.EnergyWeapons;
			source1[DesignLab.DefenseStrat.Ballistics] = enemyInfo.BallisticsWeapons;
			source1[DesignLab.DefenseStrat.HeavyBeam] = enemyInfo.HeavyBeamWeapons;
			source1[DesignLab.DefenseStrat.Point] = enemyInfo.MissileWeapons;
			source2[WeaponRole.ENERGY] = enemyInfo.EnergyDefense;
			source2[WeaponRole.BALLISTICS] = enemyInfo.BallisticsDefense;
			source2[WeaponRole.STAND_OFF] = enemyInfo.PointDefense + enemyInfo.BallisticsDefense;
			IEnumerable<Weighted<DesignLab.DefenseStrat>> weights1 = source1.Select<KeyValuePair<DesignLab.DefenseStrat, float>, Weighted<DesignLab.DefenseStrat>>((Func<KeyValuePair<DesignLab.DefenseStrat, float>, Weighted<DesignLab.DefenseStrat>>)(x =>
		   {
			   KeyValuePair<DesignLab.DefenseStrat, float> keyValuePair = x;
			   int key = (int)keyValuePair.Key;
			   keyValuePair = x;
			   int weight = (int)keyValuePair.Value;
			   return new Weighted<DesignLab.DefenseStrat>((DesignLab.DefenseStrat)key, weight);
		   }));
			DesignLab.DefenseStrat defenseStrat = WeightedChoices.Choose<DesignLab.DefenseStrat>(random, weights1);
			IEnumerable<ShipSectionAsset> availableShipSections = game.GetAvailableShipSections(playerId, ShipSectionType.Command, shipClass);
			game.GetAvailableShipSections(playerId, ShipSectionType.Mission, shipClass);
			if (!shipSections.ContainsKey(ShipSectionType.Mission) || shipSections[ShipSectionType.Mission] == null)
			{
				ShipRole role = shipClass == ShipClass.Leviathan ? ShipRole.COMMAND : ShipRole.COMBAT;
				shipSections[ShipSectionType.Mission] = DesignLab.ChooseMissionSection(game, shipClass, role, WeaponRole.BRAWLER, playerId);
			}
			switch (defenseStrat)
			{
				case DesignLab.DefenseStrat.Energy:
					shipSections[ShipSectionType.Command] = availableShipSections.FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
				   {
					   if (DesignLab.AreSectionsCompatible(shipSections[ShipSectionType.Mission], x))
						   return x.ShipOptions.Any<string[]>((Func<string[], bool>)(y => ((IEnumerable<string>)y).Contains<string>("SLD_Meson_Shields")));
					   return false;
				   }));
					if (shipSections[ShipSectionType.Command] == null)
					{
						shipSections[ShipSectionType.Command] = availableShipSections.FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
					   {
						   if (DesignLab.AreSectionsCompatible(shipSections[ShipSectionType.Mission], x))
							   return ((IEnumerable<string>)x.RequiredTechs).Contains<string>("SLD_Disruptor_Shields");
						   return false;
					   }));
						if (shipSections[ShipSectionType.Command] == null)
						{
							shipSections[ShipSectionType.Command] = availableShipSections.FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
						   {
							   if (DesignLab.AreSectionsCompatible(shipSections[ShipSectionType.Mission], x))
								   return ((IEnumerable<string>)x.RequiredTechs).Contains<string>("NRG_Energy_Absorbers");
							   return false;
						   }));
							if (shipSections[ShipSectionType.Command] == null)
								break;
							break;
						}
						break;
					}
					break;
				case DesignLab.DefenseStrat.Ballistics:
					shipSections[ShipSectionType.Command] = availableShipSections.FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
				   {
					   if (DesignLab.AreSectionsCompatible(shipSections[ShipSectionType.Mission], x))
						   return ((IEnumerable<string>)x.RequiredTechs).Contains<string>("SLD_Deflector_Shields");
					   return false;
				   }));
					if (shipSections[ShipSectionType.Command] == null)
						break;
					break;
				case DesignLab.DefenseStrat.HeavyBeam:
					Dictionary<WeaponRole, float> dictionary;
					(dictionary = source2)[WeaponRole.BALLISTICS] = dictionary[WeaponRole.BALLISTICS] + 50f;
					break;
			}
			if (!shipSections.ContainsKey(ShipSectionType.Command) || shipSections[ShipSectionType.Command] == null)
				shipSections[ShipSectionType.Command] = DesignLab.ChooseCommandSection(game, shipClass, shipSections[ShipSectionType.Mission].RealClass, ShipRole.COMBAT, WeaponRole.BRAWLER, playerId, (List<ShipSectionAsset>)null);
			WeaponRole wpnRole;
			if (defenseStrat == DesignLab.DefenseStrat.Point && random.CoinToss(0.75))
			{
				wpnRole = WeaponRole.BRAWLER;
			}
			else
			{
				float num = source2.Max<KeyValuePair<WeaponRole, float>>((Func<KeyValuePair<WeaponRole, float>, float>)(x => x.Value)) * 1.05f;
				if ((double)num != 0.0)
				{
					foreach (KeyValuePair<WeaponRole, float> keyValuePair in new Dictionary<WeaponRole, float>((IDictionary<WeaponRole, float>)source2))
						source2[keyValuePair.Key] = (float)((1.0 - (double)keyValuePair.Value / (double)num) * 100.0);
				}
				IEnumerable<Weighted<WeaponRole>> weights2 = source2.Select<KeyValuePair<WeaponRole, float>, Weighted<WeaponRole>>((Func<KeyValuePair<WeaponRole, float>, Weighted<WeaponRole>>)(x =>
			   {
				   KeyValuePair<WeaponRole, float> keyValuePair = x;
				   int key = (int)keyValuePair.Key;
				   keyValuePair = x;
				   int weight = (int)keyValuePair.Value;
				   return new Weighted<WeaponRole>((WeaponRole)key, weight);
			   }));
				wpnRole = WeightedChoices.Choose<WeaponRole>(random, weights2);
			}
			DesignInfo design = new DesignInfo();
			List<DesignSectionInfo> designSectionInfoList = new List<DesignSectionInfo>();
			List<LogicalWeapon> list = game.GameDatabase.GetAvailableWeapons(game.AssetDatabase, playerId).ToList<LogicalWeapon>();
			game.AssetDatabase.Psionics.Where<LogicalPsionic>((Func<LogicalPsionic, bool>)(x => x.IsAvailable(game.GameDatabase, playerId, false))).ToList<LogicalPsionic>();
			if (shipSections[ShipSectionType.Mission] != null && shipSections[ShipSectionType.Command] != null && !DesignLab.AreSectionsCompatible(shipSections[ShipSectionType.Mission], shipSections[ShipSectionType.Command]))
				shipSections[ShipSectionType.Command] = (ShipSectionAsset)null;
			shipSections[ShipSectionType.Engine] = DesignLab.ChooseDriveSection(game, shipClass, playerId, shipSections.Values.Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
		   {
			   if (x != null)
				   return x.Type == ShipSectionType.Mission;
			   return false;
		   })).ToList<ShipSectionAsset>());
			foreach (KeyValuePair<ShipSectionType, ShipSectionAsset> keyValuePair in shipSections)
			{
				if (keyValuePair.Value != null)
				{
					ShipSectionAsset shipSectionAsset = keyValuePair.Value;
					DesignSectionInfo designSectionInfo = new DesignSectionInfo()
					{
						DesignInfo = design
					};
					designSectionInfo.FilePath = shipSectionAsset.FileName;
					designSectionInfo.ShipSectionAsset = shipSectionAsset;
					designSectionInfo.Modules = DesignLab.ChooseModules(game, (IList<LogicalWeapon>)list, shipClass, ShipRole.COMBAT, wpnRole, shipSectionAsset, playerId, (AITechStyles)null, new List<LogicalPsionic>());
					designSectionInfo.WeaponBanks = DesignLab.ChooseWeapons(game, (IList<LogicalWeapon>)list, ShipRole.COMBAT, wpnRole, shipSectionAsset, playerId, (AITechStyles)null);
					designSectionInfo.Techs = new List<int>();
					designSectionInfo.Techs.AddRange(DesignLab.SelectShipOptions(game.GameDatabase, shipSectionAsset, playerId, ShipRole.COMBAT, wpnRole));
					if (keyValuePair.Key == ShipSectionType.Command && defenseStrat == DesignLab.DefenseStrat.Energy && shipSectionAsset.ShipOptions.Any<string[]>((Func<string[], bool>)(y => ((IEnumerable<string>)y).Contains<string>("SLD_Meson_Shields"))))
					{
						int techId = game.GameDatabase.GetTechID("SLD_Meson_Shields");
						PlayerTechInfo playerTechInfo = game.GameDatabase.GetPlayerTechInfo(playerId, techId);
						if (!designSectionInfo.Techs.Contains(techId) && playerTechInfo != null && playerTechInfo.State == TechStates.Researched)
							designSectionInfo.Techs.Add(techId);
					}
					designSectionInfoList.Add(designSectionInfo);
				}
			}
			design.Name = null;
			design.PlayerID = playerId;
			design.DesignSections = designSectionInfoList.ToArray();
			design.Role = ShipRole.COMBAT;
			design.WeaponRole = wpnRole;
			DesignLab.SummarizeDesign(game.AssetDatabase, game.GameDatabase, design);
			return design;
		}

		public static ShipSectionAsset ChooseDriveSection(
		  GameSession game,
		  ShipClass shipClass,
		  int playerID,
		  List<ShipSectionAsset> sections)
		{
			PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(playerID);
			string factionName = game.GameDatabase.GetFactionName(playerInfo.FactionID);
			ShipSectionAsset shipSectionAsset = (ShipSectionAsset)null;
			float num1 = 0.0f;
			foreach (ShipSectionAsset availableShipSection in game.GetAvailableShipSections(playerID, ShipSectionType.Engine, shipClass))
			{
				bool flag = true;
				if (sections != null)
				{
					foreach (ShipSectionAsset section in sections)
					{
						if (!DesignLab.AreSectionsCompatible(availableShipSection, section))
							flag = false;
					}
				}
				if (flag)
				{
					float num2 = availableShipSection.FtlSpeed;
					if (factionName == "human" || factionName == "zuul")
						num2 = availableShipSection.NodeSpeed;
					if (shipSectionAsset == null || (double)num2 > (double)num1)
					{
						shipSectionAsset = availableShipSection;
						num1 = num2;
					}
				}
			}
			return shipSectionAsset;
		}

		public static ShipRole GetRole(ShipSectionAsset missionsection)
		{
			if (missionsection.StationType != StationType.INVALID_TYPE)
				return ShipRole.UNDEFINED;
			if (missionsection.CommandPoints > 0)
				return ShipRole.COMMAND;
			if (missionsection.ColonizationSpace > 0)
				return ShipRole.COLONIZER;
			if (missionsection.CombatAIType == SectionEnumerations.CombatAiType.TrapDrone)
				return ShipRole.TRAPDRONE;
			if (missionsection.IsGravBoat)
				return ShipRole.GRAVBOAT;
			if (missionsection.isConstructor)
				return ShipRole.CONSTRUCTOR;
			if (missionsection.IsSupplyShip)
				return ShipRole.SUPPLY;
			if (missionsection.IsBoreShip)
				return ShipRole.BORE;
			if (missionsection.IsGateShip)
				return ShipRole.GATE;
			if (missionsection.IsAccelerator)
				return ShipRole.ACCELERATOR_GATE;
			if (missionsection.IsLoaCube)
				return ShipRole.LOA_CUBE;
			if (missionsection.RealClass == RealShipClasses.Platform)
				return ShipRole.PLATFORM;
			if (missionsection.IsScavenger)
				return ShipRole.SCAVENGER;
			if (missionsection.isPolice)
				return ShipRole.POLICE;
			if (missionsection.IsFreighter)
				return ShipRole.FREIGHTER;
			if (missionsection.isDeepScan)
				return ShipRole.SCOUT;
			if (missionsection.RealClass == RealShipClasses.Drone)
				return ShipRole.DRONE;
			if (missionsection.RealClass == RealShipClasses.AssaultShuttle && missionsection.SlaveCapacity > 0)
				return ShipRole.SLAVEDISK;
			if (missionsection.RealClass == RealShipClasses.AssaultShuttle)
				return ShipRole.ASSAULTSHUTTLE;
			if (missionsection.RealClass == RealShipClasses.BoardingPod)
				return ShipRole.BOARDINGPOD;
			if (missionsection.RealClass == RealShipClasses.Biomissile)
				return ShipRole.BIOMISSILE;
			if (missionsection.RealClass == RealShipClasses.BattleRider)
			{
				switch (missionsection.BattleRiderType)
				{
					case BattleRiderTypes.patrol:
						return ShipRole.BR_PATROL;
					case BattleRiderTypes.scout:
						return ShipRole.BR_SCOUT;
					case BattleRiderTypes.spinal:
						return ShipRole.BR_SPINAL;
					case BattleRiderTypes.escort:
						return ShipRole.BR_ESCORT;
					case BattleRiderTypes.interceptor:
						return ShipRole.BR_INTERCEPTOR;
					case BattleRiderTypes.torpedo:
						return ShipRole.BR_TORPEDO;
					default:
						return ShipRole.COMBAT;
				}
			}
			else
			{
				if (missionsection.RealClass == RealShipClasses.BattleCruiser)
					return ShipRole.BATTLECRUISER;
				if (missionsection.RealClass == RealShipClasses.BattleShip)
					return ShipRole.BATTLESHIP;
				if (!missionsection.IsCarrier)
					return ShipRole.COMBAT;
				switch (missionsection.CarrierType)
				{
					case CarrierType.Drone:
						return ShipRole.CARRIER_DRONE;
					case CarrierType.AssaultShuttle:
						return ShipRole.CARRIER_ASSAULT;
					case CarrierType.BioMissile:
						return ShipRole.CARRIER_BIO;
					case CarrierType.BoardingPod:
						return ShipRole.CARRIER_BOARDING;
					default:
						return ShipRole.CARRIER;
				}
			}
		}

		private static ShipSectionAsset ChooseMissionSection(
		  GameSession sim,
		  ShipClass shipClass,
		  ShipRole role,
		  WeaponRole wpnRole,
		  int playerID)
		{
			ShipSectionAsset shipSectionAsset = (ShipSectionAsset)null;
			RealShipClasses realClass = DesignLab.GetRealShipClassFromShipClassAndRole(shipClass, role);
			if (sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
		   {
			   if (x.RealClass == realClass && DesignLab.GetRole(x) == role && x.CarrierTypeMatchesRole(role))
				   return !x.IsSuulka;
			   return false;
		   })) == null && (role == ShipRole.CARRIER || role == ShipRole.CARRIER_ASSAULT || (role == ShipRole.CARRIER_BIO || role == ShipRole.CARRIER_DRONE) || (role == ShipRole.E_WARFARE || role == ShipRole.CARRIER_BOARDING || role == ShipRole.SCOUT)))
			{
				DesignLab.Trace("Section type for role " + (object)role + " not available. Defaulting to COMBAT.");
				role = ShipRole.COMBAT;
			}
			switch (role)
			{
				case ShipRole.COMBAT:
					return DesignLab.ChooseSectionForCombat(sim, shipClass, realClass, ShipSectionType.Mission, wpnRole, playerID, new ShipRole?(role));
				case ShipRole.CARRIER:
				case ShipRole.CARRIER_ASSAULT:
				case ShipRole.CARRIER_DRONE:
				case ShipRole.CARRIER_BIO:
				case ShipRole.CARRIER_BOARDING:
					ShipClass sc2 = ShipClass.BattleRider;
					int num1 = 0;
					foreach (ShipSectionAsset missionsection in sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.RealClass == realClass)))
					{
						if (DesignLab.GetRole(missionsection) == role && missionsection.CarrierTypeMatchesRole(role) && !missionsection.IsSuulka)
						{
							int num2 = ((IEnumerable<LogicalBank>)missionsection.Banks).Count<LogicalBank>((Func<LogicalBank, bool>)(x => WeaponEnums.IsBattleRider(x.TurretClass)));
							if (missionsection.Class == sc2 && num2 > num1 || Ship.IsShipClassBigger(missionsection.Class, sc2, false))
							{
								sc2 = missionsection.Class;
								num1 = num2;
								shipSectionAsset = missionsection;
							}
						}
					}
					return shipSectionAsset;
				case ShipRole.COMMAND:
					int num3 = 0;
					foreach (ShipSectionAsset missionsection in sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.RealClass == realClass)))
					{
						if (DesignLab.GetRole(missionsection) == role && missionsection.CommandPoints > num3 && !missionsection.IsSuulka)
						{
							shipSectionAsset = missionsection;
							num3 = missionsection.CommandPoints;
						}
					}
					return shipSectionAsset;
				case ShipRole.COLONIZER:
					int num4 = 0;
					foreach (ShipSectionAsset missionsection in sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.RealClass == realClass)))
					{
						if (DesignLab.GetRole(missionsection) == role && missionsection.ColonizationSpace > num4 && !missionsection.IsSuulka)
						{
							shipSectionAsset = missionsection;
							num4 = missionsection.ColonizationSpace;
						}
					}
					return shipSectionAsset;
				case ShipRole.CONSTRUCTOR:
					foreach (ShipSectionAsset missionsection in sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.RealClass == realClass)))
					{
						if (DesignLab.GetRole(missionsection) == role && missionsection.isConstructor && !missionsection.IsSuulka)
							shipSectionAsset = missionsection;
					}
					return shipSectionAsset;
				case ShipRole.SCOUT:
					int num5 = 0;
					foreach (ShipSectionAsset missionsection in sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.RealClass == realClass)))
					{
						if (DesignLab.GetRole(missionsection) == role && (double)missionsection.TacticalSensorRange > (double)num5 && !missionsection.IsSuulka)
						{
							shipSectionAsset = missionsection;
							num5 = missionsection.Supply;
						}
					}
					return shipSectionAsset ?? DesignLab.ChooseSectionForCombat(sim, shipClass, realClass, ShipSectionType.Mission, wpnRole, playerID, new ShipRole?(role));
				case ShipRole.SUPPLY:
					int num6 = 0;
					foreach (ShipSectionAsset missionsection in sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.RealClass == realClass)))
					{
						if (DesignLab.GetRole(missionsection) == role && missionsection.Supply > num6 && !missionsection.IsSuulka)
						{
							shipSectionAsset = missionsection;
							num6 = missionsection.Supply;
						}
					}
					return shipSectionAsset;
				case ShipRole.E_WARFARE:
					return DesignLab.ChooseSectionForCombat(sim, shipClass, realClass, ShipSectionType.Mission, wpnRole, playerID, new ShipRole?(role));
				case ShipRole.GATE:
					return sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
				   {
					   if (x.IsGateShip)
						   return !x.IsSuulka;
					   return false;
				   }));
				case ShipRole.BORE:
					return sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
				   {
					   if (x.IsBoreShip)
						   return !x.IsSuulka;
					   return false;
				   }));
				case ShipRole.FREIGHTER:
					List<ShipSectionAsset> list1 = sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
				   {
					   if (x.IsFreighter)
						   return !x.IsSuulka;
					   return false;
				   })).ToList<ShipSectionAsset>();
					if (list1.Count != 0)
						return list1.OrderBy<ShipSectionAsset, int>((Func<ShipSectionAsset, int>)(x => x.FreighterSpace)).First<ShipSectionAsset>();
					return shipSectionAsset;
				case ShipRole.SCAVENGER:
					List<ShipSectionAsset> list2 = sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
				   {
					   if (x.IsScavenger)
						   return !x.IsSuulka;
					   return false;
				   })).ToList<ShipSectionAsset>();
					if (list2.Count != 0)
						return list2.First<ShipSectionAsset>();
					return shipSectionAsset;
				case ShipRole.DRONE:
					List<ShipSectionAsset> list3 = sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
				   {
					   if (x.RealClass == RealShipClasses.Drone)
						   return !x.IsSuulka;
					   return false;
				   })).ToList<ShipSectionAsset>();
					if (list3.Count != 0)
						return list3.OrderByDescending<ShipSectionAsset, int>((Func<ShipSectionAsset, int>)(x => x.ExcludeSectionTypes.Length)).First<ShipSectionAsset>();
					return shipSectionAsset;
				case ShipRole.ASSAULTSHUTTLE:
					List<ShipSectionAsset> list4 = sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
				   {
					   if (x.RealClass == RealShipClasses.AssaultShuttle)
						   return !x.IsSuulka;
					   return false;
				   })).ToList<ShipSectionAsset>();
					if (list4.Count != 0)
						return list4.OrderByDescending<ShipSectionAsset, int>((Func<ShipSectionAsset, int>)(x => x.ExcludeSectionTypes.Length)).First<ShipSectionAsset>();
					return shipSectionAsset;
				case ShipRole.SLAVEDISK:
					List<ShipSectionAsset> list5 = sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
				   {
					   if (x.RealClass == RealShipClasses.AssaultShuttle && x.SlaveCapacity > 0)
						   return !x.IsSuulka;
					   return false;
				   })).ToList<ShipSectionAsset>();
					if (list5.Count != 0)
						return list5.First<ShipSectionAsset>();
					return shipSectionAsset;
				case ShipRole.BOARDINGPOD:
					List<ShipSectionAsset> list6 = sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
				   {
					   if (x.RealClass == RealShipClasses.BoardingPod)
						   return !x.IsSuulka;
					   return false;
				   })).ToList<ShipSectionAsset>();
					if (list6.Count != 0)
						return list6.OrderByDescending<ShipSectionAsset, int>((Func<ShipSectionAsset, int>)(x => x.ExcludeSectionTypes.Length)).First<ShipSectionAsset>();
					return shipSectionAsset;
				case ShipRole.BIOMISSILE:
					List<ShipSectionAsset> list7 = sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
				   {
					   if (x.RealClass == RealShipClasses.Biomissile)
						   return !x.IsSuulka;
					   return false;
				   })).ToList<ShipSectionAsset>();
					if (list7.Count != 0)
						return list7.OrderByDescending<ShipSectionAsset, int>((Func<ShipSectionAsset, int>)(x => x.ExcludeSectionTypes.Length)).First<ShipSectionAsset>();
					return shipSectionAsset;
				case ShipRole.TRAPDRONE:
					List<ShipSectionAsset> list8 = sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
				   {
					   if (x.CombatAIType == SectionEnumerations.CombatAiType.TrapDrone)
						   return !x.IsSuulka;
					   return false;
				   })).ToList<ShipSectionAsset>();
					if (list8.Count != 0)
						return list8.OrderByDescending<ShipSectionAsset, int>((Func<ShipSectionAsset, int>)(x => x.ExcludeSectionTypes.Length)).First<ShipSectionAsset>();
					return shipSectionAsset;
				case ShipRole.POLICE:
					return sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
				   {
					   if (x.isPolice)
						   return !x.IsSuulka;
					   return false;
				   })).FirstOrDefault<ShipSectionAsset>();
				case ShipRole.PLATFORM:
					IEnumerable<ShipSectionAsset> shipSectionAssets1 = sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
				   {
					   if (x.RealClass == RealShipClasses.Platform)
						   return !x.IsSuulka;
					   return false;
				   }));
					if (shipSectionAssets1.Any<ShipSectionAsset>())
						return sim.Random.Choose<ShipSectionAsset>(shipSectionAssets1);
					return shipSectionAsset;
				case ShipRole.BR_PATROL:
				case ShipRole.BR_SCOUT:
				case ShipRole.BR_SPINAL:
				case ShipRole.BR_ESCORT:
				case ShipRole.BR_INTERCEPTOR:
				case ShipRole.BR_TORPEDO:
					IEnumerable<ShipSectionAsset> shipSectionAssets2 = sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
				   {
					   if (x.RealClass == realClass && DesignLab.GetRole(x) == role)
						   return !x.IsSuulka;
					   return false;
				   }));
					if (shipSectionAssets2.Any<ShipSectionAsset>())
						return sim.Random.Choose<ShipSectionAsset>(shipSectionAssets2);
					return shipSectionAsset;
				case ShipRole.BATTLECRUISER:
					if (shipClass != ShipClass.Cruiser)
						return shipSectionAsset;
					IEnumerable<ShipSectionAsset> shipSectionAssets3 = sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
				   {
					   if (x.BattleRiderType == BattleRiderTypes.battlerider)
						   return !x.IsSuulka;
					   return false;
				   }));
					if (shipSectionAssets3.Any<ShipSectionAsset>())
						return sim.Random.Choose<ShipSectionAsset>(shipSectionAssets3);
					return shipSectionAsset;
				case ShipRole.BATTLESHIP:
					if (shipClass != ShipClass.Dreadnought)
						return shipSectionAsset;
					IEnumerable<ShipSectionAsset> shipSectionAssets4 = sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
				   {
					   if (x.BattleRiderType == BattleRiderTypes.battlerider)
						   return !x.IsSuulka;
					   return false;
				   }));
					if (shipSectionAssets4.Any<ShipSectionAsset>())
						return sim.Random.Choose<ShipSectionAsset>(shipSectionAssets4);
					return shipSectionAsset;
				case ShipRole.ACCELERATOR_GATE:
					return sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.IsAccelerator));
				case ShipRole.LOA_CUBE:
					return sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.IsLoaCube));
				case ShipRole.GRAVBOAT:
					return sim.GetAvailableShipSections(playerID, ShipSectionType.Mission, shipClass).FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.IsGravBoat));
				default:
					throw new ArgumentOutOfRangeException(nameof(role));
			}
		}

		private static ShipSectionAsset ChooseSectionForCombat(
		  GameSession sim,
		  ShipClass shipClass,
		  RealShipClasses realClass,
		  ShipSectionType sectionType,
		  WeaponRole wpnRole,
		  int playerID,
		  ShipRole? role = null)
		{
			List<ShipSectionAsset> list = sim.GetAvailableShipSections(playerID, sectionType, shipClass).Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
		   {
			   if (!x.IsSuulka)
				   return x.RealClass == realClass;
			   return false;
		   })).ToList<ShipSectionAsset>();
			if (sectionType == ShipSectionType.Mission && role.HasValue)
				list.RemoveAll((Predicate<ShipSectionAsset>)(x => DesignLab.GetRole(x) != role.Value));
			if (list.Count == 0)
				return (ShipSectionAsset)null;
			List<ShipPreference> shipPreferenceList = new List<ShipPreference>();
			int factionId = sim.GameDatabase.GetPlayerInfo(playerID).FactionID;
			float num1 = 0.0f;
			foreach (ShipPreference preference in DesignLab._preferences)
			{
				ShipPreference shipPref = preference;
				if (shipPref.factionID == factionId && (double)shipPref.preferenceWeight > 0.0)
				{
					ShipSectionAsset shipSectionAsset = sim.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName.ToLower() == shipPref.sectionName.ToLower()));
					if (shipSectionAsset != null && shipSectionAsset.Type == sectionType && list.Contains(shipSectionAsset))
					{
						shipPreferenceList.Add(shipPref);
						num1 += shipPref.preferenceWeight;
					}
				}
			}
			if (shipPreferenceList.Count == 0)
				return list[App.GetSafeRandom().Next(list.Count)];
			float num2 = (float)App.GetSafeRandom().NextDouble() * num1;
			foreach (ShipPreference shipPreference in shipPreferenceList)
			{
				ShipPreference shipPref = shipPreference;
				if ((double)num2 <= (double)shipPref.preferenceWeight)
					return sim.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == shipPref.sectionName));
				num2 -= shipPref.preferenceWeight;
			}
			throw new NullReferenceException("The AI couldn't decided on a ship to use on it's new design. This is probably because there were no available sections for the specified type and class of ship... ");
		}

		private static ShipSectionAsset ChooseCommandSection(
		  GameSession sim,
		  ShipClass shipClass,
		  RealShipClasses realClass,
		  ShipRole role,
		  WeaponRole wpnRole,
		  int playerID,
		  List<ShipSectionAsset> sections)
		{
			ShipSectionAsset shipSectionAsset = (ShipSectionAsset)null;
			switch (role)
			{
				case ShipRole.COMBAT:
					shipSectionAsset = DesignLab.ChooseSectionForCombat(sim, shipClass, realClass, ShipSectionType.Command, wpnRole, playerID, new ShipRole?());
					break;
				case ShipRole.SCOUT:
					int num = 0;
					using (IEnumerator<ShipSectionAsset> enumerator = sim.GetAvailableShipSections(playerID, ShipSectionType.Command, shipClass).GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							ShipSectionAsset current = enumerator.Current;
							if (current.isDeepScan && (double)current.TacticalSensorRange > (double)num && !current.IsSuulka)
							{
								shipSectionAsset = current;
								num = current.Supply;
							}
						}
						break;
					}
			}
			if (shipSectionAsset == null)
				shipSectionAsset = DesignLab.ChooseSectionForCombat(sim, shipClass, realClass, ShipSectionType.Command, wpnRole, playerID, new ShipRole?());
			return shipSectionAsset;
		}

		private static List<DesignModuleInfo> ChooseModules(
		  GameSession game,
		  IList<LogicalWeapon> availableWeapons,
		  ShipClass shipClass,
		  ShipRole role,
		  WeaponRole wpnRole,
		  ShipSectionAsset sectionAsset,
		  int playerID,
		  AITechStyles optionalAITechStyles,
		  List<LogicalPsionic> remainingPsionics)
		{
			List<DesignModuleInfo> designModuleInfoList = new List<DesignModuleInfo>();
			if (sectionAsset.Modules.Length == 0)
			{
				DesignLab.TraceVerbose("No modules required for " + sectionAsset.FileName + ".");
			}
			else
			{
				DesignLab.TraceVerbose(string.Format("Choosing modules to fit {0} ({1}, {2})...\n  Slots to fill: {3}", (object)sectionAsset.FileName, (object)role, (object)wpnRole, (object)sectionAsset.Modules.Length));
				List<LogicalModule> modulesForSection = DesignLab.GetAvailableModulesForSection(game, sectionAsset, playerID);
				if (modulesForSection.Count == 0)
				{
					DesignLab.TraceVerbose("  No modules available.");
				}
				else
				{
					DesignLab.TraceVerbose(string.Format("  Modules available: {0}", (object)modulesForSection.Count));
					string name = game.GameDatabase.GetPlayerFaction(playerID).Name;
					int num1 = remainingPsionics.Count;
					int num2 = 0;
					int num3 = 0;
					int num4 = 0;
					int num5 = 0;
					int num6 = 0;
					int num7 = 0;
					int num8 = 0;
					int num9 = 0;
					int num10 = 0;
					int num11 = 0;
					int num12 = 0;
					for (int sectionModuleMountIndex = 0; sectionModuleMountIndex < sectionAsset.Modules.Length; ++sectionModuleMountIndex)
					{
						LogicalModuleMount module = sectionAsset.Modules[sectionModuleMountIndex];
						List<LogicalModule> list1 = LogicalModule.EnumerateModuleFits((IEnumerable<LogicalModule>)modulesForSection, sectionAsset, sectionModuleMountIndex, false).Where<LogicalModule>((Func<LogicalModule, bool>)(x =>
					  {
						  if (x.NumPsionicSlots > 0)
							  return remainingPsionics.Count > 0;
						  return true;
					  })).ToList<LogicalModule>();
						if (list1.Count > 0)
						{
							LogicalModule logicalModule = (LogicalModule)null;
							if (name == "loa")
							{
								if (num10 == 0 && shipClass == ShipClass.Dreadnought && list1.Any<LogicalModule>((Func<LogicalModule, bool>)(x =>
							   {
								   if (!x.ModuleTitle.Contains("REFLUX") && !x.ModuleTitle.Contains("FUSION"))
									   return x.ModuleTitle.Contains("FISSION");
								   return true;
							   })))
								{
									logicalModule = list1.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("REFLUX"))) ?? list1.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("FUSION"))) ?? list1.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("FISSION")));
									++num10;
								}
								else if (num10 < 2 && shipClass == ShipClass.Leviathan && list1.Any<LogicalModule>((Func<LogicalModule, bool>)(x =>
							   {
								   if (!x.ModuleTitle.Contains("REFLUX") && !x.ModuleTitle.Contains("FUSION"))
									   return x.ModuleTitle.Contains("FISSION");
								   return true;
							   })))
								{
									logicalModule = list1.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("REFLUX"))) ?? list1.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("FUSION"))) ?? list1.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("FISSION")));
									++num10;
								}
								else if (list1.Any<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("BULWARK"))))
									logicalModule = list1.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("BULWARK")));
								else if (list1.Any<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("GOOP"))))
									logicalModule = list1.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("GOOP")));
							}
							else if (name == "zuul" && num12 == 0 && wpnRole == WeaponRole.BRAWLER)
							{
								if (list1.Any<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("GRAPPLER"))))
								{
									logicalModule = list1.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("GRAPPLER")));
									++num12;
								}
							}
							else if (num10 == 0 && sectionAsset.SectionName.Contains("eng") && (list1.Any<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("CAMEL"))) && (role == ShipRole.SUPPLY || wpnRole == WeaponRole.STAND_OFF)))
							{
								logicalModule = list1.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("CAMEL")));
								++num10;
							}
							else if (num11 == 0 && (sectionAsset.SectionName.Contains("eng") || shipClass == ShipClass.Leviathan) && list1.Any<LogicalModule>((Func<LogicalModule, bool>)(x =>
						   {
							   if (!x.ModuleTitle.Contains("REFLUX") && !x.ModuleTitle.Contains("FUSION"))
								   return x.ModuleTitle.Contains("FISSION");
							   return true;
						   })))
							{
								logicalModule = list1.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("REFLUX"))) ?? list1.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("FUSION"))) ?? list1.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("FISSION")));
								++num11;
							}
							else if ((sectionAsset.SectionName.Contains("mis") || shipClass == ShipClass.Leviathan) && (num1 > 0 && num2 < 3) && list1.Any<LogicalModule>((Func<LogicalModule, bool>)(x =>
						   {
							   if (!x.ModuleTitle.Contains("PROFESSORX"))
								   return x.ModuleTitle.Contains("PSIWAR");
							   return true;
						   })))
							{
								num1 = shipClass != ShipClass.Cruiser ? num1 - 3 : num1 - 1;
								logicalModule = list1.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x =>
							   {
								   if (!x.ModuleTitle.Contains("PROFESSORX"))
									   return x.ModuleTitle.Contains("PSIWAR");
								   return true;
							   }));
								++num2;
							}
							else if (remainingPsionics.Count > 0 && num4 == 0 && list1.Any<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("ABADDON"))))
							{
								logicalModule = list1.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("ABADDON")));
								++num4;
							}
							else if (remainingPsionics.Count > 0 && num5 == 0 && list1.Any<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("REMILLARD"))))
							{
								logicalModule = list1.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("REMILLARD")));
								++num5;
							}
							else if (num3 < 2 && wpnRole == WeaponRole.STAND_OFF && shipClass == ShipClass.Leviathan && list1.Any<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("CAMEL"))))
							{
								logicalModule = list1.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("CAMEL")));
								++num3;
							}
							else if (num8 == 0 && list1.Any<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("BULWARK"))))
							{
								logicalModule = list1.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("BULWARK")));
								++num8;
							}
							else if (num9 == 0 && list1.Any<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("GOOP"))))
							{
								logicalModule = list1.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("GOOP")));
								++num9;
							}
							else if (list1.Any<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("KARNAK"))) && num6 == 0)
							{
								logicalModule = list1.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("KARNAK")));
								++num6;
							}
							else if (list1.Any<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("SJET"))) && num7 == 0)
							{
								logicalModule = list1.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("SJET")));
								++num7;
							}
							else if (list1.Any<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("HEAVYBEAM"))))
								logicalModule = list1.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("HEAVYBEAM")));
							else if (list1.Any<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("TORPEDO"))))
								logicalModule = list1.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("TORPEDO")));
							else if (list1.Any<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("POINT"))))
								logicalModule = list1.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("POINT")));
							else if (list1.Any<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("KINGFISH"))))
								logicalModule = list1.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("KINGFISH")));
							else if (list1.Any<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("BOARDING"))))
								logicalModule = list1.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("BOARDING")));
							else if (list1.Any<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("BIOWAR"))))
								logicalModule = list1.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModuleTitle.Contains("BIOWAR")));
							if (logicalModule == null && name != "loa")
							{
								int index = App.GetSafeRandom().Next(list1.Count);
								logicalModule = list1[index];
								list1.RemoveAt(index);
							}
							if (logicalModule != null)
							{
								DesignLab.TraceVerbose("    " + logicalModule.ModuleName + "...");
								DesignModuleInfo designModuleInfo = new DesignModuleInfo();
								designModuleInfo.ModuleID = game.GameDatabase.GetModuleID(logicalModule.ModulePath, playerID);
								designModuleInfo.MountNodeName = module.NodeName;
								LogicalBank bank1 = ((IEnumerable<LogicalBank>)logicalModule.Banks).FirstOrDefault<LogicalBank>();
								if (bank1 != null)
								{
									if (logicalModule.ModuleTitle.Contains("GRAPPLER"))
									{
										IList<LogicalWeapon> list2 = (IList<LogicalWeapon>)availableWeapons.Where<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.PayloadType == WeaponEnums.PayloadTypes.GrappleHook)).ToList<LogicalWeapon>();
										WeaponBankInfo bank2 = DesignLab.AssignBestWeaponToBank(game, bank1, list2, role, WeaponRole.DISABLING, sectionAsset, bank1.TurretClass, bank1.TurretSize, playerID, optionalAITechStyles, name);
										designModuleInfo.WeaponID = bank2.WeaponID;
									}
									else if (logicalModule.ModuleTitle.Contains("POINT"))
									{
										IList<LogicalWeapon> list2 = (IList<LogicalWeapon>)availableWeapons.Where<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.IsPDWeapon())).ToList<LogicalWeapon>();
										WeaponBankInfo bank2 = DesignLab.AssignBestWeaponToBank(game, bank1, list2, role, wpnRole, sectionAsset, bank1.TurretClass, bank1.TurretSize, playerID, optionalAITechStyles, name);
										designModuleInfo.WeaponID = bank2.WeaponID;
									}
									else
									{
										WeaponBankInfo bank2 = DesignLab.AssignBestWeaponToBank(game, bank1, availableWeapons, role, wpnRole, sectionAsset, bank1.TurretClass, bank1.TurretSize, playerID, optionalAITechStyles, name);
										designModuleInfo.WeaponID = bank2.WeaponID;
									}
									if (WeaponEnums.IsWeaponBattleRider(bank1.TurretClass))
										designModuleInfo.DesignID = DesignLab.ChooseBattleRider(game, DesignLab.GetWeaponRiderShipRole(bank1.TurretClass, sectionAsset.IsScavenger), wpnRole, playerID);
								}
								if (logicalModule.NumPsionicSlots > 0)
								{
									for (int numPsionicSlots = logicalModule.NumPsionicSlots; remainingPsionics.Count > 0 && numPsionicSlots > 0; --numPsionicSlots)
									{
										int index = game.Random.Next(remainingPsionics.Count);
										designModuleInfo.PsionicAbilities.Add(new ModulePsionicInfo()
										{
											Ability = remainingPsionics[index].Ability
										});
										remainingPsionics.RemoveAt(index);
									}
								}
								designModuleInfoList.Add(designModuleInfo);
							}
						}
					}
				}
			}
			return designModuleInfoList;
		}

		private static List<LogicalModule> GetAvailableModulesForSection(
		  GameSession game,
		  ShipSectionAsset sectionAsset,
		  int playerID)
		{
			List<LogicalModule> logicalModuleList = new List<LogicalModule>();
			foreach (LogicalModule module1 in game.AssetDatabase.Modules)
			{
				bool flag = true;
				if (module1.Faction == sectionAsset.Faction)
				{
					foreach (LogicalModuleMount module2 in sectionAsset.Modules)
					{
						if (module2.ModuleType == module1.ModuleType)
						{
							if (module1.Techs.Count > 0)
							{
								foreach (Kerberos.Sots.Data.ShipFramework.Tech tech in module1.Techs)
								{
									if (!game.GameDatabase.PlayerHasTech(playerID, tech.Name))
										flag = false;
								}
							}
							else
								goto label_14;
							label_12:
							if (flag)
							{
								logicalModuleList.Add(module1);
								break;
							}
							continue;
						label_14:
							flag = true;
							goto label_12;
						}
					}
				}
			}
			return logicalModuleList;
		}

		public static List<DesignLab.ModuleSlotInfo> GetModuleSlotInfo(
		  App game,
		  StationInfo station,
		  int playerID)
		{
			List<DesignLab.ModuleSlotInfo> moduleSlotInfoList = new List<DesignLab.ModuleSlotInfo>();
			((IEnumerable<DesignSectionInfo>)station.DesignInfo.DesignSections).Select<DesignSectionInfo, ShipSectionAsset>((Func<DesignSectionInfo, ShipSectionAsset>)(ds => game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(sa => sa.FileName == ds.FilePath)))).ToList<ShipSectionAsset>();
			DesignSectionInfo designSection = station.DesignInfo.DesignSections[0];
			foreach (LogicalModuleMount module1 in game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == designSection.FilePath)).Modules)
			{
				DesignLab.ModuleSlotInfo moduleSlotInfo = new DesignLab.ModuleSlotInfo();
				moduleSlotInfo.mountInfo = module1;
				foreach (DesignModuleInfo module2 in designSection.Modules)
				{
					if (module2.MountNodeName == module1.NodeName)
					{
						moduleSlotInfo.currentModule = module2;
						break;
					}
				}
				moduleSlotInfoList.Add(moduleSlotInfo);
			}
			return moduleSlotInfoList;
		}

		public static List<LogicalModule> GetAvailableModulesForSlot(
		  App game,
		  StationInfo station,
		  int playerID,
		  int slot)
		{
			List<LogicalModule> logicalModuleList = new List<LogicalModule>();
			((IEnumerable<DesignSectionInfo>)station.DesignInfo.DesignSections).Select<DesignSectionInfo, ShipSectionAsset>((Func<DesignSectionInfo, ShipSectionAsset>)(ds => game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(sa => sa.FileName == ds.FilePath)))).ToList<ShipSectionAsset>();
			DesignSectionInfo designSection = station.DesignInfo.DesignSections[0];
			ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == designSection.FilePath));
			LogicalModuleMount module1 = shipSectionAsset.Modules[slot];
			foreach (LogicalModule module2 in game.AssetDatabase.Modules)
			{
				if (module2.Faction == shipSectionAsset.Faction && module2.Class == shipSectionAsset.Class && module2.ModuleType == module1.ModuleType)
				{
					bool flag = true;
					foreach (Kerberos.Sots.Data.ShipFramework.Tech tech in module2.Techs)
					{
						if (!game.GameDatabase.PlayerHasTech(playerID, tech.Name))
						{
							flag = false;
							break;
						}
					}
					if (flag)
						logicalModuleList.Add(module2);
				}
			}
			return logicalModuleList;
		}

		public static void RemoveModuleFromSlot(App game, StationInfo station, int playerID, int slot)
		{
			((IEnumerable<DesignSectionInfo>)station.DesignInfo.DesignSections).Select<DesignSectionInfo, ShipSectionAsset>((Func<DesignSectionInfo, ShipSectionAsset>)(ds => game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(sa => sa.FileName == ds.FilePath)))).ToList<ShipSectionAsset>();
			DesignSectionInfo designSection = station.DesignInfo.DesignSections[0];
			LogicalModuleMount module1 = game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == designSection.FilePath)).Modules[slot];
			foreach (DesignModuleInfo module2 in designSection.Modules)
			{
				if (module2.MountNodeName == module1.NodeName)
				{
					game.GameDatabase.RemoveDesignModule(module2);
					designSection.Modules.Remove(module2);
					break;
				}
			}
		}

		public static DesignInfo AssignWeaponsToDesign(
		  GameSession game,
		  DesignInfo di,
		  List<LogicalWeapon> availableWeapons,
		  int playerID,
		  WeaponRole wpRole,
		  AITechStyles optionalAITechStyles)
		{
			foreach (DesignSectionInfo designSection in di.DesignSections)
				designSection.WeaponBanks = DesignLab.ChooseWeapons(game, (IList<LogicalWeapon>)availableWeapons, di.Role, wpRole, designSection.ShipSectionAsset, playerID, optionalAITechStyles);
			return di;
		}

		public static List<WeaponBankInfo> ChooseWeapons(
		  GameSession game,
		  IList<LogicalWeapon> availableWeapons,
		  ShipRole role,
		  WeaponRole wpnRole,
		  ShipSectionAsset sectionAsset,
		  int playerID,
		  AITechStyles optionalAITechStyles)
		{
			DesignLab.TraceVerbose(string.Format("Choosing weapons to fit {0} ({1}, {2})...", (object)sectionAsset.FileName, (object)role, (object)wpnRole));
			List<WeaponBankInfo> weaponBankInfoList = new List<WeaponBankInfo>();
			string name = game.GameDatabase.GetPlayerFaction(playerID).Name;
			foreach (LogicalBank bank1 in sectionAsset.Banks)
			{
				if (WeaponEnums.IsWeaponBattleRider(bank1.TurretClass))
				{
					DesignLab.TraceVerbose("Bank is " + (object)bank1.TurretClass + " (for a battle rider)...");
					weaponBankInfoList.Add(new WeaponBankInfo()
					{
						BankGUID = bank1.GUID,
						WeaponID = new int?(),
						DesignID = DesignLab.ChooseBattleRider(game, DesignLab.GetWeaponRiderShipRole(bank1.TurretClass, sectionAsset.IsScavenger), wpnRole, playerID)
					});
				}
				else
				{
					WeaponBankInfo bank2 = DesignLab.AssignBestWeaponToBank(game, bank1, availableWeapons, role, wpnRole, sectionAsset, bank1.TurretClass, bank1.TurretSize, playerID, optionalAITechStyles, name);
					weaponBankInfoList.Add(bank2);
				}
			}
			return weaponBankInfoList;
		}

		public static ShipRole GetWeaponRiderShipRole(
		  WeaponEnums.TurretClasses turretClass,
		  bool isScavenger)
		{
			ShipRole shipRole = ShipRole.DRONE;
			switch (turretClass)
			{
				case WeaponEnums.TurretClasses.Biomissile:
					shipRole = ShipRole.BIOMISSILE;
					break;
				case WeaponEnums.TurretClasses.Drone:
					shipRole = ShipRole.DRONE;
					break;
				case WeaponEnums.TurretClasses.AssaultShuttle:
					shipRole = !isScavenger ? ShipRole.ASSAULTSHUTTLE : ShipRole.SLAVEDISK;
					break;
				case WeaponEnums.TurretClasses.BoardingPod:
					shipRole = ShipRole.BOARDINGPOD;
					break;
			}
			return shipRole;
		}

		public static int? ChooseBattleRider(
		  GameSession game,
		  ShipRole role,
		  WeaponRole wpnRole,
		  int playerID)
		{
			int? nullable = new int?();
			int num = 0;
			List<DesignInfo> list = game.GameDatabase.GetDesignInfosForPlayer(playerID).Where<DesignInfo>((Func<DesignInfo, bool>)(x => x.Class == ShipClass.BattleRider)).ToList<DesignInfo>();
			if (list.Count == 0)
			{
				DesignInfo design = DesignLab.DesignShip(game, ShipClass.BattleRider, role, wpnRole, playerID);
				if (design != null && design != null)
				{
					design.ID = game.GameDatabase.InsertDesignByDesignInfo(design);
					DesignLab.Trace("Player " + (object)playerID + " designed a new " + (object)design.Role + " " + (object)design.Class + ".");
				}
				list.Add(design);
			}
			foreach (DesignInfo designInfo in list)
			{
				if (designInfo.Role == role && (!nullable.HasValue || designInfo.DesignDate > num))
				{
					nullable = new int?(designInfo.ID);
					num = designInfo.DesignDate;
				}
			}
			return nullable;
		}

		private static WeaponBankInfo AssignBestWeaponToBank(
		  GameSession game,
		  LogicalBank bank,
		  IList<LogicalWeapon> availableWeapons,
		  ShipRole role,
		  WeaponRole wpnRole,
		  ShipSectionAsset section,
		  WeaponEnums.TurretClasses turretClass,
		  WeaponEnums.WeaponSizes turretSize,
		  int playerID,
		  AITechStyles optionalAITechStyles,
		  string playerFaction)
		{
			List<LogicalWeapon> list = LogicalWeapon.EnumerateWeaponFits(section.Faction, section.SectionName, (IEnumerable<LogicalWeapon>)availableWeapons, turretSize, turretClass).ToList<LogicalWeapon>();
			DesignLab.TraceVerbose(string.Format("Assigning best weapon fitting {0}, size={1}, turret class={2}:", (object)section.Faction, (object)turretSize, (object)turretClass));
			if (list.Count == 0)
				DesignLab.TraceVerbose("  None available");
			WeaponBankInfo weaponBankInfo = new WeaponBankInfo();
			weaponBankInfo.BankGUID = bank.GUID;
			if (list.Count > 0)
			{
				DesignLab.TraceVerbose(string.Format("  Scoring for {0}:", (object)wpnRole));
				weaponBankInfo.WeaponID = DesignLab.PickBestWeaponForRole(game, (IList<LogicalWeapon>)list, playerID, bank.TurretSize == WeaponEnums.WeaponSizes.Light ? WeaponRole.POINT_DEFENSE : wpnRole, optionalAITechStyles, turretClass, playerFaction);
				if (App.Log.Level >= Kerberos.Sots.Engine.LogLevel.Verbose)
				{
					if (!weaponBankInfo.WeaponID.HasValue)
					{
						DesignLab.TraceVerbose("Selected weapon: none");
					}
					else
					{
						string weaponAsset = game.GameDatabase.GetWeaponAsset(weaponBankInfo.WeaponID.Value);
						if (weaponAsset != null)
							DesignLab.TraceVerbose("Selected weapon: " + weaponAsset);
						else
							DesignLab.TraceVerbose(string.Format("Selected weapon: (not in db: {0})", (object)weaponBankInfo.WeaponID.Value));
					}
				}
			}
			return weaponBankInfo;
		}

		private static float PickTechStylesWeaponMultiplier(
		  LogicalWeapon wpn,
		  AITechStyles optionalTechStyles)
		{
			if (optionalTechStyles == null)
				return 1f;
			foreach (Kerberos.Sots.Data.WeaponFramework.Tech requiredTech in wpn.RequiredTechs)
			{
				Kerberos.Sots.Data.WeaponFramework.Tech required = requiredTech;
				if (optionalTechStyles.TechUnion.Any<Kerberos.Sots.Data.TechnologyFramework.Tech>((Func<Kerberos.Sots.Data.TechnologyFramework.Tech, bool>)(x => x.Id == required.Name)))
					return 2f;
			}
			return 1f;
		}

		private static float PickFactionWeaponModifier(
		  LogicalWeapon wpn,
		  WeaponEnums.TurretClasses weaponType,
		  string faction)
		{
			switch (faction)
			{
				case "hiver":
					if (((IEnumerable<WeaponEnums.WeaponTraits>)wpn.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.Ballistic) || wpn.PayloadType == WeaponEnums.PayloadTypes.Missile)
						return 2f;
					break;
				case "human":
					if (!((IEnumerable<WeaponEnums.WeaponTraits>)wpn.Traits).Any<WeaponEnums.WeaponTraits>((Func<WeaponEnums.WeaponTraits, bool>)(x =>
				  {
					  if (x != WeaponEnums.WeaponTraits.Ballistic)
						  return x == WeaponEnums.WeaponTraits.Energy;
					  return true;
				  })) && (weaponType != WeaponEnums.TurretClasses.Torpedo || ((IEnumerable<WeaponEnums.WeaponTraits>)wpn.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.Tracking)))
						return 0.5f;
					break;
				case "liir_zuul":
					if (weaponType == WeaponEnums.TurretClasses.Torpedo || ((IEnumerable<WeaponEnums.WeaponTraits>)wpn.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.Energy))
						return 2f;
					if (wpn.PayloadType == WeaponEnums.PayloadTypes.Missile)
						return 0.5f;
					break;
				case "loa":
					float num = 0.0f;
					if (((IEnumerable<WeaponEnums.WeaponTraits>)wpn.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.Brawler))
						++num;
					if (((IEnumerable<WeaponEnums.WeaponTraits>)wpn.Traits).Any<WeaponEnums.WeaponTraits>((Func<WeaponEnums.WeaponTraits, bool>)(x =>
				  {
					  if (x != WeaponEnums.WeaponTraits.Energy)
						  return x == WeaponEnums.WeaponTraits.Draining;
					  return true;
				  })) || wpn.PayloadType == WeaponEnums.PayloadTypes.Torpedo)
						num += 2f;
					if ((double)num > 0.0)
						return num + 1f;
					break;
				case "morrigi":
					if (((IEnumerable<WeaponEnums.WeaponTraits>)wpn.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.Energy) || wpn.PayloadType == WeaponEnums.PayloadTypes.BattleRider)
						return 2f;
					break;
				case "tarkas":
					if (((IEnumerable<WeaponEnums.WeaponTraits>)wpn.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.Ballistic))
						return 2f;
					if (((IEnumerable<WeaponEnums.WeaponTraits>)wpn.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.Energy))
						return 0.5f;
					break;
				case "zuul":
					if (((IEnumerable<WeaponEnums.WeaponTraits>)wpn.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.Ballistic) || wpn.PayloadType == WeaponEnums.PayloadTypes.Missile)
						return 3f;
					break;
			}
			return 1f;
		}

		private static float PickHighDamageWeaponScore(AssetDatabase assetdb, LogicalWeapon wpn)
		{
			float num1 = 0.0f;
			if (wpn.IsPDWeapon())
				return num1;
			int num2 = 1;
			LogicalWeapon logicalWeapon1 = wpn;
			if (!string.IsNullOrEmpty(wpn.SubWeapon) && wpn.NumSubWeapons > 0)
			{
				LogicalWeapon logicalWeapon2 = assetdb.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.WeaponName == wpn.SubWeapon));
				if (logicalWeapon2 != null)
				{
					logicalWeapon1 = logicalWeapon2;
					num2 = wpn.NumSubWeapons;
				}
			}
			float num3;
			if (logicalWeapon1.PayloadType == WeaponEnums.PayloadTypes.DOTCloud)
			{
				float num4 = (float)(1.0 + (double)logicalWeapon1.RangeTable.Maximum.Range / 500.0);
				num3 = logicalWeapon1.DOTDamage * (logicalWeapon1.TimeToLive / 0.5f) / wpn.RechargeTime * num4 * (float)num2;
				DesignLab.TraceVerbose(string.Format("  High Damage (DOT): [DOTDam*TimeToLive/0.5/RechTime(1+MaxRange/500)] [{0}*{1}/0.5/{2}*(1+{3}/500)]", (object)logicalWeapon1.DOTDamage, (object)logicalWeapon1.TimeToLive, (object)wpn.RechargeTime, (object)logicalWeapon1.RangeTable.Maximum.Range));
				DesignLab.TraceVerbose(string.Format("  {0,20}: {1,10}", (object)wpn.ToString(), (object)num3));
			}
			else if (logicalWeapon1.PayloadType == WeaponEnums.PayloadTypes.Emitter)
			{
				num3 = logicalWeapon1.RangeTable.Effective.Damage * logicalWeapon1.Duration / logicalWeapon1.BeamDamagePeriod / logicalWeapon1.RechargeTime * (float)num2;
				DesignLab.TraceVerbose(string.Format("  High Damage (Emitter): [EffDam*Duration/BeamDamPeriod/RechTime] [{0}*{1}/{2}/{3}]", (object)logicalWeapon1.RangeTable.Effective.Damage, (object)logicalWeapon1.Duration, (object)logicalWeapon1.BeamDamagePeriod, (object)logicalWeapon1.RechargeTime));
				DesignLab.TraceVerbose(string.Format("  {0,20}: {1,10}", (object)wpn.ToString(), (object)num3));
			}
			else if (logicalWeapon1.PayloadType == WeaponEnums.PayloadTypes.Beam)
			{
				num3 = (float)((double)logicalWeapon1.RangeTable.Effective.Damage * (double)logicalWeapon1.Duration / (double)logicalWeapon1.BeamDamagePeriod / (double)logicalWeapon1.RechargeTime / ((double)logicalWeapon1.RangeTable.Effective.Deviation + 1.0)) * (float)num2;
				DesignLab.TraceVerbose(string.Format("  High Damage (Beam): [EffDam*Duration/BeamDamPeriod/RechTime/(EffDev+1)] [{0}*{1}/{2}/{3}/({4}+1)]", (object)logicalWeapon1.RangeTable.Effective.Damage, (object)logicalWeapon1.Duration, (object)logicalWeapon1.BeamDamagePeriod, (object)logicalWeapon1.RechargeTime, (object)logicalWeapon1.RangeTable.Effective.Deviation));
				DesignLab.TraceVerbose(string.Format("  {0,20}: {1,10}", (object)wpn.ToString(), (object)num3));
			}
			else
			{
				float num4 = (float)(1.0 - 1.0 * (double)Math.Abs(logicalWeapon1.VolleyDeviation).Normalize(0.0f, 10f));
				float num5 = 1f + (float)logicalWeapon1.ArmorPiercingLevel;
				num3 = (float)((double)logicalWeapon1.RangeTable.Effective.Damage * (double)logicalWeapon1.NumVolleys / (double)logicalWeapon1.RechargeTime / ((double)logicalWeapon1.RangeTable.Effective.Deviation + 1.0)) * num4 * num5 * (float)num2;
				DesignLab.TraceVerbose(string.Format("  High Damage (Std): [EffDam*NumVolley/RechTime/(EffDev+1)*(1-1*VolleyDev[norm])*(1+APLvl)] [{0}*{1}/{2}/({3}+1)*(1-1*{4}[norm])*(1+{5})]", (object)logicalWeapon1.RangeTable.Effective.Damage, (object)logicalWeapon1.NumVolleys, (object)logicalWeapon1.RechargeTime, (object)logicalWeapon1.RangeTable.Effective.Deviation, (object)Math.Abs(logicalWeapon1.VolleyDeviation), (object)logicalWeapon1.ArmorPiercingLevel));
				DesignLab.TraceVerbose(string.Format("  {0,20}: {1,10}", (object)wpn.ToString(), (object)num3));
			}
			return num3;
		}

		private static float PickPointDefenseWeaponScore(LogicalWeapon wpn)
		{
			return (float)(100.0 / (double)wpn.RechargeTime + (double)wpn.RangeTable.Effective.Damage / 1024.0);
		}

		private static float PickPlanetAttackWeaponScore(LogicalWeapon wpn)
		{
			float num1 = 1f + (float)wpn.ArmorPiercingLevel;
			float num2 = wpn.PopDamage / wpn.RechargeTime * num1;
			DesignLab.TraceVerbose(string.Format("  Planet Attack: [PopDam/RchTime*(1+APLvl)] [{0}/{1}*(1+{2})]", new object[3]
			{
		(object) wpn.PopDamage,
		(object) wpn.RechargeTime,
		(object) wpn.ArmorPiercingLevel
			}));
			DesignLab.TraceVerbose(string.Format("  {0,20}: {1,10}", (object)wpn.ToString(), (object)num2));
			return num2;
		}

		private static float PickDisablingWeaponScore(AssetDatabase assetdb, LogicalWeapon wpn)
		{
			float num1 = DesignLab.PickHighDamageWeaponScore(assetdb, wpn);
			bool flag1 = ((IEnumerable<WeaponEnums.WeaponTraits>)wpn.Traits).Any<WeaponEnums.WeaponTraits>((Func<WeaponEnums.WeaponTraits, bool>)(x => x == WeaponEnums.WeaponTraits.Disabling));
			bool flag2 = ((IEnumerable<WeaponEnums.WeaponTraits>)wpn.Traits).Any<WeaponEnums.WeaponTraits>((Func<WeaponEnums.WeaponTraits, bool>)(x => x == WeaponEnums.WeaponTraits.Draining));
			float num2 = num1 * (float)((flag1 ? 10.0 : 1.0) + (flag2 ? 10.0 : 1.0));
			DesignLab.TraceVerbose(string.Format("  Disabling: [HighDamage * (10 if Disabling + 10 if Draining)] [{0} * (10 {1} + 10 {2})]", new object[3]
			{
		(object) num1,
		(object) flag1,
		(object) flag2
			}));
			DesignLab.TraceVerbose(string.Format("  {0,20}: {1,10}", (object)wpn.ToString(), (object)num2));
			return num2;
		}

		private static float PickLongRangeWeaponScore(LogicalWeapon wpn)
		{
			float num1 = 1f + (float)wpn.ArmorPiercingLevel;
			float num2 = (float)((double)wpn.RangeTable.Maximum.Range * (double)wpn.RangeTable.Maximum.Damage / (double)wpn.RechargeTime / (1.0 + (double)wpn.RangeTable.Maximum.Deviation)) * num1;
			DesignLab.TraceVerbose(string.Format("  Long Range: [maxRange*maxDam/RechTime/(1+MaxDev)*(1+APLvl)] [{0}*{1}/{2}/(1+{3})*(1+{4})]", (object)wpn.RangeTable.Maximum.Range, (object)wpn.RangeTable.Maximum.Damage, (object)wpn.RechargeTime, (object)wpn.RangeTable.Maximum.Deviation, (object)wpn.ArmorPiercingLevel));
			DesignLab.TraceVerbose(string.Format("  {0,20}: {1,10}", (object)wpn.ToString(), (object)num2));
			return num2;
		}

		private static float PickWeaponScore(
		  AssetDatabase assetdb,
		  LogicalWeapon weapon,
		  WeaponRole role,
		  AITechStyles optionalTechStyles,
		  WeaponEnums.TurretClasses weaponType,
		  string playerFaction)
		{
			float num1;
			switch (role)
			{
				case WeaponRole.STAND_OFF:
					num1 = DesignLab.PickLongRangeWeaponScore(weapon);
					break;
				case WeaponRole.BRAWLER:
					num1 = DesignLab.PickHighDamageWeaponScore(assetdb, weapon);
					break;
				case WeaponRole.POINT_DEFENSE:
					num1 = DesignLab.PickPointDefenseWeaponScore(weapon);
					break;
				case WeaponRole.PLANET_ATTACK:
					num1 = DesignLab.PickPlanetAttackWeaponScore(weapon);
					break;
				case WeaponRole.DISABLING:
					num1 = DesignLab.PickDisablingWeaponScore(assetdb, weapon);
					break;
				case WeaponRole.ENERGY:
					num1 = DesignLab.PickHighDamageWeaponScore(assetdb, weapon);
					bool flag1 = false;
					if (weapon.PayloadType == WeaponEnums.PayloadTypes.Bolt && ((IEnumerable<WeaponEnums.WeaponTraits>)weapon.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.Energy))
						flag1 = true;
					DesignLab.TraceVerbose(string.Format("  Energy: [HighDamage * 100 if Bolt or Energy Traits] [{0} * (100 {1})]", new object[2]
					{
			(object) num1,
			(object) flag1
					}));
					if (flag1)
						num1 *= 100f;
					DesignLab.TraceVerbose(string.Format("  {0,20}: {1,10}", (object)weapon.ToString(), (object)num1));
					break;
				case WeaponRole.BALLISTICS:
					num1 = DesignLab.PickHighDamageWeaponScore(assetdb, weapon);
					bool flag2 = false;
					if (weapon.PayloadType == WeaponEnums.PayloadTypes.Bolt && ((IEnumerable<WeaponEnums.WeaponTraits>)weapon.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.Ballistic))
						flag2 = true;
					DesignLab.TraceVerbose(string.Format("  Ballistics: [HighDamage * 100 if Bolt or Ballistic Traits] [{0} * (100 {1})]", new object[2]
					{
			(object) num1,
			(object) flag2
					}));
					if (flag2)
						num1 *= 100f;
					DesignLab.TraceVerbose(string.Format("  {0,20}: {1,10}", (object)weapon.ToString(), (object)num1));
					break;
				default:
					num1 = DesignLab.PickLongRangeWeaponScore(weapon);
					break;
			}
			float num2 = DesignLab.PickTechStylesWeaponMultiplier(weapon, optionalTechStyles);
			if (role != WeaponRole.POINT_DEFENSE)
				DesignLab.TraceVerbose(string.Format("  TechStyleModifier: x{0} = {1}", new object[2]
				{
		  (object) num2,
		  (object) (float) ((double) num1 * (double) num2)
				}));
			float num3 = num1 * num2;
			float num4 = DesignLab.PickFactionWeaponModifier(weapon, weaponType, playerFaction);
			if (role != WeaponRole.POINT_DEFENSE)
				DesignLab.TraceVerbose(string.Format("  FactionModifier: x{0} = [[ {1} ]]", new object[2]
				{
		  (object) num4,
		  (object) (float) ((double) num3 * (double) num4)
				}));
			return num3 * num4;
		}

		public static int? PickBestWeaponForRole(
		  GameSession game,
		  IList<LogicalWeapon> weapons,
		  int PlayerId,
		  WeaponRole role,
		  AITechStyles optionalAITechStyles,
		  WeaponEnums.TurretClasses bankType,
		  string playerFaction)
		{
			DesignLab.WeaponScore weaponScore = weapons.Select<LogicalWeapon, DesignLab.WeaponScore>((Func<LogicalWeapon, DesignLab.WeaponScore>)(x => new DesignLab.WeaponScore()
			{
				Weapon = x,
				Score = DesignLab.PickWeaponScore(game.AssetDatabase, x, role, optionalAITechStyles, bankType, playerFaction)
			})).OrderByDescending<DesignLab.WeaponScore, float>((Func<DesignLab.WeaponScore, float>)(y => y.Score)).First<DesignLab.WeaponScore>();
			return game.GameDatabase.GetWeaponID(weaponScore.Weapon.FileName, PlayerId);
		}

		private static ShipPreference[] LoadSectionPreferences(GameSession game)
		{
			List<ShipPreference> shipPreferenceList = new List<ShipPreference>();
			foreach (string[] strArray in CsvOperations.Read(ScriptHost.FileSystem, "factions\\section_preferences.csv", '"', ',', 0, 3))
			{
				ShipPreference shipPreference;
				shipPreference.factionID = game.GameDatabase.GetFactionIdFromName(strArray[0]);
				shipPreference.sectionName = strArray[1];
				shipPreference.preferenceWeight = float.Parse(strArray[2]);
				shipPreferenceList.Add(shipPreference);
			}
			return shipPreferenceList.ToArray();
		}

		private static bool AreSectionsCompatible(ShipSectionAsset section1, ShipSectionAsset section2)
		{
			return !((IEnumerable<ShipSectionType>)section1.ExcludeSectionTypes).Contains<ShipSectionType>(section2.Type) && !((IEnumerable<ShipSectionType>)section2.ExcludeSectionTypes).Contains<ShipSectionType>(section1.Type) && (!section1.SectionIsExcluded(section2) && !section2.SectionIsExcluded(section1));
		}

		private static bool ShipClassWantsAdditionalSections(
		  ShipClass value,
		  ShipSectionType sectionType)
		{
			if (sectionType == ShipSectionType.Mission)
				return true;
			switch (value)
			{
				case ShipClass.Leviathan:
				case ShipClass.Station:
					return false;
				default:
					return true;
			}
		}

		public static void SummarizeDesign(
		  AssetDatabase assetDatabase,
		  GameDatabase gameDatabase,
		  DesignInfo design)
		{
			foreach (DesignSectionInfo designSection in design.DesignSections)
			{
				designSection.DesignInfo = design;
				designSection.ShipSectionAsset = assetDatabase.GetShipSectionAsset(designSection.FilePath);
			}
			IEnumerable<ShipSectionAsset> source1 = ((IEnumerable<DesignSectionInfo>)design.DesignSections).Select<DesignSectionInfo, ShipSectionAsset>((Func<DesignSectionInfo, ShipSectionAsset>)(y => y.ShipSectionAsset)).Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.Type == ShipSectionType.Mission));
			if (source1.Count<ShipSectionAsset>() != 1)
				throw new InvalidOperationException("Ship design requires exactly one mission section.");
			ShipSectionAsset missionsection = source1.First<ShipSectionAsset>();
			int num1 = !DesignLab.ShipClassWantsAdditionalSections(missionsection.Class, ShipSectionType.Command) ? 0 : (!((IEnumerable<ShipSectionType>)missionsection.ExcludeSectionTypes).Contains<ShipSectionType>(ShipSectionType.Command) ? 1 : 0);
			IEnumerable<ShipSectionAsset> source2 = ((IEnumerable<DesignSectionInfo>)design.DesignSections).Select<DesignSectionInfo, ShipSectionAsset>((Func<DesignSectionInfo, ShipSectionAsset>)(y => y.ShipSectionAsset)).Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.Type == ShipSectionType.Command));
			if (num1 != 0)
			{
				if (source2.Count<ShipSectionAsset>() != 1)
					throw new InvalidOperationException("Ship design requires exactly one command section.");
			}
			else if (source2.Any<ShipSectionAsset>())
				throw new InvalidOperationException("Ship design cannot have a command section.");
			int num2 = !DesignLab.ShipClassWantsAdditionalSections(missionsection.Class, ShipSectionType.Engine) ? 0 : (!((IEnumerable<ShipSectionType>)missionsection.ExcludeSectionTypes).Contains<ShipSectionType>(ShipSectionType.Engine) ? 1 : 0);
			IEnumerable<ShipSectionAsset> source3 = ((IEnumerable<DesignSectionInfo>)design.DesignSections).Select<DesignSectionInfo, ShipSectionAsset>((Func<DesignSectionInfo, ShipSectionAsset>)(y => y.ShipSectionAsset)).Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.Type == ShipSectionType.Engine));
			if (num2 != 0)
			{
				if (source3.Count<ShipSectionAsset>() != 1)
					throw new InvalidOperationException("Ship design requires exactly one engine section.");
			}
			else if (source3.Any<ShipSectionAsset>())
				throw new InvalidOperationException("Ship design cannot have an engine section.");
			design.SavingsCost = 0;
			design.StratSensorRange = 0.0f;
			design.ProductionCost = 0;
			design.Armour = ((IEnumerable<DesignSectionInfo>)design.DesignSections).Sum<DesignSectionInfo>((Func<DesignSectionInfo, int>)(x => ((IEnumerable<Kerberos.Sots.Framework.Size>)x.ShipSectionAsset.Armor).Sum<Kerberos.Sots.Framework.Size>((Func<Kerberos.Sots.Framework.Size, int>)(y => y.X * y.Y))));
			design.Structure = (float)((IEnumerable<DesignSectionInfo>)design.DesignSections).Sum<DesignSectionInfo>((Func<DesignSectionInfo, int>)(x => x.ShipSectionAsset.Structure));
			design.NumTurrets = ((IEnumerable<DesignSectionInfo>)design.DesignSections).Sum<DesignSectionInfo>((Func<DesignSectionInfo, int>)(x => x.ShipSectionAsset.Mounts.Length));
			design.Mass = ((IEnumerable<DesignSectionInfo>)design.DesignSections).Sum<DesignSectionInfo>((Func<DesignSectionInfo, float>)(x => x.ShipSectionAsset.Mass));
			design.Acceleration = ((IEnumerable<DesignSectionInfo>)design.DesignSections).Sum<DesignSectionInfo>((Func<DesignSectionInfo, float>)(x => x.ShipSectionAsset.Maneuvering.LinearAccel));
			design.TopSpeed = ((IEnumerable<DesignSectionInfo>)design.DesignSections).Sum<DesignSectionInfo>((Func<DesignSectionInfo, float>)(x => x.ShipSectionAsset.Maneuvering.LinearSpeed));
			design.Class = missionsection.Class;
			design.DesignDate = gameDatabase.GetTurnCount();
			if (design.DesignSections[0].ShipSectionAsset.StationType == StationType.INVALID_TYPE && design.Class != ShipClass.Station && (design.StationType == StationType.INVALID_TYPE && design.Role == ShipRole.UNDEFINED))
				design.Role = DesignLab.GetRole(missionsection);
			design.CrewAvailable = 0;
			design.PowerAvailable = 0;
			design.SupplyAvailable = 0;
			design.CrewRequired = 0;
			design.PowerRequired = 0;
			design.SupplyRequired = 0;
			string name = gameDatabase.AssetDatabase.GetFaction(gameDatabase.GetPlayerFactionID(design.PlayerID)).Name;
			List<PlayerTechInfo> list1 = gameDatabase.GetPlayerTechInfos(design.PlayerID).Where<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.State == TechStates.Researched)).ToList<PlayerTechInfo>();
			foreach (DesignSectionInfo designSection in design.DesignSections)
			{
				DesignSectionInfo designSectionInfo = designSection;
				List<string> techs = new List<string>();
				if (designSectionInfo.Techs != null)
				{
					foreach (int tech in designSectionInfo.Techs)
						techs.Add(gameDatabase.GetTechFileID(tech));
				}
				ShipSectionAsset sectionAsset = designSectionInfo.ShipSectionAsset;
				double savingsCost = (double)sectionAsset.SavingsCost;
				design.CrewAvailable += sectionAsset.Crew;
				design.PowerAvailable += Ship.GetPowerWithTech(assetDatabase, techs, list1, sectionAsset.Power);
				design.SupplyAvailable += Ship.GetSupplyWithTech(assetDatabase, techs, sectionAsset.Supply);
				int crewRequired = sectionAsset.CrewRequired;
				double num3 = 0.0;
				if (designSectionInfo.Techs != null)
				{
					foreach (int tech1 in designSectionInfo.Techs)
					{
						string techIdentifier = gameDatabase.GetTechFileID(tech1);
						Kerberos.Sots.Data.TechnologyFramework.Tech tech2 = assetDatabase.MasterTechTree.Technologies.First<Kerberos.Sots.Data.TechnologyFramework.Tech>((Func<Kerberos.Sots.Data.TechnologyFramework.Tech, bool>)(x => x.Id == techIdentifier));
						num3 += Math.Max((double)tech2.CostMultiplier - 1.0, 0.0);
					}
				}
				design.ProductionCost += sectionAsset.ProductionCost;
				if (name == "loa")
					design.ProductionCost += (int)((double)sectionAsset.ProductionCost * (num3 * (double)gameDatabase.AssetDatabase.LoaTechModMod));
				double num4 = savingsCost + savingsCost * num3;
				float num5 = 1f;
				if (designSectionInfo.Modules != null)
				{
					foreach (DesignModuleInfo module1 in designSectionInfo.Modules)
					{
						string modulePath = gameDatabase.GetModuleAsset(module1.ModuleID);
						LogicalModule module = assetDatabase.Modules.First<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == modulePath));
						if ((double)module.CrewEfficiencyBonus > 0.0)
							num5 *= module.CrewEfficiencyBonus;
						design.CrewAvailable += module.Crew;
						design.PowerAvailable += module.PowerBonus;
						design.SupplyAvailable += module.Supply;
						crewRequired += module.CrewRequired;
						num4 += (double)module.SavingsCost;
						design.ProductionCost += module.ProductionCost;
						int? weaponId = module1.WeaponID;
						if (weaponId.HasValue)
						{
							string weaponPath = gameDatabase.GetWeaponAsset(weaponId.Value);
							LogicalWeapon logicalWeapon = assetDatabase.Weapons.First<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.FileName == weaponPath));
							int num6 = ((IEnumerable<LogicalMount>)module.Mounts).Count<LogicalMount>((Func<LogicalMount, bool>)(x => x.Bank == module.Banks[0]));
							crewRequired += logicalWeapon.isCrewPerBank ? logicalWeapon.Crew : logicalWeapon.Crew * num6;
							design.PowerRequired += logicalWeapon.isPowerPerBank ? logicalWeapon.Power : logicalWeapon.Power * num6;
							design.SupplyRequired += logicalWeapon.isSupplyPerBank ? logicalWeapon.Supply : logicalWeapon.Supply * num6;
						}
						if (design.StationType != StationType.INVALID_TYPE)
						{
							if (design.StationType == StationType.DIPLOMATIC && design.StationLevel > 0)
							{
								ModuleEnums.StationModuleType? stationModuleType1 = module1.StationModuleType;
								ModuleEnums.StationModuleType stationModuleType2 = ModuleEnums.StationModuleType.Sensor;
								if (stationModuleType1.GetValueOrDefault() == stationModuleType2 & stationModuleType1.HasValue)
								{
									design.StratSensorRange += 0.2f;
									design.TacSensorRange += 200f;
								}
							}
							else if (design.StationType == StationType.NAVAL && design.StationLevel > 0)
							{
								ModuleEnums.StationModuleType? stationModuleType1 = module1.StationModuleType;
								ModuleEnums.StationModuleType stationModuleType2 = ModuleEnums.StationModuleType.Sensor;
								if (stationModuleType1.GetValueOrDefault() == stationModuleType2 & stationModuleType1.HasValue)
								{
									design.StratSensorRange += 0.5f;
									design.TacSensorRange += 500f;
								}
							}
							else if (design.StationType == StationType.SCIENCE && design.StationLevel > 0)
							{
								ModuleEnums.StationModuleType? stationModuleType1 = module1.StationModuleType;
								ModuleEnums.StationModuleType stationModuleType2 = ModuleEnums.StationModuleType.Sensor;
								if (stationModuleType1.GetValueOrDefault() == stationModuleType2 & stationModuleType1.HasValue)
								{
									design.StratSensorRange += 0.25f;
									design.TacSensorRange += 250f;
								}
							}
							else if (design.StationType == StationType.CIVILIAN && design.StationLevel > 0)
							{
								ModuleEnums.StationModuleType? stationModuleType1 = module1.StationModuleType;
								ModuleEnums.StationModuleType stationModuleType2 = ModuleEnums.StationModuleType.Sensor;
								if (stationModuleType1.GetValueOrDefault() == stationModuleType2 & stationModuleType1.HasValue)
								{
									design.StratSensorRange += 0.25f;
									design.TacSensorRange += 500f;
								}
							}
							else if (design.StationType == StationType.GATE && design.StationLevel > 0)
							{
								ModuleEnums.StationModuleType? stationModuleType = module1.StationModuleType;
								ModuleEnums.StationModuleType valueOrDefault = stationModuleType.GetValueOrDefault();
								if (stationModuleType.HasValue)
								{
									switch (valueOrDefault)
									{
										case ModuleEnums.StationModuleType.Sensor:
											design.StratSensorRange += 0.5f;
											design.TacSensorRange += 500f;
											continue;
										case ModuleEnums.StationModuleType.Bastion:
											design.Structure += design.Structure * 0.1f;
											continue;
										default:
											continue;
									}
								}
							}
						}
					}
				}
				if (designSectionInfo.WeaponBanks != null)
				{
					List<WeaponBankInfo> list2 = designSectionInfo.WeaponBanks.ToList<WeaponBankInfo>();
					for (int iBank = 0; iBank < sectionAsset.Banks.Length; iBank++)
					{
						if (iBank < list2.Count)
						{
							int? weaponId = list2[iBank].WeaponID;
							if (weaponId.HasValue && weaponId.Value != 0)
							{
								string weaponPath = gameDatabase.GetWeaponAsset(list2[iBank].WeaponID.Value);
								LogicalWeapon logicalWeapon = assetDatabase.Weapons.First<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.FileName == weaponPath));
								int num6 = ((IEnumerable<LogicalMount>)sectionAsset.Mounts).Count<LogicalMount>((Func<LogicalMount, bool>)(x => x.Bank == sectionAsset.Banks[iBank]));
								int num7 = WeaponSizes.GuessNumBarrels(logicalWeapon.DefaultWeaponSize, sectionAsset.Banks[iBank].TurretSize);
								crewRequired += logicalWeapon.isCrewPerBank ? logicalWeapon.Crew : logicalWeapon.Crew * num6 * num7;
								design.PowerRequired += logicalWeapon.isPowerPerBank ? logicalWeapon.Power : logicalWeapon.Power * num6 * num7;
								design.SupplyRequired += logicalWeapon.isSupplyPerBank ? logicalWeapon.Supply : logicalWeapon.Supply * num6 * num7;
								num4 += (double)(logicalWeapon.Cost * num7 * num6);
								if (gameDatabase.AssetDatabase.GetFaction(gameDatabase.GetPlayerFactionID(design.PlayerID)).Name == "loa")
									design.ProductionCost += (int)((double)(logicalWeapon.Cost * num7 * num6) * 0.100000001490116);
								if (list2[iBank].DesignID.HasValue)
								{
									DesignInfo designInfo = gameDatabase.GetDesignInfo(list2[iBank].DesignID.Value);
									if (designInfo != null)
										num4 += (double)(designInfo.SavingsCost * num6);
								}
							}
						}
					}
				}
				design.CrewRequired += (int)Math.Round((double)crewRequired * (double)num5);
				design.SavingsCost += (int)num4;
			}
			design.SupplyRequired += (int)Math.Ceiling((double)design.CrewAvailable / 2.0);
			if (string.IsNullOrEmpty(design.Name))
			{
				ShipRole role = design.Role;
				DesignLab.NameGenerators nameGenerator = (uint)(role - 13) <= 1U || (uint)(role - 16) <= 2U ? DesignLab.NameGenerators.MissionSectionDerived : DesignLab.NameGenerators.FactionRandom;
				design.Name = DesignLab.GenerateDesignName(assetDatabase, gameDatabase, (DesignInfo)null, design, nameGenerator);
			}
			if (missionsection.IsGateShip)
				design.Role = ShipRole.GATE;
			else if (missionsection.IsBoreShip)
				design.Role = ShipRole.BORE;
			else if (missionsection.IsFreighter)
				design.Role = ShipRole.FREIGHTER;
			else if (missionsection.isConstructor)
				design.Role = ShipRole.CONSTRUCTOR;
			else if (missionsection.IsGravBoat)
				design.Role = ShipRole.GRAVBOAT;
			else if (missionsection.CombatAIType == SectionEnumerations.CombatAiType.TrapDrone)
				design.Role = ShipRole.TRAPDRONE;
			else if (missionsection.RealClass == RealShipClasses.AssaultShuttle)
				design.Role = missionsection.SlaveCapacity > 0 ? ShipRole.SLAVEDISK : ShipRole.ASSAULTSHUTTLE;
			else if (missionsection.RealClass == RealShipClasses.BoardingPod)
				design.Role = ShipRole.BOARDINGPOD;
			else if (missionsection.RealClass == RealShipClasses.Drone)
				design.Role = ShipRole.DRONE;
			else if (missionsection.RealClass == RealShipClasses.Biomissile)
				design.Role = ShipRole.BIOMISSILE;
			else if (missionsection.IsScavenger)
				design.Role = ShipRole.SCAVENGER;
			else if (missionsection.BattleRiderType == BattleRiderTypes.escort)
				design.Role = ShipRole.BR_ESCORT;
			else if (missionsection.BattleRiderType == BattleRiderTypes.interceptor)
				design.Role = ShipRole.BR_INTERCEPTOR;
			else if (missionsection.BattleRiderType == BattleRiderTypes.patrol)
				design.Role = ShipRole.BR_PATROL;
			else if (missionsection.BattleRiderType == BattleRiderTypes.scout)
				design.Role = ShipRole.BR_SCOUT;
			else if (missionsection.BattleRiderType == BattleRiderTypes.spinal)
				design.Role = ShipRole.BR_SPINAL;
			else if (missionsection.BattleRiderType == BattleRiderTypes.torpedo)
				design.Role = ShipRole.BR_TORPEDO;
			else if (missionsection.RealClass == RealShipClasses.BattleCruiser)
			{
				design.Role = ShipRole.BATTLECRUISER;
			}
			else
			{
				if (missionsection.RealClass != RealShipClasses.BattleShip)
					return;
				design.Role = ShipRole.BATTLESHIP;
			}
		}

		internal static string GenerateDesignName(
		  AssetDatabase assetdb,
		  GameDatabase db,
		  DesignInfo predecessor,
		  DesignInfo newDesign,
		  DesignLab.NameGenerators nameGenerator)
		{
			if (predecessor != null)
				return predecessor.Name;
			PlayerInfo playerInfo = db.GetPlayerInfo(newDesign.PlayerID);
			if (nameGenerator != DesignLab.NameGenerators.FactionRandom)
				return App.Localize(((IEnumerable<DesignSectionInfo>)newDesign.DesignSections).Select<DesignSectionInfo, ShipSectionAsset>((Func<DesignSectionInfo, ShipSectionAsset>)(x => assetdb.GetShipSectionAsset(x.FilePath))).First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(y => y.Type == ShipSectionType.Mission)).Title);
			FactionInfo factionInfo = db.GetFactionInfo(playerInfo.FactionID);
			return AssetDatabase.CommonStrings.Localize(assetdb.GetFaction(factionInfo.Name).DesignNames.GetNextStringID());
		}

		public static DesignInfo GetStationDesignInfo(
		  AssetDatabase assetdb,
		  GameDatabase gamedb,
		  int playerId,
		  StationType type,
		  int level)
		{
			string str = "";
			if (level == 0)
			{
				str = "sn_underconstruction.section";
			}
			else
			{
				switch (type)
				{
					case StationType.NAVAL:
						switch (level)
						{
							case 1:
								str = "sn_naval_outpost.section";
								break;
							case 2:
								str = "sn_naval_forward_base.section";
								break;
							case 3:
								str = "sn_naval_naval_base.section";
								break;
							case 4:
								str = "sn_naval_star_base.section";
								break;
							case 5:
								str = "sn_naval_sector_base.section";
								break;
						}
						break;
					case StationType.SCIENCE:
						switch (level)
						{
							case 1:
								str = "sn_science_field_station.section";
								break;
							case 2:
								str = "sn_science_star_lab.section";
								break;
							case 3:
								str = "sn_science_research_base.section";
								break;
							case 4:
								str = "sn_science_polytechnic_institute.section";
								break;
							case 5:
								str = "sn_science_science_center.section";
								break;
						}
						break;
					case StationType.CIVILIAN:
						switch (level)
						{
							case 1:
								str = "sn_civilian_way_station.section";
								break;
							case 2:
								str = "sn_civilian_trading_post.section";
								break;
							case 3:
								str = "sn_civilian_merchanter_station.section";
								break;
							case 4:
								str = "sn_civilian_nexus.section";
								break;
							case 5:
								str = "sn_civilian_star_city.section";
								break;
						}
						break;
					case StationType.DIPLOMATIC:
						switch (level)
						{
							case 1:
								str = "sn_diplomatic_customs_station.section";
								break;
							case 2:
								str = "sn_diplomatic_consulate.section";
								break;
							case 3:
								str = "sn_diplomatic_embassy.section";
								break;
							case 4:
								str = "sn_diplomatic_council_station.section";
								break;
							case 5:
								str = "sn_diplomatic_star_chamber.section";
								break;
						}
						break;
					case StationType.GATE:
						switch (level)
						{
							case 1:
								str = "sn_gate_gateway.section";
								break;
							case 2:
								str = "sn_gate_caster.section";
								break;
							case 3:
								str = "sn_gate_far_caster.section";
								break;
							case 4:
								str = "sn_gate_lens.section";
								break;
							case 5:
								str = "sn_gate_mirror_of_creation.section";
								break;
						}
						break;
					case StationType.MINING:
						str = "sn_mining_station.section";
						break;
					case StationType.DEFENCE:
						str = 0.ToString() + ".section";
						break;
				}
			}
			string fullPath = string.Format("factions\\{0}\\sections\\{1}", (object)gamedb.GetFactionInfo(gamedb.GetPlayerFactionID(playerId)).Name, (object)str);
			ShipSectionAsset shipSectionAsset = (ShipSectionAsset)null ?? assetdb.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == fullPath));
			DesignInfo design = new DesignInfo(playerId, App.Localize(shipSectionAsset.Title), new string[1]
			{
		fullPath
			});
			design.StationType = type;
			design.StationLevel = level;
			DesignLab.SummarizeDesign(assetdb, gamedb, design);
			return design;
		}

		public static List<WeaponRole> GetWeaponRolesForNewDesign(
		  AIStance stance,
		  ShipRole role,
		  ShipClass shipClass)
		{
			float[] numArray = new float[8];
			switch (stance)
			{
				case AIStance.EXPANDING:
					numArray[2] = 1f;
					numArray[1] = 2f;
					break;
				case AIStance.ARMING:
					numArray[2] = 1f;
					numArray[1] = 1.5f;
					numArray[3] = 1f;
					numArray[4] = 0.8f;
					numArray[5] = 0.5f;
					break;
				case AIStance.HUNKERING:
					numArray[2] = 1f;
					numArray[1] = 1.5f;
					numArray[3] = 1f;
					numArray[4] = 0.8f;
					numArray[5] = 0.5f;
					break;
				case AIStance.CONQUERING:
					numArray[2] = 1f;
					numArray[1] = 1.5f;
					numArray[3] = 1f;
					numArray[4] = 1.5f;
					numArray[5] = 0.5f;
					break;
				case AIStance.DESTROYING:
					numArray[2] = 1f;
					numArray[1] = 1.5f;
					numArray[3] = 1f;
					numArray[4] = 1.5f;
					numArray[5] = 0.5f;
					break;
				case AIStance.DEFENDING:
					numArray[2] = 1f;
					numArray[1] = 1.5f;
					numArray[3] = 1.5f;
					numArray[4] = 0.8f;
					numArray[5] = 1f;
					break;
			}
			switch (role)
			{
				case ShipRole.COMBAT:
				case ShipRole.POLICE:
				case ShipRole.PLATFORM:
				case ShipRole.BR_PATROL:
				case ShipRole.BR_SPINAL:
				case ShipRole.BR_INTERCEPTOR:
				case ShipRole.BR_TORPEDO:
				case ShipRole.BATTLECRUISER:
				case ShipRole.BATTLESHIP:
				case ShipRole.GRAVBOAT:
					numArray[2] *= 1.5f;
					break;
				case ShipRole.CARRIER:
				case ShipRole.CARRIER_ASSAULT:
				case ShipRole.CARRIER_DRONE:
				case ShipRole.CARRIER_BIO:
				case ShipRole.CARRIER_BOARDING:
					numArray[2] *= 0.0f;
					numArray[4] *= 1.5f;
					numArray[3] *= 2f;
					break;
				case ShipRole.COMMAND:
					numArray[2] *= 0.0f;
					numArray[4] *= 0.0f;
					numArray[5] *= 2f;
					break;
				case ShipRole.COLONIZER:
				case ShipRole.BR_SCOUT:
					numArray[2] *= 0.0f;
					numArray[3] *= 1.5f;
					numArray[4] *= 2f;
					break;
				case ShipRole.CONSTRUCTOR:
					numArray[2] *= 0.0f;
					numArray[4] *= 0.0f;
					numArray[3] *= 2f;
					break;
				case ShipRole.SCOUT:
				case ShipRole.BR_ESCORT:
					numArray[4] *= 0.0f;
					numArray[2] *= 0.5f;
					numArray[1] *= 1.5f;
					numArray[5] *= 3f;
					break;
				case ShipRole.SUPPLY:
					numArray[2] *= 0.0f;
					numArray[4] *= 0.0f;
					numArray[3] *= 2f;
					break;
			}
			List<WeaponRole> weaponRoleList = new List<WeaponRole>();
			for (int index = 0; index < 8; ++index)
			{
				if ((double)numArray[index] > 0.0)
					weaponRoleList.Add((WeaponRole)index);
			}
			if (weaponRoleList.Count == 0)
				weaponRoleList.Add(WeaponRole.STAND_OFF);
			return weaponRoleList;
		}

		public static WeaponRole SuggestWeaponRoleForNewDesign(
		  AIStance stance,
		  ShipRole role,
		  ShipClass shipClass)
		{
			float[] numArray = new float[8];
			switch (stance)
			{
				case AIStance.EXPANDING:
					numArray[2] = 1f;
					numArray[1] = 1f;
					break;
				case AIStance.ARMING:
					numArray[2] = 1f;
					numArray[1] = 1f;
					numArray[3] = 0.0f;
					numArray[4] = 0.8f;
					numArray[5] = 0.5f;
					break;
				case AIStance.HUNKERING:
					numArray[2] = 1f;
					numArray[1] = 1f;
					numArray[3] = 0.0f;
					numArray[4] = 0.8f;
					numArray[5] = 0.5f;
					break;
				case AIStance.CONQUERING:
					numArray[2] = 1f;
					numArray[1] = 1f;
					numArray[3] = 0.0f;
					numArray[4] = 1.5f;
					numArray[5] = 0.5f;
					break;
				case AIStance.DESTROYING:
					numArray[2] = 1f;
					numArray[1] = 1f;
					numArray[3] = 0.0f;
					numArray[4] = 1.5f;
					numArray[5] = 0.5f;
					break;
				case AIStance.DEFENDING:
					numArray[2] = 1f;
					numArray[1] = 1f;
					numArray[3] = 0.0f;
					numArray[4] = 0.8f;
					numArray[5] = 1f;
					break;
			}
			switch (role)
			{
				case ShipRole.COMBAT:
				case ShipRole.POLICE:
				case ShipRole.PLATFORM:
				case ShipRole.BR_PATROL:
				case ShipRole.BR_SPINAL:
				case ShipRole.BR_INTERCEPTOR:
				case ShipRole.BR_TORPEDO:
				case ShipRole.BATTLECRUISER:
				case ShipRole.BATTLESHIP:
					numArray[2] *= 2f;
					break;
				case ShipRole.CARRIER:
				case ShipRole.CARRIER_ASSAULT:
				case ShipRole.CARRIER_DRONE:
				case ShipRole.CARRIER_BIO:
				case ShipRole.CARRIER_BOARDING:
					numArray[2] *= 0.0f;
					numArray[4] *= 1.5f;
					numArray[3] *= 2f;
					break;
				case ShipRole.COMMAND:
					numArray[2] *= 0.0f;
					numArray[4] *= 0.0f;
					numArray[5] *= 2f;
					break;
				case ShipRole.COLONIZER:
				case ShipRole.BR_SCOUT:
					numArray[2] *= 0.0f;
					numArray[3] *= 1.5f;
					numArray[4] *= 2f;
					break;
				case ShipRole.CONSTRUCTOR:
					numArray[2] *= 0.0f;
					numArray[4] *= 0.0f;
					numArray[3] *= 2f;
					break;
				case ShipRole.SCOUT:
				case ShipRole.BR_ESCORT:
					numArray[4] *= 0.0f;
					numArray[2] *= 0.5f;
					numArray[1] *= 1.5f;
					numArray[5] *= 3f;
					break;
				case ShipRole.SUPPLY:
					numArray[2] *= 0.0f;
					numArray[4] *= 0.0f;
					numArray[3] *= 2f;
					break;
			}
			float num1 = 0.0f;
			foreach (float num2 in numArray)
				num1 += num2;
			float num3 = (float)App.GetSafeRandom().NextDouble() * num1;
			for (int index = 1; index < ((IEnumerable<float>)numArray).Count<float>(); ++index)
			{
				if ((double)num3 < (double)numArray[index])
					return (WeaponRole)index;
				num3 -= numArray[index];
			}
			return WeaponRole.BRAWLER;
		}

		public static DesignInfo GetDesignByRole(
		  GameSession game,
		  Player player,
		  AITechStyles techStyles,
		  AIStance? stance,
		  ShipRole shipRole,
		  WeaponRole? weaponRole = null)
		{
			return DesignLab.SetDefaultDesign(game, shipRole, weaponRole, player.ID, null, new bool?(), techStyles, stance);
		}

		public static DesignInfo GetBestDesignByRole(
		  GameSession game,
		  Player player,
		  AIStance? stance,
		  ShipRole desiredRole,
		  List<ShipRole> desiredRoles,
		  WeaponRole? weaponRole = null)
		{
			List<DesignInfo> currentDesignsByRole = DesignLab.GetCurrentDesignsByRole(game, player, stance, desiredRoles, weaponRole);
			int num = 0;
			DesignInfo designInfo = (DesignInfo)null;
			foreach (DesignInfo design in currentDesignsByRole)
			{
				int designScore = DesignLab.GetDesignScore(design);
				if (design.Role == desiredRole)
				{
					if (weaponRole.HasValue)
					{
						WeaponRole weaponRole1 = design.WeaponRole;
						WeaponRole? nullable = weaponRole;
						if ((weaponRole1 != nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) != 0)
						{
							designScore *= 20;
							goto label_7;
						}
					}
					designScore *= 10;
				}
			label_7:
				if (designScore > num)
				{
					num = designScore;
					designInfo = design;
				}
			}
			return designInfo;
		}

		private static int GetDesignScore(DesignInfo design)
		{
			switch (design.Class)
			{
				case ShipClass.Cruiser:
					return 2 * design.DesignDate;
				case ShipClass.Dreadnought:
					return 3 * design.DesignDate;
				case ShipClass.Leviathan:
					return 4 * design.DesignDate;
				case ShipClass.BattleRider:
					return design.DesignDate;
				default:
					return 0;
			}
		}

		public static List<DesignInfo> GetCurrentDesignsByRole(
		  GameSession game,
		  Player player,
		  AIStance? stance,
		  List<ShipRole> desiredRoles,
		  WeaponRole? weaponRole = null)
		{
			List<DesignInfo> list = game.GameDatabase.GetDesignInfosForPlayer(player.ID).ToList<DesignInfo>();
			List<DesignInfo> designInfoList = new List<DesignInfo>();
			foreach (ShipClass shipClass1 in DesignLab._designShipClassFallback)
			{
				ShipClass shipClass = shipClass1;
				if (shipClass != ShipClass.Station)
				{
					List<WeaponRole> selectedWeaponRoles = new List<WeaponRole>();
					if (weaponRole.HasValue)
						selectedWeaponRoles.Add(weaponRole.Value);
					else if (stance.HasValue)
					{
						foreach (ShipRole desiredRole in desiredRoles)
						{
							foreach (WeaponRole weaponRole1 in DesignLab.GetWeaponRolesForNewDesign(stance.Value, desiredRole, shipClass))
							{
								if (!selectedWeaponRoles.Contains(weaponRole1))
									selectedWeaponRoles.Add(weaponRole1);
							}
						}
					}
					else if (desiredRoles.Any<ShipRole>((Func<ShipRole, bool>)(x => DesignLab.DefaultShipWeaponRoles.ContainsKey(x))))
					{
						foreach (ShipRole desiredRole in desiredRoles)
						{
							if (DesignLab.DefaultShipWeaponRoles.ContainsKey(desiredRole))
								selectedWeaponRoles.Add(DesignLab.DefaultShipWeaponRoles[desiredRole]);
						}
					}
					else
						selectedWeaponRoles.Add(WeaponRole.STAND_OFF);
					foreach (DesignInfo designInfo in list.Where<DesignInfo>((Func<DesignInfo, bool>)(x =>
				   {
					   if (x.Class == shipClass && desiredRoles.Contains(x.Role))
						   return selectedWeaponRoles.Contains(x.WeaponRole);
					   return false;
				   })).ToList<DesignInfo>())
					{
						if (!designInfoList.Contains(designInfo))
							designInfoList.Add(designInfo);
					}
				}
			}
			return designInfoList;
		}

		public static ShipClass SuggestShipClassForNewDesign(
		  GameDatabase db,
		  Player player,
		  ShipRole role)
		{
			ShipClass shipClass = ShipClass.Cruiser;
			if (db.PlayerHasLeviathans(player.ID))
				shipClass = ShipClass.Leviathan;
			else if (db.PlayerHasDreadnoughts(player.ID))
				shipClass = ShipClass.Dreadnought;
			switch (role)
			{
				case ShipRole.COMBAT:
					if (shipClass == ShipClass.Leviathan && App.GetSafeRandom().Next(10) < 3)
						return ShipClass.Leviathan;
					return (shipClass == ShipClass.Leviathan || shipClass == ShipClass.Dreadnought) && App.GetSafeRandom().Next(10) < 4 ? ShipClass.Dreadnought : ShipClass.Cruiser;
				case ShipRole.CARRIER:
				case ShipRole.CARRIER_ASSAULT:
				case ShipRole.CARRIER_DRONE:
				case ShipRole.CARRIER_BIO:
				case ShipRole.CARRIER_BOARDING:
					return shipClass;
				case ShipRole.COMMAND:
					if (shipClass == ShipClass.Leviathan)
						return ShipClass.Dreadnought;
					return shipClass;
				case ShipRole.COLONIZER:
				case ShipRole.POLICE:
					return ShipClass.Cruiser;
				case ShipRole.CONSTRUCTOR:
					return ShipClass.Cruiser;
				case ShipRole.SCOUT:
					return ShipClass.Cruiser;
				case ShipRole.SUPPLY:
					if (shipClass == ShipClass.Leviathan)
						return ShipClass.Dreadnought;
					return shipClass;
				case ShipRole.DRONE:
				case ShipRole.ASSAULTSHUTTLE:
				case ShipRole.SLAVEDISK:
				case ShipRole.BR_PATROL:
				case ShipRole.BR_SCOUT:
				case ShipRole.BR_SPINAL:
				case ShipRole.BR_ESCORT:
				case ShipRole.BR_INTERCEPTOR:
				case ShipRole.BR_TORPEDO:
					return ShipClass.BattleRider;
				case ShipRole.PLATFORM:
					return ShipClass.Station;
				case ShipRole.BATTLECRUISER:
					return ShipClass.Cruiser;
				case ShipRole.BATTLESHIP:
					return ShipClass.Dreadnought;
				default:
					return ShipClass.Cruiser;
			}
		}

		public static RealShipClasses GetRealShipClassFromShipClassAndRole(
		  ShipClass desiredClass,
		  ShipRole role)
		{
			switch (role)
			{
				case ShipRole.PLATFORM:
					return RealShipClasses.Platform;
				case ShipRole.BR_PATROL:
				case ShipRole.BR_SCOUT:
				case ShipRole.BR_SPINAL:
				case ShipRole.BR_ESCORT:
				case ShipRole.BR_INTERCEPTOR:
				case ShipRole.BR_TORPEDO:
					return RealShipClasses.BattleRider;
				case ShipRole.BATTLECRUISER:
					return RealShipClasses.BattleCruiser;
				case ShipRole.BATTLESHIP:
					return RealShipClasses.BattleShip;
				default:
					switch (desiredClass)
					{
						case ShipClass.Cruiser:
							return RealShipClasses.Cruiser;
						case ShipClass.Dreadnought:
							return RealShipClasses.Dreadnought;
						case ShipClass.Leviathan:
							return RealShipClasses.Leviathan;
						default:
							return RealShipClasses.Cruiser;
					}
			}
		}

		public static string DeduceFleetTemplate(GameDatabase db, GameSession game, int FleetID)
		{
			FleetInfo fleetInfo = db.GetFleetInfo(FleetID);
			if (Kerberos.Sots.StarFleet.StarFleet.IsSuulkaFleet(db, fleetInfo) || Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(game, fleetInfo))
				return "DEFAULT_COMBAT";
			IEnumerable<ShipInfo> shipInfoByFleetId = db.GetShipInfoByFleetID(FleetID, true);
			string str = null;
			float num1 = -1f;
			List<FleetTemplate> source = new List<FleetTemplate>();
			foreach (FleetTemplate fleetTemplate in db.AssetDatabase.FleetTemplates)
			{
				if (fleetTemplate.MissionTypes.Any<MissionType>((Func<MissionType, bool>)(x =>
			   {
				   if (x != MissionType.COLONIZATION)
					   return x == MissionType.SUPPORT;
				   return true;
			   })) && Kerberos.Sots.StarFleet.StarFleet.GetColonizationSpace(game, FleetID) > 0.0)
					source.Add(fleetTemplate);
				if (fleetTemplate.MissionTypes.Any<MissionType>((Func<MissionType, bool>)(x =>
			   {
				   if (x != MissionType.CONSTRUCT_STN)
					   return x == MissionType.UPGRADE_STN;
				   return true;
			   })) && (double)Kerberos.Sots.StarFleet.StarFleet.GetConstructionPointsForFleet(game, FleetID) > 0.0)
					source.Add(fleetTemplate);
				if (fleetTemplate.MissionTypes.Any<MissionType>((Func<MissionType, bool>)(x => x == MissionType.GATE)) && db.GetShipInfoByFleetID(FleetID, true).Any<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignInfo.Role == ShipRole.GATE)))
					source.Add(fleetTemplate);
			}
			if (source.Count == 1)
				str = source.First<FleetTemplate>().Name;
			if (str == null)
			{
				if (source.Count == 0)
					source = db.AssetDatabase.FleetTemplates;
				foreach (FleetTemplate fleetTemplate in source)
				{
					float num2 = 0.0f;
					float num3 = 0.0f;
					foreach (ShipInfo shipInfo in shipInfoByFleetId)
					{
						ShipInfo ship = shipInfo;
						if (fleetTemplate.ShipIncludes.FirstOrDefault<ShipInclude>((Func<ShipInclude, bool>)(x =>
					   {
						   if (x.ShipRole != ship.DesignInfo.Role)
							   return false;
						   WeaponRole? weaponRole1 = x.WeaponRole;
						   WeaponRole weaponRole2 = ship.DesignInfo.WeaponRole;
						   if (weaponRole1.GetValueOrDefault() == weaponRole2)
							   return weaponRole1.HasValue;
						   return false;
					   })) != null)
							++num3;
						else if (fleetTemplate.ShipIncludes.FirstOrDefault<ShipInclude>((Func<ShipInclude, bool>)(x => x.ShipRole == ship.DesignInfo.Role)) != null)
							++num2;
					}
					float num4 = num3 + num2 / 2f;
					if ((double)num4 > (double)num1)
					{
						num1 = num4;
						str = fleetTemplate.Name;
					}
				}
			}
			return str;
		}

		public static int GetTemplateFillAmount(
		  GameDatabase db,
		  FleetTemplate template,
		  DesignInfo commandDesign,
		  DesignInfo fillDesign)
		{
			int commandPointQuota = db.GetDesignCommandPointQuota(db.AssetDatabase, commandDesign.ID);
			int num1 = 0;
			int num2 = 0;
			foreach (ShipInclude shipInclude in template.ShipIncludes)
			{
				if (shipInclude.InclusionType == ShipInclusionType.REQUIRED && (string.IsNullOrEmpty(shipInclude.Faction) && string.IsNullOrEmpty(shipInclude.FactionExclusion) || shipInclude.Faction == db.GetPlayerFaction(fillDesign.PlayerID).Name || !string.IsNullOrEmpty(shipInclude.FactionExclusion) && shipInclude.FactionExclusion != db.GetPlayerFaction(fillDesign.PlayerID).Name))
					num1 += fillDesign.CommandPointCost * shipInclude.Amount;
				else if (string.IsNullOrEmpty(shipInclude.Faction) && string.IsNullOrEmpty(shipInclude.FactionExclusion) || shipInclude.Faction == db.GetPlayerFaction(fillDesign.PlayerID).Name || !string.IsNullOrEmpty(shipInclude.FactionExclusion) && shipInclude.FactionExclusion != db.GetPlayerFaction(fillDesign.PlayerID).Name)
					++num2;
			}
			int num3 = commandPointQuota - num1;
			if (fillDesign.CommandPointCost == 0)
			{
				DesignLab.Warn("GetTemplateFillAmount: fillDesign.CommandPointCost is zero");
				return 0;
			}
			if (num2 != 0)
				return num3 / fillDesign.CommandPointCost / num2;
			DesignLab.Warn("GetTemplateFillAmount: fillAmount is zero");
			return 0;
		}

		public static List<int> GetMissionRequiredDesigns(
		  GameSession game,
		  MissionType missionType,
		  int player)
		{
			List<int> intList = new List<int>();
			IEnumerable<DesignInfo> designInfosForPlayer = game.GameDatabase.GetDesignInfosForPlayer(player);
			DesignInfo designInfo1 = (DesignInfo)null;
			switch (missionType)
			{
				case MissionType.COLONIZATION:
				case MissionType.SUPPORT:
					foreach (DesignInfo designInfo2 in designInfosForPlayer)
					{
						if (Kerberos.Sots.StarFleet.StarFleet.CanDesignColonize(game, designInfo2.ID))
						{
							if (designInfo1 == null)
								designInfo1 = designInfo2;
							else if (designInfo1.DesignDate < designInfo2.DesignDate)
								designInfo1 = designInfo2;
						}
					}
					intList.Add(designInfo1.ID);
					break;
				case MissionType.CONSTRUCT_STN:
					foreach (DesignInfo designInfo2 in designInfosForPlayer)
					{
						if (Kerberos.Sots.StarFleet.StarFleet.CanDesignConstruct(game, designInfo2.ID))
						{
							if (designInfo1 == null)
								designInfo1 = designInfo2;
							else if (designInfo1.DesignDate < designInfo2.DesignDate)
								designInfo1 = designInfo2;
						}
					}
					intList.Add(designInfo1.ID);
					break;
			}
			return intList;
		}

		public static DesignInfo CreateStationDesignInfo(
		  AssetDatabase assetdb,
		  GameDatabase gamedb,
		  int playerId,
		  StationType type,
		  int level,
		  bool insertDesign)
		{
			DesignInfo design = DesignLab.GetStationDesignInfo(assetdb, gamedb, playerId, type, level);
			if (insertDesign)
			{
				design.ID = gamedb.InsertDesignByDesignInfo(design);
				design = gamedb.GetDesignInfo(design.ID);
			}
			else
				DesignLab.SummarizeDesign(assetdb, gamedb, design);
			return design;
		}

		public static Rectangle GetShipSize(DesignInfo des)
		{
			if (des.Role == ShipRole.CONSTRUCTOR)
				return DesignLab.DEFAULT_DREADNAUGHT_SIZE;
			switch (des.Class)
			{
				case ShipClass.Cruiser:
					return DesignLab.DEFAULT_CRUISER_SIZE;
				case ShipClass.Dreadnought:
					return DesignLab.DEFAULT_DREADNAUGHT_SIZE;
				case ShipClass.Leviathan:
					return DesignLab.DEFAULT_LEVIATHAN_SIZE;
				default:
					return DesignLab.DEFAULT_CRUISER_SIZE;
			}
		}

		private static void PrintDesignTable(
		  StringBuilder result,
		  string title,
		  IList<DesignInfo> designs)
		{
			result.AppendLine();
			result.AppendLine(string.Format("{0} ({1}):", (object)title, (object)designs.Count));
			result.AppendLine();
			result.AppendLine("       ID | Name                           | Class       | DesignDate | isPrototyped | Role            | WeaponRole      | NumBuilt ");
			result.AppendLine("----------+--------------------------------+-------------+------------+--------------+-----------------+-----------------+----------");
			foreach (DesignInfo design in (IEnumerable<DesignInfo>)designs)
				result.AppendLine(string.Format(" {0,8} | {1,-30} | {2,-11} | {3,10} | {4,-12} | {5,-15} | {6,-15} | {7,8} ", (object)design.ID, (object)design.Name, (object)design.Class, (object)design.DesignDate, (object)design.isPrototyped, (object)design.Role, (object)design.WeaponRole, (object)design.NumBuilt));
		}

		public static void PrintPlayerDesignSummary(
		  StringBuilder result,
		  App app,
		  int playerid,
		  bool includeStationDesigns = false)
		{
			List<DesignInfo> list1 = app.GameDatabase.GetDesignInfosForPlayer(playerid).Where<DesignInfo>((Func<DesignInfo, bool>)(x =>
		   {
			   if (!includeStationDesigns)
				   return x.Class != ShipClass.Station;
			   return true;
		   })).ToList<DesignInfo>();
			List<DesignInfo> activeDesigns = app.GameDatabase.GetVisibleDesignInfosForPlayer(playerid).Where<DesignInfo>((Func<DesignInfo, bool>)(x =>
		   {
			   if (!includeStationDesigns)
				   return x.Class != ShipClass.Station;
			   return true;
		   })).ToList<DesignInfo>();
			List<DesignInfo> list2 = activeDesigns.Where<DesignInfo>((Func<DesignInfo, bool>)(x => StrategicAI.IsDesignObsolete(app.Game, playerid, x.ID))).ToList<DesignInfo>();
			List<DesignInfo> list3 = list1.Where<DesignInfo>((Func<DesignInfo, bool>)(x => !activeDesigns.Any<DesignInfo>((Func<DesignInfo, bool>)(y => y.ID == x.ID)))).ToList<DesignInfo>();
			result.Append("Design reports for player " + (object)playerid);
			if (!includeStationDesigns)
				result.Append(", excluding station designs");
			result.AppendLine(":");
			DesignLab.PrintDesignTable(result, "Active designs", (IList<DesignInfo>)activeDesigns);
			DesignLab.PrintDesignTable(result, "Obsolete designs (AI)", (IList<DesignInfo>)list2);
			DesignLab.PrintDesignTable(result, "Retired designs", (IList<DesignInfo>)list3);
		}

		private enum DefenseStrat
		{
			Energy,
			Ballistics,
			HeavyBeam,
			Point,
		}

		public class ModuleSlotInfo
		{
			public LogicalModuleMount mountInfo;
			public DesignModuleInfo currentModule;
		}

		private struct WeaponScore
		{
			public LogicalWeapon Weapon;
			public float Score;
		}

		internal enum NameGenerators
		{
			FactionRandom,
			MissionSectionDerived,
		}
	}
}
