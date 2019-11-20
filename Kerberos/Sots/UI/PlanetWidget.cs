// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.PlanetWidget
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class PlanetWidget
	{
		private string _rootPanel = "";
		public const string UITradeSlider = "partTradeSlider";
		public const string UITerraSlider = "partTerraSlider";
		public const string UIInfraSlider = "partInfraSlider";
		public const string UIOverDevSlider = "partOverDevelopment";
		public const string UIShipConSlider = "partShipConSlider";
		public const string UIOverharvestSlider = "partOverharvestSlider";
		public const string UICivPopulationSlider = "partCivSlider";
		public const string UISlaveWorkSlider = "partWorkRate";
		private int _planetID;
		public App App;
		private StellarBody _cachedPlanet;
		private PlanetInfo _cachedPlanetInfo;
		private bool _cachedPlanetReady;
		private PlanetView _planetView;
		private GameObjectSet _crits;
		private bool _initialized;
		private bool _planetViewLinked;
		private static int kWidgetID;
		private int _widgetID;

		public PlanetWidget(App app, string rootPanel)
		{
			this._rootPanel = rootPanel;
			this.App = app;
			this._crits = new GameObjectSet(this.App);
			this._planetView = this._crits.Add<PlanetView>();
			this.App.UI.PanelMessage += new UIEventPanelMessage(this.UICommChannel_OnPanelMessage);
			++PlanetWidget.kWidgetID;
			this._widgetID = PlanetWidget.kWidgetID;
		}

		public int GetPlanetID()
		{
			return this._planetID;
		}

		public void Sync(int planetID, bool PopulationSliders = false, bool ShowColonizebuttons = false)
		{
			StarSystemUI.ClearColonyDetailsControl(this.App.Game, this._rootPanel);
			PlanetInfo planetInfo = this.App.GameDatabase.GetPlanetInfo(planetID);
			if (planetInfo == null)
				return;
			this._planetID = planetID;
			this.CachePlanet(planetInfo);
			StarSystemUI.SyncPlanetDetailsControlNew(this.App.Game, this._rootPanel, planetID);
			this.App.UI.SetPropertyString(this.App.UI.Path(this._rootPanel, "systemName"), "text", this.App.GameDatabase.GetStarSystemInfo(this.App.GameDatabase.GetOrbitalObjectInfo(planetInfo.ID).StarSystemID).Name);
			ColonyInfo colonyInfoForPlanet = this.App.GameDatabase.GetColonyInfoForPlanet(planetID);
			this.App.UI.SetVisible(this.App.UI.Path(this._rootPanel, "rebellionActive"), (colonyInfoForPlanet != null ? (colonyInfoForPlanet.PlayerID != this.App.LocalPlayer.ID ? 0 : (colonyInfoForPlanet.RebellionType != RebellionType.None ? 1 : 0)) : 0) != 0);
			int num1 = 0;
			if (colonyInfoForPlanet != null && colonyInfoForPlanet.PlayerID == this.App.LocalPlayer.ID)
			{
				this.App.UI.SetPropertyString(this.App.UI.Path(this._rootPanel, "partOverharvestSlider"), "id", "__partOverharvestSlider|" + this._widgetID.ToString() + "|" + colonyInfoForPlanet.ID.ToString());
				this.App.UI.SetPropertyString(this.App.UI.Path(this._rootPanel, "partTradeSlider"), "id", "__partTradeSlider|" + this._widgetID.ToString() + "|" + colonyInfoForPlanet.ID.ToString());
				this.App.UI.SetPropertyString(this.App.UI.Path(this._rootPanel, "partTerraSlider"), "id", "__partTerraSlider|" + this._widgetID.ToString() + "|" + colonyInfoForPlanet.ID.ToString());
				this.App.UI.SetPropertyString(this.App.UI.Path(this._rootPanel, "partInfraSlider"), "id", "__partInfraSlider|" + this._widgetID.ToString() + "|" + colonyInfoForPlanet.ID.ToString());
				this.App.UI.SetPropertyString(this.App.UI.Path(this._rootPanel, "partShipConSlider"), "id", "__partShipConSlider|" + this._widgetID.ToString() + "|" + colonyInfoForPlanet.ID.ToString());
				this.App.UI.SetPropertyString(this.App.UI.Path(this._rootPanel, "partCivSlider"), "id", "__partCivSlider|" + this._widgetID.ToString() + "|" + colonyInfoForPlanet.ID.ToString());
				StarSystemUI.SyncColonyDetailsControlNew(this.App.Game, this._rootPanel, colonyInfoForPlanet.ID, this._widgetID, "");
				this.App.UI.SetPropertyBool(this._rootPanel, "expanded", true);
				if (!PopulationSliders)
					return;
				foreach (ColonyFactionInfo faction1 in colonyInfoForPlanet.Factions)
				{
					this.App.UI.AddItem(this.App.UI.Path(this._rootPanel, "MoraleRow"), "", faction1.FactionID, "", "popItem");
					string itemGlobalId = this.App.UI.GetItemGlobalID(this.App.UI.Path(this._rootPanel, "MoraleRow"), "", faction1.FactionID, "");
					this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "partPopSlider"), "id", "__partPopSlider|" + this._widgetID.ToString() + "|" + colonyInfoForPlanet.ID.ToString() + "|" + (object)faction1.FactionID);
					Faction faction2 = this.App.AssetDatabase.GetFaction(faction1.FactionID);
					this.App.UI.SetPropertyString(this.App.UI.Path(itemGlobalId, "factionicon"), "sprite", "logo_" + faction2.Name.ToLower());
					this.App.UI.SetSliderValue(this.App.UI.Path(itemGlobalId, "__partPopSlider|" + this._widgetID.ToString() + "|" + colonyInfoForPlanet.ID.ToString() + "|" + (object)faction1.FactionID), (int)((double)faction1.CivPopWeight * 100.0));
					this.App.UI.SetText(this.App.UI.Path(itemGlobalId, "gameMorale_human"), faction1.Morale.ToString());
					double num2 = (colonyInfoForPlanet.CurrentStage == Kerberos.Sots.Data.ColonyStage.GemWorld ? Colony.GetMaxCivilianPop(this.App.GameDatabase, planetInfo) * (double)this.App.AssetDatabase.GemWorldCivMaxBonus : Colony.GetMaxCivilianPop(this.App.GameDatabase, planetInfo)) * (double)colonyInfoForPlanet.CivilianWeight * (double)faction1.CivPopWeight * (double)this.App.AssetDatabase.GetFaction(this.App.GameDatabase.GetPlayerFactionID(colonyInfoForPlanet.PlayerID)).GetImmigrationPopBonusValueForFaction(this.App.AssetDatabase.GetFaction(faction1.FactionID));
					this.App.UI.SetText(this.App.UI.Path(itemGlobalId, "popRatio"), (faction1.CivilianPop / 1000000.0).ToString("0.0") + "M / " + (num2 / 1000000.0).ToString("0.0") + "M");
					++num1;
				}
				this.App.UI.SetShape(this._rootPanel, 0, 0, 360, 90 + 22 * (((IEnumerable<ColonyFactionInfo>)colonyInfoForPlanet.Factions).Count<ColonyFactionInfo>() > 1 ? ((IEnumerable<ColonyFactionInfo>)colonyInfoForPlanet.Factions).Count<ColonyFactionInfo>() : 0));
			}
			else
			{
				if (colonyInfoForPlanet != null || !ShowColonizebuttons || !(this.App.CurrentState.GetType() == typeof(StarMapState)))
					return;
				StarSystemInfo starSystemInfo = this.App.GameDatabase.GetStarSystemInfo(this.App.GameDatabase.GetOrbitalObjectInfo(planetInfo.ID).StarSystemID);
				this.App.UI.SetVisible(this.App.UI.Path(this._rootPanel, "btnColoninzePlanet"), (Kerberos.Sots.StarFleet.StarFleet.CollectAvailableFleets(this.App.Game, this.App.LocalPlayer.ID, starSystemInfo.ID, MissionType.COLONIZATION, false).Any<FleetInfo>() && StarSystemDetailsUI.CollectPlanetListItemsForColonizeMission(this.App, starSystemInfo.ID, this.App.LocalPlayer.ID).Contains<int>(planetID) ? 1 : 0) != 0);
				this.App.UI.SetPropertyString(this.App.UI.Path(this._rootPanel, "btnColoninzePlanet"), "id", "btnColoninzePlanet|" + starSystemInfo.ID.ToString() + "|" + planetInfo.ID.ToString());
			}
		}

		private void CachePlanet(PlanetInfo planetInfo)
		{
			if (this._cachedPlanet != null)
			{
				if (PlanetInfo.AreSame(planetInfo, this._cachedPlanetInfo))
					return;
				this.App.ReleaseObject((IGameObject)this._cachedPlanet);
				this._cachedPlanet = (StellarBody)null;
			}
			this._cachedPlanetInfo = planetInfo;
			this._cachedPlanetReady = false;
			this._cachedPlanet = Kerberos.Sots.GameStates.StarSystem.CreatePlanet(this.App.Game, Vector3.Zero, planetInfo, Matrix.Identity, 1f, false, Kerberos.Sots.GameStates.StarSystem.TerrestrialPlanetQuality.High);
			this._cachedPlanet.PostSetProp("AutoDraw", false);
			this._initialized = false;
			this.App.UI.SetVisible(this.App.UI.Path(this._rootPanel, "loadingCircle"), true);
		}

		public void Update()
		{
			if (this._crits == null || !this._crits.IsReady())
				return;
			if (this._cachedPlanet != null && !this._cachedPlanetReady && this._cachedPlanet.ObjectStatus != GameObjectStatus.Pending)
			{
				this._cachedPlanetReady = true;
				this._cachedPlanet.Active = true;
			}
			if (!this._cachedPlanetReady || this._initialized)
				return;
			this._planetView.PostSetProp("Planet", this._cachedPlanet != null ? this._cachedPlanet.ObjectID : 0);
			if (!this._planetViewLinked)
			{
				this.App.UI.Send((object)"SetGameObject", (object)this.App.UI.Path(this._rootPanel, "contentPreview.desc_viewport"), (object)this._planetView.ObjectID);
				this._planetViewLinked = true;
			}
			this.App.UI.SetVisible(this.App.UI.Path(this._rootPanel, "loadingCircle"), false);
			this._initialized = true;
		}

		public static bool IsOutputRateSlider(string panelName)
		{
			if (!panelName.Contains("partTradeSlider") && !panelName.Contains("partTerraSlider") && (!panelName.Contains("partInfraSlider") && !panelName.Contains("partOverDevelopment")))
				return panelName.Contains("partShipConSlider");
			return true;
		}

		protected void UICommChannel_OnPanelMessage(
		  string panelName,
		  string msgType,
		  string[] msgParams)
		{
			if (msgType == "slider_value")
			{
				if (!panelName.StartsWith("__"))
					return;
				string[] strArray = panelName.Split('|');
				if (int.Parse(strArray[1]) != this._widgetID)
					return;
				ColonyInfo colonyInfo = this.App.GameDatabase.GetColonyInfo(int.Parse(strArray[2]));
				if (colonyInfo == null)
					return;
				if (PlanetWidget.IsOutputRateSlider(panelName))
				{
					StarSystemDetailsUI.SetOutputRateNew(this.App, colonyInfo.OrbitalObjectID, panelName, msgParams[0]);
					StarSystemUI.SyncColonyDetailsControlNew(this.App.Game, this._rootPanel, colonyInfo.ID, this._widgetID, panelName);
				}
				if (strArray[0].Contains("partOverharvestSlider"))
				{
					colonyInfo.OverharvestRate = StarSystemDetailsUI.SliderValueToOutputRate(int.Parse(msgParams[0]));
					this.App.GameDatabase.UpdateColony(colonyInfo);
					StarSystemUI.SyncColonyDetailsControlNew(this.App.Game, this._rootPanel, colonyInfo.ID, this._widgetID, panelName);
				}
				else if (strArray[0].Contains("partCivSlider"))
				{
					colonyInfo.CivilianWeight = StarSystemDetailsUI.SliderValueToOutputRate(int.Parse(msgParams[0]));
					this.App.GameDatabase.UpdateColony(colonyInfo);
					StarSystemUI.SyncColonyDetailsControlNew(this.App.Game, this._rootPanel, colonyInfo.ID, this._widgetID, panelName);
				}
				else
				{
					if (!strArray[0].Contains("partPopSlider"))
						return;
					int lockedVar = int.Parse(strArray[3]);
					Dictionary<int, float> ratios = new Dictionary<int, float>();
					foreach (ColonyFactionInfo faction in colonyInfo.Factions)
						ratios.Add(faction.FactionID, faction.CivPopWeight);
					AlgorithmExtensions.DistributePercentages<int>(ref ratios, lockedVar, StarSystemDetailsUI.SliderValueToOutputRate(int.Parse(msgParams[0])));
					foreach (ColonyFactionInfo faction in colonyInfo.Factions)
					{
						faction.CivPopWeight = ratios[faction.FactionID];
						this.App.GameDatabase.UpdateCivilianPopulation(faction);
					}
					this.App.GameDatabase.UpdateColony(colonyInfo);
					StarSystemUI.SyncColonyDetailsControlNew(this.App.Game, this._rootPanel, colonyInfo.ID, this._widgetID, panelName);
				}
			}
			else
			{
				if (!(msgType == "slider_notched") || !panelName.StartsWith("__"))
					return;
				string[] strArray = panelName.Split('|');
				if (int.Parse(strArray[1]) != this._widgetID)
					return;
				ColonyInfo colonyInfo = this.App.GameDatabase.GetColonyInfo(int.Parse(strArray[2]));
				if (colonyInfo == null || !panelName.Contains("partTradeSlider"))
					return;
				PlanetWidget.UpdateTradeSliderNotchInfo(this.App, colonyInfo.ID, int.Parse(msgParams[0]));
			}
		}

		public static void UpdateTradeSliderNotchInfo(App App, int ColonyID, int value)
		{
			ColonyInfo colonyInfo = App.GameDatabase.GetColonyInfo(ColonyID);
			if (value == -1)
			{
				if (App.GameDatabase.GetSliderNotchSettingInfoForColony(colonyInfo.PlayerID, colonyInfo.ID, UISlidertype.TradeSlider) == null)
					return;
				App.GameDatabase.DeleteUISliderNotchSettingForColony(colonyInfo.PlayerID, colonyInfo.ID, UISlidertype.TradeSlider);
			}
			else
			{
				List<double> exportsForColony = App.Game.GetTradeRatesForWholeExportsForColony(colonyInfo.ID);
				UISliderNotchInfo settingInfoForColony = App.GameDatabase.GetSliderNotchSettingInfoForColony(colonyInfo.PlayerID, colonyInfo.ID, UISlidertype.TradeSlider);
				foreach (double num in exportsForColony)
				{
					if ((int)Math.Ceiling(num * 100.0) == value)
					{
						if (settingInfoForColony != null)
						{
							settingInfoForColony.SliderValue = (double)exportsForColony.IndexOf(num);
							App.GameDatabase.UpdateUISliderNotchSetting(settingInfoForColony);
							break;
						}
						App.GameDatabase.InsertUISliderNotchSetting(App.LocalPlayer.ID, UISlidertype.TradeSlider, (double)exportsForColony.IndexOf(num), colonyInfo.ID);
						break;
					}
				}
			}
		}

		public void Terminate()
		{
			if (this._cachedPlanet != null)
			{
				this.App.ReleaseObject((IGameObject)this._cachedPlanet);
				this._cachedPlanet = (StellarBody)null;
			}
			this._crits.Dispose();
			this.App.UI.PanelMessage -= new UIEventPanelMessage(this.UICommChannel_OnPanelMessage);
		}
	}
}
