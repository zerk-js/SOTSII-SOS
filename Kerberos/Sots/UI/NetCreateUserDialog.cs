// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.NetCreateUserDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class NetCreateUserDialog : Dialog
	{
		private string _enteredEmail = "";
		private string _enteredUsername = "";
		private string _enteredPassword = "";
		private string _enteredConfirmPassword = "";
		public const string OKButton = "okButton";
		public const string CancelButton = "cancelButton";
		public const string FieldEmail = "new_email";
		public const string FieldUsername = "new_username";
		public const string FieldPassword = "new_password";
		public const string FieldConfirmPassword = "new_confirm";
		private bool _success;
		private NetProcessNetworkDialog _processDialog;
		private string _eulaConfirmDialogId;

		public NetCreateUserDialog(App game)
		  : base(game, "dialogNetCreateUser")
		{
			this._app.UI.Send((object)"SetFilterMode", (object)"new_username", (object)EditBoxFilterMode.GameSpyNick.ToString());
		}

		public override void HandleScriptMessage(ScriptMessageReader mr)
		{
			switch ((Network.DialogAction)mr.ReadInteger())
			{
				case Network.DialogAction.DA_FINALIZE:
					this._success = mr.ReadBool();
					this.ShowButton(true);
					break;
				case Network.DialogAction.DA_NEWUSER_CREATING:
					this.AddDialogString(App.Localize("@NETWORKDIALOG_CREATE_NEW_USER"));
					break;
				case Network.DialogAction.DA_NEWUSER_INVALID_USERNAME:
					this.AddDialogString(App.Localize("@NETWORKDIALOG_INVALID_USERNAME"));
					break;
				case Network.DialogAction.DA_NEWUSER_NICK_IN_USE:
					this.AddDialogString("Nickname in use.");
					break;
				case Network.DialogAction.DA_NEWUSER_INVALID_PASSWORD:
					this.AddDialogString(App.Localize("@NETWORKDIALOG_INVALID_PASSWORD"));
					break;
				case Network.DialogAction.DA_NEWUSER_FAILED:
					this.AddDialogString(App.Localize("@NETWORKDIALOG_USER_CREATION_FAILED"));
					break;
				case Network.DialogAction.DA_NEWUSER_SUCCESS:
					this.AddDialogString(App.Localize("@NETWORKDIALOG_NEW_USER_CREATED"));
					break;
			}
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "dialog_opened" && panelName == this.ID)
			{
				this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "lblEmail"), "text", App.Localize("@UI_GAMELOBBY_EMAIL_COLON"));
				this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "lblUser"), "text", App.Localize("@UI_GAMELOBBY_USERNAME_COLON_B"));
				this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "lblPass"), "text", App.Localize("@UI_GAMELOBBY_PASSWORD_COLON_B"));
				this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "lblConfirm"), "text", App.Localize("@UI_GAMELOBBY_CONFIRM_PASSWORD"));
			}
			if (msgType == "button_clicked")
			{
				if (panelName == "okButton")
				{
					this._eulaConfirmDialogId = this._app.UI.CreateDialog((Dialog)new GenericQuestionDialog(this._app, App.Localize("@GAMESPY_AGREEMENT"), App.Localize("@GAMESPY_AGREEMENT_DESC"), "dialogEULAConfirm"), null);
					this._app.UI.SetVisible(this._eulaConfirmDialogId, false);
					this._app.UI.SetVisible(this._eulaConfirmDialogId, true);
				}
				else if (panelName == "cancelButton")
					this._app.UI.CloseDialog((Dialog)this, true);
			}
			if (msgType == "edit_confirmed")
			{
				if (panelName == "new_email")
					this._app.UI.Send((object)"FocusKeyboard", (object)this._app.UI.Path(this.ID, "new_username"));
				else if (panelName == "new_username")
					this._app.UI.Send((object)"FocusKeyboard", (object)this._app.UI.Path(this.ID, "new_password"));
				else if (panelName == "new_password")
				{
					this._app.UI.Send((object)"FocusKeyboard", (object)this._app.UI.Path(this.ID, "new_confirm"));
				}
				else
				{
					if (!(panelName == "new_confirm"))
						return;
					this.Confirm();
				}
			}
			else if (msgType == "text_changed")
			{
				if (panelName == "new_email")
					this._enteredEmail = msgParams[0];
				else if (panelName == "new_username")
					this._enteredUsername = msgParams[0];
				else if (panelName == "new_password")
				{
					this._enteredPassword = msgParams[0];
				}
				else
				{
					if (!(panelName == "new_confirm"))
						return;
					this._enteredConfirmPassword = msgParams[0];
				}
			}
			else
			{
				if (!(msgType == "dialog_closed"))
					return;
				if (this._processDialog != null && panelName == this._processDialog.ID)
				{
					this._processDialog = (NetProcessNetworkDialog)null;
					if (this._success)
						this._app.UI.CloseDialog((Dialog)this, true);
				}
				if (!(panelName == this._eulaConfirmDialogId) || ((IEnumerable<string>)msgParams).Count<string>() <= 0 || !(msgParams[0] == "True"))
					return;
				this.Confirm();
			}
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

		private void Confirm()
		{
			if (this._enteredPassword != this._enteredConfirmPassword)
				this._app.UI.CreateDialog((Dialog)new GenericTextDialog(this._app, App.Localize("@PASSWORDS_DO_NOT_MATCH"), App.Localize("@PASSWORDS_DO_NOT_MATCH_TEXT"), "dialogGenericMessage"), null);
			else
				this._app.Network.NewUser(this.ID, this._enteredEmail, this._enteredUsername, this._enteredPassword);
		}

		public override void Initialize()
		{
			base.Initialize();
		}

		public override string[] CloseDialog()
		{
			return new string[1] { this._success.ToString() };
		}
	}
}
