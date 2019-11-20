// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.AllShipData
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.GameStates
{
	internal class AllShipData
	{
		public readonly Ring<FactionShipData> Factions = new Ring<FactionShipData>();

		public FactionShipData GetCurrentFactionShipData()
		{
			return this.Factions.Current;
		}

		public ClassShipData GetCurrentClassShipData()
		{
			return this.Factions.Current?.SelectedClass;
		}

		public SectionTypeShipData GetCurrentSectionTypeShipData(
		  ShipSectionType sectionType)
		{
			return this.Factions.Current.SelectedClass.SectionTypes.FirstOrDefault<SectionTypeShipData>((Func<SectionTypeShipData, bool>)(x => x.SectionType == sectionType));
		}

		public IEnumerable<SectionShipData> GetCurrentSections()
		{
			SectionShipData mission = this.GetCurrentSectionData(ShipSectionType.Mission);
			if (mission != null)
				yield return mission;
			SectionShipData command = this.GetCurrentSectionData(ShipSectionType.Command);
			if (command != null)
				yield return command;
			SectionShipData engine = this.GetCurrentSectionData(ShipSectionType.Engine);
			if (engine != null)
				yield return engine;
		}

		public SectionShipData GetCurrentSectionData(ShipSectionType sectionType)
		{
			return this.GetCurrentSectionTypeShipData(sectionType)?.SelectedSection;
		}

		public ShipSectionAsset GetCurrentSection(ShipSectionType sectionType)
		{
			return this.GetCurrentSectionData(sectionType)?.Section;
		}

		public string GetCurrentSectionAssetName(ShipSectionType sectionType)
		{
			ShipSectionAsset currentSection = this.GetCurrentSection(sectionType);
			if (currentSection != null)
				return currentSection.FileName;
			return string.Empty;
		}

		public string GetCurrentSectionName(ShipSectionType sectionType)
		{
			ShipSectionAsset currentSection = this.GetCurrentSection(sectionType);
			if (currentSection != null)
				return App.Localize(currentSection.Title);
			return "unknown";
		}

		public string GetCurrentFactionName()
		{
			if (this.Factions.Current != null)
				return this.Factions.Current.Faction.Name;
			return "unknown";
		}

		public Faction GetCurrentFaction()
		{
			if (this.Factions.Current != null)
				return this.Factions.Current.Faction;
			return (Faction)null;
		}

		public ModuleShipData GetCurrentModuleMount(
		  ShipSectionAsset section,
		  string mountNodeName)
		{
			SectionShipData sectionShipData = this.GetCurrentSections().FirstOrDefault<SectionShipData>((Func<SectionShipData, bool>)(x => x.Section == section));
			if (sectionShipData == null)
				return (ModuleShipData)null;
			return sectionShipData.Modules.FirstOrDefault<ModuleShipData>((Func<ModuleShipData, bool>)(x => x.ModuleMount.NodeName == mountNodeName));
		}

		public WeaponBankShipData GetCurrentWeaponBank(
		  GameDatabase db,
		  WeaponBankInfo bankInfo)
		{
			foreach (SectionShipData currentSection in this.GetCurrentSections())
			{
				foreach (WeaponBankShipData weaponBank in currentSection.WeaponBanks)
				{
					if (bankInfo.BankGUID == weaponBank.Bank.GUID)
						return weaponBank;
				}
			}
			return (WeaponBankShipData)null;
		}

		public IWeaponShipData GetCurrentWeaponBank(WeaponBank bank)
		{
			foreach (SectionShipData currentSection in this.GetCurrentSections())
			{
				foreach (WeaponBankShipData weaponBank in currentSection.WeaponBanks)
				{
					if (weaponBank.Bank == bank.LogicalBank)
						return (IWeaponShipData)weaponBank;
				}
				if (bank.Module != null)
				{
					foreach (ModuleShipData module in currentSection.Modules)
					{
						if (module.SelectedModule != null && bank.Module.Attachment == module.ModuleMount)
						{
							foreach (LogicalBank bank1 in module.SelectedModule.Module.Banks)
							{
								if (bank1 == bank.LogicalBank)
									return (IWeaponShipData)module.SelectedModule;
							}
						}
					}
				}
			}
			return (IWeaponShipData)null;
		}

		public IEnumerable<WeaponBankShipData> GetCurrentWeaponBanks(
		  ShipSectionType sectionType)
		{
			SectionShipData section = this.GetCurrentSectionData(sectionType);
			if (section != null)
			{
				foreach (WeaponBankShipData weaponBank in section.WeaponBanks)
					yield return weaponBank;
			}
		}

		public IEnumerable<WeaponBankShipData> GetCurrentWeaponBanks()
		{
			foreach (WeaponBankShipData currentWeaponBank in this.GetCurrentWeaponBanks(ShipSectionType.Command))
				yield return currentWeaponBank;
			foreach (WeaponBankShipData currentWeaponBank in this.GetCurrentWeaponBanks(ShipSectionType.Mission))
				yield return currentWeaponBank;
			foreach (WeaponBankShipData currentWeaponBank in this.GetCurrentWeaponBanks(ShipSectionType.Engine))
				yield return currentWeaponBank;
		}

		private IEnumerable<WeaponAssignment> GetWeaponAssignments(
		  ShipSectionType sectionType)
		{
			foreach (WeaponBankShipData currentWeaponBank in this.GetCurrentWeaponBanks(sectionType))
				yield return new WeaponAssignment()
				{
					ModuleNode = "",
					Bank = currentWeaponBank.Bank,
					Weapon = currentWeaponBank.SelectedWeapon,
					DesignID = currentWeaponBank.SelectedDesign,
					InitialFireMode = currentWeaponBank.FiringMode,
					InitialTargetFilter = currentWeaponBank.FilterMode
				};
			foreach (ModuleShipData currentSectionModule in this.GetCurrentSectionModules())
			{
				if (currentSectionModule.SelectedModule != null && currentSectionModule.SelectedModule.SelectedWeapon != null)
					yield return new WeaponAssignment()
					{
						ModuleNode = currentSectionModule.ModuleMount.NodeName,
						Bank = currentSectionModule.SelectedModule.Module.Banks[0],
						Weapon = currentSectionModule.SelectedModule.SelectedWeapon,
						DesignID = currentSectionModule.SelectedModule.SelectedDesign
					};
			}
		}

		public IEnumerable<WeaponAssignment> GetWeaponAssignments()
		{
			foreach (WeaponAssignment weaponAssignment in this.GetWeaponAssignments(ShipSectionType.Command))
				yield return weaponAssignment;
			foreach (WeaponAssignment weaponAssignment in this.GetWeaponAssignments(ShipSectionType.Mission))
				yield return weaponAssignment;
			foreach (WeaponAssignment weaponAssignment in this.GetWeaponAssignments(ShipSectionType.Engine))
				yield return weaponAssignment;
		}

		public RealShipClasses? GetCurrentClass()
		{
			return this.Factions.Current?.SelectedClass?.Class;
		}

		private IEnumerable<ModuleAssignment> GetModuleAssignments(
		  ShipSectionType sectionType)
		{
			IEnumerable<ModuleShipData> sectionModules = this.GetCurrentSectionModules(sectionType);
			foreach (ModuleShipData moduleShipData in sectionModules)
			{
				if (moduleShipData.SelectedModule != null)
					yield return new ModuleAssignment()
					{
						ModuleMount = moduleShipData.ModuleMount,
						Module = moduleShipData.SelectedModule.Module,
						PsionicAbilities = moduleShipData.SelectedModule.SelectedPsionic.ToArray()
					};
			}
		}

		public IEnumerable<ModuleAssignment> GetModuleAssignments()
		{
			foreach (ModuleAssignment moduleAssignment in this.GetModuleAssignments(ShipSectionType.Command))
				yield return moduleAssignment;
			foreach (ModuleAssignment moduleAssignment in this.GetModuleAssignments(ShipSectionType.Mission))
				yield return moduleAssignment;
			foreach (ModuleAssignment moduleAssignment in this.GetModuleAssignments(ShipSectionType.Engine))
				yield return moduleAssignment;
		}

		public IEnumerable<ModuleShipData> GetCurrentSectionModules(
		  ShipSectionType sectionType)
		{
			SectionShipData section = this.GetCurrentSectionData(sectionType);
			if (section != null)
			{
				foreach (ModuleShipData module in section.Modules)
					yield return module;
			}
		}

		public IEnumerable<ModuleShipData> GetCurrentSectionModules()
		{
			foreach (ModuleShipData currentSectionModule in this.GetCurrentSectionModules(ShipSectionType.Command))
				yield return currentSectionModule;
			foreach (ModuleShipData currentSectionModule in this.GetCurrentSectionModules(ShipSectionType.Mission))
				yield return currentSectionModule;
			foreach (ModuleShipData currentSectionModule in this.GetCurrentSectionModules(ShipSectionType.Engine))
				yield return currentSectionModule;
		}

		public bool IsCurrentShipDataValid()
		{
			return this.GetCurrentSection(ShipSectionType.Mission) != null;
		}
	}
}
