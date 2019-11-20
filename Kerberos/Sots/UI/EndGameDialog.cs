// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.EndGameDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.GameStates;
using System;

namespace Kerberos.Sots.UI
{
	internal class EndGameDialog : Dialog
	{
		private string _title = "";
		private string _message = "";
		private string _graphic = "";
		private const string UIOkButton = "btnOk";
		private const string UITitleLabel = "lblTitle";
		private const string UIDescriptionLabel = "lblDescription";
		private const string UIEndGameImage = "imgEndGame";

		public EndGameDialog(App game, string title, string message, string graphic)
		  : base(game, "dialogEndGame")
		{
			this._title = title;
			this._message = message;
			this._graphic = graphic;
		}

		public override void Initialize()
		{
			this._app.UI.SetText(this._app.UI.Path(this.ID, "lblTitle"), this._title);
			this._app.UI.SetText(this._app.UI.Path(this.ID, "lblDescription"), this._message);
			this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "imgEndGame"), "sprite", this._graphic);
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "button_clicked") || !(panelName == "btnOk"))
				return;
			if (this._app.GameSetup.IsMultiplayer)
				this._app.Network.Disconnect();
			this._app.GetGameState<StarMapState>().Reset();
			this._app.UI.CloseDialog((Dialog)this, true);
			this._app.SwitchGameStateViaLoadingScreen((Action)null, (LoadingFinishedDelegate)null, (GameState)this._app.GetGameState<MainMenuState>(), (object[])null);
			this._app.UI.CloseDialog((Dialog)this, true);
		}

		public override string[] CloseDialog()
		{
			return (string[])null;
		}
	}
}
