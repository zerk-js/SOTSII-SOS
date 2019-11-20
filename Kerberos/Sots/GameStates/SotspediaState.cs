// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.SotspediaState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;

namespace Kerberos.Sots.GameStates
{
	internal class SotspediaState : GameState, IKeyBindListener
	{
		private static readonly string UIHumanTOC = "gameHumanTOC";
		private static readonly string UIHiverTOC = "gameHiverTOC";
		private static readonly string UITarkasTOC = "gameTarkasTOC";
		private static readonly string UILiirTOC = "gameLiirTOC";
		private static readonly string UIMorrigiTOC = "gameMorrigiTOC";
		private static readonly string UIZuulTOC = "gameZuulTOC";
		private static readonly string UITechTOC = "gameTechTOC";
		private static readonly string UIHumanButton = "gameHumanButton";
		private static readonly string UIHiverButton = "gameHiverButton";
		private static readonly string UITarkasButton = "gameTarkasButton";
		private static readonly string UILiirButton = "gameLiirButton";
		private static readonly string UIMorrigiButton = "gameMorrigiButton";
		private static readonly string UIZuulButton = "gameZuulButton";
		private static readonly string UITechButton = "gameTechButton";
		private static readonly string UIHumanContent = "gameHumanContent";
		private static readonly string UIHiverContent = "gameHiverContent";
		private static readonly string UITarkasContent = "gameTarkasContent";
		private static readonly string UILiirContent = "gameLiirContent";
		private static readonly string UIMorrigiContent = "gameMorrigiContent";
		private static readonly string UIZuulContent = "gameZuulContent";
		private static readonly string UITechContent = "gameTechContent";
		private static readonly string UIBackButton = "gameExitButton";
		private GameObjectSet _crits;
		private bool _searchMode;
		private string _initialPage;
		private string _currentCategory;

		public static void NavigateToLink(App app, string name)
		{
			if (app.CurrentState == app.GetGameState<SotspediaState>())
				app.GetGameState<SotspediaState>().NavigateToLink(name);
			else
				app.SwitchGameState((GameState)app.GetGameState<SotspediaState>(), (object)name);
		}

		public SotspediaState(App game)
		  : base(game)
		{
		}

		protected override void OnPrepare(GameState prev, object[] parms)
		{
			this._crits = new GameObjectSet(this.App);
			this.App.UI.LoadScreen("Sotspedia");
			this._initialPage = null;
			if (parms == null || parms.Length <= 0)
				return;
			this._initialPage = parms[0] as string;
		}

		private void UICommChannel_GameEvent(string eventName, string[] eventParams)
		{
			switch (eventName)
			{
				case "LinkClicked":
					this.ProcessGameEvent_LinkClicked(eventParams);
					break;
				case "SearchMode":
					if (eventParams[0] == "true")
					{
						this._searchMode = true;
						break;
					}
					this._searchMode = false;
					break;
			}
		}

		private string InferCategory(string pageFile)
		{
			if (pageFile.Contains("Hiver"))
				return "Hiver";
			if (pageFile.Contains("Human"))
				return "Human";
			if (pageFile.Contains("Tarka"))
				return "Tarkas";
			if (pageFile.Contains("Zuul"))
				return "Zuul";
			if (pageFile.Contains("Liir"))
				return "Liir";
			if (pageFile.Contains("Morrigi"))
				return "Morrigi";
			if (pageFile.Contains("Techs"))
				return "Tech";
			return this._currentCategory;
		}

		private void NavigateToLink(string name)
		{
			if (!this._searchMode)
				this.SetCurrentCategory(this.InferCategory(name));
			this.App.UI.SetPropertyString(this.UIArticlePage, "link", name);
		}

		private void ProcessGameEvent_LinkClicked(string[] eventParams)
		{
			this.NavigateToLink(eventParams[0]);
		}

		private string UITableOfContents
		{
			get
			{
				return string.Format("game{0}TOC", (object)this._currentCategory);
			}
		}

		private string UIArticlePage
		{
			get
			{
				return string.Format("game{0}Page", (object)this._currentCategory);
			}
		}

		private void SetCurrentCategory(string category)
		{
			this._currentCategory = category;
			this.App.UI.SetVisible(SotspediaState.UIHumanContent, this._currentCategory == "Human");
			this.App.UI.SetVisible(SotspediaState.UIHiverContent, this._currentCategory == "Hiver");
			this.App.UI.SetVisible(SotspediaState.UITarkasContent, this._currentCategory == "Tarkas");
			this.App.UI.SetVisible(SotspediaState.UILiirContent, this._currentCategory == "Liir");
			this.App.UI.SetVisible(SotspediaState.UIMorrigiContent, this._currentCategory == "Morrigi");
			this.App.UI.SetVisible(SotspediaState.UIZuulContent, this._currentCategory == "Zuul");
			this.App.UI.SetVisible(SotspediaState.UITechContent, this._currentCategory == "Tech");
		}

		private string GetPanelFromCurrentCategory()
		{
			switch (this._currentCategory)
			{
				case "Human":
					return SotspediaState.UIHumanTOC;
				case "Hiver":
					return SotspediaState.UIHiverTOC;
				case "Tarkas":
					return SotspediaState.UITarkasTOC;
				case "Liir":
					return SotspediaState.UILiirTOC;
				case "Morrigi":
					return SotspediaState.UIMorrigiTOC;
				case "Zuul":
					return SotspediaState.UIZuulTOC;
				case "Tech":
					return SotspediaState.UITechTOC;
				default:
					return "";
			}
		}

