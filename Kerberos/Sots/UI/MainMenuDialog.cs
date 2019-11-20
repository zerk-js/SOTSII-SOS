// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.MainMenuDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.GameStates;
using System;
using System.Text.RegularExpressions;

namespace Kerberos.Sots.UI
{
	internal class MainMenuDialog : Dialog
	{
		public const string UIMainMenuDialog = "gameMainMenu";
		public const string UIMainMenuDialogBack = "gameMainDialogBack";
		public const string UIMainMenuDialogQuit = "gameMainDialogQuit";
		public const string UIMainMenuDialogOptions = "gameOptionsButton";
		public const string UIMainMenuDialogSave = "gameSaveGameButton";
		public const string UIMainMenuDialogAutoMenu = "gameAutoMenuButton";
		public const string UIMainMenuDialogkeybinds = "gameKeyBindButton";
		private string _confirmExitToMenu;

		public MainMenuDialog(App game)
		  : base(game, "dialogMainMenu")
		{
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "gameMainDialogQuit")
					this._confirmExitToMenu = this._app.UI.CreateDialog((Dialog)new GenericQuestionDialog(this._app, App.Localize("@UI_CONFIRM_MENU_RETURN_TITLE"), App.Localize("@UI_CONFIRM_MENU_RETURN_DESCRIPTION"), "dialogGenericQuestion"), null);
				else if (panelName == "gameMainDialogBack")
					this._app.UI.CloseDialog((Dialog)this, true);
				else if (panelName == "gameOptionsButton")
					this._app.UI.CreateDialog((Dialog)new OptionsDialog(this._app, "OptionsDialog"), null);
				else if (panelName == "gameSaveGameButton")
				{
					if (this._app.Game == null) return;

					//string pattern = "\\s*\\((?:" + Regex.Replace(App.Localize("@AUTOSAVE_SUFFIX"), "([\\(\\)])", string.Empty) + "|Precombat)\\).*";
					//string saveName = Regex.Replace(this._app.Game.SaveGameName ?? string.Empty, pattern, string.Empty);
					string saveName = this._app.Game.SaveGameName ?? string.Empty;
					this._app.UI.CreateDialog((Dialog)new SaveGameDialog(this._app, saveName, "dialogSaveGame"), null);
				}
				else if (panelName == "gameAutoMenuButton")
				{
					this._app.UI.CreateDialog((Dialog)new AutoMenuDialog(this._app), null);
				}
				else
				{
					if (!(panelName == "gameKeyBindButton"))
						return;
					this._app.UI.CreateDialog((Dialog)new HotKeyDialog(this._app), null);
				}
			}
			else
			{
				if (!(msgType == "dialog_closed") || !(panelName == this._confirmExitToMenu))
					return;
				if (bool.Parse(msgParams[0]))
				{
					if (this._app.GameSetup.IsMultiplayer)
						this._app.Network.Disconnect();
					this._app.GetGameState<StarMapState>().Reset();
					this._app.UI.CloseDialog((Dialog)this, true);
					this._app.UI.SetVisible("gameMainMenu", false);
					this._app.SwitchGameStateViaLoadingScreen((Action)null, (LoadingFinishedDelegate)null, (GameState)this._app.GetGameState<MainMenuState>(), (object[])null);
				}
				this._confirmExitToMenu = "";
			}
		}

		public override string[] CloseDialog()
		{
			return (string[])null;
		}
	}
}
