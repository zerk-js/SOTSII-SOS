// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.ResearchCompleteDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameStates;
using System;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class ResearchCompleteDialog : Dialog
	{
		public const string OKButton = "okButton";
		public const string EncyclopediaInfoButton = "navTechInfo";
		private int _techID;
		private ResearchInfoPanel _researchInfo;

		public ResearchCompleteDialog(App game, int techid)
		  : base(game, "dialogResearchEvent")
		{
			this._techID = techid;
		}

		public override void Initialize()
		{
			if (ScriptHost.AllowConsole)
				this._app.UI.SetVisible("navTechInfo", true);
			else
				this._app.UI.SetVisible("navTechInfo", false);
			PlayerTechInfo pti = this._app.GameDatabase.GetPlayerTechInfo(this._app.LocalPlayer.ID, this._techID);
			Tech tech = this._app.AssetDatabase.MasterTechTree.Technologies.First<Tech>((Func<Tech, bool>)(x => x.Id == pti.TechFileID));
			this._app.UI.SetText(this._app.UI.Path(this.ID, "research_title"), this._app.AssetDatabase.GetLocalizedTechnologyName(tech.Id));
			if (!this._app.UserProfile.ResearchedTechs.Contains(tech.Id))
				this._app.UserProfile.ResearchedTechs.Add(tech.Id);
			this._researchInfo = new ResearchInfoPanel(this._app.UI, this._app.UI.Path(this.ID, "research_details"));
			this._researchInfo.SetTech(this._app, this._techID);
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
				if (!(panelName == "navTechInfo"))
					return;
				string techFileId = this._app.GameDatabase.GetTechFileID(this._techID);
				if (techFileId == null)
					return;
				SotspediaState.NavigateToLink(this._app, "#" + techFileId);
			}
		}

		public override string[] CloseDialog()
		{
			return (string[])null;
		}
	}
}
