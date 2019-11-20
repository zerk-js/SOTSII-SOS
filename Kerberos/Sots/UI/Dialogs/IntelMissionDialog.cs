// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.Dialogs.IntelMissionDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Strategy;
using System;
using System.Text;

namespace Kerberos.Sots.UI.Dialogs
{
	internal class IntelMissionDialog : Dialog
	{
		private readonly int _targetPlayer;
		private readonly GameSession _game;
		private readonly Label _descLabel;
		private readonly Button _okButton;
		private readonly Button _cancelButton;
		private readonly EspionagePlayerHeader _playerHeader;

		public IntelMissionDialog(GameSession game, int targetPlayer)
		  : base(game.App, "dialogIntelSummary")
		{
			this._targetPlayer = targetPlayer;
			this._game = game;
			this._descLabel = new Label(this.UI, this.UI.Path(this.ID, "lblIntelDesc"));
			this._okButton = new Button(this.UI, this.UI.Path(this.ID, "btnOk"), null);
			this._okButton.Clicked += new EventHandler(this._okButton_Clicked);
			this._cancelButton = new Button(this.UI, this.UI.Path(this.ID, "btnCancel"), null);
			this._cancelButton.Clicked += new EventHandler(this._cancelButton_Clicked);
			this._playerHeader = new EspionagePlayerHeader(this._game, this.ID);
			this.AddPanels((PanelBinding)this._descLabel, (PanelBinding)this._okButton, (PanelBinding)this._cancelButton, (PanelBinding)this._playerHeader);
		}

		public override void Initialize()
		{
			PlayerInfo playerInfo1 = this._game.GameDatabase.GetPlayerInfo(this._game.LocalPlayer.ID);
			PlayerInfo playerInfo2 = this._game.GameDatabase.GetPlayerInfo(this._targetPlayer);
			DiplomacyUI.SyncPanelColor(this._app, this.ID, playerInfo2.PrimaryColor);
			this._playerHeader.UpdateFromPlayerInfo(playerInfo1.ID, playerInfo2);
			int intelPoints = playerInfo1.IntelPoints;
			int pointsForMission = this._game.AssetDatabase.RequiredIntelPointsForMission;
			bool flag = intelPoints >= pointsForMission;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(string.Format(App.Localize("@UI_DIPLOMACY_INTEL_INFO_DESC_TARGET") + "\n", (object)playerInfo2.Name));
			stringBuilder.Append(string.Format(App.Localize("@UI_DIPLOMACY_INTEL_INFO_DESC_POINTS_REQUIRED") + "\n", (object)pointsForMission, (object)intelPoints));
			stringBuilder.Append(string.Format(App.Localize("@UI_DIPLOMACY_INTEL_INFO_DESC_CHANCE"), (object)GameSession.GetIntelSuccessRollChance(this._game.AssetDatabase, this._game.GameDatabase, playerInfo1.ID, playerInfo2.ID), (object)5));
			this._descLabel.SetText(stringBuilder.ToString());
			this._okButton.SetEnabled(flag);
		}

		private void _cancelButton_Clicked(object sender, EventArgs e)
		{
			this._app.UI.CloseDialog((Dialog)this, true);
		}

		private void _okButton_Clicked(object sender, EventArgs e)
		{
			this._game.DoIntelMission(this._targetPlayer);
			this._app.UI.CloseDialog((Dialog)this, true);
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			this.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Recursive);
		}

		public override string[] CloseDialog()
		{
			return (string[])null;
		}
	}
}
