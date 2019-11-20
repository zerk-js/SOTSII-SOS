// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.OptionsDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.GameStates;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class OptionsDialog : Dialog
	{
		private static string[] EnabledListItems = new string[2]
		{
	  App.Localize("@UI_OPTIONS_DIALOG_NO"),
	  App.Localize("@UI_OPTIONS_DIALOG_YES")
		};
		private static string[] DetailItems = new string[3]
		{
	  App.Localize("@UI_OPTIONS_DIALOG_DETAIL_LOW"),
	  App.Localize("@UI_OPTIONS_DIALOG_DETAIL_MEDIUM"),
	  App.Localize("@UI_OPTIONS_DIALOG_DETAIL_HIGH")
		};
		private const string ProfileName = "lblProfileName";
		private const string EndTurnDelay = "preferencesEndTurnDelayValueSlider";
		private const string EndTurnDelayLabel = "preferencesEndTurnDelayValueSliderLabel";
		private const string MusicVolume = "audioMusicVolumeValueSlider";
		private const string MusicVolumeLabel = "audioMusicVolumeValueSliderLabel";
		private const string SpeechVolume = "audioSpeechVolumeValueSlider";
		private const string SpeechVolumeLabel = "audioSpeechVolumeValueSliderLabel";
		private const string EffectsVolume = "audioEffectsVolumeValueSlider";
		private const string EffectsVolumeLabel = "audioEffectsVolumeValueSliderLabel";
		private const string SeperateStarMapFocus = "preferencesSeparateStarMapFocusDDL";
		private const string MenuBackgroundCombat = "preferencesMenuBackgroundCombat";
		private const string FleetCheck = "preferencesInactiveFleets";
		private const string SpeechSubtitles = "preferencesSpeechSubtitles";
		private const string AudioEnabled = "audioEnabledDDL";
		private const string gfxPreferredDisplay = "graphicsPreferredDisplayDDL";
		private const string gfxDisplayMode = "graphicsDisplayModeDDL";
		private const string gfxAntialiasting = "graphicsAntialiasingDDL";
		private const string gfxTextureQuality = "graphicsTextureQualityDDL";
		private const string gfxDepthOfField = "graphicsDepthOfFieldDDL";
		private const string gfxShadowQuality = "graphicsShadowQualityDDL";
		private const string gfxCreaseShading = "graphicsCreaseShadingDDL";
		private const string gfxParticleDetail = "graphicsParticleDetailDDL";
		private const string JoinGlobal = "preferencesJoinGlobal";
		private const string AutoSave = "preferencesAutoSave";
		private const string EndTurnDelayFormat = "{0} sec";
		private const string VolumeFormat = "{0}%";
		private Settings _currentSettings;

		public OptionsDialog(App game, string template = "OptionsDialog")
		  : base(game, template)
		{
		}

		public override void Initialize()
		{
			this.InitializeComponents();
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "gameOptionsOK")
				{
					this.CommitSettings();
					this._app.UI.CloseDialog((Dialog)this, true);
				}
				else
				{
					if (!(panelName == "gameOptionsReset"))
						return;
					this._app.UI.CloseDialog((Dialog)this, true);
				}
			}
			else if (msgType == "slider_value")
			{
				switch (panelName)
				{
					case "preferencesEndTurnDelayValueSlider":
						this._app.UI.SetText("preferencesEndTurnDelayValueSliderLabel", string.Format("{0} sec", (object)msgParams[0]));
						this._currentSettings.EndTurnDelay = int.Parse(msgParams[0]);
						break;
					case "audioMusicVolumeValueSlider":
						this._app.UI.SetText("audioMusicVolumeValueSliderLabel", string.Format("{0}%", (object)msgParams[0]));
						this._currentSettings.MusicVolume = int.Parse(msgParams[0]);
						this._app.PostSetVolumeMusic(this._currentSettings.MusicVolume);
						break;
					case "audioSpeechVolumeValueSlider":
						this._app.UI.SetText("audioSpeechVolumeValueSliderLabel", string.Format("{0}%", (object)msgParams[0]));
						this._currentSettings.SpeechVolume = int.Parse(msgParams[0]);
						this._app.PostSetVolumeSpeech(this._currentSettings.SpeechVolume);
						this._app.PostEnableSpeechSounds(true);
						this._app.PostRequestSpeech("COMBAT_005-01_human_SelectionAffirmed", 1000, 120, 0.0f);
						break;
					case "audioEffectsVolumeValueSlider":
						this._app.UI.SetText("audioEffectsVolumeValueSliderLabel", string.Format("{0}%", (object)msgParams[0]));
						this._currentSettings.EffectsVolume = int.Parse(msgParams[0]);
						this._app.PostSetVolumeEffects(this._currentSettings.EffectsVolume);
						this._app.PostRequestEffectSound("Explode_BattleRiderDeath");
						this._app.PostEnableEffectsSounds(true);
						break;
				}
			}
			else
			{
				if (!(msgType == "list_sel_changed"))
					return;
				switch (panelName)
				{
					case "graphicsAntialiasingDDL":
						this._currentSettings.gfxAntialiasting = int.Parse(msgParams[0]);
						break;
					case "graphicsPreferredDisplayDDL":
						this._currentSettings.gfxPreferredDisplay = int.Parse(msgParams[0]);
						break;
					case "graphicsDisplayModeDDL":
						this._currentSettings.gfxDisplayMode = int.Parse(msgParams[0]);
						break;
					case "graphicsCreaseShadingDDL":
						this._currentSettings.gfxCreaseShading = int.Parse(msgParams[0]);
						break;
					case "graphicsDepthOfFieldDDL":
						this._currentSettings.gfxDepthOfField = int.Parse(msgParams[0]);
						break;
					case "graphicsParticleDetailDDL":
						this._currentSettings.gfxParticleDetail = int.Parse(msgParams[0]);
						break;
					case "graphicsShadowQualityDDL":
						this._currentSettings.gfxShadowQuality = int.Parse(msgParams[0]);
						break;
					case "graphicsTextureQualityDDL":
						this._currentSettings.gfxTextureQuality = int.Parse(msgParams[0]);
						break;
					case "preferencesSeparateStarMapFocusDDL":
						if (int.Parse(msgParams[0]) > 0)
						{
							this._currentSettings.SeperateStarMapFocus = true;
							break;
						}
						this._currentSettings.SeperateStarMapFocus = false;
						break;
					case "audioEnabledDDL":
						if (int.Parse(msgParams[0]) > 0)
						{
							this._currentSettings.AudioEnabled = true;
							break;
						}
						this._currentSettings.AudioEnabled = false;
						break;
					case "preferencesMenuBackgroundCombat":
						if (int.Parse(msgParams[0]) > 0)
						{
							this._currentSettings.LoadMenuCombat = true;
							break;
						}
						this._currentSettings.LoadMenuCombat = false;
						break;
					case "preferencesInactiveFleets":
						this._currentSettings.CheckForInactiveFleets = int.Parse(msgParams[0]) > 0;
						break;
					case "preferencesJoinGlobal":
						this._currentSettings.JoinGlobalChat = int.Parse(msgParams[0]) > 0;
						break;
					case "preferencesAutoSave":
						this._currentSettings.AutoSave = int.Parse(msgParams[0]) > 0;
						break;
					case "preferencesSpeechSubtitles":
						if (int.Parse(msgParams[0]) > 0)
						{
							this._currentSettings.SpeechSubtitles = true;
							break;
						}
						this._currentSettings.SpeechSubtitles = false;
						break;
				}
			}
		}

		protected void PopulateListItems(string listId, string[] options, int? defaultOption = null)
		{
			this._app.UI.ClearItems(listId);
			for (int userItemId = 0; userItemId < ((IEnumerable<string>)options).Count<string>(); ++userItemId)
				this._app.UI.AddItem(listId, string.Empty, userItemId, options[userItemId]);
			if (!defaultOption.HasValue)
				return;
			this._app.UI.SetSelection(listId, defaultOption.Value);
		}

		protected void InitializeComponents()
		{
			this._currentSettings = new Settings(this._app.SettingsDir);
			this._currentSettings.CopyFrom(this._app.GameSettings);
			this._app.UI.SetText("lblProfileName", this._app.UserProfile.ProfileName);
			int endTurnDelay = this._app.GameSettings.EndTurnDelay;
			this._app.UI.SetSliderValue("preferencesEndTurnDelayValueSlider", endTurnDelay);
			this._app.UI.SetText("preferencesEndTurnDelayValueSliderLabel", string.Format("{0} sec", (object)endTurnDelay));
			this._app.UI.SetSliderValue("audioMusicVolumeValueSlider", this._app.GameSettings.MusicVolume);
			this._app.UI.SetText("audioMusicVolumeValueSliderLabel", string.Format("{0}%", (object)this._app.GameSettings.MusicVolume));
			this._app.UI.SetSliderValue("audioSpeechVolumeValueSlider", this._app.GameSettings.SpeechVolume);
			this._app.UI.SetText("audioSpeechVolumeValueSliderLabel", string.Format("{0}%", (object)this._app.GameSettings.SpeechVolume));
			this._app.UI.SetSliderValue("audioEffectsVolumeValueSlider", this._app.GameSettings.EffectsVolume);
			this._app.UI.SetText("audioEffectsVolumeValueSliderLabel", string.Format("{0}%", (object)this._app.GameSettings.EffectsVolume));
			this.PopulateListItems("preferencesSeparateStarMapFocusDDL", OptionsDialog.EnabledListItems, new int?(this._app.GameSettings.SeperateStarMapFocus ? 1 : 0));
			this.PopulateListItems("preferencesMenuBackgroundCombat", OptionsDialog.EnabledListItems, new int?(this._app.GameSettings.LoadMenuCombat ? 1 : 0));
			this.PopulateListItems("preferencesInactiveFleets", OptionsDialog.EnabledListItems, new int?(this._app.GameSettings.CheckForInactiveFleets ? 1 : 0));
			this.PopulateListItems("preferencesJoinGlobal", OptionsDialog.EnabledListItems, new int?(this._app.GameSettings.JoinGlobalChat ? 1 : 0));
			this.PopulateListItems("preferencesAutoSave", OptionsDialog.EnabledListItems, new int?(this._app.GameSettings.AutoSave ? 1 : 0));
			this.PopulateListItems("preferencesSpeechSubtitles", OptionsDialog.EnabledListItems, new int?(this._app.GameSettings.SpeechSubtitles ? 1 : 0));
			this.PopulateListItems("audioEnabledDDL", OptionsDialog.EnabledListItems, new int?(this._app.GameSettings.AudioEnabled ? 1 : 0));
			this.PopulateListItems("graphicsTextureQualityDDL", OptionsDialog.DetailItems, new int?(this._app.GameSettings.gfxTextureQuality));
			this.PopulateListItems("graphicsDepthOfFieldDDL", OptionsDialog.DetailItems, new int?(this._app.GameSettings.gfxDepthOfField));
			this.PopulateListItems("graphicsShadowQualityDDL", OptionsDialog.DetailItems, new int?(this._app.GameSettings.gfxShadowQuality));
			this.PopulateListItems("graphicsCreaseShadingDDL", OptionsDialog.DetailItems, new int?(this._app.GameSettings.gfxCreaseShading));
			this.PopulateListItems("graphicsParticleDetailDDL", OptionsDialog.DetailItems, new int?(this._app.GameSettings.gfxParticleDetail));
		}

		protected void CommitSettings()
		{
			this._app.GameSettings.CopyFrom(this._currentSettings);
			this._app.GameSettings.Save();
			this._app.GameSettings.Commit(this._app);
		}

		public override string[] CloseDialog()
		{
			if (this._app.CurrentState == this._app.GetGameState<MainMenuState>())
			{
				this._app.PostEnableEffectsSounds(false);
				this._app.PostEnableSpeechSounds(false);
			}
			return (string[])null;
		}
	}
}
