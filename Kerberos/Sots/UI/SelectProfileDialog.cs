// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.SelectProfileDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class SelectProfileDialog : Dialog
	{
		public const string CreateButton = "createButton";
		public const string DeleteButton = "deleteButton";
		public const string OKButton = "okButton";
		private string _enterProfileNameDialog;
		private string _confirmDeleteDialog;
		private int _selection;

		public SelectProfileDialog(App game)
		  : base(game, "dialogSelectProfile")
		{
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			base.OnPanelMessage(panelName, msgType, msgParams);
			if (msgType == "button_clicked")
			{
				if (panelName == "createButton")
					this._enterProfileNameDialog = this._app.UI.CreateDialog((Dialog)new GenericTextEntryDialog(this._app, App.Localize("@PROFILE_DIALOG"), App.Localize("@PROFILE_CREATE_DIALOG"), App.Localize("@GENERAL_DEFACTO"), 16, 2, true, EditBoxFilterMode.ProfileName), null);
				else if (panelName == "deleteButton")
				{
					this._confirmDeleteDialog = this._app.UI.CreateDialog((Dialog)new GenericQuestionDialog(this._app, App.Localize("@DELETE_HEADER"), string.Format(App.Localize("@DELETE_DIALOG"), (object)Profile.GetAvailableProfiles()[this._selection].ProfileName), "dialogGenericQuestion"), null);
				}
				else
				{
					if (!(panelName == "okButton"))
						return;
					List<Profile> availableProfiles = Profile.GetAvailableProfiles();
					this._app.UserProfile = availableProfiles[this._selection];
					this._app.GameSettings.LastProfile = availableProfiles[this._selection].ProfileName;
					this._app.GameSettings.Save();
					if (!HotKeyManager.GetAvailableProfiles().Contains(this._app.UserProfile.ProfileName))
						this._app.HotKeyManager.CreateProfile(this._app.UserProfile.ProfileName);
					this._app.HotKeyManager.LoadProfile(this._app.UserProfile.ProfileName, false);
					this._app.UI.CloseDialog((Dialog)this, true);
				}
			}
			else if (msgType == "dialog_closed")
			{
				if (panelName == this._enterProfileNameDialog)
				{
					if (!bool.Parse(msgParams[0]))
						return;
					foreach (Profile availableProfile in Profile.GetAvailableProfiles())
					{
						if (availableProfile.ProfileName == msgParams[1])
						{
							this._enterProfileNameDialog = this._app.UI.CreateDialog((Dialog)new GenericTextEntryDialog(this._app, App.Localize("@PROFILE_DIALOG"), App.Localize("@PROFILE_CREATE_DIALOG"), App.Localize("@GENERAL_DEFACTO"), 16, 2, false, EditBoxFilterMode.ProfileName), null);
							this._app.UI.CreateDialog((Dialog)new GenericTextDialog(this._app, App.Localize("@ALREADY_EXISTS"), App.Localize("@ALREADY_EXISTS_TEXT"), "dialogGenericMessage"), null);
							return;
						}
					}
					new Profile().CreateProfile(msgParams[1]);
					this.RefreshProfileList();
				}
				else
				{
					if (!(panelName == this._confirmDeleteDialog) || !bool.Parse(msgParams[0]))
						return;
					List<Profile> availableProfiles = Profile.GetAvailableProfiles();
					availableProfiles[this._selection].DeleteProfile();
					if (availableProfiles.Count<Profile>() == 1)
						this._enterProfileNameDialog = this._app.UI.CreateDialog((Dialog)new GenericTextEntryDialog(this._app, App.Localize("@PROFILE_DIALOG"), App.Localize("@PROFILE_CREATE_DIALOG"), App.Localize("@GENERAL_DEFACTO"), 16, 2, false, EditBoxFilterMode.ProfileName), null);
					this.RefreshProfileList();
				}
			}
			else
			{
				if (!(msgType == "list_sel_changed"))
					return;
				this._selection = int.Parse(msgParams[0]);
			}
		}

		private void RefreshProfileList()
		{
			List<Profile> availableProfiles = Profile.GetAvailableProfiles();
			int userItemId = 0;
			this._app.UI.ClearItems(this._app.UI.Path(this.ID, "profileList"));
			foreach (Profile profile in availableProfiles)
			{
				this._app.UI.AddItem(this._app.UI.Path(this.ID, "profileList"), "", userItemId, profile.ProfileName);
				++userItemId;
			}
			this._app.UI.SetSelection(this._app.UI.Path(this.ID, "profileList"), 0);
		}

		public override void Initialize()
		{
			if (Profile.GetAvailableProfiles().Count<Profile>() == 0)
				this._enterProfileNameDialog = this._app.UI.CreateDialog((Dialog)new GenericTextEntryDialog(this._app, App.Localize("@PROFILE_DIALOG"), App.Localize("@PROFILE_CREATE_DIALOG"), App.Localize("@GENERAL_DEFACTO"), 16, 2, false, EditBoxFilterMode.ProfileName), null);
			else
				this.RefreshProfileList();
			base.Initialize();
		}

		public override string[] CloseDialog()
		{
			return new string[0];
		}
	}
}