		protected override void UICommChannel_OnPanelMessage(
		  string panelName,
		  string msgType,
		  string[] msgParams)
		{
			if (msgType == "button_clicked")
			{
				if (panelName == SotspediaState.UIHumanButton)
					this.SetCurrentCategory("Human");
				else if (panelName == SotspediaState.UIHiverButton)
					this.SetCurrentCategory("Hiver");
				else if (panelName == SotspediaState.UITarkasButton)
					this.SetCurrentCategory("Tarkas");
				else if (panelName == SotspediaState.UILiirButton)
					this.SetCurrentCategory("Liir");
				else if (panelName == SotspediaState.UIMorrigiButton)
					this.SetCurrentCategory("Morrigi");
				else if (panelName == SotspediaState.UIZuulButton)
					this.SetCurrentCategory("Zuul");
				else if (panelName == SotspediaState.UITechButton)
				{
					this.SetCurrentCategory("Tech");
				}
				else
				{
					if (!(panelName == SotspediaState.UIBackButton))
						return;
					if (this.App.PreviousState != null && (this.App.PreviousState is StarMapState || this.App.PreviousState is ResearchScreenState))
						this.App.SwitchGameState(this.App.PreviousState);
					else if (this.App.PreviousState != null && this.App.PreviousState is DesignScreenState)
						this.App.SwitchGameState<StarMapState>();
					else
						this.App.SwitchGameStateViaLoadingScreen((Action)null, (LoadingFinishedDelegate)null, this.App.PreviousState ?? (GameState)this.App.GetGameState<MainMenuState>());
				}
			}
			else
			{
				if (!(msgType == "text_changed") || !panelName.StartsWith("searchBox"))
					return;
				this.App.UI.Send((object)"UpdateSearchTerms", (object)this.GetPanelFromCurrentCategory(), (object)msgParams[0]);
			}
		}

		protected override void OnEnter()
		{
			this.App.UI.SetScreen("Sotspedia");
			this.App.UI.GameEvent += new UIEventGameEvent(this.UICommChannel_GameEvent);
			this.App.UI.Send((object)"SetCategory", (object)SotspediaState.UIHumanTOC, (object)"Human");
			this.App.UI.Send((object)"SetCategory", (object)SotspediaState.UIHiverTOC, (object)"Hiver");
			this.App.UI.Send((object)"SetCategory", (object)SotspediaState.UITarkasTOC, (object)"Tarkas");
			this.App.UI.Send((object)"SetCategory", (object)SotspediaState.UILiirTOC, (object)"Liir");
			this.App.UI.Send((object)"SetCategory", (object)SotspediaState.UIMorrigiTOC, (object)"Morrigi");
			this.App.UI.Send((object)"SetCategory", (object)SotspediaState.UIZuulTOC, (object)"Zuul");
			this.App.UI.Send((object)"SetCategory", (object)SotspediaState.UITechTOC, (object)"Tech");
			this.SetCurrentCategory("Tarkas");
			List<object> objectList = new List<object>();
			List<string> researchedTechs = this.App.UserProfile.ResearchedTechs;
			objectList.Add((object)"SotspediaSetUnlockKeys");
			objectList.Add((object)SotspediaState.UIHumanTOC);
			objectList.Add((object)researchedTechs.Count);
			foreach (string str in researchedTechs)
				objectList.Add((object)str);
			this.App.UI.Send(objectList.ToArray());
			if (this._initialPage != null)
				this.NavigateToLink(this._initialPage);
			this.App.HotKeyManager.AddListener((IKeyBindListener)this);
		}

		protected override void OnExit(GameState prev, ExitReason reason)
		{
			this.App.HotKeyManager.RemoveListener((IKeyBindListener)this);
		}

		protected override void OnUpdate()
		{
		}

		public override bool IsReady()
		{
			return this._crits != null && this._crits.IsReady() && base.IsReady();
		}

		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}

		public bool OnKeyBindPressed(HotKeyManager.HotKeyActions action, string gamestates)
		{
			if (gamestates.Contains(this.Name))
			{
				switch (action)
				{
					case HotKeyManager.HotKeyActions.State_Starmap:
						this.App.UI.LockUI();
						this.App.SwitchGameState<StarMapState>();
						return true;
					case HotKeyManager.HotKeyActions.State_BuildScreen:
						return false;
					case HotKeyManager.HotKeyActions.State_DesignScreen:
						this.App.UI.LockUI();
						this.App.SwitchGameState<DesignScreenState>((object)false, (object)this.Name);
						return true;
					case HotKeyManager.HotKeyActions.State_ResearchScreen:
						this.App.UI.LockUI();
						this.App.SwitchGameState<ResearchScreenState>();
						return true;
					case HotKeyManager.HotKeyActions.State_ComparativeAnalysysScreen:
						return false;
					case HotKeyManager.HotKeyActions.State_EmpireSummaryScreen:
						this.App.UI.LockUI();
						this.App.SwitchGameState<EmpireSummaryState>();
						return true;
					case HotKeyManager.HotKeyActions.State_SotspediaScreen:
						return false;
					case HotKeyManager.HotKeyActions.State_DiplomacyScreen:
						this.App.UI.LockUI();
						this.App.SwitchGameState<DiplomacyScreenState>();
						return true;
				}
			}
			return false;
		}
	}
}
