// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ShipFramework.ShipSection
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Kerberos.Sots.Data.ShipFramework
{
	public class ShipSection : IXmlLoadSave
	{
		public string Title = "";
		public string Description = "";
		public string CombatAiType = "";
		public string ModelPath = "";
		public string DamageModelPath = "";
		public string DamageEffectPath = "";
		public string DestroyedModelPath = "";
		public string DestroyedEffectPath = "";
		public string AmbientSound = "";
		public string EngineSound = "";
		public string UnderAttackSound = "";
		public string DestroyedSound = "";
		public Size TopArmor = new Size() { X = 10, Y = 10 };
		public Size BottomArmor = new Size()
		{
			X = 10,
			Y = 10
		};
		public Size SideArmor = new Size()
		{
			X = 10,
			Y = 10
		};
		public string StationType = "";
		public List<Bank> Banks = new List<Bank>();
		public List<ModuleMount> Modules = new List<ModuleMount>();
		public List<BattleRiderMount> BattleRiderMounts = new List<BattleRiderMount>();
		public List<Tech> Techs = new List<Tech>();
		public List<ExcludedSection> ExcludedSections = new List<ExcludedSection>();
		public List<ExcludedType> ExcludedTypes = new List<ExcludedType>();
		public List<AvailablePsionicAbility> PsionicAbilities = new List<AvailablePsionicAbility>();
		public List<WeaponGroup> WeaponGroups = new List<WeaponGroup>();
		public List<ShipOptionGroup> ShipOptionGroups = new List<ShipOptionGroup>();
		internal const string XmlShipSectionName = "ShipSection";
		private const string XmlNameAiType = "AiType";
		private const string XmlNameRealClass = "RealClass";
		private const string XmlNameTitle = "Title";
		private const string XmlNameDescription = "Description";
		private const string XmlNameModelPath = "ModelFile";
		private const string XmlNameDamagedModelPath = "DamagedModelFile";
		private const string XmlNameDamagedEffectPath = "DamagedEffectFile";
		private const string XmlNameDestroyedModelPath = "DestroyedModelFile";
		private const string XmlNameDestroyedEffectPath = "DestroyedEffectFile";
		private const string XmlNameAmbientSound = "AmbientSound";
		private const string XmlNameEngineSound = "EngineSound";
		private const string XmlNameCameraDistanceFactor = "CameraDistanceFactor";
		private const string XmlNameStruct = "Struct";
		private const string XmlNameStructDamageAmount = "StructDamageAmount";
		private const string XmlNameDeathDamage = "DeathDamage";
		private const string XmlNameExplosionRadius = "ExplosionRadius";
		private const string XmlNameMass = "Mass";
		private const string XmlNameTop = "Top";
		private const string XmlNameBottom = "Bottom";
		private const string XmlNameSide = "Side";
		private const string XmlNameCrew = "Crew";
		private const string XmlNameCrewRequired = "CrewRequired";
		private const string XmlNamePower = "Power";
		private const string XmlNameSupply = "Supply";
		private const string XmlNameECM = "ECM";
		private const string XmlNameECCM = "ECCM";
		private const string XmlNameColonizerSpace = "ColonizerSpace";
		private const string XmlNameSavingsCost = "SavingsCost";
		private const string XmlNameProductionCost = "ProductionCost";
		private const string XmlNameFtlSpeed = "FtlSpeed";
		private const string XmlNameNodeSpeed = "NodeSpeed";
		private const string XmlNameMissionTime = "MissionTime";
		private const string XmlNameCommandPoints = "CommandPoints";
		private const string XmlNameRepairPoints = "RepairPoints";
		private const string XmlNameTerraformingPoints = "TerraformingPoints";
		private const string XmlNameFreighterSpace = "FreighterSpace";
		private const string XmlNameSignature = "Signature";
		private const string XmlNameTacticalSensorRange = "TacticalSensorRange";
		private const string XmlNameStrategicSensorRange = "StrategicSensorRange";
		private const string XmlNameLaunchDelay = "LaunchDelay";
		private const string XmlNameDockingDelay = "DockingDelay";
		private const string XmlNameBattleRiderReserveSize = "BattleRiderReserveSize";
		private const string XmlNameStationType = "StationType";
		private const string XmlNameStationLevel = "StationLevel";
		private const string XmlNameIsConstructor = "IsConstructor";
		private const string XmlNameIsFreighter = "IsFreighter";
		private const string XmlNameConstructionPoints = "ConstructionPoints";
		private const string XmlNameIsPolice = "IsPolice";
		private const string XmlPsionicPowerLevel = "PsionicPowerLevel";
		private const string XmlNameAcceleration = "Acceleration";
		private const string XmlNameRotationalAccelerationYaw = "RotationalAccelerationYaw";
		private const string XmlNameRotationalAccelerationPitch = "RotationalAccelerationPitch";
		private const string XmlNameRotationalAccelerationRoll = "RotationalAccelerationRoll";
		private const string XmlNameDecceleration = "Decceleration";
		private const string XmlNameLinearSpeed = "LinearSpeed";
		private const string XmlNameRotationSpeed = "RotationSpeed";
		private const string XmlNameBanks = "Banks";
		private const string XmlNameBank = "Bank";
		private const string XmlNameTechs = "RequiredTechs";
		private const string XmlNameTech = "Tech";
		private const string XmlNameModules = "Modules";
		private const string XmlNameModule = "Module";
		private const string XmlNameExcludedSections = "ExcludedSections";
		private const string XmlNameExcludedSection = "ExcludedSection";
		private const string XmlNameExcludedTypes = "ExcludedTypes";
		private const string XmlNameExcludedType = "ExcludedType";
		private const string XmlNamePsionicAbilities = "PsionicAbilities";
		private const string XmlNameWeaponGroups = "WeaponGroups";
		private const string XmlNameShipOptionGroups = "ShipOptionGroups";
		private const string XmlSlaveCapacity = "SlaveCapacity";
		private const string XmlNameFleetSpeedModifier = "FleetSpeedModifier";
		private const string XmlPreviewImage = "PreviewImage";
		public string SavePath;
		public ShipSectionType ShipType;
		public ShipClass ShipClass;
		public RealShipClasses? RealShipClass;
		public float CameraDistanceFactor;
		public int Struct;
		public int StructDamageAmount;
		public float DeathDamage;
		public float ExplosionRadius;
		public int Mass;
		public int Crew;
		public int CrewRequired;
		public int Power;
		public int Supply;
		public float ECM;
		public float ECCM;
		public int ColonizerSpace;
		public int SavingsCost;
		public int ProductionCost;
		public float FtlSpeed;
		public float NodeSpeed;
		public float MissionTime;
		public int CommandPoints;
		public int RepairPoints;
		public int TerraformingPoints;
		public int FreighterSpace;
		public float Signature;
		public float TacticalSensorRange;
		public float StrategicSensorRange;
		public float LaunchDelay;
		public float DockingDelay;
		public int BattleRiderReserveSize;
		public int StationLevel;
		public bool isConstructor;
		public bool isFreighter;
		public int ConstructionPoints;
		public bool isPolice;
		public float PsionicPowerLevel;
		public int SlaveCapacity;
		public float FleetSpeedModifier;
		public string PreviewImage;
		public float Acceleration;
		public float RotationalAccelerationYaw;
		public float RotationalAccelerationPitch;
		public float RotationalAccelerationRoll;
		public float Decceleration;
		public float LinearSpeed;
		public float RotationSpeed;

		public string XmlName
		{
			get
			{
				throw XmlHelper.NoXmlNameException;
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.RealShipClass, "RealClass", ref node);
			XmlHelper.AddNode((object)this.Title, "Title", ref node);
			XmlHelper.AddNode((object)this.Description, "Description", ref node);
			XmlHelper.AddNode((object)this.CombatAiType, "AiType", ref node);
			XmlHelper.AddNode((object)this.ModelPath, "ModelFile", ref node);
			XmlHelper.AddNode((object)this.DamageModelPath, "DamagedModelFile", ref node);
			XmlHelper.AddNode((object)this.DamageEffectPath, "DamagedEffectFile", ref node);
			XmlHelper.AddNode((object)this.DestroyedModelPath, "DestroyedModelFile", ref node);
			XmlHelper.AddNode((object)this.DestroyedEffectPath, "DestroyedEffectFile", ref node);
			XmlHelper.AddNode((object)this.AmbientSound, "AmbientSound", ref node);
			XmlHelper.AddNode((object)this.EngineSound, "EngineSound", ref node);
			XmlHelper.AddNode((object)this.CameraDistanceFactor, "CameraDistanceFactor", ref node);
			XmlHelper.AddNode((object)this.Struct, "Struct", ref node);
			XmlHelper.AddNode((object)this.StructDamageAmount, "StructDamageAmount", ref node);
			XmlHelper.AddNode((object)this.DeathDamage, "DeathDamage", ref node);
			XmlHelper.AddNode((object)this.ExplosionRadius, "ExplosionRadius", ref node);
			XmlHelper.AddNode((object)this.Mass, "Mass", ref node);
			XmlHelper.AddNode((object)this.TopArmor, "Top", ref node);
			XmlHelper.AddNode((object)this.BottomArmor, "Bottom", ref node);
			XmlHelper.AddNode((object)this.SideArmor, "Side", ref node);
			XmlHelper.AddNode((object)this.Crew, "Crew", ref node);
			XmlHelper.AddNode((object)this.CrewRequired, "CrewRequired", ref node);
			XmlHelper.AddNode((object)this.Power, "Power", ref node);
			XmlHelper.AddNode((object)this.Supply, "Supply", ref node);
			XmlHelper.AddNode((object)this.ECM, "ECM", ref node);
			XmlHelper.AddNode((object)this.ECCM, "ECCM", ref node);
			XmlHelper.AddNode((object)this.ColonizerSpace, "ColonizerSpace", ref node);
			XmlHelper.AddNode((object)this.SavingsCost, "SavingsCost", ref node);
			XmlHelper.AddNode((object)this.ProductionCost, "ProductionCost", ref node);
			XmlHelper.AddNode((object)this.FtlSpeed, "FtlSpeed", ref node);
			XmlHelper.AddNode((object)this.NodeSpeed, "NodeSpeed", ref node);
			XmlHelper.AddNode((object)this.MissionTime, "MissionTime", ref node);
			XmlHelper.AddNode((object)this.CommandPoints, "CommandPoints", ref node);
			XmlHelper.AddNode((object)this.RepairPoints, "RepairPoints", ref node);
			XmlHelper.AddNode((object)this.TerraformingPoints, "TerraformingPoints", ref node);
			XmlHelper.AddNode((object)this.FreighterSpace, "FreighterSpace", ref node);
			XmlHelper.AddNode((object)this.Signature, "Signature", ref node);
			XmlHelper.AddNode((object)this.TacticalSensorRange, "TacticalSensorRange", ref node);
			XmlHelper.AddNode((object)this.StrategicSensorRange, "StrategicSensorRange", ref node);
			XmlHelper.AddNode((object)this.LaunchDelay, "LaunchDelay", ref node);
			XmlHelper.AddNode((object)this.DockingDelay, "DockingDelay", ref node);
			XmlHelper.AddNode((object)this.BattleRiderReserveSize, "BattleRiderReserveSize", ref node);
			XmlHelper.AddNode((object)this.StationType, "StationType", ref node);
			XmlHelper.AddNode((object)this.StationLevel, "StationLevel", ref node);
			XmlHelper.AddNode((object)this.isConstructor, "IsConstructor", ref node);
			XmlHelper.AddNode((object)this.isFreighter, "IsFreighter", ref node);
			XmlHelper.AddNode((object)this.ConstructionPoints, "ConstructionPoints", ref node);
			XmlHelper.AddNode((object)this.isPolice, "IsPolice", ref node);
			XmlHelper.AddNode((object)this.PsionicPowerLevel, "PsionicPowerLevel", ref node);
			XmlHelper.AddNode((object)this.SlaveCapacity, "SlaveCapacity", ref node);
			XmlHelper.AddNode((object)this.FleetSpeedModifier, "FleetSpeedModifier", ref node);
			XmlHelper.AddNode((object)this.PreviewImage, "PreviewImage", ref node);
			XmlHelper.AddNode((object)this.Acceleration, "Acceleration", ref node);
			XmlHelper.AddNode((object)this.RotationalAccelerationYaw, "RotationalAccelerationYaw", ref node);
			XmlHelper.AddNode((object)this.RotationalAccelerationPitch, "RotationalAccelerationPitch", ref node);
			XmlHelper.AddNode((object)this.RotationalAccelerationRoll, "RotationalAccelerationRoll", ref node);
			XmlHelper.AddNode((object)this.Decceleration, "Decceleration", ref node);
			XmlHelper.AddNode((object)this.LinearSpeed, "LinearSpeed", ref node);
			XmlHelper.AddNode((object)this.RotationSpeed, "RotationSpeed", ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.Banks, "Banks", ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.Techs, "RequiredTechs", "Tech", ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.Modules, "Modules", "Module", ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.ExcludedSections, "ExcludedSections", "ExcludedSection", ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.ExcludedTypes, "ExcludedTypes", "ExcludedType", ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.PsionicAbilities, "PsionicAbilities", "PsionicAbility", ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.WeaponGroups, "WeaponGroups", "WeaponGroup", ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.ShipOptionGroups, "ShipOptionGroups", ShipOptionGroup.XmlNameShipOptionGroup, ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			string data = XmlHelper.GetData<string>(node, "RealClass");
			this.RealShipClass = string.IsNullOrEmpty(data) ? new RealShipClasses?() : new RealShipClasses?((RealShipClasses)Enum.Parse(typeof(RealShipClasses), data));
			this.Title = XmlHelper.GetData<string>(node, "Title");
			this.Description = XmlHelper.GetData<string>(node, "Description");
			this.CombatAiType = XmlHelper.GetData<string>(node, "AiType");
			this.ModelPath = XmlHelper.GetData<string>(node, "ModelFile");
			this.DamageModelPath = XmlHelper.GetData<string>(node, "DamagedModelFile");
			this.DamageEffectPath = XmlHelper.GetData<string>(node, "DamagedEffectFile");
			this.DestroyedModelPath = XmlHelper.GetData<string>(node, "DestroyedModelFile");
			this.DestroyedEffectPath = XmlHelper.GetData<string>(node, "DestroyedEffectFile");
			this.AmbientSound = XmlHelper.GetData<string>(node, "AmbientSound");
			this.EngineSound = XmlHelper.GetDataOrDefault<string>(node["EngineSound"], "");
			this.CameraDistanceFactor = XmlHelper.GetDataOrDefault<float>(node["CameraDistanceFactor"], 1f);
			this.Struct = XmlHelper.GetData<int>(node, "Struct");
			this.StructDamageAmount = XmlHelper.GetData<int>(node, "StructDamageAmount");
			this.DeathDamage = XmlHelper.GetData<float>(node, "DeathDamage");
			this.ExplosionRadius = XmlHelper.GetData<float>(node, "ExplosionRadius");
			this.Mass = XmlHelper.GetData<int>(node, "Mass");
			this.TopArmor = (Size)XmlHelper.GetData<string>(node, "Top");
			this.BottomArmor = (Size)XmlHelper.GetData<string>(node, "Bottom");
			this.SideArmor = (Size)XmlHelper.GetData<string>(node, "Side");
			this.Crew = XmlHelper.GetData<int>(node, "Crew");
			this.CrewRequired = XmlHelper.GetData<int>(node, "CrewRequired");
			this.Power = XmlHelper.GetData<int>(node, "Power");
			this.Supply = XmlHelper.GetData<int>(node, "Supply");
			this.ECM = XmlHelper.GetData<float>(node, "ECM");
			this.ECCM = XmlHelper.GetData<float>(node, "ECCM");
			this.ColonizerSpace = XmlHelper.GetData<int>(node, "ColonizerSpace");
			this.SavingsCost = XmlHelper.GetData<int>(node, "SavingsCost");
			this.ProductionCost = XmlHelper.GetData<int>(node, "ProductionCost");
			this.FtlSpeed = XmlHelper.GetData<float>(node, "FtlSpeed");
			this.NodeSpeed = XmlHelper.GetData<float>(node, "NodeSpeed");
			this.MissionTime = XmlHelper.GetData<float>(node, "MissionTime");
			this.CommandPoints = XmlHelper.GetData<int>(node, "CommandPoints");
			this.RepairPoints = XmlHelper.GetData<int>(node, "RepairPoints");
			this.TerraformingPoints = XmlHelper.GetData<int>(node, "TerraformingPoints");
			this.FreighterSpace = XmlHelper.GetData<int>(node, "FreighterSpace");
			this.Signature = XmlHelper.GetData<float>(node, "Signature");
			this.TacticalSensorRange = XmlHelper.GetData<float>(node, "TacticalSensorRange");
			this.StrategicSensorRange = XmlHelper.GetData<float>(node, "StrategicSensorRange");
			this.LaunchDelay = XmlHelper.GetData<float>(node, "LaunchDelay");
			this.DockingDelay = XmlHelper.GetData<float>(node, "DockingDelay");
			this.BattleRiderReserveSize = XmlHelper.GetData<int>(node, "BattleRiderReserveSize");
			this.StationType = XmlHelper.GetData<string>(node, "StationType");
			this.StationLevel = XmlHelper.GetData<int>(node, "StationLevel");
			this.isConstructor = XmlHelper.GetData<bool>(node, "IsConstructor");
			this.isFreighter = XmlHelper.GetData<bool>(node, "IsFreighter");
			this.ConstructionPoints = XmlHelper.GetData<int>(node, "ConstructionPoints");
			this.isPolice = XmlHelper.GetData<bool>(node, "IsPolice");
			this.PsionicPowerLevel = XmlHelper.GetData<float>(node, "PsionicPowerLevel");
			this.SlaveCapacity = XmlHelper.GetData<int>(node, "SlaveCapacity");
			this.FleetSpeedModifier = XmlHelper.GetDataOrDefault<float>(node["FleetSpeedModifier"], 1f);
			this.PreviewImage = XmlHelper.GetData<string>(node, "PreviewImage");
			this.Acceleration = XmlHelper.GetData<float>(node, "Acceleration");
			this.RotationalAccelerationYaw = XmlHelper.GetData<float>(node, "RotationalAccelerationYaw");
			this.RotationalAccelerationPitch = XmlHelper.GetData<float>(node, "RotationalAccelerationPitch");
			this.RotationalAccelerationRoll = XmlHelper.GetData<float>(node, "RotationalAccelerationRoll");
			this.Decceleration = XmlHelper.GetData<float>(node, "Decceleration");
			this.LinearSpeed = XmlHelper.GetData<float>(node, "LinearSpeed");
			this.RotationSpeed = XmlHelper.GetData<float>(node, "RotationSpeed");
			this.Banks = XmlHelper.GetDataObjectCollection<Bank>(node, "Banks", "Bank");
			this.Techs = XmlHelper.GetDataObjectCollection<Tech>(node, "RequiredTechs", "Tech");
			this.Modules = XmlHelper.GetDataObjectCollection<ModuleMount>(node, "Modules", "Module");
			this.ExcludedSections = XmlHelper.GetDataObjectCollection<ExcludedSection>(node, "ExcludedSections", "ExcludedSection");
			this.ExcludedTypes = XmlHelper.GetDataObjectCollection<ExcludedType>(node, "ExcludedTypes", "ExcludedType");
			this.PsionicAbilities = XmlHelper.GetDataObjectCollection<AvailablePsionicAbility>(node, "PsionicAbilities", "PsionicAbility");
			this.WeaponGroups = XmlHelper.GetDataObjectCollection<WeaponGroup>(node, "WeaponGroups", "WeaponGroup");
			this.ShipOptionGroups = XmlHelper.GetDataObjectCollection<ShipOptionGroup>(node, "ShipOptionGroups", ShipOptionGroup.XmlNameShipOptionGroup);
		}
	}
}
