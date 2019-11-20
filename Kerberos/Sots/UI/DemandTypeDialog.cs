// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.DemandTypeDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Strategy;
using System.Collections.Generic;

namespace Kerberos.Sots.UI
{
	internal class DemandTypeDialog : Dialog
	{
		public static Dictionary<DemandType, string> DemandTypeLocMap = new Dictionary<DemandType, string>()
	{
	  {
		DemandType.ProvinceDemand,
		"@UI_DIPLOMACY_DEMAND_PROVINCE"
	  },
	  {
		DemandType.ResearchPointsDemand,
		"@UI_DIPLOMACY_DEMAND_RESEARCHPOINTS"
	  },
	  {
		DemandType.SavingsDemand,
		"@UI_DIPLOMACY_DEMAND_SAVINGS"
	  },
	  {
		DemandType.SlavesDemand,
		"@UI_DIPLOMACY_DEMAND_SLAVES"
	  },
	  {
		DemandType.SurrenderDemand,
		"@UI_DIPLOMACY_DEMAND_SURRENDER"
	  },
	  {
		DemandType.SystemInfoDemand,
		"@UI_DIPLOMACY_DEMAND_SYSTEMINFO"
	  },
	  {
		DemandType.WorldDemand,
		"@UI_DIPLOMACY_DEMAND_WORLD"
	  }
	};
		private const string DemandPanel = "pnlDemand";
		private const string BoxPanel = "pnlBoxPanel";
		private const string DemMoneyButton = "btnDemMoney";
		private const string DemSystemInfoButton = "btnDemSystemInfo";
		private const string DemResearchPointsButton = "btnDemResearchPoints";
		private const string DemSlavesButton = "btnDemSlaves";
		private const string DemWorldButton = "btnDemWorld";
		private const string DemProvinceButton = "btnDemProvince";
		private const string DemSurrenderButton = "btnDemSurrender";
		private const string CancelButton = "btnCancel";
		private int _otherPlayer;

		public DemandTypeDialog(App game, int otherPlayer, string template = "dialogDemandType")
		  : base(game, template)
		{
			this._otherPlayer = otherPlayer;
		}

