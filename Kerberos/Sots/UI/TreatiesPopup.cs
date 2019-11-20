// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.TreatiesPopup
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class TreatiesPopup : Dialog
	{
		public static string DoneButton = "btnTreatyOk";
		public static string TreatyList = "lstTreaties";
		public static string AddTreatyButton = "btnAddTreaty";
		public static string RemoveTreatyButton = "btnRemoveTreaty";
		private int _otherPlayer;
		private string _treatyEditDialog;
		private int? _selectedTreaty;
		private List<TreatyInfo> _treaties;

		public TreatiesPopup(App game, int otherPlayer, string template = "TreatiesPopup")
		  : base(game, template)
		{
			this._otherPlayer = otherPlayer;
		}

		public override void Initialize()
		{
			DiplomacyUI.SyncDiplomacyPopup(this._app, this.ID, this._otherPlayer);
			this.SyncTreaties();
		}

		private void SyncTreaties()
		{
			this._app.UI.ClearItems(TreatiesPopup.TreatyList);
			this._treaties = this._app.GameDatabase.GetTreatyInfos().Where<TreatyInfo>((Func<TreatyInfo, bool>)(x =>
		   {
			   if (x.ReceivingPlayerId == this._otherPlayer && x.InitiatingPlayerId == this._app.LocalPlayer.ID)
				   return true;
			   if (x.ReceivingPlayerId == this._app.LocalPlayer.ID)
				   return x.InitiatingPlayerId == this._otherPlayer;
			   return false;
		   })).ToList<TreatyInfo>();
			foreach (TreatyInfo treaty in this._treaties)
			{
				if (treaty.Type == TreatyType.Limitation)
				{
					LimitationTreatyInfo limitationTreatyInfo = (LimitationTreatyInfo)treaty;
					this._app.UI.AddItem(TreatiesPopup.TreatyList, string.Empty, treaty.ID, string.Empty);
					this._app.UI.SetText(this._app.UI.Path(this._app.UI.GetItemGlobalID(TreatiesPopup.TreatyList, string.Empty, treaty.ID, string.Empty), "lblHeader"), string.Format("{0}", (object)App.Localize(TreatyEditDialog.LimitationTreatyTypeLocMap[limitationTreatyInfo.LimitationType])));
				}
				else
				{
					this._app.UI.AddItem(TreatiesPopup.TreatyList, string.Empty, treaty.ID, string.Empty);
					this._app.UI.SetText(this._app.UI.Path(this._app.UI.GetItemGlobalID(TreatiesPopup.TreatyList, string.Empty, treaty.ID, string.Empty), "lblHeader"), App.Localize(TreatyEditDialog.TreatyTypeLocMap[treaty.Type]));
				}
			}
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "list_sel_changed")
			{
				if (!(panelName == TreatiesPopup.TreatyList) || string.IsNullOrEmpty(msgParams[0]))
					return;
				this._selectedTreaty = new int?(int.Parse(msgParams[0]));
			}
			else if (msgType == "button_clicked")
			{
				if (panelName == TreatiesPopup.DoneButton)
					this._app.UI.CloseDialog((Dialog)this, true);
				else if (panelName == TreatiesPopup.AddTreatyButton)
				{
					this._treatyEditDialog = this._app.UI.CreateDialog((Dialog)new TreatyEditDialog(this._app, this._otherPlayer, "TreatyConfigurationPopup"), null);
				}
				else
				{
					if (!(panelName == TreatiesPopup.RemoveTreatyButton) || !this._selectedTreaty.HasValue || this._treaties.First<TreatyInfo>((Func<TreatyInfo, bool>)(x => x.ID == this._selectedTreaty.Value)).StartingTurn <= this._app.GameDatabase.GetTurnCount())
						return;
					this._app.GameDatabase.RemoveTreatyInfo(this._selectedTreaty.Value);
					this._app.UI.ClearSelection(TreatiesPopup.TreatyList);
					this._selectedTreaty = new int?();
					this.SyncTreaties();
				}
			}
			else
			{
				if (!(msgType == "dialog_closed") || !(panelName == this._treatyEditDialog))
					return;
				this.SyncTreaties();
				this._app.UI.ClearSelection(TreatiesPopup.TreatyList);
				this._selectedTreaty = new int?();
			}
		}

		public override string[] CloseDialog()
		{
			return new List<string>().ToArray();
		}
	}
}
