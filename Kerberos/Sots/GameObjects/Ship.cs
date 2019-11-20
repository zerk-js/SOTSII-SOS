// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameObjects.Ship
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Combat;
using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Ships;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_SHIP)]
	internal class Ship : CompoundGameObject, IDisposable, IPosition, IOrientatable, IActive
	{
		private readonly List<IGameObject> _objects = new List<IGameObject>();
		private bool _bAuthoritive = true;
		private Faction _faction;
		private int _currentTurretIndex;
		private int _currentModuleIndex;
		private bool _active;
		private bool _checkStatusBootstrapped;
		private bool _isDeployed;
		private bool _isDisposed;
		private bool _isUnderAttack;
		private float _accuracyModifier;
		private float _pdAccuracyModifier;
		private int _parentID;
		private int _parentDatabaseID;
		private SuulkaType _suulkaType;
		private bool _isSystemDefenceBoat;
		private bool _defenseBoatActive;
		private bool _isPolice;
		private bool _isPolicePatrolling;
		private bool _isQShip;
		private bool _isTrapDrone;
		private bool _isAcceleratorHoop;
		private bool _isListener;
		private bool _isLoa;
		private bool _isGardener;
		private Sphere _boundingSphere;
		private List<SpecWeaponControl> _weaponControls;
		private Turret.FiringEnum _turretFiring;
		private Player _player;
		private bool _visible;
		private CombatStance _stance;
		private int _currentPsiPower;
		private int _maxPsiPower;
		private float _sensorRange;
		private float _bonusSpottedRange;
		private CloakedState _cloakedState;
		private int _databaseID;
		private int _designID;
		private int _reserveSize;
		private int _riderIndex;
		private ShipClass _shipClass;
		private RealShipClasses _realShipClass;
		private ShipRole _shipRole;
		private SectionEnumerations.CombatAiType _combatAI;
		private ShipFleetAbilityType _abilityType;
		private BattleRiderTypes _battleRiderType;
		private float _signature;
		private List<Ship.DetectionState> _detectionStates;
		private int _inputId;
		private IGameObject _target;
		private bool _blindFireActive;
		private TaskGroup _taskGroup;
		private bool _isPlayerControlled;
		private WeaponRole _wpnRole;
		private string _priorityWeapon;
		private bool _bIsDriveless;
		private bool _bIsDestroyed;
		private bool _bIsNeutronStar;
		private bool _bHasRetreated;
		private bool _bHitByNodeCannon;
		private bool _instantlyKilled;
		private bool _bCanAcceptMoveOrders;
		private bool _bDockedWithParent;
		private bool _bCarrierCanLaunch;
		private bool _bIsPlanetAssaultShip;
		private bool _bAssaultingPlanet;
		private bool _bCanAvoid;

		public bool IsUnderAttack
		{
			get
			{
				return this._isUnderAttack;
			}
		}

		public float AccuracyModifier
		{
			get
			{
				return this._accuracyModifier;
			}
		}

		public float PDAccuracyModifier
		{
			get
			{
				return this._pdAccuracyModifier;
			}
		}

		public int ParentID
		{
			get
			{
				return this._parentID;
			}
			set
			{
				this._parentID = value;
			}
		}

		public int ParentDatabaseID
		{
			get
			{
				return this._parentDatabaseID;
			}
			set
			{
				this._parentDatabaseID = value;
			}
		}

		public bool IsSuulka
		{
			get
			{
				return this._suulkaType != SuulkaType.None;
			}
		}

		public bool IsSystemDefenceBoat
		{
			get
			{
				return this._isSystemDefenceBoat;
			}
		}

		public bool DefenseBoatActive
		{
			get
			{
				return this._defenseBoatActive;
			}
			set
			{
				if (!this._isSystemDefenceBoat || this._defenseBoatActive || !value)
					return;
				this.PostSetProp("ActivateDefenseBoat");
				this._defenseBoatActive = value;
			}
		}

		public bool IsPolice
		{
			get
			{
				return this._isPolice;
			}
		}

		public bool IsPolicePatrolling
		{
			get
			{
				return this._isPolicePatrolling;
			}
		}

		public bool IsQShip
		{
			get
			{
				return this._isQShip;
			}
		}

		public bool IsTrapDrone
		{
			get
			{
				return this._isTrapDrone;
			}
		}

		public bool IsAcceleratorHoop
		{
			get
			{
				return this._isAcceleratorHoop;
			}
		}

		public bool IsListener
		{
			get
			{
				return this._isListener;
			}
		}

		public bool IsNPCFreighter
		{
			get
			{
				if (this._shipRole == ShipRole.FREIGHTER)
					return !this._isQShip;
				return false;
			}
		}

		public bool IsLoa
		{
			get
			{
				return this._isLoa;
			}
		}

		public bool IsGardener
		{
			get
			{
				return this._isGardener;
			}
		}

		public Sphere ShipSphere
		{
			get
			{
				return this._boundingSphere;
			}
		}

		public List<SpecWeaponControl> WeaponControls
		{
			get
			{
				return this._weaponControls;
			}
		}

		public bool WeaponControlsIsInitilized
		{
			get
			{
				return this._weaponControls != null;
			}
		}

		public void InitializeWeaponControls()
		{
			this._weaponControls = new List<SpecWeaponControl>();
		}

		public bool IsCarrier
		{
			get
			{
				return this.BattleRiderSquads.Count<BattleRiderSquad>() > 0;
			}
		}

		public bool IsBattleRider
		{
			get
			{
				return this is BattleRiderShip;
			}
		}

		public bool IsWraithAbductor
		{
			get
			{
				return this is WraithAbductorShip;
			}
		}

		public Faction Faction
		{
			get
			{
				return this._faction;
			}
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
				Section missionSection = this.MissionSection;
				CompoundCollisionShape compoundCollisionShape = this.CompoundCollisionShape;
				foreach (Section section in this.Sections)
				{
					if (section != missionSection)
						section.PostSetParent((IGameObject)missionSection, section.GetTag<ShipSectionAsset>().Type.ToString(), section.GetTag<ShipSectionAsset>().Type.ToString());
				}
				foreach (CollisionShape sectionCollisionShape in this.SectionCollisionShapes)
				{
					CollisionShape sectionShape = sectionCollisionShape;
					Section state1 = this.Sections.First<Section>((Func<Section, bool>)(x => x.GetTag<ShipSectionAsset>() == sectionShape.GetTag<ShipSectionAsset>()));
					sectionShape.PostSetAggregate((IGameObject)state1);
					foreach (IGameObject state2 in this.CollisionShapes.Where<CollisionShape>((Func<CollisionShape, bool>)(x => x.GetTag<CollisionShape>() == sectionShape)))
						state2.PostSetAggregate((IGameObject)state1);
					if (state1 == missionSection)
						sectionShape.PostAttach((IGameObject)compoundCollisionShape);
					else
						sectionShape.PostAttach((IGameObject)state1, (IGameObject)compoundCollisionShape, (IGameObject)state1, state1.GetTag<ShipSectionAsset>().Type.ToString(), (IGameObject)missionSection, state1.GetTag<ShipSectionAsset>().Type.ToString());
				}
				foreach (GenericCollisionShape turretShape in this.TurretShapes)
				{
					Turret tag = turretShape.GetTag<Turret>();
					LogicalMount mount = tag.GetTag<LogicalMount>();
					IGameObject socket2 = (IGameObject)null;
					if (mount.Bank.Section != null)
						socket2 = (IGameObject)this.Sections.First<Section>((Func<Section, bool>)(x => x.GetTag<ShipSectionAsset>() == mount.Bank.Section));
					if (mount.Bank.Module != null)
						socket2 = (IGameObject)this.Modules.First<Module>((Func<Module, bool>)(x => x.GetTag<LogicalModule>() == mount.Bank.Module));
					if (socket2 != null)
					{
						turretShape.PostSetAggregate((IGameObject)tag);
						turretShape.PostAttach((IGameObject)tag, (IGameObject)compoundCollisionShape, (IGameObject)tag, "", socket2, mount.NodeName);
					}
				}
				foreach (CollisionShape moduleCollisionShape in this.ModuleCollisionShapes)
				{
					CollisionShape moduleShape = moduleCollisionShape;
					Module module = moduleShape.GetTag<Module>();
					Section section = this.Sections.First<Section>((Func<Section, bool>)(x => x.GetTag<ShipSectionAsset>() == module.Attachment.Section));
					moduleShape.PostSetAggregate((IGameObject)module);
					foreach (IGameObject state in this.CollisionShapes.Where<CollisionShape>((Func<CollisionShape, bool>)(x => x.GetTag<CollisionShape>() == moduleShape)))
						state.PostSetAggregate((IGameObject)module);
					moduleShape.PostAttach((IGameObject)module, (IGameObject)compoundCollisionShape, (IGameObject)module, "", (IGameObject)section, module.Attachment.NodeName);
				}
				foreach (MountObject mountObject in this.MountObjects)
				{
					MountObject mo = mountObject;
					IGameObject parent = this._objects.First<IGameObject>((Func<IGameObject, bool>)(x => x.ObjectID == mo.ParentID));
					mo.PostSetParent(parent, mo.NodeName);
				}
				foreach (TurretBase turretBase in this.TurretBases)
				{
					LogicalMount mount = turretBase.GetTag<Turret>().GetTag<LogicalMount>();
					IGameObject parent = (IGameObject)null;
					if (mount.Bank.Section != null)
						parent = (IGameObject)this.Sections.First<Section>((Func<Section, bool>)(x => x.GetTag<ShipSectionAsset>() == mount.Bank.Section));
					if (mount.Bank.Module != null)
						parent = (IGameObject)this.Modules.First<Module>((Func<Module, bool>)(x => x.GetTag<LogicalModule>() == mount.Bank.Module));
					if (parent != null)
						turretBase.PostSetParent(parent, mount.NodeName);
				}
				foreach (Module module1 in this.Modules)
				{
					Module module = module1;
					Section section = this.Sections.First<Section>((Func<Section, bool>)(x => x.GetTag<ShipSectionAsset>() == module.Attachment.Section));
					module.PostSetParent((IGameObject)section, module.Attachment.NodeName);
				}
				compoundCollisionShape.PostAttach((IGameObject)this.RigidBody);
				missionSection.PostSetParent((IGameObject)this.RigidBody);
				this.Maneuvering.PostAttach((IGameObject)this.RigidBody);
				IGameObject[] gameObjectArray;
				if (this.Shield != null)
					gameObjectArray = new IGameObject[4]
					{
			(IGameObject) this.RigidBody,
			(IGameObject) this.Maneuvering,
			(IGameObject) this.CompoundCollisionShape,
			(IGameObject) this.Shield
					};
				else
					gameObjectArray = new IGameObject[3]
					{
			(IGameObject) this.RigidBody,
			(IGameObject) this.Maneuvering,
			(IGameObject) this.CompoundCollisionShape
					};
				this.PostObjectAddObjects(((IEnumerable<IGameObject>)gameObjectArray).Concat<IGameObject>((IEnumerable<IGameObject>)this.Sections).Concat<IGameObject>((IEnumerable<IGameObject>)this.Modules).Concat<IGameObject>((IEnumerable<IGameObject>)this.WeaponBanks).Concat<IGameObject>((IEnumerable<IGameObject>)this.MountObjects).Concat<IGameObject>((IEnumerable<IGameObject>)this.TurretBases).Concat<IGameObject>((IEnumerable<IGameObject>)this.BattleRiderSquads).Concat<IGameObject>((IEnumerable<IGameObject>)this.Psionics).Concat<IGameObject>((IEnumerable<IGameObject>)this.AttachableEffects).ToArray<IGameObject>());
				this.RigidBody.PostSetAggregate((IGameObject)this);
				compoundCollisionShape.PostSetProp("SetPairedObject", (object)compoundCollisionShape.ObjectID, (object)this.ObjectID);
				this._bIsPlanetAssaultShip = this.WeaponBanks.Any<WeaponBank>((Func<WeaponBank, bool>)(x => WeaponEnums.IsPlanetAssaultWeapon(x.TurretClass)));
			}
			return GameObjectStatus.Ready;
		}

		public static bool IsShipClassBigger(ShipClass sc1, ShipClass sc2, bool sameSizeIsTrue = false)
		{
			if (sc1 == sc2)
				return sameSizeIsTrue;
			bool flag = false;
			switch (sc2)
			{
				case ShipClass.Cruiser:
					flag = sc1 != ShipClass.BattleRider;
					break;
				case ShipClass.Dreadnought:
					flag = sc1 == ShipClass.Leviathan;
					break;
				case ShipClass.BattleRider:
					flag = true;
					break;
			}
			return flag;
		}

		public static bool IsStationSize(RealShipClasses rsc)
		{
			switch (rsc)
			{
				case RealShipClasses.Station:
				case RealShipClasses.Platform:
					return true;
				default:
					return false;
			}
		}

		public static bool IsBattleRiderSize(RealShipClasses rsc)
		{
			switch (rsc)
			{
				case RealShipClasses.BattleRider:
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

		public static Ship CreateShip(
		  GameSession game,
		  Matrix world,
		  ShipInfo shipInfo,
		  int parentId,
		  int inputId,
		  int playerId = 0,
		  bool isInDeepSpace = false,
		  IEnumerable<Player> playersInCombat = null)
		{
			DesignInfo design = shipInfo.DesignInfo ?? game.GameDatabase.GetDesignInfo(shipInfo.DesignID);
			Player player = playerId == 0 ? game.GetPlayerObject(design.PlayerID) : game.App.GetGameObject(playerId) as Player;
			return Ship.CreateShip(game.App, world, design, shipInfo.ShipName, shipInfo.SerialNumber, parentId, inputId, player, shipInfo.ID, shipInfo.RiderIndex, shipInfo.ParentID, true, false, isInDeepSpace, playersInCombat);
		}

		public static Ship CreateShip(
		  App game,
		  Matrix world,
		  DesignInfo design,
		  string shipName,
		  int serialNumber,
		  int parentId,
		  int inputId,
		  Player player,
		  int shipInfoID = 0,
		  int riderindex = -1,
		  int parentDBID = -1,
		  bool autoAddDrawable = true,
		  bool defenseBoatActive = false,
		  bool isInDeepSpace = false,
		  IEnumerable<Player> playersInCombat = null)
		{
			IEnumerable<string> source = ((IEnumerable<DesignSectionInfo>)design.DesignSections).Select<DesignSectionInfo, string>((Func<DesignSectionInfo, string>)(x => x.FilePath));
			IEnumerable<string> weapons = game.AssetDatabase.Weapons.Select<LogicalWeapon, string>((Func<LogicalWeapon, string>)(x => x.Name));
			IEnumerable<string> modules = game.AssetDatabase.Modules.Select<LogicalModule, string>((Func<LogicalModule, string>)(x => x.ModuleName));
			List<WeaponAssignment> weaponAssignmentList = new List<WeaponAssignment>();
			List<SectionInstanceInfo> sectionInstanceInfoList = new List<SectionInstanceInfo>();
			List<ModuleAssignment> moduleAssignmentList = new List<ModuleAssignment>();
			if (shipInfoID != 0)
				sectionInstanceInfoList = game.GameDatabase.GetShipSectionInstances(shipInfoID).ToList<SectionInstanceInfo>();
			AssignedSectionTechs[] assignedSectionTechsArray = new AssignedSectionTechs[3];
			for (int index = 0; index < assignedSectionTechsArray.Length; ++index)
				assignedSectionTechsArray[index] = new AssignedSectionTechs();
			foreach (DesignSectionInfo designSection in design.DesignSections)
			{
				DesignSectionInfo sectionInfo = designSection;
				ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == sectionInfo.FilePath));
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
							Module = logicalModule,
							PsionicAbilities = designModuleInfo.PsionicAbilities != null ? designModuleInfo.PsionicAbilities.Select<ModulePsionicInfo, SectionEnumerations.PsionicAbility>((Func<ModulePsionicInfo, SectionEnumerations.PsionicAbility>)(x => x.Ability)).ToArray<SectionEnumerations.PsionicAbility>() : new SectionEnumerations.PsionicAbility[0]
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
				foreach (int tech in sectionInfo.Techs)
					assignedSectionTechsArray[(int)shipSectionAsset.Type].Techs.Add(tech);
			}
			ShipInfo shipInfo = game.GameDatabase.GetShipInfo(shipInfoID, false);
			IEnumerable<ShipSectionAsset> vSections = source.Select<string, ShipSectionAsset>((Func<string, ShipSectionAsset>)(x => game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(y => y.FileName == x))));
			CreateShipParams createShipParams = new CreateShipParams();
			createShipParams.AutoAddDrawable = autoAddDrawable;
			createShipParams.player = player;
			if (playersInCombat != null)
				createShipParams.playersInCombat = playersInCombat.ToList<Player>();
			createShipParams.sections = vSections;
			createShipParams.sectionInstances = (IEnumerable<SectionInstanceInfo>)sectionInstanceInfoList;
			createShipParams.turretHousings = game.AssetDatabase.TurretHousings;
			createShipParams.weapons = game.AssetDatabase.Weapons;
			createShipParams.preferredWeapons = design.StationType == StationType.INVALID_TYPE ? game.AssetDatabase.Weapons.Where<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => weapons.Contains<string>(x.Name))) : game.AssetDatabase.Weapons.Where<LogicalWeapon>((Func<LogicalWeapon, bool>)(x =>
		  {
			  if (!weapons.Contains<string>(x.Name))
				  return false;
			  if ((double)x.Range <= 1500.0)
				  return x.DefaultWeaponSize == WeaponEnums.WeaponSizes.VeryLight;
			  return true;
		  }));
			createShipParams.assignedWeapons = (IEnumerable<WeaponAssignment>)weaponAssignmentList;
			createShipParams.modules = game.AssetDatabase.Modules;
			createShipParams.preferredModules = game.AssetDatabase.Modules.Where<LogicalModule>((Func<LogicalModule, bool>)(x => modules.Contains<string>(x.ModuleName)));
			createShipParams.assignedModules = (IEnumerable<ModuleAssignment>)moduleAssignmentList;
			createShipParams.psionics = game.AssetDatabase.Psionics;
			createShipParams.assignedTechs = assignedSectionTechsArray;
			createShipParams.faction = game.AssetDatabase.Factions.First<Faction>((Func<Faction, bool>)(x => vSections.First<ShipSectionAsset>().Faction == x.Name));
			createShipParams.shipName = shipName;
			createShipParams.shipDesignName = design != null ? design.Name : "";
			createShipParams.serialNumber = serialNumber;
			createShipParams.parentID = parentId;
			createShipParams.inputID = inputId;
			createShipParams.role = design.Role;
			createShipParams.wpnRole = design.WeaponRole;
			createShipParams.databaseId = shipInfoID;
			createShipParams.designId = design.ID;
			createShipParams.isKillable = true;
			createShipParams.enableAI = true;
			createShipParams.isInDeepSpace = isInDeepSpace;
			createShipParams.riderindex = riderindex;
			createShipParams.parentDBID = parentDBID;
			createShipParams.curPsiPower = shipInfo != null ? shipInfo.PsionicPower : 0;
			createShipParams.spawnMatrix = new Matrix?(world);
			createShipParams.defenceBoatIsActive = defenseBoatActive;
			createShipParams.priorityWeapon = design.PriorityWeaponName;
			createShipParams.obtainedSlaves = shipInfo != null ? shipInfo.SlavesObtained : 0.0;
			return Ship.CreateShip(game, createShipParams);
		}

		public static Ship CreateShip(App game, CreateShipParams createShipParams)
		{
			ShipSectionAsset shipSectionAsset = createShipParams.sections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(section => section.Type == ShipSectionType.Mission));
			if (shipSectionAsset.IsWraithAbductor)
				return (Ship)new WraithAbductorShip(game, createShipParams);
			if (shipSectionAsset.IsBattleRider)
				return (Ship)new BattleRiderShip(game, createShipParams);
			return new Ship(game, createShipParams);
		}

		public static WeaponModelPaths GetWeaponModelPathsWithFixAssetNameForDLC(
		  LogicalWeapon weapon,
		  Faction faction,
		  string preferredMount)
		{
			WeaponModelPaths weaponModelPaths = LogicalWeapon.GetWeaponModelPaths(weapon, faction);
			weaponModelPaths.ModelPath = Ship.FixAssetNameForDLC(weaponModelPaths.ModelPath, preferredMount);
			weaponModelPaths.DefaultModelPath = Ship.FixAssetNameForDLC(weaponModelPaths.ModelPath, preferredMount);
			return weaponModelPaths;
		}

		public static string GetPreferredMount(
		  App game,
		  Player player,
		  Faction faction,
		  List<ShipSectionAsset> sections)
		{
			Subfaction subfaction = faction.Subfactions[Math.Min(player.SubfactionIndex, faction.Subfactions.Length - 1)];
			string empty = string.Empty;
			string str = game.LocalPlayer != player || !subfaction.DlcID.HasValue || game.Steam.HasDLC((int)subfaction.DlcID.Value) ? subfaction.MountName : faction.Subfactions[0].MountName;
			foreach (ShipSectionAsset section in sections)
			{
				if (!ScriptHost.FileSystem.FileExists(string.Format("\\{0}\\{1}", (object)str, (object)section.ModelName) + "~"))
				{
					str = string.Empty;
					break;
				}
			}
			return str;
		}

		public static string FixAssetNameForDLC(string unbasedPath, string preferredMount)
		{
			if (string.IsNullOrEmpty(unbasedPath))
				return unbasedPath;
			string str1 = string.Format("\\{0}\\{1}", (object)preferredMount, (object)unbasedPath);
			if (ScriptHost.FileSystem.FileExists(str1 + "~"))
				return str1;
			string str2 = string.Format("\\eof\\{0}", (object)unbasedPath);
			if (ScriptHost.FileSystem.FileExists(str2 + "~"))
				return str2;
			return string.Format("\\base\\{0}", (object)unbasedPath);
		}

		private void Prepare(App game, CreateShipParams createShipParams)
		{
			string preferredMount = Ship.GetPreferredMount(game, createShipParams.player, createShipParams.faction, createShipParams.sections.ToList<ShipSectionAsset>());
			game.UI.GameEvent += this.UICommChannel_GameEvent;
			this._taskGroup = null;
			this._bIsDestroyed = false;
			this._bHasRetreated = false;
			this._bHitByNodeCannon = false;
			this._bIsDriveless = true;
			this._bCanAcceptMoveOrders = true;
			this._checkStatusBootstrapped = false;
			this._visible = true;
			this._bDockedWithParent = false;
			this._bCarrierCanLaunch = true;
			this._bIsPlanetAssaultShip = false;
			this._bAssaultingPlanet = false;
			this._bAuthoritive = true;
			this._isUnderAttack = false;
			this._instantlyKilled = false;
			this._isDeployed = false;
			this._priorityWeapon = createShipParams.priorityWeapon;
			this._sensorRange = 0f;
			this._faction = createShipParams.faction;
			this._databaseID = createShipParams.databaseId;
			this._player = createShipParams.player;
			this._bIsNeutronStar = (game.Game != null && game.Game.ScriptModules.NeutronStar != null && game.Game.ScriptModules.NeutronStar.PlayerID == this._player.ID);
			this._isGardener = (game.Game != null && game.Game.ScriptModules.Gardeners != null && game.Game.ScriptModules.Gardeners.GardenerDesignId == createShipParams.designId);
			this._wpnRole = createShipParams.wpnRole;
			this._inputId = createShipParams.inputID;
			this._boundingSphere = new Sphere(createShipParams.player.ObjectID, Vector3.Zero, 10f);
			this._reserveSize = (from x in createShipParams.sections
								 select x.ReserveSize).Sum();
			this._riderIndex = createShipParams.riderindex;
			this._parentID = createShipParams.parentID;
			this._parentDatabaseID = createShipParams.parentDBID;
			this._turretFiring = Turret.FiringEnum.NotFiring;
			this._blindFireActive = false;
			this._isLoa = (this._faction.Name == "loa");
			this._accuracyModifier = 0f;
			if (this._isLoa)
			{
				this._accuracyModifier = 0.25f;
			}
			if (createShipParams.sections.Any((ShipSectionAsset x) => x.IsFireControl))
			{
				this._accuracyModifier += 0.25f;
			}
			else if (createShipParams.sections.Any((ShipSectionAsset x) => x.IsAIControl))
			{
				this._accuracyModifier += 0.5f;
			}
			this._accuracyModifier = Math.Max(1f - this._accuracyModifier, 0f);
			this._currentTurretIndex = 0;
			this._currentTurretIndex = 0;
			this._shipClass = createShipParams.sections.First<ShipSectionAsset>().Class;
			this._realShipClass = createShipParams.sections.First<ShipSectionAsset>().RealClass;
			this._shipRole = createShipParams.role;
			this._isSystemDefenceBoat = false;
			this._defenseBoatActive = false;
			this._isPolice = false;
			this._suulkaType = SuulkaType.None;
			this._isQShip = false;
			this._isAcceleratorHoop = false;
			this._isListener = false;
			this._combatAI = SectionEnumerations.CombatAiType.Normal;
			this._abilityType = ShipFleetAbilityType.None;
			this._battleRiderType = BattleRiderTypes.Unspecified;
			string text = "Medium";
			bool flag = false;
			bool flag2 = false;
			int num = 0;
			foreach (ShipSectionAsset shipSectionAsset in createShipParams.sections)
			{
				if (!flag && shipSectionAsset.ManeuveringType != "")
				{
					text = shipSectionAsset.ManeuveringType;
					flag = true;
				}
				if (shipSectionAsset.Type == ShipSectionType.Engine || this._shipClass == ShipClass.BattleRider || this._shipClass == ShipClass.Leviathan)
				{
					this._bIsDriveless = false;
				}
				if (shipSectionAsset.Type == ShipSectionType.Mission)
				{
					this._combatAI = shipSectionAsset.CombatAIType;
					this._battleRiderType = shipSectionAsset.BattleRiderType;
					this._abilityType = shipSectionAsset.ShipFleetAbilityType;
					this._suulkaType = shipSectionAsset.SuulkaType;
					num = shipSectionAsset.StationLevel;
				}
				if (this._shipRole == ShipRole.FREIGHTER && shipSectionAsset.FileName.Contains("_qship"))
				{
					this._isQShip = true;
				}
				flag2 = (flag2 || shipSectionAsset.IsGravBoat);
				this._isAcceleratorHoop = (this._isAcceleratorHoop || shipSectionAsset.IsAccelerator);
				this._isListener = (this._isListener || shipSectionAsset.IsListener);
				this._isPolice = (this._isPolice || shipSectionAsset.isPolice);
				this._isSystemDefenceBoat = (this._isSystemDefenceBoat || shipSectionAsset.RealClass == RealShipClasses.SystemDefenseBoat);
			}
			this._isTrapDrone = (this._combatAI == SectionEnumerations.CombatAiType.TrapDrone);
			this._isPolicePatrolling = this._isPolice;
			if (createShipParams.sections.Count<ShipSectionAsset>() == 1)
			{
				this._bIsDriveless = false;
			}
			this._bCanAcceptMoveOrders = (this._shipClass != ShipClass.Station && (this._shipRole != ShipRole.FREIGHTER || this._isQShip) && !this._isTrapDrone && !this._isAcceleratorHoop && !this._isGardener);
			if (this._bCanAcceptMoveOrders && this._shipClass == ShipClass.BattleRider)
			{
				this._bCanAcceptMoveOrders = (createShipParams.parentID == 0);
			}
			if (!flag)
			{
				switch (this._shipClass)
				{
					case ShipClass.Cruiser:
						text = "Medium";
						break;
					case ShipClass.Dreadnought:
					case ShipClass.Leviathan:
					case ShipClass.Station:
						text = "Slow";
						break;
					case ShipClass.BattleRider:
						text = "Fast";
						break;
				}
			}
			this._bonusSpottedRange = 0f;
			if (num > 0)
			{
				if (num == 5)
				{
					this._bonusSpottedRange = -1f;
				}
				else
				{
					this._bonusSpottedRange += (float)(num - 1) * game.AssetDatabase.GlobalSpotterRangeData.StationLVLOffset;
				}
			}
			if (this._combatAI == SectionEnumerations.CombatAiType.Meteor)
			{
				this._bonusSpottedRange = -1f;
			}
			if (this._bonusSpottedRange == 0f)
			{
				this._bonusSpottedRange = game.AssetDatabase.GlobalSpotterRangeData.SpotterValues[(int)GlobalSpotterRangeData.GetTypeFromShipClass(this._shipClass)];
			}
			List<SectionInstanceInfo> list = createShipParams.sectionInstances.ToList<SectionInstanceInfo>();
			List<PlayerTechInfo> list2 = (this._player != null && game.GameDatabase != null && this._player.IsStandardPlayer) ? game.GameDatabase.GetPlayerTechInfos(this._player.ID).ToList<PlayerTechInfo>() : new List<PlayerTechInfo>();
			List<SectionEnumerations.DesignAttribute> attributes = new List<SectionEnumerations.DesignAttribute>();
			if (createShipParams.designId != 0)
			{
				attributes = game.GameDatabase.GetDesignAttributesForDesign(createShipParams.designId).ToList<SectionEnumerations.DesignAttribute>();
			}
			this._designID = createShipParams.designId;
			DesignInfo designInfo = null;
			ShipInfo shipInfo = null;
			FleetInfo fleetInfo = null;
			List<AdmiralInfo.TraitType> list3 = new List<AdmiralInfo.TraitType>();
			if (game.GameDatabase != null)
			{
				designInfo = game.GameDatabase.GetDesignInfo(this._designID);
				shipInfo = game.GameDatabase.GetShipInfo(createShipParams.databaseId, true);
				if (shipInfo != null)
				{
					fleetInfo = game.GameDatabase.GetFleetInfo(shipInfo.FleetID);
					if (fleetInfo != null)
					{
						list3 = game.GameDatabase.GetAdmiralTraits(fleetInfo.AdmiralID).ToList<AdmiralInfo.TraitType>();
					}
				}
			}
			createShipParams.faction.AddFactionReference(game);
			if (shipInfo != null && fleetInfo != null && designInfo != null)
			{
				this._maxPsiPower = ShipInfo.GetMaxPsionicPower(game, designInfo, game.GameDatabase.GetAdmiralTraits(fleetInfo.AdmiralID).ToList<AdmiralInfo.TraitType>());
			}
			else
			{
				this._maxPsiPower = (int)(createShipParams.faction.PsionicPowerPerCrew * (float)(from x in createShipParams.sections
																								 select x.Crew).Sum());
			}
			if (this.IsSuulka)
			{
				this._maxPsiPower = (int)(from x in createShipParams.sections
										  select x.PsionicPowerLevel).Sum();
				this._maxPsiPower = ((this._maxPsiPower > 0) ? this._maxPsiPower : 1000);
			}
			bool flag3 = list.Count == 0;
			this._currentPsiPower = Math.Min(createShipParams.curPsiPower, this._maxPsiPower);
			if (flag3)
			{
				this._currentPsiPower = this._maxPsiPower;
			}
			this._pdAccuracyModifier = Math.Max(1f - Player.GetPDAccuracyBonus(game.AssetDatabase, list2), 0f);
			int num2 = 0;
			List<string> allShipTechsIds = new List<string>();
			foreach (AssignedSectionTechs assignedSectionTechs in createShipParams.assignedTechs)
			{
				if (assignedSectionTechs != null)
				{
					foreach (int techId in assignedSectionTechs.Techs)
					{
						string techFileID = game.GameDatabase.GetTechFileID(techId);
						allShipTechsIds.Add(techFileID);
						if (techFileID == "IND_Stealth_Armor")
						{
							num2++;
						}
					}
				}
			}
			bool flag4 = createShipParams.isKillable && !this._bIsNeutronStar;
			bool hasStealthTech = num2 == createShipParams.sections.Count<ShipSectionAsset>();
			List<object> list4 = new List<object>();
			list4.Add(0);
			list4.Add((this._player != null) ? this._player.ObjectID : 0);
			list4.Add(createShipParams.inputID);
			list4.Add(createShipParams.AutoAddDrawable);
			list4.Add((!string.IsNullOrEmpty(createShipParams.shipName)) ? createShipParams.shipName.ToUpper() : "");
			list4.Add(createShipParams.serialNumber.ToString());
			list4.Add(createShipParams.shipDesignName);
			list4.Add(this._designID);
			list4.Add(this._databaseID);
			list4.Add((int)this._shipClass);
			list4.Add((int)this._shipRole);
			list4.Add((this._faction.FactionObj != null) ? this._faction.FactionObj.ObjectID : 0);
			list4.Add(this._suulkaType);
			list4.Add(this._isLoa);
			list4.Add(this._isAcceleratorHoop);
			list4.Add(this._isQShip);
			list4.Add(this._realShipClass == RealShipClasses.Platform);
			list4.Add(this._bIsNeutronStar);
			list4.Add(this._isGardener);
			list4.Add(flag2);
			list4.Add(this._isPolice);
			list4.Add(flag4);
			list4.Add(this._player.PlayerInfo.AutoUseGoopModules);
			list4.Add(this._player.PlayerInfo.AutoUseJokerModules);
			list4.Add(this._player.PlayerInfo.AutoAoe);
			list4.Add(createShipParams.sections.FirstOrDefault<ShipSectionAsset>().StationLevel);
			list4.Add(createShipParams.enableAI);
			list4.Add(this.CombatAI);
			list4.Add(game.AssetDatabase.ShipEMPEffect.Name);
			list4.Add(this._bonusSpottedRange);
			list4.Add(this._reserveSize);
			list4.Add((from x in createShipParams.sections
					   select x.SlaveCapacity).Sum());
			list4.Add(createShipParams.obtainedSlaves);
			list4.Add(this._maxPsiPower);
			list4.Add(this._currentPsiPower);
			list4.Add((from x in createShipParams.sections
					   select x.ShipExplosiveDamage).Sum());
			list4.Add((from x in createShipParams.sections
					   select x.ShipExplosiveRange).Max());
			list4.Add(this.GetBaseSignature(game, createShipParams.sections.ToList<ShipSectionAsset>(), hasStealthTech));
			list4.Add(this.CalcShipCritModifier(attributes, 1f));
			list4.Add(this.CalcRepairCritModifier(attributes, list3, 1f));
			list4.Add(this.CalcCrewDeathFromStructureModifier(attributes, 0));
			list4.Add(this.CalcCrewDeathFromBoardingModifier(attributes, 0));
			list4.Add(Ship.GetBioMissileBonusModifier(game, this, createShipParams.sections.First((ShipSectionAsset x) => x.Type == ShipSectionType.Mission)));
			list4.Add(Ship.GetElectricEffectModifier(game.AssetDatabase, allShipTechsIds));
			list4.Add(Ship.HasAbsorberTech(createShipParams.sections.ToList<ShipSectionAsset>(), allShipTechsIds));
			list4.Add(Ship.GetPsiResistanceFromTech(game.AssetDatabase, allShipTechsIds));
			List<object> list5 = list4;
			bool flag5;
			if (Player.HasNodeDriveTech(list2))
			{
				flag5 = createShipParams.sections.Any((ShipSectionAsset x) => x.NodeSpeed > 0f);
			}
			else
			{
				flag5 = false;
			}
			list5.Add(flag5);
			float num3 = 0f;
			if (this._shipClass == ShipClass.Cruiser || this._shipClass == ShipClass.Dreadnought)
			{
				num3 = Player.GetSubversionRange(game.AssetDatabase, list2, this._isLoa);
			}
			list4.Add(num3);
			if (num3 > 0f)
			{
				if (this._isLoa)
				{
					list4.Add(game.AssetDatabase.GetTechBonus<float>("PSI_Subversion", "missilechanceL"));
					list4.Add(game.AssetDatabase.GetTechBonus<float>("PSI_Subversion", "dronechanceL"));
				}
				else
				{
					list4.Add(game.AssetDatabase.GetTechBonus<float>("PSI_Subversion", "missilechanceN"));
					list4.Add(game.AssetDatabase.GetTechBonus<float>("PSI_Subversion", "dronechanceN"));
				}
			}
			list4.Add(Player.HasWarpPulseTech(list2) && this._shipClass != ShipClass.BattleRider);
			list4.Add(this.GetPlanetDamageBonusFromAdmiralTraits(fleetInfo, list3));
			list4.Add(this.GetBaseROFBonusFromAdmiralTraits(game.GameDatabase, fleetInfo, list3, createShipParams.playersInCombat));
			list4.Add(this.GetInStandOffROFBonusFromAdmiralTraits(fleetInfo, list3));
			list4.Add(this._isSystemDefenceBoat);
			if (this._isSystemDefenceBoat)
			{
				this._defenseBoatActive = createShipParams.defenceBoatIsActive;
				list4.Add(createShipParams.defenceBoatIsActive);
				float num4 = 0f;
				Vector3 vector = Vector3.Zero;
				OrbitalObjectInfo orbitalObjectInfo = game.GameDatabase.GetOrbitalObjectInfo(createShipParams.defenceBoatOrbitalID);
				if (orbitalObjectInfo != null)
				{
					vector = game.GameDatabase.GetOrbitalTransform(orbitalObjectInfo.ID).Position;
					PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(orbitalObjectInfo.ID);
					if (planetInfo != null)
					{
						num4 = game.AssetDatabase.PlanetGenerationRules.GetStellarBodyParams(base.App.Game, planetInfo.ID).Radius + 375f;
					}
				}
				else if (createShipParams.spawnMatrix != null)
				{
					vector = createShipParams.spawnMatrix.Value.Position;
				}
				list4.Add(num4);
				list4.Add(vector);
			}
			if (this.IsWraithAbductor || this.IsBattleRider)
			{
				ShipSectionAsset shipSectionAsset2 = createShipParams.sections.FirstOrDefault((ShipSectionAsset x) => x.Type == ShipSectionType.Mission);
				float num5 = (from x in createShipParams.sections
							  select x.MissionTime).Sum();
				PlayerTechInfo playerTechInfo = game.GameDatabase.GetPlayerTechInfos(this._player.ID).FirstOrDefault((PlayerTechInfo x) => x.TechFileID == "NRG_SWS_Systems");
				if (playerTechInfo != null && playerTechInfo.State == TechStates.Researched)
				{
					num5 += num5 * game.AssetDatabase.GetTechBonus<float>(playerTechInfo.TechFileID, "ridertimebonus");
				}
				list4.Add((num5 > 0f) ? num5 : 30f);
				if (this.IsBattleRider)
				{
					Ship gameObject = game.GetGameObject<Ship>(createShipParams.parentID);
					BattleRiderSquad battleRiderSquad = (gameObject != null) ? gameObject.AssignRiderToSquad(this as BattleRiderShip, this._riderIndex) : null;
					list4.Add(this._riderIndex);
					list4.Add((battleRiderSquad != null) ? battleRiderSquad.ObjectID : 0);
					list4.Add((int)((shipSectionAsset2 != null) ? shipSectionAsset2.BattleRiderType : BattleRiderTypes.Unspecified));
					list4.Add((shipSectionAsset2.BattleRiderType == BattleRiderTypes.boardingpod) ? game.GameDatabase.GetStratModifier<float>(StratModifiers.BoardingPartyModifier, this._player.ID) : 1f);
				}
			}
			game.AddExistingObject(this, list4.ToArray());
			string text2 = this.Faction.Name;
			if (text2 == "liir_zuul")
			{
				if (this._realShipClass == RealShipClasses.BattleCruiser || this._realShipClass == RealShipClasses.BattleShip || this._realShipClass == RealShipClasses.SystemDefenseBoat)
				{
					text2 = "zuul";
				}
				else
				{
					text2 = "liir";
				}
			}
			ShipSpeedModifiers shipSpeedModifiers = Player.GetShipSpeedModifiers(game.AssetDatabase, this._player, this._realShipClass, list2, createShipParams.isInDeepSpace);
			float num6 = (from x in createShipParams.sections
						  select x.Maneuvering.Deacceleration).Sum();
			float num7 = Math.Max(this.CalcTopSpeed(attributes, (from x in createShipParams.sections
																 select x.Maneuvering.LinearSpeed).Sum()), 0f);
			App app = base.App;
			object[] array = new object[17];
			array[0] = base.ObjectID;
			array[1] = Math.Max(this.CalcAccel(attributes, (from x in createShipParams.sections
															select x.Maneuvering.LinearAccel).Sum()), 0f) * shipSpeedModifiers.LinearAccelModifier;
			array[2] = Math.Max(this.CalcTurnThrust(attributes, (from x in createShipParams.sections
																 select x.Maneuvering.RotAccel.X).Sum()), 0f) * shipSpeedModifiers.RotAccelModifier;
			array[3] = Math.Max(this.CalcTurnThrust(attributes, (from x in createShipParams.sections
																 select x.Maneuvering.RotAccel.Y).Sum()), 0f) * shipSpeedModifiers.RotAccelModifier;
			array[4] = Math.Max(this.CalcTurnThrust(attributes, (from x in createShipParams.sections
																 select x.Maneuvering.RotAccel.Z).Sum()), 0f) * shipSpeedModifiers.RotAccelModifier;
			array[5] = num7 * shipSpeedModifiers.SpeedModifier;
			array[6] = Math.Max(this.CalcTurnSpeed(attributes, (from x in createShipParams.sections
																select x.Maneuvering.RotationSpeed).Sum()), 0f) * shipSpeedModifiers.RotSpeedModifier;
			array[7] = ((num6 >= 1f) ? num6 : 2f);
			array[8] = 0f;
			array[9] = 0f;
			array[10] = 0f;
			array[11] = 0f;
			array[12] = this.GetInPursueSpeedBonusFromAdmiralTraits(fleetInfo, list3);
			array[13] = this.CanAcceptMoveOrders;
			array[14] = text;
			array[15] = text2;
			array[16] = createShipParams.inputID;
			ShipManeuvering shipManeuvering = app.AddObject<ShipManeuvering>(array);
			shipManeuvering.MaxShipSpeed = num7;
			this.AddObject(shipManeuvering);
			CompoundCollisionShape value = base.App.AddObject<CompoundCollisionShape>(new object[0]);
			this.AddObject(value);
			LogicalEffect turretEffect = new LogicalEffect
			{
				Name = "effects\\Weapons\\NuclearMissile_Impact.effect"
			};
			float kineticDampeningValue = Player.GetKineticDampeningValue(game.AssetDatabase, list2);
			bool flag6 = (from x in createShipParams.sections
						  select x.StrategicSensorRange).Sum() == 0f;
			bool flag7 = (from x in createShipParams.sections
						  select x.TacticalSensorRange).Sum() == 0f;
			int num8 = 0;
			using (IEnumerator<ShipSectionAsset> enumerator = createShipParams.sections.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ShipSectionAsset section = enumerator.Current;
					List<string> list6 = new List<string>();
					if (game.GameDatabase != null && createShipParams.assignedTechs[(int)section.Type] != null)
					{
						foreach (int techId2 in createShipParams.assignedTechs[(int)section.Type].Techs)
						{
							list6.Add(game.GameDatabase.GetTechFileID(techId2));
						}
					}
					int armorBonusFromTech = Ship.GetArmorBonusFromTech(game.AssetDatabase, list6);
					int num9 = Ship.GetPermArmorBonusFromTech(game.AssetDatabase, list6);
					if (game.GameDatabase != null)
					{
						num9 += ((this._realShipClass != RealShipClasses.AssaultShuttle && this._realShipClass != RealShipClasses.Biomissile && this._realShipClass != RealShipClasses.BoardingPod && this._realShipClass != RealShipClasses.Drone && this._realShipClass != RealShipClasses.EscapePod && this._realShipClass != RealShipClasses.BattleCruiser && this._realShipClass != RealShipClasses.BattleRider && this._realShipClass != RealShipClasses.BattleShip) ? game.GameDatabase.GetStratModifier<int>(StratModifiers.PhaseDislocationARBonus, createShipParams.player.ID) : 0);
					}
					CollisionShape collisionShape = base.App.AddObject<CollisionShape>(new object[]
					{
						PathHelpers.Combine(new string[]
						{
							Path.GetDirectoryName(section.ModelName),
							Path.GetFileNameWithoutExtension(section.ModelName) + "_convex.obj"
						})
					});
					collisionShape.SetTag(section);
					this.AddObject(collisionShape);
					CollisionShape collisionShape2 = string.IsNullOrEmpty(section.DamagedModelName) ? null : base.App.AddObject<CollisionShape>(new object[]
					{
						PathHelpers.Combine(new string[]
						{
							Path.GetDirectoryName(section.DamagedModelName),
							Path.GetFileNameWithoutExtension(section.DamagedModelName) + "_convex.obj"
						})
					});
					if (collisionShape2 != null)
					{
						collisionShape2.SetTag(collisionShape);
						this.AddObject(collisionShape2);
					}
					string text3 = (!string.IsNullOrEmpty(section.DestroyedModelName)) ? PathHelpers.Combine(new string[]
					{
						Path.GetDirectoryName(section.DestroyedModelName),
						Path.GetFileNameWithoutExtension(section.DestroyedModelName) + "_convex.obj"
					}) : string.Empty;
					CollisionShape collisionShape3 = base.App.AddObject<CollisionShape>(new object[]
					{
						text3,
						section.Type.ToString(),
						section.Type.ToString()
					});
					collisionShape3.SetTag(collisionShape);
					this.AddObject(collisionShape3);
					float num10 = this.CalcScannerRange(attributes, section.TacticalSensorRange);
					float num11 = section.StrategicSensorRange;
					if (section.Type == ShipSectionType.Mission)
					{
						if (flag7)
						{
							num10 = ((this._shipClass == ShipClass.BattleRider) ? game.AssetDatabase.DefaultBRTacSensorRange : game.AssetDatabase.DefaultTacSensorRange);
						}
						if (flag6)
						{
							num11 = ((this._shipClass == ShipClass.BattleRider) ? 0f : game.AssetDatabase.DefaultStratSensorRange);
						}
					}
					this._sensorRange = Math.Max(num10, this._sensorRange);
					int supplyWithTech = Ship.GetSupplyWithTech(game.AssetDatabase, list6, section.Supply);
					int powerWithTech = Ship.GetPowerWithTech(game.AssetDatabase, list6, list2, section.Power);
					int structureWithTech = Ship.GetStructureWithTech(game.AssetDatabase, list6, section.Structure);
					SectionInstanceInfo sectionInstanceInfo = null;
					if (designInfo != null)
					{
						DesignSectionInfo dsi = designInfo.DesignSections.FirstOrDefault((DesignSectionInfo x) => x.ShipSectionAsset == section);
						if (dsi != null)
						{
							sectionInstanceInfo = ((list.Count > 0) ? list.FirstOrDefault((SectionInstanceInfo x) => x.SectionID == dsi.ID) : null);
						}
					}
					List<object> list7 = new List<object>();
					list7.Add(Ship.FixAssetNameForDLC(section.ModelName, preferredMount));
					list7.Add(collisionShape.ObjectID);
					list7.Add(section.DamagedModelName ?? string.Empty);
					list7.Add((collisionShape2 != null) ? collisionShape2.ObjectID : 0);
					list7.Add(section.DestroyedModelName);
					list7.Add(collisionShape3.ObjectID);
					LogicalShipSpark[] array2 = base.App.AssetDatabase.ShipSparks.ToArray<LogicalShipSpark>();
					int num12 = array2.Length;
					list7.Add(num12);
					for (int j = 0; j < num12; j++)
					{
						list7.Add(array2[j].Type);
						list7.Add(array2[j].SparkEffect.Name);
					}
					list7.Add(section.Type.ToString());
					list7.Add(section.Type.ToString());
					list7.Add(section.SectionName ?? string.Empty);
					list7.Add(section.Type);
					list7.Add(section.Mass);
					list7.Add(section.AmbientSound);
					list7.Add(section.EngineSound);
					list7.Add(section.UnderAttackSound);
					list7.Add(section.DestroyedSound);
					list7.Add(structureWithTech);
					list7.Add(section.LowStruct);
					list7.Add(section.Crew);
					list7.Add(section.CrewRequired);
					list7.Add(powerWithTech);
					list7.Add(supplyWithTech);
					list7.Add(section.ProductionCost);
					list7.Add(section.ECM);
					list7.Add(section.ECCM);
					list7.Add(this.CalcSignature(attributes, section.Signature));
					list7.Add(num10);
					list7.Add(num11);
					list7.Add(section.isDeepScan);
					list7.Add(section.hasJammer);
					list7.Add(this.GetCloakingType(list6, section.cloakingType));
					list7.Add((sectionInstanceInfo != null) ? Math.Min(sectionInstanceInfo.Structure, structureWithTech) : structureWithTech);
					list7.Add((sectionInstanceInfo != null) ? sectionInstanceInfo.Crew : section.Crew);
					list7.Add((sectionInstanceInfo != null) ? sectionInstanceInfo.Supply : supplyWithTech);
					list7.Add(Ship.GetRichocetModifier(game.AssetDatabase, list6));
					list7.Add(Ship.GetBeamReflectModifier(game.AssetDatabase, list6));
					list7.Add(Ship.GetLaserReflectModifier(game.AssetDatabase, list6));
					list7.Add(kineticDampeningValue);
					list7.Add(base.ObjectID);
					list7.Add(section.DeathEffect.Name ?? string.Empty);
					list7.Add(section.ReactorFailureDeathEffect.Name ?? string.Empty);
					list7.Add(section.ReactorCriticalDeathEffect.Name ?? string.Empty);
					list7.Add(section.AbsorbedDeathEffect.Name ?? string.Empty);
					list7.Add(section.GetExtraArmorLayers() + num9);
					for (int k = 0; k < 4; k++)
					{
						int num13 = Ship.CalcArmorWidthModifier(attributes, 0) + armorBonusFromTech;
						int num14 = section.Armor[k].Y + num13;
						if (sectionInstanceInfo != null && sectionInstanceInfo.Armor.ContainsKey((ArmorSide)k) && sectionInstanceInfo.Armor[(ArmorSide)k].Height == num14)
						{
							list7.Add(sectionInstanceInfo.Armor[(ArmorSide)k]);
						}
						else
						{
							list7.Add(section.CreateFreshArmor((ArmorSide)k, num13));
						}
					}
					Section section4 = base.App.AddObject<Section>(list7.ToArray());
					section4.SetTag(section);
					section4.ShipSectionAsset = section;
					this.AddObject(section4);
					List<WeaponInstanceInfo> source = (sectionInstanceInfo != null) ? game.GameDatabase.GetWeaponInstances(sectionInstanceInfo.ID).ToList<WeaponInstanceInfo>() : new List<WeaponInstanceInfo>();
					List<WeaponInstanceInfo> weaponIns = (from x in source
														  where x.ModuleInstanceID == null || x.ModuleInstanceID.Value == 0
														  select x).ToList<WeaponInstanceInfo>();
					for (int l = 0; l < section.Banks.Length; l++)
					{
						LogicalBank bank = section.Banks[l];
						this.CreateBankDetails(createShipParams.turretHousings, createShipParams.weapons, createShipParams.preferredWeapons, createShipParams.assignedWeapons, weaponIns, list2, createShipParams.faction, shipInfo, fleetInfo, preferredMount, turretEffect, section, section4, null, bank, flag3);
					}
					this.CreateBattleRiderSquads(section4, null);
					List<ModuleInstanceInfo> list8 = (sectionInstanceInfo != null) ? game.GameDatabase.GetModuleInstances(sectionInstanceInfo.ID).ToList<ModuleInstanceInfo>() : new List<ModuleInstanceInfo>();
					for (int m = 0; m < section.Modules.Length; m++)
					{
						LogicalModuleMount moduleMount = section.Modules[m];
						if (createShipParams.assignedModules != null)
						{
							LogicalModule logicalModule = null;
							ModuleAssignment moduleAssignment = createShipParams.assignedModules.FirstOrDefault((ModuleAssignment x) => x.ModuleMount == moduleMount);
							if (moduleAssignment != null)
							{
								logicalModule = moduleAssignment.Module;
							}
							if (logicalModule == null && (!string.IsNullOrEmpty(moduleMount.AssignedModuleName) || section.Class == ShipClass.Station))
							{
								logicalModule = LogicalModule.EnumerateModuleFits(createShipParams.preferredModules, section, m, false).FirstOrDefault<LogicalModule>();
							}
							if (logicalModule != null)
							{
								CollisionShape collisionShape4 = base.App.AddObject<CollisionShape>(new object[]
								{
									PathHelpers.Combine(new string[]
									{
										Path.GetDirectoryName(logicalModule.ModelPath),
										Path.GetFileNameWithoutExtension(logicalModule.ModelPath) + "_convex.obj"
									}),
									"",
									moduleMount.NodeName
								});
								this.AddObject(collisionShape4);
								string text4 = (!string.IsNullOrEmpty(logicalModule.LowStructModelPath)) ? PathHelpers.Combine(new string[]
								{
									Path.GetDirectoryName(logicalModule.LowStructModelPath),
									Path.GetFileNameWithoutExtension(logicalModule.LowStructModelPath) + "_convex.obj"
								}) : string.Empty;
								CollisionShape collisionShape5 = null;
								if (!string.IsNullOrEmpty(text4))
								{
									collisionShape5 = base.App.AddObject<CollisionShape>(new object[]
									{
										text4
									});
									this.AddObject(collisionShape5);
								}
								string text5 = (!string.IsNullOrEmpty(logicalModule.DeadModelPath)) ? PathHelpers.Combine(new string[]
								{
									Path.GetDirectoryName(logicalModule.DeadModelPath),
									Path.GetFileNameWithoutExtension(logicalModule.DeadModelPath) + "_convex.obj"
								}) : string.Empty;
								CollisionShape collisionShape6 = null;
								if (!string.IsNullOrEmpty(text5))
								{
									collisionShape6 = base.App.AddObject<CollisionShape>(new object[]
									{
										text5
									});
									this.AddObject(collisionShape6);
								}
								Module module = this.CreateModule(moduleMount, logicalModule, list8, preferredMount, this, section4, collisionShape4, collisionShape5, collisionShape6, flag4, this._combatAI == SectionEnumerations.CombatAiType.Comet);
								module.Attachment = moduleMount;
								module.LogicalModule = logicalModule;
								module.AttachedSection = section4;
								module.SetTag(logicalModule);
								collisionShape4.SetTag(module);
								if (collisionShape5 != null)
								{
									collisionShape5.SetTag(collisionShape4);
								}
								if (collisionShape6 != null)
								{
									collisionShape6.SetTag(collisionShape4);
								}
								this.AddObject(module);
								int modInstId = 0;
								ModuleInstanceInfo moduleInstanceInfo = list8.FirstOrDefault((ModuleInstanceInfo x) => x.ModuleNodeID == moduleMount.NodeName);
								if (moduleInstanceInfo != null)
								{
									modInstId = moduleInstanceInfo.ID;
								}
								List<WeaponInstanceInfo> weaponIns2 = (from x in source
																	   where x.ModuleInstanceID == modInstId
																	   select x).ToList<WeaponInstanceInfo>();
								for (int n = 0; n < logicalModule.Banks.Length; n++)
								{
									LogicalBank bank2 = logicalModule.Banks[n];
									this.CreateBankDetails(createShipParams.turretHousings, createShipParams.weapons, createShipParams.preferredWeapons, createShipParams.assignedWeapons, weaponIns2, list2, createShipParams.faction, shipInfo, fleetInfo, preferredMount, turretEffect, section, section4, module, bank2, flag3);
								}
								this.CreateBattleRiderSquads(section4, module);
							}
						}
					}
					num8++;
				}
			}
			List<SectionEnumerations.PsionicAbility> list9 = new List<SectionEnumerations.PsionicAbility>();
			foreach (SectionEnumerations.PsionicAbility[] array3 in from x in createShipParams.sections
																	select x.PsionicAbilities)
			{
				SectionEnumerations.PsionicAbility[] array4 = array3;
				for (int i = 0; i < array4.Length; i++)
				{
					SectionEnumerations.PsionicAbility psionic = array4[i];
					if (!list9.Any((SectionEnumerations.PsionicAbility x) => x == psionic))
					{
						list9.Add(psionic);
					}
				}
			}
			if (createShipParams.assignedModules != null)
			{
				foreach (ModuleAssignment moduleAssignment2 in createShipParams.assignedModules)
				{
					if (moduleAssignment2.PsionicAbilities != null)
					{
						SectionEnumerations.PsionicAbility[] array4 = moduleAssignment2.PsionicAbilities;
						for (int i = 0; i < array4.Length; i++)
						{
							SectionEnumerations.PsionicAbility psionic = array4[i];
							if (!list9.Any((SectionEnumerations.PsionicAbility x) => x == psionic))
							{
								list9.Add(psionic);
							}
						}
					}
				}
			}
			if (this.Modules != null)
			{
				if (this.Modules.Any((Module x) => x.LogicalModule.AbilityType == ModuleEnums.ModuleAbilities.AbaddonLaser))
				{
					list9.Add(SectionEnumerations.PsionicAbility.AbaddonLaser);
				}
			}
			if (createShipParams.addPsionics)
			{
				this.CreatePsionics(createShipParams.psionics, this, list9);
			}
			Section section2 = null;
			LogicalShield logicalShield = null;
			if (this._combatAI == SectionEnumerations.CombatAiType.VonNeumannDisc)
			{
				logicalShield = base.App.AssetDatabase.Shields.FirstOrDefault((LogicalShield x) => x.TechID == "SLD_Shield_Mk._IV");
				section2 = this.MissionSection;
			}
			else
			{
				logicalShield = base.App.AssetDatabase.Shields.FirstOrDefault((LogicalShield x) => allShipTechsIds.Contains(x.TechID));
				if (logicalShield == null)
				{
					using (IEnumerator<ShipSectionAsset> enumerator = createShipParams.sections.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							ShipSectionAsset ssa = enumerator.Current;
							logicalShield = base.App.AssetDatabase.Shields.FirstOrDefault((LogicalShield x) => ssa.ShipOptions.Any((string[] y) => y.Contains(x.TechID)));
							if (logicalShield != null)
							{
								break;
							}
						}
					}
				}
			}
			if (logicalShield == null)
			{
				section2 = this.Sections.FirstOrDefault((Section x) => x.ShipSectionAsset.Type == ShipSectionType.Command);
				if (section2 != null)
				{
					if (section2.ShipSectionAsset.Title.Contains("DISRUPTOR"))
					{
						logicalShield = base.App.AssetDatabase.Shields.FirstOrDefault((LogicalShield x) => x.TechID == "SLD_Disruptor_Shields");
					}
					else if (section2.ShipSectionAsset.Title.Contains("DEFLECTOR"))
					{
						logicalShield = base.App.AssetDatabase.Shields.FirstOrDefault((LogicalShield x) => x.TechID == "SLD_Deflector_Shields");
					}
				}
			}
			if (logicalShield != null)
			{
				if (section2 == null)
				{
					foreach (Section section3 in this.Sections)
					{
						foreach (string[] source2 in section3.ShipSectionAsset.ShipOptions)
						{
							if (source2.Contains(logicalShield.TechID))
							{
								section2 = section3;
								break;
							}
						}
						if (section2 != null)
						{
							break;
						}
					}
				}
				if (section2 != null)
				{
					Shield value2 = new Shield(game, this, logicalShield, section2, list2, true);
					this.AddObject(value2);
				}
			}
			App app2 = base.App;
			array = new object[2];
			array[0] = ((!flag4) ? 1000f : 1f) * (from x in createShipParams.sections
												  select x.Mass).Sum();
			array[1] = createShipParams.isKillable;
			RigidBody value3 = app2.AddObject<RigidBody>(array);
			this.AddObject(value3);
			if (createShipParams.spawnMatrix != null)
			{
				this.Position = createShipParams.spawnMatrix.Value.Position;
				this.Rotation = createShipParams.spawnMatrix.Value.EulerAngles;
				this.Maneuvering.Destination = this.Position;
			}
			if (!string.IsNullOrEmpty(this._priorityWeapon))
			{
				WeaponBank weaponBank = this.WeaponBanks.FirstOrDefault((WeaponBank x) => x.Weapon.WeaponName == this._priorityWeapon);
				if (weaponBank != null)
				{
					this.PostSetProp("SetPriorityWeapon", weaponBank.Weapon.GameObject.ObjectID);
				}
			}
			if (this.IsWraithAbductor)
			{
				WeaponBank weaponBank2 = this.WeaponBanks.FirstOrDefault((WeaponBank x) => x.TurretClass == WeaponEnums.TurretClasses.AssaultShuttle);
				if (weaponBank2 != null && weaponBank2.Weapon != null)
				{
					this.PostSetProp("SetShipWeapon", weaponBank2.Weapon.GameObject.ObjectID);
				}
			}
			else if (this._combatAI == SectionEnumerations.CombatAiType.LocustFighter)
			{
				LogicalWeapon logicalWeapon = base.App.AssetDatabase.Weapons.FirstOrDefault((LogicalWeapon x) => x.WeaponName == "AssaultLocustFighter");
				if (logicalWeapon != null)
				{
					logicalWeapon.AddGameObjectReference();
					this.PostSetProp("SetShipWeapon", logicalWeapon.GameObject.ObjectID);
				}
			}
			if (this._combatAI == SectionEnumerations.CombatAiType.VonNeumannDisc)
			{
				VonNeumannDiscTypes vonNeumannDiscTypes = VonNeumannDiscControl.DiscTypeFromMissionSection(this.MissionSection);
				if (vonNeumannDiscTypes == VonNeumannDiscTypes.EMPULSER)
				{
					List<object> list10 = new List<object>();
					list10.Add(base.ObjectID);
					list10.Add("effects\\Weapons\\EMP_Wave.effect");
					list10.Add(750f);
					list10.Add(true);
					EMPulsar value4 = base.App.AddObject<EMPulsar>(list10.ToArray());
					this.AddObject(value4);
					return;
				}
				if (vonNeumannDiscTypes == VonNeumannDiscTypes.SCREAMER)
				{
					List<object> list11 = new List<object>();
					list11.Add(base.ObjectID);
					list11.Add("");
					list11.Add(750f);
					WildWeasel value5 = base.App.AddObject<WildWeasel>(list11.ToArray());
					this.AddObject(value5);
				}
			}
		}
		private void CreateBankDetails(
	  IEnumerable<LogicalTurretHousing> turretHousings,
	  IEnumerable<LogicalWeapon> weapons,
	  IEnumerable<LogicalWeapon> preferredWeapons,
	  IEnumerable<WeaponAssignment> assignedWeapons,
	  IEnumerable<WeaponInstanceInfo> weaponIns,
	  List<PlayerTechInfo> playerTechs,
	  Faction faction,
	  ShipInfo ship,
	  FleetInfo fleet,
	  string preferredMount,
	  LogicalEffect turretEffect,
	  ShipSectionAsset section,
	  Section sectionObj,
	  Module module,
	  LogicalBank bank,
	  bool isTestMode)
		{
			if (WeaponEnums.IsBattleRider(bank.TurretClass))
			{
				if (module == null)
				{
					int objectId1 = this.ObjectID;
				}
				else
				{
					int objectId2 = module.ObjectID;
				}
				IEnumerable<LogicalMount> logicalMounts = module != null ? ((IEnumerable<LogicalMount>)module.LogicalModule.Mounts).Where<LogicalMount>((Func<LogicalMount, bool>)(x => x.Bank == bank)) : ((IEnumerable<LogicalMount>)section.Mounts).Where<LogicalMount>((Func<LogicalMount, bool>)(x => x.Bank == bank));
				int num = 0;
				int designID = 0;
				int targetFilter = 0;
				int fireMode = 0;
				string moduleNodeName = module != null ? module.Attachment.NodeName : "";
				WeaponBank weaponBank = (WeaponBank)null;
				LogicalWeapon weapon = Ship.SelectWeapon(section, bank, assignedWeapons, preferredWeapons, weapons, moduleNodeName, out designID, out targetFilter, out fireMode);
				bool flag = WeaponEnums.IsWeaponBattleRider(bank.TurretClass);
				if (weapon != null)
				{
					weapon.AddGameObjectReference();
					num = weapon.GameObject.ObjectID;
					if (flag)
					{
						weaponBank = new WeaponBank(this.App, (IGameObject)this, bank, module, weapon, Player.GetWeaponLevelFromTechs(weapon, playerTechs.ToList<PlayerTechInfo>()), designID, 0, 0, bank.TurretSize, bank.TurretClass);
						weaponBank.AddExistingObject(this.App);
						this.AddObject((IGameObject)weaponBank);
					}
				}
				MountObject.WeaponModels weaponModels = new MountObject.WeaponModels();
				weaponModels.FillOutModelFilesWithWeapon(weapon, faction, preferredMount, weapons);
				foreach (LogicalMount logicalMount in logicalMounts)
				{
					BattleRiderMount battleRiderMount = this.App.AddObject<BattleRiderMount>((object)num, (object)this.ObjectID, (object)(sectionObj != null ? sectionObj.ObjectID : 0), (object)(module != null ? module.ObjectID : 0), (object)(weaponBank != null ? weaponBank.ObjectID : 0), (object)weaponModels.WeaponModelPath.ModelPath, (object)weaponModels.WeaponModelPath.DefaultModelPath, (object)weaponModels.SubWeaponModelPath.ModelPath, (object)weaponModels.SubWeaponModelPath.DefaultModelPath, (object)weaponModels.SecondaryWeaponModelPath.ModelPath, (object)weaponModels.SecondaryWeaponModelPath.DefaultModelPath, (object)weaponModels.SecondarySubWeaponModelPath.ModelPath, (object)weaponModels.SecondarySubWeaponModelPath.DefaultModelPath, (object)logicalMount.NodeName, (object)bank.TurretClass);
					if (flag)
					{
						battleRiderMount.IsWeapon = true;
						battleRiderMount.DesignID = designID;
					}
					this.AddObject((IGameObject)battleRiderMount);
					battleRiderMount.ParentID = module != null ? module.ObjectID : sectionObj.ObjectID;
					battleRiderMount.SquadIndex = this.BattleRiderMounts.Count<BattleRiderMount>() - 1;
					battleRiderMount.NodeName = logicalMount.NodeName;
					battleRiderMount.WeaponBank = bank;
					battleRiderMount.BankIcon = weaponBank == null ? "" : weaponBank.Weapon.IconSpriteName;
					battleRiderMount.AssignedSection = sectionObj;
					battleRiderMount.AssignedModule = module;
				}
			}
			else if (this.Faction.Name == "slavers")
			{
				Faction faction1 = this.App.AssetDatabase.Factions.FirstOrDefault<Faction>((Func<Faction, bool>)(x => x.Name == "zuul"));
				this.CreateTurretsForBank(preferredMount, faction1, ship, fleet, section, sectionObj, assignedWeapons, preferredWeapons, weapons, turretHousings, weaponIns, playerTechs, turretEffect, module, bank, isTestMode);
			}
			else
				this.CreateTurretsForBank(preferredMount, faction, ship, fleet, section, sectionObj, assignedWeapons, preferredWeapons, weapons, turretHousings, weaponIns, playerTechs, turretEffect, module, bank, isTestMode);
		}

		private void CreateBattleRiderSquads(Section section, Module module)
		{
			int parentID = module != null ? module.ObjectID : section.ObjectID;
			List<BattleRiderMount> list1 = this.BattleRiderMounts.Where<BattleRiderMount>((Func<BattleRiderMount, bool>)(x => x.ParentID == parentID)).ToList<BattleRiderMount>();
			if (list1.Count == 0)
				return;
			list1.Sort((Comparison<BattleRiderMount>)((x, y) => x.SquadIndex.CompareTo(y.SquadIndex)));
			List<object> objectList1 = new List<object>();
			objectList1.Add((object)this.ObjectID);
			objectList1.Add((object)section.ObjectID);
			objectList1.Add((object)(module != null ? module.ObjectID : 0));
			if (module != null)
			{
				objectList1.Add((object)0.0f);
				objectList1.Add((object)0.0f);
			}
			else
			{
				objectList1.Add((object)section.ShipSectionAsset.LaunchDelay);
				objectList1.Add((object)section.ShipSectionAsset.DockingDelay);
			}
			objectList1.Add((object)3f);
			objectList1.Add((object)(this._realShipClass == RealShipClasses.Platform));
			objectList1.Add((object)list1.First<BattleRiderMount>().BankIcon);
			int num = this.BattleRiderSquads.Count<BattleRiderSquad>();
			if (module != null || this.OnlyOneSquad())
			{
				objectList1.Add((object)num);
				objectList1.Add((object)list1.Count);
				foreach (BattleRiderMount battleRiderMount in list1)
					objectList1.Add((object)battleRiderMount.ObjectID);
				BattleRiderSquad battleRiderSquad = this.App.AddObject<BattleRiderSquad>(objectList1.ToArray());
				this.AddObject((IGameObject)battleRiderSquad);
				battleRiderSquad.ParentID = parentID;
				battleRiderSquad.AttachedSection = section;
				battleRiderSquad.AttachedModule = module;
				battleRiderSquad.NumRiders = list1.Count;
				battleRiderSquad.Mounts.AddRange((IEnumerable<BattleRiderMount>)list1);
			}
			else
			{
				List<LogicalBank> logicalBankList = new List<LogicalBank>();
				foreach (BattleRiderMount battleRiderMount in list1)
				{
					if (!logicalBankList.Contains(battleRiderMount.WeaponBank))
						logicalBankList.Add(battleRiderMount.WeaponBank);
				}
				foreach (LogicalBank logicalBank in logicalBankList)
				{
					LogicalBank bank = logicalBank;
					List<BattleRiderMount> list2 = list1.Where<BattleRiderMount>((Func<BattleRiderMount, bool>)(x => x.WeaponBank == bank)).ToList<BattleRiderMount>();
					int numRidersPerSquad = BattleRiderSquad.GetNumRidersPerSquad(bank.TurretClass, this._shipClass, list2.Count);
					List<BattleRiderMount> battleRiderMountList = new List<BattleRiderMount>();
					foreach (BattleRiderMount battleRiderMount1 in list2)
					{
						battleRiderMountList.Add(battleRiderMount1);
						if (battleRiderMountList.Count >= numRidersPerSquad)
						{
							List<object> objectList2 = new List<object>();
							objectList2.AddRange((IEnumerable<object>)objectList1);
							objectList2.Add((object)num);
							objectList2.Add((object)numRidersPerSquad);
							foreach (BattleRiderMount battleRiderMount2 in battleRiderMountList)
								objectList2.Add((object)battleRiderMount2.ObjectID);
							BattleRiderSquad battleRiderSquad = this.App.AddObject<BattleRiderSquad>(objectList2.ToArray());
							this.AddObject((IGameObject)battleRiderSquad);
							battleRiderSquad.ParentID = parentID;
							battleRiderSquad.AttachedSection = section;
							battleRiderSquad.AttachedModule = module;
							battleRiderSquad.NumRiders = numRidersPerSquad;
							battleRiderSquad.Mounts.AddRange((IEnumerable<BattleRiderMount>)battleRiderMountList);
							++num;
							battleRiderMountList.Clear();
						}
					}
				}
			}
		}

		public void ExternallyAssignShieldToShip(App game, LogicalShield logShield)
		{
			if (this.Shield != null)
				return;
			List<PlayerTechInfo> playerTechs = this._player == null || game.GameDatabase == null ? new List<PlayerTechInfo>() : game.GameDatabase.GetPlayerTechInfos(this._player.ID).ToList<PlayerTechInfo>();
			this.AddObject((IGameObject)new Shield(this.App, this, logShield, this.MissionSection, playerTechs, false));
			this.PostObjectAddObjects((IGameObject)this.Shield);
		}

		protected Ship(App game, CreateShipParams createShipParams)
		{
			this.Prepare(game, createShipParams);
		}

		private void AddObject(IGameObject value)
		{
			this._objects.Add(value);
		}

		private void RemoveObject(IGameObject value)
		{
			this._objects.Remove(value);
		}

		private void RemoveObjects(List<IGameObject> objs)
		{
			foreach (IGameObject gameObject in objs)
			{
				this.App.CurrentState.RemoveGameObject(gameObject);
				this.RemoveObject(gameObject);
			}
		}

		public static float GetTurretHealth(WeaponEnums.WeaponSizes turretSize)
		{
			switch (turretSize)
			{
				case WeaponEnums.WeaponSizes.VeryLight:
					return 10f;
				case WeaponEnums.WeaponSizes.Light:
					return 20f;
				case WeaponEnums.WeaponSizes.Medium:
					return 30f;
				case WeaponEnums.WeaponSizes.Heavy:
					return 40f;
				case WeaponEnums.WeaponSizes.VeryHeavy:
					return 60f;
				case WeaponEnums.WeaponSizes.SuperHeavy:
					return 100f;
				default:
					return 10f;
			}
		}

		private static float GetTurretShapeRadius(WeaponEnums.WeaponSizes turretSize)
		{
			switch (turretSize)
			{
				case WeaponEnums.WeaponSizes.VeryLight:
					return 1f;
				case WeaponEnums.WeaponSizes.Light:
					return 2.5f;
				case WeaponEnums.WeaponSizes.Medium:
					return 5f;
				case WeaponEnums.WeaponSizes.Heavy:
					return 10f;
				case WeaponEnums.WeaponSizes.VeryHeavy:
					return 20f;
				case WeaponEnums.WeaponSizes.SuperHeavy:
					return 30f;
				default:
					return 1f;
			}
		}

		private static bool GetTurretNeedsCollision(
		  WeaponEnums.TurretClasses turretClass,
		  string turretModelName)
		{
			if (turretModelName.ToLower() == "turret_dummy")
				return false;
			switch (turretClass)
			{
				case WeaponEnums.TurretClasses.Standard:
				case WeaponEnums.TurretClasses.Missile:
					return true;
				default:
					return false;
			}
		}

		public static LogicalWeapon SelectWeapon(
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
			{
				logicalWeapon = weapons.First<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.PayloadType != WeaponEnums.PayloadTypes.BattleRider));
				App.Log.Warn(string.Format("No weapon found to match {0}, {1}! on [{2}] Picking an inappropriate default to keep the game from crashing.", (object)bank.TurretSize, (object)bank.TurretClass, (object)section.FileName), "design");
			}
			logicalWeapon.AddGameObjectReference();
			return logicalWeapon;
		}

		private float CalcTurnSpeed(
		  List<SectionEnumerations.DesignAttribute> attributes,
		  float value)
		{
			if (attributes.Count == 0)
				return value;
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Fast_In_The_Curves))
				value *= 1.1f;
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Nimble_Lil_Minx))
				value *= 1.1f;
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Bit_Of_A_Hog))
				value *= 0.9f;
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Muscle_Machine))
				value *= 0.8f;
			return value;
		}

		private float CalcTurnThrust(
		  List<SectionEnumerations.DesignAttribute> attributes,
		  float value)
		{
			if (attributes.Count == 0)
				return value;
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Nimble_Lil_Minx))
				value *= 1.2f;
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Bit_Of_A_Hog))
				value *= 0.9f;
			return value;
		}

		private float CalcTopSpeed(
		  List<SectionEnumerations.DesignAttribute> attributes,
		  float value)
		{
			if (attributes.Count == 0)
				return value;
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Muscle_Machine))
				value *= 1.1f;
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Ghost_Of_The_Hood))
				value *= 1.1f;
			return value;
		}

		private float CalcAccel(
		  List<SectionEnumerations.DesignAttribute> attributes,
		  float value)
		{
			if (attributes.Count == 0 || !attributes.Contains(SectionEnumerations.DesignAttribute.Muscle_Machine))
				return value;
			value *= 1.2f;
			return value;
		}

		private float CalcRateOfFire(
		  List<SectionEnumerations.DesignAttribute> attributes,
		  float value)
		{
			if (attributes.Count == 0)
				return value;
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Aces_And_Eights))
				value *= 0.85f;
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Ol_Yellow_Streak))
				value *= 1.1f;
			return value;
		}

		private float CalcSignature(
		  List<SectionEnumerations.DesignAttribute> attributes,
		  float value)
		{
			if (attributes.Count == 0)
				return value;
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Aces_And_Eights))
				value *= 1.2f;
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Ol_Yellow_Streak))
				value *= 0.8f;
			return value;
		}

		private float CalcScannerRange(
		  List<SectionEnumerations.DesignAttribute> attributes,
		  float value)
		{
			if (attributes.Count == 0)
				return value;
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Louis_And_Clark))
				value *= 1.1f;
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Four_Eyes))
				value *= 0.9f;
			return value;
		}

		private float CalcBallisticWeaponRange(
		  List<SectionEnumerations.DesignAttribute> attributes,
		  float value)
		{
			if (attributes.Count == 0 || !attributes.Contains(SectionEnumerations.DesignAttribute.Sniper))
				return value;
			value *= 1.25f;
			return value;
		}

		private float GetBaseSignature(App game, List<ShipSectionAsset> sections, bool hasStealthTech)
		{
			if (sections == null || sections.Count == 0)
				return 0.0f;
			float num = 0.0f;
			if (sections.Any<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.isDeepScan)))
				num += 30f;
			if (hasStealthTech)
				num -= game.AssetDatabase.TacStealthArmorBonus;
			switch (this.ShipClass)
			{
				case ShipClass.Dreadnought:
					num += 50f;
					break;
				case ShipClass.Leviathan:
				case ShipClass.Station:
					num += 100f;
					break;
				case ShipClass.BattleRider:
					if (this.RealShipClass == RealShipClasses.Drone)
					{
						num -= 50f;
						break;
					}
					num -= 25f;
					break;
			}
			return num;
		}

		private float CalcShipCritModifier(
		  List<SectionEnumerations.DesignAttribute> attributes,
		  float value)
		{
			if (attributes.Count == 0 || !attributes.Contains(SectionEnumerations.DesignAttribute.Hard_Luck_Ship))
				return value;
			value *= 2f;
			return value;
		}

		private float CalcRepairCritModifier(
		  List<SectionEnumerations.DesignAttribute> attributes,
		  List<AdmiralInfo.TraitType> admiralTraits,
		  float value)
		{
			if (attributes.Count == 0)
				return value;
			float num = 1f;
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Spirit_Of_The_Yorktown))
				num += 0.5f;
			if (admiralTraits.Contains(AdmiralInfo.TraitType.Elite))
				++num;
			return value * num;
		}

		private int CalcCrewDeathFromStructureModifier(
		  List<SectionEnumerations.DesignAttribute> attributes,
		  int value)
		{
			if (attributes.Count == 0 || !attributes.Contains(SectionEnumerations.DesignAttribute.Death_Trap))
				return value;
			++value;
			return value;
		}

		private int CalcCrewDeathFromBoardingModifier(
		  List<SectionEnumerations.DesignAttribute> attributes,
		  int value)
		{
			if (attributes.Count == 0 || !attributes.Contains(SectionEnumerations.DesignAttribute.Death_Trap))
				return value;
			++value;
			return value;
		}

		public static int CalcArmorWidthModifier(
		  List<SectionEnumerations.DesignAttribute> attributes,
		  int value)
		{
			if (attributes.Count == 0)
				return value;
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Ol_Ironsides))
				value += 2;
			if (attributes.Contains(SectionEnumerations.DesignAttribute.Ghost_Of_The_Hood))
				value -= 2;
			return value;
		}

		public static bool HasAbsorberTech(List<ShipSectionAsset> sections, List<string> techs)
		{
			if (!sections.Any<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.IsAbsorberSection)))
				return techs.Contains("NRG_Energy_Absorbers");
			return true;
		}

		public static int GetPsiResistanceFromTech(AssetDatabase ab, List<string> techs)
		{
			if (techs.Contains("CYB_PsiShield"))
				return ab.GetTechBonus<int>("CYB_PsiShield", "psipower");
			return 0;
		}

		public static int GetArmorBonusFromTech(AssetDatabase ab, List<string> techs)
		{
			if (techs.Count == 0)
				return 0;
			if (techs.Contains("IND_Polysteel"))
				return ab.GetTechBonus<int>("IND_Polysteel", "armorlayers");
			if (techs.Contains("IND_MagnoCeramic_Latices"))
				return ab.GetTechBonus<int>("IND_MagnoCeramic_Latices", "armorlayers");
			if (techs.Contains("IND_Quark_Resonators"))
				return ab.GetTechBonus<int>("IND_Quark_Resonators", "armorlayers");
			if (techs.Contains("IND_Adamantine_Alloys"))
				return ab.GetTechBonus<int>("IND_Adamantine_Alloys", "armorlayers");
			return 0;
		}

		public static int GetPermArmorBonusFromTech(AssetDatabase ab, List<string> techs)
		{
			if (techs.Contains("IND_Adamantine_Alloys"))
				return ab.GetTechBonus<int>("IND_Adamantine_Alloys", "permarmorlayers");
			return 0;
		}

		public static float GetRichocetModifier(AssetDatabase ab, List<string> techs)
		{
			if (techs.Count == 0)
				return 0.0f;
			if (techs.Contains("IND_Polysteel"))
				return ab.GetTechBonus<float>("IND_Polysteel", "ricochet");
			if (techs.Contains("IND_MagnoCeramic_Latices"))
				return ab.GetTechBonus<float>("IND_MagnoCeramic_Latices", "ricochet");
			if (techs.Contains("IND_Quark_Resonators"))
				return ab.GetTechBonus<float>("IND_Quark_Resonators", "ricochet");
			if (techs.Contains("IND_Adamantine_Alloys"))
				return ab.GetTechBonus<float>("IND_Adamantine_Alloys", "ricochet");
			return 0.0f;
		}

		public static float GetBeamReflectModifier(AssetDatabase ab, List<string> techs)
		{
			if (techs.Count == 0)
				return 0.0f;
			if (techs.Contains("IND_Reflective"))
				return ab.GetTechBonus<float>("IND_Reflective", "beamdamage");
			if (techs.Contains("IND_Improved_Reflective"))
				return ab.GetTechBonus<float>("IND_Improved_Reflective", "beamdamage");
			return 0.0f;
		}

		public static float GetLaserReflectModifier(AssetDatabase ab, List<string> techs)
		{
			if (techs.Count == 0)
				return 0.0f;
			if (techs.Contains("IND_Reflective"))
				return ab.GetTechBonus<float>("IND_Reflective", "laserricochet");
			if (techs.Contains("IND_Improved_Reflective"))
				return ab.GetTechBonus<float>("IND_Improved_Reflective", "laserricochet");
			return 0.0f;
		}

		public static float GetElectricEffectModifier(AssetDatabase ab, List<string> techs)
		{
			if (techs.Count == 0)
				return 1f;
			float num = 1f;
			if (techs.Contains("IND_Electronic_Hardening"))
				num += ab.GetTechBonus<float>("IND_Electronic_Hardening", "damage");
			return num;
		}

		public static int GetPowerWithTech(
		  AssetDatabase ab,
		  List<string> techs,
		  List<PlayerTechInfo> playerTechs,
		  int currentPower)
		{
			if (techs.Count == 0)
				return currentPower;
			float num1 = 1f;
			if (techs.Contains("NRG_Plasma_Induction"))
				num1 += ab.GetTechBonus<float>("NRG_Plasma_Induction", "power");
			if (techs.Contains("NRG_Wave_Amplification"))
				num1 += ab.GetTechBonus<float>("NRG_Wave_Amplification", "power");
			float num2 = num1 + Player.GetPowerBonus(ab, playerTechs);
			return (int)((double)currentPower * (double)num2);
		}

		public static int GetSupplyWithTech(AssetDatabase ab, List<string> techs, int currentSupply)
		{
			if (techs.Count == 0)
				return currentSupply;
			float num = 1f;
			if (techs.Contains("NRG_Plasma_Focusing"))
				num += ab.GetTechBonus<float>("NRG_Plasma_Focusing", "supply");
			return (int)((double)currentSupply * (double)num);
		}

		public static int GetStructureWithTech(AssetDatabase ab, List<string> techs, int currentStruct)
		{
			if (techs.Count == 0)
				return currentStruct;
			float num = 1f;
			if (techs.Contains("SLD_Structural_Fields"))
				num += ab.GetTechBonus<float>("SLD_Structural_Fields", "structure");
			return (int)((double)currentStruct * (double)num);
		}

		private CloakingType GetCloakingType(
		  List<string> techs,
		  CloakingType sectionCloakType)
		{
			switch (sectionCloakType)
			{
				case CloakingType.None:
					return CloakingType.None;
				case CloakingType.Cloaking:
					if (techs.Contains("SLD_Improved_Cloaking"))
						return CloakingType.ImprovedCloaking;
					if (techs.Contains("SLD_Intangibility"))
						return CloakingType.Intangible;
					break;
			}
			return sectionCloakType;
		}

		private float GetPlanetDamageBonusFromAdmiralTraits(
		  FleetInfo fleet,
		  List<AdmiralInfo.TraitType> traits)
		{
			float num = 0.0f;
			if (fleet != null && traits.Contains(AdmiralInfo.TraitType.Sherman))
				num += 2f;
			return num;
		}

		private float GetBaseROFBonusFromAdmiralTraits(
		  GameDatabase db,
		  FleetInfo fleet,
		  List<AdmiralInfo.TraitType> traits,
		  List<Player> playersInCombat)
		{
			float num = 0.0f;
			if (fleet != null && traits.Any<AdmiralInfo.TraitType>((Func<AdmiralInfo.TraitType, bool>)(x =>
		   {
			   if (x != AdmiralInfo.TraitType.Defender && x != AdmiralInfo.TraitType.Attacker && x != AdmiralInfo.TraitType.GloryHound)
				   return x == AdmiralInfo.TraitType.Technophobe;
			   return true;
		   })))
			{
				int? systemOwningPlayer = db.GetSystemOwningPlayer(fleet.SystemID);
				DiplomacyState diplomacyState = systemOwningPlayer.HasValue ? db.GetDiplomacyStateBetweenPlayers(systemOwningPlayer.Value, fleet.PlayerID) : DiplomacyState.UNKNOWN;
				if (traits.Contains(AdmiralInfo.TraitType.Attacker))
					num += diplomacyState == DiplomacyState.WAR || !systemOwningPlayer.HasValue ? 0.1f : -0.1f;
				if (traits.Contains(AdmiralInfo.TraitType.Defender))
					num += !systemOwningPlayer.HasValue || systemOwningPlayer.Value != fleet.PlayerID && diplomacyState != DiplomacyState.ALLIED ? -0.1f : 0.1f;
				if (traits.Contains(AdmiralInfo.TraitType.GloryHound))
					num += !systemOwningPlayer.HasValue || systemOwningPlayer.Value != fleet.PlayerID && diplomacyState != DiplomacyState.ALLIED ? 0.0f : 0.2f;
				if (traits.Contains(AdmiralInfo.TraitType.Technophobe) && playersInCombat.Any<Player>((Func<Player, bool>)(x =>
			   {
				   if (x.ID != fleet.ID && x.Faction.Name == "loa")
					   return db.GetDiplomacyStateBetweenPlayers(x.ID, fleet.PlayerID) == DiplomacyState.WAR;
				   return false;
			   })))
					num += 0.15f;
			}
			return num;
		}

		private float GetInStandOffROFBonusFromAdmiralTraits(
		  FleetInfo fleet,
		  List<AdmiralInfo.TraitType> traits)
		{
			float num = 0.0f;
			if (fleet != null && traits.Contains(AdmiralInfo.TraitType.ArtilleryExpert))
				num += 0.1f;
			return num;
		}

		private float GetInPursueSpeedBonusFromAdmiralTraits(
		  FleetInfo fleet,
		  List<AdmiralInfo.TraitType> traits)
		{
			float num = 0.0f;
			if (fleet != null && traits.Contains(AdmiralInfo.TraitType.Hunter))
				num += 0.15f;
			return num;
		}

		private static float GetBioMissileBonusModifier(
		  App game,
		  Ship ship,
		  ShipSectionAsset missionSection)
		{
			float num = 1f;
			if (ship.IsSuulka)
				num = game.AssetDatabase.SuulkaPsiBonuses.First<SuulkaPsiBonus>((Func<SuulkaPsiBonus, bool>)(x => x.Name == missionSection.SectionName)).BioMissileMultiplyer;
			return num;
		}

		public static int GetNumShipsThatShouldTarget(Ship ship)
		{
			switch (ship.ShipClass)
			{
				case ShipClass.Cruiser:
					return 2;
				case ShipClass.Dreadnought:
					return 3;
				case ShipClass.Leviathan:
					return 5;
				case ShipClass.BattleRider:
					return 1;
				case ShipClass.Station:
					return 6;
				default:
					return 2;
			}
		}

		private IList<Turret> CreateTurretsForBank(
		  string preferredMount,
		  Faction faction,
		  ShipInfo ship,
		  FleetInfo fleet,
		  ShipSectionAsset section,
		  Section sectionObj,
		  IEnumerable<WeaponAssignment> assignedWeapons,
		  IEnumerable<LogicalWeapon> preferredWeapons,
		  IEnumerable<LogicalWeapon> weapons,
		  IEnumerable<LogicalTurretHousing> turretHousings,
		  IEnumerable<WeaponInstanceInfo> weaponIns,
		  List<PlayerTechInfo> playerTechs,
		  LogicalEffect turretEffect,
		  Module module,
		  LogicalBank bank,
		  bool isTestMode)
		{
			List<Turret> turretList = new List<Turret>();
			try
			{
				int designID = 0;
				int targetFilter = 0;
				int fireMode = 0;
				string moduleNodeName = module != null ? module.Attachment.NodeName : "";
				LogicalWeapon weapon = Ship.SelectWeapon(section, bank, assignedWeapons, preferredWeapons, weapons, moduleNodeName, out designID, out targetFilter, out fireMode);
				LogicalTurretClass weaponTurretClass = weapon.GetLogicalTurretClassForMount(bank.TurretSize, bank.TurretClass);
				LogicalTurretHousing housing = turretHousings.First<LogicalTurretHousing>((Func<LogicalTurretHousing, bool>)(housingCandidate =>
			   {
				   if (weaponTurretClass.TurretClass == housingCandidate.Class && weapon.DefaultWeaponSize == housingCandidate.WeaponSize)
					   return bank.TurretSize == housingCandidate.MountSize;
				   return false;
			   }));
				float turretHealth = Ship.GetTurretHealth(bank.TurretSize);
				float turretShapeRadius = Ship.GetTurretShapeRadius(bank.TurretSize);
				bool flag = faction.Name == "swarm" && this.RealShipClass == RealShipClasses.Dreadnought;
				if (!isTestMode && weapon != null && weapon.PayloadType == WeaponEnums.PayloadTypes.MegaBeam)
				{
					PlayerTechInfo playerTechInfo = playerTechs.FirstOrDefault<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == "DRV_Neutrino_Blast_Wave"));
					if (playerTechInfo == null || playerTechInfo.State != TechStates.Researched)
						return (IList<Turret>)turretList;
				}
				MountObject.WeaponModels weaponModels = new MountObject.WeaponModels();
				weaponModels.FillOutModelFilesWithWeapon(weapon, faction, weapons);
				int weaponLevelFromTechs = Player.GetWeaponLevelFromTechs(weapon, playerTechs.ToList<PlayerTechInfo>());
				if (this._player.IsAI() && (bank.TurretSize == WeaponEnums.WeaponSizes.Light || bank.TurretSize == WeaponEnums.WeaponSizes.VeryLight) && (ship == null || ship.DesignInfo == null || ((IEnumerable<DesignSectionInfo>)ship.DesignInfo.DesignSections).Sum<DesignSectionInfo>((Func<DesignSectionInfo, int>)(x => ((IEnumerable<LogicalBank>)x.ShipSectionAsset.Banks).Count<LogicalBank>((Func<LogicalBank, bool>)(y =>
			{
				if (y.TurretSize != WeaponEnums.WeaponSizes.Light)
					return y.TurretSize != WeaponEnums.WeaponSizes.VeryLight;
				return false;
			})))) > 0))
					targetFilter = 4;
				WeaponBank weaponBank = new WeaponBank(this.App, (IGameObject)this, bank, module, weapon, weaponLevelFromTechs, designID, targetFilter, fireMode, bank.TurretSize, bank.TurretClass);
				weaponBank.AddExistingObject(this.App);
				this.AddObject((IGameObject)weaponBank);
				LogicalBank localBank = bank;
				foreach (LogicalMount logicalMount in ((IEnumerable<LogicalMount>)section.Mounts).Where<LogicalMount>((Func<LogicalMount, bool>)(x => x.Bank == localBank)))
				{
					LogicalMount mount = logicalMount;
					string model = Ship.FixAssetNameForDLC(weaponTurretClass.GetBaseModel(faction, mount, housing), preferredMount);
					string baseDamageModel = weaponTurretClass.GetBaseDamageModel(faction, mount, housing);
					string turretModelName = Ship.FixAssetNameForDLC(weaponTurretClass.GetTurretModelName(faction, mount, housing), preferredMount);
					string str = Ship.FixAssetNameForDLC(weaponTurretClass.GetBarrelModelName(faction, mount), preferredMount);
					GenericCollisionShape state1 = (GenericCollisionShape)null;
					if (flag || Ship.GetTurretNeedsCollision(bank.TurretClass, turretModelName))
					{
						state1 = this.App.AddObject<GenericCollisionShape>((object)GenericCollisionShape.CollisionShapeType.SPHERE, (object)turretShapeRadius);
						this.AddObject((IGameObject)state1);
					}
					TurretBase state2 = (TurretBase)null;
					if (!string.IsNullOrEmpty(model))
					{
						state2 = new TurretBase(this.App, model, baseDamageModel, sectionObj, module);
						this.AddObject((IGameObject)state2);
					}
					float num = turretHealth;
					if (weaponIns != null)
					{
						WeaponInstanceInfo weaponInstanceInfo = weaponIns.FirstOrDefault<WeaponInstanceInfo>((Func<WeaponInstanceInfo, bool>)(x => x.NodeName == mount.NodeName));
						if (weaponInstanceInfo != null)
							num = weaponInstanceInfo.Structure;
					}
					Turret turret = new Turret(this.App, new Turret.TurretDescription()
					{
						BarrelModelName = str,
						DestroyedTurretEffect = turretEffect,
						Housing = housing,
						LogicalBank = bank,
						Mount = mount,
						ParentObject = (IGameObject)((GameObject)module ?? (GameObject)sectionObj),
						Section = sectionObj,
						Module = module,
						Ship = this,
						SInfo = ship,
						Fleet = fleet,
						TurretBase = state2,
						CollisionShapeRadius = turretShapeRadius,
						TurretCollisionShape = state1,
						TurretHealth = num,
						MaxTurretHealth = turretHealth,
						TurretModelName = turretModelName,
						WeaponBank = weaponBank,
						Weapon = weapon,
						TechModifiers = Player.ObtainWeaponTechModifiers(this.App.AssetDatabase, localBank.TurretClass, weapon, (IEnumerable<PlayerTechInfo>)playerTechs),
						WeaponModels = weaponModels,
						TurretIndex = this._currentTurretIndex
					});
					++this._currentTurretIndex;
					if (state2 != null)
						state2.SetTag((object)turret);
					this.AddObject((IGameObject)turret);
					if (state1 != null)
						state1.SetTag((object)turret);
					turretList.Add(turret);
				}
				if (module != null)
				{
					if (module.LogicalModule != null)
					{
						foreach (LogicalMount logicalMount in ((IEnumerable<LogicalMount>)module.LogicalModule.Mounts).Where<LogicalMount>((Func<LogicalMount, bool>)(x => x.Bank == localBank)))
						{
							LogicalMount mount = logicalMount;
							string model = Ship.FixAssetNameForDLC(weaponTurretClass.GetBaseModel(faction, mount, housing), preferredMount);
							string baseDamageModel = weaponTurretClass.GetBaseDamageModel(faction, mount, housing);
							string turretModelName = Ship.FixAssetNameForDLC(weaponTurretClass.GetTurretModelName(faction, mount, housing), preferredMount);
							string str = Ship.FixAssetNameForDLC(weaponTurretClass.GetBarrelModelName(faction, mount), preferredMount);
							GenericCollisionShape state1 = (GenericCollisionShape)null;
							if (flag || Ship.GetTurretNeedsCollision(bank.TurretClass, turretModelName))
							{
								state1 = this.App.AddObject<GenericCollisionShape>((object)GenericCollisionShape.CollisionShapeType.SPHERE, (object)turretShapeRadius);
								this.AddObject((IGameObject)state1);
							}
							TurretBase state2 = (TurretBase)null;
							if (!string.IsNullOrEmpty(model))
							{
								state2 = new TurretBase(this.App, model, baseDamageModel, sectionObj, module);
								this.AddObject((IGameObject)state2);
							}
							float num = turretHealth;
							if (weaponIns != null)
							{
								WeaponInstanceInfo weaponInstanceInfo = weaponIns.FirstOrDefault<WeaponInstanceInfo>((Func<WeaponInstanceInfo, bool>)(x => x.NodeName == mount.NodeName));
								if (weaponInstanceInfo != null)
									num = weaponInstanceInfo.Structure;
							}
							Turret turret = new Turret(this.App, new Turret.TurretDescription()
							{
								BarrelModelName = str,
								DestroyedTurretEffect = turretEffect,
								Housing = housing,
								LogicalBank = bank,
								Mount = mount,
								ParentObject = (IGameObject)((GameObject)module ?? (GameObject)sectionObj),
								Section = sectionObj,
								Module = module,
								Ship = this,
								SInfo = ship,
								Fleet = fleet,
								TurretBase = state2,
								CollisionShapeRadius = turretShapeRadius,
								TurretCollisionShape = state1,
								TurretHealth = num,
								MaxTurretHealth = turretHealth,
								TurretModelName = turretModelName,
								WeaponBank = weaponBank,
								Weapon = weapon,
								WeaponModels = weaponModels,
								TechModifiers = Player.ObtainWeaponTechModifiers(this.App.AssetDatabase, localBank.TurretClass, weapon, (IEnumerable<PlayerTechInfo>)playerTechs)
							});
							if (state2 != null)
								state2.SetTag((object)turret);
							this.AddObject((IGameObject)turret);
							if (state1 != null)
								state1.SetTag((object)turret);
							turretList.Add(turret);
						}
						foreach (Turret turret in turretList)
						{
							if (module.LogicalModule.AbilityType == ModuleEnums.ModuleAbilities.Tendril)
								module.PostSetProp("SetTendrilTurret", turret.ObjectID);
						}
					}
				}
			}
			catch (Exception ex)
			{
				App.Log.Warn(ex.ToString(), "design");
			}
			return (IList<Turret>)turretList;
		}

		public float GetTacSensorBonus(LogicalModule logModule, StationType stationType, int level)
		{
			if (stationType == StationType.INVALID_TYPE || logModule.ModuleType != ModuleEnums.ModuleSlotTypes.Sensor.ToString())
				return logModule.SensorBonus;
			switch (stationType)
			{
				case StationType.NAVAL:
					return 500f;
				case StationType.SCIENCE:
					return 250f;
				case StationType.CIVILIAN:
					return 500f;
				case StationType.DIPLOMATIC:
					return 200f;
				case StationType.GATE:
					return 500f;
				default:
					return 0.0f;
			}
		}

		public bool OnlyOneSquad()
		{
			switch (this._combatAI)
			{
				case SectionEnumerations.CombatAiType.SwarmerHive:
				case SectionEnumerations.CombatAiType.SwarmerQueen:
				case SectionEnumerations.CombatAiType.VonNeumannBerserkerMotherShip:
				case SectionEnumerations.CombatAiType.VonNeumannDisc:
				case SectionEnumerations.CombatAiType.LocustMoon:
				case SectionEnumerations.CombatAiType.LocustWorld:
				case SectionEnumerations.CombatAiType.MorrigiRelic:
					return true;
				default:
					return false;
			}
		}

		public Module CreateModule(
		  LogicalModuleMount modMount,
		  LogicalModule logModule,
		  List<ModuleInstanceInfo> modInstances,
		  string preferredMount,
		  Ship ship,
		  Section section,
		  CollisionShape shape,
		  CollisionShape lowStructShape,
		  CollisionShape deadShape,
		  bool isKillable,
		  bool removeWhenKilled)
		{
			float structure = logModule.Structure;
			ModuleInstanceInfo moduleInstanceInfo = modInstances.FirstOrDefault<ModuleInstanceInfo>((Func<ModuleInstanceInfo, bool>)(x => x.ModuleNodeID == modMount.NodeName));
			if (moduleInstanceInfo != null)
				structure = (float)moduleInstanceInfo.Structure;
			List<object> objectList = new List<object>();
			objectList.Add((object)ScriptHost.AllowConsole);
			objectList.Add((object)ship.ObjectID);
			objectList.Add((object)section.ObjectID);
			objectList.Add((object)Ship.FixAssetNameForDLC(logModule.ModelPath, preferredMount));
			objectList.Add((object)shape.ObjectID);
			objectList.Add((object)Ship.FixAssetNameForDLC(logModule.LowStructModelPath, preferredMount));
			objectList.Add((object)(lowStructShape != null ? lowStructShape.ObjectID : 0));
			objectList.Add((object)Ship.FixAssetNameForDLC(logModule.DeadModelPath, preferredMount));
			objectList.Add((object)(deadShape != null ? deadShape.ObjectID : 0));
			objectList.Add((object)isKillable);
			objectList.Add((object)removeWhenKilled);
			objectList.Add((object)this._currentModuleIndex);
			objectList.Add((object)"");
			objectList.Add((object)modMount.NodeName);
			objectList.Add((object)logModule.AmbientSound);
			objectList.Add((object)logModule.AbilityType.ToString());
			objectList.Add((object)logModule.LowStruct);
			objectList.Add((object)logModule.Structure);
			objectList.Add((object)logModule.StructureBonus);
			objectList.Add((object)logModule.AbilitySupply);
			objectList.Add((object)logModule.Crew);
			objectList.Add((object)logModule.CrewRequired);
			objectList.Add((object)logModule.Supply);
			objectList.Add((object)logModule.PowerBonus);
			objectList.Add((object)logModule.RepairPointsBonus);
			objectList.Add((object)logModule.ECCM);
			objectList.Add((object)logModule.ECM);
			objectList.Add((object)logModule.AccelBonus);
			objectList.Add((object)logModule.CriticalHitBonus);
			objectList.Add((object)logModule.AccuracyBonus);
			objectList.Add((object)logModule.ROFBonus);
			objectList.Add((object)logModule.CrewEfficiencyBonus);
			objectList.Add((object)logModule.DamageBonus);
			objectList.Add((object)this.GetTacSensorBonus(logModule, section.ShipSectionAsset.StationType, section.ShipSectionAsset.StationLevel));
			objectList.Add((object)logModule.PsionicPowerBonus);
			objectList.Add((object)logModule.PsionicStaminaBonus);
			objectList.Add((object)logModule.AdmiralSurvivalBonus);
			objectList.Add((object)structure);
			objectList.Add((object)logModule.DamageEffect.Name);
			objectList.Add((object)logModule.DeathEffect.Name);
			objectList.Add((object)500f);
			Module module;
			switch (logModule.AbilityType)
			{
				case ModuleEnums.ModuleAbilities.Tentacle:
					module = (Module)this.App.AddObject<SuulkaTentacleModule>(objectList.ToArray());
					break;
				case ModuleEnums.ModuleAbilities.Tendril:
					module = (Module)this.App.AddObject<SuulkaTendrilModule>(objectList.ToArray());
					break;
				case ModuleEnums.ModuleAbilities.GoopArmorRepair:
					objectList.Add((object)logModule.AbilitySupply);
					objectList.Add((object)3f);
					module = (Module)this.App.AddObject<GoopModule>(objectList.ToArray());
					break;
				case ModuleEnums.ModuleAbilities.JokerECM:
					objectList.Add((object)logModule.AbilitySupply);
					objectList.Add((object)3f);
					module = (Module)this.App.AddObject<JokerECMModule>(objectList.ToArray());
					break;
				case ModuleEnums.ModuleAbilities.AbaddonLaser:
					module = (Module)this.App.AddObject<AbaddonLaserModule>(objectList.ToArray());
					break;
				default:
					module = this.App.AddObject<Module>(objectList.ToArray());
					break;
			}
			++this._currentModuleIndex;
			if (module != null)
				module.IsAlive = (double)structure > 0.0;
			return module;
		}

		private void CreatePsionics(
		  IEnumerable<LogicalPsionic> psionics,
		  Ship owner,
		  List<SectionEnumerations.PsionicAbility> psionicList)
		{
			if (psionics == null)
				return;
			foreach (SectionEnumerations.PsionicAbility psionic in psionicList)
			{
				SectionEnumerations.PsionicAbility psionicType = psionic;
				LogicalPsionic logicalPsionic = psionics.FirstOrDefault<LogicalPsionic>((Func<LogicalPsionic, bool>)(x => x.Name == psionicType.ToString()));
				if (logicalPsionic != null)
				{
					Psionic state = (Psionic)null;
					List<object> objectList = new List<object>();
					objectList.Add((object)owner.ObjectID);
					objectList.Add((object)logicalPsionic.MinPower);
					objectList.Add((object)logicalPsionic.MaxPower);
					objectList.Add((object)logicalPsionic.BaseCost);
					objectList.Add((object)logicalPsionic.Range);
					objectList.Add((object)logicalPsionic.BaseDamage);
					objectList.Add((object)logicalPsionic.CastorEffect.Name);
					objectList.Add((object)logicalPsionic.CastEffect.Name);
					objectList.Add((object)logicalPsionic.ApplyEffect.Name);
					objectList.Add((object)logicalPsionic.Model);
					objectList.Add((object)this.IsSuulka);
					if (this.IsSuulka)
					{
						string suulkaName = this.MissionSection.ShipSectionAsset.SectionName;
						SuulkaPsiBonus suulkaPsiBonus = this.App.AssetDatabase.SuulkaPsiBonuses.First<SuulkaPsiBonus>((Func<SuulkaPsiBonus, bool>)(x => x.Name == suulkaName));
						objectList.Add((object)suulkaPsiBonus.Rate[(int)psionicType]);
						objectList.Add((object)suulkaPsiBonus.PsiEfficiency[(int)psionicType]);
						objectList.Add((object)suulkaPsiBonus.PsiDrainMultiplyer);
						objectList.Add((object)suulkaPsiBonus.LifeDrainMultiplyer);
						objectList.Add((object)suulkaPsiBonus.TKFistMultiplyer);
						objectList.Add((object)suulkaPsiBonus.CrushMultiplyer);
						objectList.Add((object)suulkaPsiBonus.FearMultiplyer);
						objectList.Add((object)suulkaPsiBonus.ControlDuration);
						objectList.Add((object)suulkaPsiBonus.MovementMultiplyer);
					}
					switch (psionicType)
					{
						case SectionEnumerations.PsionicAbility.TKFist:
							state = (Psionic)this.App.AddObject<TKFist>(objectList.ToArray());
							break;
						case SectionEnumerations.PsionicAbility.Hold:
							state = (Psionic)this.App.AddObject<Hold>(objectList.ToArray());
							break;
						case SectionEnumerations.PsionicAbility.Crush:
							state = (Psionic)this.App.AddObject<Crush>(objectList.ToArray());
							break;
						case SectionEnumerations.PsionicAbility.Reflector:
							state = (Psionic)this.App.AddObject<Reflector>(objectList.ToArray());
							break;
						case SectionEnumerations.PsionicAbility.Repair:
							state = (Psionic)this.App.AddObject<Repair>(objectList.ToArray());
							break;
						case SectionEnumerations.PsionicAbility.AbaddonLaser:
							state = (Psionic)this.App.AddObject<AbaddonLaser>(objectList.ToArray());
							break;
						case SectionEnumerations.PsionicAbility.Fear:
							state = (Psionic)this.App.AddObject<Fear>(objectList.ToArray());
							break;
						case SectionEnumerations.PsionicAbility.Inspiration:
							state = (Psionic)this.App.AddObject<Inspiration>(objectList.ToArray());
							break;
						case SectionEnumerations.PsionicAbility.Reveal:
							state = (Psionic)this.App.AddObject<Reveal>(objectList.ToArray());
							break;
						case SectionEnumerations.PsionicAbility.Posses:
							state = (Psionic)this.App.AddObject<Posses>(objectList.ToArray());
							break;
						case SectionEnumerations.PsionicAbility.Listen:
							state = (Psionic)this.App.AddObject<Listen>(objectList.ToArray());
							break;
						case SectionEnumerations.PsionicAbility.Block:
							state = (Psionic)this.App.AddObject<Block>(objectList.ToArray());
							break;
						case SectionEnumerations.PsionicAbility.PsiDrain:
							state = (Psionic)this.App.AddObject<PsiDrain>(objectList.ToArray());
							break;
						case SectionEnumerations.PsionicAbility.WildFire:
							state = (Psionic)this.App.AddObject<WildFire>(objectList.ToArray());
							break;
						case SectionEnumerations.PsionicAbility.Control:
							state = (Psionic)this.App.AddObject<Control>(objectList.ToArray());
							break;
						case SectionEnumerations.PsionicAbility.LifeDrain:
							state = (Psionic)this.App.AddObject<LifeDrain>(objectList.ToArray());
							break;
						case SectionEnumerations.PsionicAbility.Mirage:
							state = (Psionic)this.App.AddObject<Mirage>(objectList.ToArray());
							break;
						case SectionEnumerations.PsionicAbility.FalseFriend:
							state = (Psionic)this.App.AddObject<FalseFriend>(objectList.ToArray());
							break;
						case SectionEnumerations.PsionicAbility.Invisibility:
							state = (Psionic)this.App.AddObject<Invisibility>(objectList.ToArray());
							break;
						case SectionEnumerations.PsionicAbility.Movement:
							state = (Psionic)this.App.AddObject<Movement>(objectList.ToArray());
							break;
					}
					if (state != null)
					{
						state.Type = psionicType;
						state.SetTag((object)owner);
						this.AddObject((IGameObject)state);
					}
				}
			}
		}

		public ShipManeuvering Maneuvering
		{
			get
			{
				return this._objects.OfType<ShipManeuvering>().First<ShipManeuvering>();
			}
		}

		public IEnumerable<WeaponBank> WeaponBanks
		{
			get
			{
				return this._objects.OfType<WeaponBank>();
			}
		}

		public RigidBody RigidBody
		{
			get
			{
				return this._objects.OfType<RigidBody>().First<RigidBody>();
			}
		}

		public Shield Shield
		{
			get
			{
				return this._objects.OfType<Shield>().FirstOrDefault<Shield>();
			}
		}

		public CompoundCollisionShape CompoundCollisionShape
		{
			get
			{
				return this._objects.OfType<CompoundCollisionShape>().First<CompoundCollisionShape>();
			}
		}

		public IEnumerable<GenericCollisionShape> TurretShapes
		{
			get
			{
				return this._objects.OfType<GenericCollisionShape>();
			}
		}

		public IEnumerable<BattleRiderSquad> BattleRiderSquads
		{
			get
			{
				return this._objects.OfType<BattleRiderSquad>();
			}
		}

		public IEnumerable<BattleRiderMount> BattleRiderMounts
		{
			get
			{
				return this._objects.OfType<BattleRiderMount>();
			}
		}

		public IEnumerable<MountObject> MountObjects
		{
			get
			{
				return this._objects.OfType<MountObject>();
			}
		}

		public IEnumerable<CollisionShape> CollisionShapes
		{
			get
			{
				return this._objects.OfType<CollisionShape>();
			}
		}

		public IEnumerable<CollisionShape> SectionCollisionShapes
		{
			get
			{
				return this._objects.OfType<CollisionShape>().Where<CollisionShape>((Func<CollisionShape, bool>)(x => x.GetTag() is ShipSectionAsset));
			}
		}

		public IEnumerable<CollisionShape> ModuleCollisionShapes
		{
			get
			{
				return this._objects.OfType<CollisionShape>().Where<CollisionShape>((Func<CollisionShape, bool>)(x => x.GetTag() is Module));
			}
		}

		public IEnumerable<Section> Sections
		{
			get
			{
				return this._objects.OfType<Section>().Where<Section>((Func<Section, bool>)(x => x.GetTag() is ShipSectionAsset));
			}
		}

		public IEnumerable<Turret> Turrets
		{
			get
			{
				return this._objects.OfType<Turret>();
			}
		}

		public IEnumerable<TurretBase> TurretBases
		{
			get
			{
				return this._objects.OfType<TurretBase>();
			}
		}

		public IEnumerable<Module> Modules
		{
			get
			{
				return this._objects.OfType<Module>();
			}
		}

		public Section MissionSection
		{
			get
			{
				return this.Sections.First<Section>((Func<Section, bool>)(x => x.GetTag<ShipSectionAsset>().Type == ShipSectionType.Mission));
			}
		}

		public IEnumerable<Psionic> Psionics
		{
			get
			{
				return this._objects.OfType<Psionic>();
			}
		}

		public IEnumerable<AttachableEffectObject> AttachableEffects
		{
			get
			{
				return this._objects.OfType<AttachableEffectObject>();
			}
		}

		public void Dispose()
		{
			if (!this._isDisposed)
			{
				this.App.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
				if (this._faction != null)
					this._faction.ReleaseFactionReference(this.App);
				this._faction = (Faction)null;
				this._target = (IGameObject)null;
				this._taskGroup = (TaskGroup)null;
				foreach (IGameObject state in this._objects)
				{
					state.ClearTag();
					if (state is IDisposable)
						(state as IDisposable).Dispose();
				}
				this.ClearTag();
				this.App.ReleaseObjects(this._objects.Concat<IGameObject>((IEnumerable<IGameObject>)new Ship[1]
				{
		  this
				}));
			}
			this._isDisposed = true;
		}

		public Vector3 Position
		{
			get
			{
				return this.Maneuvering.Position;
			}
			set
			{
				this.RigidBody.PostSetPosition(value);
				this.Maneuvering.Position = value;
			}
		}

		public Vector3 Rotation
		{
			get
			{
				return this.Maneuvering.Rotation;
			}
			set
			{
				this.RigidBody.PostSetRotation(value);
				this.Maneuvering.Rotation = value;
			}
		}

		public Vector3 Velocity
		{
			get
			{
				return this.Maneuvering.Velocity;
			}
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

		public bool Deployed
		{
			get
			{
				return this._isDeployed;
			}
			set
			{
				if (value == this._isDeployed)
					return;
				this._isDeployed = value;
				this.PostSetProp("deployed", true);
				this.DisableManeuvering(true);
			}
		}

		public BattleRiderSquad AssignRiderToSquad(BattleRiderShip brs, int riderIndex)
		{
			if (riderIndex <= -1)
				return this.BattleRiderSquads.FirstOrDefault<BattleRiderSquad>();
			foreach (BattleRiderSquad battleRiderSquad in this.BattleRiderSquads)
			{
				BattleRiderMount battleRiderMount = battleRiderSquad.Mounts.FirstOrDefault<BattleRiderMount>((Func<BattleRiderMount, bool>)(x => x.SquadIndex == riderIndex));
				if (battleRiderMount != null && BattleRiderMount.CanBattleRiderConnect(battleRiderMount.WeaponBank.TurretClass, brs.BattleRiderType, brs.ShipClass))
					return battleRiderSquad;
			}
			return (BattleRiderSquad)null;
		}

		public Turret.FiringEnum ListenTurretFiring
		{
			get
			{
				return this._turretFiring;
			}
		}

		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
			switch (eventName)
			{
				case "FireOnObject":
					this.ProcessGameEvent_FireOnObject(eventParams);
					break;
				case "ClearTargets":
					this.ProcessGameEvent_ClearTargets(eventParams);
					break;
				case "HoldFire":
					this.ProcessGameEvent_HoldFire(eventParams);
					break;
				case "FireOnObjectWithSpecWeapon":
					this.ProcessGameEvent_FireOnObjectWithSpecWeapon(eventParams);
					break;
				case "RemoveShipContainingID":
					this.ProcessGameEvent_RemoveShipContainingID(eventParams);
					break;
				case "RemoveObjectsInCompound":
					this.ProcessGameEvent_RemoveObjectsInCompound(eventParams);
					break;
			}
		}

		private void ProcessGameEvent_RemoveObjectsInCompound(string[] eventParams)
		{
			char ch = '_';
			string[] strArray = eventParams[0].Split(ch);
			int objID = int.Parse(strArray[0]);
			if (!this._objects.Any<IGameObject>((Func<IGameObject, bool>)(x => x.ObjectID == objID)) || ((IEnumerable<string>)strArray).Count<string>() < 2)
				return;
			for (int index = 1; index < ((IEnumerable<string>)strArray).Count<string>(); ++index)
			{
				int id = int.Parse(strArray[index]);
				IGameObject gameObject = this._objects.FirstOrDefault<IGameObject>((Func<IGameObject, bool>)(x => x.ObjectID == id));
				if (gameObject != null)
					this.RemoveObject(gameObject);
			}
		}

		private void ProcessGameEvent_RemoveShipContainingID(string[] eventParams)
		{
			char ch = '_';
			int num = int.Parse(eventParams[0].Split(ch)[0]);
			bool flag = false;
			foreach (IGameObject gameObject in this._objects)
			{
				if (num == gameObject.ObjectID)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
				return;
			this.Dispose();
		}

		private void ProcessGameEvent_ClearTargets(string[] eventParams)
		{
			char ch = '_';
			int shipId = int.Parse(eventParams[0].Split(ch)[0]);
			if (!this._objects.Any<IGameObject>((Func<IGameObject, bool>)(x => x.ObjectID == shipId)))
				return;
			if (this.IsCarrier || this._shipClass == ShipClass.BattleRider)
				this.PostSetProp("ClearTarget");
			foreach (IGameObject weaponBank in this.WeaponBanks)
				weaponBank.PostSetProp("ClearTarget");
			foreach (IGameObject state in this.Modules.Where<Module>((Func<Module, bool>)(x =>
		   {
			   if (!(x is SuulkaTendrilModule))
				   return x is SuulkaTentacleModule;
			   return true;
		   })))
				state.PostSetProp("ClearTarget");
		}

		private void ProcessGameEvent_HoldFire(string[] eventParams)
		{
			char ch = '_';
			string[] strArray = eventParams[0].Split(ch);
			if (int.Parse(strArray[0]) != this.ObjectID)
				return;
			this.PostSetProp("HoldFire", int.Parse(strArray[1]) == 1);
		}

		private void ProcessGameEvent_FireOnObject(string[] eventParams)
		{
			char ch = '_';
			string[] strArray = eventParams[0].Split(ch);
			if (int.Parse(strArray[0]) != this.ObjectID)
				return;
			bool setAsRiderTarget = int.Parse(strArray[6]) == 1 || this.App.CurrentState is DesignScreenState || this.App.CurrentState is ComparativeAnalysysState;
			this.SetShipTarget(int.Parse(strArray[1]), new Vector3(float.Parse(strArray[3]), float.Parse(strArray[4]), float.Parse(strArray[5])), setAsRiderTarget, int.Parse(strArray[2]));
		}

		private void ProcessGameEvent_FireOnObjectWithSpecWeapon(string[] eventParams)
		{
			char ch = '^';
			string[] msg = eventParams[0].Split(ch);
			if (msg.Length < 6 || int.Parse(msg[0]) != this.ObjectID)
				return;
			foreach (IGameObject state in this.Turrets.Where<Turret>((Func<Turret, bool>)(x => x.Weapon.WeaponName == msg[1])))
				state.PostSetProp("SetTarget", (object)int.Parse(msg[2]), (object)new Vector3(float.Parse(msg[3]), float.Parse(msg[4]), float.Parse(msg[5])), (object)false);
		}

		public void SyncAltitude()
		{
			if (this._shipClass == ShipClass.BattleRider || this._shipClass == ShipClass.Station || this._combatAI != SectionEnumerations.CombatAiType.Normal)
				return;
			this.PostSetProp(nameof(SyncAltitude), this.Position.Y);
		}

		public void SetShipTarget(
		  int targetId,
		  Vector3 localPosition,
		  bool setAsRiderTarget = true,
		  int subTargetId = 0)
		{
			this.PostSetProp("SetMainShipTarget", (object)targetId, (object)subTargetId, (object)localPosition, (object)setAsRiderTarget);
			this._target = this.App.GetGameObject(targetId);
		}

		public void SetBlindFireTarget(
		  Vector3 center,
		  Vector3 localPosition,
		  float radius,
		  float fireDuration)
		{
			this.PostSetProp(nameof(SetBlindFireTarget), (object)center, (object)localPosition, (object)radius, (object)fireDuration);
			this._blindFireActive = true;
			this._target = (IGameObject)null;
		}

		public void SetShipSpecWeaponTarget(int weaponID, int targetId, Vector3 localPosition)
		{
			foreach (IGameObject state in this.WeaponBanks.Where<WeaponBank>((Func<WeaponBank, bool>)(x => x.Weapon.UniqueWeaponID == weaponID)))
				state.PostSetProp("SetTarget", (object)targetId, (object)localPosition);
		}

		public void SetShipWeaponToggleOn(int weaponID, bool on)
		{
			foreach (WeaponBank weaponBank in this.WeaponBanks.Where<WeaponBank>((Func<WeaponBank, bool>)(x => x.Weapon.UniqueWeaponID == weaponID)))
				weaponBank.ToggleState = on;
		}

		public void SetShipPositionalTarget(int weaponID, Vector3 targetPos, bool clearCurrentTargets)
		{
			WeaponBank weaponBank = this.WeaponBanks.FirstOrDefault<WeaponBank>((Func<WeaponBank, bool>)(x => x.Weapon.UniqueWeaponID == weaponID));
			if (weaponBank == null)
				return;
			this.PostSetProp("SetPositionTarget", (object)weaponBank.Weapon.GameObject.ObjectID, (object)targetPos, (object)clearCurrentTargets);
		}

		public void InitialSetPos(Vector3 position, Vector3 rotation)
		{
			this.Position = position;
			this.Rotation = rotation;
			this.Maneuvering.Destination = position;
		}

		public void KillShip(bool instantRemove = false)
		{
			if (!this._bIsDestroyed)
				this.PostSetProp(nameof(KillShip), instantRemove);
			this._bIsDestroyed = true;
			this.IsDriveless = true;
		}

		public override bool OnEngineMessage(InteropMessageID messageId, ScriptMessageReader message)
		{
			if (base.OnEngineMessage(messageId, message))
				return true;
			InteropMessageID interopMessageId = messageId;
			switch (messageId)
			{
				case InteropMessageID.IMID_SCRIPT_OBJECT_RELEASE:
					if (message.ReadInteger() == this.ObjectID)
						this.Dispose();
					return true;
				case InteropMessageID.IMID_SCRIPT_OBJECT_SETPROP:
					string str = message.ReadString();
					if (str == "Position")
					{
						this.Position = new Vector3(message.ReadSingle(), message.ReadSingle(), message.ReadSingle());
						return true;
					}
					if (str == "Stance")
					{
						this._stance = (CombatStance)message.ReadInteger();
						if (this._stance == CombatStance.RETREAT && !this.Maneuvering.RetreatData.SetDestination)
							this.Maneuvering.RetreatDestination = Vector3.Normalize(this.Maneuvering.Position) * this.Maneuvering.RetreatData.SystemRadius;
						return true;
					}
					if (str == "SetAcceptMoveOrders")
					{
						this._bCanAcceptMoveOrders = message.ReadInteger() == 1;
						return true;
					}
					if (str == "UnderAttack")
					{
						this._isUnderAttack = message.ReadBool();
						return true;
					}
					if (str == "DockedWithParent")
					{
						this._bDockedWithParent = message.ReadBool();
						return true;
					}
					if (str == "CarrierReadyToLaunch")
					{
						this._bCarrierCanLaunch = message.ReadBool();
						return true;
					}
					if (str == "AssaultingPlanet")
					{
						this._bAssaultingPlanet = message.ReadBool();
						return true;
					}
					if (str == "Cloaked")
					{
						this._cloakedState = (CloakedState)message.ReadInteger();
						return true;
					}
					if (str == "SetShipSphere")
					{
						this._boundingSphere.center.X = message.ReadSingle();
						this._boundingSphere.center.Y = message.ReadSingle();
						this._boundingSphere.center.Z = message.ReadSingle();
						this._boundingSphere.radius = message.ReadSingle();
						return true;
					}
					if (str == "WeaponFiringStateChanged")
					{
						this._turretFiring = (Turret.FiringEnum)message.ReadInteger();
						return true;
					}
					if (str == "SetPriorityWeapon")
					{
						this._priorityWeapon = message.ReadString();
						return true;
					}
					if (str == "ActivatedDeadObjects")
					{
						List<int> ids = new List<int>();
						int num = message.ReadInteger();
						for (int index = 0; index < num; ++index)
							ids.Add(message.ReadInteger());
						this.HandleActivatedDeadObjects(ids);
						return true;
					}
					if (str == "ShipKilled")
					{
						this._instantlyKilled = message.ReadBool();
						this._bIsDestroyed = true;
						this.IsDriveless = true;
						return true;
					}
					if (str == "BlindFireActive")
					{
						this._blindFireActive = message.ReadBool();
						return true;
					}
					if (str == "Retreated")
					{
						if (this._shipRole != ShipRole.FREIGHTER)
							this._bHasRetreated = true;
						this.Active = false;
						return true;
					}
					if (str == "HitByNodeCannon")
					{
						this._bHitByNodeCannon = true;
						this.Active = false;
						return true;
					}
					if (str == "DisablePolicePatrol")
					{
						this._isPolicePatrolling = false;
						if (this._taskGroup != null)
						{
							this._taskGroup.RemoveShip(this);
							this._taskGroup = (TaskGroup)null;
						}
						return true;
					}
					if (str == "ActivateDefenseBoat")
					{
						this._defenseBoatActive = true;
						return true;
					}
					if (str == "SectionKilled")
					{
						int num = message.ReadInteger();
						foreach (Section section in this.Sections)
						{
							if (num == section.ObjectID)
							{
								section.IsAlive = false;
								this.HandleSectionsKilled();
								break;
							}
						}
						return true;
					}
					if (str == "ModuleKilled")
					{
						int num1 = message.ReadInteger();
						int num2 = message.ReadInteger();
						foreach (Module module in this.Modules)
						{
							if (num1 == module.ObjectID)
							{
								module.IsAlive = false;
								module.DestroyedByPlayer = num2;
								this.HandleModuleKilled();
								break;
							}
						}
						return true;
					}
					if (str == "DisableTurrets")
					{
						int sectID = message.ReadInteger();
						bool flag = message.ReadBool();
						foreach (IGameObject state in this.Turrets.Where<Turret>((Func<Turret, bool>)(x => x.ParentID == sectID)))
							state.PostSetProp("Disable", flag);
						return true;
					}
					if (str == "KillShipNoExplode")
					{
						this.DisableManeuvering(true);
						foreach (IGameObject turret in this.Turrets)
							turret.PostSetProp("Disable", true);
						this._bIsDestroyed = true;
						return true;
					}
					if (str == "UpdatePsiPower")
					{
						this._currentPsiPower = message.ReadInteger();
						return true;
					}
					break;
				case InteropMessageID.IMID_SCRIPT_OBJECT_SETPLAYER:
					int id = message.ReadInteger();
					bool flag1 = message.ReadBool();
					this.Player = this.App.GetGameObject<Player>(id);
					if (flag1)
						this.DisableManeuvering(true);
					return true;
				default:
					App.Log.Warn("Unhandled message (id=" + (object)interopMessageId + ").", "design");
					break;
			}
			return false;
		}

		private void HandleActivatedDeadObjects(List<int> ids)
		{
			List<IGameObject> objs = new List<IGameObject>();
			foreach (int id1 in ids)
			{
				int id = id1;
				IGameObject gameObject = this._objects.FirstOrDefault<IGameObject>((Func<IGameObject, bool>)(x => x.ObjectID == id));
				if (gameObject != null)
				{
					if (gameObject is Section)
						(gameObject as Section).IsAlive = false;
					else if (gameObject is Module)
					{
						(gameObject as Module).IsAlive = false;
						Section attachedSection = (gameObject as Module).AttachedSection;
						if (attachedSection == null || !attachedSection.IsAlive)
							objs.Add(gameObject);
					}
					else if (gameObject is MountObject)
						objs.Add(gameObject);
					else if (gameObject is Shield)
						objs.Add(gameObject);
				}
			}
			this.HandleSectionsKilled();
			this.HandleModuleKilled();
			this.RemoveObjects(objs);
		}

		private void HandleSectionsKilled()
		{
			float val1_1 = 0.0f;
			Vector3 zero = Vector3.Zero;
			float val1_2 = 0.0f;
			float val1_3 = 0.0f;
			float num1 = 0.0f;
			int num2 = 0;
			int num3 = 0;
			foreach (Section section in this.Sections)
			{
				ShipSectionAsset tag = section.GetTag<ShipSectionAsset>();
				if (section.IsAlive)
				{
					++num2;
					val1_1 += tag.Maneuvering.LinearAccel;
					zero.X += tag.Maneuvering.RotAccel.X;
					zero.Y += tag.Maneuvering.RotAccel.Y;
					zero.Z += tag.Maneuvering.RotAccel.Z;
					val1_2 += tag.Maneuvering.LinearSpeed;
					val1_3 += tag.Maneuvering.RotationSpeed;
				}
				else
				{
					++num3;
					val1_1 += ((double)tag.Maneuvering.LinearAccel < 0.0 ? 1.1f : 0.1f) * tag.Maneuvering.LinearAccel;
					zero.X += ((double)tag.Maneuvering.RotAccel.X < 0.0 ? 1.1f : 0.1f) * tag.Maneuvering.RotAccel.X;
					zero.Y += ((double)tag.Maneuvering.RotAccel.Y < 0.0 ? 1.1f : 0.1f) * tag.Maneuvering.RotAccel.Y;
					zero.Z += ((double)tag.Maneuvering.RotAccel.Z < 0.0 ? 1.1f : 0.1f) * tag.Maneuvering.RotAccel.Z;
					val1_2 += ((double)tag.Maneuvering.LinearSpeed < 0.0 ? 1.1f : 0.1f) * tag.Maneuvering.LinearSpeed;
					val1_3 += ((double)tag.Maneuvering.RotationSpeed < 0.0 ? 1.1f : 0.1f) * tag.Maneuvering.RotationSpeed;
				}
				num1 += tag.Maneuvering.Deacceleration;
				if (tag.Type == ShipSectionType.Engine && !section.IsAlive)
					this.IsDriveless = true;
			}
			float num4 = (double)num1 >= 1.0 ? num1 : 2f;
			zero.X = Math.Max(zero.X, 5f);
			zero.Y = Math.Max(zero.Y, 5f);
			zero.Z = Math.Max(zero.Z, 5f);
			this.Maneuvering.PostSetProp("SetManeuveringInfo", (object)Math.Max(val1_1, 10f), (object)zero, (object)Math.Max(val1_2, 10f), (object)Math.Max(val1_3, 5f), (object)num4, (object)this._bIsDriveless);
			List<IGameObject> objs = new List<IGameObject>();
			foreach (TurretBase turretBase in this.TurretBases)
			{
				if (turretBase.AttachedSection != null && !turretBase.AttachedSection.IsAlive)
					objs.Add((IGameObject)turretBase);
			}
			this.RemoveObjects(objs);
		}

		private void HandleModuleKilled()
		{
			List<IGameObject> objs = new List<IGameObject>();
			foreach (TurretBase turretBase in this.TurretBases)
			{
				if (turretBase.AttachedSection != null && !turretBase.AttachedSection.IsAlive || turretBase.AttachedModule != null && !turretBase.AttachedModule.IsAlive)
					objs.Add((IGameObject)turretBase);
			}
			this.RemoveObjects(objs);
		}

		private void DisableManeuvering(bool disable)
		{
			this.Maneuvering.PostSetProp("SetDriveless", disable);
			this.IsDriveless = disable;
		}

		public bool IsDetected(Player player)
		{
			if (this._detectionStates == null || this._detectionStates.Count == 0)
				return true;
			foreach (Ship.DetectionState detectionState in this._detectionStates)
			{
				if (detectionState.playerID == player.ID)
					return detectionState.scanned;
			}
			return false;
		}

		public bool IsVisible(Player player)
		{
			foreach (Ship.DetectionState detectionState in this._detectionStates)
			{
				if (detectionState.playerID == player.ID)
					return detectionState.spotted;
			}
			return false;
		}

		public Player Player
		{
			get
			{
				return this._player;
			}
			set
			{
				this.PostSetPlayer(value.ObjectID);
				this._player = value;
			}
		}

		public bool Visible
		{
			get
			{
				return this._visible;
			}
			set
			{
				this.PostSetProp("SetVisible", value);
				this._visible = value;
			}
		}

		public CombatStance CombatStance
		{
			get
			{
				return this._stance;
			}
		}

		public void SetCombatStance(CombatStance stance)
		{
			this.PostSetProp(nameof(SetCombatStance), (int)stance);
			this._stance = stance;
		}

		public int CurrentPsiPower
		{
			get
			{
				return this._currentPsiPower;
			}
		}

		public int MaxPsiPower
		{
			get
			{
				return this._maxPsiPower;
			}
		}

		public float SensorRange
		{
			get
			{
				return this._sensorRange;
			}
		}

		public float BonusSpottedRange
		{
			get
			{
				return this._bonusSpottedRange;
			}
		}

		public CloakedState CloakedState
		{
			get
			{
				return this._cloakedState;
			}
		}

		public void SetCloaked(bool cloaked)
		{
			this.PostSetProp(nameof(SetCloaked), cloaked);
		}

		public int DatabaseID
		{
			get
			{
				return this._databaseID;
			}
		}

		public int DesignID
		{
			get
			{
				return this._designID;
			}
		}

		public int ReserveSize
		{
			get
			{
				return this._reserveSize;
			}
		}

		public int RiderIndex
		{
			get
			{
				return this._riderIndex;
			}
		}

		public ShipClass ShipClass
		{
			get
			{
				return this._shipClass;
			}
		}

		public RealShipClasses RealShipClass
		{
			get
			{
				return this._realShipClass;
			}
		}

		public int GetAddedResourcesAsTaskObjective()
		{
			switch (this._combatAI)
			{
				case SectionEnumerations.CombatAiType.SwarmerHive:
					return 50;
				case SectionEnumerations.CombatAiType.SwarmerQueenLarva:
					return 50;
				case SectionEnumerations.CombatAiType.SwarmerQueen:
					return 100;
				case SectionEnumerations.CombatAiType.VonNeumannPlanetKiller:
					return 300;
				case SectionEnumerations.CombatAiType.CommandMonitor:
					return 50;
				default:
					return 0;
			}
		}

		public int GetCruiserEquivalent()
		{
			switch (this._shipClass)
			{
				case ShipClass.Cruiser:
					return 1;
				case ShipClass.Dreadnought:
					return 3;
				case ShipClass.Leviathan:
					return 9;
				case ShipClass.BattleRider:
					return 0;
				case ShipClass.Station:
					return 27;
				default:
					return 0;
			}
		}

		public ShipRole ShipRole
		{
			get
			{
				return this._shipRole;
			}
		}

		public SectionEnumerations.CombatAiType CombatAI
		{
			get
			{
				return this._combatAI;
			}
		}

		public ShipFleetAbilityType AbilityType
		{
			get
			{
				return this._abilityType;
			}
		}

		public BattleRiderTypes BattleRiderType
		{
			get
			{
				return this._battleRiderType;
			}
		}

		public float Signature
		{
			get
			{
				return this._signature;
			}
			set
			{
				this._signature = value;
			}
		}

		public Ship.DetectionState GetDetectionStateForPlayer(int playerID)
		{
			if (this._detectionStates == null)
				this._detectionStates = new List<Ship.DetectionState>();
			foreach (Ship.DetectionState detectionState in this._detectionStates)
			{
				if (detectionState.playerID == playerID)
					return detectionState;
			}
			Ship.DetectionState detectionState1 = new Ship.DetectionState(playerID);
			this._detectionStates.Add(detectionState1);
			return detectionState1;
		}

		public Turret GetTurretWithWeaponTrait(WeaponEnums.WeaponTraits trait)
		{
			foreach (Turret turret in this.Turrets)
			{
				if (((IEnumerable<WeaponEnums.WeaponTraits>)turret.Weapon.Traits).Any<WeaponEnums.WeaponTraits>((Func<WeaponEnums.WeaponTraits, bool>)(x => x == trait)))
					return turret;
			}
			return (Turret)null;
		}

		public WeaponBank GetWeaponBankWithWeaponTrait(WeaponEnums.WeaponTraits trait)
		{
			foreach (WeaponBank weaponBank in this.WeaponBanks)
			{
				if (((IEnumerable<WeaponEnums.WeaponTraits>)weaponBank.Weapon.Traits).Any<WeaponEnums.WeaponTraits>((Func<WeaponEnums.WeaponTraits, bool>)(x => x == trait)))
					return weaponBank;
			}
			return (WeaponBank)null;
		}

		public int InputID
		{
			get
			{
				return this._inputId;
			}
		}

		public IGameObject Target
		{
			get
			{
				return this._target;
			}
			set
			{
				this._target = value;
			}
		}

		public bool BlindFireActive
		{
			get
			{
				return this._blindFireActive;
			}
			set
			{
				this._blindFireActive = value;
			}
		}

		public TaskGroup TaskGroup
		{
			get
			{
				return this._taskGroup;
			}
			set
			{
				this._taskGroup = value;
			}
		}

		public bool IsPlayerControlled
		{
			get
			{
				return this._isPlayerControlled;
			}
			set
			{
				if (this._isPlayerControlled != value && value && this._taskGroup != null)
				{
					this._taskGroup.RemoveShip(this);
					this._taskGroup = (TaskGroup)null;
				}
				this._isPlayerControlled = value;
			}
		}

		public WeaponRole WeaponRole
		{
			get
			{
				return this._wpnRole;
			}
		}

		public string PriorityWeaponName
		{
			get
			{
				return this._priorityWeapon;
			}
		}

		public bool IsDriveless
		{
			get
			{
				return this._bIsDriveless;
			}
			set
			{
				this._bIsDriveless = value;
				if (!value || this.TaskGroup == null)
					return;
				this.TaskGroup.RemoveShip(this);
			}
		}

		public bool IsDestroyed
		{
			get
			{
				return this._bIsDestroyed;
			}
		}

		public bool IsNeutronStar
		{
			get
			{
				return this._bIsNeutronStar;
			}
		}

		public bool HasRetreated
		{
			get
			{
				return this._bHasRetreated;
			}
		}

		public bool HitByNodeCannon
		{
			get
			{
				return this._bHitByNodeCannon;
			}
		}

		public static bool IsActiveShip(Ship ship)
		{
			if (ship != null && !ship.IsDestroyed && (!ship.DockedWithParent && !ship.HasRetreated) && !ship.HitByNodeCannon)
				return !ship.IsNeutronStar;
			return false;
		}

		public bool InstantlyKilled
		{
			get
			{
				return this._instantlyKilled;
			}
		}

		public bool CanAcceptMoveOrders
		{
			get
			{
				return this._bCanAcceptMoveOrders;
			}
		}

		public bool DockedWithParent
		{
			get
			{
				return this._bDockedWithParent;
			}
		}

		public bool CarrierCanLaunch
		{
			get
			{
				return this._bCarrierCanLaunch;
			}
		}

		public bool IsPlanetAssaultShip
		{
			get
			{
				return this._bIsPlanetAssaultShip;
			}
		}

		public bool AssaultingPlanet
		{
			get
			{
				return this._bAssaultingPlanet;
			}
		}

		public bool CanAvoid
		{
			get
			{
				return this._bCanAvoid;
			}
			set
			{
				if (value != this._bCanAvoid)
					this.Maneuvering.PostSetProp(nameof(CanAvoid), value);
				this._bCanAvoid = value;
			}
		}

		public bool Authoritive
		{
			get
			{
				return this._bAuthoritive;
			}
			set
			{
				if (value != this._bAuthoritive)
				{
					foreach (IGameObject turret in this.Turrets)
						turret.PostSetProp("IsAutoritive", value);
				}
				this._bAuthoritive = value;
			}
		}

		public class DetectionState
		{
			public int playerID;
			public bool spotted;
			public bool scanned;

			public DetectionState(int player_id)
			{
				this.playerID = player_id;
				this.spotted = false;
				this.scanned = false;
			}
		}
	}
}
