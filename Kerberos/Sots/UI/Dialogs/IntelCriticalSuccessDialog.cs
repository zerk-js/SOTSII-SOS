// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.Dialogs.IntelCriticalSuccessDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Kerberos.Sots.UI.Dialogs
{
	internal class IntelCriticalSuccessDialog : Dialog
	{
		private readonly int _targetPlayer;
		private readonly GameSession _game;
		private Button _okButton;
		private EspionagePlayerHeader _playerHeader;
		private Label _descLabel;
		private DropDownList[] _intelDdls;
		private DropDownList _blameDdl;

		public IntelCriticalSuccessDialog(GameSession game, int targetPlayer)
		  : base(game.App, "dialogIntelMajorSuccess")
		{
			this._targetPlayer = targetPlayer;
			this._game = game;
		}

		private void RepopulateIntelDDLs(DropDownList ignore)
		{
			Dictionary<DropDownList, IntelMissionDesc> dictionary = new Dictionary<DropDownList, IntelMissionDesc>();
			IEnumerable<DropDownList> dropDownLists = ((IEnumerable<DropDownList>)this._intelDdls).Where<DropDownList>((Func<DropDownList, bool>)(x => x != ignore));
			foreach (DropDownList index in dropDownLists)
			{
				if (index.SelectedItem != null)
					dictionary[index] = (IntelMissionDesc)index.SelectedItem;
			}
			foreach (DropDownList key in dropDownLists)
			{
				HashSet<IntelMissionDesc> source = new HashSet<IntelMissionDesc>((IEnumerable<IntelMissionDesc>)this._game.AssetDatabase.IntelMissions);
				foreach (KeyValuePair<DropDownList, IntelMissionDesc> keyValuePair in dictionary)
				{
					if (keyValuePair.Key != key)
						source.Remove(keyValuePair.Value);
				}
				key.Clear();
				foreach (IntelMissionDesc intelMissionDesc in source)
					key.AddItem((object)intelMissionDesc, intelMissionDesc.Name);
				IntelMissionDesc intelMissionDesc1 = (IntelMissionDesc)null;
				if (!dictionary.TryGetValue(key, out intelMissionDesc1))
				{
					intelMissionDesc1 = source.FirstOrDefault<IntelMissionDesc>();
					if (intelMissionDesc1 != null)
						dictionary[key] = intelMissionDesc1;
				}
				key.SetSelection((object)intelMissionDesc1);
			}
		}

		private void RepopulateBlameDDL()
		{
			List<PlayerInfo> list = this._game.GameDatabase.GetPlayerInfos().Where<PlayerInfo>((Func<PlayerInfo, bool>)(x =>
		   {
			   if (x.isStandardPlayer)
				   return x.ID != this._game.LocalPlayer.ID;
			   return false;
		   })).ToList<PlayerInfo>();
			this._blameDdl.Clear();
			this._blameDdl.AddItem((object)string.Empty, App.Localize("@UI_DIPLOMACY_INTEL_CRITICAL_SUCCESS_NOBLAME"));
			foreach (PlayerInfo playerInfo in list)
			{
				this._blameDdl.AddItem((object)playerInfo, playerInfo.Name);
				this._blameDdl.GetLastItemID();
			}
			if (list.Count > 0)
				this._blameDdl.SetSelection((object)list[0]);
			else
				this._blameDdl.SetSelection((object)string.Empty);
			this.BlameDDLSelectionChanged((object)null, (EventArgs)null);
		}

		private IEnumerable<IntelMissionDesc> GetSelectedMissions()
		{
			List<IntelMissionDesc> intelMissionDescList = new List<IntelMissionDesc>();
			foreach (DropDownList intelDdl in this._intelDdls)
			{
				if (intelDdl.SelectedItem != null)
					intelMissionDescList.Add((IntelMissionDesc)intelDdl.SelectedItem);
			}
			return (IEnumerable<IntelMissionDesc>)intelMissionDescList;
		}

		private void IntelDDLSelectionChanged(object sender, EventArgs e)
		{
			this.RepopulateIntelDDLs(sender as DropDownList);
		}

		private void BlameDDLSelectionChanged(object sender, EventArgs e)
		{
			PlayerInfo selectedItem = this._blameDdl.SelectedItem as PlayerInfo;
			if (selectedItem == null)
			{
				this._app.UI.SetVisible(this.UI.Path(this.ID, "imgBlameAvatar"), false);
				this._app.UI.SetVisible(this.UI.Path(this.ID, "imgBlameBadge"), false);
			}
			else
			{
				this._app.UI.SetVisible(this.UI.Path(this.ID, "imgBlameAvatar"), true);
				this._app.UI.SetVisible(this.UI.Path(this.ID, "imgBlameBadge"), true);
				this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "imgBlameAvatar"), "sprite", Path.GetFileNameWithoutExtension(selectedItem.AvatarAssetPath));
				this._app.UI.SetPropertyString(this._app.UI.Path(this.ID, "imgBlameBadge"), "sprite", Path.GetFileNameWithoutExtension(selectedItem.BadgeAssetPath));
			}
		}

		private void _okButton_Clicked(object sender, EventArgs e)
		{
			this._app.Game.DoIntelMissionCriticalSuccess(this._targetPlayer, this.GetSelectedMissions(), this._blameDdl.SelectedItem as PlayerInfo);
			this._app.UI.CloseDialog((Dialog)this, true);
		}

		public override void Initialize()
		{
			this._okButton = new Button(this.UI, this.UI.Path(this.ID, "btnOk"), null);
			this._okButton.Clicked += new EventHandler(this._okButton_Clicked);
			this._playerHeader = new EspionagePlayerHeader(this._game, this.ID);
			this._descLabel = new Label(this.UI, this.UI.Path(this.ID, "lblIntelDesc"));
			this._intelDdls = new DropDownList[3]
			{
		new DropDownList(this.UI, this.UI.Path(this.ID, "ddlIntel1")),
		new DropDownList(this.UI, this.UI.Path(this.ID, "ddlIntel2")),
		new DropDownList(this.UI, this.UI.Path(this.ID, "ddlIntel3"))
			};
			foreach (DropDownList intelDdl in this._intelDdls)
				intelDdl.SelectionChanged += new EventHandler(this.IntelDDLSelectionChanged);
			this._blameDdl = new DropDownList(this.UI, this.UI.Path(this.ID, "ddlBlame"));
			this._blameDdl.SelectionChanged += new EventHandler(this.BlameDDLSelectionChanged);
			this.AddPanels((PanelBinding)this._okButton, (PanelBinding)this._playerHeader, (PanelBinding)this._descLabel, (PanelBinding)this._blameDdl);
			this.AddPanels((PanelBinding[])this._intelDdls);
			PlayerInfo playerInfo = this._game.GameDatabase.GetPlayerInfo(this._targetPlayer);
			this._playerHeader.UpdateFromPlayerInfo(this._game.LocalPlayer.ID, playerInfo);
			DiplomacyUI.SyncPanelColor(this._app, this.ID, playerInfo.PrimaryColor);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(App.Localize("@UI_DIPLOMACY_INTEL_CRITICAL_SUCCESS_TITLE") + "\n");
			stringBuilder.Append(string.Format(App.Localize("@UI_DIPLOMACY_INTEL_INFO_DESC_TARGET") + "\n", (object)playerInfo.Name));
			this._descLabel.SetText(stringBuilder.ToString());
			this.RepopulateIntelDDLs((DropDownList)null);
			this.RepopulateBlameDDL();
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			this.TryPanelMessage(panelName, msgType, msgParams, PanelBinding.PanelMessageTargetFlags.Recursive);
		}

		public override string[] CloseDialog()
		{
			return (string[])null;
		}
	}
}
