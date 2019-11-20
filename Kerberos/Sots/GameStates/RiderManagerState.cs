// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.RiderManagerState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.GameStates
{
	internal class RiderManagerState : GameState, IKeyBindListener
	{
		private static readonly string UIExitButton = "gameCancelMissionButton";
		private RiderManager _manager;
		private int _targetSystemID;
		private List<ShipDummy> _shipDummies;
		private List<IGameObject> _pendingObjects;
		private GameObjectSet _crits;
		private OrbitCameraController _camera;
		private FleetWidget _fleetWidget;

		public RiderManagerState(App game)
		  : base(game)
		{
		}

		public static bool CanOpen(GameSession sim, int systemID)
		{
			if (sim == null || systemID <= 0)
				return false;
			return sim.GameDatabase.GetFleetInfoBySystemID(systemID, FleetType.FL_NORMAL | FleetType.FL_RESERVE | FleetType.FL_DEFENSE).Any<FleetInfo>((Func<FleetInfo, bool>)(x => x.PlayerID == sim.LocalPlayer.ID));
		}

		protected override void OnPrepare(GameState prev, object[] stateParams)
		{
			if (this.App.GameDatabase == null)
			{
				this.App.NewGame();
				this._targetSystemID = ((IEnumerable<object>)stateParams).Count<object>() <= 0 ? this.App.GameDatabase.GetPlayerHomeworld(this.App.LocalPlayer.ID).SystemID : (int)stateParams[0];
				DesignInfo design1 = new DesignInfo();
				design1.PlayerID = this.App.LocalPlayer.ID;
				design1.DesignSections = new DesignSectionInfo[2];
				design1.DesignSections[0] = new DesignSectionInfo();
				design1.DesignSections[0].FilePath = "factions\\" + this.App.LocalPlayer.Faction.Name + "\\sections\\br_eng_fusion.section";
				design1.DesignSections[1] = new DesignSectionInfo();
				design1.DesignSections[1].FilePath = "factions\\" + this.App.LocalPlayer.Faction.Name + "\\sections\\br_msn_spinal.section";
				int designID1 = this.App.GameDatabase.InsertDesignByDesignInfo(design1);
				int? reserveFleetId = this.App.GameDatabase.GetReserveFleetID(this.App.LocalPlayer.ID, this._targetSystemID);
				for (int index = 0; index < 5; ++index)
					this.App.GameDatabase.InsertShip(reserveFleetId.Value, designID1, null, (ShipParams)0, new int?(), 0);
				DesignInfo design2 = new DesignInfo();
				design2.PlayerID = this.App.LocalPlayer.ID;
				design2.DesignSections = new DesignSectionInfo[2];
				design2.DesignSections[0] = new DesignSectionInfo();
				design2.DesignSections[0].FilePath = "factions\\" + this.App.LocalPlayer.Faction.Name + "\\sections\\br_eng_fusion.section";
				design2.DesignSections[1] = new DesignSectionInfo();
				design2.DesignSections[1].FilePath = "factions\\" + this.App.LocalPlayer.Faction.Name + "\\sections\\br_msn_scout.section";
				int designID2 = this.App.GameDatabase.InsertDesignByDesignInfo(design2);
				for (int index = 0; index < 5; ++index)
					this.App.GameDatabase.InsertShip(reserveFleetId.Value, designID2, null, (ShipParams)0, new int?(), 0);
				DesignInfo design3 = new DesignInfo();
				design3.PlayerID = this.App.LocalPlayer.ID;
				design3.Name = "My Fun Design Has A Long Ass Name";
				design3.Role = ShipRole.CARRIER;
				design3.DesignSections = new DesignSectionInfo[3];
				design3.DesignSections[0] = new DesignSectionInfo();
				design3.DesignSections[0].FilePath = "factions\\" + this.App.LocalPlayer.Faction.Name + "\\sections\\cr_eng_fusion.section";
				design3.DesignSections[1] = new DesignSectionInfo();
				design3.DesignSections[1].FilePath = "factions\\" + this.App.LocalPlayer.Faction.Name + "\\sections\\cr_mis_brcarrier.section";
				design3.DesignSections[2] = new DesignSectionInfo();
				design3.DesignSections[2].FilePath = "factions\\" + this.App.LocalPlayer.Faction.Name + "\\sections\\cr_cmd_assault.section";
				int designID3 = this.App.GameDatabase.InsertDesignByDesignInfo(design3);
				Random random = new Random();
				for (int index = 0; index < 5; ++index)
				{
					foreach (SectionInstanceInfo shipSectionInstance in this.App.GameDatabase.GetShipSectionInstances(this.App.GameDatabase.InsertShip(reserveFleetId.Value, designID3, null, (ShipParams)0, new int?(), 0)))
					{
						shipSectionInstance.Structure = (int)((double)shipSectionInstance.Structure * (double)random.NextSingle());
						this.App.GameDatabase.UpdateSectionInstance(shipSectionInstance);
					}
				}
				DesignInfo design4 = new DesignInfo();
				design4.PlayerID = this.App.LocalPlayer.ID;
				design4.Name = "Repair Dem";
				design4.Role = ShipRole.CARRIER;
				design4.DesignSections = new DesignSectionInfo[3];
				design4.DesignSections[0] = new DesignSectionInfo();
				design4.DesignSections[0].FilePath = "factions\\" + this.App.LocalPlayer.Faction.Name + "\\sections\\cr_eng_fusion.section";
				design4.DesignSections[1] = new DesignSectionInfo();
				design4.DesignSections[1].FilePath = "factions\\" + this.App.LocalPlayer.Faction.Name + "\\sections\\cr_mis_repair.section";
				design4.DesignSections[2] = new DesignSectionInfo();
				design4.DesignSections[2].FilePath = "factions\\" + this.App.LocalPlayer.Faction.Name + "\\sections\\cr_cmd_assault.section";
				int designID4 = this.App.GameDatabase.InsertDesignByDesignInfo(design4);
				for (int index = 0; index < 3; ++index)
					this.App.GameDatabase.InsertShip(reserveFleetId.Value, designID4, null, (ShipParams)0, new int?(), 0);
				DesignInfo designInfo = new DesignInfo();
				design4.PlayerID = this.App.LocalPlayer.ID;
				design4.Name = "Little MEEP";
				design4.DesignSections = new DesignSectionInfo[2];
				design4.DesignSections[0] = new DesignSectionInfo();
				design4.DesignSections[0].FilePath = "factions\\" + this.App.LocalPlayer.Faction.Name + "\\sections\\br_eng_fusion.section";
				design4.DesignSections[1] = new DesignSectionInfo();
				design4.DesignSections[1].FilePath = "factions\\" + this.App.LocalPlayer.Faction.Name + "\\sections\\br_msn_spinal.section";
				this.App.GameDatabase.InsertDesignByDesignInfo(design4);
				DesignInfo design5 = new DesignInfo();
				design5.PlayerID = this.App.LocalPlayer.ID;
				design5.Name = "My Fun Leviathan";
				design5.Role = ShipRole.CARRIER;
				design5.Class = ShipClass.Leviathan;
				design5.DesignSections = new DesignSectionInfo[1];
				design5.DesignSections[0] = new DesignSectionInfo();
				design5.DesignSections[0].FilePath = "factions\\" + this.App.LocalPlayer.Faction.Name + "\\sections\\lv_carrier.section";
				int designID5 = this.App.GameDatabase.InsertDesignByDesignInfo(design5);
				for (int index = 0; index < 5; ++index)
					this.App.GameDatabase.InsertShip(reserveFleetId.Value, designID5, null, (ShipParams)0, new int?(), 0);
			}
			else
				this._targetSystemID = ((IEnumerable<object>)stateParams).Count<object>() <= 0 ? this.App.GameDatabase.GetPlayerHomeworld(this.App.LocalPlayer.ID).SystemID : (int)stateParams[0];
			this.App.UI.LoadScreen("RiderManager");
			this._pendingObjects = new List<IGameObject>();
			this._shipDummies = new List<ShipDummy>();
			this._crits = new GameObjectSet(this.App);
			this._fleetWidget = new FleetWidget(this.App, "RiderManager.gameFleetWidget");
			this._fleetWidget.SeparateDefenseFleet = false;
			this._fleetWidget.ShipSelectionEnabled = true;
			this.App.UI.ClearItems("RiderManager.riderList");
		}

		protected override void OnEnter()
		{
			this.App.UI.SetScreen("RiderManager");
			this._manager = new RiderManager(this.App, "RiderManager");
			this._camera = new OrbitCameraController(this.App);
			this._camera.DesiredPitch = MathHelper.DegreesToRadians(0.0f);
			this._camera.TargetPosition = new Vector3(0.0f, 0.0f, 0.0f);
			this._camera.MaxDistance = 6000f;
			this._camera.DesiredDistance = this._camera.MaxDistance;
			this._manager.PostSetProp("CameraController", (IGameObject)this._camera);
			this._manager.PostSetActive(true);
			IEnumerable<FleetInfo> byPlayerAndSystem = this.App.GameDatabase.GetFleetsByPlayerAndSystem(this.App.LocalPlayer.ID, this._targetSystemID, FleetType.FL_ALL);
			this._manager.SetSyncedFleets(byPlayerAndSystem.ToList<FleetInfo>());
			this.SyncFleetShipModels();
			this._fleetWidget.ShipFilter += new FleetWidget.FleetWidgetShipFilter(this.FleetWidgetShipFilter);
			this._fleetWidget.ShowEmptyFleets = false;
			this._fleetWidget.SetSyncedFleets(byPlayerAndSystem.ToList<FleetInfo>());
			this._manager.PostSetProp("SetFleetWidget", (IGameObject)this._fleetWidget);
			this.App.HotKeyManager.AddListener((IKeyBindListener)this);
		}

		public FleetWidget.FilterShips FleetWidgetShipFilter(
		  ShipInfo ship,
		  DesignInfo design)
		{
			int num1 = 0;
			foreach (DesignSectionInfo designSection in design.DesignSections)
			{
				ShipSectionAsset shipSectionAsset = this.App.AssetDatabase.GetShipSectionAsset(designSection.FilePath);
				if (shipSectionAsset.BattleRiderType != BattleRiderTypes.Unspecified)
				{
					int battleRiderType = (int)shipSectionAsset.BattleRiderType;
				}
				num1 += RiderManager.GetNumRiderSlots(this.App, designSection);
			}
			int num2 = 0;
			foreach (DesignSectionInfo designSection in design.DesignSections)
				num2 += this.App.AssetDatabase.GetShipSectionAsset(designSection.FilePath).ReserveSize;
			if (num2 > 0 || num1 > 0)
				return FleetWidget.FilterShips.Enable;
			return design.Class == ShipClass.BattleRider ? FleetWidget.FilterShips.Ignore : FleetWidget.FilterShips.Ignore;
		}

		private void SyncFleetShipModels()
		{
			foreach (int syncedShip in this._manager.GetSyncedShips())
			{
				ShipInfo shipInfo = this.App.GameDatabase.GetShipInfo(syncedShip, true);
				List<object> objectList = new List<object>();
				objectList.Add((object)0);
				ShipDummy state = new ShipDummy(this.App, CreateShipDummyParams.ObtainShipDummyParams(this.App, shipInfo));
				this.App.AddExistingObject((IGameObject)state, objectList.ToArray());
				this._manager.PostObjectAddObjects((IGameObject)state);
				this._pendingObjects.Add((IGameObject)state);
				Vector3? shipFleetPosition = this.App.GameDatabase.GetShipFleetPosition(shipInfo.ID);
				state.PostSetProp("SetShipID", shipInfo.ID);
				state.PostSetProp("SetDesignID", shipInfo.DesignID);
				int commandPointCost = this.App.GameDatabase.GetShipCommandPointCost(shipInfo.ID, true);
				state.PostSetProp("SetShipCommandCost", commandPointCost);
				state.FleetID = shipInfo.FleetID;
				state.PostSetProp("SetShipName", shipInfo.ShipName);
				if (shipFleetPosition.HasValue)
					state.PostSetProp("SetFleetPosition", (object)shipFleetPosition.Value.X, (object)shipFleetPosition.Value.Y, (object)shipFleetPosition.Value.Z);
				this._shipDummies.Add(state);
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
					this._crits.Add(pendingObject);
					gameObjectList.Add(pendingObject);
				}
			}
			foreach (IGameObject gameObject in gameObjectList)
				this._pendingObjects.Remove(gameObject);
			if (this._fleetWidget == null)
				return;
			this._fleetWidget.OnUpdate();
		}

		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}

		protected override void UICommChannel_OnPanelMessage(
		  string panelName,
		  string msgType,
		  string[] msgParams)
		{
			if (!(panelName == RiderManagerState.UIExitButton))
				return;
			this.App.SwitchGameState<StarMapState>();
		}

		public override bool IsReady()
		{
			if (!this._crits.IsReady() || !base.IsReady())
				return false;
			return base.IsReady();
		}

		protected override void OnExit(GameState next, ExitReason reason)
		{
			this.App.HotKeyManager.RemoveListener((IKeyBindListener)this);
			this._manager.Dispose();
			this._fleetWidget.Dispose();
			if (this._camera != null)
			{
				this._camera.Dispose();
				this._camera = (OrbitCameraController)null;
			}
			if (this._crits == null)
				return;
			this._crits.Dispose();
			this._crits = (GameObjectSet)null;
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
