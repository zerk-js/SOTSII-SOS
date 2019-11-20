// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.NetConnectionDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;

namespace Kerberos.Sots.UI
{
	internal class NetConnectionDialog : Dialog
	{
		public const string cancelButton = "dialogButtonCancel";
		public const string okButton = "dialogButtonOk";
		private string _title;
		private string _text;
		private bool _success;

		public NetConnectionDialog(App app, string title = "Connecting", string text = "", string template = "dialogNetConnection")
		  : base(app, template)
		{
			this._title = title;
			this._text = text;
		}

		public override void Initialize()
		{
			this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "title"), "text", this._title);
			this.RefreshText();
		}

		public override void HandleScriptMessage(ScriptMessageReader mr)
		{
			switch ((Network.DialogAction)mr.ReadInteger())
			{
				case Network.DialogAction.DA_FINALIZE:
					this._success = mr.ReadBool();
					this._app.UI.CloseDialog((Dialog)this, true);
					break;
				case Network.DialogAction.DA_RAW_STRING:
					this.AddString(mr.ReadString());
					break;
				case Network.DialogAction.DA_CONNECT_CONNECTING:
					this.AddString("Connecting to Host.");
					break;
				case Network.DialogAction.DA_CONNECT_FAILED:
					this.AddString("Failed to connect to Host.");
					break;
				case Network.DialogAction.DA_CONNECT_SUCCESS:
					this.AddString("Connection to Host succeeded.");
					break;
				case Network.DialogAction.DA_CONNECT_TIMED_OUT:
					this.AddString("Connection to Host timed out.");
					break;
				case Network.DialogAction.DA_CONNECT_INVALID_PASSWORD:
					this.AddString("Invalid password.");
					break;
				case Network.DialogAction.DA_CONNECT_NAT_FAILURE:
					this.AddString("NAT Negotiation failed.");
					break;
			}
		}

		public void AddString(string text)
		{
			NetConnectionDialog connectionDialog = this;
			connectionDialog._text = connectionDialog._text + "\n" + text;
			this.RefreshText();
		}

		public void ClearText()
		{
			this._text = string.Empty;
			this.RefreshText();
		}

		public void RefreshText()
		{
			this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "textbox"), "text", this._text);
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "button_clicked") || !(panelName == "dialogButtonOk") && !(panelName == "dialogButtonCancel"))
				return;
			this._app.UI.CloseDialog((Dialog)this, true);
		}

		public override string[] CloseDialog()
		{
			return new string[1] { this._success.ToString() };
		}
	}
}
