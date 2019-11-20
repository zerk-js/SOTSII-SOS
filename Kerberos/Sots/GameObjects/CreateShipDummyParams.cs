// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameObjects.CreateShipDummyParams
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kerberos.Sots.GameObjects
{
	internal class CreateShipDummyParams
	{
		public string PreferredMount = "";
		public IEnumerable<ShipSectionAsset> Sections = (IEnumerable<ShipSectionAsset>)new ShipSectionAsset[0];
		public IEnumerable<LogicalWeapon> PreferredWeapons = (IEnumerable<LogicalWeapon>)new LogicalWeapon[0];
		public IEnumerable<WeaponAssignment> AssignedWeapons = (IEnumerable<WeaponAssignment>)new WeaponAssignment[0];
		public IEnumerable<LogicalModule> PreferredModules = (IEnumerable<LogicalModule>)new LogicalModule[0];
		public IEnumerable<ModuleAssignment> AssignedModules = (IEnumerable<ModuleAssignment>)new ModuleAssignment[0];
		public int ShipID;
		public Faction ShipFaction;

		public static CreateShipDummyParams ObtainShipDummyParams(
		  App game,
		  ShipInfo shipInfo)
		{
			IEnumerable<string> modules = game.AssetDatabase.Modules.Select<LogicalModule, string>((Func<LogicalModule, string>)(x => x.ModuleName));
			IEnumerable<string> weapons = game.AssetDatabase.Weapons.Select<LogicalWeapon, string>((Func<LogicalWeapon, string>)(x => x.Name));
			DesignInfo designInfo = shipInfo.DesignInfo;
			List<ShipSectionAsset> shipSectionAssetList = new List<ShipSectionAsset>();
			List<ModuleAssignment> moduleAssignmentList = new List<ModuleAssignment>();
			List<WeaponAssignment> weaponAssignmentList = new List<WeaponAssignment>();
			foreach (DesignSectionInfo designSection in designInfo.DesignSections)
			{
				DesignSectionInfo sectionInfo = designSection;
				ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == sectionInfo.FilePath));
				shipSectionAssetList.Add(shipSectionAsset);
				foreach (LogicalBank bank1 in shipSectionAsset.Banks)
				{
					LogicalBank bank = bank1;
					WeaponBankInfo weaponBankInfo = sectionInfo.WeaponBanks.FirstOrDefault<WeaponBankInfo>((Func<WeaponBankInfo, bool>)(x => x.BankGUID == bank.GUID));
					bool flag = false;
					if (weaponBankInfo != null && weaponBankInfo.WeaponID.HasValue)
					{
						string weaponName = Path.GetFileNameWithoutExtension(game.GameDatabase.GetWeaponAsset(weaponBankInfo.WeaponID.Value));
						WeaponAssignment weaponAssignment = new WeaponAssignment()
						{
							ModuleNode = "",
							Bank = bank,
							Weapon = game.AssetDatabase.Weapons.First<LogicalWeapon>((Func<LogicalWeapon, bool>)(weapon => string.Equals(weapon.WeaponName, weaponName, StringComparison.InvariantCultureIgnoreCase))),
							DesignID = weaponBankInfo == null || !weaponBankInfo.DesignID.HasValue ? 0 : weaponBankInfo.DesignID.Value,
							InitialTargetFilter = new int?(weaponBankInfo.FilterMode ?? 0),
							InitialFireMode = new int?(weaponBankInfo.FiringMode ?? 0)
						};
						weaponAssignmentList.Add(weaponAssignment);
						flag = true;
					}
					if (!flag && !string.IsNullOrEmpty(bank.DefaultWeaponName))
					{
						WeaponAssignment weaponAssignment = new WeaponAssignment()
						{
							ModuleNode = "",
							Bank = bank,
							Weapon = game.AssetDatabase.Weapons.First<LogicalWeapon>((Func<LogicalWeapon, bool>)(weapon => string.Equals(weapon.WeaponName, bank.DefaultWeaponName, StringComparison.InvariantCultureIgnoreCase))),
							DesignID = weaponBankInfo == null || !weaponBankInfo.DesignID.HasValue ? 0 : weaponBankInfo.DesignID.Value
						};
						weaponAssignmentList.Add(weaponAssignment);
					}
				}
				foreach (LogicalModuleMount module in shipSectionAsset.Modules)
				{
					LogicalModuleMount sectionModule = module;
					DesignModuleInfo designModuleInfo = sectionInfo.Modules.FirstOrDefault<DesignModuleInfo>((Func<DesignModuleInfo, bool>)(x => x.MountNodeName == sectionModule.NodeName));
					if (designModuleInfo != null)
					{
						string path = game.GameDatabase.GetModuleAsset(designModuleInfo.ModuleID);
						LogicalModule logicalModule = game.AssetDatabase.Modules.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == path));
						moduleAssignmentList.Add(new ModuleAssignment()
						{
							ModuleMount = sectionModule,
							Module = logicalModule
						});
						if (designModuleInfo.WeaponID.HasValue)
						{
							string weaponPath = game.GameDatabase.GetWeaponAsset(designModuleInfo.WeaponID.Value);
							WeaponAssignment weaponAssignment = new WeaponAssignment()
							{
								ModuleNode = designModuleInfo.MountNodeName,
								Bank = logicalModule.Banks[0],
								Weapon = game.AssetDatabase.Weapons.First<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.FileName == weaponPath)),
								DesignID = 0
							};
							weaponAssignmentList.Add(weaponAssignment);
						}
					}
				}
			}
			ShipSectionAsset missionSection = shipSectionAssetList.FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.Type == ShipSectionType.Mission));
			Faction faction = game.AssetDatabase.Factions.First<Faction>((Func<Faction, bool>)(x => missionSection.Faction == x.Name));
			Player playerObject = game.Game.GetPlayerObject(designInfo.PlayerID);
			Subfaction subfaction = faction.Subfactions[Math.Min(playerObject.SubfactionIndex, faction.Subfactions.Length - 1)];
			return new CreateShipDummyParams()
			{
				ShipID = shipInfo.ID,
				PreferredMount = Ship.GetPreferredMount(game, playerObject, faction, shipSectionAssetList),
				ShipFaction = faction,
				Sections = (IEnumerable<ShipSectionAsset>)shipSectionAssetList.ToArray(),
				AssignedModules = (IEnumerable<ModuleAssignment>)moduleAssignmentList.ToArray(),
				PreferredModules = game.AssetDatabase.Modules.Where<LogicalModule>((Func<LogicalModule, bool>)(x => modules.Contains<string>(x.ModuleName))),
				AssignedWeapons = (IEnumerable<WeaponAssignment>)weaponAssignmentList.ToArray(),
				PreferredWeapons = game.AssetDatabase.Weapons.Where<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => weapons.Contains<string>(x.Name)))
			};
		}
	}
}
