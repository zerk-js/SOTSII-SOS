// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.NetCreateGameDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.UI
{
	internal class NetCreateGameDialog : Dialog
	{
		public const string OKButton = "buttonGameSetup";
		public const string LoadButton = "btnLoad";
		public const string CancelButton = "btnGameDlgCancel";
		public const string GameNameBox = "game_name";
		public const string PassBox = "game_password";
		private string _enteredGameName;
		private string _enteredPass;
		private LoadGameDialog _loadGameDlg;
		private bool _cancelled;
		private bool _loaded;

		public NetCreateGameDialog(App game)
		  : base(game, "dialogNetCreateGame")
		{
			this._enteredGameName = "";
			this._enteredPass = "";
		}

		protected bool ValidateGameName()
		{
			if (!string.IsNullOrEmpty(this._enteredGameName) && this._enteredGameName.Length <= 30 && this._enteredGameName.Length >= 3)
				return true;
			this._app.UI.CreateDialog((Dialog)new GenericTextDialog(this._app, "@ERROR", "@GAMENAMEERROR", "dialogGenericMessage"), null);
			return false;
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "dialog_opened")
			{
				if (!(panelName == this.ID))
					return;
				this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "lblUser"), "text", App.Localize("@UI_GAMELOBBY_USERNAME_COLON"));
				this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "lblPass"), "text", App.Localize("@UI_GAMELOBBY_PASSWORD_COLON"));
			}
			else if (msgType == "dialog_closed")
			{
				if (this._loadGameDlg == null || !(panelName == this._loadGameDlg.ID))
					return;
				this._loaded = msgParams[0] == "True";
				if (!this._loaded)
					return;
				this._cancelled = false;
				this._app.UI.CloseDialog((Dialog)this, true);
			}
			else if (msgType == "button_clicked")
			{
				if (panelName == "btnGameDlgCancel")
				{
					this._cancelled = true;
					this._app.UI.CloseDialog((Dialog)this, true);
				}
				else if (panelName == "buttonGameSetup")
				{
					if (!this.ValidateGameName())
						return;
					this._cancelled = false;
					this._app.UI.CloseDialog((Dialog)this, true);
				}
				else
				{
					if (!(panelName == "btnLoad") || !this.ValidateGameName())
						return;
					this._app.GameSetup.IsMultiplayer = true;
					this._loadGameDlg = new LoadGameDialog(this._app, null);
					this._app.UI.CreateDialog((Dialog)this._loadGameDlg, null);
				}
			}
			else
			{
				if (!(msgType == "text_changed"))
					return;
				if (panelName == "game_name")
				{
					this._enteredGameName = msgParams[0];
				}
				else
				{
					if (!(panelName == "game_password"))
						return;
					this._enteredPass = msgParams[0];
				}
			}
		}

		public override void Initialize()
		{
			base.Initialize();
		}

		public override string[] CloseDialog()
		{
			return new string[4]
			{
		this._cancelled.ToString(),
		this._loaded.ToString(),
		this._enteredGameName,
		this._enteredPass
			};
		}
	}
}
