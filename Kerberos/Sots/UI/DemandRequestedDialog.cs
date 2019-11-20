// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.DemandRequestedDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;

namespace Kerberos.Sots.UI
{
	internal class DemandRequestedDialog : Dialog
	{
		private const string RequestHeader = "lblRequestHeader";
		private const string AcceptButton = "btnAccept";
		private const string DeclineButton = "btnDecline";
		private DemandInfo _demand;

		public DemandRequestedDialog(App game, DemandInfo di, string template = "dialogRequested")
		  : base(game, template)
		{
			this._demand = di;
		}

		public override void Initialize()
		{
			this.SyncDetails();
		}

		private void SyncDetails()
		{
			new DiplomacyCard(this._app, this._demand.InitiatingPlayer, this.UI, "pnlRequest").Initialize();
			this._app.UI.SetText("lblTitle", "@EV_DEMAND_REQUESTED");
			this._app.UI.SetText("lblRequestHeader", string.Format(App.Localize("@UI_DIPLOMACY_DEMAND_REQUESTED"), (object)this._app.GameDatabase.GetPlayerInfo(this._demand.InitiatingPlayer).Name, (object)this._demand.ToString(this._app.GameDatabase)));
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "button_clicked"))
				return;
			if (panelName == "btnAccept")
			{
				this._app.Game.AcceptDemand(this._demand);
				this._app.UI.CloseDialog((Dialog)this, true);
			}
			else
			{
				if (!(panelName == "btnDecline"))
					return;
				this._app.Game.DeclineDemand(this._demand);
				this._app.UI.CloseDialog((Dialog)this, true);
			}
		}

		public override string[] CloseDialog()
		{
			return (string[])null;
		}
	}
}
