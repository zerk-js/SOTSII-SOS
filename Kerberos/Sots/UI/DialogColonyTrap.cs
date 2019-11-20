// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.DialogColonyTrap
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class DialogColonyTrap : Dialog
	{
		private List<SystemWidget> _systemWidgets = new List<SystemWidget>();
		public const string OKButton = "okButton";
		private int _systemID;
		private List<PlanetWidget> _planetWidgets;
		private App App;
		private GameObjectSet _crits;
		private bool _critsInitialized;
		private List<ColonyTrapInfo> _existingTraps;
		private List<int> _placedTraps;
		private int _fleetID;
		private DialogColonyTrap.PlanetFilterMode _currentFilterMode;

		public DialogColonyTrap(App app, int systemid, int fleetid)
		  : base(app, "dialogColonyTrapper")
		{
			this._systemID = systemid;
			this.App = app;
			this._fleetID = fleetid;
		}

		public override void Initialize()
		{
			StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(this._systemID);
			this._crits = new GameObjectSet(this._app);
			this._existingTraps = this.App.GameDatabase.GetColonyTrapInfosAtSystem(this._systemID).Where<ColonyTrapInfo>((Func<ColonyTrapInfo, bool>)(x => this.App.GameDatabase.GetFleetInfo(x.FleetID).PlayerID == this.App.LocalPlayer.ID)).ToList<ColonyTrapInfo>();
			this._placedTraps = new List<int>();
			this._currentFilterMode = DialogColonyTrap.PlanetFilterMode.AllPlanets;
			this._planetWidgets = new List<PlanetWidget>();
			this.SetSyncedSystem(starSystemInfo);
			this.UpdateNumRemainingTraps();
		}

		private void UpdateNumRemainingTraps()
		{
			PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID);
			if (this._fleetID == 0 || !(this.App.AssetDatabase.GetFaction(playerInfo.FactionID).Name.ToLower() == "morrigi"))
				return;
			List<ShipInfo> shipInfoList = new List<ShipInfo>();
			foreach (ShipInfo shipInfo in this.App.GameDatabase.GetShipInfoByFleetID(this._fleetID, true).ToList<ShipInfo>())
			{
				if (((IEnumerable<DesignSectionInfo>)shipInfo.DesignInfo.DesignSections).Select<DesignSectionInfo, ShipSectionAsset>((Func<DesignSectionInfo, ShipSectionAsset>)(x => x.ShipSectionAsset)).FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.IsTrapShip)) != null)
					shipInfoList.Add(shipInfo);
			}
			int num = shipInfoList.Count - this._placedTraps.Count;
			this.App.UI.SetText(this.UI.Path(this.ID, "remainingTraps"), num.ToString() + " Remaining " + (num == 1 ? "Trap" : "Traps"));
			this.SetTrapInputsEnabled(num != 0);
		}

		private void SetTrapInputsEnabled(bool enabled)
		{
			foreach (PlanetWidget planetWidget in this._planetWidgets)
			{
				PlanetWidget widget = planetWidget;
				if (!this._existingTraps.Any<ColonyTrapInfo>((Func<ColonyTrapInfo, bool>)(x => x.PlanetID == widget.GetPlanetID())) && (enabled || !this._placedTraps.Any<int>((Func<int, bool>)(x => x == widget.GetPlanetID()))))
				{
					this.App.UI.SetPropertyBool("applyTrap|" + widget.GetPlanetID().ToString(), "input_enabled", enabled);
					this.App.UI.SetEnabled("applyTrap|" + widget.GetPlanetID().ToString(), enabled);
				}
			}
		}

		protected override void OnUpdate()
		{
			if (!this._critsInitialized && this._crits.IsReady())
			{
				this._critsInitialized = true;
				this._crits.Activate();
			}
			foreach (PlanetWidget planetWidget in this._planetWidgets)
				planetWidget.Update();
			foreach (SystemWidget systemWidget in this._systemWidgets)
				systemWidget.Update();
		}

		protected void SetSyncedSystem(StarSystemInfo system)
		{
			this.App.UI.ClearItems("system_list");
			this.App.UI.ClearDisabledItems("system_list");
			foreach (SystemWidget systemWidget in this._systemWidgets)
				systemWidget.Terminate();
			foreach (PlanetWidget planetWidget in this._planetWidgets)
				planetWidget.Terminate();
			this._planetWidgets.Clear();
			List<PlanetInfo> planetInfoList = this.FilteredPlanetList(system);
			this.App.UI.AddItem("system_list", "", system.ID, "", "systemTitleCard");
			this._systemWidgets.Add(new SystemWidget(this.App, this.App.UI.GetItemGlobalID("system_list", "", system.ID, "")));
			this._systemWidgets.Last<SystemWidget>().Sync(system.ID);
			PlayerInfo playerInfo = this.App.GameDatabase.GetPlayerInfo(this.App.LocalPlayer.ID);
			foreach (PlanetInfo planetInfo in planetInfoList)
			{
				PlanetInfo planet = planetInfo;
				if (this.App.AssetDatabase.IsPotentialyHabitable(planet.Type))
				{
					this.App.UI.AddItem("system_list", "", planet.ID + 999999, "", "planetDetailsM_Card");
					this._planetWidgets.Add(new PlanetWidget(this.App, this.App.UI.GetItemGlobalID("system_list", "", planet.ID + 999999, "")));
					this._planetWidgets.Last<PlanetWidget>().Sync(planet.ID, false, false);
					string itemGlobalId = this.App.UI.GetItemGlobalID("system_list", "", planet.ID + 999999, "");
					this.App.UI.SetPropertyString(this.UI.Path(itemGlobalId, "applyTrap"), "id", "applyTrap|" + planet.ID.ToString());
					if (this._existingTraps.Any<ColonyTrapInfo>((Func<ColonyTrapInfo, bool>)(x => x.PlanetID == planet.ID)))
					{
						this.App.UI.SetChecked("applyTrap|" + planet.ID.ToString(), true);
						this.App.UI.SetPropertyBool("applyTrap|" + planet.ID.ToString(), "input_enabled", false);
					}
					else
						this.App.UI.SetChecked("applyTrap|" + planet.ID.ToString(), false);
					if (this.App.GameDatabase.GetColonyInfoForPlanet(planet.ID) == null && this.App.AssetDatabase.GetFaction(playerInfo.FactionID).Name.ToLower() == "morrigi")
						this.App.UI.SetVisible(this.UI.Path(itemGlobalId, "applyTrap|" + planet.ID.ToString()), true);
				}
				else if (this.App.AssetDatabase.IsGasGiant(planet.Type))
				{
					this.App.UI.AddItem("system_list", "", planet.ID + 999999, "", "gasgiantDetailsM_Card");
					this._planetWidgets.Add(new PlanetWidget(this.App, this.App.UI.GetItemGlobalID("system_list", "", planet.ID + 999999, "")));
					this._planetWidgets.Last<PlanetWidget>().Sync(planet.ID, false, false);
				}
				else if (this.App.AssetDatabase.IsMoon(planet.Type))
				{
					this.App.UI.AddItem("system_list", "", planet.ID + 999999, "", "moonDetailsM_Card");
					this._planetWidgets.Add(new PlanetWidget(this.App, this.App.UI.GetItemGlobalID("system_list", "", planet.ID + 999999, "")));
					this._planetWidgets.Last<PlanetWidget>().Sync(planet.ID, false, false);
				}
			}
		}

		private List<PlanetInfo> FilteredPlanetList(StarSystemInfo system)
		{
			List<PlanetInfo> list = ((IEnumerable<PlanetInfo>)this.App.GameDatabase.GetStarSystemPlanetInfos(system.ID)).ToList<PlanetInfo>();
			List<PlanetInfo> planetInfoList = new List<PlanetInfo>();
			foreach (PlanetInfo planetInfo in list)
			{
				if (this.App.GameDatabase.IsSurveyed(this.App.LocalPlayer.ID, system.ID))
				{
					if (this._currentFilterMode == DialogColonyTrap.PlanetFilterMode.AllPlanets)
						planetInfoList.Add(planetInfo);
					else if (this._currentFilterMode == DialogColonyTrap.PlanetFilterMode.SurveyedPlanets)
					{
						if (this.App.GameDatabase.GetColonyIntelForPlanet(this.App.LocalPlayer.ID, planetInfo.ID) == null)
							planetInfoList.Add(planetInfo);
					}
					else if (this._currentFilterMode == DialogColonyTrap.PlanetFilterMode.OwnedPlanets)
					{
						AIColonyIntel colonyIntelForPlanet = this.App.GameDatabase.GetColonyIntelForPlanet(this.App.LocalPlayer.ID, planetInfo.ID);
						if (colonyIntelForPlanet != null && colonyIntelForPlanet.OwningPlayerID == this.App.LocalPlayer.ID)
							planetInfoList.Add(planetInfo);
					}
					else if (this._currentFilterMode == DialogColonyTrap.PlanetFilterMode.EnemyPlanets)
					{
						AIColonyIntel colonyIntelForPlanet = this.App.GameDatabase.GetColonyIntelForPlanet(this.App.LocalPlayer.ID, planetInfo.ID);
						if (colonyIntelForPlanet != null && colonyIntelForPlanet.OwningPlayerID != this.App.LocalPlayer.ID)
							planetInfoList.Add(planetInfo);
					}
				}
			}
			return planetInfoList;
		}

		private void SetTraps()
		{
			if (this._fleetID == 0)
				return;
			List<ShipInfo> source = new List<ShipInfo>();
			foreach (ShipInfo shipInfo in this.App.GameDatabase.GetShipInfoByFleetID(this._fleetID, true).ToList<ShipInfo>())
			{
				if (((IEnumerable<DesignSectionInfo>)shipInfo.DesignInfo.DesignSections).Select<DesignSectionInfo, ShipSectionAsset>((Func<DesignSectionInfo, ShipSectionAsset>)(x => x.ShipSectionAsset)).FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.IsTrapShip)) != null)
					source.Add(shipInfo);
			}
			foreach (int placedTrap in this._placedTraps)
			{
				ShipInfo trapShip = source.First<ShipInfo>();
				this.App.Game.SetAColonyTrap(trapShip, this.App.LocalPlayer.ID, this._systemID, placedTrap);
				source.Remove(trapShip);
				if (source.Count == 0)
					break;
			}
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (!(panelName == "ITS_A_TRAP"))
					return;
				this.SetTraps();
				this.App.UI.CloseDialog((Dialog)this, true);
			}
			else
			{
				if (!(msgType == "checkbox_clicked"))
					return;
				bool flag = int.Parse(msgParams[0]) > 0;
				if (!panelName.StartsWith("applyTrap"))
					return;
				int result;
				if (!int.TryParse(panelName.Split('|')[1], out result))
					return;
				if (flag)
				{
					if (!this._placedTraps.Contains(result))
						this._placedTraps.Add(result);
				}
				else if (this._placedTraps.Contains(result))
					this._placedTraps.Remove(result);
				this.UpdateNumRemainingTraps();
			}
		}

		private void SetColonyViewMode(int mode)
		{
			if (mode == 0)
			{
				this._app.UI.SetVisible(this._app.UI.Path(this.ID, "system_map"), true);
				this._app.UI.SetVisible(this._app.UI.Path(this.ID, "gameStarSystemViewport"), false);
			}
			else
			{
				this._app.UI.SetVisible(this._app.UI.Path(this.ID, "system_map"), false);
				this._app.UI.SetVisible(this._app.UI.Path(this.ID, "gameStarSystemViewport"), true);
			}
		}

		public override string[] CloseDialog()
		{
			if (this._crits != null)
				this._crits.Dispose();
			this._crits = (GameObjectSet)null;
			foreach (PlanetWidget planetWidget in this._planetWidgets)
				planetWidget.Terminate();
			foreach (SystemWidget systemWidget in this._systemWidgets)
				systemWidget.Terminate();
			return (string[])null;
		}

		private enum PlanetFilterMode
		{
			AllPlanets,
			SurveyedPlanets,
			OwnedPlanets,
			EnemyPlanets,
		}
	}
}
