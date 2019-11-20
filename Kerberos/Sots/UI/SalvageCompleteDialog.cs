// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.SalvageCompleteDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.TechnologyFramework;
using System;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class SalvageCompleteDialog : Dialog
	{
		public const string OKButton = "okButton";
		private int _techID;

		public SalvageCompleteDialog(App game, int techid)
		  : base(game, "dialogSalvageEvent")
		{
			this._techID = techid;
		}

		public override void Initialize()
		{
			PlayerTechInfo pti = this._app.GameDatabase.GetPlayerTechInfo(this._app.LocalPlayer.ID, this._techID);
			Tech tech = this._app.AssetDatabase.MasterTechTree.Technologies.First<Tech>((Func<Tech, bool>)(x => x.Id == pti.TechFileID));
			this._app.UI.SetText(this._app.UI.Path(this.ID, "research_title"), this._app.AssetDatabase.GetLocalizedTechnologyName(tech.Id));
			this._app.UI.SetText(this._app.UI.Path(this.ID, "research_details"), App.Localize("@TECH_DESC_" + tech.Id));
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "button_clicked") || !(panelName == "okButton"))
				return;
			this._app.UI.CloseDialog((Dialog)this, true);
		}

		public override string[] CloseDialog()
		{
			return (string[])null;
		}
	}
}
