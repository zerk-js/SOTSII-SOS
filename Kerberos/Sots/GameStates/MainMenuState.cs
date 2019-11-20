// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.MainMenuState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.UI;
using System.Collections.Generic;
using System.IO;

namespace Kerberos.Sots.GameStates
{
	internal class MainMenuState : GameState
	{
		private List<object> _postLoginParms = new List<object>();
		private string _profileGUID = "";
		private MainMenuScene _scene;
		private GameState _postLoginState;

		public MainMenuState(App game)
		  : base(game)
		{
		}

		protected void SetNextState(GameState state, params object[] parms)
		{
			this._postLoginState = state;
			this._postLoginParms.Clear();
			this._postLoginParms.AddRange((IEnumerable<object>)parms);
		}

		private void ShowProfileDialog()
		{
			this._profileGUID = this.App.UI.CreateDialog((Dialog)new SelectProfileDialog(this.App), null);
		}

		protected override void OnPrepare(GameState prev, object[] parms)
		{
			if (this.App.GameSettings.LoadMenuCombat)
				this._scene = new MainMenuScene();
			if (this._scene != null)
				this._scene.Enter(this.App);
			this.App.UI.LoadScreen("MainMenu");
			this.App.Network.EnableChatWidgetPlayerList(false);
			this.App.Network.EnableChatWidget(false);
		}

		private bool CanContinueGame()
		{
			return this.App.UserProfile != null && File.Exists(this.App.UserProfile.LastGamePlayed);
		}

		protected override void OnEnter()
		{
			this.App.UI.SetScreen("MainMenu");
			this.App.UI.SetVisible("mainMenuElements", true);
			this.App.UI.SetVisible("gameCredits", false);
			this.App.UI.SetEnabled("gameContinueButton", this.CanContinueGame());
			this.App.UI.SetEnabled("gameCinematicsButton", false);
			this.App.UI.SetText("verNumLabel", "2.0.25104.1");
			bool flag = this.App.Steam.BLoggedOn();
			this.App.UI.SetEnabled("gameMultiplayerButton", flag);
			this.App.UI.SetTooltip("gameMultiplayerButton", flag ? "" : App.Localize("@UI_LOGINTO_STEAM_MULTIPLAYER"));
			this.App.UI.PanelMessage += new UIEventPanelMessage(this.UICommChannel_PanelMessage);
			this._postLoginState = (GameState)null;
			this._postLoginParms.Clear();
			this.App.PostPlayMusic("Main_Menu");
			this.App.PostEnableEffectsSounds(false);
			this.App.PostEnableSpeechSounds(false);
			if (this._scene != null)
				this._scene.Activate();
			if (!this.App.ProfileSelected)
				this.App.UI.CreateDialog((Dialog)new SelectProfileDialog(this.App), null);
			if (!GameDatabase.CheckForPre_EOFSaves(this.App))
				return;
			this.App.UI.CreateDialog((Dialog)new GenericTextDialog(this.App, App.Localize("@PRE_EOF_GAMESAVES_DETECTED"), App.Localize("@PRE_EOF_GAMESAVES_DETECTED_MESSAGE"), "dialogGenericMessage"), null);
		}

		private void UICommChannel_PanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "dialog_closed")
			{
				if (!(panelName == this._profileGUID) || this._postLoginState == null)
					return;
				this.App.SwitchGameState(this._postLoginState, this._postLoginParms.ToArray());
			}
			else
			{
				if (!(msgType == "button_clicked"))
					return;
				if (panelName == "gameExitButton")
					this.App.RequestExit();
				else if (panelName == "gameContinueButton")
				{
					if (this.CanContinueGame())
					{
						this.App.GameSetup.IsMultiplayer = false;
						this.App.UILoadGame(this.App.UserProfile.LastGamePlayed);
					}
					else
						this.App.UI.SetEnabled("gameContinueButton", false);
				}
				else if (panelName == "gameCreateGameButton")
				{
					if (!this.App.ProfileSelected)
					{
						this.SetNextState((GameState)this.App.GetGameState<GameSetupState>(), (object)true);
						this.ShowProfileDialog();
					}
					else
					{
						this.App.ResetGameSetup();
						this.App.GameSetup.IsMultiplayer = false;
						this.App.SwitchGameState<GameSetupState>((object)true);
					}
				}
				else if (panelName == "gameLoadGameButton")
				{
					this.App.ResetGameSetup();
					this.App.GameSetup.IsMultiplayer = false;
					this.App.UI.CreateDialog((Dialog)new LoadGameDialog(this.App, null), null);
				}
				else if (panelName == "gameMultiplayerButton")
				{
					if (!this.App.ProfileSelected)
					{
						this.App.GameSetup.IsMultiplayer = true;
						this.SetNextState((GameState)this.App.GetGameState<StarMapLobbyState>(), (object)LobbyEntranceState.Browser);
						this.ShowProfileDialog();
					}
					else
					{
						this.App.ResetGameSetup();
						this.App.GameSetup.IsMultiplayer = true;
						this.App.SwitchGameState<StarMapLobbyState>((object)LobbyEntranceState.Browser);
					}
				}
				else if (panelName == "gameSotspediaButton")
					this.App.SwitchGameState("SotspediaState");
				else if (panelName == "gameProfileButton")
					this.ShowProfileDialog();
				else if (panelName == "gameOptionsButton")
					this.App.UI.CreateDialog((Dialog)new OptionsDialog(this.App, "OptionsDialog"), null);
				else if (panelName == "gameCinematicsButton")
					this.App.SwitchGameState("CinematicsState");
				else if (panelName == "gameCreditsButton")
				{
					this.App.UI.SetVisible("mainMenuElements", false);
					this.App.UI.SetVisible("gameCredits", true);
					this.App.UI.SetTextFile("gameCreditsText", Path.Combine(AssetDatabase.CommonStrings.UnrootedDirectory, "credits.txt"));
					this.App.UI.Send((object)"SetCreditScrollPosition", (object)"gameCreditsText", (object)0.0f);
					this.App.UI.ForceLayout("gameCredits");
				}
				else
				{
					if (!(panelName == "gameCreditsCloseButton"))
						return;
					this.App.UI.SetVisible("mainMenuElements", true);
					this.App.UI.SetVisible("gameCredits", false);
				}
			}
		}

		protected override void OnExit(GameState prev, ExitReason reason)
		{
			if (this._scene != null)
			{
				this._scene.Exit();
				this._scene = (MainMenuScene)null;
			}
			this.App.UI.PanelMessage -= new UIEventPanelMessage(this.UICommChannel_PanelMessage);
			this.App.UI.DeleteScreen("MainMenu");
		}

		protected override void OnUpdate()
		{
			if (this._scene == null)
				return;
			this._scene.Update();
		}

		public override bool IsReady()
		{
			return (this._scene == null || this._scene.IsReady()) && base.IsReady();
		}

		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
			switch (messageID)
			{
				case InteropMessageID.IMID_SCRIPT_OBJECT_ADD:
					if (this._scene == null)
						break;
					this._scene.AddObject(mr);
					break;
				case InteropMessageID.IMID_SCRIPT_OBJECT_RELEASE:
					if (this._scene == null)
						break;
					this._scene.RemoveObject(mr);
					break;
				case InteropMessageID.IMID_SCRIPT_OBJECTS_RELEASE:
					if (this._scene == null)
						break;
					this._scene.RemoveObjects(mr);
					break;
				default:
					App.Log.Warn("Unhandled message (id=" + (object)messageID + ").", "engine");
					break;
			}
		}
	}
}
