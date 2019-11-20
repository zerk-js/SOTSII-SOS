// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.PlanetManagerDialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.ModuleFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class PlanetManagerDialog : Dialog
	{
		private static readonly string UIExitButton = "okButton";
		private Dictionary<ModuleEnums.StationModuleType, int> _queuedItemMap = new Dictionary<ModuleEnums.StationModuleType, int>();
		private List<SystemWidget> _systemWidgets = new List<SystemWidget>();
		private List<PlanetWidget> _planetWidgets;
		private App App;
		private StarSystemInfo _selectedSystem;
		private PlanetManagerDialog.PlanetFilterMode _currentFilterMode;
		private PlanetManagerDialog.PlanetOrderMode _currentOrderMode;

		public PlanetManagerDialog(App game, string template = "dialogPlanetManager")
		  : base(game, template)
		{
			this.App = game;
		}

		public override void Initialize()
		{
			this.App.UI.UnlockUI();
			this.App.UI.AddItem("filterDropdown", "", 0, App.Localize("@UI_PLANET_MANAGER_ALL_PLANETS"));
			this.App.UI.AddItem("filterDropdown", "", 1, App.Localize("@UI_PLANET_MANAGER_SURVEYED_PLANETS"));
			this.App.UI.AddItem("filterDropdown", "", 2, App.Localize("@UI_PLANET_MANAGER_OWNED_PLANETS"));
			this.App.UI.AddItem("filterDropdown", "", 3, App.Localize("@UI_PLANET_MANAGER_ENEMY_PLANETS"));
			this.App.UI.SetSelection("filterDropdown", 0);
			this._currentFilterMode = PlanetManagerDialog.PlanetFilterMode.AllPlanets;
			this.App.UI.AddItem("orderDropdown", "", 0, App.Localize("@UI_PLANET_MANAGER_ORDERBY_POS"));
			this.App.UI.AddItem("orderDropdown", "", 1, App.Localize("@UI_PLANET_MANAGER_ORDERBY_HAZARD"));
			this.App.UI.AddItem("orderDropdown", "", 2, App.Localize("@UI_PLANET_MANAGER_ORDERBY_SIZE"));
			this.App.UI.AddItem("orderDropdown", "", 3, App.Localize("@UI_PLANET_MANAGER_ORDERBY_RESOURCES"));
			this.App.UI.AddItem("orderDropdown", "", 4, App.Localize("@UI_PLANET_MANAGER_ORDERBY_BIOSPHERE"));
			this.App.UI.AddItem("orderDropdown", "", 5, App.Localize("@UI_PLANET_MANAGER_ORDERBY_DEVCOST"));
			this.App.UI.AddItem("orderDropdown", "", 6, App.Localize("@UI_PLANET_MANAGER_ORDERBY_INFRA"));
			this.App.UI.SetSelection("orderDropdown", 0);
			this._currentOrderMode = PlanetManagerDialog.PlanetOrderMode.SystemOrder;
			this.App.UI.SetListCleanClear("system_list", true);
			EmpireBarUI.SyncTitleFrame(this.App);
			this._planetWidgets = new List<PlanetWidget>();
			this.SyncPlanetList();
		}

		protected override void OnUpdate()
		{
			foreach (PlanetWidget planetWidget in this._planetWidgets)
				planetWidget.Update();
			foreach (SystemWidget systemWidget in this._systemWidgets)
				systemWidget.Update();
		}

		protected void SetSyncedSystem(StarSystemInfo system)
		{
			this._selectedSystem = system;
			if (this._app.CurrentState.Name == "StarMapState")
			{
				StarMapState currentState = (StarMapState)this._app.CurrentState;
				currentState.StarMap.SetFocus((IGameObject)currentState.StarMap.Systems.Reverse[system.ID]);
				currentState.StarMap.Select((IGameObject)currentState.StarMap.Systems.Reverse[system.ID]);
			}
			this.App.UI.ClearItems("system_list");
			foreach (SystemWidget systemWidget in this._systemWidgets)
				systemWidget.Terminate();
			this._systemWidgets.Clear();
			foreach (PlanetWidget planetWidget in this._planetWidgets)
				planetWidget.Terminate();
			this._planetWidgets.Clear();
			this.App.UI.ClearItems("system_list");
			List<PlanetInfo> planetInfoList = this.FilteredPlanetList(system);
			this.App.UI.AddItem("system_list", "", system.ID, "", "systemTitleCard");
			this._systemWidgets.Add(new SystemWidget(this.App, this.App.UI.GetItemGlobalID("system_list", "", system.ID, "")));
			this._systemWidgets.Last<SystemWidget>().Sync(system.ID);
			foreach (PlanetInfo planetInfo in planetInfoList)
			{
				if (this.App.AssetDatabase.IsPotentialyHabitable(planetInfo.Type))
				{
					this.App.UI.AddItem("system_list", "", planetInfo.ID + 999999, "", "planetDetailsM_Card");
					string itemGlobalId = this.App.UI.GetItemGlobalID("system_list", "", planetInfo.ID + 999999, "");
					this._planetWidgets.Add(new PlanetWidget(this.App, itemGlobalId));
					this._planetWidgets.Last<PlanetWidget>().Sync(planetInfo.ID, false, true);
					this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "MoraleRow"), "id", "MoraleRow|" + (object)planetInfo.ID);
				}
				else if (this.App.AssetDatabase.IsGasGiant(planetInfo.Type))
				{
					this.App.UI.AddItem("system_list", "", planetInfo.ID + 999999, "", "gasgiantDetailsM_Card");
					string itemGlobalId = this.App.UI.GetItemGlobalID("system_list", "", planetInfo.ID + 999999, "");
					this._planetWidgets.Add(new PlanetWidget(this.App, itemGlobalId));
					this._planetWidgets.Last<PlanetWidget>().Sync(planetInfo.ID, false, false);
					this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "MoraleRow"), "id", "MoraleRow|" + (object)planetInfo.ID);
				}
				else if (this.App.AssetDatabase.IsMoon(planetInfo.Type))
				{
					this.App.UI.AddItem("system_list", "", planetInfo.ID + 999999, "", "moonDetailsM_Card");
					string itemGlobalId = this.App.UI.GetItemGlobalID("system_list", "", planetInfo.ID + 999999, "");
					this._planetWidgets.Add(new PlanetWidget(this.App, itemGlobalId));
					this._planetWidgets.Last<PlanetWidget>().Sync(planetInfo.ID, false, false);
					this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "MoraleRow"), "id", "MoraleRow|" + (object)planetInfo.ID);
				}
			}
		}

		private List<PlanetInfo> FilteredPlanetList(StarSystemInfo system)
		{
			List<PlanetInfo> list = ((IEnumerable<PlanetInfo>)this.App.GameDatabase.GetStarSystemPlanetInfos(system.ID)).ToList<PlanetInfo>();
			List<PlanetInfo> source = new List<PlanetInfo>();
			foreach (PlanetInfo planetInfo in list)
			{
				if (this.App.GameDatabase.IsSurveyed(this.App.LocalPlayer.ID, system.ID))
				{
					if (this._currentFilterMode == PlanetManagerDialog.PlanetFilterMode.AllPlanets)
						source.Add(planetInfo);
					else if (this._currentFilterMode == PlanetManagerDialog.PlanetFilterMode.SurveyedPlanets)
					{
						if (this.App.GameDatabase.GetColonyIntelForPlanet(this.App.LocalPlayer.ID, planetInfo.ID) == null)
							source.Add(planetInfo);
					}
					else if (this._currentFilterMode == PlanetManagerDialog.PlanetFilterMode.OwnedPlanets)
					{
						AIColonyIntel colonyIntelForPlanet = this.App.GameDatabase.GetColonyIntelForPlanet(this.App.LocalPlayer.ID, planetInfo.ID);
						if (colonyIntelForPlanet != null && colonyIntelForPlanet.OwningPlayerID == this.App.LocalPlayer.ID)
							source.Add(planetInfo);
					}
					else if (this._currentFilterMode == PlanetManagerDialog.PlanetFilterMode.EnemyPlanets)
					{
						AIColonyIntel colonyIntelForPlanet = this.App.GameDatabase.GetColonyIntelForPlanet(this.App.LocalPlayer.ID, planetInfo.ID);
						if (colonyIntelForPlanet != null && colonyIntelForPlanet.OwningPlayerID != this.App.LocalPlayer.ID)
							source.Add(planetInfo);
					}
				}
			}
			if (source.Any<PlanetInfo>())
			{
				if (this._currentOrderMode == PlanetManagerDialog.PlanetOrderMode.PlanetHazard)
				{
					PlanetManagerDialog.sortPlanetHazard sortPlanetHazard = new PlanetManagerDialog.sortPlanetHazard(this._app);
					source.Sort((IComparer<PlanetInfo>)sortPlanetHazard);
				}
				else if (this._currentOrderMode == PlanetManagerDialog.PlanetOrderMode.PlanetDevCost)
				{
					PlanetManagerDialog.sortPlanetDevCost sortPlanetDevCost = new PlanetManagerDialog.sortPlanetDevCost(this._app);
					source.Sort((IComparer<PlanetInfo>)sortPlanetDevCost);
				}
				else if (this._currentOrderMode == PlanetManagerDialog.PlanetOrderMode.PlanetSize)
				{
					PlanetManagerDialog.sortPlanetSize sortPlanetSize = new PlanetManagerDialog.sortPlanetSize(this._app);
					source.Sort((IComparer<PlanetInfo>)sortPlanetSize);
				}
				else if (this._currentOrderMode == PlanetManagerDialog.PlanetOrderMode.PlanetResources)
				{
					PlanetManagerDialog.sortPlanetResources sortPlanetResources = new PlanetManagerDialog.sortPlanetResources(this._app);
					source.Sort((IComparer<PlanetInfo>)sortPlanetResources);
				}
				else if (this._currentOrderMode == PlanetManagerDialog.PlanetOrderMode.PlanetBioSphere)
				{
					PlanetManagerDialog.sortPlanetBiosphere sortPlanetBiosphere = new PlanetManagerDialog.sortPlanetBiosphere(this._app);
					source.Sort((IComparer<PlanetInfo>)sortPlanetBiosphere);
				}
				else if (this._currentOrderMode == PlanetManagerDialog.PlanetOrderMode.PlanetInfra)
				{
					PlanetManagerDialog.sortPlanetInfra sortPlanetInfra = new PlanetManagerDialog.sortPlanetInfra(this._app);
					source.Sort((IComparer<PlanetInfo>)sortPlanetInfra);
				}
			}
			return source;
		}

		protected void SyncPlanetList()
		{
			this.App.UI.ClearItems("sys_list_left");
			List<StarSystemInfo> list = this.App.GameDatabase.GetStarSystemInfos().ToList<StarSystemInfo>();
			list.Sort((Comparison<StarSystemInfo>)((x, y) => string.Compare(x.Name, y.Name)));
			bool flag = true;
			foreach (StarSystemInfo system in list)
			{
				if (this.FilteredPlanetList(system).Count > 0)
				{
					this.App.UI.AddItem("sys_list_left", "", system.ID, system.Name);
					if (flag)
					{
						this.SetSyncedSystem(system);
						flag = false;
					}
				}
			}
		}

		protected override void OnPanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (msgType == "list_sel_changed")
			{
				if (panelName == "filterDropdown")
				{
					this._currentFilterMode = (PlanetManagerDialog.PlanetFilterMode)int.Parse(msgParams[0]);
					this.SyncPlanetList();
				}
				else if (panelName == "orderDropdown")
				{
					this._currentOrderMode = (PlanetManagerDialog.PlanetOrderMode)int.Parse(msgParams[0]);
					if (this._selectedSystem != (StarSystemInfo)null)
						this.SetSyncedSystem(this._selectedSystem);
				}
				else if (panelName == "sys_list_left")
				{
					StarSystemInfo starSystemInfo = this.App.GameDatabase.GetStarSystemInfo(int.Parse(msgParams[0]));
					if (starSystemInfo != (StarSystemInfo)null)
						this.SetSyncedSystem(starSystemInfo);
				}
			}
			if (msgType == "button_clicked")
			{
				if (panelName == PlanetManagerDialog.UIExitButton)
					this.App.UI.CloseDialog((Dialog)this, true);
				else if (panelName.StartsWith("btnColoninzePlanet"))
				{
					string[] strArray = panelName.Split('|');
					int targetSystem = int.Parse(strArray[1]);
					int targetPlanet = int.Parse(strArray[2]);
					if (this.App.CurrentState.GetType() == typeof(StarMapState))
						((StarMapState)this.App.CurrentState).ShowColonizePlanetOverlay(targetSystem, targetPlanet);
					this.App.UI.CloseDialog((Dialog)this, true);
				}
			}
			if (!(msgType == "mouse_enter") || !panelName.StartsWith("MoraleRow"))
				return;
			int orbitalObjectID = int.Parse(panelName.Split('|')[0]);
			int x = int.Parse(msgParams[0]);
			int y = int.Parse(msgParams[1]);
			ColonyInfo colonyInfoForPlanet = this.App.GameDatabase.GetColonyInfoForPlanet(orbitalObjectID);
			if (colonyInfoForPlanet == null || this.App.LocalPlayer.ID != colonyInfoForPlanet.PlayerID)
				return;
			StarSystemUI.ShowMoraleEventToolTip(this.App.Game, colonyInfoForPlanet.ID, x, y);
		}

		public override string[] CloseDialog()
		{
			foreach (PlanetWidget planetWidget in this._planetWidgets)
				planetWidget.Terminate();
			foreach (SystemWidget systemWidget in this._systemWidgets)
				systemWidget.Terminate();
			return (string[])null;
		}

		private enum PlanetFilterMode
		{
			AllPlanets,
			SurveyedPlanets,
			OwnedPlanets,
			EnemyPlanets,
		}

		private enum PlanetOrderMode
		{
			SystemOrder,
			PlanetHazard,
			PlanetSize,
			PlanetResources,
			PlanetBioSphere,
			PlanetDevCost,
			PlanetInfra,
		}

		public class sortPlanetHazard : IComparer<PlanetInfo>
		{
			private App _App;

			public sortPlanetHazard(App _app)
			{
				this._App = _app;
			}

			public int Compare(PlanetInfo a, PlanetInfo b)
			{
				float planetHazardRating1 = this._App.GameDatabase.GetPlanetHazardRating(this._App.LocalPlayer.ID, a.ID, false);
				float planetHazardRating2 = this._App.GameDatabase.GetPlanetHazardRating(this._App.LocalPlayer.ID, b.ID, false);
				if ((double)planetHazardRating1 > (double)planetHazardRating2)
					return 1;
				return (double)planetHazardRating1 < (double)planetHazardRating2 ? -1 : 0;
			}
		}

		public class sortPlanetDevCost : IComparer<PlanetInfo>
		{
			private App _App;

			public sortPlanetDevCost(App _app)
			{
				this._App = _app;
			}

			public int Compare(PlanetInfo a, PlanetInfo b)
			{
				double num1 = Colony.EstimateColonyDevelopmentCost(this._App.Game, a.ID, this._App.LocalPlayer.ID);
				double num2 = Colony.EstimateColonyDevelopmentCost(this._App.Game, b.ID, this._App.LocalPlayer.ID);
				if (num1 > num2)
					return 1;
				return num1 < num2 ? -1 : 0;
			}
		}

		public class sortPlanetSize : IComparer<PlanetInfo>
		{
			private App _App;

			public sortPlanetSize(App _app)
			{
				this._App = _app;
			}

			public int Compare(PlanetInfo a, PlanetInfo b)
			{
				float size1 = a.Size;
				float size2 = b.Size;
				if ((double)size1 < (double)size2)
					return 1;
				return (double)size1 > (double)size2 ? -1 : 0;
			}
		}

		public class sortPlanetResources : IComparer<PlanetInfo>
		{
			private App _App;

			public sortPlanetResources(App _app)
			{
				this._App = _app;
			}

			public int Compare(PlanetInfo a, PlanetInfo b)
			{
				float resources1 = (float)a.Resources;
				float resources2 = (float)b.Resources;
				if ((double)resources1 < (double)resources2)
					return 1;
				return (double)resources1 > (double)resources2 ? -1 : 0;
			}
		}

		public class sortPlanetBiosphere : IComparer<PlanetInfo>
		{
			private App _App;

			public sortPlanetBiosphere(App _app)
			{
				this._App = _app;
			}

			public int Compare(PlanetInfo a, PlanetInfo b)
			{
				float biosphere1 = (float)a.Biosphere;
				float biosphere2 = (float)b.Biosphere;
				if ((double)biosphere1 < (double)biosphere2)
					return 1;
				return (double)biosphere1 > (double)biosphere2 ? -1 : 0;
			}
		}

		public class sortPlanetInfra : IComparer<PlanetInfo>
		{
			private App _App;

			public sortPlanetInfra(App _app)
			{
				this._App = _app;
			}

			public int Compare(PlanetInfo a, PlanetInfo b)
			{
				float infrastructure1 = a.Infrastructure;
				float infrastructure2 = b.Infrastructure;
				if ((double)infrastructure1 < (double)infrastructure2)
					return 1;
				return (double)infrastructure1 > (double)infrastructure2 ? -1 : 0;
			}
		}
	}
}
