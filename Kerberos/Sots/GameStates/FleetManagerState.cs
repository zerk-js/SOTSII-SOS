// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.FleetManagerState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.GameStates
{
	internal class FleetManagerState : GameState, IKeyBindListener
	{
		protected static readonly string UICancelButton = "gameCancelMissionButton";
		protected static readonly string UICommitButton = "gameConfirmMissionButton";
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
		private OrbitCameraController _camera;
		private GameObjectSet _crits;
		private FleetManager _manager;
		private Sky _sky;
		private int _prevFleetID;
		private int _commandQuota;
		private string _contextMenuID;
		private string _shipContextMenuID;
		private int _contextSlot;
		private string _admiralManagerDialog;
		private int _contextMenuShip;
		private List<ShipDummy> _shipDummies;
		private bool _finalSync;
		private FleetWidget _fleetWidget;

		public FleetManagerState(App game)
		  : base(game)
		{
		}

		protected void OnBack()
		{
		}

		protected override void OnPrepare(GameState prev, object[] stateParams)
		{
			if (this.App.GameDatabase == null)
				this.App.NewGame();
			this._targetSystemID = ((IEnumerable<object>)stateParams).Count<object>() <= 0 ? this.App.GameDatabase.GetPlayerHomeworld(this.App.LocalPlayer.ID).SystemID : (int)stateParams[0];
			this._prevFleetID = -1;
			this._sky = new Sky(this.App, SkyUsage.InSystem, 0);
			this._crits = new GameObjectSet(this.App);
			this._crits.Add((IGameObject)this._sky);
			this._manager = new FleetManager(this.App);
			this._shipDummies = new List<ShipDummy>();
			this.SyncFleetShipModels(this._targetSystemID);
			this.App.UI.LoadScreen("FleetManager");
			this._contextMenuID = this.App.UI.CreatePanelFromTemplate("FleetManagerContextMenu", null);
			this._shipContextMenuID = this.App.UI.CreatePanelFromTemplate("FleetManagerShipContextMenu", null);
			this._fleetWidget = new FleetWidget(this.App, "FleetManager.fleetDetailsWidget.gameFleetList");
			this._fleetWidget.DisableTooltips = true;
			this._fleetWidget.SeparateDefenseFleet = false;
			this._fleetWidget.EnableMissionButtons = false;
			this._fleetWidget.OnFleetsModified += new FleetWidget.FleetsModifiedDelegate(this.FleetsModified);
			this._fleetWidget.EnableCreateFleetButton = false;
		}

		private void FleetsModified(App app, int[] modifiedFleetIds)
		{
			this.RefreshFleetShipModels();
		}

		protected override void OnEnter()
		{
			this._finalSync = false;
			this.App.UI.SetScreen("FleetManager");
			this.App.UI.UnlockUI();
			this.App.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
			this._sky.Active = true;
			this._camera = new OrbitCameraController(this.App);
			this._camera.DesiredPitch = MathHelper.DegreesToRadians(-75f);
			this._camera.TargetPosition = new Vector3(0.0f, 0.0f, 0.0f);
			this._camera.MaxDistance = 6000f;
			this._camera.DesiredDistance = this._camera.MaxDistance;
			this.App.UI.ClearItems("partFleetShips");
			this._manager.PostSetProp("CameraController", (IGameObject)this._camera);
			this._manager.PostSetProp("InputEnabled", true);
			this._manager.PostSetProp("SyncShipList", "partFleetShips");
			this._manager.PostSetProp("SyncCommandPointDisplay", "cmdpointsValue");
			this._manager.Active = true;
			this.App.GameDatabase.GetPlayerHomeworld(this.App.LocalPlayer.ID);
			IEnumerable<FleetInfo> fleetInfoBySystemId = this.App.GameDatabase.GetFleetInfoBySystemID(this._targetSystemID, FleetType.FL_ALL);
			this.SyncShipContextMenu(fleetInfoBySystemId);
			EmpireBarUI.SyncTitleFrame(this.App);
			this._fleetWidget.SetSyncedFleets(fleetInfoBySystemId.Where<FleetInfo>((Func<FleetInfo, bool>)(x =>
		   {
			   if (x.PlayerID == this.App.LocalPlayer.ID)
				   return !Kerberos.Sots.StarFleet.StarFleet.IsGardenerFleet(this.App.Game, x);
			   return false;
		   })).ToList<FleetInfo>());
			this._manager.PostSetProp("FleetWidget", this._fleetWidget.ObjectID);
			this._fleetWidget.ShipSelectionEnabled = true;
			if (this.App.LocalPlayer.Faction.Name == "loa")
			{
				this.App.UI.SetEnabled("fleetmanagerloaCompose", true);
				this.App.UI.SetVisible("fleetmanagerloaCompose", true);
				this.App.UI.SetVisible("ngpWarning", true);
			}
			else
			{
				this.App.UI.SetEnabled("fleetmanagerloaCompose", false);
				this.App.UI.SetVisible("fleetmanagerloaCompose", false);
				this.App.UI.SetVisible("ngpWarning", false);
			}
			this.App.UI.AutoSize("buttonPanel");
			this.App.UI.ForceLayout("buttonPanel");
			this.App.UI.AutoSize("fleetDetailsWidget");
			this.App.UI.ForceLayout("fleetDetailsWidget");
			this.App.UI.AutoSize("leftPanel");
			this.App.UI.ForceLayout("leftPanel");
			this.App.UI.AutoSize("buttonPanel");
			this.App.UI.ForceLayout("buttonPanel");
			this.App.UI.AutoSize("fleetDetailsWidget");
			this.App.UI.ForceLayout("fleetDetailsWidget");
			this.App.UI.AutoSize("leftPanel");
			this.App.UI.ForceLayout("leftPanel");
			this.App.HotKeyManager.AddListener((IKeyBindListener)this);
		}

		private void ShowFleetPopup(string[] eventParams)
		{
			this.App.UI.AutoSize(this._contextMenuID);
			this._contextSlot = int.Parse(eventParams[3]);
			this.App.UI.ShowTooltip(this._contextMenuID, float.Parse(eventParams[1]), float.Parse(eventParams[2]));
		}

		private void ShowShipPopup(string[] eventParams)
		{
			this._contextMenuShip = int.Parse(eventParams[3]);
			this.SyncShipContextMenu(this.App.GameDatabase.GetFleetInfoBySystemID(this._targetSystemID, FleetType.FL_NORMAL | FleetType.FL_RESERVE));
			this.App.UI.ShowTooltip(this._shipContextMenuID, float.Parse(eventParams[1]), float.Parse(eventParams[2]));
		}

		public static bool CanOpen(GameSession sim, int systemID)
		{
			if (sim == null || systemID <= 0)
				return false;
			return sim.GameDatabase.GetFleetInfoBySystemID(systemID, FleetType.FL_NORMAL | FleetType.FL_RESERVE).Any<FleetInfo>((Func<FleetInfo, bool>)(x => x.PlayerID == sim.LocalPlayer.ID));
		}

		private void SyncFleetShipModels(int systemID)
		{
			foreach (FleetInfo fleetInfo in this.App.GameDatabase.GetFleetInfoBySystemID(systemID, FleetType.FL_NORMAL | FleetType.FL_RESERVE))
			{
				if (fleetInfo.PlayerID == this.App.LocalPlayer.ID)
				{
					foreach (ShipInfo shipInfo in this.App.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, true))
					{
						DesignSectionInfo designSectionInfo = ((IEnumerable<DesignSectionInfo>)shipInfo.DesignInfo.DesignSections).FirstOrDefault<DesignSectionInfo>((Func<DesignSectionInfo, bool>)(x => x.ShipSectionAsset.Type == ShipSectionType.Mission));
						if ((designSectionInfo == null || !ShipSectionAsset.IsBattleRiderClass(designSectionInfo.ShipSectionAsset.RealClass)) && !shipInfo.DesignInfo.IsLoaCube())
						{
							List<object> objectList = new List<object>();
							objectList.Add((object)0);
							ShipDummy state = new ShipDummy(this.App, CreateShipDummyParams.ObtainShipDummyParams(this.App, shipInfo));
							this.App.AddExistingObject((IGameObject)state, objectList.ToArray());
							this._manager.PostObjectAddObjects((IGameObject)state);
							Vector3? shipFleetPosition = this.App.GameDatabase.GetShipFleetPosition(shipInfo.ID);
							state.PostSetProp("SetShipID", shipInfo.ID);
							state.PostSetProp("SetDesignID", shipInfo.DesignID);
							int commandPointCost = this.App.GameDatabase.GetShipCommandPointCost(shipInfo.ID, true);
							state.PostSetProp("SetShipCommandCost", commandPointCost);
							state.FleetID = fleetInfo.ID;
							state.PostSetProp("SetShipName", shipInfo.ShipName);
							if (shipFleetPosition.HasValue)
								state.PostSetProp("SetFleetPosition", (object)shipFleetPosition.Value.X, (object)shipFleetPosition.Value.Y, (object)shipFleetPosition.Value.Z);
							this._shipDummies.Add(state);
						}
					}
				}
			}
		}

		private void RefreshFleetShipModels()
		{
			foreach (FleetInfo fleetInfo in this.App.GameDatabase.GetFleetInfoBySystemID(this._targetSystemID, FleetType.FL_ALL).ToList<FleetInfo>())
			{
				if (fleetInfo.PlayerID == this.App.LocalPlayer.ID)
				{
					foreach (ShipInfo shipInfo in this.App.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, false))
					{
						foreach (ShipDummy shipDummy in this._shipDummies)
						{
							if (shipDummy.ShipID == shipInfo.ID && shipDummy.FleetID != shipInfo.FleetID)
							{
								shipDummy.FleetID = shipInfo.FleetID;
								shipDummy.PostSetProp("ClearFleetPosition");
							}
						}
					}
				}
			}
			if (this._fleetWidget.SelectedFleet == -1)
				return;
			this._manager.PostSetProp("LayoutGridForFleet", (object)this._fleetWidget.SelectedFleet);
		}

		private void SyncShipContextMenu(IEnumerable<FleetInfo> fleets)
		{
			int num = 0;
			foreach (FleetInfo fleet in fleets)
			{
				string panelId = this._shipContextMenuID + ".menuItem" + (object)num;
				string propertyValue = "Move to " + fleet.Name + " Fleet";
				this.App.UI.SetPropertyString(panelId + ".idle.menulabel", "text", propertyValue);
				this.App.UI.SetPropertyString(panelId + ".mouse_over.menulabel", "text", propertyValue);
				this.App.UI.SetPropertyString(panelId + ".pressed.menulabel", "text", propertyValue);
				this.App.UI.SetPropertyString(panelId + ".disabled.menulabel", "text", propertyValue);
				this.App.UI.SetVisible(panelId, true);
				++num;
			}
			for (int index = num; index < 10; ++index)
				this.App.UI.SetVisible(this._shipContextMenuID + ".menuItem" + (object)index, false);
			this.App.UI.AutoSize(this._shipContextMenuID);
		}

		private void SyncFleetShipsList(int fleetID)
		{
			this._manager.PostSetProp("FleetSelectionChanged", (object)this._prevFleetID, (object)fleetID);
			IEnumerable<ShipInfo> shipInfoByFleetId = this.App.GameDatabase.GetShipInfoByFleetID(fleetID, false);
			int num = 0;
			int val1 = 0;
			foreach (ShipInfo shipInfo in shipInfoByFleetId)
			{
				++num;
				val1 = Math.Max(val1, this.App.GameDatabase.GetDesignCommandPointQuota(this.App.AssetDatabase, shipInfo.DesignID));
			}
			this._manager.PostSetProp("SetCommandQuota", val1);
			this._commandQuota = val1;
			this._prevFleetID = fleetID;
		}

		protected override void OnUpdate()
		{
		}

		protected override void OnExit(GameState prev, ExitReason reason)
		{
			this.App.HotKeyManager.RemoveListener((IKeyBindListener)this);
			if (this._manager != null)
			{
				this._manager.Dispose();
				this._manager = (FleetManager)null;
			}
			if (this._fleetWidget != null)
			{
				this._fleetWidget.OnFleetsModified -= new FleetWidget.FleetsModifiedDelegate(this.FleetsModified);
				this._fleetWidget.Dispose();
				this._fleetWidget = (FleetWidget)null;
			}
			foreach (ShipDummy shipDummy in this._shipDummies)
				shipDummy.Dispose();
			this._shipDummies = (List<ShipDummy>)null;
			if (this._camera != null)
			{
				this._camera.Dispose();
				this._camera = (OrbitCameraController)null;
			}
			if (this._crits != null)
			{
				this._crits.Dispose();
				this._crits = (GameObjectSet)null;
			}
			this.App.UI.DestroyPanel(this._contextMenuID);
			this.App.UI.DestroyPanel(this._shipContextMenuID);
			this.App.UI.GameEvent -= new UIEventGameEvent(this.UICommChannel_GameEvent);
		}

		public override bool IsReady()
		{
			if (!this._crits.IsReady() || !base.IsReady())
				return false;
			bool flag = true;
			foreach (GameObject shipDummy in this._shipDummies)
			{
				if (shipDummy.ObjectStatus == GameObjectStatus.Pending)
					flag = false;
			}
			return flag;
		}

		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
			switch (messageID)
			{
				case InteropMessageID.IMID_SCRIPT_OBJECT_RELEASE:
					this.RemoveObject(mr);
					break;
				case InteropMessageID.IMID_SCRIPT_OBJECTS_RELEASE:
					this.RemoveObjects(mr);
					break;
				case InteropMessageID.IMID_SCRIPT_SYNC_FLEET_POSITIONS:
					mr.ReadInteger();
					int num = mr.ReadInteger();
					for (int index = 0; index < num; ++index)
					{
						bool flag = mr.ReadBool();
						this.App.GameDatabase.UpdateShipFleetPosition(mr.ReadInteger(), !flag ? new Vector3?() : new Vector3?(new Vector3(mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle())));
					}
					if (!this._finalSync)
						break;
					this.App.SwitchGameState<StarMapState>();
					break;
			}
		}

		protected void RemoveObject(ScriptMessageReader data)
		{
			this.RemoveGameObject(this.App.GetGameObject(data.ReadInteger()));
		}

		protected void RemoveObjects(ScriptMessageReader data)
		{
			for (int id = data.ReadInteger(); id != 0; id = data.ReadInteger())
				this.RemoveGameObject(this.App.GetGameObject(id));
		}

		public override void RemoveGameObject(IGameObject gameObject)
		{
			if (gameObject == null)
				return;
			IGameObject gameObject1 = this._crits.Objects.FirstOrDefault<IGameObject>((Func<IGameObject, bool>)(x =>
		   {
			   if (x.ObjectID == gameObject.ObjectID)
				   return x is IDisposable;
			   return false;
		   }));
			if (gameObject1 != null)
			{
				(gameObject1 as IDisposable).Dispose();
				this._crits.Remove(gameObject1);
			}
			else
			{
				IGameObject gameObject2 = this.App.GetGameObject(gameObject.ObjectID);
				if (gameObject2 == null)
					return;
				this.App.ReleaseObject(gameObject2);
			}
		}

		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
		}

		public void Refresh()
		{
			this.RefreshFleetShipModels();
			this._fleetWidget.SetSyncedFleets(this.App.GameDatabase.GetFleetInfoBySystemID(this._targetSystemID, FleetType.FL_ALL).Where<FleetInfo>((Func<FleetInfo, bool>)(x =>
		   {
			   if (x.Type == FleetType.FL_RESERVE)
				   return x.PlayerID == this.App.LocalPlayer.ID;
			   return true;
		   })).ToList<FleetInfo>());
		}

		protected override void UICommChannel_OnPanelMessage(
		  string panelName,
		  string msgType,
		  string[] msgParams)
		{
			if (msgType == "list_sel_changed")
			{
				if (panelName == "gameFleetList")
				{
					int fleetID = 0;
					if (!string.IsNullOrEmpty(msgParams[0]))
						fleetID = int.Parse(msgParams[0]);
					this.SyncFleetShipsList(fleetID);
				}
				if (!(panelName == "partFleetShips"))
					;
			}
			else if (msgType == "button_clicked")
			{
				if (panelName == FleetManagerState.UIExitButton)
				{
					this._finalSync = true;
					this._manager.PostSetProp("RequestResync");
				}
				else if (panelName == "gameTutorialButton")
					this.App.UI.SetVisible("FleetManagerTutorial", true);
				else if (panelName == "fleetManagerTutImage")
					this.App.UI.SetVisible("FleetManagerTutorial", false);
				else if (panelName == "fleetManagerCreateFleet")
					this._admiralManagerDialog = this.App.UI.CreateDialog((Dialog)new AdmiralManagerDialog(this.App, this.App.LocalPlayer.ID, this._targetSystemID, false, "AdmiralManagerDialog"), null);
				else if (panelName == "fleetmanagerloaCompose")
					this.App.UI.CreateDialog((Dialog)new DialogLoaFleetCompositor(this.App, MissionType.NO_MISSION), null);
				else if (panelName == "fleetManagerUpperButton")
					this._manager.PostSetProp("SelectLevel", 2);
				else if (panelName == "fleetManagerMiddleButton")
					this._manager.PostSetProp("SelectLevel", 1);
				else if (panelName == "fleetManagerLowerButton")
					this._manager.PostSetProp("SelectLevel", 0);
				else if (panelName == "gameFormationVButton")
					this._manager.PostSetProp("VFormation");
				else if (panelName == "gameFormationLineButton")
				{
					this._manager.PostSetProp("LineFormation");
				}
				else
				{
					if (!panelName.StartsWith("menuItem"))
						return;
					int num = int.Parse(panelName.Substring(8));
					IEnumerable<FleetInfo> fleetInfoBySystemId = this.App.GameDatabase.GetFleetInfoBySystemID(this._targetSystemID, FleetType.FL_ALL);
					int toFleetID = 0;
					foreach (FleetInfo fleetInfo in fleetInfoBySystemId)
					{
						if (num == 0)
						{
							toFleetID = fleetInfo.ID;
							break;
						}
						--num;
					}
					if (toFleetID == 0 || this._contextMenuShip == 0)
						return;
					this.App.GameDatabase.TransferShip(this._contextMenuShip, toFleetID);
					foreach (ShipDummy shipDummy in this._shipDummies)
					{
						if (shipDummy.ShipID == this._contextMenuShip)
						{
							shipDummy.FleetID = toFleetID;
							shipDummy.PostSetProp("ClearFleetPosition");
						}
					}
					this.SyncFleetShipsList(this._prevFleetID);
					this._fleetWidget.SetSyncedFleets(this.App.GameDatabase.GetFleetInfoBySystemID(this._targetSystemID, FleetType.FL_ALL).Where<FleetInfo>((Func<FleetInfo, bool>)(x =>
				   {
					   if (x.Type == FleetType.FL_RESERVE)
						   return x.PlayerID == this.App.LocalPlayer.ID;
					   return true;
				   })).ToList<FleetInfo>());
				}
			}
			else
			{
				if (!(msgType == "dialog_closed") || !(panelName == this._admiralManagerDialog))
					return;
				this.RefreshFleetShipModels();
				this._fleetWidget.SetSyncedFleets(this.App.GameDatabase.GetFleetInfoBySystemID(this._targetSystemID, FleetType.FL_ALL).Where<FleetInfo>((Func<FleetInfo, bool>)(x =>
			   {
				   if (x.Type == FleetType.FL_RESERVE)
					   return x.PlayerID == this.App.LocalPlayer.ID;
				   return true;
			   })).ToList<FleetInfo>());
			}
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
