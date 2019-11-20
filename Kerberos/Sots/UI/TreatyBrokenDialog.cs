// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.TreatyBrokenDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using System;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class TreatyBrokenDialog : Dialog
	{
		private const string DoneButton = "btnDone";
		private const string ConsequenceList = "lstConsequences";
		private const string DescLabel = "lblDesc";
		private TreatyInfo _treatyInfo;
		private bool _isVictim;

		public TreatyBrokenDialog(App game, int treatyId, bool isVictim)
		  : base(game, "dialogTreatyBroken")
		{
			this._treatyInfo = game.GameDatabase.GetTreatyInfos().ToList<TreatyInfo>().FirstOrDefault<TreatyInfo>((Func<TreatyInfo, bool>)(x => x.ID == treatyId));
			this._isVictim = isVictim;
			if (this._treatyInfo != null)
				return;
			this._app.UI.CloseDialog((Dialog)this, true);
		}

		public override void Initialize()
		{
			PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(this._treatyInfo.InitiatingPlayerId == this._app.LocalPlayer.ID ? this._treatyInfo.ReceivingPlayerId : this._treatyInfo.InitiatingPlayerId);
			if (this._isVictim)
				this._app.UI.SetPropertyString("lblDesc", "text", string.Format(App.Localize("@UI_TREATY_BROKEN_VICTIM"), (object)playerInfo.Name));
			else
				this._app.UI.SetPropertyString("lblDesc", "text", string.Format(App.Localize("@UI_TREATY_BROKEN_OFFENDER"), (object)playerInfo.Name));
			this._app.UI.ClearItems("lstConsequences");
			if (this._treatyInfo != null)
			{
				foreach (TreatyConsequenceInfo consequence in this._treatyInfo.Consequences)
					this._app.UI.AddItem("lstConsequences", string.Empty, consequence.ID, string.Format("{0} {1}", (object)App.Localize(TreatyEditDialog.ConsequenceTypeLocMap[consequence.Type]), (object)consequence.ConsequenceValue));
			}
			if (this._isVictim)
				return;
			this._app.GameDatabase.RemoveTreatyInfo(this._treatyInfo.ID);
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "button_clicked") || !(panelName == "btnDone"))
				return;
			this._app.UI.CloseDialog((Dialog)this, true);
		}

		public override string[] CloseDialog()
		{
			return (string[])null;
		}
	}
}
