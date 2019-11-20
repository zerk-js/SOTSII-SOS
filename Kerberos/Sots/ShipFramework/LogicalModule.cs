// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.ShipFramework.LogicalModule
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.GameStates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kerberos.Sots.ShipFramework
{
	internal class LogicalModule
	{
		public string Faction = "";
		public string ModuleName = "";
		public string ModuleTitle = "";
		public string Description = "";
		public string ModuleType = "";
		public string ModulePath = "";
		public string ModelPath = "";
		public string LowStructModelPath = "";
		public string DeadModelPath = "";
		public string AmbientSound = "";
		public string Icon = "";
		public ShipClass Class = ShipClass.BattleRider;
		public List<Tech> Techs = new List<Tech>();
		public ModuleEnums.ModuleAbilities AbilityType;
		public float LowStruct;
		public int AbilitySupply;
		public int Crew;
		public int CrewRequired;
		public int Supply;
		public float Structure;
		public float StructureBonus;
		public int ArmorBonus;
		public float ECCM;
		public float ECM;
		public int RepairPointsBonus;
		public float AccelBonus;
		public float CriticalHitBonus;
		public float AccuracyBonus;
		public float ROFBonus;
		public float CrewEfficiencyBonus;
		public float DamageBonus;
		public float SensorBonus;
		public float AdmiralSurvivalBonus;
		public float PsionicPowerBonus;
		public float PsionicStaminaBonus;
		public int PowerBonus;
		public LogicalEffect DamageEffect;
		public LogicalEffect DeathEffect;
		public bool AssignByDefault;
		public int SavingsCost;
		public int UpkeepCost;
		public int ProductionCost;
		public int NumPsionicSlots;
		public LogicalBank[] Banks;
		public LogicalMount[] Mounts;
		public LogicalPsionic[] Psionics;
		public ShipSectionType[] ExcludeSectionTypes;
		public string[] ExcludeSections;
		public string[] IncludeSections;

		public bool SectionIsExcluded(ShipSectionAsset section)
		{
			string sectionName = Path.GetFileNameWithoutExtension(section.FileName);
			if (((IEnumerable<ShipSectionType>)this.ExcludeSectionTypes).Any<ShipSectionType>((Func<ShipSectionType, bool>)(x => x == section.Type)))
				return true;
			if (this.IncludeSections.Length > 0)
				return !((IEnumerable<string>)this.IncludeSections).Any<string>((Func<string, bool>)(x => x == sectionName));
			return ((IEnumerable<string>)this.ExcludeSections).Any<string>((Func<string, bool>)(x => x == sectionName));
		}

		public static IEnumerable<LogicalModule> EnumerateModuleFits(
		  IEnumerable<LogicalModule> modules,
		  ShipSectionAsset section,
		  int sectionModuleMountIndex,
		  bool debugStations = false)
		{
			LogicalModuleMount mount = section.Modules[sectionModuleMountIndex];
			if (section.Class == ShipClass.Station && !debugStations && !ModuleShipData.DebugAutoAssignModules)
			{
				if (!string.IsNullOrEmpty(mount.AssignedModuleName))
				{
					foreach (LogicalModule module in modules)
					{
						if (module.AssignByDefault && module.Faction == section.Faction && (module.ModuleName == mount.AssignedModuleName && !module.SectionIsExcluded(section)))
							yield return module;
					}
				}
				else
				{
					foreach (LogicalModule module in modules)
					{
						if (module.AssignByDefault && module.ModuleType == mount.ModuleType && (!module.SectionIsExcluded(section) && module.Faction == section.Faction))
							yield return module;
					}
				}
			}
			else if (!string.IsNullOrEmpty(mount.AssignedModuleName))
			{
				foreach (LogicalModule module in modules)
				{
					if (module.Faction == section.Faction && module.ModuleName == mount.AssignedModuleName && !module.SectionIsExcluded(section))
						yield return module;
				}
			}
			else
			{
				foreach (LogicalModule module in modules)
				{
					if (module.ModuleType == mount.ModuleType && !module.SectionIsExcluded(section) && module.Faction == section.Faction)
						yield return module;
				}
			}
		}
	}
}
