// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.GiveTypeDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using System.Collections.Generic;

namespace Kerberos.Sots.UI
{
	internal class GiveTypeDialog : Dialog
	{
		public static Dictionary<GiveType, string> GiveTypeLocMap = new Dictionary<GiveType, string>()
	{
	  {
		GiveType.GiveResearchPoints,
		"@UI_DIPLOMACY_GIVE_RESEARCH_MONEY"
	  },
	  {
		GiveType.GiveSavings,
		"@UI_DIPLOMACY_GIVE_SAVINGS"
	  }
	};
		private const string GiveMoneyButton = "btnMoney";
		private const string GiveResearchMoneyButton = "btnResearch";
		private const string CancelButton = "btnCancel";
		private int _otherPlayer;

		public GiveTypeDialog(App game, int otherPlayer, string template = "dialogGiveType")
		  : base(game, template)
		{
			this._otherPlayer = otherPlayer;
		}

		public override void Initialize()
		{
			PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID);
			this._app.GameDatabase.GetPlayerInfo(this._otherPlayer);
			DiplomacyUI.SyncDiplomacyPopup(this._app, this.ID, this._otherPlayer);
			this._app.UI.SetEnabled("btnMoney", playerInfo.Savings > 0.0);
			this._app.UI.SetEnabled("btnResearch", playerInfo.Savings > 0.0);
			this._app.UI.SetButtonText("btnMoney", string.Format(App.Localize(GiveTypeDialog.GiveTypeLocMap[GiveType.GiveSavings])));
			this._app.UI.SetButtonText("btnResearch", string.Format(App.Localize(GiveTypeDialog.GiveTypeLocMap[GiveType.GiveResearchPoints])));
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "btnMoney")
					this._app.UI.CreateDialog((Dialog)new GiveScalarDialog(this._app, GiveType.GiveSavings, this._otherPlayer, "dialogGiveScalar"), null);
				else if (panelName == "btnResearch")
				{
					this._app.UI.CreateDialog((Dialog)new GiveScalarDialog(this._app, GiveType.GiveResearchPoints, this._otherPlayer, "dialogGiveScalar"), null);
				}
				else
				{
					if (!(panelName == "btnCancel"))
						return;
					this._app.UI.CloseDialog((Dialog)this, true);
				}
			}
			else
			{
				if (!(msgType == "dialog_closed") || !(msgParams[0] != "true"))
					return;
				this._app.UI.CloseDialog((Dialog)this, true);
			}
		}

		public override string[] CloseDialog()
		{
			return (string[])null;
		}
	}
}