		public override void Initialize()
		{
			DiplomacyUI.SyncDiplomacyPopup(this._app, this.ID, this._otherPlayer);
			PlayerInfo playerInfo1 = this._app.GameDatabase.GetPlayerInfo(this._app.Game.LocalPlayer.ID);
			PlayerInfo playerInfo2 = this._app.GameDatabase.GetPlayerInfo(this._otherPlayer);
			this._app.UI.SetEnabled("btnDemMoney", this._app.Game.CanPerformLocalDiplomacyAction(playerInfo2, DiplomacyAction.DEMAND, new RequestType?(), new DemandType?(DemandType.SavingsDemand)));
			this._app.UI.SetEnabled("btnDemSystemInfo", this._app.Game.CanPerformLocalDiplomacyAction(playerInfo2, DiplomacyAction.DEMAND, new RequestType?(), new DemandType?(DemandType.SystemInfoDemand)));
			this._app.UI.SetEnabled("btnDemResearchPoints", this._app.Game.CanPerformLocalDiplomacyAction(playerInfo2, DiplomacyAction.DEMAND, new RequestType?(), new DemandType?(DemandType.ResearchPointsDemand)));
			this._app.UI.SetEnabled("btnDemSlaves", this._app.Game.CanPerformLocalDiplomacyAction(playerInfo2, DiplomacyAction.DEMAND, new RequestType?(), new DemandType?(DemandType.SlavesDemand)));
			this._app.UI.SetEnabled("btnDemWorld", this._app.Game.CanPerformLocalDiplomacyAction(playerInfo2, DiplomacyAction.DEMAND, new RequestType?(), new DemandType?(DemandType.WorldDemand)));
			this._app.UI.SetEnabled("btnDemProvince", this._app.Game.CanPerformLocalDiplomacyAction(playerInfo2, DiplomacyAction.DEMAND, new RequestType?(), new DemandType?(DemandType.ProvinceDemand)));
			this._app.UI.SetEnabled("btnDemSurrender", this._app.Game.CanPerformLocalDiplomacyAction(playerInfo2, DiplomacyAction.DEMAND, new RequestType?(), new DemandType?(DemandType.SurrenderDemand)));
			this._app.UI.SetButtonText("btnDemMoney", string.Format(App.Localize("@UI_DIPLOMACY_DEMAND_SAVINGS"), (object)this._app.Game.GetDiplomacyActionCost(DiplomacyAction.DEMAND, new RequestType?(), new DemandType?(DemandType.SavingsDemand))));
			this._app.UI.SetButtonText("btnDemSystemInfo", string.Format(App.Localize("@UI_DIPLOMACY_DEMAND_SYSTEMINFO"), (object)this._app.Game.GetDiplomacyActionCost(DiplomacyAction.DEMAND, new RequestType?(), new DemandType?(DemandType.SystemInfoDemand))));
			this._app.UI.SetButtonText("btnDemResearchPoints", string.Format(App.Localize("@UI_DIPLOMACY_DEMAND_RESEARCHPOINTS"), (object)this._app.Game.GetDiplomacyActionCost(DiplomacyAction.DEMAND, new RequestType?(), new DemandType?(DemandType.ResearchPointsDemand))));
			this._app.UI.SetButtonText("btnDemSlaves", string.Format(App.Localize("@UI_DIPLOMACY_DEMAND_SLAVES"), (object)this._app.Game.GetDiplomacyActionCost(DiplomacyAction.DEMAND, new RequestType?(), new DemandType?(DemandType.SlavesDemand))));
			this._app.UI.SetButtonText("btnDemWorld", string.Format(App.Localize("@UI_DIPLOMACY_DEMAND_WORLD"), (object)this._app.Game.GetDiplomacyActionCost(DiplomacyAction.DEMAND, new RequestType?(), new DemandType?(DemandType.WorldDemand))));
			this._app.UI.SetButtonText("btnDemProvince", string.Format(App.Localize("@UI_DIPLOMACY_DEMAND_PROVINCE"), (object)this._app.Game.GetDiplomacyActionCost(DiplomacyAction.DEMAND, new RequestType?(), new DemandType?(DemandType.ProvinceDemand))));
			this._app.UI.SetButtonText("btnDemSurrender", string.Format(App.Localize("@UI_DIPLOMACY_DEMAND_SURRENDER"), (object)this._app.Game.GetDiplomacyActionCost(DiplomacyAction.DEMAND, new RequestType?(), new DemandType?(DemandType.SurrenderDemand))));
			if (this._app.AssetDatabase.GetFaction(playerInfo1.FactionID).HasSlaves())
			{
				this._app.UI.SetPropertyInt("pnlDemand", "height", 210);
				this._app.UI.SetPropertyInt(this._app.UI.Path("pnlDemand", "pnlBoxPanel"), "height", 160);
				this._app.UI.SetVisible("btnDemSlaves", true);
			}
			else
			{
				this._app.UI.SetPropertyInt("pnlDemand", "height", 190);
				this._app.UI.SetPropertyInt(this._app.UI.Path("pnlDemand", "pnlBoxPanel"), "height", 140);
				this._app.UI.SetVisible("btnDemSlaves", false);
			}
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "btnDemMoney")
					this._app.UI.CreateDialog((Dialog)new DemandScalarDialog(this._app, DemandType.SavingsDemand, this._otherPlayer, "dialogRequestScalar"), null);
				else if (panelName == "btnDemSystemInfo")
					this._app.UI.CreateDialog((Dialog)new DemandSystemSelectDialog(this._app, DemandType.SystemInfoDemand, this._otherPlayer, "dialogRequestSystemSelect"), null);
				else if (panelName == "btnDemResearchPoints")
					this._app.UI.CreateDialog((Dialog)new DemandScalarDialog(this._app, DemandType.ResearchPointsDemand, this._otherPlayer, "dialogRequestScalar"), null);
				else if (panelName == "btnDemSlaves")
					this._app.UI.CreateDialog((Dialog)new DemandScalarDialog(this._app, DemandType.SlavesDemand, this._otherPlayer, "dialogRequestScalar"), null);
				else if (panelName == "btnDemWorld")
					this._app.UI.CreateDialog((Dialog)new DemandSystemSelectDialog(this._app, DemandType.WorldDemand, this._otherPlayer, "dialogRequestSystemSelect"), null);
				else if (panelName == "btnDemProvince")
					this._app.UI.CreateDialog((Dialog)new DemandSystemSelectDialog(this._app, DemandType.ProvinceDemand, this._otherPlayer, "dialogRequestSystemSelect"), null);
				else if (panelName == "btnDemSurrender")
				{
					this._app.UI.CreateDialog((Dialog)new DemandSystemSelectDialog(this._app, DemandType.SurrenderDemand, this._otherPlayer, "dialogRequestSystemSelect"), null);
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
