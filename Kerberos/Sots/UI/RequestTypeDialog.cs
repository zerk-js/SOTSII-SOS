// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.RequestTypeDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Strategy;
using System.Collections.Generic;

namespace Kerberos.Sots.UI
{
	internal class RequestTypeDialog : Dialog
	{
		public static Dictionary<RequestType, string> RequestTypeLocMap = new Dictionary<RequestType, string>()
	{
	  {
		RequestType.EstablishEnclaveRequest,
		"@UI_DIPLOMACY_REQUEST_ESTABLISHENCLAVE"
	  },
	  {
		RequestType.GatePermissionRequest,
		"@UI_DIPLOMACY_REQUEST_GATEPERMISSION"
	  },
	  {
		RequestType.MilitaryAssistanceRequest,
		"@UI_DIPLOMACY_REQUEST_MILITARYASSISTANCE"
	  },
	  {
		RequestType.ResearchPointsRequest,
		"@UI_DIPLOMACY_REQUEST_RESEARCHPOINTS"
	  },
	  {
		RequestType.SavingsRequest,
		"@UI_DIPLOMACY_REQUEST_SAVINGS"
	  },
	  {
		RequestType.SystemInfoRequest,
		"@UI_DIPLOMACY_REQUEST_SYSTEMINFO"
	  }
	};
		private const string ReqMoneyButton = "btnReqMoney";
		private const string ReqSystemInfoButton = "btnReqSystemInfo";
		private const string ReqResearchPointsButton = "btnReqResearchPoints";
		private const string ReqMilitaryAssistanceButton = "btnReqMilitaryAssistance";
		private const string ReqGatePermissionButton = "btnReqGatePermission";
		private const string ReqEstablishEnclaveButton = "btnReqEstablishEnclave";
		private const string CancelButton = "btnCancel";
		private int _otherPlayer;

		public RequestTypeDialog(App game, int otherPlayer, string template = "dialogRequestType")
		  : base(game, template)
		{
			this._otherPlayer = otherPlayer;
		}

		public override void Initialize()
		{
			this._app.GameDatabase.GetPlayerInfo(this._app.LocalPlayer.ID);
			PlayerInfo playerInfo = this._app.GameDatabase.GetPlayerInfo(this._otherPlayer);
			DiplomacyUI.SyncDiplomacyPopup(this._app, this.ID, this._otherPlayer);
			this._app.UI.SetEnabled("btnReqMoney", this._app.Game.CanPerformLocalDiplomacyAction(playerInfo, DiplomacyAction.REQUEST, new RequestType?(RequestType.SavingsRequest), new DemandType?()));
			this._app.UI.SetEnabled("btnReqSystemInfo", this._app.Game.CanPerformLocalDiplomacyAction(playerInfo, DiplomacyAction.REQUEST, new RequestType?(RequestType.SystemInfoRequest), new DemandType?()));
			this._app.UI.SetEnabled("btnReqResearchPoints", this._app.Game.CanPerformLocalDiplomacyAction(playerInfo, DiplomacyAction.REQUEST, new RequestType?(RequestType.ResearchPointsRequest), new DemandType?()));
			this._app.UI.SetEnabled("btnReqMilitaryAssistance", this._app.Game.CanPerformLocalDiplomacyAction(playerInfo, DiplomacyAction.REQUEST, new RequestType?(RequestType.MilitaryAssistanceRequest), new DemandType?()));
			this._app.UI.SetEnabled("btnReqGatePermission", this._app.Game.CanPerformLocalDiplomacyAction(playerInfo, DiplomacyAction.REQUEST, new RequestType?(RequestType.GatePermissionRequest), new DemandType?()));
			this._app.UI.SetEnabled("btnReqEstablishEnclave", this._app.Game.CanPerformLocalDiplomacyAction(playerInfo, DiplomacyAction.REQUEST, new RequestType?(RequestType.EstablishEnclaveRequest), new DemandType?()));
			this._app.UI.SetButtonText("btnReqMoney", string.Format(App.Localize("@UI_DIPLOMACY_REQUEST_SAVINGS"), (object)this._app.Game.GetDiplomacyActionCost(DiplomacyAction.REQUEST, new RequestType?(RequestType.SavingsRequest), new DemandType?())));
			this._app.UI.SetButtonText("btnReqSystemInfo", string.Format(App.Localize("@UI_DIPLOMACY_REQUEST_SYSTEMINFO"), (object)this._app.Game.GetDiplomacyActionCost(DiplomacyAction.REQUEST, new RequestType?(RequestType.SystemInfoRequest), new DemandType?())));
			this._app.UI.SetButtonText("btnReqResearchPoints", string.Format(App.Localize("@UI_DIPLOMACY_REQUEST_RESEARCHPOINTS"), (object)this._app.Game.GetDiplomacyActionCost(DiplomacyAction.REQUEST, new RequestType?(RequestType.ResearchPointsRequest), new DemandType?())));
			this._app.UI.SetButtonText("btnReqMilitaryAssistance", string.Format(App.Localize("@UI_DIPLOMACY_REQUEST_MILITARYASSISTANCE"), (object)this._app.Game.GetDiplomacyActionCost(DiplomacyAction.REQUEST, new RequestType?(RequestType.MilitaryAssistanceRequest), new DemandType?())));
			this._app.UI.SetButtonText("btnReqGatePermission", string.Format(App.Localize("@UI_DIPLOMACY_REQUEST_GATEPERMISSION"), (object)this._app.Game.GetDiplomacyActionCost(DiplomacyAction.REQUEST, new RequestType?(RequestType.GatePermissionRequest), new DemandType?())));
			this._app.UI.SetButtonText("btnReqEstablishEnclave", string.Format(App.Localize("@UI_DIPLOMACY_REQUEST_ESTABLISHENCLAVE"), (object)this._app.Game.GetDiplomacyActionCost(DiplomacyAction.REQUEST, new RequestType?(RequestType.EstablishEnclaveRequest), new DemandType?())));
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "btnReqMoney")
					this._app.UI.CreateDialog((Dialog)new RequestScalarDialog(this._app, RequestType.SavingsRequest, this._otherPlayer, "dialogRequestScalar"), null);
				else if (panelName == "btnReqSystemInfo")
					this._app.UI.CreateDialog((Dialog)new RequestSystemSelectDialog(this._app, RequestType.SystemInfoRequest, this._otherPlayer, "dialogRequestSystemSelect"), null);
				else if (panelName == "btnReqResearchPoints")
					this._app.UI.CreateDialog((Dialog)new RequestScalarDialog(this._app, RequestType.ResearchPointsRequest, this._otherPlayer, "dialogRequestScalar"), null);
				else if (panelName == "btnReqMilitaryAssistance")
					this._app.UI.CreateDialog((Dialog)new RequestSystemSelectDialog(this._app, RequestType.MilitaryAssistanceRequest, this._otherPlayer, "dialogRequestSystemSelect"), null);
				else if (panelName == "btnReqGatePermission")
					this._app.UI.CreateDialog((Dialog)new RequestSystemSelectDialog(this._app, RequestType.GatePermissionRequest, this._otherPlayer, "dialogRequestSystemSelect"), null);
				else if (panelName == "btnReqEstablishEnclave")
				{
					this._app.UI.CreateDialog((Dialog)new RequestSystemSelectDialog(this._app, RequestType.EstablishEnclaveRequest, this._otherPlayer, "dialogRequestSystemSelect"), null);
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
