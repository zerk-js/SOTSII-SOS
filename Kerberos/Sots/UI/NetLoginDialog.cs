// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.NetLoginDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;

namespace Kerberos.Sots.UI
{
	internal class NetLoginDialog : Dialog
	{
		public const string CreateButton = "createButton";
		public const string DeleteButton = "deleteButton";
		public const string NewUserButton = "buttonNewUser";
		public const string LANTab = "buttonLAN";
		public const string InternetTab = "buttonInternet";
		public const string OKButton = "buttonLogin";
		public const string CancelButton = "buttonCancel";
		public const string UserBox = "login_username";
		public const string PassBox = "login_password";
		private string _enteredUser;
		private string _enteredPass;
		private bool _lanmode;
		private bool _cancelled;
		private NetProcessNetworkDialog _processDialog;
		private bool _success;

		public NetLoginDialog(App game)
		  : base(game, "dialogNetLogin")
		{
			this._enteredUser = "";
			this._enteredPass = "";
			this._app.UI.Send((object)"SetFilterMode", (object)"login_username", (object)EditBoxFilterMode.GameSpyNick.ToString());
		}

		public override void HandleScriptMessage(ScriptMessageReader mr)
		{
			switch (mr.ReadInteger())
			{
				case 0:
					this._success = mr.ReadBool();
					this.ShowButton(true);
					break;
				case 2:
					this.AddDialogString(App.Localize("@GAMESPY_CONNECTING"));
					break;
				case 3:
					this.AddDialogString(App.Localize("@GAMESPY_CONNECT_FAILED"));
					break;
				case 4:
					this.AddDialogString(App.Localize("@GAMESPY_INVALID_PASS"));
					break;
				case 5:
					this.AddDialogString(App.Localize("@INVALID_USERNAME_TEXT"));
					break;
				case 9:
					this.AddDialogString(App.Localize("@GAMESPY_CONNECT_CHAT_FAILED"));
					break;
				case 10:
					this.AddDialogString(App.Localize("@GAMESPY_CONNECTED"));
					break;
			}
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
			else if (msgType == "button_clicked")
			{
				if (panelName == "buttonLAN")
					this.SetLANMode(true);
				else if (panelName == "buttonInternet")
					this.SetLANMode(false);
				else if (panelName == "buttonNewUser")
					this._app.UI.CreateDialog((Dialog)new NetCreateUserDialog(this._app), null);
				else if (panelName == "buttonCancel")
				{
					this._cancelled = true;
					this._app.UI.CloseDialog((Dialog)this, true);
				}
				else
				{
					if (!(panelName == "buttonLogin"))
						return;
					if (this._lanmode)
					{
						if (string.IsNullOrEmpty(this._enteredUser) || this._enteredUser.Contains(" "))
						{
							this._app.UI.CreateDialog((Dialog)new GenericTextDialog(this._app, App.Localize("@INVALID_USER"), App.Localize("@INVALID_USERNAME_TEXT"), "dialogGenericMessage"), null);
						}
						else
						{
							this._app.Network.Login(this._enteredUser);
							this._app.Network.IsOffline = true;
							this._app.UI.CloseDialog((Dialog)this, true);
						}
					}
					else if (string.IsNullOrEmpty(this._enteredUser) || this._enteredUser.Contains(" "))
						this._app.UI.CreateDialog((Dialog)new GenericTextDialog(this._app, App.Localize("@INVALID_USER"), App.Localize("@INVALID_USERNAME_TEXT"), "dialogGenericMessage"), null);
					else if (!this._app.Network.IsLoggedIn)
					{
						this._app.Network.Login(this._enteredUser);
						this._app.Network.IsOffline = false;
					}
					else
					{
						this._app.Network.IsOffline = false;
						this._app.UI.CloseDialog((Dialog)this, true);
					}
				}
			}
			else if (msgType == "text_changed")
			{
				if (panelName == "login_username")
				{
					this._enteredUser = msgParams[0];
				}
				else
				{
					if (!(panelName == "login_password"))
						return;
					this._enteredPass = msgParams[0];
				}
			}
			else
			{
				if (!(msgType == "dialog_closed") || this._processDialog == null || !(panelName == this._processDialog.ID))
					return;
				this._processDialog = (NetProcessNetworkDialog)null;
				if (!this._success)
					return;
				this._app.Network.IsLoggedIn = true;
				this._app.UI.CloseDialog((Dialog)this, true);
			}
		}

		public override void Initialize()
		{
			base.Initialize();
			this.SetLANMode(true);
		}

		public void SetResult(bool success)
		{
			this._success = success;
		}

		public void AddDialogString(string val)
		{
			if (this._processDialog == null)
			{
				this._processDialog = new NetProcessNetworkDialog(this._app);
				this._app.UI.CreateDialog((Dialog)this._processDialog, null);
			}
			this._processDialog.AddDialogString(val);
		}

		public void ShowButton(bool val)
		{
			if (this._processDialog == null)
				return;
			this._processDialog.ShowButton(val);
		}

		private void SetLANMode(bool val)
		{
			this._lanmode = val;
			if (val)
			{
				this._app.UI.SetEnabled(this._app.UI.Path(this.ID, "login_username"), false);
				this._app.UI.SetEnabled(this._app.UI.Path(this.ID, "login_password"), false);
				this._app.UI.SetEnabled(this._app.UI.Path(this.ID, "lblUser"), false);
				this._app.UI.SetEnabled(this._app.UI.Path(this.ID, "lblPass"), false);
				this._app.UI.SetEnabled(this._app.UI.Path(this.ID, "buttonNewUser"), false);
				this._app.UI.SetChecked(this._app.UI.Path(this.ID, "buttonLAN"), true);
				this._app.UI.SetChecked(this._app.UI.Path(this.ID, "buttonInternet"), false);
				this._enteredUser = this._app.UserProfile.ProfileName;
				this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "login_username"), "text", this._app.UserProfile.ProfileName);
				this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "login_password"), "text", "");
			}
			else
			{
				this._app.UI.SetEnabled(this._app.UI.Path(this.ID, "login_username"), true);
				this._app.UI.SetEnabled(this._app.UI.Path(this.ID, "login_password"), true);
				this._app.UI.SetEnabled(this._app.UI.Path(this.ID, "lblUser"), true);
				this._app.UI.SetEnabled(this._app.UI.Path(this.ID, "lblPass"), true);
				this._app.UI.SetEnabled(this._app.UI.Path(this.ID, "buttonNewUser"), true);
				this._app.UI.SetChecked(this._app.UI.Path(this.ID, "buttonLAN"), false);
				this._app.UI.SetChecked(this._app.UI.Path(this.ID, "buttonInternet"), true);
				this._enteredUser = this._app.UserProfile.Username ?? "";
				this._enteredPass = this._app.UserProfile.Password ?? "";
				this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "login_username"), "text", this._enteredUser);
				this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "login_password"), "text", this._enteredPass);
				this._app.UI.SetEnabled(this._app.UI.Path(this.ID, "login_username"), (!this._app.Network.IsLoggedIn ? 1 : 0) != 0);
				this._app.UI.SetEnabled(this._app.UI.Path(this.ID, "login_password"), (!this._app.Network.IsLoggedIn ? 1 : 0) != 0);
			}
		}

		public override string[] CloseDialog()
		{
			return new string[1] { this._cancelled.ToString() };
		}
	}
}
