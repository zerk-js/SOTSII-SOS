// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.StarSystemUI
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class StarSystemUI
	{
		public const string UIBorderTitle = "title";
		public const string UISystemViewButton = "gameSystemButton";
		public const string UIStellarClassText = "partStellarClass";
		public const string UIMiniMapPanel = "partMiniSystem";
		public const string UIPlanetListControl = "gamePlanetList";
		public const string UIColonyDetails = "colonyControl";
		public const string UIPlanetDetails = "gamePlanetDetails";
		public const string UIMoonDetails = "gameMoonDetails";
		public const string UIGasGiantDetails = "gameGasGiantDetails";
		public const string UIStarDetails = "gameStarDetails";
		private static string UIMoraleEventTooltip;

		internal static string ShowMoraleEventToolTip(GameSession game, int colonyid, int x, int y)
		{
			if (StarSystemUI.UIMoraleEventTooltip == null)
				StarSystemUI.UIMoraleEventTooltip = game.UI.CreatePanelFromTemplate("moraleeventspopup", null);
			List<MoraleEventHistory> list = game.GameDatabase.GetMoraleHistoryEventsForColony(colonyid).ToList<MoraleEventHistory>();
			for (int index = 0; index < 5; ++index)
			{
				string panelId = game.UI.Path(StarSystemUI.UIMoraleEventTooltip, "event" + index.ToString());
				if (index < list.Count || index == 0)
				{
					game.UI.SetVisible(panelId, true);
					if (list.Count != 0)
					{
						game.UI.SetPropertyString(panelId, "label", App.Localize("@UI_" + list[index].moraleEvent.ToString()));
						game.UI.SetPropertyString(panelId, "value", list[index].value.ToString());
					}
					else
					{
						game.UI.SetPropertyString(panelId, "label", App.Localize("@UI_NO_EVENTS"));
						game.UI.SetPropertyString(panelId, "value", "");
					}
				}
				else
					game.UI.SetVisible(panelId, false);
			}
			game.UI.AutoSize(StarSystemUI.UIMoraleEventTooltip);
			game.UI.ForceLayout(StarSystemUI.UIMoraleEventTooltip);
			game.UI.ShowTooltip(StarSystemUI.UIMoraleEventTooltip, (float)x, (float)y);
			return StarSystemUI.UIMoraleEventTooltip;
		}

		internal static void SyncStarDetailsControl(GameSession game, string panelId, int systemId)
		{
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(systemId);
			int num = game.GameDatabase.GetPlanetInfosOrbitingStar(starSystemInfo.ID).Count<PlanetInfo>();
			string upperInvariant = starSystemInfo.Name.ToUpperInvariant();
			string stellarClass = starSystemInfo.StellarClass;
			string stellarActivity = new StellarClass(starSystemInfo.StellarClass).GetStellarActivity();
			string propertyValue = num.ToString();
			game.UI.SetPropertyString(game.UI.Path(panelId, "title"), "text", upperInvariant);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partStellarClass"), "value", stellarClass);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partSolarActivity"), "value", stellarActivity);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partPlanetCount"), "value", propertyValue);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partStellarClass"), "text", stellarClass);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partSolarActivity"), "text", stellarActivity);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partPlanetCount"), "text", propertyValue);
			game.UI.AutoSize(panelId);
		}

		internal static void SyncStarDetailsStations(
		  GameSession game,
		  string panelId,
		  int systemId,
		  int playerId)
		{
			game.GameDatabase.GetStarSystemInfo(systemId);
			if (game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerInfo(playerId).FactionID).CanUseGate())
			{
				if (game.GameDatabase.GetHiverGateForSystem(systemId, playerId) != null)
				{
					game.UI.AddItem(game.UI.Path(panelId, "stations"), "", 10, "");
					string itemGlobalId = game.UI.GetItemGlobalID(game.UI.Path(panelId, "stations"), "", 10, "");
					game.UI.SetPropertyString(game.UI.Path(itemGlobalId, "icon"), "sprite", "systemTagGateStation");
					game.UI.SetVisible(game.UI.Path(itemGlobalId, "ring"), false);
				}
				else if (game.GameDatabase.SystemHasGate(systemId, playerId))
				{
					game.UI.AddItem(game.UI.Path(panelId, "stations"), "", 11, "");
					string itemGlobalId = game.UI.GetItemGlobalID(game.UI.Path(panelId, "stations"), "", 11, "");
					game.UI.SetPropertyString(game.UI.Path(itemGlobalId, "icon"), "sprite", "systemTagGate");
					game.UI.SetVisible(game.UI.Path(itemGlobalId, "ring"), false);
				}
			}
			List<StationInfo> list = game.GameDatabase.GetStationForSystemAndPlayer(systemId, playerId).ToList<StationInfo>();
			bool flag1 = list.Any<StationInfo>((Func<StationInfo, bool>)(x => x.DesignInfo.StationType == StationType.NAVAL));
			bool flag2 = list.Any<StationInfo>((Func<StationInfo, bool>)(x => x.DesignInfo.StationType == StationType.SCIENCE));
			bool flag3 = list.Any<StationInfo>((Func<StationInfo, bool>)(x => x.DesignInfo.StationType == StationType.DIPLOMATIC));
			bool flag4 = list.Any<StationInfo>((Func<StationInfo, bool>)(x => x.DesignInfo.StationType == StationType.CIVILIAN));
			bool flag5 = false;
			bool flag6 = false;
			bool flag7 = false;
			bool flag8 = false;
			int supportedBySystem = game.GameDatabase.GetNumberMaxStationsSupportedBySystem(game, systemId, playerId);
			if (supportedBySystem > 0)
			{
				for (int index = 0; index < supportedBySystem; ++index)
				{
					if (flag1 && !flag5)
						flag5 = true;
					else if (flag2 && !flag6)
						flag6 = true;
					else if (flag3 && !flag7)
						flag7 = true;
					else if (flag4 && !flag8)
					{
						flag8 = true;
					}
					else
					{
						game.UI.AddItem(game.UI.Path(panelId, "stations"), "", 100 + index, "");
						string itemGlobalId = game.UI.GetItemGlobalID(game.UI.Path(panelId, "stations"), "", 100 + index, "");
						game.UI.SetVisible(game.UI.Path(itemGlobalId, "icon"), false);
						game.UI.SetVisible(game.UI.Path(itemGlobalId, "ring"), true);
					}
				}
			}
			if (flag4)
			{
				game.UI.AddItem(game.UI.Path(panelId, "stations"), "", 4, "");
				string itemGlobalId = game.UI.GetItemGlobalID(game.UI.Path(panelId, "stations"), "", 4, "");
				game.UI.SetPropertyString(game.UI.Path(itemGlobalId, "icon"), "sprite", "systemTagCivilian");
				game.UI.SetVisible(game.UI.Path(itemGlobalId, "ring"), (flag8 ? 1 : 0) != 0);
			}
			if (flag3)
			{
				game.UI.AddItem(game.UI.Path(panelId, "stations"), "", 3, "");
				string itemGlobalId = game.UI.GetItemGlobalID(game.UI.Path(panelId, "stations"), "", 3, "");
				game.UI.SetPropertyString(game.UI.Path(itemGlobalId, "icon"), "sprite", "systemTagDiplomatic");
				game.UI.SetVisible(game.UI.Path(itemGlobalId, "ring"), (flag7 ? 1 : 0) != 0);
			}
			if (flag2)
			{
				game.UI.AddItem(game.UI.Path(panelId, "stations"), "", 2, "");
				string itemGlobalId = game.UI.GetItemGlobalID(game.UI.Path(panelId, "stations"), "", 2, "");
				game.UI.SetPropertyString(game.UI.Path(itemGlobalId, "icon"), "sprite", "systemTagScience");
				game.UI.SetVisible(game.UI.Path(itemGlobalId, "ring"), (flag6 ? 1 : 0) != 0);
			}
			if (!flag1)
				return;
			game.UI.AddItem(game.UI.Path(panelId, "stations"), "", 1, "");
			string itemGlobalId1 = game.UI.GetItemGlobalID(game.UI.Path(panelId, "stations"), "", 1, "");
			game.UI.SetPropertyString(game.UI.Path(itemGlobalId1, "icon"), "sprite", "AnchorTag");
			game.UI.SetVisible(game.UI.Path(itemGlobalId1, "ring"), (flag5 ? 1 : 0) != 0);
		}

		internal static void SyncGasGiantDetailsControl(
		  GameSession game,
		  string panelId,
		  int orbitalId)
		{
			OrbitalObjectInfo orbitalObjectInfo = game.GameDatabase.GetOrbitalObjectInfo(orbitalId);
			PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(orbitalId);
			int num = game.GameDatabase.CountMoons(orbitalId);
			string upperInvariant = orbitalObjectInfo.Name.ToUpperInvariant();
			string sizeString = StarSystemDetailsUI.GetSizeString(planetInfo.Size);
			string propertyValue1 = App.Localize("@UI_GAS_GIANT_TYPE");
			string propertyValue2 = num.ToString();
			game.UI.SetPropertyString(game.UI.Path(panelId, "title"), "text", upperInvariant);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partSize"), "value", sizeString);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partType"), "value", propertyValue1);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partMoonCount"), "value", propertyValue2);
			game.UI.AutoSize(panelId);
		}

		internal static void SyncMoonDetailsControl(GameSession game, string panelId, int orbitalId)
		{
			PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(orbitalId);
			OrbitalObjectInfo orbitalObjectInfo = game.GameDatabase.GetOrbitalObjectInfo(orbitalId);
			int resources = planetInfo.Resources;
			string upperInvariant = orbitalObjectInfo.Name.ToUpperInvariant();
			string sizeString = StarSystemDetailsUI.GetSizeString(planetInfo.Size);
			string propertyValue = resources.ToString("N0");
			game.UI.SetPropertyString(game.UI.Path(panelId, "title"), "text", upperInvariant);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partSize"), "value", sizeString);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partResources"), "value", propertyValue);
		}

		internal static void SyncAsteroidDetailsControl(
		  GameSession game,
		  string panelId,
		  int orbitalId)
		{
			LargeAsteroidInfo largeAsteroidInfo = game.GameDatabase.GetLargeAsteroidInfo(orbitalId);
			OrbitalObjectInfo orbitalObjectInfo = game.GameDatabase.GetOrbitalObjectInfo(orbitalId);
			int resources = largeAsteroidInfo.Resources;
			string upperInvariant = orbitalObjectInfo.Name.ToUpperInvariant();
			string propertyValue = resources.ToString("N0");
			game.UI.SetPropertyString(game.UI.Path(panelId, "title"), "text", upperInvariant);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partSize"), "value", "");
			game.UI.SetPropertyString(game.UI.Path(panelId, "partResources"), "value", propertyValue);
		}

		internal static void SyncPlanetDetailsControl(GameSession game, string panelId, int orbitalId)
		{
			PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(orbitalId);
			OrbitalObjectInfo orbitalObjectInfo = game.GameDatabase.GetOrbitalObjectInfo(orbitalId);
			float planetHazardRating1 = game.GameDatabase.GetPlanetHazardRating(game.LocalPlayer.ID, orbitalId, false);
			float planetHazardRating2 = game.GameDatabase.GetNonAbsolutePlanetHazardRating(game.LocalPlayer.ID, orbitalId, false);
			int resources = planetInfo.Resources;
			int biosphere = planetInfo.Biosphere;
			ColonyInfo colonyInfoForPlanet = game.GameDatabase.GetColonyInfoForPlanet(planetInfo.ID);
			double civilianPopulation = game.GameDatabase.GetCivilianPopulation(orbitalId, colonyInfoForPlanet != null ? game.GameDatabase.GetPlayerInfo(colonyInfoForPlanet.PlayerID).FactionID : 0, colonyInfoForPlanet != null && game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerFactionID(colonyInfoForPlanet.PlayerID)).HasSlaves());
			float infrastructure = planetInfo.Infrastructure;
			int num1 = game.GameDatabase.CountMoons(orbitalId);
			int num2 = (int)Colony.EstimateColonyDevelopmentCost(game, orbitalId, game.LocalPlayer.ID);
			string upperInvariant = orbitalObjectInfo.Name.ToUpperInvariant();
			string propertyValue1 = num1.ToString();
			string propertyValue2 = biosphere.ToString("N0");
			string propertyValue3 = resources.ToString("N0");
			string propertyValue4 = civilianPopulation.ToString("N0");
			string propertyValue5 = App.Localize("@UI_PLANET_TYPE_" + planetInfo.Type.ToUpperInvariant());
			string infraString = StarSystemDetailsUI.GetInfraString(infrastructure);
			string hazardString = StarSystemDetailsUI.GetHazardString(planetHazardRating1);
			string propertyValue6 = num2.ToString("N0");
			string sizeString = StarSystemDetailsUI.GetSizeString(planetInfo.Size);
			game.UI.SetPropertyString(game.UI.Path(panelId, "title"), "text", upperInvariant);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partSize"), "value", sizeString);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partResources"), "value", propertyValue3);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partBiosphere"), "value", propertyValue2);
			if (num2 == 0 || !game.GameDatabase.CanColonizePlanet(game.LocalPlayer.ID, orbitalId, game.GameDatabase.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, game.LocalPlayer.ID)))
			{
				game.UI.SetVisible(game.UI.Path(panelId, "partDevCost"), false);
			}
			else
			{
				game.UI.SetVisible(game.UI.Path(panelId, "partDevCost"), true);
				game.UI.SetPropertyString(game.UI.Path(panelId, "partDevCost"), "value", propertyValue6);
			}
			game.UI.SetPropertyString(game.UI.Path(panelId, "partType"), "value", propertyValue5);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partCivPop"), "value", propertyValue4);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partInfra"), "value", infraString);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partMoonCount"), "value", propertyValue1);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partHazard"), "value", hazardString);
			game.UI.SetSliderValue(game.UI.Path(panelId, "partClimateSlider"), (int)planetHazardRating2);
			if (game.LocalPlayer.Faction.Name == "loa")
			{
				game.UI.SetVisible(game.UI.Path(panelId, "partClimateSlider"), false);
				game.UI.SetVisible(game.UI.Path(panelId, "loaSuitability"), true);
				game.UI.SetVisible(game.UI.Path(panelId, "partHazard"), false);
				game.UI.SetVisible(game.UI.Path(panelId, "parthazardLevel"), false);
				game.UI.SetText(game.UI.Path(panelId, "loaSuitability"), "Growth Potential: " + Colony.GetLoaGrowthPotential(game, planetInfo.ID, game.GameDatabase.GetOrbitalObjectInfo(planetInfo.ID).StarSystemID, game.LocalPlayer.ID).ToString("0.0%"));
			}
			else
			{
				game.UI.SetVisible(game.UI.Path(panelId, "loaSuitability"), false);
				game.UI.SetVisible(game.UI.Path(panelId, "partClimateSlider"), true);
				game.UI.SetVisible(game.UI.Path(panelId, "partHazard"), true);
				game.UI.SetVisible(game.UI.Path(panelId, "parthazardLevel"), true);
			}
			Vector3 vector3_1 = new Vector3((float)byte.MaxValue, 0.0f, 0.0f);
			Vector3 vector3_2 = new Vector3(0.0f, (float)byte.MaxValue, 0.0f);
			Vector3 vector3_3 = new Vector3((float)byte.MaxValue, (float)byte.MaxValue, (float)byte.MaxValue);
			game.UI.SetPropertyColor(game.UI.Path(panelId, "partResources"), "color", vector3_3);
			game.UI.SetPropertyColor(game.UI.Path(panelId, "partBiosphere"), "color", vector3_3);
			game.UI.SetPropertyColor(game.UI.Path(panelId, "partCivPop"), "color", vector3_3);
			game.UI.SetPropertyColor(game.UI.Path(panelId, "partInfra"), "color", vector3_3);
			game.UI.SetPropertyColor(game.UI.Path(panelId, "partHazard"), "color", vector3_3);
			game.UI.SetVisible(game.UI.Path(panelId, "rebellionActive"), (colonyInfoForPlanet != null ? (colonyInfoForPlanet.PlayerID != game.LocalPlayer.ID ? 0 : (colonyInfoForPlanet.RebellionType != RebellionType.None ? 1 : 0)) : 0) != 0);
			if (colonyInfoForPlanet == null)
				return;
			ColonyHistoryData historyForColony = game.GameDatabase.GetLastColonyHistoryForColony(colonyInfoForPlanet.ID);
			if (historyForColony == null)
				return;
			if (historyForColony.resources < resources)
				game.UI.SetPropertyColor(game.UI.Path(panelId, "partResources"), "color", vector3_2);
			else if (historyForColony.resources > resources)
				game.UI.SetPropertyColor(game.UI.Path(panelId, "partResources"), "color", vector3_1);
			if (historyForColony.biosphere < biosphere)
				game.UI.SetPropertyColor(game.UI.Path(panelId, "partBiosphere"), "color", vector3_2);
			else if (historyForColony.biosphere > biosphere)
				game.UI.SetPropertyColor(game.UI.Path(panelId, "partBiosphere"), "color", vector3_1);
			if (historyForColony.civ_pop < civilianPopulation)
				game.UI.SetPropertyColor(game.UI.Path(panelId, "partCivPop"), "color", vector3_2);
			else if (historyForColony.civ_pop > civilianPopulation)
				game.UI.SetPropertyColor(game.UI.Path(panelId, "partCivPop"), "color", vector3_1);
			if ((double)historyForColony.infrastructure < (double)infrastructure)
				game.UI.SetPropertyColor(game.UI.Path(panelId, "partInfra"), "color", vector3_2);
			else if ((double)historyForColony.infrastructure < (double)infrastructure)
				game.UI.SetPropertyColor(game.UI.Path(panelId, "partInfra"), "color", vector3_1);
			if ((double)historyForColony.hazard > (double)planetHazardRating1)
			{
				game.UI.SetPropertyColor(game.UI.Path(panelId, "partHazard"), "color", vector3_2);
			}
			else
			{
				if ((double)historyForColony.hazard >= (double)planetHazardRating1)
					return;
				game.UI.SetPropertyColor(game.UI.Path(panelId, "partHazard"), "color", vector3_1);
			}
		}

		public static void SyncPlanetCardPanelColor(App game, string panelName, Vector3 color)
		{
			foreach (string str in new List<string>()
	  {
		"BOL1",
		"BOL2",
		"BOL3",
		"BOL4",
		"BOL5",
		"BOL6",
		"BOL7",
		"BOL8",
		"L_Cap",
		"R_Cap",
		"PC_OWNER_S"
	  })
				game.UI.SetPropertyColorNormalized(game.UI.Path(panelName, str), nameof(color), color);
		}

		internal static void SyncPlanetDetailsControlNew(
		  GameSession game,
		  string panelId,
		  int orbitalId)
		{
			PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(orbitalId);
			OrbitalObjectInfo orbitalObjectInfo = game.GameDatabase.GetOrbitalObjectInfo(orbitalId);
			float planetHazardRating1 = game.GameDatabase.GetPlanetHazardRating(game.LocalPlayer.ID, orbitalId, false);
			float planetHazardRating2 = game.GameDatabase.GetNonAbsolutePlanetHazardRating(game.LocalPlayer.ID, orbitalId, false);
			int resources = planetInfo.Resources;
			int biosphere = planetInfo.Biosphere;
			double civilianPopulation = game.GameDatabase.GetCivilianPopulation(orbitalId, 0, false);
			float infrastructure = planetInfo.Infrastructure;
			int num1 = game.GameDatabase.CountMoons(orbitalId);
			int num2 = (int)Colony.EstimateColonyDevelopmentCost(game, orbitalId, game.LocalPlayer.ID);
			ColonyInfo colonyInfoForPlanet = game.GameDatabase.GetColonyInfoForPlanet(orbitalId);
			string upperInvariant = orbitalObjectInfo.Name.ToUpperInvariant();
			string propertyValue1 = num1.ToString();
			string propertyValue2 = biosphere.ToString("N0");
			string propertyValue3 = resources.ToString("N0");
			string propertyValue4 = civilianPopulation.ToString("N0");
			string propertyValue5 = App.Localize("@UI_PLANET_TYPE_" + planetInfo.Type.ToUpperInvariant());
			string infraString = StarSystemDetailsUI.GetInfraString(infrastructure);
			string hazardString = StarSystemDetailsUI.GetHazardString(planetHazardRating1);
			string propertyValue6 = num2.ToString("N0");
			string sizeString = StarSystemDetailsUI.GetSizeString(planetInfo.Size);
			string propertyValue7 = "0";
			string propertyValue8 = "0";
			string propertyValue9 = "0";
			string propertyValue10 = "0";
			if (colonyInfoForPlanet != null)
			{
				propertyValue7 = colonyInfoForPlanet.ImperialPop.ToString("N0");
				propertyValue9 = colonyInfoForPlanet.EconomyRating.ToString("N0");
				propertyValue8 = Colony.GetIndustrialOutput(game, colonyInfoForPlanet, planetInfo).ToString("N0");
				propertyValue10 = Colony.GetColonySupportCost(game.AssetDatabase, game.GameDatabase, colonyInfoForPlanet).ToString("N0");
			}
			game.UI.SetPropertyString(game.UI.Path(panelId, "title"), "text", upperInvariant);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partSize"), "text", sizeString);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partResources"), "text", propertyValue3);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partBiosphere"), "text", propertyValue2);
			if (game.GameDatabase.IsHazardousPlanet(game.LocalPlayer.ID, orbitalId, game.GameDatabase.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, game.LocalPlayer.ID)))
			{
				game.UI.SetVisible(game.UI.Path(panelId, "Unsuitable"), true);
				game.UI.SetVisible(game.UI.Path(panelId, "Suitable"), false);
			}
			else
			{
				game.UI.SetVisible(game.UI.Path(panelId, "Unsuitable"), false);
				game.UI.SetVisible(game.UI.Path(panelId, "Suitable"), true);
			}
			if (!game.GameDatabase.CanColonizePlanet(game.LocalPlayer.ID, orbitalId, game.GameDatabase.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, game.LocalPlayer.ID)))
			{
				game.UI.SetPropertyString(game.UI.Path(panelId, "partDevCost"), "text", "Prohibitive");
			}
			else
			{
				game.UI.SetVisible(game.UI.Path(panelId, "partDevCost"), true);
				game.UI.SetPropertyString(game.UI.Path(panelId, "partDevCost"), "text", propertyValue6);
			}
			if (colonyInfoForPlanet != null)
			{
				if (colonyInfoForPlanet.PlayerID == game.LocalPlayer.ID)
				{
					game.UI.SetVisible(game.UI.Path(panelId, "devText"), false);
					game.UI.SetVisible(game.UI.Path(panelId, "incomeText"), true);
					game.UI.SetPropertyString(game.UI.Path(panelId, "partDevCost"), "text", (Colony.GetTaxRevenue(game.App, colonyInfoForPlanet) - Colony.GetColonySupportCost(game.AssetDatabase, game.GameDatabase, colonyInfoForPlanet)).ToString("N0"));
				}
				else
				{
					game.UI.SetVisible(game.UI.Path(panelId, "devText"), true);
					game.UI.SetVisible(game.UI.Path(panelId, "incomeText"), false);
					game.UI.SetPropertyString(game.UI.Path(panelId, "partDevCost"), "text", "Inhabited");
				}
				PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(colonyInfoForPlanet.PlayerID);
				StarSystemUI.SyncPlanetCardPanelColor(game.App, panelId + ".itemDetails.ownerPC", playerInfo.PrimaryColor);
				StarSystemUI.SyncPlanetCardPanelColor(game.App, panelId + ".expanded.ownerPC", playerInfo.PrimaryColor);
			}
			else
				game.UI.SetVisible(panelId + ".itemDetails.ownerPC", false);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partType"), "text", propertyValue5);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partCivPop"), "text", propertyValue4);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partImpPop"), "text", propertyValue7);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partInfra"), "text", infraString);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partMoonCount"), "text", propertyValue1);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partHazard"), "text", hazardString);
			game.UI.SetSliderValue(game.UI.Path(panelId, "partClimateSlider"), (int)planetHazardRating2);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partEconRating"), "text", propertyValue9);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partLifeSupCost"), "text", propertyValue10);
			game.UI.SetPropertyString(game.UI.Path(panelId, "partIndustrial"), "text", propertyValue8);
			if (game.LocalPlayer.Faction.Name == "loa")
			{
				game.UI.SetVisible(game.UI.Path(panelId, "partClimateSlider"), false);
				game.UI.SetVisible(game.UI.Path(panelId, "loaSuitability"), true);
				game.UI.SetVisible(game.UI.Path(panelId, "partHazard"), false);
				game.UI.SetVisible(game.UI.Path(panelId, "parthazardLevel"), false);
				game.UI.SetText(game.UI.Path(panelId, "loaSuitability"), "Growth Potential: " + Colony.GetLoaGrowthPotential(game, planetInfo.ID, game.GameDatabase.GetOrbitalObjectInfo(planetInfo.ID).StarSystemID, game.LocalPlayer.ID).ToString("0.0%"));
			}
			else
			{
				game.UI.SetVisible(game.UI.Path(panelId, "loaSuitability"), false);
				game.UI.SetVisible(game.UI.Path(panelId, "partClimateSlider"), true);
				game.UI.SetVisible(game.UI.Path(panelId, "partHazard"), true);
				game.UI.SetVisible(game.UI.Path(panelId, "parthazardLevel"), true);
			}
		}

		private static string FormatName(string name, int colonyID, int widgetID)
		{
			return "__" + name + "|" + widgetID.ToString() + "|" + colonyID.ToString();
		}

		internal static void ClearColonyDetailsControl(GameSession sim, string panelId)
		{
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partCivPop"), "label", "");
			sim.UI.SetVisible(sim.UI.Path(panelId, "incomeText"), false);
			sim.UI.SetVisible(sim.UI.Path(panelId, "devText"), true);
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partStage"), "text", "");
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partOverharvestSlider", "right_label"), "text", "");
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "civEqualibText"), "text", "");
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partCivPop"), "text", "");
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partImpPop"), "text", "");
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partDevCost"), "text", "");
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partEconRating"), "text", "");
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partLifeSupCost"), "text", "");
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partIndustrial"), "text", "");
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partTerraSlider", "right_label"), "text", "");
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partInfraSlider", "right_label"), "text", "");
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partShipConSlider", "right_label"), "text", "");
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partOverDevelopment", "right_label"), "text", "");
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partWorkRate", "right_label"), "text", "");
			sim.UI.SetText(sim.UI.Path(panelId, "gameMorale_human"), "");
			sim.UI.SetText(sim.UI.Path(panelId, "gameMorale_zuul"), "");
			sim.UI.SetText(sim.UI.Path(panelId, "gameMorale_liir_zuul"), "");
			sim.UI.SetText(sim.UI.Path(panelId, "gameMorale_tarkas"), "");
			sim.UI.SetText(sim.UI.Path(panelId, "gameMorale_hiver"), "");
			sim.UI.SetText(sim.UI.Path(panelId, "gameMorale_morrigi"), "");
			sim.UI.SetText(sim.UI.Path(panelId, "gameMorale_loa"), "");
		}

		internal static void SyncColonyDetailsControlNew(
		  GameSession sim,
		  string panelId,
		  int colonyId,
		  int widgetId,
		  string sliderName)
		{
			ColonyInfo colonyInfo = sim.GameDatabase.GetColonyInfo(colonyId);
			PlanetInfo planetInfo = sim.GameDatabase.GetPlanetInfo(colonyInfo.OrbitalObjectID);
			double civilianPopulation1 = sim.GameDatabase.GetCivilianPopulation(colonyInfo.OrbitalObjectID, 0, false);
			float economyRating = colonyInfo.EconomyRating;
			int colonySupportCost = (int)Colony.GetColonySupportCost(sim.AssetDatabase, sim.GameDatabase, colonyInfo);
			int num1 = (int)Colony.GetTaxRevenue(sim.App, colonyInfo) - colonySupportCost;
			int industrialOutput = (int)Colony.GetIndustrialOutput(sim, colonyInfo, planetInfo);
			float biosphereDelta = Colony.GetBiosphereDelta(sim, colonyInfo, planetInfo, 0.0);
			float num2 = (float)Colony.GetInfrastructureDelta(sim, colonyInfo, planetInfo) * 100f;
			float terraformingDelta = Colony.GetTerraformingDelta(sim, colonyInfo, planetInfo, 0.0);
			float shipConstResources = Colony.GetShipConstResources(sim, colonyInfo, planetInfo);
			float num3 = (float)((double)colonyInfo.OverdevelopProgress / (double)Colony.GetOverdevelopmentTarget(sim, planetInfo) * 100.0);
			int num4 = (double)colonyInfo.OverdevelopRate == 0.0 ? -1 : (int)Math.Ceiling(((double)Colony.GetOverdevelopmentTarget(sim, planetInfo) - (double)colonyInfo.OverdevelopProgress) / ((double)industrialOutput * (double)colonyInfo.OverdevelopRate));
			bool flag1 = (double)sim.GameDatabase.GetPlanetHazardRating(sim.LocalPlayer.ID, colonyInfo.OrbitalObjectID, false) != 0.0;
			bool flag2 = (double)planetInfo.Infrastructure < 1.0;
			bool flag3 = true;
			bool flag4 = Colony.CanBeOverdeveloped(sim.AssetDatabase, sim.GameDatabase, colonyInfo, planetInfo);
			int num5 = (int)Colony.CalcOverharvestRate(sim.AssetDatabase, sim.GameDatabase, colonyInfo, planetInfo);
			bool flag5 = sim.AssetDatabase.GetFaction(sim.GameDatabase.GetPlayerFactionID(colonyInfo.PlayerID)).HasSlaves();
			string propertyValue1 = num5.ToString("N0");
			string propertyValue2 = civilianPopulation1.ToString("N0");
			string propertyValue3 = colonyInfo.ImperialPop.ToString("N0");
			string propertyValue4 = num1.ToString("N0");
			string propertyValue5 = ((int)((double)economyRating * 100.0)).ToString("N0");
			string propertyValue6 = colonySupportCost.ToString("N0");
			string propertyValue7 = industrialOutput.ToString("N0");
			string propertyValue8 = terraformingDelta.ToString("F2");
			int sliderValue1 = StarSystemDetailsUI.OutputRateToSliderValue(colonyInfo.TerraRate);
			string propertyValue9 = num2.ToString("F2");
			int sliderValue2 = StarSystemDetailsUI.OutputRateToSliderValue(colonyInfo.InfraRate);
			string propertyValue10 = shipConstResources.ToString("N0");
			int sliderValue3 = StarSystemDetailsUI.OutputRateToSliderValue(colonyInfo.ShipConRate);
			string propertyValue11 = string.Format(App.Localize("@UI_GAMECOLONYDETAILS_WORKRATECHANGE"), (object)Colony.GetSlaveIndustrialOutput(sim, colonyInfo).ToString("N0"), (object)Colony.GetKilledSlavePopulation(sim, colonyInfo, sim.GameDatabase.GetSlavePopulation(planetInfo.ID, sim.GameDatabase.GetPlayerFactionID(colonyInfo.PlayerID))).ToString("N0"));
			int sliderValue4 = StarSystemDetailsUI.OutputRateToSliderValue(colonyInfo.SlaveWorkRate);
			int sliderValue5 = StarSystemDetailsUI.OutputRateToSliderValue(colonyInfo.OverharvestRate);
			string propertyValue12 = string.Format("{0:0.00}% ({1} {2})", (object)num3, num4 == -1 ? (object)"∞" : (object)num4.ToString(), num4 == 1 ? (object)App.Localize("@UI_GENERAL_TURN") : (object)App.Localize("@UI_GENERAL_TURNS"));
			int sliderValue6 = StarSystemDetailsUI.OutputRateToSliderValue(colonyInfo.OverdevelopRate);
			int sliderValue7 = StarSystemDetailsUI.OutputRateToSliderValue(colonyInfo.TradeRate);
			int num6 = (int)((double)colonyInfo.CivilianWeight * 100.0);
			string propertyValue13 = string.Format("{0:0}% ({1} {2})", (object)num6, (double)biosphereDelta > 0.0 ? (object)("+" + biosphereDelta.ToString()) : (object)biosphereDelta.ToString(), (object)App.Localize("@UI_GAMECOLONYDETAILS_BIOSPHERE"));
			sim.UI.SetVisible(sim.UI.Path(panelId, "pnlSlaves"), (flag5 ? 1 : 0) != 0);
			sim.UI.SetVisible(sim.UI.Path(panelId, "pnlMorale"), (!flag5 ? 1 : 0) != 0);
			sim.UI.SetVisible(sim.UI.Path(panelId, "partCivSlider"), (!flag5 ? 1 : 0) != 0);
			sim.UI.SetVisible(sim.UI.Path(panelId, "partHardenedStructure"), (sim.GameDatabase.GetStratModifier<bool>(StratModifiers.AllowHardenedStructures, colonyInfo.PlayerID) ? 1 : 0) != 0);
			sim.UI.SetVisible(sim.UI.Path(panelId, "lblHardenedStructure"), (sim.GameDatabase.GetStratModifier<bool>(StratModifiers.AllowHardenedStructures, colonyInfo.PlayerID) ? 1 : 0) != 0);
			sim.UI.SetChecked(sim.UI.Path(panelId, "partHardenedStructure"), (colonyInfo.isHardenedStructures ? 1 : 0) != 0);
			sim.UI.SetVisible(sim.UI.Path(panelId, StarSystemUI.FormatName("partTerraSlider", colonyId, widgetId)), (flag1 ? 1 : 0) != 0);
			sim.UI.SetVisible(sim.UI.Path(panelId, StarSystemUI.FormatName("partInfraSlider", colonyId, widgetId)), (flag2 ? 1 : 0) != 0);
			sim.UI.SetVisible(sim.UI.Path(panelId, StarSystemUI.FormatName("partShipConSlider", colonyId, widgetId)), (flag3 ? 1 : 0) != 0);
			sim.UI.SetVisible(sim.UI.Path(panelId, StarSystemUI.FormatName("partOverDevelopment", colonyId, widgetId)), (flag4 ? 1 : 0) != 0);
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partStage"), "text", App.Localize("@UI_COLONYSTAGE_" + colonyInfo.CurrentStage.ToString().ToUpper()));
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partOverharvestSlider", "right_label"), "text", propertyValue1);
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "civEqualibText"), "text", propertyValue13);
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partCivPop"), "text", propertyValue2);
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partImpPop"), "text", propertyValue3);
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partDevCost"), "text", propertyValue4);
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partEconRating"), "text", propertyValue5);
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partLifeSupCost"), "text", propertyValue6);
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partIndustrial"), "text", propertyValue7);
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partTerraSlider", "right_label"), "text", propertyValue8);
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partInfraSlider", "right_label"), "text", propertyValue9);
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partShipConSlider", "right_label"), "text", propertyValue10);
			sim.UI.SetText(sim.UI.Path(panelId, "shipconstructionValue"), flag3 ? propertyValue10 : "");
			sim.UI.SetText(sim.UI.Path(panelId, "infrastructureValue"), flag2 ? propertyValue9 : "");
			sim.UI.SetText(sim.UI.Path(panelId, "terraformingValue"), flag1 ? propertyValue8 : "");
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partOverDevelopment", "right_label"), "text", propertyValue12);
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partWorkRate", "right_label"), "text", propertyValue11);
			if (sliderName != StarSystemUI.FormatName("partTradeSlider", colonyId, widgetId))
			{
				sim.UI.SetSliderValue(sim.UI.Path(panelId, StarSystemUI.FormatName("partTradeSlider", colonyId, widgetId)), sliderValue7);
				sim.UI.ClearSliderNotches(sim.UI.Path(panelId, StarSystemUI.FormatName("partTradeSlider", colonyId, widgetId)));
				foreach (double num7 in sim.GetTradeRatesForWholeExportsForColony(colonyInfo.ID))
					sim.UI.AddSliderNotch(sim.UI.Path(panelId, StarSystemUI.FormatName("partTradeSlider", colonyId, widgetId)), (int)Math.Ceiling(num7 * 100.0));
			}
			if (sliderName != StarSystemUI.FormatName("partCivSlider", colonyId, widgetId))
				sim.UI.SetSliderValue(sim.UI.Path(panelId, StarSystemUI.FormatName("partCivSlider", colonyId, widgetId)), num6);
			if (sliderName != StarSystemUI.FormatName("partTerraSlider", colonyId, widgetId))
				sim.UI.SetSliderValue(sim.UI.Path(panelId, StarSystemUI.FormatName("partTerraSlider", colonyId, widgetId)), sliderValue1);
			if (sliderName != StarSystemUI.FormatName("partInfraSlider", colonyId, widgetId))
				sim.UI.SetSliderValue(sim.UI.Path(panelId, StarSystemUI.FormatName("partInfraSlider", colonyId, widgetId)), sliderValue2);
			if (sliderName != StarSystemUI.FormatName("partShipConSlider", colonyId, widgetId))
				sim.UI.SetSliderValue(sim.UI.Path(panelId, StarSystemUI.FormatName("partShipConSlider", colonyId, widgetId)), sliderValue3);
			if (sliderName != StarSystemUI.FormatName("partOverDevelopment", colonyId, widgetId))
				sim.UI.SetSliderValue(sim.UI.Path(panelId, StarSystemUI.FormatName("partOverDevelopment", colonyId, widgetId)), sliderValue6);
			if (sliderName != "partOverharvestSlider")
			{
				sim.UI.SetSliderRange(sim.UI.Path(panelId, StarSystemUI.FormatName("partOverharvestSlider", colonyId, widgetId)), (int)((double)sim.GameDatabase.GetStratModifier<float>(StratModifiers.MinOverharvestRate, colonyInfo.PlayerID) * 100.0), 100);
				sim.UI.SetSliderValue(sim.UI.Path(panelId, StarSystemUI.FormatName("partOverharvestSlider", colonyId, widgetId)), sliderValue5);
			}
			if (sliderName != "partWorkRate")
				sim.UI.SetSliderValue(sim.UI.Path(panelId, "partWorkRate"), sliderValue4);
			foreach (Faction faction in sim.AssetDatabase.Factions)
			{
				if (faction.IsPlayable)
					sim.UI.SetText(sim.UI.Path(panelId, "gameMorale_" + faction.Name), "--");
			}
			foreach (ColonyFactionInfo civilianPopulation2 in sim.GameDatabase.GetCivilianPopulations(colonyInfo.OrbitalObjectID))
			{
				string factionName = sim.GameDatabase.GetFactionName(civilianPopulation2.FactionID);
				sim.UI.SetText(sim.UI.Path(panelId, "gameMorale_" + factionName), string.Format("{0:0}", (object)civilianPopulation2.Morale));
			}
			sim.UI.AutoSize(sim.UI.Path(panelId, "partConstructionSliders", "ColonyModifiers"));
			sim.UI.AutoSize(sim.UI.Path(panelId, "partConstructionSliders"));
			sim.UI.AutoSize(sim.UI.Path(panelId, "pnlMoraleStats"));
			sim.UI.SetPostMouseOverEvents("moraleeventtooltipover", true);
			sim.UI.SetPostMouseOverEvents(sim.UI.Path(panelId, "MoraleRow"), true);
			foreach (ColonyFactionInfo faction1 in colonyInfo.Factions)
			{
				string itemGlobalId = sim.UI.GetItemGlobalID(sim.UI.Path(panelId, "MoraleRow"), "", faction1.FactionID, "");
				Faction faction2 = sim.AssetDatabase.GetFaction(faction1.FactionID);
				sim.UI.SetPropertyString(sim.UI.Path(itemGlobalId, "factionicon"), "sprite", "logo_" + faction2.Name.ToLower());
				sim.UI.SetSliderValue(sim.UI.Path(itemGlobalId, "__partPopSlider|" + widgetId.ToString() + "|" + colonyInfo.ID.ToString() + "|" + (object)faction1.FactionID), (int)((double)faction1.CivPopWeight * 100.0));
				sim.UI.SetText(sim.UI.Path(itemGlobalId, "gameMorale_human"), faction1.Morale.ToString());
				double num7 = (colonyInfo.CurrentStage == Kerberos.Sots.Data.ColonyStage.GemWorld ? Colony.GetMaxCivilianPop(sim.GameDatabase, planetInfo) * (double)sim.AssetDatabase.GemWorldCivMaxBonus : Colony.GetMaxCivilianPop(sim.GameDatabase, planetInfo)) * (double)colonyInfo.CivilianWeight * (double)faction1.CivPopWeight * (double)sim.AssetDatabase.GetFaction(sim.GameDatabase.GetPlayerFactionID(colonyInfo.PlayerID)).GetImmigrationPopBonusValueForFaction(sim.AssetDatabase.GetFaction(faction1.FactionID));
				sim.UI.SetText(sim.UI.Path(itemGlobalId, "popRatio"), (faction1.CivilianPop / 1000000.0).ToString("0.0") + "M / " + (num7 / 1000000.0).ToString("0.0") + "M");
			}
		}

		public static void SyncPanelColor(App game, string panelName, Vector3 color)
		{
			foreach (string str in new List<string>()
	  {
		"TLC",
		"TRC",
		"BLC",
		"BRC",
		"TC",
		"BC",
		"FILL"
	  })
				game.UI.SetPropertyColorNormalized(game.UI.Path(panelName, str), nameof(color), color);
		}

		internal static void SyncColonyDetailsControl(
		  GameSession sim,
		  string panelId,
		  int colonyId,
		  string sliderName = "")
		{
			ColonyInfo colonyInfo = sim.GameDatabase.GetColonyInfo(colonyId);
			PlayerInfo playerInfo = sim.GameDatabase.GetPlayerInfo(colonyInfo.PlayerID);
			StarSystemUI.SyncPanelColor(sim.App, sim.UI.Path(panelId, "pnlColonyStats"), playerInfo.PrimaryColor);
			StarSystemUI.SyncPanelColor(sim.App, sim.UI.Path(panelId, "pnlMoraleStats"), playerInfo.PrimaryColor);
			StarSystemUI.SyncPanelColor(sim.App, sim.UI.Path(panelId, "partConstructionSliders"), playerInfo.PrimaryColor);
			Faction faction1 = sim.AssetDatabase.GetFaction(sim.GameDatabase.GetPlayerFactionID(colonyInfo.PlayerID));
			PlanetInfo planetInfo = sim.GameDatabase.GetPlanetInfo(colonyInfo.OrbitalObjectID);
			double civilianPopulation1 = sim.GameDatabase.GetCivilianPopulation(colonyInfo.OrbitalObjectID, faction1.ID, faction1.HasSlaves());
			double slavePopulation = sim.GameDatabase.GetSlavePopulation(colonyInfo.OrbitalObjectID, faction1.ID);
			float economyRating = colonyInfo.EconomyRating;
			int colonySupportCost = (int)Colony.GetColonySupportCost(sim.AssetDatabase, sim.GameDatabase, colonyInfo);
			int num1 = (int)Colony.GetTaxRevenue(sim.App, colonyInfo) - colonySupportCost;
			int industrialOutput = (int)Colony.GetIndustrialOutput(sim, colonyInfo, planetInfo);
			float biosphereDelta = Colony.GetBiosphereDelta(sim, colonyInfo, planetInfo, 0.0);
			float num2 = (float)Colony.GetInfrastructureDelta(sim, colonyInfo, planetInfo) * 100f;
			float terraformingDelta = Colony.GetTerraformingDelta(sim, colonyInfo, planetInfo, 0.0);
			float shipConstResources = Colony.GetShipConstResources(sim, colonyInfo, planetInfo);
			float num3 = (float)((double)colonyInfo.OverdevelopProgress / (double)Colony.GetOverdevelopmentTarget(sim, planetInfo) * 100.0);
			int num4 = (double)colonyInfo.OverdevelopRate == 0.0 ? -1 : (int)Math.Ceiling(((double)Colony.GetOverdevelopmentTarget(sim, planetInfo) - (double)colonyInfo.OverdevelopProgress) / ((double)industrialOutput * (double)colonyInfo.OverdevelopRate));
			bool flag1 = (double)sim.GameDatabase.GetPlanetHazardRating(sim.LocalPlayer.ID, colonyInfo.OrbitalObjectID, false) != 0.0;
			bool flag2 = (double)planetInfo.Infrastructure < 1.0;
			bool flag3 = true;
			bool flag4 = Colony.CanBeOverdeveloped(sim.AssetDatabase, sim.GameDatabase, colonyInfo, planetInfo);
			int num5 = (int)Colony.CalcOverharvestRate(sim.AssetDatabase, sim.GameDatabase, colonyInfo, planetInfo);
			bool flag5 = sim.AssetDatabase.GetFaction(sim.GameDatabase.GetPlayerFactionID(colonyInfo.PlayerID)).HasSlaves();
			bool flag6 = colonyInfo.PlayerID == sim.LocalPlayer.ID;
			string propertyValue1 = num5.ToString("N0");
			string propertyValue2 = civilianPopulation1.ToString("N0");
			string propertyValue3 = slavePopulation.ToString("N0");
			string propertyValue4 = colonyInfo.ImperialPop.ToString("N0");
			string propertyValue5 = num1.ToString("N0");
			string propertyValue6 = ((int)((double)economyRating * 100.0)).ToString("N0");
			string propertyValue7 = colonySupportCost.ToString("N0");
			string propertyValue8 = industrialOutput.ToString("N0");
			string propertyValue9 = terraformingDelta.ToString("F2");
			int sliderValue1 = StarSystemDetailsUI.OutputRateToSliderValue(colonyInfo.TerraRate);
			string propertyValue10 = num2.ToString("F2");
			int sliderValue2 = StarSystemDetailsUI.OutputRateToSliderValue(colonyInfo.InfraRate);
			string propertyValue11 = shipConstResources.ToString("N0");
			int sliderValue3 = StarSystemDetailsUI.OutputRateToSliderValue(colonyInfo.ShipConRate);
			string propertyValue12 = string.Format(App.Localize("@UI_GAMECOLONYDETAILS_WORKRATECHANGE"), (object)Colony.GetSlaveIndustrialOutput(sim, colonyInfo).ToString("N0"), (object)Colony.GetKilledSlavePopulation(sim, colonyInfo, sim.GameDatabase.GetSlavePopulation(planetInfo.ID, sim.GameDatabase.GetPlayerFactionID(colonyInfo.PlayerID))).ToString("N0"));
			int sliderValue4 = StarSystemDetailsUI.OutputRateToSliderValue(colonyInfo.SlaveWorkRate);
			int sliderValue5 = StarSystemDetailsUI.OutputRateToSliderValue(colonyInfo.OverharvestRate);
			string propertyValue13 = string.Format("{0:0.00}% ({1} {2})", (object)num3, num4 == -1 ? (object)"∞" : (object)num4.ToString(), num4 == 1 ? (object)App.Localize("@UI_GENERAL_TURN") : (object)App.Localize("@UI_GENERAL_TURNS"));
			int sliderValue6 = StarSystemDetailsUI.OutputRateToSliderValue(colonyInfo.OverdevelopRate);
			float sliderValue7 = (float)StarSystemDetailsUI.OutputRateToSliderValue(colonyInfo.TradeRate);
			int num6 = (int)((double)colonyInfo.CivilianWeight * 100.0);
			string propertyValue14 = string.Format("{0:0}% ({1} {2})", (object)num6, (double)biosphereDelta > 0.0 ? (object)("+" + biosphereDelta.ToString()) : (object)biosphereDelta.ToString(), (object)App.Localize("@UI_GAMECOLONYDETAILS_BIOSPHERE"));
			sim.UI.SetVisible(sim.UI.Path(panelId, "pnlSlaves"), (flag5 ? 1 : 0) != 0);
			sim.UI.SetVisible(sim.UI.Path(panelId, "partSlavePop"), (flag5 ? 1 : 0) != 0);
			sim.UI.SetVisible(sim.UI.Path(panelId, "pnlMorale"), (!flag5 ? 1 : 0) != 0);
			sim.UI.SetVisible(sim.UI.Path(panelId, "partCivSlider"), (!flag5 ? 1 : 0) != 0);
			sim.UI.SetVisible(sim.UI.Path(panelId, "partHardenedStructure"), (sim.GameDatabase.GetStratModifier<bool>(StratModifiers.AllowHardenedStructures, colonyInfo.PlayerID) ? 1 : 0) != 0);
			sim.UI.SetVisible(sim.UI.Path(panelId, "lblHardenedStructure"), (sim.GameDatabase.GetStratModifier<bool>(StratModifiers.AllowHardenedStructures, colonyInfo.PlayerID) ? 1 : 0) != 0);
			sim.UI.SetChecked(sim.UI.Path(panelId, "partHardenedStructure"), (colonyInfo.isHardenedStructures ? 1 : 0) != 0);
			sim.UI.SetEnabled(sim.UI.Path(panelId, "partHardenedStructure"), (flag6 ? 1 : 0) != 0);
			sim.UI.SetVisible(sim.UI.Path(panelId, "partTerraSlider"), (flag1 ? 1 : 0) != 0);
			sim.UI.SetEnabled(sim.UI.Path(panelId, "partTerraSlider"), (flag6 ? 1 : 0) != 0);
			sim.UI.SetVisible(sim.UI.Path(panelId, "partInfraSlider"), (flag2 ? 1 : 0) != 0);
			sim.UI.SetEnabled(sim.UI.Path(panelId, "partInfraSlider"), (flag6 ? 1 : 0) != 0);
			sim.UI.SetVisible(sim.UI.Path(panelId, "partShipConSlider"), (flag3 ? 1 : 0) != 0);
			sim.UI.SetEnabled(sim.UI.Path(panelId, "partShipConSlider"), (flag6 ? 1 : 0) != 0);
			sim.UI.SetVisible(sim.UI.Path(panelId, "partOverDevelopment"), (flag4 ? 1 : 0) != 0);
			sim.UI.SetEnabled(sim.UI.Path(panelId, "partOverDevelopment"), (flag6 ? 1 : 0) != 0);
			sim.UI.SetEnabled(sim.UI.Path(panelId, "partTradeSlider"), (flag6 ? 1 : 0) != 0);
			sim.UI.SetEnabled(sim.UI.Path(panelId, "partCivSlider"), (flag6 ? 1 : 0) != 0);
			sim.UI.SetEnabled(sim.UI.Path(panelId, "partOverharvestSlider"), (flag6 ? 1 : 0) != 0);
			sim.UI.SetEnabled(sim.UI.Path(panelId, "partWorkRate"), (flag6 ? 1 : 0) != 0);
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partStage"), "text", App.Localize("@UI_COLONYSTAGE_" + colonyInfo.CurrentStage.ToString().ToUpper()));
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partOverharvestSlider", "right_label"), "text", propertyValue1);
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partCivSlider", "right_label"), "text", propertyValue14);
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partCivPop"), "value", propertyValue2);
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partSlavePop"), "value", propertyValue3);
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partImpPop"), "value", propertyValue4);
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partIncome"), "value", propertyValue5);
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partEconRating"), "value", propertyValue6);
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partLifeSupCost"), "value", propertyValue7);
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partIndustrial"), "value", propertyValue8);
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partTerraSlider", "right_label"), "text", propertyValue9);
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partInfraSlider", "right_label"), "text", propertyValue10);
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partShipConSlider", "right_label"), "text", propertyValue11);
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partOverDevelopment", "right_label"), "text", propertyValue13);
			sim.UI.SetPropertyString(sim.UI.Path(panelId, "partWorkRate", "right_label"), "text", propertyValue12);
			if (sliderName != "partTradeSlider")
			{
				sim.UI.SetSliderValue(sim.UI.Path(panelId, "partTradeSlider"), (int)sliderValue7);
				sim.UI.ClearSliderNotches(sim.UI.Path(panelId, "partTradeSlider"));
				foreach (double num7 in sim.GetTradeRatesForWholeExportsForColony(colonyInfo.ID))
					sim.UI.AddSliderNotch(sim.UI.Path(panelId, "partTradeSlider"), (int)Math.Ceiling(num7 * 100.0));
			}
			if (sliderName != "partCivSlider")
				sim.UI.SetSliderValue(sim.UI.Path(panelId, "partCivSlider"), num6);
			if (sliderName != "partTerraSlider")
				sim.UI.SetSliderValue(sim.UI.Path(panelId, "partTerraSlider"), sliderValue1);
			if (sliderName != "partInfraSlider")
				sim.UI.SetSliderValue(sim.UI.Path(panelId, "partInfraSlider"), sliderValue2);
			if (sliderName != "partShipConSlider")
				sim.UI.SetSliderValue(sim.UI.Path(panelId, "partShipConSlider"), sliderValue3);
			if (sliderName != "partOverDevelopment")
				sim.UI.SetSliderValue(sim.UI.Path(panelId, "partOverDevelopment"), sliderValue6);
			if (sliderName != "partOverharvestSlider")
			{
				sim.UI.SetSliderRange(sim.UI.Path(panelId, "partOverharvestSlider"), (int)((double)sim.GameDatabase.GetStratModifier<float>(StratModifiers.MinOverharvestRate, colonyInfo.PlayerID) * 100.0), 100);
				sim.UI.SetSliderValue(sim.UI.Path(panelId, "partOverharvestSlider"), sliderValue5);
			}
			if (sliderName != "partWorkRate")
				sim.UI.SetSliderValue(sim.UI.Path(panelId, "partWorkRate"), sliderValue4);
			foreach (Faction faction2 in sim.AssetDatabase.Factions)
			{
				if (faction2.IsPlayable)
				{
					sim.UI.SetText(sim.UI.Path(panelId, "MoraleBar.MoraleRow.gameMorale_" + faction2.Name + ".value"), "--");
					sim.UI.SetPropertyColor(sim.UI.Path(panelId, "MoraleBar.MoraleRow.gameMorale_" + faction2.Name + ".value"), "color", (float)byte.MaxValue, (float)byte.MaxValue, (float)byte.MaxValue);
					sim.UI.SetVisible(sim.UI.Path(panelId, "MoraleBar.MoraleRow.gameMorale_" + faction2.Name), false);
					sim.UI.SetTooltip(sim.UI.Path(panelId, "MoraleBar.MoraleRow.gameMorale_" + faction2.Name), faction2.Name);
				}
			}
			foreach (ColonyFactionInfo civilianPopulation2 in sim.GameDatabase.GetCivilianPopulations(colonyInfo.OrbitalObjectID))
			{
				string factionName = sim.GameDatabase.GetFactionName(civilianPopulation2.FactionID);
				sim.UI.SetText(sim.UI.Path(panelId, "MoraleBar.MoraleRow.gameMorale_" + factionName + ".value"), string.Format("{0:0}", (object)civilianPopulation2.Morale));
				sim.UI.SetVisible(sim.UI.Path(panelId, "MoraleBar.MoraleRow.gameMorale_" + factionName), true);
			}
			sim.UI.AutoSize(sim.UI.Path(panelId, "partConstructionSliders", "ColonyModifiers"));
			sim.UI.AutoSize(sim.UI.Path(panelId, "partConstructionSliders"));
			sim.UI.AutoSize(sim.UI.Path(panelId, "pnlMoraleStats"));
			ColonyHistoryData historyForColony = sim.GameDatabase.GetLastColonyHistoryForColony(colonyId);
			Vector3 vector3_1 = new Vector3((float)byte.MaxValue, 0.0f, 0.0f);
			Vector3 vector3_2 = new Vector3(0.0f, (float)byte.MaxValue, 0.0f);
			Vector3 vector3_3 = new Vector3((float)byte.MaxValue, (float)byte.MaxValue, (float)byte.MaxValue);
			sim.UI.SetPropertyColor(sim.UI.Path(panelId, "partCivPop"), "color", vector3_3);
			sim.UI.SetPropertyColor(sim.UI.Path(panelId, "partSlavePop"), "color", vector3_3);
			sim.UI.SetPropertyColor(sim.UI.Path(panelId, "partImpPop"), "color", vector3_3);
			sim.UI.SetPropertyColor(sim.UI.Path(panelId, "partIncome"), "color", vector3_3);
			sim.UI.SetPropertyColor(sim.UI.Path(panelId, "partEconRating"), "color", vector3_3);
			sim.UI.SetPropertyColor(sim.UI.Path(panelId, "partLifeSupCost"), "color", vector3_3);
			sim.UI.SetPropertyColor(sim.UI.Path(panelId, "partIndustrial"), "color", vector3_3);
			if (historyForColony != null)
			{
				if (historyForColony.civ_pop < civilianPopulation1)
					sim.UI.SetPropertyColor(sim.UI.Path(panelId, "partCivPop"), "color", vector3_2);
				else if (historyForColony.civ_pop > civilianPopulation1)
					sim.UI.SetPropertyColor(sim.UI.Path(panelId, "partCivPop"), "color", vector3_1);
				if (historyForColony.slave_pop < slavePopulation)
					sim.UI.SetPropertyColor(sim.UI.Path(panelId, "partSlavePop"), "color", vector3_2);
				else if (historyForColony.slave_pop > slavePopulation)
					sim.UI.SetPropertyColor(sim.UI.Path(panelId, "partSlavePop"), "color", vector3_1);
				if (historyForColony.imp_pop < colonyInfo.ImperialPop)
					sim.UI.SetPropertyColor(sim.UI.Path(panelId, "partImpPop"), "color", vector3_2);
				else if (historyForColony.imp_pop > colonyInfo.ImperialPop)
					sim.UI.SetPropertyColor(sim.UI.Path(panelId, "partImpPop"), "color", vector3_1);
				if (0 < num1)
					sim.UI.SetPropertyColor(sim.UI.Path(panelId, "partIncome"), "color", vector3_2);
				else if (0 > num1)
					sim.UI.SetPropertyColor(sim.UI.Path(panelId, "partIncome"), "color", vector3_1);
				if ((double)historyForColony.econ_rating < (double)economyRating)
					sim.UI.SetPropertyColor(sim.UI.Path(panelId, "partEconRating"), "color", vector3_2);
				else if ((double)historyForColony.econ_rating > (double)economyRating)
					sim.UI.SetPropertyColor(sim.UI.Path(panelId, "partEconRating"), "color", vector3_1);
				if (0 > colonySupportCost)
					sim.UI.SetPropertyColor(sim.UI.Path(panelId, "partLifeSupCost"), "color", vector3_2);
				else if (0 < colonySupportCost)
					sim.UI.SetPropertyColor(sim.UI.Path(panelId, "partLifeSupCost"), "color", vector3_1);
				if (historyForColony.industrial_output < industrialOutput)
					sim.UI.SetPropertyColor(sim.UI.Path(panelId, "partIndustrial"), "color", vector3_2);
				else if (historyForColony.industrial_output > industrialOutput)
					sim.UI.SetPropertyColor(sim.UI.Path(panelId, "partIndustrial"), "color", vector3_1);
			}
			List<ColonyFactionMoraleHistory> list = sim.GameDatabase.GetLastColonyMoraleHistoryForColony(colonyId).ToList<ColonyFactionMoraleHistory>();
			foreach (ColonyFactionInfo civilianPopulation2 in sim.GameDatabase.GetCivilianPopulations(colonyInfo.OrbitalObjectID))
			{
				ColonyFactionInfo civFaction = civilianPopulation2;
				ColonyFactionMoraleHistory factionMoraleHistory = list.FirstOrDefault<ColonyFactionMoraleHistory>((Func<ColonyFactionMoraleHistory, bool>)(x => x.factionid == civFaction.FactionID));
				if (factionMoraleHistory != null)
				{
					string factionName = sim.GameDatabase.GetFactionName(civFaction.FactionID);
					if (factionMoraleHistory.morale < civFaction.Morale)
						sim.UI.SetPropertyColor(sim.UI.Path(panelId, "MoraleBar.MoraleRow.gameMorale_" + factionName + ".value"), "color", vector3_2);
					else if (factionMoraleHistory.morale > civFaction.Morale)
						sim.UI.SetPropertyColor(sim.UI.Path(panelId, "MoraleBar.MoraleRow.gameMorale_" + factionName + ".value"), "color", vector3_1);
				}
			}
		}

		internal static void SyncSystemDetailsWidget(
		  App game,
		  string panelName,
		  int systemId,
		  bool showScreenNavButtons,
		  bool show = true)
		{
			if (systemId == 0)
			{
				game.UI.SetVisible(panelName, false);
			}
			else
			{
				game.UI.SetVisible(panelName, show);
				string panelId1 = game.UI.Path(panelName, "title");
				string panelId2 = game.UI.Path(panelName, "partStellarClass");
				string mapPanelId = game.UI.Path(panelName, "partMiniSystem");
				string panelId3 = game.UI.Path(panelName, "ScreenNavButtons");
				game.UI.SetVisible(panelId3, showScreenNavButtons);
				game.UI.SetVisible(game.UI.Path(panelId3, "gameBuildButton.active"), false);
				game.UI.SetText(game.UI.Path(panelId3, "numTurns"), "");
				if (systemId == 0)
				{
					game.UI.SetText(panelId2, string.Empty);
				}
				else
				{
					StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(systemId);
					game.UI.SetText(panelId1, string.Format("{0} SYSTEM", (object)starSystemInfo.Name.ToUpperInvariant()));
					game.UI.SetText(panelId2, starSystemInfo.StellarClass);
					int? systemOwningPlayer = game.GameDatabase.GetSystemOwningPlayer(systemId);
					if (systemOwningPlayer.HasValue && systemOwningPlayer.Value == game.LocalPlayer.ID)
					{
						IEnumerable<BuildOrderInfo> buildOrdersForSystem = game.GameDatabase.GetBuildOrdersForSystem(systemId);
						bool flag = false;
						foreach (BuildOrderInfo buildOrderInfo in buildOrdersForSystem)
						{
							if (game.GameDatabase.GetDesignInfo(buildOrderInfo.DesignID).PlayerID == game.LocalPlayer.ID)
							{
								flag = true;
								break;
							}
						}
						IEnumerable<RetrofitOrderInfo> retrofitOrdersForSystem = game.GameDatabase.GetRetrofitOrdersForSystem(systemId);
						foreach (RetrofitOrderInfo retrofitOrderInfo in retrofitOrdersForSystem)
						{
							if (game.GameDatabase.GetDesignInfo(retrofitOrderInfo.DesignID).PlayerID == game.LocalPlayer.ID)
							{
								flag = true;
								break;
							}
						}
						double num1 = 0.0;
						double num2 = 0.0;
						float num3 = 0.0f;
						List<ColonyInfo> colonyInfoList = new List<ColonyInfo>();
						foreach (int orbitalObjectID in game.GameDatabase.GetStarSystemPlanets(systemId).ToList<int>())
						{
							ColonyInfo colonyInfoForPlanet = game.GameDatabase.GetColonyInfoForPlanet(orbitalObjectID);
							if (colonyInfoForPlanet != null)
							{
								num1 += Colony.GetTaxRevenue(game, colonyInfoForPlanet);
								num2 += (double)colonyInfoForPlanet.ShipConRate;
								num3 += Colony.GetConstructionPoints(game.Game, colonyInfoForPlanet);
								colonyInfoList.Add(colonyInfoForPlanet);
							}
						}
						float num4 = num3 * game.Game.GetStationBuildModifierForSystem(systemId, game.LocalPlayer.ID);
						int num5 = 0;
						int num6 = 0;
						foreach (BuildOrderInfo buildOrderInfo in buildOrdersForSystem)
						{
							num6 += buildOrderInfo.Progress;
							num5 += buildOrderInfo.ProductionTarget;
						}
						int num7 = (int)Math.Ceiling((double)(num5 - num6) / (double)num4);
						int num8 = 0;
						if (retrofitOrdersForSystem.Count<RetrofitOrderInfo>() > 0)
						{
							ShipInfo shipInfo = game.GameDatabase.GetShipInfo(retrofitOrdersForSystem.First<RetrofitOrderInfo>().ShipID, true);
							num8 = (int)Kerberos.Sots.StarFleet.StarFleet.GetTimeRequiredToRetrofit(game, shipInfo, retrofitOrdersForSystem.Count<RetrofitOrderInfo>());
						}
						game.UI.SetVisible(game.UI.Path(panelId3, "gameBuildButton.active"), (flag ? 1 : 0) != 0);
						if (num7 > 0 || num8 > 0)
							game.UI.SetText(game.UI.Path(panelId3, "numTurns"), num8 <= num7 ? num7.ToString() : num8.ToString());
						else
							game.UI.SetText(game.UI.Path(panelId3, "numTurns"), "");
					}
				}
				bool flag1 = game.GameDatabase.IsSurveyed(game.LocalPlayer.ID, systemId);
				game.UI.SetEnabled("gameSystemButton", flag1);
				game.UI.SetVisible(game.UI.Path(panelName, "SysBorder_Unsurveyed"), (!flag1 ? 1 : 0) != 0);
				game.UI.SetVisible(game.UI.Path(panelName, "SysBorder_Surveyed"), (flag1 ? 1 : 0) != 0);
				game.UI.SetVisible(game.UI.Path(panelName, "UnSurveyed"), (!flag1 ? 1 : 0) != 0);
				game.UI.SetVisible(game.UI.Path(panelName, "Surveyed"), (flag1 ? 1 : 0) != 0);
				game.UI.SetVisible(game.UI.Path(panelName, "btnSystemOpen"), (game.GameDatabase.GetPlayerColonySystemIDs(game.LocalPlayer.ID).Contains<int>(systemId) ? 1 : 0) != 0);
				game.UI.SetChecked(game.UI.Path(panelName, "btnSystemOpen"), (game.GameDatabase.GetStarSystemInfo(systemId).IsOpen ? 1 : 0) != 0);
				StarSystemMapUI.Sync(game, systemId, mapPanelId, true);
				game.UI.AutoSize(panelName);
			}
		}

		internal static void SyncPlanetDetailsWidget(
		  GameSession game,
		  string panelName,
		  int systemId,
		  int orbitId,
		  IGameObject planetViewObject,
		  PlanetView planetView)
		{
			game.UI.SetVisible(game.UI.Path(panelName, "gamePlanetDetails"), false);
			game.UI.SetVisible(game.UI.Path(panelName, "gameMoonDetails"), false);
			game.UI.SetVisible(game.UI.Path(panelName, "gameGasGiantDetails"), false);
			game.UI.SetVisible(game.UI.Path(panelName, "gameStarDetails"), false);
			if (planetView != null)
				planetView.PostSetProp("Planet", planetViewObject != null ? planetViewObject.ObjectID : 0);
			if (systemId == 0 || orbitId == -2)
				return;
			PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(orbitId);
			if (planetInfo == null)
			{
				if (game.GameDatabase.GetLargeAsteroidInfo(orbitId) == null)
				{
					game.UI.SetVisible("gameStarDetails", true);
					StarSystemUI.SyncStarDetailsControl(game, "gameStarDetails", systemId);
					if (planetView != null)
						game.UI.Send((object)"SetGameObject", (object)game.UI.Path("gameStarDetails", "Planet_panel"), (object)planetView.ObjectID);
					bool flag = game.GameDatabase.IsSurveyed(game.LocalPlayer.ID, systemId);
					game.UI.SetVisible(game.UI.Path(panelName, "gameStarDetails.StarBorder_UnSurveyed"), (!flag ? 1 : 0) != 0);
					game.UI.SetVisible(game.UI.Path(panelName, "gameStarDetails.StarBorder_Surveyed"), (flag ? 1 : 0) != 0);
					game.UI.SetVisible(game.UI.Path(panelName, "gameStarDetails.Star_Unsurveyed"), (!flag ? 1 : 0) != 0);
					game.UI.SetVisible(game.UI.Path(panelName, "gameStarDetails.Star_Surveyed"), (flag ? 1 : 0) != 0);
				}
				else
				{
					game.UI.SetVisible("gameMoonDetails", true);
					StarSystemUI.SyncAsteroidDetailsControl(game, "gameMoonDetails", orbitId);
				}
			}
			else if (StellarBodyTypes.IsTerrestrial(planetInfo.Type.ToLowerInvariant()))
			{
				game.UI.SetVisible("gamePlanetDetails", true);
				StarSystemUI.SyncPlanetDetailsControl(game, "gamePlanetDetails", orbitId);
				if (planetView == null)
					return;
				game.UI.Send((object)"SetGameObject", (object)game.UI.Path("gamePlanetDetails", "Planet_panel"), (object)planetView.ObjectID);
			}
			else if (planetInfo.Type.ToLowerInvariant() == StellarBodyTypes.Gaseous)
			{
				game.UI.SetVisible("gameGasGiantDetails", true);
				StarSystemUI.SyncGasGiantDetailsControl(game, "gameGasGiantDetails", orbitId);
				if (planetView == null)
					return;
				game.UI.Send((object)"SetGameObject", (object)game.UI.Path("gameGasGiantDetails", "Planet_panel"), (object)planetView.ObjectID);
			}
			else
			{
				if (!(planetInfo.Type.ToLowerInvariant() == StellarBodyTypes.Barren))
					return;
				game.UI.SetVisible("gameMoonDetails", true);
				StarSystemUI.SyncMoonDetailsControl(game, "gameMoonDetails", orbitId);
				if (planetView == null)
					return;
				game.UI.Send((object)"SetGameObject", (object)game.UI.Path("gameMoonDetails", "Planet_panel"), (object)planetView.ObjectID);
			}
		}

		internal static void SyncColonyDetailsWidget(
		  GameSession sim,
		  string panelName,
		  int planetId,
		  string sliderName = "")
		{
			AIColonyIntel colonyIntelForPlanet = sim.GameDatabase.GetColonyIntelForPlanet(sim.LocalPlayer.ID, planetId);
			if (colonyIntelForPlanet == null || colonyIntelForPlanet.OwningPlayerID == 0 || !colonyIntelForPlanet.ColonyID.HasValue)
			{
				sim.UI.SetVisible(panelName, false);
			}
			else
			{
				bool flag = false;
				if (colonyIntelForPlanet != null)
				{
					PlayerInfo playerInfo = sim.GameDatabase.GetPlayerInfo(colonyIntelForPlanet.OwningPlayerID);
					if (!playerInfo.isStandardPlayer && playerInfo.includeInDiplomacy)
						flag = true;
				}
				if (colonyIntelForPlanet != null && colonyIntelForPlanet.OwningPlayerID == sim.LocalPlayer.ID || flag)
				{
					sim.UI.SetVisible(panelName, true);
					StarSystemUI.SyncColonyDetailsControl(sim, "colonyControl", colonyIntelForPlanet.ColonyID.Value, sliderName);
					sim.UI.SetEnabled(sim.UI.Path(sliderName, "btnAbandon"), (!flag ? 1 : 0) != 0);
					sim.UI.SetVisible(sim.UI.Path(sliderName, "btnAbandon"), (!flag ? 1 : 0) != 0);
				}
				else
					sim.UI.SetVisible(panelName, false);
			}
		}

		internal static void SyncPlanetListWidget(
		  GameSession sim,
		  string panelName,
		  IEnumerable<int> orbitalIds)
		{
			if (orbitalIds.Count<int>() == 0)
			{
				sim.UI.SetVisible(panelName, false);
			}
			else
			{
				sim.UI.SetVisible(panelName, true);
				FleetUI.SyncPlanetListControl(sim, "gamePlanetList", orbitalIds);
			}
		}
	}
}
