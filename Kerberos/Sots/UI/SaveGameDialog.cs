// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.SaveGameDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kerberos.Sots.UI
{
	internal class SaveGameDialog : Dialog
	{
		private int _minChars = 1;
		private int _maxChars = 64;
		protected int _selectedIndex = -1;
		protected Dictionary<int, string> _selectionFileNames = new Dictionary<int, string>();
		public const string EditBoxPanel = "gameSaveName";
		public const string SaveButton = "buttonSave";
		public const string DeleteButton = "buttonDelete";
		public const string CancelButton = "buttonCancel";
		public const string GameList = "gameList";
		public const string FileExtension = ".sots2save";
		private string _deleteFilename;
		private int _deleteIndex;
		private string _enteredText;
		private bool _choice;
		private string _reallyDeleteDialog;
		private string _fileExistsDialog;
		private bool isSave = false;

		public SaveGameDialog(App game, string defaultName, string template = "dialogSaveGame")
		  : base(game, template)
		{
			this._enteredText = defaultName ?? string.Empty;
			this.isSave = template == "dialogSaveGame";
		}

		protected virtual void OnSelectionCleared()
		{
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == "buttonSave")
				{
					this.Confirm();
					return;
				}
				if (panelName == "buttonCancel")
				{
					this._choice = false;
					this._app.UI.CloseDialog((Dialog)this, true);
					return;
				}
				if (panelName.StartsWith("buttonDelete"))
				{
					string[] strArray = panelName.Split('|');
					this._deleteIndex = int.Parse(strArray[1]);
					this._deleteFilename = strArray[2];
					this._reallyDeleteDialog = this._app.UI.CreateDialog((Dialog)new GenericQuestionDialog(this._app, "@DELETE_SAVED_GAME", "@DELETE_SAVED_GAME_TEXT", "dialogGenericQuestion"), null);
					return;
				}
			}
			else if (msgType == "dialog_closed")
			{
				if (panelName == this._reallyDeleteDialog)
				{
					if (bool.Parse(msgParams[0]))
					{
						this._app.UI.RemoveItems(this._app.UI.Path(this.ID, "gameList"), this._deleteIndex);
						File.Delete(this._deleteFilename);
						this._selectedIndex = -1;
						this.OnSelectionCleared();
						this._selectionFileNames.Remove(this._deleteIndex);
					}
				}
				else if (panelName == this._fileExistsDialog && bool.Parse(msgParams[0]))
					this.SaveGame();
			}
			else if (msgType == "edit_confirmed")
			{
				if (panelName == "gameSaveName")
				{
					this.Confirm();
					return;
				}
			}
			else if (msgType == "text_changed")
			{
				if (panelName == "gameSaveName")
				{
					this._enteredText = msgParams[0];
					return;
				}
			}
			else if (msgType == "list_sel_changed")
			{
				if (string.IsNullOrEmpty(msgParams[0]))
					return;
				this._selectedIndex = int.Parse(msgParams[0]);
				Path.GetFileNameWithoutExtension(this._selectionFileNames[this._selectedIndex]);
				this._app.UI.SetPropertyString("gameSaveName", "text", this._enteredText);
			}
			base.OnPanelMessage(panelName, msgType, msgParams);
		}

		public virtual void Confirm()
		{
			if (this._enteredText.Count<char>() < this._minChars)
				this._app.UI.CreateDialog((Dialog)new GenericTextDialog(this._app, App.Localize("@INVALID_NAME"), string.Format(App.Localize("@INVALID_NAME_TEXT"), (object)this._minChars), "dialogGenericMessage"), null);
			else if (File.Exists(Path.Combine(this._app.SaveDir, this._enteredText + ".sots2save")))
				this._fileExistsDialog = this._app.UI.CreateDialog((Dialog)new GenericQuestionDialog(this._app, App.Localize("@OVERWRITE_SAVED_GAME"), App.Localize("@OVERWRITE_SAVED_GAME_TEXT"), "dialogGenericQuestion"), null);
			else
				this.SaveGame();
		}

		public void SaveGame()
		{
			this._app.Game.Save(Path.Combine(this._app.SaveDir, this._enteredText + ".sots2save"));
			this._choice = true;
			this._app.UI.CloseDialog((Dialog)this, true);
			this._app.UI.CreateDialog((Dialog)new GenericTextDialog(this._app, App.Localize("@DIALOG_SUCCESS_HEADER"), App.Localize("@DIALOG_GAME_SAVED_SUCCESS"), "dialogGenericMessage"), null);
		}

		public override void Initialize()
		{
			base.Initialize();
			this._app.UI.Send((object)"SetMaxChars", (object)"gameSaveName", (object)this._maxChars);
			this._app.UI.Send((object)"SetFileMode", (object)"gameSaveName", (object)true);
			this._app.UI.SetPropertyString("gameSaveName", "text", this._enteredText);
			SavedGameFilename[] availableSavedGames = this._app.GetAvailableSavedGames(!isSave);
			int num = 0;
			foreach (SavedGameFilename savedGameFilename in availableSavedGames)
			{
				this._app.UI.AddItem(this._app.UI.Path(this.ID, "gameList"), "", num, "");
				string itemGlobalId1 = this._app.UI.GetItemGlobalID(this._app.UI.Path(this.ID, "gameList"), "", num, "");
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "saveName"), "text", Path.GetFileNameWithoutExtension(savedGameFilename.RootedFilename));
				SaveGameDialog.GameSummary gameSummary = new SaveGameDialog.GameSummary(savedGameFilename.RootedFilename, this._app.AssetDatabase);
				int length = gameSummary.Players.Length;
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "numPlayers"), "text", length.ToString() + " " + App.Localize("@PLAYERNAME"));
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "numTurns"), "text", App.Localize("@GENERAL_TURN") + " " + gameSummary.Turn.ToString());
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "mapName"), "text", App.Localize("@LOADGAME_MAP") + " " + gameSummary.MapName);
				this._app.UI.SetPropertyString(this._app.UI.Path(itemGlobalId1, "buttonDelete"), "id", "buttonDelete|" + num.ToString() + "|" + savedGameFilename.RootedFilename);
				this._selectionFileNames.Add(num, savedGameFilename.RootedFilename);
				int userItemId = 1;
				foreach (PlayerInfo player in gameSummary.Players)
				{
					string panelId = this._app.UI.Path(itemGlobalId1, "player" + userItemId.ToString() + "info");
					this._app.UI.SetVisible(panelId, true);
					this._app.UI.SetPropertyString(this._app.UI.Path(panelId, "playernum"), "text", "?");
					this._app.UI.SetPropertyString(this._app.UI.Path(panelId, "playeravatar"), "sprite", Path.GetFileNameWithoutExtension(player.AvatarAssetPath));
					this._app.UI.SetPropertyString(this._app.UI.Path(panelId, "badge"), "sprite", Path.GetFileNameWithoutExtension(player.BadgeAssetPath));
					this._app.UI.SetVisible(this._app.UI.Path(panelId, "eliminatedState"), (player.isDefeated ? 1 : 0) != 0);
					this._app.UI.SetPropertyColorNormalized(this._app.UI.Path(panelId, "primaryColor"), "color", player.PrimaryColor);
					this._app.UI.SetPropertyColorNormalized(this._app.UI.Path(panelId, "secondaryColor"), "color", player.SecondaryColor);
					this._app.UI.AddItem(this._app.UI.Path(itemGlobalId1, "playerList"), "", userItemId, "?- " + player.Name);
					string itemGlobalId2 = this._app.UI.GetItemGlobalID(this._app.UI.Path(itemGlobalId1, "playerList"), "", userItemId, "?- " + player.Name);
					this._app.UI.SetPropertyColorNormalized(this._app.UI.Path(itemGlobalId1, "playerList", itemGlobalId2), "color", player.PrimaryColor);
					++userItemId;
				}
				++num;
			}
		}

		public override string[] CloseDialog()
		{
			return new string[2]
			{
		this._choice.ToString(),
		this._enteredText
			};
		}

		public class GameSummary
		{
			public readonly PlayerInfo[] Players;
			public readonly int Turn;
			public readonly string MapName;

			public GameSummary(string filename, AssetDatabase assetdb)
			{
				this.Players = new PlayerInfo[0];
				this.Turn = 0;
				this.MapName = string.Empty;
				try
				{
					using (GameDatabase gameDatabase = GameDatabase.Connect(filename, assetdb))
					{
						try
						{
							this.Players = gameDatabase.GetPlayerInfos().Where<PlayerInfo>((Func<PlayerInfo, bool>)(x => x.isStandardPlayer)).ToArray<PlayerInfo>();
						}
						catch (Exception ex)
						{
						}
						try
						{
							this.Turn = gameDatabase.GetTurnCount();
						}
						catch (Exception ex)
						{
						}
						try
						{
							this.MapName = gameDatabase.GetMapName();
						}
						catch (Exception ex)
						{
						}
					}
				}
				catch (Exception ex)
				{
				}
			}
		}
	}
}
