// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.ShipFramework.ShipSectionAsset
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kerberos.Sots.ShipFramework
{
	internal class ShipSectionAsset
	{
		public string AmbientSound = "";
		public string EngineSound = "";
		public string UnderAttackSound = "";
		public string DestroyedSound = "";
		public List<string[]> ShipOptions = new List<string[]>();
		public int Structure = 1000;
		public float Mass = 10000f;
		public int SavingsCost = 30000;
		public int ProductionCost = 2000;
		public float FleetSpeedModifier = 1f;
		public string ManeuveringType = "";
		public ShipManeuveringInfo Maneuvering = new ShipManeuveringInfo();
		public readonly Kerberos.Sots.Framework.Size[] Armor = new Kerberos.Sots.Framework.Size[4]
		{
	  new Kerberos.Sots.Framework.Size() { X = 10, Y = 10 },
	  new Kerberos.Sots.Framework.Size() { X = 10, Y = 10 },
	  new Kerberos.Sots.Framework.Size() { X = 10, Y = 10 },
	  new Kerberos.Sots.Framework.Size() { X = 10, Y = 10 }
		};
		public string Title;
		public string Description;
		public string FileName;
		public string SectionName;
		public string Faction;
		public ShipSectionType Type;
		public ShipClass Class;
		public RealShipClasses RealClass;
		public BattleRiderTypes BattleRiderType;
		public string ModelName;
		public string DestroyedModelName;
		public string DamagedModelName;
		public string[] RequiredTechs;
		public LogicalBank[] Banks;
		public LogicalMount[] Mounts;
		public LogicalModuleMount[] Modules;
		public ShipSectionType[] ExcludeSectionTypes;
		public SectionEnumerations.PsionicAbility[] PsionicAbilities;
		public LogicalEffect DamageEffect;
		public LogicalEffect DeathEffect;
		public LogicalEffect ReactorFailureDeathEffect;
		public LogicalEffect ReactorCriticalDeathEffect;
		public LogicalEffect AbsorbedDeathEffect;
		public string[] ExcludeSections;
		public CarrierType CarrierType;
		public bool IsCarrier;
		public bool IsBattleRider;
		public bool isPolice;
		public bool isPropaganda;
		public bool IsSuperTransport;
		public bool IsBoreShip;
		public bool IsSupplyShip;
		public bool IsGateShip;
		public bool IsFreighter;
		public bool IsScavenger;
		public bool IsWraithAbductor;
		public bool IsAccelerator;
		public bool IsLoaCube;
		public bool IsTrapShip;
		public bool IsGravBoat;
		public bool IsAbsorberSection;
		public bool IsFireControl;
		public bool IsAIControl;
		public bool IsListener;
		public int LowStruct;
		public SuulkaType SuulkaType;
		public int ColonizationSpace;
		public int TerraformingSpace;
		public int ConstructionPoints;
		public int FreighterSpace;
		public int ReserveSize;
		public int RepairPoints;
		public float FtlSpeed;
		public float NodeSpeed;
		public float MissionTime;
		public float LaunchDelay;
		public float DockingDelay;
		public int Crew;
		public int CrewRequired;
		public int Power;
		public int Supply;
		public int SlaveCapacity;
		public float ShipExplosiveDamage;
		public float ShipExplosiveRange;
		public float PsionicPowerLevel;
		public float ECCM;
		public float ECM;
		public int CommandPoints;
		public float Signature;
		public float TacticalSensorRange;
		public float StrategicSensorRange;
		public StationType StationType;
		public SectionEnumerations.CombatAiType CombatAIType;
		public ShipFleetAbilityType ShipFleetAbilityType;
		public int StationLevel;
		public bool isConstructor;
		public bool isDeepScan;
		public bool isMineLayer;
		public CloakingType cloakingType;
		public bool hasJammer;

		private static PlatformTypes? GetPlatformType(string sectionname)
		{
			if (sectionname == "sn_dronesat" || sectionname == "sn_drone" || sectionname == "sn_drone_satellite")
				return new PlatformTypes?(PlatformTypes.dronesat);
			if (sectionname == "sn_brsat" || sectionname == "sn_br_satellite")
				return new PlatformTypes?(PlatformTypes.brsat);
			if (sectionname == "sn_torpsat" || sectionname == "sn_torpedo_satellite" || sectionname == "sn_torp_satellite")
				return new PlatformTypes?(PlatformTypes.torpsat);
			if (sectionname == "sn_scansat" || sectionname == "sn_scan_satellite")
				return new PlatformTypes?(PlatformTypes.scansat);
			if (sectionname == "sn_asteroid_monitor")
				return new PlatformTypes?(PlatformTypes.monitorsat);
			if (sectionname == "sn_missile_satellite")
				return new PlatformTypes?(PlatformTypes.missilesat);
			return new PlatformTypes?();
		}

		public PlatformTypes? GetPlatformType()
		{
			return ShipSectionAsset.GetPlatformType(this.SectionName);
		}

		public override string ToString()
		{
			return this.Faction + " " + this.Class.ToString() + " " + this.ModelName + " (" + this.Type.ToString() + ")";
		}

		private static string GetShipDefaultDeathEffect(ShipClass sc, BattleRiderTypes brt = BattleRiderTypes.Unspecified)
		{
			switch (sc)
			{
				case ShipClass.Cruiser:
					return "effects\\ShipDeath\\cr_death.effect";
				case ShipClass.Dreadnought:
				case ShipClass.Leviathan:
					return "effects\\ShipDeath\\dn_death.effect";
				case ShipClass.BattleRider:
					if (brt == BattleRiderTypes.biomissile)
						return "effects\\Weapons\\biomissile_impact.effect";
					break;
			}
			return "effects\\ShipDeath\\placeholder.effect";
		}

		private static string GetReactorShieldFailureDeathEffect(ShipClass sc, ShipSectionType sst)
		{
			if (sst == ShipSectionType.Engine)
			{
				switch (sc)
				{
					case ShipClass.Cruiser:
						return "effects\\ShipDeath\\Cruiser_SRF.effect";
					case ShipClass.Dreadnought:
						return "effects\\ShipDeath\\Dreadnought_SRF.effect";
					case ShipClass.Leviathan:
						return "effects\\ShipDeath\\Levi_SRF.effect";
				}
			}
			return ShipSectionAsset.GetShipDefaultDeathEffect(sc, BattleRiderTypes.Unspecified);
		}

		private static string GetReactorCriticalDeathEffect(
		  ShipClass sc,
		  ShipSectionType sst,
		  string fileName)
		{
			if (sst == ShipSectionType.Engine || sc == ShipClass.Leviathan)
			{
				switch (sc)
				{
					case ShipClass.Cruiser:
						if (fileName.Contains("Reflex") || fileName.Contains("reflex") || (fileName.Contains("Antimatter") || fileName.Contains("antimatter")))
							return "effects\\ShipDeath\\Cruiser_CRF_Antimatter.effect";
						if (fileName.Contains("Fusion") || fileName.Contains("fusion"))
							return "effects\\ShipDeath\\Cruiser_CRF_Fusion.effect";
						break;
					case ShipClass.Dreadnought:
						if (fileName.Contains("Reflex") || fileName.Contains("reflex") || (fileName.Contains("Antimatter") || fileName.Contains("antimatter")))
							return "effects\\ShipDeath\\Dreadnought_CRF_Antimatter.effect";
						if (fileName.Contains("Fusion") || fileName.Contains("fusion"))
							return "effects\\ShipDeath\\Dreadnought_CRF_Fusion.effect";
						break;
					case ShipClass.Leviathan:
						return "effects\\ShipDeath\\Levi_CRF_Antimatter.effect";
				}
			}
			return ShipSectionAsset.GetShipDefaultDeathEffect(sc, BattleRiderTypes.Unspecified);
		}

		private static string GetAbsorbedDeathEffect(ShipClass sc, ShipSectionType sst)
		{
			if (sst == ShipSectionType.Mission)
			{
				switch (sc)
				{
					case ShipClass.Cruiser:
						return "effects\\ShipDeath\\Cruiser_SRF.effect";
					case ShipClass.Dreadnought:
						return "effects\\ShipDeath\\Dreadnought_SRF.effect";
					case ShipClass.Leviathan:
						return "effects\\ShipDeath\\Levi_SRF.effect";
				}
			}
			return "";
		}

		public bool IsSuulka
		{
			get
			{
				return this.SuulkaType != SuulkaType.None;
			}
		}

		public int GetExtraArmorLayers()
		{
			switch (this.Class)
			{
				case ShipClass.Cruiser:
				case ShipClass.Station:
					return 1;
				case ShipClass.Dreadnought:
					return 2;
				case ShipClass.Leviathan:
					return 3;
				default:
					return 0;
			}
		}

		public DamagePattern CreateFreshArmor(ArmorSide side, int ArmorWidthModifier)
		{
			return new DamagePattern(this.Armor[(int)side].X, Math.Max(0, this.Armor[(int)side].Y + ArmorWidthModifier));
		}

		public bool SectionIsExcluded(ShipSectionAsset section)
		{
			string sectionName = Path.GetFileNameWithoutExtension(section.FileName);
			if (!((IEnumerable<ShipSectionType>)this.ExcludeSectionTypes).Any<ShipSectionType>((Func<ShipSectionType, bool>)(x => x == section.Type)))
				return ((IEnumerable<string>)this.ExcludeSections).Any<string>((Func<string, bool>)(x => x == sectionName));
			return true;
		}

		public bool CarrierTypeMatchesRole(ShipRole role)
		{
			switch (role)
			{
				case ShipRole.CARRIER:
					if (this.CarrierType != CarrierType.Destroyer && this.CarrierType != CarrierType.BattleCruiser)
						return this.CarrierType == CarrierType.BattleShip;
					return true;
				case ShipRole.CARRIER_ASSAULT:
					return this.CarrierType == CarrierType.AssaultShuttle;
				case ShipRole.CARRIER_DRONE:
					return this.CarrierType == CarrierType.Drone;
				case ShipRole.CARRIER_BIO:
					return this.CarrierType == CarrierType.BioMissile;
				case ShipRole.CARRIER_BOARDING:
					return this.CarrierType == CarrierType.BoardingPod;
				default:
					return false;
			}
		}

		public static bool IsBattleRiderClass(RealShipClasses shipClass)
		{
			switch (shipClass)
			{
				case RealShipClasses.BattleRider:
				case RealShipClasses.BattleCruiser:
				case RealShipClasses.BattleShip:
				case RealShipClasses.Drone:
				case RealShipClasses.BoardingPod:
				case RealShipClasses.EscapePod:
				case RealShipClasses.AssaultShuttle:
				case RealShipClasses.Biomissile:
					return true;
				default:
					return false;
			}
		}

		public static bool IsWeaponBattleRiderClass(RealShipClasses shipClass)
		{
			switch (shipClass)
			{
				case RealShipClasses.Drone:
				case RealShipClasses.BoardingPod:
				case RealShipClasses.EscapePod:
				case RealShipClasses.AssaultShuttle:
				case RealShipClasses.Biomissile:
					return true;
				default:
					return false;
			}
		}

		public static CarrierType GetCarrierType(List<LogicalBank> banks)
		{
			int[] numArray = new int[8];
			for (int index = 0; index < 8; ++index)
				numArray[index] = 0;
			foreach (LogicalBank bank in banks)
			{
				switch (bank.TurretClass)
				{
					case WeaponEnums.TurretClasses.Biomissile:
						++numArray[3];
						continue;
					case WeaponEnums.TurretClasses.Drone:
						++numArray[1];
						continue;
					case WeaponEnums.TurretClasses.AssaultShuttle:
						++numArray[2];
						continue;
					case WeaponEnums.TurretClasses.DestroyerRider:
						++numArray[5];
						continue;
					case WeaponEnums.TurretClasses.CruiserRider:
						++numArray[6];
						continue;
					case WeaponEnums.TurretClasses.DreadnoughtRider:
						++numArray[7];
						continue;
					case WeaponEnums.TurretClasses.BoardingPod:
						++numArray[4];
						continue;
					default:
						continue;
				}
			}
			CarrierType carrierType = CarrierType.None;
			int num = 0;
			for (int index = 0; index < 8; ++index)
			{
				if (numArray[index] > num)
				{
					num = numArray[index];
					carrierType = (CarrierType)index;
				}
			}
			return carrierType;
		}

		public void LoadFromXml(
		  AssetDatabase assetdb,
		  string filename,
		  string faction,
		  ShipSectionType sectionType,
		  ShipClass sectionClass)
		{
			ShipSection ss = new ShipSection();
			ShipXmlUtility.LoadShipSectionFromXml(filename, ref ss);
			this.Type = sectionType;
			this.Class = sectionClass;
			this.Faction = faction;
			this.SectionName = Path.GetFileNameWithoutExtension(filename);
			string str1 = "";
			switch (this.Class)
			{
				case ShipClass.Cruiser:
					str1 = "CR";
					break;
				case ShipClass.Dreadnought:
					str1 = "DN";
					break;
				case ShipClass.Leviathan:
					str1 = "LV";
					break;
				case ShipClass.BattleRider:
					str1 = "BR";
					break;
				case ShipClass.Station:
					str1 = "SN";
					break;
			}
			this.ModelName = FileSystemHelpers.StripMountName(ss.ModelPath);
			string str2 = Path.Combine(Path.GetDirectoryName(this.ModelName), "Damage_" + str1 + "_" + this.Type.ToString() + "_Default.scene");
			this.DestroyedModelName = string.IsNullOrEmpty(ss.DestroyedModelPath) ? str2 : ss.DestroyedModelPath;
			this.DamagedModelName = ss.DamageModelPath;
			this.AmbientSound = ss.AmbientSound;
			this.EngineSound = ss.EngineSound;
			string str3 = string.Format("COMBAT_023-01_{0}_GeneralShipsBeingAttacked", (object)faction);
			string str4 = "";
			switch (this.Class)
			{
				case ShipClass.Cruiser:
					str4 = string.Format("COMBAT_029-01_{0}_CruiserDestroyed", (object)faction);
					break;
				case ShipClass.Dreadnought:
					str4 = string.Format("COMBAT_030-01_{0}_DreadnoughtDestroyed", (object)faction);
					break;
				case ShipClass.Leviathan:
					str4 = string.Format("COMBAT_031-01_{0}_LeviathanDestroyed", (object)faction);
					break;
				case ShipClass.BattleRider:
					str4 = string.Format("COMBAT_020-01_{0}_BattleRiderDestroyed", (object)faction);
					break;
				case ShipClass.Station:
					switch (this.StationType)
					{
						case StationType.NAVAL:
							str3 = string.Format("COMBAT_067-01_{0}_NavalStationUnderAttack", (object)faction);
							str4 = string.Format("COMBAT_066-01_{0}_NavalStationDestroyed", (object)faction);
							break;
						case StationType.SCIENCE:
							str3 = string.Format("COMBAT_069-01_{0}_ScienceStationUnderAttack", (object)faction);
							str4 = string.Format("COMBAT_068-01_{0}_ScienceStationDestroyed", (object)faction);
							break;
						case StationType.CIVILIAN:
							str3 = string.Format("COMBAT_071-01_{0}_CivilianStationUnderAttack", (object)faction);
							str4 = string.Format("COMBAT_072-01_{0}_CivilianStationDestroyed", (object)faction);
							break;
						case StationType.DIPLOMATIC:
							str3 = string.Format("COMBAT_070a-01_{0}_DiplomaticStationUnderAttack", (object)faction);
							str4 = string.Format("COMBAT_070-01_{0}_DiplomaticStationDestroyed", (object)faction);
							break;
					}
					break;
			}
			this.UnderAttackSound = string.IsNullOrEmpty(ss.UnderAttackSound) ? str3 : ss.UnderAttackSound;
			this.DestroyedSound = string.IsNullOrEmpty(ss.DestroyedSound) ? str4 : ss.DestroyedSound;
			this.Title = ss.Title;
			this.Description = ss.Description;
			if (string.IsNullOrWhiteSpace(this.Title))
				this.Title = Path.GetFileNameWithoutExtension(filename);
			string withoutExtension = Path.GetFileNameWithoutExtension(filename);
			this.CombatAIType = !string.IsNullOrEmpty(ss.CombatAiType) ? (SectionEnumerations.CombatAiType)Enum.Parse(typeof(SectionEnumerations.CombatAiType), ss.CombatAiType) : SectionEnumerations.CombatAiType.Normal;
			switch (this.CombatAIType)
			{
				case SectionEnumerations.CombatAiType.Drone:
					this.SetBattleRiderType(BattleRiderTypes.drone);
					break;
				case SectionEnumerations.CombatAiType.AssaultShuttle:
					this.SetBattleRiderType(BattleRiderTypes.assaultshuttle);
					break;
				case SectionEnumerations.CombatAiType.NodeFighter:
					this.SetBattleRiderType(BattleRiderTypes.nodefighter);
					break;
				case SectionEnumerations.CombatAiType.Swarmer:
				case SectionEnumerations.CombatAiType.SwarmerGuardian:
				case SectionEnumerations.CombatAiType.VonNeumannPyramid:
				case SectionEnumerations.CombatAiType.LocustFighter:
				case SectionEnumerations.CombatAiType.MorrigiCrow:
					this.SetBattleRiderType(BattleRiderTypes.battlerider);
					break;
				case SectionEnumerations.CombatAiType.VonNeumannSeekerProbe:
					this.SetBattleRiderType(BattleRiderTypes.assaultshuttle);
					break;
				default:
					this.SetBattleRiderType(ObtainShipClassTypes.GetBattleRiderTypeByName(this.Class, withoutExtension));
					break;
			}
			if (withoutExtension.Contains("protectorate"))
				this.ShipFleetAbilityType = ShipFleetAbilityType.Protectorate;
			else if (withoutExtension.Contains("suulka_the_hidden"))
				this.ShipFleetAbilityType = ShipFleetAbilityType.Hidden;
			else if (withoutExtension.Contains("suulka_the_deaf"))
				this.ShipFleetAbilityType = ShipFleetAbilityType.Deaf;
			this.IsSuperTransport = this.SectionName.StartsWith("lv_supertransport", StringComparison.InvariantCultureIgnoreCase);
			this.IsBoreShip = this.SectionName.EndsWith("bore", StringComparison.InvariantCultureIgnoreCase);
			this.IsSupplyShip = this.SectionName.Contains("_supply");
			this.IsGateShip = this.SectionName.StartsWith("cr_mis_gate", StringComparison.InvariantCultureIgnoreCase);
			this.IsTrapShip = this.SectionName.StartsWith("cr_mis_colonytrickster", StringComparison.InvariantCultureIgnoreCase);
			this.IsGravBoat = this.SectionName.StartsWith("cr_mis_gravboat", StringComparison.InvariantCultureIgnoreCase);
			this.IsAbsorberSection = this.SectionName.Contains("_absorber") || this.SectionName.Contains("_absorbtion");
			this.IsListener = this.SectionName.Contains("_listener");
			this.IsFireControl = this.Title.Contains("CR_CMD_FIRECONTROL") || this.Title.Contains("CR_CMD_FIRE_CONTROL");
			this.IsAIControl = this.Title.Contains("CR_CMD_AI") || this.Title.Contains("DN_CMD_AI");
			this.SuulkaType = this.GetSuulkaType(this.SectionName);
			this.IsFreighter = ss.isFreighter;
			this.FreighterSpace = ss.FreighterSpace;
			this.isPolice = ss.isPolice;
			this.SlaveCapacity = ss.SlaveCapacity;
			this.isPropaganda = this.SectionName.StartsWith("cr_mis_propaganda", StringComparison.InvariantCultureIgnoreCase);
			this.IsAccelerator = this.SectionName.StartsWith("cr_mis_ngp", StringComparison.InvariantCultureIgnoreCase);
			this.IsLoaCube = this.SectionName.StartsWith("cr_mis_cube", StringComparison.InvariantCultureIgnoreCase);
			this.IsScavenger = this.FileName.Contains("mis_scavenger") || this.FileName.Contains("dn_mis_subjugator");
			this.IsWraithAbductor = this.FileName.Contains("wraith_abductor");
			this.Armor[1] = new Kerberos.Sots.Framework.Size()
			{
				X = ss.TopArmor.X,
				Y = ss.TopArmor.Y
			};
			this.Armor[3] = new Kerberos.Sots.Framework.Size()
			{
				X = ss.BottomArmor.X,
				Y = ss.BottomArmor.Y
			};
			this.Armor[2] = this.Armor[0] = new Kerberos.Sots.Framework.Size()
			{
				X = ss.SideArmor.X,
				Y = ss.SideArmor.Y
			};
			this.Structure = ss.Struct;
			this.LowStruct = ss.StructDamageAmount;
			this.Mass = (float)ss.Mass;
			this.SavingsCost = ss.SavingsCost;
			this.ProductionCost = ss.ProductionCost;
			this.ColonizationSpace = ss.ColonizerSpace;
			this.TerraformingSpace = ss.TerraformingPoints;
			this.ConstructionPoints = ss.ConstructionPoints;
			this.ReserveSize = ss.BattleRiderReserveSize;
			this.RepairPoints = ss.RepairPoints;
			this.FtlSpeed = ss.FtlSpeed;
			this.NodeSpeed = ss.NodeSpeed;
			this.MissionTime = ss.MissionTime;
			this.LaunchDelay = ss.LaunchDelay;
			this.DockingDelay = ss.DockingDelay;
			this.Crew = ss.Crew;
			this.CrewRequired = ss.CrewRequired;
			this.Power = ss.Power;
			this.Supply = ss.Supply;
			this.ECM = ss.ECM;
			this.ECCM = ss.ECCM;
			this.CommandPoints = ss.CommandPoints;
			this.Signature = ss.Signature;
			this.TacticalSensorRange = ss.TacticalSensorRange;
			this.ShipExplosiveDamage = ss.DeathDamage;
			this.ShipExplosiveRange = ss.ExplosionRadius;
			this.PsionicPowerLevel = ss.PsionicPowerLevel;
			if ((double)this.TacticalSensorRange <= 0.0)
				this.TacticalSensorRange = 20000f;
			this.DamageEffect = new LogicalEffect()
			{
				Name = !string.IsNullOrEmpty(ss.DamageEffectPath) ? ss.DamageEffectPath : ShipSectionAsset.GetShipDefaultDeathEffect(this.Class, this.BattleRiderType)
			};
			this.DeathEffect = new LogicalEffect()
			{
				Name = !string.IsNullOrEmpty(ss.DestroyedEffectPath) ? ss.DestroyedEffectPath : ShipSectionAsset.GetShipDefaultDeathEffect(this.Class, this.BattleRiderType)
			};
			this.ReactorFailureDeathEffect = new LogicalEffect()
			{
				Name = ShipSectionAsset.GetReactorShieldFailureDeathEffect(this.Class, this.Type)
			};
			this.ReactorCriticalDeathEffect = new LogicalEffect()
			{
				Name = ShipSectionAsset.GetReactorCriticalDeathEffect(this.Class, this.Type, withoutExtension)
			};
			this.AbsorbedDeathEffect = new LogicalEffect()
			{
				Name = ShipSectionAsset.GetAbsorbedDeathEffect(this.Class, this.Type)
			};
			this.StrategicSensorRange = ss.StrategicSensorRange;
			this.FleetSpeedModifier = ss.FleetSpeedModifier;
			if ((double)this.StrategicSensorRange <= 0.0)
				this.StrategicSensorRange = assetdb.DefaultStratSensorRange;
			this.StationType = ss.StationType != null ? SectionEnumerations.StationTypesWithInvalid[ss.StationType] : StationType.INVALID_TYPE;
			this.StationLevel = ss.StationLevel;
			this.isConstructor = ss.isConstructor;
			this.Maneuvering.LinearAccel = ss.Acceleration;
			this.Maneuvering.RotAccel.X = ss.RotationalAccelerationYaw;
			this.Maneuvering.RotAccel.Y = ss.RotationalAccelerationPitch;
			this.Maneuvering.RotAccel.Z = ss.RotationalAccelerationRoll;
			this.Maneuvering.Deacceleration = ss.Decceleration;
			this.Maneuvering.LinearSpeed = ss.LinearSpeed;
			this.Maneuvering.RotationSpeed = ss.RotationSpeed;
			HashSet<string> source1 = new HashSet<string>();
			foreach (Kerberos.Sots.Data.ShipFramework.Tech tech in ss.Techs)
			{
				source1.Add(tech.Name);
				this.isDeepScan = this.isDeepScan || tech.Name == "CCC_Advanced_Sensors";
				this.hasJammer = this.hasJammer || tech.Name == "CCC_Sensor_Jammer";
				if (this.cloakingType == CloakingType.None)
				{
					switch (tech.Name)
					{
						case "SLD_Cloaking":
							this.cloakingType = CloakingType.Cloaking;
							continue;
						case "SLD_Improved_Cloaking":
							this.cloakingType = CloakingType.ImprovedCloaking;
							continue;
						default:
							continue;
					}
				}
			}
			List<HashSet<string>> stringSetList = new List<HashSet<string>>();
			foreach (ShipOptionGroup shipOptionGroup in ss.ShipOptionGroups)
			{
				HashSet<string> stringSet = new HashSet<string>();
				foreach (ShipOption shipOption in shipOptionGroup.ShipOptions)
					stringSet.Add(shipOption.Name);
				stringSetList.Add(stringSet);
			}
			switch (sectionClass)
			{
				case ShipClass.Cruiser:
					source1.Add("ENG_Cruiser_Construction");
					break;
				case ShipClass.Dreadnought:
					source1.Add("ENG_Dreadnought_Construction");
					break;
				case ShipClass.Leviathan:
					source1.Add("ENG_Leviathian_Construction");
					break;
			}
			List<LogicalModuleMount> logicalModuleMountList = new List<LogicalModuleMount>();
			foreach (ModuleMount module in ss.Modules)
			{
				LogicalModuleMount logicalModuleMount = new LogicalModuleMount()
				{
					Section = this
				};
				logicalModuleMount.AssignedModuleName = module.AssignedModuleName;
				logicalModuleMount.ModuleType = module.Type;
				logicalModuleMount.NodeName = module.NodeName;
				logicalModuleMount.FrameX = module.FrameX;
				logicalModuleMount.FrameY = module.FrameY;
				logicalModuleMountList.Add(logicalModuleMount);
			}
			List<LogicalBank> banks = new List<LogicalBank>();
			List<LogicalMount> logicalMountList = new List<LogicalMount>();
			foreach (Bank bank in ss.Banks)
			{
				LogicalBank logicalBank = new LogicalBank()
				{
					TurretSize = (WeaponEnums.WeaponSizes)Enum.Parse(typeof(WeaponEnums.WeaponSizes), bank.Size),
					Section = this,
					Module = (LogicalModule)null,
					GUID = Guid.Parse(bank.Id),
					DefaultWeaponName = bank.DefaultWeapon
				};
				logicalBank.TurretClass = (WeaponEnums.TurretClasses)Enum.Parse(typeof(WeaponEnums.TurretClasses), bank.Class);
				logicalBank.FrameX = bank.FrameX;
				logicalBank.FrameY = bank.FrameY;
				this.IsCarrier = this.IsCarrier || WeaponEnums.IsBattleRider(logicalBank.TurretClass);
				this.isMineLayer = this.isMineLayer || logicalBank.TurretClass == WeaponEnums.TurretClasses.Minelayer;
				banks.Add(logicalBank);
				foreach (Mount mount in bank.Mounts)
				{
					LogicalMount logicalMount = new LogicalMount()
					{
						Bank = logicalBank,
						NodeName = mount.NodeName,
						TurretOverload = mount.TurretOverload,
						BarrelOverload = mount.BarrelOverload,
						BaseOverload = mount.BaseOverload,
						FireAnimName = mount.SectionFireAnimation != null ? mount.SectionFireAnimation : "",
						ReloadAnimName = mount.SectionReloadAnimation != null ? mount.SectionReloadAnimation : "",
						Yaw = {
			  Min = mount.YawMin,
			  Max = mount.YawMax
			},
						Pitch = {
			  Min = mount.PitchMin,
			  Max = mount.PitchMax
			}
					};
					logicalMount.Pitch.Min = Math.Max(-90f, logicalMount.Pitch.Min);
					logicalMount.Pitch.Max = Math.Min(90f, logicalMount.Pitch.Max);
					logicalMountList.Add(logicalMount);
				}
			}
			if (this.IsCarrier)
				this.CarrierType = ShipSectionAsset.GetCarrierType(banks);
			List<string> stringList = new List<string>();
			List<ShipSectionType> shipSectionTypeList = new List<ShipSectionType>();
			foreach (ExcludedSection excludedSection in ss.ExcludedSections)
				stringList.Add(excludedSection.Name);
			foreach (ExcludedType excludedType in ss.ExcludedTypes)
			{
				ShipSectionType shipSectionType = ShipSectionType.Command;
				if (excludedType.Name == "Engine")
					shipSectionType = ShipSectionType.Engine;
				else if (excludedType.Name == "Mission")
					shipSectionType = ShipSectionType.Mission;
				shipSectionTypeList.Add(shipSectionType);
			}
			List<SectionEnumerations.PsionicAbility> psionicAbilityList = new List<SectionEnumerations.PsionicAbility>();
			foreach (AvailablePsionicAbility psionicAbility in ss.PsionicAbilities)
				psionicAbilityList.Add((SectionEnumerations.PsionicAbility)Enum.Parse(typeof(SectionEnumerations.PsionicAbility), psionicAbility.Name));
			foreach (IEnumerable<string> source2 in stringSetList)
				this.ShipOptions.Add(source2.ToArray<string>());
			this.RequiredTechs = source1.ToArray<string>();
			this.Banks = banks.ToArray();
			this.Mounts = logicalMountList.ToArray();
			this.Modules = logicalModuleMountList.ToArray();
			this.ExcludeSections = stringList.ToArray();
			this.ExcludeSectionTypes = shipSectionTypeList.ToArray();
			this.PsionicAbilities = psionicAbilityList.ToArray();
			if (!ss.RealShipClass.HasValue)
			{
				if (this.RealClass != RealShipClasses.BattleCruiser && this.RealClass != RealShipClasses.BattleShip)
					this.RealClass = ObtainShipClassTypes.GetRealShipClass(this.Class, this.BattleRiderType, filename);
			}
			else
				this.RealClass = ss.RealShipClass.Value;
			if (this.CombatAIType != SectionEnumerations.CombatAiType.VonNeumannDisc)
				return;
			this.cloakingType = CloakingType.ImprovedCloaking;
		}

		private void SetBattleRiderType(BattleRiderTypes brt)
		{
			this.BattleRiderType = brt;
			this.IsBattleRider = brt != BattleRiderTypes.Unspecified;
		}

		private SuulkaType GetSuulkaType(string sectionName)
		{
			if (sectionName == "lv_suulka_the_cannibal")
				return SuulkaType.TheCannibal;
			if (sectionName == "lv_suulka_the_deaf")
				return SuulkaType.TheDeaf;
			if (sectionName == "lv_suulka_the_hidden")
				return SuulkaType.TheHidden;
			if (sectionName == "lv_suulka_the_immortal")
				return SuulkaType.TheImmortal;
			if (sectionName == "lv_suulka_the_kraken")
				return SuulkaType.TheKraken;
			if (sectionName == "lv_suulka_the_shaper")
				return SuulkaType.TheBloodweaver;
			if (sectionName == "lv_suulka_the_siren")
				return SuulkaType.TheSiren;
			if (sectionName == "lv_suulka_usurper")
				return SuulkaType.TheUsurper;
			return sectionName == "lv_blackelder" ? SuulkaType.TheBlack : SuulkaType.None;
		}
	}
}
