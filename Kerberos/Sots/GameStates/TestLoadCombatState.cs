// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.TestLoadCombatState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Strategy;
using System;
using System.IO;
using System.Linq;
using System.Xml;

namespace Kerberos.Sots.GameStates
{
	internal class TestLoadCombatState : GameState
	{
		private string[] _availableConfigFiles;
		private string _selectedConfigFile;

		public TestLoadCombatState(App game)
		  : base(game)
		{
		}

		protected override void OnPrepare(GameState prev, object[] parms)
		{
			this._availableConfigFiles = Directory.EnumerateFiles(Path.Combine(this.App.GameRoot, "data"), "*.xml", SearchOption.TopDirectoryOnly).ToArray<string>();
			this._selectedConfigFile = null;
			this.App.UI.LoadScreen("TestLoadCombat");
		}

		protected override void OnEnter()
		{
			if (this.App.LocalPlayer == null)
				this.App.NewGame();
			this.App.UI.SetScreen("TestLoadCombat");
			this.App.UI.ClearItems("combatConfigList");
			for (int userItemId = 0; userItemId < this._availableConfigFiles.Length; ++userItemId)
				this.App.UI.AddItem("combatConfigList", string.Empty, userItemId, Path.GetFileNameWithoutExtension(this._availableConfigFiles[userItemId]));
			this.App.UI.SetEnabled("gameNextButton", false);
		}

		protected override void OnExit(GameState prev, ExitReason reason)
		{
			this._availableConfigFiles = (string[])null;
			this._selectedConfigFile = null;
		}

		protected override void UICommChannel_OnPanelMessage(
		  string panelName,
		  string msgType,
		  string[] msgParams)
		{
			if (msgType == "list_sel_changed")
			{
				if (!(panelName == "combatConfigList"))
					return;
				if (!string.IsNullOrEmpty(msgParams[0]))
				{
					if (this._selectedConfigFile == null)
						this.App.UI.SetEnabled("gameNextButton", true);
					this._selectedConfigFile = this._availableConfigFiles[int.Parse(msgParams[0])];
				}
				else
				{
					if (this._selectedConfigFile != null)
						this.App.UI.SetEnabled("gameNextButton", false);
					this._selectedConfigFile = null;
				}
			}
			else
			{
				if (!(msgType == "button_clicked"))
					return;
				if (panelName == "gameExitButton")
				{
					this.GoBack();
				}
				else
				{
					if (!(panelName == "gameNextButton"))
						return;
					this.GoNext();
				}
			}
		}

		private void GoBack()
		{
			this.App.SwitchGameState("MainMenuState");
		}

		private void GoNext()
		{
			try
			{
				string selectedConfigFile = this._selectedConfigFile;
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(selectedConfigFile);
				this.App.SwitchGameState<CombatState>((object)new PendingCombat(), (object)xmlDocument, (object)true);
			}
			catch (Exception ex)
			{
				this.App.UI.SetEnabled("gameNextButton", false);
				throw;
			}
		}

		protected override void OnUpdate()
		{
		}

		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
	}
}
