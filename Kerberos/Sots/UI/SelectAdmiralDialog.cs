// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.SelectAdmiralDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class SelectAdmiralDialog : Dialog
	{
		private int _systemid;
		private App App;
		private int _currentAdmiralID;
		private int _currentShipID;
		private int _currentDesignID;
		private string _nameFleetDialog;
		private string _transfercubesDialog;

		public SelectAdmiralDialog(App game, int systemID, string template = "dialogSelectAdmiral")
		  : base(game, template)
		{
			this._systemid = systemID;
			this.App = game;
		}

		public override void Initialize()
		{
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "selectadmiralbtn")
				{
					this.App.UI.CloseDialog((Dialog)this, true);
					this.App.UI.CreateDialog((Dialog)new AdmiralManagerDialog(this.App, this.App.LocalPlayer.ID, this._systemid, true, "AdmiralManagerDialog"), null);
				}
				else if (panelName == "autoselectadmiralbtn")
				{
					this.App.UI.SetVisible(this.ID, false);
					this.AutoChooseAdmiral();
				}
				else if (panelName == "cancelbtn")
					this.App.UI.CloseDialog((Dialog)this, true);
			}
			if (!(msgType == "dialog_closed"))
				return;
			if (panelName == this._nameFleetDialog)
			{
				if (bool.Parse(msgParams[0]))
				{
					int num = this._app.GameDatabase.InsertFleet(this.App.LocalPlayer.ID, this._currentAdmiralID, this._systemid, this._systemid, this._app.GameDatabase.ResolveNewFleetName(this._app, this.App.LocalPlayer.ID, msgParams[1]), FleetType.FL_NORMAL);
					if (this._app.LocalPlayer.Faction.Name == "loa")
					{
						FleetInfo fleetInfo = this._app.GameDatabase.GetFleetsByPlayerAndSystem(this.App.LocalPlayer.ID, this._systemid, FleetType.FL_RESERVE).First<FleetInfo>();
						if (fleetInfo == null)
							return;
						ShipInfo shipInfo = this._app.GameDatabase.GetShipInfoByFleetID(fleetInfo.ID, true).FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignInfo.IsLoaCube()));
						DesignInfo designInfo = this._app.GameDatabase.GetDesignInfo(this._currentDesignID);
						if (shipInfo == null || designInfo == null)
							return;
						this._transfercubesDialog = this._app.UI.CreateDialog((Dialog)new DialogLoaShipTransfer(this._app, num, fleetInfo.ID, shipInfo.ID, designInfo.ProductionCost), null);
					}
					else
					{
						this._app.GameDatabase.TransferShip(this._currentShipID, num);
						this.App.UI.CreateDialog((Dialog)new FleetCompositorDialog(this.App, this._systemid, num, "dialogFleetCompositor"), null);
						this.App.UI.CloseDialog((Dialog)this, true);
					}
				}
				else
					this.App.UI.SetVisible(this.ID, true);
			}
			else
			{
				if (!(panelName == this._transfercubesDialog) || ((IEnumerable<string>)msgParams).Count<string>() != 4)
					return;
				int fleetID = int.Parse(msgParams[0]);
				int.Parse(msgParams[1]);
				int shipID = int.Parse(msgParams[2]);
				int Loacubes = int.Parse(msgParams[3]);
				ShipInfo shipInfo1 = this._app.GameDatabase.GetShipInfo(shipID, true);
				ShipInfo shipInfo2 = this._app.GameDatabase.GetShipInfoByFleetID(fleetID, false).FirstOrDefault<ShipInfo>((Func<ShipInfo, bool>)(x => x.DesignInfo.IsLoaCube()));
				if (shipInfo2 == null)
					this._app.GameDatabase.InsertShip(fleetID, shipInfo1.DesignInfo.ID, "Cube", (ShipParams)0, new int?(), Loacubes);
				else
					this._app.GameDatabase.UpdateShipLoaCubes(shipInfo2.ID, shipInfo2.LoaCubes + Loacubes);
				if (shipInfo1.LoaCubes <= Loacubes)
					this._app.GameDatabase.RemoveShip(shipInfo1.ID);
				else
					this._app.GameDatabase.UpdateShipLoaCubes(shipInfo1.ID, shipInfo1.LoaCubes - Loacubes);
				if (this._app.CurrentState == this._app.GetGameState<StarMapState>())
					this._app.GetGameState<StarMapState>().RefreshStarmap(StarMapState.StarMapRefreshType.REFRESH_NORMAL);
				if (this._app.CurrentState == this._app.GetGameState<FleetManagerState>())
					this._app.GetGameState<FleetManagerState>().Refresh();
				this.App.UI.CloseDialog((Dialog)this, true);
			}
		}

		public void AutoChooseAdmiral()
		{
			List<AdmiralInfo> list = this._app.GameDatabase.GetAdmiralInfosForPlayer(this._app.LocalPlayer.ID).Where<AdmiralInfo>((Func<AdmiralInfo, bool>)(x => this._app.GameDatabase.GetFleetInfoByAdmiralID(x.ID, FleetType.FL_NORMAL) == null)).ToList<AdmiralInfo>();
			AdmiralInfo admiralInfo = list.FirstOrDefault<AdmiralInfo>((Func<AdmiralInfo, bool>)(x => this.App.GameDatabase.GetAdmiralTraits(x.ID).Any<AdmiralInfo.TraitType>((Func<AdmiralInfo.TraitType, bool>)(j => AdmiralInfo.IsGoodTrait(j))))) ?? list.First<AdmiralInfo>();
			ShipInfo shipInfo1 = (ShipInfo)null;
			DesignInfo designInfo1 = (DesignInfo)null;
			int? reserveFleetId = this._app.GameDatabase.GetReserveFleetID(this._app.LocalPlayer.ID, this._systemid);
			if (reserveFleetId.HasValue)
			{
				if (this._app.LocalPlayer.Faction.Name == "loa")
				{
					Kerberos.Sots.StarFleet.StarFleet.ConvertFleetIntoLoaCubes(this._app.Game, reserveFleetId.Value);
					int fleetLoaCubeValue = Kerberos.Sots.StarFleet.StarFleet.GetFleetLoaCubeValue(this._app.Game, reserveFleetId.Value);
					foreach (DesignInfo designInfo2 in this._app.GameDatabase.GetDesignInfosForPlayer(this._app.LocalPlayer.ID).Where<DesignInfo>((Func<DesignInfo, bool>)(x =>
				   {
					   if (x.Class == ShipClass.Cruiser)
						   return x.GetCommandPoints() > 0;
					   return false;
				   })).ToList<DesignInfo>())
					{
						if (designInfo1 == null)
							designInfo1 = designInfo2;
						else if (designInfo1.ProductionCost > designInfo2.ProductionCost)
							designInfo1 = designInfo2;
					}
					if (designInfo1 != null && designInfo1.ProductionCost > fleetLoaCubeValue)
						designInfo1 = (DesignInfo)null;
				}
				else
				{
					foreach (ShipInfo shipInfo2 in this._app.GameDatabase.GetShipInfoByFleetID(reserveFleetId.Value, false))
					{
						if (this._app.GameDatabase.GetShipCommandPointQuota(shipInfo2.ID) > 0)
						{
							shipInfo1 = shipInfo2;
							break;
						}
					}
				}
			}
			if (shipInfo1 == null && designInfo1 == null)
			{
				this._app.UI.CreateDialog((Dialog)new GenericTextDialog(this._app, App.Localize("@UI_FLEET_DIALOG_CANNOTCREATEFLEET_TITLE"), App.Localize("@UI_FLEET_DIALOG_CANNOTCREATEFLEET_DESC"), "dialogGenericMessage"), null);
				this._app.UI.CloseDialog((Dialog)this, true);
			}
			else
			{
				this._currentAdmiralID = admiralInfo.ID;
				if (shipInfo1 != null)
					this._currentShipID = shipInfo1.ID;
				if (designInfo1 != null)
					this._currentDesignID = designInfo1.ID;
				this._nameFleetDialog = this._app.UI.CreateDialog((Dialog)new GenericTextEntryDialog(this._app, App.Localize("@UI_FLEET_DIALOG_FLEETNAME_TITLE"), App.Localize("@UI_FLEET_DIALOG_FLEETNAME_DESC"), this._app.GameDatabase.ResolveNewFleetName(this._app, this._app.LocalPlayer.ID, this._app.Game.NamesPool.GetFleetName(this._app.GameDatabase.GetFactionName(this._app.GameDatabase.GetPlayerFactionID(this._app.LocalPlayer.ID)))), 24, 1, true, EditBoxFilterMode.None), null);
			}
		}

		public override string[] CloseDialog()
		{
			return (string[])null;
		}
	}
}
