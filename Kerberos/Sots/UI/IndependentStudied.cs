// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.IndependentStudied
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.GameStates;

namespace Kerberos.Sots.UI
{
	internal class IndependentStudied : Dialog
	{
		public const string OkButton = "okButton";
		public const string DiploButton = "DiplomacyButton";
		private int _playerID;

		public IndependentStudied(App game, int playerid)
		  : base(game, "dialogIndependentSurvalenceComplete")
		{
			this._playerID = playerid;
		}

		public override void Initialize()
		{
			PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(this._playerID);
			this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "tech_desc"), "text", App.Localize("@UI_INDEPENDENT_DESCRIPTION_" + playerInfo.Name.ToUpper()));
			this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "radimage"), "sprite", "Independent_Splash_" + playerInfo.Name.ToUpper());
			this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "faction_name"), "text", App.Localize("@UI_INDEPENDENT_COMPLETE_" + playerInfo.Name.ToUpper()) + "  -  " + App.Localize("@UI_INDEPENDENT_TECH_" + playerInfo.Name.ToUpper()));
			this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "advantage"), "text", App.Localize("@UI_INDEPENDENT_BONUS_" + playerInfo.Name.ToUpper()));
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "button_clicked"))
				return;
			if (panelName == "okButton")
				this._app.UI.CloseDialog((Dialog)this, true);
			if (!(panelName == "DiplomacyButton"))
				return;
			this._app.SwitchGameState<DiplomacyScreenState>();
			this._app.UI.CloseDialog((Dialog)this, true);
		}

		protected override void OnUpdate()
		{
		}

		public override string[] CloseDialog()
		{
			return (string[])null;
		}
	}
}
