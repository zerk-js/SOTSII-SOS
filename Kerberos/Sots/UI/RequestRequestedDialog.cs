// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.RequestRequestedDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;

namespace Kerberos.Sots.UI
{
	internal class RequestRequestedDialog : Dialog
	{
		private const string RequestHeader = "lblRequestHeader";
		private const string AcceptButton = "btnAccept";
		private const string DeclineButton = "btnDecline";
		private RequestInfo _request;

		public RequestRequestedDialog(App game, RequestInfo ri, string template = "dialogRequested")
		  : base(game, template)
		{
			this._request = ri;
		}

		public override void Initialize()
		{
			this.SyncDetails();
		}

		private void SyncDetails()
		{
			new DiplomacyCard(this._app, this._request.InitiatingPlayer, this.UI, "pnlRequest").Initialize();
			this._app.UI.SetText("lblTitle", "@EV_REQUEST_REQUESTED");
			this._app.UI.SetText("lblRequestHeader", string.Format(App.Localize("@UI_DIPLOMACY_REQUEST_REQUESTED"), (object)this._app.GameDatabase.GetPlayerInfo(this._request.InitiatingPlayer).Name, (object)this._request.ToString(this._app.GameDatabase)));
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "button_clicked"))
				return;
			if (panelName == "btnAccept")
			{
				this._app.Game.AcceptRequest(this._request);
				this._app.UI.CloseDialog((Dialog)this, true);
			}
			else
			{
				if (!(panelName == "btnDecline"))
					return;
				this._app.Game.DeclineRequest(this._request);
				this._app.UI.CloseDialog((Dialog)this, true);
			}
		}

		public override string[] CloseDialog()
		{
			return (string[])null;
		}
	}
}
