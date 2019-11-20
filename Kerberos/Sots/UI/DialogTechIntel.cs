// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.DialogTechIntel
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.GameStates;
using System;
using System.IO;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class DialogTechIntel : Dialog
	{
		public const string OKButton = "okButton";
		public const string EncyclopediaInfoButton = "navTechInfo";
		private int _techID;
		private PlayerInfo _targetPlayer;
		private ResearchInfoPanel _researchInfo;

		public DialogTechIntel(App game, int techid, PlayerInfo targetPlayer)
		  : base(game, "dialogResearchIntel")
		{
			this._techID = techid;
			this._targetPlayer = targetPlayer;
		}

		public override void Initialize()
		{
			this._app.UI.SetVisible("navTechInfo", false);
			string pti = this._app.GameDatabase.GetTechFileID(this._techID);
			this._app.AssetDatabase.MasterTechTree.Technologies.First<Tech>((Func<Tech, bool>)(x => x.Id == pti));
			this._app.UI.SetText(this._app.UI.Path(this.ID, "subheader"), "Stole intel from " + this._targetPlayer.Name);
			this._researchInfo = new ResearchInfoPanel(this._app.UI, this._app.UI.Path(this.ID, "research_details"));
			this._researchInfo.SetTech(this._app, this._techID);
			this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "playerAvatar"), "sprite", Path.GetFileNameWithoutExtension(this._targetPlayer.AvatarAssetPath));
			this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "playerBadge"), "sprite", Path.GetFileNameWithoutExtension(this._targetPlayer.BadgeAssetPath));
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
