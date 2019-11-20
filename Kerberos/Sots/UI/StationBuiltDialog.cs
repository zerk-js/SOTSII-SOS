// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.StationBuiltDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class StationBuiltDialog : Dialog
	{
		public const string OKButton = "event_dialog_close";
		private int _stationID;
		private string _enteredStationName;

		public StationBuiltDialog(App game, int stationid)
		  : base(game, "dialogStationBuilt")
		{
			this._stationID = stationid;
		}

		public override void Initialize()
		{
			StationInfo stationInfo = this._app.GameDatabase.GetStationInfo(this._stationID);
			OrbitalObjectInfo orbitalObjectInfo1 = this._app.GameDatabase.GetOrbitalObjectInfo(this._stationID);
			OrbitalObjectInfo orbitalObjectInfo2 = this._app.GameDatabase.GetOrbitalObjectInfo(orbitalObjectInfo1.ParentID.Value);
			if (stationInfo == null || orbitalObjectInfo1 == null)
			{
				this._app.UI.CloseDialog((Dialog)this, true);
			}
			else
			{
				this._app.UI.SetText(this._app.UI.Path(this.ID, "station_class"), string.Format(App.Localize("@STATION_LEVEL"), (object)stationInfo.DesignInfo.StationLevel.ToString(), (object)stationInfo.DesignInfo.StationType.ToDisplayText(this._app.LocalPlayer.Faction.Name)));
				this._app.UI.SetText(this._app.UI.Path(this.ID, "upkeep_cost"), string.Format(App.Localize("@STATION_UPKEEP_COST"), (object)GameSession.CalculateStationUpkeepCost(this._app.GameDatabase, this._app.AssetDatabase, stationInfo).ToString()));
				if (stationInfo.DesignInfo.StationType == StationType.NAVAL)
					this._app.UI.SetText(this._app.UI.Path(this.ID, "naval_capacity"), string.Format(App.Localize("@STATION_FLEET_CAPACITY"), (object)this._app.GameDatabase.GetSystemSupportedCruiserEquivalent(this._app.Game, orbitalObjectInfo2.StarSystemID, this._app.LocalPlayer.ID).ToString()));
				else
					this._app.UI.SetVisible(this._app.UI.Path(this.ID, "naval_capacity"), false);
				StationUI.SyncStationDetailsWidget(this._app.Game, "detailsCard", this._stationID, true);
				StarSystemInfo starSystemInfo = this._app.GameDatabase.GetStarSystemInfo(orbitalObjectInfo2.StarSystemID);
				this._app.UI.SetText("gameStationName", orbitalObjectInfo1.Name);
				this._enteredStationName = orbitalObjectInfo2.Name;
				this._app.UI.SetText(this._app.UI.Path(this.ID, "system_name"), string.Format(App.Localize("@STATION_BUILT"), (object)starSystemInfo.Name).ToUpperInvariant());
			}
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (!(panelName == "event_dialog_close"))
					return;
				OrbitalObjectInfo orbitalObjectInfo = this._app.GameDatabase.GetOrbitalObjectInfo(this._stationID);
				if (orbitalObjectInfo != null)
				{
					orbitalObjectInfo.Name = this._enteredStationName;
					this._app.GameDatabase.UpdateOrbitalObjectInfo(orbitalObjectInfo);
				}
				if (!string.IsNullOrWhiteSpace(this._enteredStationName) && this._enteredStationName.Count<char>() > 0)
					this._app.UI.CloseDialog((Dialog)this, true);
				else
					this._app.UI.CreateDialog((Dialog)new GenericTextDialog(this._app, App.Localize("@INVALID_STATION_NAME"), App.Localize("@INVALID_STATION_NAME_TEXT"), "dialogGenericMessage"), null);
			}
			else
			{
				if (!(msgType == "text_changed") || !(panelName == "gameStationName"))
					return;
				this._enteredStationName = msgParams[0];
			}
		}

		public override string[] CloseDialog()
		{
			return (string[])null;
		}
	}
}
