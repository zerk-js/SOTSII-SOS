// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.FeasibilityCompleteDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.Strategy;
using System;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class FeasibilityCompleteDialog : Dialog
	{
		public const string OKButton = "okButton";
		private TurnEvent _event;
		private ResearchInfoPanel _researchInfo;

		public FeasibilityCompleteDialog(App game, TurnEvent ev)
		  : base(game, "dialogFeasibilityEvent")
		{
			this._event = ev;
		}

		public override void Initialize()
		{
			PlayerTechInfo pti = this._app.GameDatabase.GetPlayerTechInfo(this._app.LocalPlayer.ID, this._event.TechID);
			this._app.UI.SetText(this._app.UI.Path(this.ID, "feasible_title"), string.Format(App.Localize("@FEASIBILITY_RESULT"), (object)this._app.AssetDatabase.GetLocalizedTechnologyName(this._app.AssetDatabase.MasterTechTree.Technologies.First<Tech>((Func<Tech, bool>)(x => x.Id == pti.TechFileID)).Id), (object)this._event.FeasibilityPercent));
			this._app.UI.SetText(this._app.UI.Path(this.ID, "feasible_details"), this._event.GetEventMessage(this._app.Game));
			this._researchInfo = new ResearchInfoPanel(this._app.UI, this._app.UI.Path(this.ID, "research_details"));
			this._researchInfo.SetTech(this._app, this._event.TechID);
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "button_clicked"))
				return;
			if (panelName == "okButton")
			{
				this._app.UI.CloseDialog((Dialog)this, true);
			}
			else
			{
				if (!(panelName == "researchButton"))
					return;
				int researchingTechId = this._app.GameDatabase.GetPlayerResearchingTechID(this._app.LocalPlayer.ID);
				if (researchingTechId != 0)
					this._app.GameDatabase.UpdatePlayerTechState(this._app.LocalPlayer.ID, researchingTechId, TechStates.Core);
				int feasibilityStudyTechId = this._app.GameDatabase.GetPlayerFeasibilityStudyTechId(this._app.LocalPlayer.ID);
				if (feasibilityStudyTechId != 0)
					ResearchScreenState.CancelResearchProject(this._app, this._app.LocalPlayer.ID, feasibilityStudyTechId);
				this._app.GameDatabase.UpdatePlayerTechState(this._app.LocalPlayer.ID, this._event.TechID, TechStates.Researching);
				this._app.PostRequestSpeech(string.Format("STRAT_029-01_{0}_StartResearch", (object)this._app.GameDatabase.GetFactionName(this._app.GameDatabase.GetPlayerFactionID(this._app.LocalPlayer.ID))), 50, 120, 0.0f);
				this._app.UI.CloseDialog((Dialog)this, true);
			}
		}

		public override string[] CloseDialog()
		{
			return (string[])null;
		}
	}
}
