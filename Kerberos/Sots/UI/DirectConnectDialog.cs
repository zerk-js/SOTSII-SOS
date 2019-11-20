// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.DirectConnectDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.UI
{
	internal class DirectConnectDialog : Dialog
	{
		private string _addressText = string.Empty;
		private string _passwordText = string.Empty;
		public const string dialogName = "directConnectDialog";
		public const string AddressEditBox = "editAddress";
		public const string PasswordEditBox = "editPassword";
		public const string ConfirmButton = "buttonOk";
		public const string CancelButton = "buttonCancel";
		private bool _choice;

		public DirectConnectDialog(App game)
		  : base(game, "directConnectDialog")
		{
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "buttonOk")
				{
					this.Confirm();
					return;
				}
				if (panelName == "buttonCancel")
				{
					this._choice = false;
					this._app.UI.CloseDialog((Dialog)this, true);
				}
			}
			else if (msgType == "edit_confirmed")
			{
				if (panelName == "editAddress" || panelName == "editPassword")
				{
					this.Confirm();
					return;
				}
			}
			else if (msgType == "text_changed")
			{
				if (panelName == "editAddress")
				{
					this._addressText = msgParams[0];
					return;
				}
				if (panelName == "editPassword")
				{
					this._passwordText = msgParams[0];
					return;
				}
			}
			base.OnPanelMessage(panelName, msgType, msgParams);
		}

		private void Confirm()
		{
			if (string.IsNullOrEmpty(this._addressText))
			{
				this._app.UI.CreateDialog((Dialog)new GenericTextDialog(this._app, "Error!", "Please enter an address!", "dialogGenericMessage"), null);
			}
			else
			{
				this._choice = true;
				this._app.UI.CloseDialog((Dialog)this, true);
			}
		}

		public override string[] CloseDialog()
		{
			return new string[3]
			{
		this._choice.ToString(),
		this._addressText,
		this._passwordText
			};
		}
	}
}
