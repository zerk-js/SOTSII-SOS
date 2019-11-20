// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.ComparativeAnalysysState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Combat;
using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Data.WeaponFramework;
using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Kerberos.Sots.GameStates
{
	internal class ComparativeAnalysysState : GameState, IKeyBindListener
	{
		private static readonly bool _defaultShowDebugControls = true;
		private static readonly string[] _debugAnimTracks = new string[3]
		{
	  "idle",
	  "combat_ready",
	  "combat_unready"
		};
		private static readonly string[] _debugAnimMode = new string[3]
		{
	  "once",
	  "loop",
	  "hold"
		};
		private static readonly string DebugFactionsList = "debugFactionsList";
		private static readonly string DebugEmpireColorList = "debugEmpireColors";
		private static readonly string DebugShipColor = "debugShipColor";
		private static readonly string DebugAnimEdit = "debugAnimEdit";
		private static readonly string DebugAnimTracks = "debugAnimTracks";
		private static readonly string DebugAnimMode = "debugAnimMode";
		private static readonly string DebugPlayAnim = "debugPlayAnim";
		private static readonly string DebugStopAnim = "debugStopAnim";
		private static Dictionary<string, ComparativeAnalysysState.BankFilter> BankFilters = new Dictionary<string, ComparativeAnalysysState.BankFilter>()
	{
	  {
		"filterAll",
		new ComparativeAnalysysState.BankFilter()
		{
		  Value = (object) 0,
		  Enabled = false
		}
	  },
	  {
		"filterVeryLight",
		new ComparativeAnalysysState.BankFilter()
		{
		  Value = (object) WeaponEnums.WeaponSizes.VeryLight,
		  Enabled = false
		}
	  },
	  {
		"filterLight",
		new ComparativeAnalysysState.BankFilter()
		{
		  Value = (object) WeaponEnums.WeaponSizes.Light,
		  Enabled = false
		}
	  },
	  {
		"filterMedium",
		new ComparativeAnalysysState.BankFilter()
		{
		  Value = (object) WeaponEnums.WeaponSizes.Medium,
		  Enabled = false
		}
	  },
	  {
		"filterHeavy",
		new ComparativeAnalysysState.BankFilter()
		{
		  Value = (object) WeaponEnums.WeaponSizes.Heavy,
		  Enabled = false
		}
	  },
	  {
		"filterVeryHeavy",
		new ComparativeAnalysysState.BankFilter()
		{
		  Value = (object) WeaponEnums.WeaponSizes.VeryHeavy,
		  Enabled = false
		}
	  },
	  {
		"filterSuperHeavy",
		new ComparativeAnalysysState.BankFilter()
		{
		  Value = (object) WeaponEnums.WeaponSizes.SuperHeavy,
		  Enabled = false
		}
	  },
	  {
		"filterModules",
		new ComparativeAnalysysState.BankFilter()
		{
		  Value = (object) 0,
		  Enabled = false
		}
	  }
	};
		private static bool FilterVisible = true;
		private bool _showDebugControls = ComparativeAnalysysState._defaultShowDebugControls;
		private List<Faction> _factionsList = new List<Faction>();
		private int? _selectedDesign = new int?();
		private string _selectedDesignPW = "";
		private string _debugCurrentAnimTrack = string.Empty;
		private Dictionary<ShipSectionType, List<Dictionary<string, bool>>> _shipOptionGroups = new Dictionary<ShipSectionType, List<Dictionary<string, bool>>>();
		private const string UICommitButton = "gameCommitButton";
		private const string UIExitButton = "gameExitButton";
		private const string UIDesignRemovedButton = "designRemove";
		private const string UIModuleSelectorPanel = "gameModuleSelector";
		private const string UIModuleList = "gameModuleList";
		private const string UIWeaponDesignSelectorPanel = "gameDesignSelector";
		private const string UIWeaponDesignList = "gameWeaponDesignList";
		private const string UIClassList = "gameClassList";
		private const string UIFactionList = "gamePlayerSelect";
		private const string UIDesignList = "gameDesignList";
		private const string UICommandSectionList = "gameCommandList";
		private const string UICommandPanel = "CommandDesign";
		private const string UIMissionPanel = "MissionDesign";
		private const string UIEnginePanel = "EngineDesign";
		private const string UIMissionSectionList = "gameMissionList";
		private const string UIEngineSectionList = "gameEngineList";
		private const string UIFilterAll = "filterAll";
		private const string UIFilterVeryLight = "filterVeryLight";
		private const string UIFilterLight = "filterLight";
		private const string UIFilterMedium = "filterMedium";
		private const string UIFilterHeavy = "filterHeavy";
		private const string UIFilterVeryHeavy = "filterVeryHeavy";
		private const string UIFilterSuperHeavy = "filterSuperHeavy";
		private const string UIFilterModules = "filterModules";
		private const string UIFiltersToggle = "expandFilters";
		private const string UIFilterPanel = "BankFilter";
		private const string UISpecialList = "specialList";
		private ComparativeAnalysysState.CombatShipQueue _combatShipQueue;
		private GameObjectSet _crits;
		private OrbitCameraController _camera;
		private CombatInput _input;
		private AllShipData _selection;
		private ShipBuilder _builder;
		private Vector3 _playerColour1;
		private Vector3 _playerColour2;
		private Sky _sky;
		private TargetArena _targetArena;
		private bool _inWeaponTestScreen;
		private ShipHoloView _shipHoloView;
		private string _previousState;
		private bool _swappedShip;
		private bool _shouldRefresh;
		private bool _updateCamTarget;
		private WeaponSelector _weaponSelector;
		private ModuleSelector _moduleSelector;
		private PsionicSelector _psionicSelector;
		private string _designName;
		private string _originalName;
		private int _deleteItemID;
		private string _deleteItemDialog;
		private int _PsionicIndex;
		private bool _RetrofitMode;
		private int _SelectedPlayer;
		private int _debugCurrentAnimMode;
		private List<DesignInfo> EncounteredDesigns;
		private ModuleShipData _selectedModule;
		private IWeaponShipData _selectedWeaponBank;
		private bool _currentShipDirty;
		private bool _screenReady;

		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}

		private ComparativeAnalysysState.CombatShipQueue GuaranteedCombatShipQueue
		{
			get
			{
				if (this._combatShipQueue == null)
					this._combatShipQueue = new ComparativeAnalysysState.CombatShipQueue();
				return this._combatShipQueue;
			}
		}

		private static bool DesignScreenAllowsShipClass(ShipSectionAsset value)
		{
			if (value.CombatAIType == SectionEnumerations.CombatAiType.TrapDrone)
				return false;
			switch (value.RealClass)
			{
				case RealShipClasses.BoardingPod:
				case RealShipClasses.EscapePod:
				case RealShipClasses.Biomissile:
				case RealShipClasses.Station:
					return false;
				default:
					return true;
			}
		}

		protected override void OnPrepare(GameState prev, object[] stateParams)
		{
			if (this.App.LocalPlayer == null)
			{
				this.App.NewGame();
				if (!this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID).Homeworld.HasValue)
					throw new ArgumentException("Design screen requires a home world.");
				this._showDebugControls = true;
			}
			else if (stateParams.Length == 2)
			{
				this._previousState = stateParams[1] as string;
				this._showDebugControls = (bool)stateParams[0];
			}
			else
				this._showDebugControls = true;
			this._targetArena = new TargetArena(this.App, "human");
			this._crits = new GameObjectSet(this.App);
			this._sky = new Sky(this.App, SkyUsage.InSystem, 0);
			this._crits.Add((IGameObject)this._sky);
			this._camera = this._crits.Add<OrbitCameraController>((object)string.Empty);
			this._shipHoloView = new ShipHoloView(this.App, this._camera);
			this._shipHoloView.PostSetProp("AutoFitToView", false);
			this._crits.Add((IGameObject)this._shipHoloView);
			this._input = this._crits.Add<CombatInput>((object)string.Empty);
			this._input.PlayerId = this.App.LocalPlayer.ObjectID;
			this._input.PostSetProp("DisableMouseTimer", true);
			this._input.PostSetProp("DisableDragSelect", true);
			this._input.PostSetProp("DisableUserUnitSelect", true);
			this._input.PostSetProp("DisablePsiBar", true);
			this._input.PostSetProp("WeaponLauncher", 0);
			this._selection = new AllShipData();
			this._builder = new ShipBuilder(this.App);
			this._playerColour1 = new Vector3((float)byte.MaxValue, 55f, 55f);
			this._playerColour2 = new Vector3(55f, 55f, (float)byte.MaxValue);
			this.App.UI.LoadScreen("ComparativeAnalysys");
			this.App.UI.LoadScreen("DesignWeaponTest");
			this.App.Game.AvailableShipSectionsChanged();
			this._input.CameraID = this._camera.ObjectID;
			this._camera.MaxDistance = 2000f;
			this._camera.DesiredDistance = 200f;
			this._camera.DesiredYaw = 90f;
			this._camera.YawEnabled = false;
			this._factionsList.Clear();
			this._factionsList.AddRange(this.App.AssetDatabase.Factions);
			this.EncounteredDesigns = this.App.GameDatabase.GetDesignsEncountered(this.App.LocalPlayer.ID).ToList<DesignInfo>();
			if (this._showDebugControls)
			{
				List<ShipSectionAsset> source1 = new List<ShipSectionAsset>(this.App.Game.GetAvailableShipSections(this.App.LocalPlayer.ID));
				foreach (ShipSectionAsset shipSection in this.App.AssetDatabase.ShipSections)
				{
					ShipSectionAsset ssa = shipSection;
					if (!source1.Any<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == ssa.FileName)))
						this.App.GameDatabase.InsertSectionAsset(ssa.FileName, this.App.LocalPlayer.ID);
				}
				List<LogicalWeapon> source2 = new List<LogicalWeapon>(this.App.GameDatabase.GetAvailableWeapons(this.App.AssetDatabase, this.App.LocalPlayer.ID));
				foreach (LogicalWeapon weapon in this.App.AssetDatabase.Weapons)
				{
					LogicalWeapon lw = weapon;
					if (!source2.Any<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.FileName == lw.FileName)))
						this.App.GameDatabase.InsertWeapon(lw, this.App.LocalPlayer.ID);
				}
				List<LogicalModule> source3 = new List<LogicalModule>(this.App.GameDatabase.GetAvailableModules(this.App.AssetDatabase, this.App.LocalPlayer.ID));
				foreach (LogicalModule module in this.App.AssetDatabase.Modules)
				{
					LogicalModule lm = module;
					if (!source3.Any<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == lm.ModulePath)))
						this.App.GameDatabase.InsertModule(lm, this.App.LocalPlayer.ID);
				}
			}
			IEnumerable<Faction> factions = this.App.AssetDatabase.Factions;
			List<ShipSectionAsset> shipSectionAssetList = new List<ShipSectionAsset>();
			foreach (PlayerInfo standardPlayerInfo in this.App.GameDatabase.GetStandardPlayerInfos())
			{
				PlayerInfo player = standardPlayerInfo;
				if (this.EncounteredDesigns.Any<DesignInfo>((Func<DesignInfo, bool>)(x =>
			   {
				   if (x.PlayerID == player.ID)
					   return ((IEnumerable<DesignSectionInfo>)x.DesignSections).Any<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(y => ComparativeAnalysysState.DesignScreenAllowsShipClass(y.ShipSectionAsset)));
				   return false;
			   })))
					shipSectionAssetList.AddRange(this.App.Game.GetAvailableShipSections(player.ID).Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => ComparativeAnalysysState.DesignScreenAllowsShipClass(x))));
			}
			foreach (ShipSectionAsset section in shipSectionAssetList)
				this.CollectWeapons(section);
		}

		private void CollectWeapons(ShipSectionAsset section)
		{
			IEnumerable<Faction> factions = this.App.AssetDatabase.Factions;
			FactionShipData t = this._selection.Factions.FirstOrDefault<FactionShipData>((Func<FactionShipData, bool>)(x => x.Faction.Name == section.Faction));
			if (t == null)
			{
				t = new FactionShipData()
				{
					Faction = factions.First<Faction>((Func<Faction, bool>)(x => x.Name == section.Faction))
				};
				this._selection.Factions.Add(t);
			}
			ClassShipData classShipData = t.Classes.FirstOrDefault<ClassShipData>((Func<ClassShipData, bool>)(x => x.Class == section.RealClass));
			if (classShipData == null)
			{
				classShipData = new ClassShipData()
				{
					Class = section.RealClass
				};
				t.Classes.Add(classShipData);
			}
			SectionTypeShipData sectionTypeShipData = classShipData.SectionTypes.FirstOrDefault<SectionTypeShipData>((Func<SectionTypeShipData, bool>)(x => x.SectionType == section.Type));
			if (sectionTypeShipData == null)
			{
				sectionTypeShipData = new SectionTypeShipData()
				{
					SectionType = section.Type
				};
				classShipData.SectionTypes.Add(sectionTypeShipData);
			}
			SectionShipData sectionShipData = new SectionShipData();
			sectionShipData.Section = section;
			for (int index = 0; index < section.Banks.Length; ++index)
			{
				LogicalBank bank = section.Banks[index];
				WeaponBankShipData weaponBankShipData = new WeaponBankShipData();
				weaponBankShipData.Section = sectionShipData;
				weaponBankShipData.BankIndex = index;
				weaponBankShipData.Bank = bank;
				if (!string.IsNullOrEmpty(bank.DefaultWeaponName))
					weaponBankShipData.SelectedWeapon = this.App.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => string.Equals(x.WeaponName, bank.DefaultWeaponName, StringComparison.InvariantCultureIgnoreCase)));
				this.CollectWeapons(section, (IWeaponShipData)weaponBankShipData, bank.TurretSize, bank.TurretClass, weaponBankShipData.SelectedWeapon);
				sectionShipData.WeaponBanks.Add(weaponBankShipData);
			}
			for (int sectionModuleMountIndex = 0; sectionModuleMountIndex < section.Modules.Length; ++sectionModuleMountIndex)
			{
				ModuleShipData moduleShipData = new ModuleShipData();
				moduleShipData.Section = sectionShipData;
				moduleShipData.ModuleIndex = sectionModuleMountIndex;
				moduleShipData.ModuleMount = section.Modules[sectionModuleMountIndex];
				foreach (LogicalModule logicalModule in (IEnumerable<LogicalModule>)LogicalModule.EnumerateModuleFits(this.App.GameDatabase.GetAvailableModules(this.App.AssetDatabase, this._SelectedPlayer), section, sectionModuleMountIndex, this._showDebugControls).ToList<LogicalModule>())
				{
					LogicalModule module = logicalModule;
					ModuleData moduleData = new ModuleData();
					moduleData.Module = module;
					moduleData.SelectedPsionic = new List<SectionEnumerations.PsionicAbility>();
					for (int index = 0; index < module.NumPsionicSlots; ++index)
						moduleData.SelectedPsionic.Add(SectionEnumerations.PsionicAbility.None);
					if (module.Banks.Length > 0)
					{
						if (!string.IsNullOrEmpty(module.Banks[0].DefaultWeaponName))
							moduleData.SelectedWeapon = this.App.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => string.Equals(x.WeaponName, module.Banks[0].DefaultWeaponName, StringComparison.InvariantCultureIgnoreCase)));
						this.CollectWeapons(section, (IWeaponShipData)moduleData, module.Banks[0].TurretSize, module.Banks[0].TurretClass, moduleData.SelectedWeapon);
					}
					moduleShipData.Modules.Add(moduleData);
				}
				if (moduleShipData.Modules.Count > 0)
					sectionShipData.Modules.Add(moduleShipData);
			}
			sectionTypeShipData.Sections.Add(sectionShipData);
			if (sectionTypeShipData.Sections.Count <= 0)
				return;
			sectionTypeShipData.SelectedSection = sectionTypeShipData.Sections[0];
		}

		private void RepopulateDebugControls()
		{
			if (!this._showDebugControls)
				return;
			this.App.UI.ClearItems(ComparativeAnalysysState.DebugEmpireColorList);
			for (int index = 0; index < Player.DefaultPrimaryPlayerColors.Count; ++index)
			{
				Vector3 primaryPlayerColor = Player.DefaultPrimaryPlayerColors[index];
				this.App.UI.Send((object)"AddItem", (object)ComparativeAnalysysState.DebugEmpireColorList, (object)index.ToString(), (object)"colorbox");
				this.App.UI.Send((object)"SetItemColor", (object)ComparativeAnalysysState.DebugEmpireColorList, (object)index.ToString(), (object)primaryPlayerColor.X, (object)primaryPlayerColor.Y, (object)primaryPlayerColor.Z);
			}
			this.App.UI.ClearItems(ComparativeAnalysysState.DebugAnimTracks);
			for (int userItemId = 0; userItemId < ComparativeAnalysysState._debugAnimTracks.Length; ++userItemId)
				this.App.UI.AddItem(ComparativeAnalysysState.DebugAnimTracks, string.Empty, userItemId, ComparativeAnalysysState._debugAnimTracks[userItemId]);
			this.SetSelectedAnimTrack(ComparativeAnalysysState._debugAnimTracks[0], string.Empty);
			this.App.UI.ClearItems(ComparativeAnalysysState.DebugAnimMode);
			for (int userItemId = 0; userItemId < ComparativeAnalysysState._debugAnimMode.Length; ++userItemId)
				this.App.UI.AddItem(ComparativeAnalysysState.DebugAnimMode, string.Empty, userItemId, ComparativeAnalysysState._debugAnimMode[userItemId]);
			this.App.UI.SetSelection(ComparativeAnalysysState.DebugAnimMode, 0);
		}

		private bool CanWeaponUseDesign(
		  ShipSectionAsset section,
		  WeaponEnums.TurretClasses turretClass,
		  DesignInfo design)
		{
			RealShipClasses correspondingShipClass;
			if (!WeaponEnums.RequiresDesign(turretClass, out correspondingShipClass) || section.IsWraithAbductor)
				return false;
			foreach (DesignSectionInfo designSection in design.DesignSections)
			{
				ShipSectionAsset shipSectionAsset = this.App.AssetDatabase.GetShipSectionAsset(designSection.FilePath);
				if ((!section.IsScavenger || design.Role == ShipRole.SLAVEDISK) && (section.IsScavenger || design.Role != ShipRole.SLAVEDISK) && (shipSectionAsset != null && shipSectionAsset.RealClass == correspondingShipClass))
					return true;
			}
			return false;
		}

		private IEnumerable<int> CollectWeaponDesigns(
		  ShipSectionAsset section,
		  WeaponEnums.TurretClasses turretClass)
		{
			IEnumerable<DesignInfo> designs = this.App.GameDatabase.GetDesignInfosForPlayer(this._SelectedPlayer);
			foreach (DesignInfo design in designs)
			{
				if (this.CanWeaponUseDesign(section, turretClass, design))
					yield return design.ID;
			}
		}

		private void CollectWeapons(
		  ShipSectionAsset section,
		  IWeaponShipData weaponBank,
		  WeaponEnums.WeaponSizes turretSize,
		  WeaponEnums.TurretClasses turretClass,
		  LogicalWeapon includeSelectedWeapon)
		{
			if (WeaponEnums.IsBattleRider(turretClass))
			{
				LogicalWeapon logicalWeapon = LogicalWeapon.EnumerateWeaponFits(section.Faction, section.SectionName, this.App.GameDatabase.GetAvailableWeapons(this.App.AssetDatabase, this._SelectedPlayer).Where<LogicalWeapon>((Func<LogicalWeapon, bool>)(weapon => weapon.IsVisible)), turretSize, turretClass).FirstOrDefault<LogicalWeapon>();
				if (logicalWeapon != null && !section.IsWraithAbductor)
				{
					weaponBank.RequiresDesign = turretClass != WeaponEnums.TurretClasses.Biomissile;
					weaponBank.DesignIsSelectable = WeaponEnums.DesignIsSelectable(turretClass);
					if (turretClass == WeaponEnums.TurretClasses.Biomissile)
					{
						IEnumerable<LogicalWeapon> collection = LogicalWeapon.EnumerateWeaponFits(section.Faction, section.SectionName, this.App.GameDatabase.GetAvailableWeapons(this.App.AssetDatabase, this._SelectedPlayer).Where<LogicalWeapon>((Func<LogicalWeapon, bool>)(weapon => weapon.IsVisible)), turretSize, turretClass);
						weaponBank.Weapons.AddRange(collection);
					}
					else
						weaponBank.Weapons.Add(logicalWeapon);
					weaponBank.Designs.AddRange(this.CollectWeaponDesigns(section, turretClass));
				}
			}
			else
			{
				IEnumerable<LogicalWeapon> collection = LogicalWeapon.EnumerateWeaponFits(section.Faction, section.SectionName, this.App.GameDatabase.GetAvailableWeapons(this.App.AssetDatabase, this._SelectedPlayer).Where<LogicalWeapon>((Func<LogicalWeapon, bool>)(weapon => weapon.IsVisible)), turretSize, turretClass);
				weaponBank.Weapons.AddRange(collection);
			}
			if (includeSelectedWeapon != null && !weaponBank.Weapons.Contains(includeSelectedWeapon))
				weaponBank.Weapons.Add(includeSelectedWeapon);
			if (weaponBank.Weapons.Count > 0)
				weaponBank.SelectedWeapon = weaponBank.Weapons[0];
			if (weaponBank.Designs.Count <= 0)
				return;
			weaponBank.SelectedDesign = weaponBank.Designs[0];
		}

		private void EnableWeaponTestMode(bool enabled)
		{
			this.App.UI.SetVisible("pnlMainDesign", !enabled);
			this.App.UI.SetVisible("pnlDesignWeaponTest", enabled);
			if (enabled)
				this.App.UI.SetParent("gameWeaponsPanel2", "pnlWeaponsTest");
			else
				this.App.UI.SetParent("gameWeaponsPanel2", "pnlWeapons");
			this.App.UI.SetVisible("psionicArea", false);
		}

		protected override void OnEnter()
		{
			this.App.UI.UnlockUI();
			if (this.App.LocalPlayer == null)
				this.App.NewGame();
			this._weaponSelector = new WeaponSelector(this.App.UI, "gameWeaponSelector", "");
			this._weaponSelector.SelectedWeaponChanged += new WeaponSelectionChangedEventHandler(this.WeaponSelectorSelectedWeaponChanged);
			this._moduleSelector = new ModuleSelector(this.App.UI, "gameModuleSelector", "");
			this._moduleSelector.SelectedModuleChanged += new ModuleSelectionChangedEventHandler(this.ModuleSelectorSelectedModuleChanged);
			this._psionicSelector = new PsionicSelector(this.App.UI, "psionicSelector", "");
			this._psionicSelector.SelectedPsionicChanged += new PsionicSelectionChangedEventHandler(this.PsionicSelectorSelectedPsionicChanged);
			this._targetArena.Activate();
			this._sky.Active = true;
			this._input.Active = true;
			this._camera.Active = true;
			this._input.PostSetProp("DisableCombatInputMouseOver", true);
			this.App.UI.SetScreen("ComparativeAnalysys");
			this._inWeaponTestScreen = false;
			this.App.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
			this.App.UI.SetVisible("gameDebugControls", this._showDebugControls);
			this.App.UI.Send((object)"SetGameObject", (object)"designShip", (object)this._shipHoloView.ObjectID);
			this.App.UI.SetPropertyBool("gameModuleList", "only_user_events", true);
			this.App.UI.SetPropertyBool("gameWeaponDesignList", "only_user_events", true);
			this.App.UI.SetPropertyBool(ComparativeAnalysysState.UISectionList(ShipSectionType.Command), "only_user_events", true);
			this.App.UI.SetPropertyBool(ComparativeAnalysysState.UISectionList(ShipSectionType.Engine), "only_user_events", true);
			this.App.UI.SetPropertyBool(ComparativeAnalysysState.UISectionList(ShipSectionType.Mission), "only_user_events", true);
			this._RetrofitMode = false;
			EmpireBarUI.SyncTitleFrame(this.App);
			this.PopulateFactionsList();
			this._SelectedPlayer = this.EncounteredDesigns.First<DesignInfo>().PlayerID;
			this.SetSelectedFaction(this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(this._SelectedPlayer)), "init");
			this.SetSelectedClass(RealShipClasses.Cruiser, "init");
			this.PopulateSectionLists();
			this.PopulateDesignList();
			this.PopulateFactionList();
			this.App.UI.SetEnabled("designRemove", false);
			this._screenReady = true;
			this._shouldRefresh = true;
			this.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_SET_AUTHORITIVE_STATE, (object)true);
			this.App.UI.SetChecked("filterAll", true);
			this.SelectFilter("filterAll", true);
			this.App.UI.Send((object)"EnableScriptMessages", (object)"gameWeaponsPanel2", (object)true);
			this.App.HotKeyManager.AddListener((IKeyBindListener)this);
		}

		private void WeaponSelectorSelectedWeaponChanged(object sender, bool isRightClick)
		{
			WeaponAssignment weaponAssignment1 = this._selection.GetWeaponAssignments().FirstOrDefault<WeaponAssignment>((Func<WeaponAssignment, bool>)(x => x.Weapon == this._weaponSelector.SelectedWeapon));
			int? fireMode = new int?();
			int? filterMode = new int?();
			if (weaponAssignment1 != null)
			{
				fireMode = weaponAssignment1.InitialFireMode;
				filterMode = weaponAssignment1.InitialTargetFilter;
			}
			IWeaponShipData selectedBank = this.GetWeaponSelectorBank();
			if (selectedBank.SelectedWeapon != this._weaponSelector.SelectedWeapon)
			{
				if (selectedBank != null)
				{
					selectedBank.SelectedWeapon = this._weaponSelector.SelectedWeapon;
					selectedBank.FiringMode = fireMode;
					selectedBank.FilterMode = filterMode;
					WeaponAssignment weaponAssignment2 = this._selection.GetWeaponAssignments().FirstOrDefault<WeaponAssignment>((Func<WeaponAssignment, bool>)(x => x.Bank == selectedBank.Bank));
					if (weaponAssignment2 != null)
					{
						weaponAssignment2.InitialFireMode = fireMode;
						weaponAssignment2.InitialTargetFilter = filterMode;
					}
				}
				if (isRightClick)
				{
					LogicalBank logicalBank = !(selectedBank is WeaponBankShipData) ? (!(selectedBank is ModuleShipData) ? (LogicalBank)null : ((IEnumerable<LogicalBank>)(selectedBank as ModuleShipData).SelectedModule.Module.Banks).First<LogicalBank>()) : (selectedBank as WeaponBankShipData).Bank;
					if (logicalBank != null)
						this.SetWeaponsAll(this._weaponSelector.SelectedWeapon, logicalBank.TurretSize, logicalBank.TurretClass, fireMode, filterMode);
				}
				if (!this._RetrofitMode)
				{
					this._selectedDesign = new int?();
					this._selectedDesignPW = "";
				}
			}
			this.CurrentShipChanged();
			this.HideWeaponSelector();
		}

		private void PsionicSelectorSelectedPsionicChanged(object sender, bool isRightClick)
		{
			ModuleShipData selectedModule = this._selectedModule;
			if (selectedModule != null)
				selectedModule.SelectedModule.SelectedPsionic[this._PsionicIndex] = !(this._psionicSelector.SelectedPsionic.Name != "No Psionic") || !(this._psionicSelector.SelectedPsionic.Name != "") ? SectionEnumerations.PsionicAbility.None : (SectionEnumerations.PsionicAbility)Enum.Parse(typeof(SectionEnumerations.PsionicAbility), this._psionicSelector.SelectedPsionic.Name);
			if (!this._RetrofitMode)
			{
				this._selectedDesign = new int?();
				this._selectedDesignPW = "";
			}
			this.CurrentShipChanged();
			this.HidePsionicSelector();
		}

		private void ModuleSelectorSelectedModuleChanged(object sender, bool isRightClick)
		{
			ModuleShipData selectedModule1 = this._selectedModule;
			if (selectedModule1 != null)
			{
				LogicalModule selectedModule = this._moduleSelector.SelectedModule;
				if (selectedModule.ModuleName == App.Localize("@UI_MODULENAME_NO_MODULE"))
				{
					selectedModule1.SelectedModule = (ModuleData)null;
					this.CurrentShipChanged();
				}
				else if (selectedModule1.SelectedModule == null || selectedModule != selectedModule1.SelectedModule.Module)
				{
					selectedModule1.SelectedModule = selectedModule1.Modules.First<ModuleData>((Func<ModuleData, bool>)(x => x.Module == selectedModule));
					this.CurrentShipChanged();
				}
			}
			if (!this._RetrofitMode)
			{
				this._selectedDesign = new int?();
				this._selectedDesignPW = "";
			}
			this.HideModuleSelector();
		}

		protected override void OnExit(GameState prev, ExitReason reason)
		{
			this.App.HotKeyManager.RemoveListener((IKeyBindListener)this);
			this._designName = string.Empty;
			if (this._weaponSelector != null)
			{
				this._weaponSelector.Dispose();
				this._weaponSelector = (WeaponSelector)null;
			}
			if (this._psionicSelector != null)
			{
				this._psionicSelector.Dispose();
				this._psionicSelector = (PsionicSelector)null;
			}
			if (this._moduleSelector != null)
			{
				this._moduleSelector.Dispose();
				this._moduleSelector = (ModuleSelector)null;
			}
			if (this._targetArena != null)
			{
				this._targetArena.Dispose();
				this._targetArena = (TargetArena)null;
			}
			this._combatShipQueue = (ComparativeAnalysysState.CombatShipQueue)null;
			this._camera.Active = false;
			this._camera.TargetID = 0;
			this._builder.Dispose();
			this._input.Active = false;
			if (this._crits != null)
			{
				this._crits.Dispose();
				this._crits = (GameObjectSet)null;
			}
			this.App.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
			this._showDebugControls = ComparativeAnalysysState._defaultShowDebugControls;
		}

		protected override void OnUpdate()
		{
			if (this._targetArena != null)
				this._targetArena.Update();
			if (!this._builder.Loading)
			{
				if (!this._swappedShip)
					this.SyncWeaponUi();
				this._swappedShip = true;
			}
			else
				this._swappedShip = false;
			this._builder.Update();
			if (this._builder.Ship != null && this._builder.Ship.Active && (this._camera.TargetID != this._builder.Ship.ObjectID || this._updateCamTarget))
			{
				if (this._camera.TargetID == this._builder.Ship.GetObjectID() && this._updateCamTarget)
					this._camera.PostSetProp("TargetID", this._builder.Ship.GetObjectID());
				else
					this._camera.TargetID = this._builder.Ship.GetObjectID();
				this._updateCamTarget = false;
			}
			this.RefreshCurrentShip();
		}

		public override bool IsReady()
		{
			if (this._crits != null && this._crits.IsReady())
				return base.IsReady();
			return false;
		}

		public ComparativeAnalysysState(App game)
		  : base(game)
		{
		}

		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
			if (!(eventName == "IconClicked"))
				return;
			this.HideModuleSelector();
			this.HideWeaponSelector();
			this.HidePsionicSelector();
			this.CurrentShipChanged();
		}

		private void HideModuleSelector()
		{
			if (this._selectedModule != null)
				this._shipHoloView.ClearSelection();
			this._selectedModule = (ModuleShipData)null;
			this._moduleSelector.SetVisible(false);
		}

		private void ShowModuleSelector(ModuleShipData moduleData)
		{
			bool flag = true;
			if (moduleData != null)
			{
				this._selectedModule = moduleData;
				if (moduleData.Modules.Count > 0)
				{
					this.PopulateModuleSelector(moduleData.Modules);
					flag = false;
				}
			}
			if (!flag)
				return;
			this.HideModuleSelector();
		}

		private void HideWeaponSelector()
		{
			if (this._selectedWeaponBank != null)
				this._shipHoloView.ClearSelection();
			this._selectedWeaponBank = (IWeaponShipData)null;
			this._weaponSelector.SetVisible(false);
			this.App.UI.SetVisible("gameDesignSelector", false);
		}

		private void HidePsionicSelector()
		{
			if (this._selectedModule != null)
				this._shipHoloView.ClearSelection();
			this._selectedModule = (ModuleShipData)null;
			this._psionicSelector.SetVisible(false);
			this.App.UI.SetVisible(this._psionicSelector.ID, false);
		}

		private void PopulateWeaponSelector(
		  List<LogicalWeapon> weapons,
		  LogicalWeapon selected,
		  LogicalBank bank)
		{
			this.App.UI.MovePanelToMouse(this._weaponSelector.ID, UICommChannel.AnchorPoint.TopLeft, new Vector2(-4f, -4f));
			if (this._RetrofitMode && bank != null)
			{
				List<LogicalWeapon> list1 = weapons.Where<LogicalWeapon>((Func<LogicalWeapon, bool>)(x =>
			   {
				   if (((IEnumerable<LogicalTurretClass>)x.TurretClasses).Any<LogicalTurretClass>((Func<LogicalTurretClass, bool>)(j => j.TurretClass == bank.TurretClass)) && x.PayloadType == selected.PayloadType)
					   return x.DefaultWeaponSize == selected.DefaultWeaponSize;
				   return false;
			   })).ToList<LogicalWeapon>();
				if (selected.PayloadType == WeaponEnums.PayloadTypes.Bolt)
				{
					bool islaser = ((IEnumerable<WeaponEnums.WeaponTraits>)selected.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.Laser);
					List<LogicalWeapon> list2 = list1.Where<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => islaser == ((IEnumerable<WeaponEnums.WeaponTraits>)x.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.Laser))).ToList<LogicalWeapon>();
					bool ballistic = ((IEnumerable<WeaponEnums.WeaponTraits>)selected.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.Ballistic);
					list1 = list2.Where<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => ballistic == ((IEnumerable<WeaponEnums.WeaponTraits>)x.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.Ballistic))).ToList<LogicalWeapon>();
				}
				this._weaponSelector.SetAvailableWeapons((IEnumerable<LogicalWeapon>)list1, selected);
			}
			else
				this._weaponSelector.SetAvailableWeapons((IEnumerable<LogicalWeapon>)weapons.OrderBy<LogicalWeapon, WeaponEnums.WeaponSizes>((Func<LogicalWeapon, WeaponEnums.WeaponSizes>)(x => x.DefaultWeaponSize)), selected);
			this._weaponSelector.SetVisible(true);
		}

		private void PopulateModuleSelector(List<ModuleData> modules)
		{
			this.App.UI.MovePanelToMouse(this._moduleSelector.ID, UICommChannel.AnchorPoint.TopLeft, new Vector2(-4f, -4f));
			this._moduleSelector.SetAvailableModules(modules.Select<ModuleData, LogicalModule>((Func<ModuleData, LogicalModule>)(x => x.Module)), this._selectedModule.SelectedModule != null ? this._selectedModule.SelectedModule.Module : (LogicalModule)null, true);
			this._moduleSelector.SetVisible(true);
		}

		private bool PsiNotSelected(LogicalPsionic psionic)
		{
			SectionEnumerations.PsionicAbility psi = (SectionEnumerations.PsionicAbility)Enum.Parse(typeof(SectionEnumerations.PsionicAbility), psionic.Name);
			IEnumerable<ModuleAssignment> moduleAssignments = this._selection.GetModuleAssignments();
			moduleAssignments.ToList<ModuleAssignment>();
			return !moduleAssignments.Any<ModuleAssignment>((Func<ModuleAssignment, bool>)(x => ((IEnumerable<SectionEnumerations.PsionicAbility>)x.PsionicAbilities).Any<SectionEnumerations.PsionicAbility>((Func<SectionEnumerations.PsionicAbility, bool>)(y => y == psi))));
		}

		private void PopulatePsionicSelector(
		  IEnumerable<LogicalPsionic> psionics,
		  LogicalPsionic selected)
		{
			this.App.UI.MovePanelToMouse(this._psionicSelector.ID, UICommChannel.AnchorPoint.TopLeft, new Vector2(-4f, 4f));
			this._psionicSelector.SetAvailablePsionics(psionics.Where<LogicalPsionic>((Func<LogicalPsionic, bool>)(x =>
		   {
			   if (this.PsiNotSelected(x))
				   return x.IsAvailable(this.App.GameDatabase, this._SelectedPlayer, false);
			   return false;
		   })), selected, true);
			this._psionicSelector.SetVisible(true);
		}

		private void PopulateWeaponDesignSelector(List<int> weaponDesigns, int selected)
		{
			this.App.UI.SetVisible("gameDesignSelector", true);
			this.App.UI.ClearItems("gameWeaponDesignList");
			int userItemId1 = -1;
			for (int userItemId2 = 0; userItemId2 != weaponDesigns.Count; ++userItemId2)
			{
				int weaponDesign = weaponDesigns[userItemId2];
				DesignInfo designInfo = this.App.GameDatabase.GetDesignInfo(weaponDesign);
				this.App.UI.AddItem("gameWeaponDesignList", string.Empty, userItemId2, designInfo.Name);
				if (weaponDesign == selected)
					userItemId1 = userItemId2;
			}
			if (userItemId1 == -1)
				return;
			this.App.UI.SetSelection("gameWeaponDesignList", userItemId1);
		}

		private void ShowWeaponSelector(IWeaponShipData weaponBankData)
		{
			bool flag = true;
			if (weaponBankData != null)
			{
				this._selectedWeaponBank = weaponBankData;
				if (weaponBankData.RequiresDesign)
				{
					if (weaponBankData.DesignIsSelectable && weaponBankData.Designs.Count > 0)
					{
						this.PopulateWeaponDesignSelector(weaponBankData.Designs, weaponBankData.SelectedDesign);
						flag = false;
					}
				}
				else if (weaponBankData.Weapons.Count > 0)
				{
					this.PopulateWeaponSelector(weaponBankData.Weapons, weaponBankData.SelectedWeapon, weaponBankData.Bank);
					flag = false;
				}
			}
			if (!flag)
				return;
			this.HideWeaponSelector();
			this.HidePsionicSelector();
		}

		private void ShowPsionicSelector(Module module, int psiindex)
		{
			bool flag = true;
			if (module != null)
				this.PopulatePsionicSelector(this.App.AssetDatabase.Psionics, module._module.Psionics[psiindex]);
			if (flag)
				return;
			this.HidePsionicSelector();
		}

		private static string UISectionList(ShipSectionType sectionType)
		{
			switch (sectionType)
			{
				case ShipSectionType.Command:
					return "gameCommandList";
				case ShipSectionType.Mission:
					return "gameMissionList";
				case ShipSectionType.Engine:
					return "gameEngineList";
				default:
					throw new ArgumentOutOfRangeException(nameof(sectionType));
			}
		}

		private static string UISectionStats(ShipSectionType sectionType)
		{
			switch (sectionType)
			{
				case ShipSectionType.Command:
					return "CommandDesign";
				case ShipSectionType.Mission:
					return "MissionDesign";
				case ShipSectionType.Engine:
					return "EngineDesign";
				default:
					throw new ArgumentOutOfRangeException(nameof(sectionType));
			}
		}

		protected void SelectFilter(string panel, bool enabled)
		{
			if (ComparativeAnalysysState.BankFilters[panel].Enabled == enabled)
				return;
			ComparativeAnalysysState.BankFilters[panel].Enabled = enabled;
			if (enabled)
			{
				if (panel == "filterAll")
				{
					foreach (KeyValuePair<string, ComparativeAnalysysState.BankFilter> bankFilter in ComparativeAnalysysState.BankFilters)
					{
						if (bankFilter.Key != panel)
						{
							bankFilter.Value.Enabled = false;
							this.App.UI.SetChecked(bankFilter.Key, false);
						}
					}
				}
				else
				{
					ComparativeAnalysysState.BankFilters["filterAll"].Enabled = false;
					this.App.UI.SetChecked("filterAll", false);
				}
			}
			if (!ComparativeAnalysysState.BankFilters.Where<KeyValuePair<string, ComparativeAnalysysState.BankFilter>>((Func<KeyValuePair<string, ComparativeAnalysysState.BankFilter>, bool>)(x => x.Value.Enabled)).Any<KeyValuePair<string, ComparativeAnalysysState.BankFilter>>())
			{
				ComparativeAnalysysState.BankFilters["filterAll"].Enabled = true;
				this.App.UI.SetChecked("filterAll", true);
			}
			this._currentShipDirty = true;
		}

		protected override void UICommChannel_OnPanelMessage(
		  string panelName,
		  string msgType,
		  string[] msgParams)
		{
			if (msgType == "filter_mode")
			{
				this.HideModuleSelector();
				this.HidePsionicSelector();
				this.HideWeaponSelector();
				int id = int.Parse(msgParams[0]);
				int filterMode = int.Parse(msgParams[1]);
				WeaponBank gameObject = (WeaponBank)this.App.GetGameObject(id);
				if (gameObject == null)
					return;
				this.SetWeaponsAllFilterMode(gameObject.Weapon, filterMode);
			}
			else if (msgType == "checkbox_clicked")
			{
				if (!panelName.Contains("filter"))
					return;
				this.SelectFilter(panelName, int.Parse(msgParams[0]) != 0);
			}
			else if (msgType == "button_clicked")
			{
				if (panelName == "expandFilters")
				{
					ComparativeAnalysysState.FilterVisible = !ComparativeAnalysysState.FilterVisible;
					this.App.UI.SetVisible("BankFilter", ComparativeAnalysysState.FilterVisible);
				}
				else if (panelName == "missileTest_1")
				{
					if (this._targetArena == null)
						return;
					this._targetArena.LaunchWeapon((IGameObject)this._builder.Ship, 5);
				}
				else if (panelName == "missileTest_2")
				{
					if (this._targetArena == null)
						return;
					this._targetArena.LaunchWeapon((IGameObject)this._builder.Ship, 10);
				}
				else if (panelName == "missileTest_3")
				{
					if (this._targetArena == null)
						return;
					this._targetArena.LaunchWeapon((IGameObject)this._builder.Ship, 20);
				}
				else if (panelName == "gameExitButton")
				{
					if (this._inWeaponTestScreen)
					{
						this.EnableWeaponTestMode(false);
						this._inWeaponTestScreen = false;
						this._shouldRefresh = true;
						this._updateCamTarget = true;
						this._shipHoloView.PostSetProp("CenterCamera");
						if (this._builder.Ship != null)
						{
							this._builder.Ship.Maneuvering.PostSetProp("ResetPosition");
							this._builder.Ship.PostSetProp("SetValidateCurrentPosition", false);
							this._builder.Ship.PostSetProp("StopAnims");
							this._builder.Ship.PostSetProp("ClearDamageVisuals");
							this._builder.Ship.SetShipTarget(0, Vector3.Zero, true, 0);
							this._builder.Ship.PostSetProp("SetDisableLaunching", true);
							this._builder.Ship.PostSetProp("FullyHealShip");
							this._builder.Ship.PostSetProp("SetCombatReady", (object)true, (object)2f);
							this._targetArena.ResetTargetPositions();
							this._builder.ForceSyncRiders();
							this.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_CLEAR_WEAPON_SPAWNS);
						}
						this._camera.DesiredYaw = MathHelper.DegreesToRadians(-91f);
						this._camera.DesiredYaw = MathHelper.DegreesToRadians(-90f);
						this._camera.YawEnabled = false;
						this._input.PostSetProp("DisableCombatInputMouseOver", true);
						this._input.PostSetProp("WeaponLauncher", 0);
					}
					else
					{
						this.App.UI.LockUI();
						if (this._previousState == "StarMapState")
							this.App.SwitchGameState<StarMapState>();
						else if (this._previousState == "BuildScreenState")
							this.App.SwitchGameState<BuildScreenState>();
						else
							this.App.SwitchGameState<StarMapState>();
					}
				}
				else if ("starmap" == panelName)
					this.App.SwitchGameState<StarMapState>();
				else if (!("weaponTest" == panelName))
					;
			}
			else if (msgType == "color_changed")
			{
				if (!(panelName == ComparativeAnalysysState.DebugShipColor))
					return;
				this.App.LocalPlayer.PostSetProp("Color2", (object)new Vector4(float.Parse(msgParams[0]), float.Parse(msgParams[1]), float.Parse(msgParams[2]), 1f));
			}
			else
			{
				if (!(msgType == "list_sel_changed"))
					return;
				if (panelName == "gameClassList")
				{
					this.SetSelectedClass((RealShipClasses)Enum.Parse(typeof(RealShipClasses), msgParams[0]), "gameClassList");
					this._RetrofitMode = false;
					this._shouldRefresh = true;
				}
				else if (panelName == "gamePlayerSelect")
				{
					int playerId = int.Parse(msgParams[0]);
					this._SelectedPlayer = playerId;
					string factionName = this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(playerId));
					this.SetSelectedClass(RealShipClasses.Cruiser, "");
					this.SetSelectedFaction(factionName, "");
				}
				else
				{
					if (!(panelName == "gameDesignList") || string.IsNullOrEmpty(msgParams[0]))
						return;
					this.SetSelectedDesign(int.Parse(msgParams[0]), "gameDesignList");
					this._RetrofitMode = false;
					this._shouldRefresh = true;
				}
			}
		}

		private void SetWeaponsAll(
		  LogicalWeapon weapon,
		  WeaponEnums.WeaponSizes turretSize,
		  WeaponEnums.TurretClasses turretClass,
		  int? fireMode,
		  int? filterMode)
		{
			foreach (WeaponBankShipData currentWeaponBank in this._selection.GetCurrentWeaponBanks())
			{
				WeaponBankShipData weaponBank = currentWeaponBank;
				if (weaponBank.Bank.TurretClass == turretClass && weaponBank.Bank.TurretSize == turretSize && weaponBank.Weapons.Contains(weapon))
				{
					bool flag = false;
					if (this._RetrofitMode && weapon.PayloadType == weaponBank.SelectedWeapon.PayloadType && weapon.DefaultWeaponSize == weaponBank.SelectedWeapon.DefaultWeaponSize)
					{
						if (weapon.PayloadType == WeaponEnums.PayloadTypes.Bolt)
						{
							if (((IEnumerable<WeaponEnums.WeaponTraits>)weapon.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.Laser) && ((IEnumerable<WeaponEnums.WeaponTraits>)weaponBank.SelectedWeapon.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.Laser))
								flag = true;
							if (((IEnumerable<WeaponEnums.WeaponTraits>)weapon.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.Ballistic) && ((IEnumerable<WeaponEnums.WeaponTraits>)weaponBank.SelectedWeapon.Traits).Contains<WeaponEnums.WeaponTraits>(WeaponEnums.WeaponTraits.Ballistic))
								flag = true;
						}
						else
							flag = true;
					}
					if (!this._RetrofitMode || this._RetrofitMode && flag)
					{
						weaponBank.SelectedWeapon = weapon;
						weaponBank.FiringMode = fireMode;
						weaponBank.FilterMode = filterMode;
						WeaponAssignment weaponAssignment = this._selection.GetWeaponAssignments().FirstOrDefault<WeaponAssignment>((Func<WeaponAssignment, bool>)(x => x.Bank == weaponBank.Bank));
						if (weaponAssignment != null)
						{
							weaponAssignment.InitialFireMode = fireMode;
							weaponAssignment.InitialTargetFilter = filterMode;
						}
					}
				}
			}
		}

		private void SetWeaponsAllFireMode(LogicalWeapon weapon, int fireMode)
		{
			foreach (WeaponAssignment weaponAssignment in this._selection.GetWeaponAssignments())
			{
				WeaponAssignment wa = weaponAssignment;
				if (wa.Weapon == weapon)
				{
					wa.InitialFireMode = new int?(fireMode);
					WeaponBankShipData weaponBankShipData = this._selection.GetCurrentWeaponBanks().FirstOrDefault<WeaponBankShipData>((Func<WeaponBankShipData, bool>)(x => x.Bank == wa.Bank));
					if (weaponBankShipData != null)
						weaponBankShipData.FiringMode = new int?(fireMode);
				}
			}
		}

		private void SetWeaponsAllFilterMode(LogicalWeapon weapon, int filterMode)
		{
			foreach (WeaponAssignment weaponAssignment in this._selection.GetWeaponAssignments())
			{
				WeaponAssignment wa = weaponAssignment;
				if (wa.Weapon == weapon)
				{
					wa.InitialTargetFilter = new int?(filterMode);
					WeaponBankShipData weaponBankShipData = this._selection.GetCurrentWeaponBanks().FirstOrDefault<WeaponBankShipData>((Func<WeaponBankShipData, bool>)(x => x.Bank == wa.Bank));
					if (weaponBankShipData != null)
						weaponBankShipData.FilterMode = new int?(filterMode);
				}
			}
		}

		private void PlayAnim()
		{
			if (string.IsNullOrEmpty(this._debugCurrentAnimTrack) || this._builder.Ship == null)
				return;
			this._builder.Ship.PostSetProp(nameof(PlayAnim), (object)this._debugCurrentAnimTrack, (object)(this._debugCurrentAnimMode + 1));
		}

		private void StopAnim()
		{
			if (this._builder.Ship == null)
				return;
			this._builder.Ship.PostSetProp("StopAnims");
		}

		private void SetSelectedAnimTrack(string trackName, string trigger)
		{
			if (this._debugCurrentAnimTrack == trackName)
				return;
			this._debugCurrentAnimTrack = trackName;
			if (trigger != ComparativeAnalysysState.DebugAnimEdit)
				this.App.UI.SetText(ComparativeAnalysysState.DebugAnimEdit, trackName);
			if (!(trigger == ComparativeAnalysysState.DebugAnimTracks))
				return;
			int userItemId = -1;
			for (int index = 0; index < ComparativeAnalysysState._debugAnimTracks.Length; ++index)
			{
				if (ComparativeAnalysysState._debugAnimTracks[index] == trackName)
				{
					userItemId = index;
					break;
				}
			}
			if (userItemId >= 0)
				this.App.UI.SetSelection(ComparativeAnalysysState.DebugAnimTracks, userItemId);
			else
				this.App.UI.ClearSelection(ComparativeAnalysysState.DebugAnimTracks);
		}

		private IWeaponShipData GetWeaponSelectorBank()
		{
			if (this._selectedWeaponBank != null)
				return this._selectedWeaponBank;
			if (this._selectedModule != null)
				return (IWeaponShipData)this._selectedModule.SelectedModule;
			return (IWeaponShipData)null;
		}

		private RealShipClasses SelectedClass
		{
			get
			{
				return this._selection.Factions.Current.SelectedClass.Class;
			}
		}

		private void SetSelectedClass(RealShipClasses shipClass, string trigger)
		{
			ClassShipData classShipData = this._selection.Factions.Current.Classes.FirstOrDefault<ClassShipData>((Func<ClassShipData, bool>)(x => x.Class == shipClass));
			if (classShipData == null)
				return;
			this._selection.Factions.Current.SelectedClass = classShipData;
			if (trigger != "gameClassList")
				this.App.UI.SetSelection("gameClassList", (int)shipClass);
			this.PopulateSectionLists();
			this.PopulateDesignList();
			this._RetrofitMode = false;
			if (!(trigger != "init"))
				return;
			this.CurrentShipChanged();
		}

		private void SetSelectedDesign(int designId, string trigger)
		{
			if (designId <= 0)
				return;
			DesignInfo designInfo = this.App.GameDatabase.GetDesignInfo(designId);
			if (designInfo == null)
				return;
			if (trigger != "gameDesignList")
				this.App.UI.SetSelection("gameDesignList", designId);
			this._selectedDesign = new int?(designId);
			this._designName = designInfo.Name;
			this._originalName = designInfo.Name;
			this._selectedDesignPW = designInfo.PriorityWeaponName;
			foreach (ModuleShipData currentSectionModule in this._selection.GetCurrentSectionModules())
				currentSectionModule.SelectedModule = (ModuleData)null;
			foreach (DesignSectionInfo designSection in designInfo.DesignSections)
			{
				DesignSectionInfo section = designSection;
				ShipSectionAsset section1 = this.App.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == section.FilePath));
				ShipSectionType sectionType = section1 != null ? section1.Type : ShipSectionType.Mission;
				this.SetSelectedSection(section1, sectionType, trigger, true, (DesignInfo)null);
				int num = 0;
				foreach (WeaponBankInfo weaponBank in section.WeaponBanks)
				{
					WeaponBankShipData currentWeaponBank = this._selection.GetCurrentWeaponBank(this.App.GameDatabase, weaponBank);
					if (currentWeaponBank != null)
					{
						if (weaponBank.WeaponID.HasValue)
						{
							string weaponFile = this.App.GameDatabase.GetWeaponAsset(weaponBank.WeaponID.Value);
							currentWeaponBank.SelectedWeapon = currentWeaponBank.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.FileName == weaponFile));
							currentWeaponBank.FiringMode = weaponBank.FiringMode;
							currentWeaponBank.FilterMode = weaponBank.FilterMode;
						}
						if (weaponBank.DesignID.HasValue)
							currentWeaponBank.SelectedDesign = weaponBank.DesignID.HasValue ? weaponBank.DesignID.Value : 0;
						++num;
					}
				}
				foreach (DesignModuleInfo module in section.Modules)
				{
					string moduleAsset = this.App.GameDatabase.GetModuleAsset(module.ModuleID);
					ModuleShipData currentModuleMount = this._selection.GetCurrentModuleMount(this.App.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == section.FilePath)), module.MountNodeName);
					if (currentModuleMount == null)
					{
						ComparativeAnalysysState.Warn(string.Format("Module mount {0} not found for design '{1}', section '{2}'.", (object)module.MountNodeName, (object)designInfo.Name, (object)section1.FileName));
					}
					else
					{
						currentModuleMount.SelectedModule = currentModuleMount.Modules.FirstOrDefault<ModuleData>((Func<ModuleData, bool>)(x => x.Module.ModulePath == moduleAsset));
						if (currentModuleMount.SelectedModule != null)
							currentModuleMount.SelectedModule.SelectedPsionic.Clear();
						if (module.PsionicAbilities != null)
						{
							foreach (ModulePsionicInfo psionicAbility in module.PsionicAbilities)
								currentModuleMount.SelectedModule.SelectedPsionic.Add(psionicAbility.Ability);
						}
						if (currentModuleMount.SelectedModule != null && module.WeaponID.HasValue)
						{
							string weaponFile = this.App.GameDatabase.GetWeaponAsset(module.WeaponID.Value);
							currentModuleMount.SelectedModule.SelectedWeapon = currentModuleMount.SelectedModule.Weapons.First<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.FileName == weaponFile));
						}
					}
				}
			}
			this._RetrofitMode = false;
			this.SyncSectionTechs(designInfo);
			this.CurrentShipChanged();
		}

		private void SetSelectedSectionById(
		  ShipSectionType sectionType,
		  string msgParam,
		  string trigger)
		{
			SectionShipData sectionShipData = (SectionShipData)null;
			SectionTypeShipData sectionTypeShipData = this._selection.GetCurrentSectionTypeShipData(sectionType);
			int id;
			if (sectionTypeShipData != null && this.App.UI.ParseListItemId(msgParam, out id) && (id >= 0 && id < sectionTypeShipData.Sections.Count))
				sectionShipData = sectionTypeShipData.Sections[id];
			ShipSectionAsset section = (ShipSectionAsset)null;
			if (sectionShipData != null)
				section = sectionShipData.Section;
			this.SetSelectedSection(section, sectionType, trigger, true, (DesignInfo)null);
			foreach (WeaponBankShipData weaponBank in this._selection.GetCurrentSectionData(sectionType).WeaponBanks)
			{
				weaponBank.Designs.Clear();
				weaponBank.Designs.AddRange(this.CollectWeaponDesigns(section, weaponBank.Bank.TurretClass));
			}
		}

		private void SetSelectedSection(
		  ShipSectionAsset section,
		  ShipSectionType sectionType,
		  string trigger,
		  bool refreshOtherSections = true,
		  DesignInfo design = null)
		{
			SectionTypeShipData sectionTypeShipData = this._selection.GetCurrentSectionTypeShipData(sectionType);
			if (sectionTypeShipData == null)
				return;
			this.HideWeaponSelector();
			this.HidePsionicSelector();
			SectionShipData section1 = sectionTypeShipData.Sections.FirstOrDefault<SectionShipData>((Func<SectionShipData, bool>)(x => x.Section == section));
			if (trigger != "gameDesignList" && section1 != null && this.IsExcluded(section1))
				return;
			sectionTypeShipData.SelectedSection = section1;
			if (trigger != "" && trigger != "gameDesignList")
			{
				this.App.UI.ClearSelection("gameDesignList");
				this._selectedDesign = new int?();
				this._selectedDesignPW = "";
			}
			if (trigger != ComparativeAnalysysState.UISectionList(sectionType))
				this.App.UI.SetSelection(ComparativeAnalysysState.UISectionList(sectionType), sectionTypeShipData.Sections.IndexOf(section1));
			if (section != null)
				ShipDesignUI.SyncSectionArmor(this.App, ComparativeAnalysysState.UISectionStats(sectionType), section, design);
			this.RefreshExcludedSections();
			if (!refreshOtherSections || section == null)
				return;
			switch (section.Type)
			{
				case ShipSectionType.Command:
					SectionShipData currentSectionData1 = this._selection.GetCurrentSectionData(ShipSectionType.Mission);
					SectionShipData currentSectionData2 = this._selection.GetCurrentSectionData(ShipSectionType.Engine);
					SectionShipData sectionShipData1 = this.ConfirmIfAvailableSection(currentSectionData1, ShipSectionType.Mission);
					SectionShipData sectionShipData2 = this.ConfirmIfAvailableSection(currentSectionData2, ShipSectionType.Engine);
					ShipSectionAsset section2 = sectionShipData1 == null ? (ShipSectionAsset)null : sectionShipData1.Section;
					ShipSectionAsset section3 = sectionShipData2 == null ? (ShipSectionAsset)null : sectionShipData2.Section;
					if (currentSectionData1 != sectionShipData1)
						this.SetSelectedSection(section2, ShipSectionType.Mission, trigger, false, (DesignInfo)null);
					if (currentSectionData2 == sectionShipData2)
						break;
					this.SetSelectedSection(section3, ShipSectionType.Engine, trigger, false, (DesignInfo)null);
					break;
				case ShipSectionType.Engine:
					SectionShipData currentSectionData3 = this._selection.GetCurrentSectionData(ShipSectionType.Command);
					SectionShipData currentSectionData4 = this._selection.GetCurrentSectionData(ShipSectionType.Mission);
					SectionShipData sectionShipData3 = this.ConfirmIfAvailableSection(currentSectionData3, ShipSectionType.Command);
					SectionShipData sectionShipData4 = this.ConfirmIfAvailableSection(currentSectionData4, ShipSectionType.Mission);
					ShipSectionAsset section4 = sectionShipData3 == null ? (ShipSectionAsset)null : sectionShipData3.Section;
					ShipSectionAsset section5 = sectionShipData4 == null ? (ShipSectionAsset)null : sectionShipData4.Section;
					if (currentSectionData3 != sectionShipData3)
						this.SetSelectedSection(section4, ShipSectionType.Command, trigger, false, (DesignInfo)null);
					if (currentSectionData4 == sectionShipData4)
						break;
					this.SetSelectedSection(section5, ShipSectionType.Mission, trigger, false, (DesignInfo)null);
					break;
				default:
					SectionShipData currentSectionData5 = this._selection.GetCurrentSectionData(ShipSectionType.Command);
					SectionShipData currentSectionData6 = this._selection.GetCurrentSectionData(ShipSectionType.Engine);
					SectionShipData sectionShipData5 = this.ConfirmIfAvailableSection(currentSectionData5, ShipSectionType.Command);
					SectionShipData sectionShipData6 = this.ConfirmIfAvailableSection(currentSectionData6, ShipSectionType.Engine);
					ShipSectionAsset section6 = sectionShipData5 == null ? (ShipSectionAsset)null : sectionShipData5.Section;
					ShipSectionAsset section7 = sectionShipData6 == null ? (ShipSectionAsset)null : sectionShipData6.Section;
					if (currentSectionData5 != sectionShipData5)
						this.SetSelectedSection(section6, ShipSectionType.Command, trigger, true, (DesignInfo)null);
					if (currentSectionData6 == sectionShipData6)
						break;
					this.SetSelectedSection(section7, ShipSectionType.Engine, trigger, true, (DesignInfo)null);
					break;
			}
		}

		private SectionShipData ConfirmIfAvailableSection(
		  SectionShipData proposedSection,
		  ShipSectionType sectionType)
		{
			if (proposedSection != null && !this.IsExcluded(proposedSection))
				return proposedSection;
			SectionTypeShipData sectionTypeShipData = this._selection.GetCurrentSectionTypeShipData(sectionType);
			if (sectionTypeShipData == null)
				return (SectionShipData)null;
			return sectionTypeShipData.Sections.FirstOrDefault<SectionShipData>((Func<SectionShipData, bool>)(x => !this.IsExcluded(x)));
		}

		private bool IsExcluded(SectionShipData currSection, SectionShipData desSection)
		{
			return currSection != null && desSection != null && currSection.Section.SectionIsExcluded(desSection.Section);
		}

		private bool IsExcluded(SectionShipData section)
		{
			SectionShipData currentSectionData1 = this._selection.GetCurrentSectionData(ShipSectionType.Command);
			SectionShipData currentSectionData2 = this._selection.GetCurrentSectionData(ShipSectionType.Mission);
			SectionShipData currentSectionData3 = this._selection.GetCurrentSectionData(ShipSectionType.Engine);
			return this.IsExcluded(currentSectionData1, section) || this.IsExcluded(currentSectionData2, section) || this.IsExcluded(currentSectionData3, section);
		}

		private void RefreshExcludedSections()
		{
			this.RefreshExcludedSections("gameMissionList", ShipSectionType.Mission);
			this.RefreshExcludedSections("gameEngineList", ShipSectionType.Engine);
			this.RefreshExcludedSections("gameCommandList", ShipSectionType.Command);
		}

		private void RefreshExcludedSections(string listId, ShipSectionType sectionType)
		{
			List<int> intList = new List<int>();
			SectionTypeShipData sectionTypeShipData = this._selection.GetCurrentSectionTypeShipData(sectionType);
			if (sectionTypeShipData != null)
			{
				for (int index = 0; index < sectionTypeShipData.Sections.Count; ++index)
				{
					if (this.IsExcluded(sectionTypeShipData.Sections[index]))
						intList.Add(index);
				}
			}
			this.App.UI.SetDisabledItems(listId, (IEnumerable<int>)intList);
		}

		private string StripSection(string str)
		{
			string[] strArray = str.Split('\\', '/');
			if (strArray.Length > 0)
				str = strArray[strArray.Length - 1];
			str = str.Replace(".section", "");
			return str;
		}

		private string GetWeaponBankTooltip(IWeaponShipData weaponBank)
		{
			if (!weaponBank.RequiresDesign)
			{
				string str = weaponBank.SelectedWeapon.Name;
				if (weaponBank.Bank != null)
					str = str + " (" + (object)weaponBank.Bank.TurretClass + ", " + (object)weaponBank.Bank.TurretSize + ")";
				return str;
			}
			if (weaponBank.SelectedDesign == 0)
				return App.Localize("@UI_DEFAULT") + " " + weaponBank.SelectedWeapon.Name;
			return this.App.GameDatabase.GetDesignInfo(weaponBank.SelectedDesign).Name;
		}

		private void SyncWeaponUi()
		{
			if (this._builder.Ship == null)
				return;
			List<WeaponBankShipData> list = this._selection.GetCurrentWeaponBanks().ToList<WeaponBankShipData>();
			if (!ComparativeAnalysysState.BankFilters.Any<KeyValuePair<string, ComparativeAnalysysState.BankFilter>>((Func<KeyValuePair<string, ComparativeAnalysysState.BankFilter>, bool>)(x =>
		   {
			   if (x.Key == "filterAll")
				   return x.Value.Enabled;
			   return false;
		   })))
			{
				IEnumerable<WeaponEnums.WeaponSizes> enabledSizes = ComparativeAnalysysState.BankFilters.Where<KeyValuePair<string, ComparativeAnalysysState.BankFilter>>((Func<KeyValuePair<string, ComparativeAnalysysState.BankFilter>, bool>)(x =>
			   {
				   if (x.Value.Value is WeaponEnums.WeaponSizes)
					   return x.Value.Enabled;
				   return false;
			   })).Select<KeyValuePair<string, ComparativeAnalysysState.BankFilter>, WeaponEnums.WeaponSizes>((Func<KeyValuePair<string, ComparativeAnalysysState.BankFilter>, WeaponEnums.WeaponSizes>)(x => (WeaponEnums.WeaponSizes)x.Value.Value));
				list = list.Where<WeaponBankShipData>((Func<WeaponBankShipData, bool>)(x => enabledSizes.Contains<WeaponEnums.WeaponSizes>(x.Bank.TurretSize))).ToList<WeaponBankShipData>();
			}
			if (!this._builder.Loading)
				this._shipHoloView.SetShip(this._builder.Ship);
			if (this._shouldRefresh)
			{
				this._shipHoloView.PostSetProp("FitToView");
				this._shouldRefresh = false;
			}
			foreach (WeaponBankShipData weaponBankShipData in list)
			{
				WeaponBankShipData weaponBank = weaponBankShipData;
				WeaponBank weaponBank1 = this._builder.Ship.WeaponBanks.FirstOrDefault<WeaponBank>((Func<WeaponBank, bool>)(shipWeaponBank => shipWeaponBank.LogicalBank == weaponBank.Bank));
				if (weaponBank1 != null)
					this._shipHoloView.AddWeaponGroupIcon(weaponBank1);
			}
			if (!ComparativeAnalysysState.BankFilters["filterModules"].Enabled && !ComparativeAnalysysState.BankFilters["filterAll"].Enabled)
				return;
			foreach (ModuleShipData currentSectionModule in this._selection.GetCurrentSectionModules())
			{
				ModuleShipData moduleSlot = currentSectionModule;
				ModuleData selectedModuleData = moduleSlot.SelectedModule;
				Module selectedModule = (Module)null;
				if (selectedModuleData != null)
					selectedModule = this._builder.Ship.Modules.FirstOrDefault<Module>((Func<Module, bool>)(module =>
				   {
					   if (module.LogicalModule == selectedModuleData.Module)
						   return moduleSlot.ModuleMount == module.Attachment;
					   return false;
				   }));
				string iconSpriteName = "moduleicon_no_selection";
				if (selectedModule != null)
					iconSpriteName = selectedModule.LogicalModule.Icon;
				this._shipHoloView.AddModuleIcon(selectedModule, this._builder.Ship.Sections.FirstOrDefault<Kerberos.Sots.GameObjects.Section>((Func<Kerberos.Sots.GameObjects.Section, bool>)(section => section.ShipSectionAsset == moduleSlot.Section.Section)), moduleSlot.ModuleMount.NodeName, iconSpriteName);
				if (selectedModule != null && (selectedModuleData.Module.Banks.Length > 0 || selectedModuleData.Module.Psionics.Length > 0))
				{
					foreach (LogicalBank bank1 in selectedModule.LogicalModule.Banks)
					{
						LogicalBank bank = bank1;
						WeaponBank weaponBank = this._builder.Ship.WeaponBanks.FirstOrDefault<WeaponBank>((Func<WeaponBank, bool>)(shipWeaponBank =>
					   {
						   if (shipWeaponBank.LogicalBank == bank)
							   return shipWeaponBank.Module == selectedModule;
						   return false;
					   }));
						if (weaponBank != null)
							this._shipHoloView.AddWeaponGroupIcon(weaponBank);
					}
					if (selectedModuleData.SelectedPsionic != null)
					{
						int elementid = 0;
						foreach (int num in selectedModuleData.SelectedPsionic)
						{
							int int32 = Convert.ToInt32((object)(SectionEnumerations.PsionicAbility)num);
							this._shipHoloView.AddPsionicIcon(selectedModule, int32, elementid);
							++elementid;
						}
					}
				}
			}
		}

		private void PopulateFactionsList()
		{
			this.App.UI.ClearItems(ComparativeAnalysysState.DebugFactionsList);
			for (int userItemId = 0; userItemId < this._factionsList.Count; ++userItemId)
				this.App.UI.AddItem(ComparativeAnalysysState.DebugFactionsList, string.Empty, userItemId, this._factionsList[userItemId].Name);
		}

		public void SetSelectedFaction(string factionName, string trigger)
		{
			FactionShipData t = this._selection.Factions.FirstOrDefault<FactionShipData>((Func<FactionShipData, bool>)(x => x.Faction.Name == factionName));
			if (t == null)
				return;
			ClassShipData oldShipClassData = this._selection.GetCurrentClassShipData();
			this._selection.Factions.SetCurrent(t);
			if (trigger != ComparativeAnalysysState.DebugFactionsList)
			{
				int userItemId = this._factionsList.IndexOf(t.Faction);
				this.App.UI.SetSelection(ComparativeAnalysysState.DebugFactionsList, userItemId);
			}
			if (trigger != "init")
				this.CurrentShipChanged();
			this.PopulateClassList();
			if (oldShipClassData != null && this._selection.Factions.Current.Classes.FirstOrDefault<ClassShipData>((Func<ClassShipData, bool>)(x => x.Class == oldShipClassData.Class)) != null)
				this.SetSelectedClass(oldShipClassData.Class, trigger);
			else if (this._selection.Factions.Current.Classes.FirstOrDefault<ClassShipData>((Func<ClassShipData, bool>)(x => x.Class == RealShipClasses.Cruiser)) != null)
			{
				this.SetSelectedClass(RealShipClasses.Cruiser, trigger);
			}
			else
			{
				if (!this._selection.Factions.Current.Classes.Any<ClassShipData>())
					return;
				this.SetSelectedClass(this._selection.Factions.Current.Classes.First<ClassShipData>().Class, trigger);
			}
		}

		private IEnumerable<RealShipClasses> CollectAvailableShipClasses()
		{
			FactionShipData faction = this._selection.GetCurrentFactionShipData();
			foreach (RealShipClasses realShipClasses1 in Enum.GetValues(typeof(RealShipClasses)).Cast<RealShipClasses>())
			{
				RealShipClasses shipClass = realShipClasses1;
				if ((faction == null || faction.Classes.Any<ClassShipData>((Func<ClassShipData, bool>)(x => x.Class == shipClass))) && this.EncounteredDesigns.Any<DesignInfo>((Func<DesignInfo, bool>)(x =>
			  {
				  if (this._SelectedPlayer != x.PlayerID)
					  return false;
				  RealShipClasses? realShipClass = x.GetRealShipClass();
				  RealShipClasses realShipClasses = shipClass;
				  if (realShipClass.GetValueOrDefault() == realShipClasses)
					  return realShipClass.HasValue;
				  return false;
			  })))
					yield return shipClass;
			}
		}

		private void PopulateClassList()
		{
			this.App.UI.ClearItems("gameClassList");
			foreach (RealShipClasses availableShipClass in this.CollectAvailableShipClasses())
				this.App.UI.AddItem("gameClassList", string.Empty, (int)availableShipClass, availableShipClass.Localize());
		}

		private void PopulateFactionList()
		{
			this.App.UI.ClearItems("gamePlayerSelect");
			foreach (PlayerInfo standardPlayerInfo in this.App.GameDatabase.GetStandardPlayerInfos())
			{
				PlayerInfo ply = standardPlayerInfo;
				if (this.EncounteredDesigns.Any<DesignInfo>((Func<DesignInfo, bool>)(x =>
			   {
				   if (x.PlayerID == ply.ID)
					   return ((IEnumerable<DesignSectionInfo>)x.DesignSections).Any<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(y => ComparativeAnalysysState.DesignScreenAllowsShipClass(y.ShipSectionAsset)));
				   return false;
			   })))
					this.App.UI.AddItem("gamePlayerSelect", string.Empty, ply.ID, ply.Name);
			}
			this.App.UI.SetSelection("gamePlayerSelect", this.EncounteredDesigns.First<DesignInfo>().PlayerID);
		}

		private void PopulateSectionLists()
		{
			this.PopulateSectionList(ShipSectionType.Mission);
			this.PopulateSectionList(ShipSectionType.Command);
			this.PopulateSectionList(ShipSectionType.Engine);
		}

		private void PopulateSectionList(ShipSectionType sectionType)
		{
			string str = ComparativeAnalysysState.UISectionList(sectionType);
			SectionTypeShipData sectionTypeShipData = this._selection.GetCurrentSectionTypeShipData(sectionType);
			this.App.UI.SetVisible(str, sectionTypeShipData != null);
			ShipSectionAsset section = (ShipSectionAsset)null;
			this.App.UI.ClearItems(str);
			if (sectionTypeShipData != null)
			{
				for (int userItemId = 0; userItemId < sectionTypeShipData.Sections.Count; ++userItemId)
				{
					if (this._showDebugControls || !sectionTypeShipData.Sections[userItemId].Section.IsSuulka)
						this.App.UI.AddItem(str, string.Empty, userItemId, App.Localize(sectionTypeShipData.Sections[userItemId].Section.Title));
				}
				if (sectionTypeShipData.SelectedSection != null)
					section = sectionTypeShipData.SelectedSection.Section;
			}
			this.SetSelectedSection(section, sectionType, "init", true, (DesignInfo)null);
		}

		private DesignSectionInfo SummarizeDesignSection(
		  ShipSectionType type,
		  int playerId)
		{
			string sectionAssetName = this._selection.GetCurrentSectionAssetName(type);
			if (string.IsNullOrEmpty(sectionAssetName))
				return (DesignSectionInfo)null;
			SectionShipData currentSectionData = this._selection.GetCurrentSectionData(type);
			List<WeaponBankInfo> weaponBankInfoList = new List<WeaponBankInfo>();
			foreach (WeaponBankShipData weaponBank in currentSectionData.WeaponBanks)
			{
				int? nullable1 = new int?();
				int? nullable2 = new int?();
				if (weaponBank.SelectedWeapon != null)
				{
					nullable1 = this.App.GameDatabase.GetWeaponID(weaponBank.SelectedWeapon.FileName, playerId);
					if (weaponBank.SelectedDesign != 0)
						nullable2 = new int?(weaponBank.SelectedDesign);
				}
				weaponBankInfoList.Add(new WeaponBankInfo()
				{
					WeaponID = nullable1,
					DesignID = nullable2,
					FiringMode = weaponBank.FiringMode,
					FilterMode = weaponBank.FilterMode,
					BankGUID = weaponBank.Bank.GUID
				});
			}
			List<DesignModuleInfo> designModuleInfoList = new List<DesignModuleInfo>();
			foreach (ModuleShipData moduleShipData in currentSectionData.Modules.Where<ModuleShipData>((Func<ModuleShipData, bool>)(x => x.SelectedModule != null)))
			{
				int moduleId = this.App.GameDatabase.GetModuleID(moduleShipData.SelectedModule.Module.ModulePath, playerId);
				int? nullable1 = new int?();
				int? nullable2 = new int?();
				if (moduleShipData.SelectedModule.SelectedWeapon != null)
				{
					nullable1 = this.App.GameDatabase.GetWeaponID(moduleShipData.SelectedModule.SelectedWeapon.FileName, playerId);
					if (moduleShipData.SelectedModule.SelectedDesign != 0)
						nullable2 = new int?(moduleShipData.SelectedModule.SelectedDesign);
				}
				DesignModuleInfo designModuleInfo = new DesignModuleInfo()
				{
					MountNodeName = moduleShipData.ModuleMount.NodeName,
					ModuleID = moduleId,
					WeaponID = nullable1,
					DesignID = nullable2
				};
				foreach (SectionEnumerations.PsionicAbility psionicAbility in moduleShipData.SelectedModule.SelectedPsionic)
					designModuleInfo.PsionicAbilities.Add(new ModulePsionicInfo()
					{
						Ability = psionicAbility
					});
				designModuleInfoList.Add(designModuleInfo);
			}
			List<int> intList = new List<int>();
			if (this._shipOptionGroups != null && this._shipOptionGroups.ContainsKey(type))
			{
				foreach (Dictionary<string, bool> dictionary in this._shipOptionGroups[type])
				{
					foreach (KeyValuePair<string, bool> keyValuePair in dictionary)
					{
						if (keyValuePair.Value)
							intList.Add(this.App.GameDatabase.GetTechID(keyValuePair.Key));
					}
				}
			}
			return new DesignSectionInfo()
			{
				FilePath = sectionAssetName,
				WeaponBanks = weaponBankInfoList,
				Modules = designModuleInfoList,
				Techs = intList
			};
		}

		private DesignInfo SummarizeDesign(int playerId, bool finalsummerize = false)
		{
			if (!this._selection.IsCurrentShipDataValid())
				return (DesignInfo)null;
			List<DesignSectionInfo> designSectionInfoList = new List<DesignSectionInfo>();
			DesignSectionInfo designSectionInfo1 = this.SummarizeDesignSection(ShipSectionType.Command, playerId);
			if (designSectionInfo1 != null)
				designSectionInfoList.Add(designSectionInfo1);
			DesignSectionInfo mission = this.SummarizeDesignSection(ShipSectionType.Mission, playerId);
			if (mission != null)
			{
				this.App.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == mission.FilePath));
				designSectionInfoList.Add(mission);
			}
			DesignSectionInfo designSectionInfo2 = this.SummarizeDesignSection(ShipSectionType.Engine, playerId);
			if (designSectionInfo2 != null)
				designSectionInfoList.Add(designSectionInfo2);
			if (designSectionInfoList.Count == 0)
				return (DesignInfo)null;
			DesignInfo design = new DesignInfo()
			{
				PlayerID = playerId,
				Name = string.IsNullOrEmpty(this._designName) ? string.Empty : this._designName,
				DesignSections = designSectionInfoList.ToArray()
			};
			design.Name = this._RetrofitMode || finalsummerize ? this.App.GameDatabase.ResolveNewDesignName(playerId, design.Name) : this.App.GameDatabase.ResolveNewDesignName(playerId, string.Empty);
			DesignLab.SummarizeDesign(this.App.AssetDatabase, this.App.GameDatabase, design);
			return design;
		}

		private void ShowCommitDialog()
		{
			this.HidePsionicSelector();
			this.HideWeaponSelector();
			this.HideModuleSelector();
			this.App.UI.SetVisible("submit_dialog", true);
			DesignInfo designInfo = this.SummarizeDesign(this.App.LocalPlayer.ID, false);
			this._designName = this._RetrofitMode ? this.App.GameDatabase.ResolveNewDesignName(this.App.LocalPlayer.ID, this._originalName) : this.App.GameDatabase.ResolveNewDesignName(this.App.LocalPlayer.ID, designInfo.Name);
			this.App.UI.SetText("edit_design_name", this._designName);
			if (this._RetrofitMode)
			{
				this.App.UI.SetText("submit_dialog_title", App.Localize("@UI_DESIGN_CONFIRM_RETROFIT"));
				this.App.UI.SetEnabled("edit_design_name", false);
			}
			else
			{
				this.App.UI.SetText("submit_dialog_title", App.Localize("@UI_DESIGN_ENTER_DESIGN_NAME"));
				this.App.UI.SetEnabled("edit_design_name", true);
			}
		}

		private void CommitDesign()
		{
			this.App.UI.SetVisible("submit_dialog", false);
			DesignInfo design = this.SummarizeDesign(this.App.LocalPlayer.ID, true);
			design.Name = this.App.GameDatabase.ResolveNewDesignName(this.App.LocalPlayer.ID, design.Name);
			if (this._RetrofitMode)
			{
				if (this._selectedDesign.HasValue)
				{
					design.RetrofitBaseID = this._selectedDesign.Value;
					DesignInfo designInfo = this.App.GameDatabase.GetDesignInfo(this._selectedDesign.Value);
					design.isAttributesDiscovered = designInfo.isAttributesDiscovered;
				}
				design.isPrototyped = true;
			}
			if (this.App.GameDatabase.GetTurnCount() == 1)
				design.isPrototyped = true;
			if (design.Role == ShipRole.DRONE || design.Role == ShipRole.ASSAULTSHUTTLE || design.Class == ShipClass.BattleRider)
				design.isPrototyped = true;
			if (this._builder.Ship != null)
				design.PriorityWeaponName = this._builder.Ship.PriorityWeaponName;
			int num = this.App.GameDatabase.InsertDesignByDesignInfo(design);
			if (this._RetrofitMode && this._selectedDesign.HasValue && this.App.GameDatabase.GetDesignAttributesForDesign(this._selectedDesign.Value).Any<SectionEnumerations.DesignAttribute>())
				this.App.GameDatabase.InsertDesignAttribute(num, this.App.GameDatabase.GetDesignAttributesForDesign(this._selectedDesign.Value).First<SectionEnumerations.DesignAttribute>());
			this.PopulateDesignList();
			this.App.UI.SetSelection("gameDesignList", num);
			this.UpdateWeaponDesigns_DesignAdded(num);
			this.App.PostEnableSpeechSounds(true);
			this.App.PostRequestSpeech(string.Format("STRAT_036-01_{0}_DesignSaved", (object)this.App.GameDatabase.GetFactionName(this.App.GameDatabase.GetPlayerFactionID(this.App.LocalPlayer.ID))), 50, 120, 0.0f);
			this._RetrofitMode = false;
		}

		private void UpdateWeaponDesigns_DesignAdded(int designId)
		{
		}

		private void PopulateDesignList()
		{
			if (this._showDebugControls)
				return;
			List<DesignInfo> list = this.EncounteredDesigns.Where<DesignInfo>((Func<DesignInfo, bool>)(x =>
		   {
			   RealShipClasses? realShipClass = x.GetRealShipClass();
			   RealShipClasses selectedClass = this.SelectedClass;
			   if ((realShipClass.GetValueOrDefault() != selectedClass ? 0 : (realShipClass.HasValue ? 1 : 0)) != 0)
				   return x.PlayerID == this._SelectedPlayer;
			   return false;
		   })).ToList<DesignInfo>();
			ShipDesignUI.PopulateDesignList(this.App, "gameDesignList", (IEnumerable<DesignInfo>)list);
			if (!list.Any<DesignInfo>())
				return;
			this.App.UI.SetSelection("gameDesignList", list.First<DesignInfo>().ID);
		}

		private int GetBestBuildSite()
		{
			IEnumerable<StationInfo> stationInfosByPlayerId = this.App.GameDatabase.GetStationInfosByPlayerID(this.App.LocalPlayer.ID);
			if (stationInfosByPlayerId.Any<StationInfo>())
				return stationInfosByPlayerId.First<StationInfo>().OrbitalObjectID;
			return 0;
		}

		private void CurrentShipChanged()
		{
			this._currentShipDirty = true;
		}

		private void SelectTech(string panelId, bool enabled)
		{
			ShipSectionType sectionType;
			int optionIndex;
			string tech;
			this.GetTechInfoFromPanel(panelId, out sectionType, out optionIndex, out tech);
			if (this._shipOptionGroups[sectionType][optionIndex][tech] == enabled)
				return;
			this.ChangeTechGroupComboSelection(sectionType, optionIndex, tech, enabled);
			if (tech == "IND_Stealth_Armor")
				this.ChangeTechMultiSelection(tech, enabled);
			else if (tech.Contains("SLD"))
				this.CheckShieldTech();
			this._currentShipDirty = true;
		}

		private string GetTechPanel(ShipSectionType sectionType, int optionIndex, string techId)
		{
			return ("tech|" + (object)sectionType + "|" + (object)optionIndex + "|" + techId).Replace('.', '¿');
		}

		private void GetTechInfoFromPanel(
		  string panel,
		  out ShipSectionType sectionType,
		  out int optionIndex,
		  out string tech)
		{
			string[] strArray = panel.Split('|');
			sectionType = (ShipSectionType)Enum.Parse(typeof(ShipSectionType), strArray[1]);
			optionIndex = int.Parse(strArray[2]);
			tech = strArray[3];
			tech = tech.Replace('¿', '.');
		}

		private void ChangeTechMultiSelection(string tech, bool enabled)
		{
			foreach (KeyValuePair<ShipSectionType, List<Dictionary<string, bool>>> shipOptionGroup in this._shipOptionGroups)
			{
				Dictionary<string, bool> dictionary = shipOptionGroup.Value.FirstOrDefault<Dictionary<string, bool>>((Func<Dictionary<string, bool>, bool>)(x => x.ContainsKey(tech)));
				if (dictionary != null)
				{
					int optionIndex = shipOptionGroup.Value.IndexOf(dictionary);
					this.App.UI.SetChecked(this.GetTechPanel(shipOptionGroup.Key, optionIndex, tech), enabled);
					this.ChangeTechGroupComboSelection(shipOptionGroup.Key, optionIndex, tech, enabled);
				}
				else if (enabled)
				{
					this.ChangeTechMultiSelection(tech, false);
					this.App.UI.CreateDialog((Dialog)new GenericTextDialog(this.App, "Unable to Select Tech", "Unable to select this technology because it must be present on all sections.", "dialogGenericMessage"), null);
					break;
				}
			}
		}

		private void ChangeTechGroupComboSelection(
		  ShipSectionType section,
		  int optionIndex,
		  string tech,
		  bool enabled)
		{
			this._shipOptionGroups[section][optionIndex][tech] = enabled;
			this.App.UI.SetEnabled(this.GetTechPanel(section, optionIndex, tech), enabled);
			this.App.UI.SetChecked(this.GetTechPanel(section, optionIndex, tech), enabled);
			if (!enabled)
				return;
			Dictionary<string, bool> source = this._shipOptionGroups[section][optionIndex];
			foreach (KeyValuePair<string, bool> keyValuePair in source.ToList<KeyValuePair<string, bool>>())
			{
				if (keyValuePair.Key != tech)
				{
					if (keyValuePair.Key == "IND_Stealth_Armor")
					{
						this.ChangeTechMultiSelection(keyValuePair.Key, false);
					}
					else
					{
						this.App.UI.SetChecked(this.GetTechPanel(section, optionIndex, keyValuePair.Key), false);
						source[keyValuePair.Key] = false;
					}
				}
			}
		}

		private void ClearTechSelections()
		{
			foreach (KeyValuePair<ShipSectionType, List<Dictionary<string, bool>>> shipOptionGroup in this._shipOptionGroups)
			{
				foreach (Dictionary<string, bool> source in shipOptionGroup.Value)
				{
					foreach (KeyValuePair<string, bool> keyValuePair in source.ToList<KeyValuePair<string, bool>>())
					{
						int optionIndex = shipOptionGroup.Value.IndexOf(source);
						this.App.UI.SetChecked(this.GetTechPanel(shipOptionGroup.Key, optionIndex, keyValuePair.Key), false);
						source[keyValuePair.Key] = false;
					}
				}
			}
		}

		private void PopulateTechList(string sectionPanel, ShipSectionType sectionType)
		{
			ShipSectionAsset currentSection = this._selection.GetCurrentSection(sectionType);
			this.App.UI.ClearItems(this.App.UI.Path(sectionPanel, "specialList"));
			if (currentSection == null)
				return;
			this._shipOptionGroups[sectionType] = new List<Dictionary<string, bool>>();
			int userItemId = 0;
			int optionIndex = 0;
			foreach (string[] shipOption in currentSection.ShipOptions)
			{
				Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
				int num = 0;
				foreach (string techId in shipOption)
				{
					dictionary[techId] = false;
					this.App.UI.AddItem(this.App.UI.Path(sectionPanel, "specialList"), string.Empty, userItemId, this.App.AssetDatabase.GetLocalizedTechnologyName(techId), "ShipOptionItem");
					string itemGlobalId = this.App.UI.GetItemGlobalID(this.App.UI.Path(sectionPanel, "specialList"), string.Empty, userItemId++, string.Empty);
					this.App.UI.SetTooltip(itemGlobalId, App.Localize("@TECH_DESC_" + techId));
					this.App.UI.SetEnabled(this.App.UI.Path(itemGlobalId, "btnTech"), false);
					this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "btnTech"), "id", this.GetTechPanel(sectionType, optionIndex, techId));
					++num;
				}
				if (num > 0)
				{
					this._shipOptionGroups[sectionType].Add(dictionary);
					this.App.UI.AddItem(this.App.UI.Path(sectionPanel, "specialList"), string.Empty, userItemId++, string.Empty, "ShipOptionDivider");
					++optionIndex;
				}
			}
			this.App.UI.ShakeViolently(this.App.UI.Path(sectionPanel, "specialList"));
			this.App.UI.Reshape(sectionPanel);
		}

		private void DeactivateShipOptionsForSection(ShipSectionType type)
		{
			if (!this._shipOptionGroups.ContainsKey(type))
				return;
			int index1 = 0;
			foreach (Dictionary<string, bool> dictionary in this._shipOptionGroups[type])
			{
				foreach (string index2 in dictionary.Keys.ToList<string>())
					this._shipOptionGroups[type][index1][index2] = false;
				++index1;
			}
		}

		private void SyncSectionTechs(DesignInfo design)
		{
			ComparativeAnalysysState.Trace(" ==== Syncing Section Techs ====");
			this._shipOptionGroups.Clear();
			this.PopulateTechList("CommandDesign", ShipSectionType.Command);
			this.PopulateTechList("MissionDesign", ShipSectionType.Mission);
			this.PopulateTechList("EngineDesign", ShipSectionType.Engine);
			this.ClearTechSelections();
			foreach (DesignSectionInfo designSection in design.DesignSections)
			{
				DesignSectionInfo section = designSection;
				ShipSectionAsset shipSectionAsset = this.App.AssetDatabase.ShipSections.FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == section.FilePath));
				foreach (int tech1 in section.Techs)
				{
					string tech = this.App.GameDatabase.GetTechFileID(tech1);
					if (this._shipOptionGroups.Keys.Contains<ShipSectionType>(shipSectionAsset.Type))
					{
						Dictionary<string, bool> dictionary = this._shipOptionGroups[shipSectionAsset.Type].FirstOrDefault<Dictionary<string, bool>>((Func<Dictionary<string, bool>, bool>)(x => x.Keys.Contains<string>(tech)));
						if (dictionary != null)
						{
							int optionIndex = this._shipOptionGroups[shipSectionAsset.Type].IndexOf(dictionary);
							this.ChangeTechGroupComboSelection(shipSectionAsset.Type, optionIndex, tech, true);
						}
					}
				}
			}
			this.CheckShieldTech();
		}

		private void CheckShieldTech()
		{
			foreach (SectionShipData currentSection in this._selection.GetCurrentSections())
			{
				SectionShipData section = currentSection;
				if (this._shipOptionGroups.ContainsKey(section.Section.Type) && section.Section.FileName.Contains("shield"))
				{
					KeyValuePair<ShipSectionType, List<Dictionary<string, bool>>> keyValuePair1 = this._shipOptionGroups.FirstOrDefault<KeyValuePair<ShipSectionType, List<Dictionary<string, bool>>>>((Func<KeyValuePair<ShipSectionType, List<Dictionary<string, bool>>>, bool>)(x => x.Key == section.Section.Type));
					Dictionary<string, bool> source = keyValuePair1.Value.FirstOrDefault<Dictionary<string, bool>>((Func<Dictionary<string, bool>, bool>)(x => x.Any<KeyValuePair<string, bool>>((Func<KeyValuePair<string, bool>, bool>)(y =>
				  {
					  if (y.Key != "SLD_Structural_Fields")
						  return y.Key.Contains("SLD");
					  return false;
				  }))));
					if (source != null && !source.Any<KeyValuePair<string, bool>>((Func<KeyValuePair<string, bool>, bool>)(x => x.Value)))
					{
						int optionIndex = keyValuePair1.Value.IndexOf(source);
						KeyValuePair<string, bool> keyValuePair2 = source.First<KeyValuePair<string, bool>>((Func<KeyValuePair<string, bool>, bool>)(x => x.Key.Contains("SLD")));
						source[keyValuePair2.Key] = true;
						this.App.UI.SetChecked(this.GetTechPanel(keyValuePair1.Key, optionIndex, keyValuePair2.Key), true);
					}
				}
			}
		}

		private void RefreshCurrentShip()
		{
			if (!this._currentShipDirty || !this._screenReady)
				return;
			this.App.PostEnableSpeechSounds(false);
			this.EnableWeaponTestMode(false);
			this._input.SelectedID = 0;
			this._currentShipDirty = false;
			this._camera.TargetID = 0;
			this._camera.DesiredYaw = MathHelper.DegreesToRadians(-90f);
			this._camera.DesiredPitch = MathHelper.DegreesToRadians(-90f);
			ComparativeAnalysysState.Trace("Loading ship:");
			ComparativeAnalysysState.Trace("Faction : \"" + this._selection.GetCurrentFactionName() + "\".");
			ComparativeAnalysysState.Trace("Command : \"" + this._selection.GetCurrentSectionAssetName(ShipSectionType.Command) + "\".");
			ComparativeAnalysysState.Trace("Mission : \"" + this._selection.GetCurrentSectionAssetName(ShipSectionType.Mission) + "\".");
			ComparativeAnalysysState.Trace("Engine :  \"" + this._selection.GetCurrentSectionAssetName(ShipSectionType.Engine) + "\".");
			bool flag1 = this._selection.GetCurrentSection(ShipSectionType.Command) != null;
			bool flag2 = this._selection.GetCurrentSection(ShipSectionType.Engine) != null;
			this.App.UI.SetVisible("gameCommandList", flag1);
			this.App.UI.SetVisible("CommandDesign", flag1);
			this.App.UI.SetVisible("gameCommandListBG", flag1);
			this.App.UI.SetVisible("gameEngineList", flag2);
			this.App.UI.SetVisible("EngineDesign", flag2);
			this.App.UI.SetVisible("gameEngineListBG", flag2);
			string text1 = App.Localize(this._selection.GetCurrentSection(ShipSectionType.Mission).Description);
			string text2 = flag1 ? App.Localize(this._selection.GetCurrentSection(ShipSectionType.Command).Description) : string.Empty;
			string text3 = flag2 ? App.Localize(this._selection.GetCurrentSection(ShipSectionType.Engine).Description) : string.Empty;
			this.App.UI.SetTooltip("gameCommandList.expand", text2);
			this.App.UI.SetTooltip("gameMissionList.expand", text1);
			this.App.UI.SetTooltip("gameEngineList.expand", text3);
			DesignInfo design = this.SummarizeDesign(this._SelectedPlayer, false);
			bool flag3 = this.App.GameDatabase.GetPlayerTechInfos(this._SelectedPlayer).Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x =>
		   {
			   if (x.State == TechStates.Researched)
				   return x.TechFileID == "ENG_Orbital_Drydocks";
			   return false;
		   }));
			if (design != null)
			{
				DesignInfo olddesign = (DesignInfo)null;
				if (this._selectedDesign.HasValue)
					olddesign = this.App.GameDatabase.GetDesignInfo(this._selectedDesign.Value);
				if (olddesign != null)
				{
					this.App.UI.SetVisible("designNameTag", true);
					bool flag4 = olddesign.GetRealShipClass().HasValue && (olddesign.GetRealShipClass().Value != RealShipClasses.Drone && olddesign.GetRealShipClass().Value != RealShipClasses.AssaultShuttle);
					this.App.UI.SetVisible("gameRetrofitButton", flag3 && flag4);
					this.App.UI.SetEnabled("gameRetrofitButton", flag3 && !this._RetrofitMode && (olddesign.isPrototyped && olddesign.DesignDate != this.App.GameDatabase.GetTurnCount()) && flag4);
					if (this._RetrofitMode)
						this.App.UI.SetText("RetrofitCostPanel.game_designretrofitcost", Kerberos.Sots.StarFleet.StarFleet.CalculateRetrofitCost(this.App, olddesign, this.SummarizeDesign(this.App.LocalPlayer.ID, false)).ToString());
					this.App.UI.SetVisible("RetrofitCostPanel", this._RetrofitMode);
					if (this._RetrofitMode)
						this.App.UI.SetText(this.App.UI.Path("gameCommitButton"), App.Localize("@UI_DESIGN_RETROFIT_SUBMIT"));
					else
						this.App.UI.SetText(this.App.UI.Path("gameCommitButton"), App.Localize("@UI_DESIGN_SUBMIT_DESIGN"));
					if (olddesign.isPrototyped)
					{
						this.App.UI.SetVisible("ShipCost", false);
						this.App.UI.SetVisible("ShipProductionCost", true);
						ShipDesignUI.SyncCost(this.App, "ShipProductionCost", design);
						this.App.UI.SetText(this.App.UI.Path("ShipProductionCost", "gameShipsProduced"), this.App.GameDatabase.GetNumShipsBuiltFromDesign(this._selectedDesign.Value).ToString());
						this.App.UI.SetText(this.App.UI.Path("ShipProductionCost", "gameShipsDestroyed"), this.App.GameDatabase.GetNumShipsDestroyedFromDesign(this._selectedDesign.Value).ToString());
						this.App.UI.SetText(this.App.UI.Path("ShipProductionCost", "gameDesignComissionHeader"), string.Format(App.Localize("@UI_DESIGN_DATE_HEADER"), (object)olddesign.DesignDate));
						this.App.UI.SetText(this.App.UI.Path("designNameTag"), olddesign.Name);
					}
					else
					{
						this.App.UI.SetVisible("ShipCost", true);
						this.App.UI.SetVisible("ShipProductionCost", false);
						ShipDesignUI.SyncCost(this.App, "ShipCost", design);
						this.App.UI.SetText(this.App.UI.Path("designNameTag"), olddesign.Name);
					}
					if (olddesign.isAttributesDiscovered)
					{
						IEnumerable<SectionEnumerations.DesignAttribute> attributesForDesign = this.App.GameDatabase.GetDesignAttributesForDesign(olddesign.ID);
						if (attributesForDesign.Count<SectionEnumerations.DesignAttribute>() > 0)
						{
							this.App.UI.SetVisible("attributeNameTagPanel", true);
							this.App.UI.SetText("attributeNameTagPanel.attributeNameTag", App.Localize("@UI_" + attributesForDesign.First<SectionEnumerations.DesignAttribute>().ToString()));
							this.App.UI.SetTooltip("attributeNameTagPanel", App.Localize("@UI_" + attributesForDesign.First<SectionEnumerations.DesignAttribute>().ToString() + "_TOOLTIP"));
						}
						else
							this.App.UI.SetVisible("attributeNameTagPanel", false);
					}
					else
						this.App.UI.SetVisible("attributeNameTagPanel", false);
				}
				else
				{
					this.App.UI.SetVisible("RetrofitCostPanel", false);
					this.App.UI.SetVisible("designNameTag", false);
					this.App.UI.SetVisible("attributeNameTagPanel", false);
					this.App.UI.SetVisible("ShipCost", true);
					this.App.UI.SetVisible("ShipProductionCost", false);
					this.App.UI.SetVisible("gameRetrofitButton", false);
					this.App.UI.SetText(this.App.UI.Path("gameCommitButton"), App.Localize("@UI_DESIGN_SUBMIT_DESIGN"));
					ShipDesignUI.SyncCost(this.App, "ShipCost", design);
				}
				ShipDesignUI.SyncSpeed(this.App, design);
				ShipDesignUI.SyncSupplies(this.App, design);
				this.SyncSectionTechs(design);
				foreach (DesignSectionInfo designSection in design.DesignSections)
				{
					DesignSectionInfo section = designSection;
					ShipSectionAsset section1 = this.App.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == section.FilePath));
					ShipSectionType sectionType = section1 != null ? section1.Type : ShipSectionType.Mission;
					this.SetSelectedSection(section1, sectionType, string.Empty, false, design);
					this.App.UI.SetText(ComparativeAnalysysState.UISectionList(sectionType), App.Localize(section1.Title));
				}
				design = this.SummarizeDesign(this._SelectedPlayer, false);
			}
			if (this._targetArena != null)
			{
				RealShipClasses? currentClass = this._selection.GetCurrentClass();
				if (currentClass.HasValue)
					this._targetArena.SetShipClass(currentClass.Value);
			}
			if (!this._selection.IsCurrentShipDataValid())
			{
				ComparativeAnalysysState.Warn("Uninitialized data no ship switch can be completed.");
			}
			else
			{
				ComparativeAnalysysState.Trace("Loading ship start");
				this._builder.New(this.App.GetPlayer(this._SelectedPlayer), (IEnumerable<ShipSectionAsset>)((IEnumerable<ShipSectionAsset>)new ShipSectionAsset[3]
				{
		  this._selection.GetCurrentSection(ShipSectionType.Command),
		  this._selection.GetCurrentSection(ShipSectionType.Mission),
		  this._selection.GetCurrentSection(ShipSectionType.Engine)
				}).Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x != null)).ToArray<ShipSectionAsset>(), this.App.AssetDatabase.TurretHousings, this.App.AssetDatabase.Weapons, Enumerable.Empty<LogicalWeapon>(), (IEnumerable<WeaponAssignment>)this._selection.GetWeaponAssignments().ToList<WeaponAssignment>(), this.App.AssetDatabase.Modules, this.App.AssetDatabase.ModulesToAssignByDefault, (IEnumerable<ModuleAssignment>)this._selection.GetModuleAssignments().ToList<ModuleAssignment>(), this.App.AssetDatabase.Psionics, design.DesignSections, this._selection.GetCurrentFaction(), design.Name, this._selectedDesignPW);
				ComparativeAnalysysState.Trace("Ship swap");
				if (this._builder.Ship == null)
					return;
				this._builder.Ship.PostSetProp("StopAnims");
				this._builder.Ship.PostSetProp("SetValidateCurrentPosition", false);
				this._builder.Ship.Maneuvering.PostSetProp("CanAvoid", false);
				this._builder.Ship.PostSetProp("SetDisableLaunching", true);
				this._builder.Ship.PostSetProp("SetDisableAutoLaunching", true);
				this._input.SelectedID = this._builder.Ship.ObjectID;
			}
		}

		private static void Warn(string message)
		{
			App.Log.Warn(message, "design");
		}

		private static void Trace(string message)
		{
			App.Log.Trace(message, "design");
		}

		public bool OnKeyBindPressed(HotKeyManager.HotKeyActions action, string gamestates)
		{
			if (gamestates.Contains(this.Name))
			{
				switch (action)
				{
					case HotKeyManager.HotKeyActions.State_Starmap:
						this.App.UI.LockUI();
						this.App.SwitchGameState<StarMapState>();
						return true;
					case HotKeyManager.HotKeyActions.State_BuildScreen:
						return false;
					case HotKeyManager.HotKeyActions.State_DesignScreen:
						this.App.UI.LockUI();
						this.App.SwitchGameState<DesignScreenState>((object)false, (object)this.Name);
						return true;
					case HotKeyManager.HotKeyActions.State_ResearchScreen:
						this.App.UI.LockUI();
						this.App.SwitchGameState<ResearchScreenState>();
						return true;
					case HotKeyManager.HotKeyActions.State_ComparativeAnalysysScreen:
						return false;
					case HotKeyManager.HotKeyActions.State_EmpireSummaryScreen:
						this.App.UI.LockUI();
						this.App.SwitchGameState<EmpireSummaryState>();
						return true;
					case HotKeyManager.HotKeyActions.State_SotspediaScreen:
						this.App.UI.LockUI();
						this.App.SwitchGameState<SotspediaState>();
						return true;
					case HotKeyManager.HotKeyActions.State_DiplomacyScreen:
						this.App.UI.LockUI();
						this.App.SwitchGameState<DiplomacyScreenState>();
						return true;
				}
			}
			return false;
		}

		private class CombatShipQueue
		{
			private float _AIoffsetz = -3000f;
			private readonly XmlDocument _config = CombatConfig.CreateEmptyCombatConfigXml();
			private float _offset;
			private float _offsetz;
			private float _AIoffset;

			public void AddDesign(App game, ShipBuilder builder, int count, int playerID)
			{
				for (int index = 0; index < count; ++index)
				{
					List<WeaponAssignment> weaponAssignmentList = new List<WeaponAssignment>();
					if (builder.Ship != null)
					{
						foreach (WeaponBank weaponBank in builder.Ship.WeaponBanks)
							weaponAssignmentList.Add(new WeaponAssignment()
							{
								ModuleNode = "",
								Bank = weaponBank.LogicalBank,
								Weapon = weaponBank.Weapon,
								DesignID = weaponBank.DesignID,
								InitialFireMode = new int?(weaponBank.FireMode),
								InitialTargetFilter = new int?(weaponBank.TargetFilter)
							});
					}
					List<ModuleAssignment> moduleAssignmentList = new List<ModuleAssignment>();
					if (builder.Ship != null)
					{
						foreach (Module module in builder.Ship.Modules)
							moduleAssignmentList.Add(new ModuleAssignment()
							{
								ModuleMount = module.Attachment,
								Module = module.LogicalModule,
								PsionicAbilities = (SectionEnumerations.PsionicAbility[])null
							});
					}
					IEnumerable<string> sectionFileNames = builder.Sections.Select<ShipSectionAsset, string>((Func<ShipSectionAsset, string>)(x => x.FileName));
					Vector3 position = new Vector3(this._offset, 0.0f, this._offsetz);
					Vector3 rotation = new Vector3(0.0f, 0.0f, 0.0f);
					if (playerID != game.LocalPlayer.ID)
					{
						position.X = this._AIoffset;
						position.Z = this._AIoffsetz;
						rotation.X = MathHelper.DegreesToRadians(180f);
						this._AIoffset += 500f;
						if ((double)this._AIoffset >= 1999.0)
						{
							this._AIoffsetz -= 500f;
							this._AIoffset = 0.0f;
						}
					}
					else
					{
						this._offset += 500f;
						if ((double)this._offset >= 1999.0)
						{
							this._offsetz += 500f;
							this._offset = 0.0f;
						}
					}
					CombatConfig.GetGameObjectsElement(this._config).AppendChild((XmlNode)CombatConfig.ExportXmlElementFromShipParameters(game, this._config, sectionFileNames, (IEnumerable<WeaponAssignment>)weaponAssignmentList, (IEnumerable<ModuleAssignment>)moduleAssignmentList, playerID, position, rotation));
				}
				ComparativeAnalysysState.CombatShipQueue.SetText(game, "Added ship " + (object)CombatConfig.GetGameObjectsElement(this._config).ChildNodes.Count);
			}

			public void Start(App game, string levelFile, int system)
			{
				XmlDocument xmlDocument;
				if (!string.IsNullOrWhiteSpace(levelFile))
				{
					xmlDocument = new XmlDocument();
					xmlDocument.Load(ScriptHost.FileSystem, levelFile);
				}
				else
					xmlDocument = CombatConfig.CreateEmptyCombatConfigXml();
				CombatConfig.AppendConfigXml(xmlDocument, this._config);
				if (!string.Equals("data/scratch_combat.xml", levelFile))
					xmlDocument.Save(Path.Combine(game.GameRoot, "data/scratch_combat.xml"));
				CombatState gameState = game.GetGameState<CombatState>();
				game.SwitchGameState((GameState)gameState, (object)system, (object)xmlDocument);
				ComparativeAnalysysState.CombatShipQueue.SetText(game, "Starting combat!");
			}

			private static void SetText(App game, string message)
			{
				App.Log.Trace(message, "design");
				game.UI.SetText("combatStatus", message);
			}
		}

		private class BankFilter
		{
			public object Value { get; set; }

			public bool Enabled { get; set; }
		}
	}
}
