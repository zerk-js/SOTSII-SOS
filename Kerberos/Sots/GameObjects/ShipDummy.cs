// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameObjects.ShipDummy
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_SHIPDUMMY)]
	internal class ShipDummy : CompoundGameObject, IDisposable, IActive
	{
		private readonly List<IGameObject> _objects = new List<IGameObject>();
		private List<ShipDummy.ShipDummyPart> _dummyParts = new List<ShipDummy.ShipDummyPart>();
		private bool _active;
		private bool _checkStatusBootstrapped;
		public int ShipID;
		private int _fleetID;
		private ShipClass _shipClass;

		public int FleetID
		{
			get
			{
				return this._fleetID;
			}
			set
			{
				this._fleetID = value;
				this.PostSetProp("SetFleetID", value);
			}
		}

		public ShipDummy(App game, CreateShipDummyParams dummyParams)
		{
			this.ShipID = dummyParams.ShipID;
			this._checkStatusBootstrapped = false;
			this._shipClass = dummyParams.Sections.First<ShipSectionAsset>().Class;
			ShipSectionAsset shipSectionAsset = dummyParams.Sections.FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.Type == ShipSectionType.Mission));
			ShipDummy.ShipDummyPart shipDummyPart1 = new ShipDummy.ShipDummyPart();
			shipDummyPart1.Model = game.AddObject<StaticModel>((object)Ship.FixAssetNameForDLC(shipSectionAsset.ModelName, dummyParams.PreferredMount));
			shipDummyPart1.IsSection = true;
			this._dummyParts.Add(shipDummyPart1);
			this._objects.Add((IGameObject)shipDummyPart1.Model);
			foreach (ShipSectionAsset section in dummyParams.Sections)
			{
				ShipDummy.ShipDummyPart sectionPart = shipDummyPart1;
				if (section != shipSectionAsset)
				{
					ShipDummy.ShipDummyPart shipDummyPart2 = new ShipDummy.ShipDummyPart();
					shipDummyPart2.Model = game.AddObject<StaticModel>((object)Ship.FixAssetNameForDLC(section.ModelName, dummyParams.PreferredMount));
					shipDummyPart2.AttachedModel = (IGameObject)shipDummyPart1.Model;
					shipDummyPart2.AttachedNodeName = section.Type.ToString();
					shipDummyPart2.IsSection = true;
					this._dummyParts.Add(shipDummyPart2);
					this._objects.Add((IGameObject)shipDummyPart2.Model);
					sectionPart = shipDummyPart2;
				}
				for (int index = 0; index < section.Banks.Length; ++index)
				{
					LogicalBank bank = section.Banks[index];
					this.AddTurretsToShipDummy(game, dummyParams.PreferredMount, dummyParams.ShipFaction, section, sectionPart, dummyParams.AssignedWeapons, dummyParams.PreferredWeapons, game.AssetDatabase.Weapons, game.AssetDatabase.TurretHousings, (LogicalModule)null, (ShipDummy.ShipDummyPart)null, bank);
				}
				for (int sectionModuleMountIndex = 0; sectionModuleMountIndex < section.Modules.Length; ++sectionModuleMountIndex)
				{
					LogicalModuleMount moduleMount = section.Modules[sectionModuleMountIndex];
					if (dummyParams.AssignedModules != null)
					{
						LogicalModule module = (LogicalModule)null;
						ModuleAssignment moduleAssignment = dummyParams.AssignedModules.FirstOrDefault<ModuleAssignment>((Func<ModuleAssignment, bool>)(x => x.ModuleMount == moduleMount));
						if (moduleAssignment != null)
							module = moduleAssignment.Module;
						if (module == null)
							module = LogicalModule.EnumerateModuleFits(dummyParams.PreferredModules, section, sectionModuleMountIndex, false).FirstOrDefault<LogicalModule>();
						if (module != null)
						{
							ShipDummy.ShipDummyPart modulePart = new ShipDummy.ShipDummyPart();
							modulePart.Model = game.AddObject<StaticModel>((object)module.ModelPath);
							modulePart.AttachedModel = (IGameObject)sectionPart.Model;
							modulePart.AttachedNodeName = moduleMount.NodeName;
							this._dummyParts.Add(modulePart);
							this._objects.Add((IGameObject)modulePart.Model);
							for (int index = 0; index < module.Banks.Length; ++index)
							{
								LogicalBank bank = module.Banks[index];
								this.AddTurretsToShipDummy(game, dummyParams.PreferredMount, dummyParams.ShipFaction, section, sectionPart, dummyParams.AssignedWeapons, dummyParams.PreferredWeapons, game.AssetDatabase.Weapons, game.AssetDatabase.TurretHousings, module, modulePart, bank);
							}
						}
					}
				}
			}
			this._objects.Add((IGameObject)game.AddObject<RigidBody>((object)1f, (object)false));
		}

		private void AddTurretsToShipDummy(
		  App game,
		  string preferredMount,
		  Faction faction,
		  ShipSectionAsset section,
		  ShipDummy.ShipDummyPart sectionPart,
		  IEnumerable<WeaponAssignment> assignedWeapons,
		  IEnumerable<LogicalWeapon> preferredWeapons,
		  IEnumerable<LogicalWeapon> weapons,
		  IEnumerable<LogicalTurretHousing> turretHousings,
		  LogicalModule module,
		  ShipDummy.ShipDummyPart modulePart,
		  LogicalBank bank)
		{
			int designID = 0;
			int targetFilter = 0;
			int fireMode = 0;
			string moduleNodeName = modulePart != null ? modulePart.AttachedNodeName : "";
			LogicalWeapon weapon = ShipDummy.SelectWeapon(section, bank, assignedWeapons, preferredWeapons, weapons, moduleNodeName, out designID, out targetFilter, out fireMode);
			LogicalTurretClass weaponTurretClass = weapon.GetLogicalTurretClassForMount(bank.TurretSize, bank.TurretClass);
			if (weaponTurretClass == null)
			{
				App.Log.Warn(string.Format("Ship Dummy - did not find weapon turret class for: Bank Size [" + bank.TurretSize.ToString() + "], Bank Class [" + bank.TurretClass.ToString() + "] in section [" + section.FileName + "]"), "design");
			}
			else
			{
				LogicalTurretHousing housing = turretHousings.First<LogicalTurretHousing>((Func<LogicalTurretHousing, bool>)(housingCandidate =>
			   {
				   if (weaponTurretClass.TurretClass == housingCandidate.Class && weapon.DefaultWeaponSize == housingCandidate.WeaponSize)
					   return bank.TurretSize == housingCandidate.MountSize;
				   return false;
			   }));
				new MountObject.WeaponModels().FillOutModelFilesWithWeapon(weapon, faction, weapons);
				LogicalBank localBank = bank;
				foreach (LogicalMount mount in ((IEnumerable<LogicalMount>)section.Mounts).Where<LogicalMount>((Func<LogicalMount, bool>)(x => x.Bank == localBank)))
				{
					string baseModel = Ship.FixAssetNameForDLC(weaponTurretClass.GetBaseModel(faction, mount, housing), preferredMount);
					string turretModelName = Ship.FixAssetNameForDLC(weaponTurretClass.GetTurretModelName(faction, mount, housing), preferredMount);
					string barrelModelName = Ship.FixAssetNameForDLC(weaponTurretClass.GetBarrelModelName(faction, mount), preferredMount);
					this.AddTurretModels(game, baseModel, turretModelName, barrelModelName, mount.NodeName, sectionPart);
				}
				if (modulePart == null || module == null)
					return;
				foreach (LogicalMount mount in ((IEnumerable<LogicalMount>)module.Mounts).Where<LogicalMount>((Func<LogicalMount, bool>)(x => x.Bank == localBank)))
				{
					string baseModel = Ship.FixAssetNameForDLC(weaponTurretClass.GetBaseModel(faction, mount, housing), preferredMount);
					string turretModelName = Ship.FixAssetNameForDLC(weaponTurretClass.GetTurretModelName(faction, mount, housing), preferredMount);
					string barrelModelName = Ship.FixAssetNameForDLC(weaponTurretClass.GetBarrelModelName(faction, mount), preferredMount);
					this.AddTurretModels(game, baseModel, turretModelName, barrelModelName, mount.NodeName, modulePart);
				}
			}
		}

		private void AddTurretModels(
		  App game,
		  string baseModel,
		  string turretModelName,
		  string barrelModelName,
		  string attachedNodeName,
		  ShipDummy.ShipDummyPart attachedPart)
		{
			if (!string.IsNullOrEmpty(baseModel))
			{
				ShipDummy.ShipDummyPart shipDummyPart = new ShipDummy.ShipDummyPart();
				shipDummyPart.Model = game.AddObject<StaticModel>((object)baseModel);
				shipDummyPart.AttachedModel = (IGameObject)attachedPart.Model;
				shipDummyPart.AttachedNodeName = attachedNodeName;
				this._dummyParts.Add(shipDummyPart);
				this._objects.Add((IGameObject)shipDummyPart.Model);
			}
			if (string.IsNullOrEmpty(turretModelName))
				return;
			ShipDummy.ShipDummyPart shipDummyPart1 = new ShipDummy.ShipDummyPart();
			shipDummyPart1.Model = game.AddObject<StaticModel>((object)turretModelName);
			shipDummyPart1.AttachedModel = (IGameObject)attachedPart.Model;
			shipDummyPart1.AttachedNodeName = attachedNodeName;
			this._dummyParts.Add(shipDummyPart1);
			this._objects.Add((IGameObject)shipDummyPart1.Model);
			if (string.IsNullOrEmpty(barrelModelName))
				return;
			ShipDummy.ShipDummyPart shipDummyPart2 = new ShipDummy.ShipDummyPart();
			shipDummyPart2.Model = game.AddObject<StaticModel>((object)barrelModelName);
			shipDummyPart2.AttachedModel = (IGameObject)shipDummyPart1.Model;
			shipDummyPart2.AttachedNodeName = "barrel";
			this._dummyParts.Add(shipDummyPart2);
			this._objects.Add((IGameObject)shipDummyPart2.Model);
		}

		private static LogicalWeapon SelectWeapon(
		  ShipSectionAsset section,
		  LogicalBank bank,
		  IEnumerable<WeaponAssignment> assignedWeapons,
		  IEnumerable<LogicalWeapon> preferredWeapons,
		  IEnumerable<LogicalWeapon> weapons,
		  string moduleNodeName,
		  out int designID,
		  out int targetFilter,
		  out int fireMode)
		{
			LogicalWeapon logicalWeapon = (LogicalWeapon)null;
			designID = 0;
			targetFilter = 0;
			fireMode = 0;
			if (assignedWeapons != null)
			{
				WeaponAssignment weaponAssignment = assignedWeapons.FirstOrDefault<WeaponAssignment>((Func<WeaponAssignment, bool>)(x =>
			   {
				   if (x.Bank != bank)
					   return false;
				   if (x.ModuleNode != null)
					   return x.ModuleNode == moduleNodeName;
				   return true;
			   }));
				if (weaponAssignment != null)
				{
					logicalWeapon = weaponAssignment.Weapon;
					designID = weaponAssignment.DesignID;
					targetFilter = weaponAssignment.InitialTargetFilter ?? 0;
					fireMode = weaponAssignment.InitialFireMode ?? 0;
				}
			}
			if (logicalWeapon == null && !string.IsNullOrEmpty(bank.DefaultWeaponName))
				logicalWeapon = weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => string.Equals(x.WeaponName, bank.DefaultWeaponName, StringComparison.InvariantCultureIgnoreCase)));
			if (logicalWeapon == null && preferredWeapons != null)
				logicalWeapon = LogicalWeapon.EnumerateWeaponFits(section.Faction, section.SectionName, preferredWeapons, bank.TurretSize, bank.TurretClass).FirstOrDefault<LogicalWeapon>();
			if (logicalWeapon == null)
				logicalWeapon = LogicalWeapon.EnumerateWeaponFits(section.Faction, section.SectionName, weapons, bank.TurretSize, bank.TurretClass).FirstOrDefault<LogicalWeapon>();
			if (logicalWeapon == null)
				logicalWeapon = weapons.First<LogicalWeapon>();
			return logicalWeapon;
		}

		protected override GameObjectStatus OnCheckStatus()
		{
			GameObjectStatus gameObjectStatus = base.OnCheckStatus();
			if (gameObjectStatus != GameObjectStatus.Ready)
				return gameObjectStatus;
			if (this._objects.Any<IGameObject>((Func<IGameObject, bool>)(x => x.ObjectStatus == GameObjectStatus.Pending)))
				return GameObjectStatus.Pending;
			if (!this._checkStatusBootstrapped)
			{
				this._checkStatusBootstrapped = true;
				ShipDummy.ShipDummyPart shipDummyPart = this._dummyParts.First<ShipDummy.ShipDummyPart>((Func<ShipDummy.ShipDummyPart, bool>)(x =>
			   {
				   if (x.IsSection)
					   return x.AttachedModel == null;
				   return false;
			   }));
				foreach (ShipDummy.ShipDummyPart dummyPart in this._dummyParts)
				{
					if (dummyPart != shipDummyPart)
					{
						if (dummyPart.IsSection)
							dummyPart.Model.PostSetParent(dummyPart.AttachedModel, dummyPart.AttachedNodeName, dummyPart.AttachedNodeName);
						else
							dummyPart.Model.PostSetParent(dummyPart.AttachedModel, dummyPart.AttachedNodeName);
					}
				}
				shipDummyPart.Model.PostSetParent((IGameObject)this.RigidBody);
				this.PostObjectAddObjects(((IEnumerable<IGameObject>)new IGameObject[1]
				{
		  (IGameObject) this.RigidBody
				}).Concat<IGameObject>((IEnumerable<IGameObject>)this._dummyParts.Select<ShipDummy.ShipDummyPart, StaticModel>((Func<ShipDummy.ShipDummyPart, StaticModel>)(x => x.Model))).ToArray<IGameObject>());
				this.RigidBody.PostSetAggregate((IGameObject)this);
				List<StaticModel> list = this._dummyParts.Where<ShipDummy.ShipDummyPart>((Func<ShipDummy.ShipDummyPart, bool>)(x => x.IsSection)).Select<ShipDummy.ShipDummyPart, StaticModel>((Func<ShipDummy.ShipDummyPart, StaticModel>)(y => y.Model)).ToList<StaticModel>();
				List<object> objectList = new List<object>();
				objectList.Add((object)list.Count);
				foreach (StaticModel staticModel in list)
					objectList.Add((object)staticModel.ObjectID);
				this.PostSetProp("CreateBoundingBox", objectList.ToArray());
				this.PostSetProp("Activate");
			}
			return GameObjectStatus.Ready;
		}

		public RigidBody RigidBody
		{
			get
			{
				return this._objects.OfType<RigidBody>().First<RigidBody>();
			}
		}

		public ShipClass ShipClass
		{
			get
			{
				return this._shipClass;
			}
		}

		public void Dispose()
		{
			this.App.ReleaseObjects(this._objects.Concat<IGameObject>((IEnumerable<IGameObject>)new ShipDummy[1]
			{
		this
			}));
		}

		public bool Active
		{
			get
			{
				return this._active;
			}
			set
			{
				if (value == this._active)
					return;
				this._active = value;
				this.PostSetActive(this._active);
			}
		}

		private class ShipDummyPart
		{
			public string AttachedNodeName = "";
			public StaticModel Model;
			public IGameObject AttachedModel;
			public bool IsSection;
		}
	}
}
