// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.DefenseManagerState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.GameStates
{
	internal class DefenseManagerState : BasicStarSystemState, IKeyBindListener
	{
		private static readonly string UIExitButton = "gameExitButton";
		private const string UIMissionAdmiralName = "gameAdmiralName";
		private const string UIMissionAdmiralFleet = "gameAdmiralFleet";
		private const string UIMissionAdmiralSkills = "gameAdmiralSkills";
		private const string UIMissionAdmiralAvatar = "gameAdmiralAvatar";
		protected const int UIItemMissionTotalTime = 0;
		protected const int UIItemMissionTravelTime = 1;
		protected const int UIItemMissionTime = 2;
		protected const int UIItemMissionBuildTime = 3;
		protected const int UIItemMissionCostSeparator = 4;
		protected const int UIItemMissionCost = 5;
		protected const int UIItemMissionSupportTime = 6;
		private int _targetSystemID;
		private int _selectedPlanetID;
		private GameObjectSet _dmcrits;
		private DefenseManager _manager;
		private List<IGameObject> _pendingObjects;
		private bool _finishing;
		private FleetWidget _fleetWidget;
		private DefenseWidget _defenseWidget;
		private CombatInput _input;

		public DefenseManagerState(App game)
		  : base(game)
		{
		}

		protected override void OnBack()
		{
		}

		protected override void OnPrepare(GameState prev, object[] stateParams)
		{
			if (this.App.GameDatabase == null)
			{
				this.App.NewGame();
				this._targetSystemID = this.App.GameDatabase.GetPlayerHomeworld(this.App.LocalPlayer.ID).SystemID;
				DesignInfo design = new DesignInfo();
				design.PlayerID = this.App.LocalPlayer.ID;
				design.DesignSections = new DesignSectionInfo[1];
				design.DesignSections[0] = new DesignSectionInfo()
				{
					DesignInfo = design
				};
				design.DesignSections[0].FilePath = "factions\\" + this.App.LocalPlayer.Faction.Name + "\\sections\\sn_drone_satellite.section";
				int designID = this.App.GameDatabase.InsertDesignByDesignInfo(design);
				int id = this.App.GameDatabase.InsertOrGetDefenseFleetInfo(this._targetSystemID, this.App.LocalPlayer.ID).ID;
				for (int index = 0; index < 5; ++index)
					this.App.GameDatabase.InsertShip(id, designID, null, (ShipParams)0, new int?(), 0);
			}
			this._targetSystemID = ((IEnumerable<object>)stateParams).Count<object>() <= 0 ? this.App.GameDatabase.GetPlayerHomeworld(this.App.LocalPlayer.ID).SystemID : (int)stateParams[0];
			PlanetInfo planetInfo = this.App.GameDatabase.GetPlanetInfosOrbitingStar(this._targetSystemID).FirstOrDefault<PlanetInfo>();
			this._selectedPlanetID = planetInfo == null ? 0 : planetInfo.ID;
			base.OnPrepare(prev, new object[2]
			{
		(object) this._targetSystemID,
		(object) this._selectedPlanetID
			});
			this._pendingObjects = new List<IGameObject>();
			this._manager = new DefenseManager(this.App);
			this._dmcrits = new GameObjectSet(this.App);
			this.App.UI.LoadScreen("DefenseManager");
			this._fleetWidget = new FleetWidget(this.App, "DefenseManager.gameFleetList");
			bool enabled = true;
			if (((IEnumerable<object>)stateParams).Count<object>() >= 2)
				enabled = (bool)stateParams[1];
			this._fleetWidget.EnableRightClick = enabled;
			this._fleetWidget.SetEnabled(enabled);
			this._defenseWidget = new DefenseWidget(this.App, "defenseItemTray");
			this._fleetWidget.OnFleetsModified += new FleetWidget.FleetsModifiedDelegate(this.FleetsModified);
		}

		private void FleetsModified(App app, int[] modifiedFleetIds)
		{
			this._defenseWidget.SetSyncedFleet(this.App.GameDatabase.InsertOrGetDefenseFleetInfo(this._targetSystemID, this.App.LocalPlayer.ID).ID);
		}

		protected override void OnEnter()
		{
			base.OnEnter();
			this.App.UI.SetScreen("DefenseManager");
			this._finishing = false;
			this._dmcrits.Activate();
			this.Camera.DesiredDistance = 150000f;
			this.Camera.DesiredPitch = MathHelper.DegreesToRadians(-45f);
			this.Camera.MaxDistance = 500000f;
			this._manager.PostSetProp("SetStarSystem", (IGameObject)this._starsystem);
			this._manager.PostSetProp("SyncFleetList", this.App.UI.Path("fleetDetailsWidget", "gameFleetList"));
			this._manager.PostSetProp("LocalPlayerObjectID", this.App.LocalPlayer.ObjectID);
			this._manager.PostSetProp("SetFleetWidget", (IGameObject)this._fleetWidget);
			this._manager.PostSetProp("SetDefenseWidget", (IGameObject)this._defenseWidget);
			this._manager.Active = true;
			float width = this.App.AssetDatabase.MineFieldParams.Width;
			float length = this.App.AssetDatabase.MineFieldParams.Length;
			this._manager.PostSetProp("SetMinefieldSize", (object)(Math.Sqrt((double)length * (double)length + (double)width * (double)width) + 500.0), (object)(double)this.App.AssetDatabase.PolicePatrolRadius);
			this.SyncFleetShipModels();
			this._starsystem.PostSetProp("AutoDrawEnabled", false);
			this._starsystem.PostSetProp("ZoneMapEnabled", true);
			this._starsystem.PostSetProp("ZoneFocusEnabled", true);
			this._input = new CombatInput();
			this._dmcrits.Add((IGameObject)this._input);
			this._fleetWidget.PreferredSelectMode = true;
			List<FleetInfo> list = this.App.GameDatabase.GetFleetsByPlayerAndSystem(this.App.LocalPlayer.ID, this._targetSystemID, FleetType.FL_NORMAL | FleetType.FL_RESERVE | FleetType.FL_DEFENSE).ToList<FleetInfo>();
			list.RemoveAll((Predicate<FleetInfo>)(x => Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(this.App.Game, x)));
			this._fleetWidget.SetSyncedFleets(list);
			if (this._targetSystemID != 0)
			{
				this._fleetWidget.ListStations = true;
				this._fleetWidget.SetSyncedStations(this.App.GameDatabase.GetStationForSystemAndPlayer(this._targetSystemID, this.App.LocalPlayer.ID).ToList<StationInfo>());
			}
			this._defenseWidget.SetSyncedFleet(this.App.GameDatabase.InsertOrGetDefenseFleetInfo(this._targetSystemID, this.App.LocalPlayer.ID).ID);
			this.SyncPlanetTypeInfo();
			this.SyncSDBSlots();
			this.App.HotKeyManager.AddListener((IKeyBindListener)this);
		}

		private void SyncPlanetTypeInfo()
		{
			foreach (PlanetInfo systemPlanetInfo in (IEnumerable<PlanetInfo>)this.App.GameDatabase.GetStarSystemPlanetInfos(this._targetSystemID))
				this._manager.PostSetProp("DefenseSyncPlanetInfo", (object)systemPlanetInfo.ID, (object)systemPlanetInfo.Type);
		}

		public void SyncSDBSlots()
		{
			FleetInfo fleetInfo = this.App.GameDatabase.GetFleetInfo(this._defenseWidget.GetSynchedFleet());
			if (fleetInfo == null)
				return;
			foreach (ShipInfo shipInfo in this.App.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, true))
			{
				bool flag = false;
				SDBInfo sdbInfo = (SDBInfo)null;
				foreach (DesignSectionInfo designSection in shipInfo.DesignInfo.DesignSections)
				{
					if (designSection.FilePath.ToLower().Contains("_sdb"))
					{
						flag = true;
						sdbInfo = this.App.GameDatabase.GetSDBInfoFromShip(shipInfo.ID);
						break;
					}
				}
				if (flag && sdbInfo != null)
					this._manager.PostSetProp(nameof(SyncSDBSlots), (object)sdbInfo.ShipId, (object)sdbInfo.OrbitalId);
			}
		}

		private void SyncFleetShipModels()
		{
			foreach (FleetInfo fleetInfo in this.App.GameDatabase.GetFleetsByPlayerAndSystem(this.App.LocalPlayer.ID, this._targetSystemID, FleetType.FL_ALL))
			{
				if (fleetInfo.PlayerID == this.App.LocalPlayer.ID)
				{
					IEnumerable<ShipInfo> source = this.App.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, true).Where<ShipInfo>((Func<ShipInfo, bool>)(x => !((IEnumerable<DesignSectionInfo>)x.DesignInfo.DesignSections).Any<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(y => ShipSectionAsset.IsBattleRiderClass(y.ShipSectionAsset.RealClass)))));
					this._manager.PostSetProp("AddFleet", (object)fleetInfo.ID, (object)source.Count<ShipInfo>());
					foreach (ShipInfo shipInfo in source)
					{
						List<object> objectList = new List<object>();
						objectList.Add((object)0);
						ShipDummy state = new ShipDummy(this.App, CreateShipDummyParams.ObtainShipDummyParams(this.App, shipInfo));
						this.App.AddExistingObject((IGameObject)state, objectList.ToArray());
						this._manager.PostObjectAddObjects((IGameObject)state);
						this._pendingObjects.Add((IGameObject)state);
						Vector3? shipFleetPosition = this.App.GameDatabase.GetShipFleetPosition(shipInfo.ID);
						Matrix? shipSystemPosition = this.App.GameDatabase.GetShipSystemPosition(shipInfo.ID);
						state.PostSetProp("SetShipID", shipInfo.ID);
						int commandPointCost = this.App.GameDatabase.GetCommandPointCost(shipInfo.DesignID);
						state.PostSetProp("SetShipCommandCost", commandPointCost);
						state.PostSetProp("SetFleetID", fleetInfo.ID);
						state.PostSetProp("SetShipName", shipInfo.ShipName);
						if (shipFleetPosition.HasValue)
							state.PostSetProp("SetFleetPosition", (object)shipFleetPosition.Value.X, (object)shipFleetPosition.Value.Y, (object)shipFleetPosition.Value.Z);
						if (shipSystemPosition.HasValue)
							state.PostSetProp("SetSystemTransform", shipSystemPosition.Value);
					}
				}
			}
		}

		protected override void OnUpdate()
		{
			List<IGameObject> gameObjectList = new List<IGameObject>();
			foreach (IGameObject pendingObject in this._pendingObjects)
			{
				if (pendingObject.ObjectStatus == GameObjectStatus.Ready)
				{
					if (pendingObject is IActive)
						(pendingObject as IActive).Active = true;
					this._dmcrits.Add(pendingObject);
					gameObjectList.Add(pendingObject);
				}
			}
			foreach (IGameObject gameObject in gameObjectList)
				this._pendingObjects.Remove(gameObject);
			if (this._fleetWidget == null || !this._fleetWidget.DefenseFleetUpdated || this._defenseWidget == null)
				return;
			this._fleetWidget.DefenseFleetUpdated = false;
			this._defenseWidget.SetSyncedFleet(this.App.GameDatabase.InsertOrGetDefenseFleetInfo(this._targetSystemID, this.App.LocalPlayer.ID).ID);
		}

		protected override void OnExit(GameState prev, ExitReason reason)
		{
			this.App.HotKeyManager.RemoveListener((IKeyBindListener)this);
			this._pendingObjects.Clear();
			this._fleetWidget.Dispose();
			this._defenseWidget.Dispose();
			this._dmcrits.Dispose();
			this._dmcrits = (GameObjectSet)null;
			this._manager.Dispose();
			base.OnExit(prev, reason);
		}

		public override bool IsReady()
		{
			if (!this._dmcrits.IsReady())
				return false;
			return base.IsReady();
		}

		protected override void OnUIGameEvent(string eventName, string[] eventParams)
		{
		}

		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
			switch (messageID)
			{
				case InteropMessageID.IMID_SCRIPT_SYNC_DEFENSE_POSITIONS:
					int num1 = mr.ReadInteger();
					for (int index1 = 0; index1 < num1; ++index1)
					{
						mr.ReadInteger();
						int num2 = mr.ReadInteger();
						for (int index2 = 0; index2 < num2; ++index2)
						{
							if (mr.ReadBool())
							{
								bool flag = mr.ReadBool();
								this.App.GameDatabase.UpdateShipSystemPosition(mr.ReadInteger(), !flag ? new Matrix?() : new Matrix?(new Matrix(mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle())));
							}
						}
					}
					if (this._finishing)
					{
						this.App.SwitchGameState<StarMapState>();
						break;
					}
					break;
				case InteropMessageID.IMID_SCRIPT_SYNC_DEFENSEBOAT_DATA:
					int shipID = mr.ReadInteger();
					int OrbitalID = mr.ReadInteger();
					this.App.GameDatabase.RemoveSDBByShipID(shipID);
					if (OrbitalID != 0)
					{
						this.App.GameDatabase.InsertSDB(OrbitalID, shipID);
						break;
					}
					break;
			}
			base.OnEngineMessage(messageID, mr);
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "list_sel_changed")
			{
				if (!(panelName == "gameFleetList") || string.IsNullOrEmpty(msgParams[0]))
					return;
				int.Parse(msgParams[0]);
			}
			else if (msgType == "button_clicked")
			{
				if (!(panelName == DefenseManagerState.UIExitButton))
					return;
				this._manager.PostSetProp("SyncSystemPositions");
				this._finishing = true;
			}
			else if (!(msgType == "DragAndDropEvent"))
				;
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
						this.App.UI.LockUI();
						this.App.SwitchGameState<BuildScreenState>((object)this._targetSystemID);
						return true;
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
	}
}
