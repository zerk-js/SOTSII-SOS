// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.ShipFramework.ModuleLibrary
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Kerberos.Sots.ShipFramework
{
	internal static class ModuleLibrary
	{
		private static IEnumerable<string> ExtractModuleFiles(string filename)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(ScriptHost.FileSystem, filename);
			XmlElement listnode = doc["ModuleList"];
			XmlElement sectionsnode = listnode["Modules"];
			foreach (XmlElement xmlElement in sectionsnode.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name == "Module")))
				yield return PathHelpers.FixSeparators(xmlElement.GetAttribute("File"));
		}

		public static LogicalModule CreateLogicalModuleFromFile(
		  string moduleFile,
		  string faction)
		{
			ShipModule sm = new ShipModule();
			ShipModuleXmlUtility.LoadShipModuleFromXml(moduleFile, ref sm);
			LogicalModule logicalModule = new LogicalModule()
			{
				ModuleTitle = sm.ModuleTitle,
				Description = sm.Description,
				ModuleType = sm.ModuleType ?? string.Empty,
				ModulePath = PathHelpers.Combine("factions", faction, "modules", Path.GetFileName(moduleFile))
			};
			logicalModule.ModuleName = Path.GetFileNameWithoutExtension(logicalModule.ModulePath);
			logicalModule.ModelPath = sm.ModelPath;
			logicalModule.LowStructModelPath = sm.DamagedModelPath ?? string.Empty;
			logicalModule.DeadModelPath = sm.DestroyedModelPath ?? string.Empty;
			logicalModule.LowStruct = (float)sm.StructDamageAmount;
			logicalModule.AmbientSound = sm.AmbientSound;
			if (!string.IsNullOrEmpty(sm.AbilityType))
				logicalModule.AbilityType = (ModuleEnums.ModuleAbilities)Enum.Parse(typeof(ModuleEnums.ModuleAbilities), sm.AbilityType);
			logicalModule.AbilitySupply = sm.AbilitySupply;
			if (logicalModule.AbilitySupply == 0)
			{
				if (logicalModule.AbilityType == ModuleEnums.ModuleAbilities.GoopArmorRepair)
					logicalModule.AbilitySupply = 3;
				else if (logicalModule.AbilityType == ModuleEnums.ModuleAbilities.JokerECM)
					logicalModule.AbilitySupply = 5;
			}
			logicalModule.Crew = sm.Crew;
			logicalModule.CrewRequired = sm.CrewRequired;
			logicalModule.Supply = sm.Supply;
			logicalModule.Structure = (float)sm.Structure;
			logicalModule.StructureBonus = (float)sm.StructureBonus;
			logicalModule.ArmorBonus = sm.ArmorBonus;
			logicalModule.ECCM = sm.ECCM;
			logicalModule.ECM = sm.ECM;
			logicalModule.RepairPointsBonus = sm.RepairPointsBonus;
			logicalModule.AccelBonus = sm.AccelerationBonus / 100f;
			logicalModule.CriticalHitBonus = sm.CriticalHitBonus;
			logicalModule.SensorBonus = sm.SensorBonus;
			logicalModule.AdmiralSurvivalBonus = sm.AdmiralSurvivalBonus;
			logicalModule.PsionicPowerBonus = sm.PsionicPowerBonus;
			logicalModule.PsionicStaminaBonus = sm.PsionicStaminaBonus;
			logicalModule.PowerBonus = sm.Power;
			logicalModule.DamageEffect = new LogicalEffect()
			{
				Name = sm.DamagedEffectPath ?? string.Empty
			};
			logicalModule.DeathEffect = new LogicalEffect()
			{
				Name = sm.DestroyedEffectPath ?? string.Empty
			};
			logicalModule.AssignByDefault = sm.AssignByDefault;
			logicalModule.Icon = sm.IconSprite;
			logicalModule.ROFBonus = sm.ROFBonus;
			logicalModule.CrewEfficiencyBonus = sm.CrewEfficiencyBonus;
			if (logicalModule.AbilityType == ModuleEnums.ModuleAbilities.KarnakTargeting)
			{
				logicalModule.AccuracyBonus = 10f;
				logicalModule.DamageBonus = 0.15f;
			}
			if (logicalModule.ModulePath.Contains("cr_"))
				logicalModule.Class = ShipClass.Cruiser;
			else if (logicalModule.ModulePath.Contains("dn_"))
				logicalModule.Class = ShipClass.Dreadnought;
			else if (logicalModule.ModulePath.Contains("lv_"))
				logicalModule.Class = ShipClass.Leviathan;
			else if (logicalModule.ModulePath.Contains("sn_"))
				logicalModule.Class = ShipClass.Station;
			List<LogicalBank> logicalBankList = new List<LogicalBank>();
			List<LogicalMount> logicalMountList = new List<LogicalMount>();
			List<LogicalPsionic> logicalPsionicList = new List<LogicalPsionic>();
			foreach (Bank bank in sm.Banks)
			{
				LogicalBank logicalBank = new LogicalBank()
				{
					TurretSize = (WeaponEnums.WeaponSizes)Enum.Parse(typeof(WeaponEnums.WeaponSizes), bank.Size),
					Section = (ShipSectionAsset)null,
					Module = logicalModule,
					GUID = Guid.Parse(bank.Id),
					DefaultWeaponName = bank.DefaultWeapon
				};
				logicalBank.TurretClass = (WeaponEnums.TurretClasses)Enum.Parse(typeof(WeaponEnums.TurretClasses), bank.Class);
				logicalBankList.Add(logicalBank);
				foreach (Mount mount in bank.Mounts)
				{
					LogicalMount logicalMount = new LogicalMount()
					{
						Bank = logicalBank,
						NodeName = mount.NodeName,
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
			List<string> stringList1 = new List<string>();
			List<ShipSectionType> shipSectionTypeList = new List<ShipSectionType>();
			foreach (ExcludedSection excludedSection in sm.ExcludedSections)
				stringList1.Add(excludedSection.Name);
			foreach (ExcludedType excludedType in sm.ExcludedTypes)
			{
				ShipSectionType shipSectionType = ShipSectionType.Command;
				if (excludedType.Name == "Engine")
					shipSectionType = ShipSectionType.Engine;
				else if (excludedType.Name == "Mission")
					shipSectionType = ShipSectionType.Mission;
				shipSectionTypeList.Add(shipSectionType);
			}
			List<string> stringList2 = new List<string>();
			foreach (IncludedSection includedSection in sm.IncludedSections)
				stringList2.Add(includedSection.Name);
			logicalModule.NumPsionicSlots = 0;
			if (logicalModule.ModuleTitle.Contains("PROFESSORX") || logicalModule.ModuleTitle.Contains("PSIWAR"))
			{
				if (logicalModule.Class == ShipClass.Cruiser)
					logicalModule.NumPsionicSlots = 1;
				else if (logicalModule.Class == ShipClass.Dreadnought)
					logicalModule.NumPsionicSlots = 3;
				for (int index = 0; index < logicalModule.NumPsionicSlots; ++index)
				{
					LogicalPsionic logicalPsionic = new LogicalPsionic();
					logicalPsionicList.Add(logicalPsionic);
				}
			}
			logicalModule.Banks = logicalBankList.ToArray();
			logicalModule.Mounts = logicalMountList.ToArray();
			logicalModule.Psionics = logicalPsionicList.ToArray();
			logicalModule.Techs = sm.Techs;
			logicalModule.SavingsCost = sm.SavingsCost;
			logicalModule.UpkeepCost = sm.UpkeepCost;
			logicalModule.ProductionCost = sm.ProductionCost;
			logicalModule.ExcludeSections = stringList1.ToArray();
			logicalModule.IncludeSections = stringList2.ToArray();
			logicalModule.ExcludeSectionTypes = shipSectionTypeList.ToArray();
			logicalModule.Faction = faction;
			return logicalModule;
		}

		public static IEnumerable<LogicalModule> Enumerate()
		{
			foreach (string directory in ScriptHost.FileSystem.FindDirectories("factions\\*"))
			{
				string fileName = PathHelpers.Combine(directory, "modules\\_modules.xml");
				if (ScriptHost.FileSystem.FileExists(fileName))
				{
					string faction = Path.GetFileNameWithoutExtension(directory);
					IEnumerator<string> enumerator = ModuleLibrary.ExtractModuleFiles(fileName).GetEnumerator();
					while (enumerator.MoveNext())
					{
						string file = enumerator.Current;
						yield return ModuleLibrary.CreateLogicalModuleFromFile(file, faction);
					}
				}
			}
		}
	}
}
