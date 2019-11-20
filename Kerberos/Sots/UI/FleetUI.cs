// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.FleetUI
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.StarMapElements;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class FleetUI
	{
		public const string UIFleetItemDetails = "partFleetDetails";
		public const string UIFleetItemEscortShips = "EscortShipsList";
		public const string UIFleetItemEssentialShips = "EssentialShipsList";
		public const string UIPlanetsTab = "planetsTab";
		public const string UIFleetsTab = "fleetsTab";
		public const string UIFleetList = "partSystemFleets";
		public const string UIPlanetList = "partSystemPlanets";
		public const string UIFleetSelectedControl = "gameFleetList";
		public static bool ShowFleetListDefault;

		public static int StarItemID
		{
			get
			{
				return -1;
			}
		}

		public static void SyncSelectablePlanetListControl(
		  GameSession game,
		  string planetListId,
		  string subsection,
		  string name,
		  Vector4 color,
		  bool setSelColor)
		{
			string globalId = game.UI.GetGlobalID(planetListId + "." + subsection);
			game.UI.SetPropertyString(globalId + ".listitem_name", "text", name);
			if (setSelColor)
			{
				Vector4 vector4 = new Vector4((float)byte.MaxValue, (float)byte.MaxValue, (float)byte.MaxValue, 100f);
				if ((double)color.W > 0.0)
					vector4 = new Vector4(color.X * 0.5f, color.Y * 0.5f, color.Z * 0.5f, color.W);
				game.UI.SetPropertyColor(globalId + ".LC;" + globalId + ".RC;" + globalId + ".BG", nameof(color), vector4);
			}
			game.UI.SetPropertyColor(globalId + ".colony_insert.LC;" + globalId + ".colony_insert.RC;" + globalId + ".colony_insert.BG", nameof(color), color);
		}

		public static void SyncPlanetItemControl(
		  GameSession game,
		  string panelName,
		  OrbitalObjectInfo orbital)
		{
			PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(orbital.ID);
			if (planetInfo == null)
				return;
			game.GameDatabase.GetMoons(orbital.ID);
			string propertyValue = App.Localize("@UI_PLANET_TYPE_" + planetInfo.Type.ToUpperInvariant());
			Vector4 color = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
			AIColonyIntel colonyIntelForPlanet = game.GameDatabase.GetColonyIntelForPlanet(game.LocalPlayer.ID, planetInfo.ID);
			if (colonyIntelForPlanet != null)
			{
				PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(colonyIntelForPlanet.OwningPlayerID);
				color.X = playerInfo.PrimaryColor.X * (float)byte.MaxValue;
				color.Y = playerInfo.PrimaryColor.Y * (float)byte.MaxValue;
				color.Z = playerInfo.PrimaryColor.Z * (float)byte.MaxValue;
				color.W = (float)byte.MaxValue;
			}
			game.UI.AddItem(panelName, string.Empty, orbital.ID, string.Empty);
			string itemGlobalId = game.UI.GetItemGlobalID(panelName, string.Empty, orbital.ID, string.Empty);
			string panelId = game.UI.Path(itemGlobalId, "expand_button");
			game.UI.SetVisible(panelId, false);
			bool flag = game.GameDatabase.CanColonizePlanet(game.LocalPlayer.ID, planetInfo.ID, game.GameDatabase.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, game.LocalPlayer.ID)) || colonyIntelForPlanet != null && colonyIntelForPlanet.PlayerID == game.LocalPlayer.ID;
			game.UI.SetVisible(game.UI.Path(itemGlobalId, "header_idle.idle.h_good"), (flag ? 1 : 0) != 0);
			game.UI.SetVisible(game.UI.Path(itemGlobalId, "header_idle.idle.h_bad"), (!flag ? 1 : 0) != 0);
			game.UI.SetVisible(game.UI.Path(itemGlobalId, "header_idle.mouse_over.h_good"), (flag ? 1 : 0) != 0);
			game.UI.SetVisible(game.UI.Path(itemGlobalId, "header_idle.mouse_over.h_bad"), (!flag ? 1 : 0) != 0);
			game.UI.SetVisible(game.UI.Path(itemGlobalId, "header_sel.idle.h_good"), (flag ? 1 : 0) != 0);
			game.UI.SetVisible(game.UI.Path(itemGlobalId, "header_sel.idle.h_bad"), (!flag ? 1 : 0) != 0);
			game.UI.SetVisible(game.UI.Path(itemGlobalId, "header_sel.mouse_over.h_good"), (flag ? 1 : 0) != 0);
			game.UI.SetVisible(game.UI.Path(itemGlobalId, "header_sel.mouse_over.h_bad"), (!flag ? 1 : 0) != 0);
			FleetUI.SyncSelectablePlanetListControl(game, itemGlobalId, "header_idle.idle", orbital.Name, color, false);
			FleetUI.SyncSelectablePlanetListControl(game, itemGlobalId, "header_idle.mouse_over", orbital.Name, color, false);
			FleetUI.SyncSelectablePlanetListControl(game, itemGlobalId, "header_sel.idle", orbital.Name, color, true);
			FleetUI.SyncSelectablePlanetListControl(game, itemGlobalId, "header_sel.mouse_over", orbital.Name, color, true);
			game.UI.SetPropertyString(game.UI.Path(itemGlobalId, "planetItemPlanetType"), "text", propertyValue);
			game.UI.AutoSize(game.UI.Path(itemGlobalId, "expanded"));
		}

		public static void SyncExistingPlanet(
		  GameSession game,
		  string panelName,
		  OrbitalObjectInfo orbital)
		{
			PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(orbital.ID);
			if (planetInfo == null)
				return;
			game.GameDatabase.GetMoons(orbital.ID);
			string propertyValue = App.Localize("@UI_PLANET_TYPE_" + planetInfo.Type.ToUpperInvariant());
			Vector4 color = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
			AIColonyIntel colonyIntelForPlanet = game.GameDatabase.GetColonyIntelForPlanet(game.LocalPlayer.ID, planetInfo.ID);
			if (colonyIntelForPlanet != null)
			{
				PlayerInfo playerInfo = game.GameDatabase.GetPlayerInfo(colonyIntelForPlanet.PlayerID);
				color.X = playerInfo.PrimaryColor.X * (float)byte.MaxValue;
				color.Y = playerInfo.PrimaryColor.Y * (float)byte.MaxValue;
				color.Z = playerInfo.PrimaryColor.Z * (float)byte.MaxValue;
				color.W = (float)byte.MaxValue;
			}
			string planetListId = panelName;
			bool flag = game.GameDatabase.CanColonizePlanet(game.LocalPlayer.ID, planetInfo.ID, game.GameDatabase.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, game.LocalPlayer.ID)) || colonyIntelForPlanet != null && colonyIntelForPlanet.OwningPlayerID == game.LocalPlayer.ID;
			game.UI.SetVisible(game.UI.Path(planetListId, "header_idle.idle.h_good"), (flag ? 1 : 0) != 0);
			game.UI.SetVisible(game.UI.Path(planetListId, "header_idle.idle.h_bad"), (!flag ? 1 : 0) != 0);
			game.UI.SetVisible(game.UI.Path(planetListId, "header_idle.mouse_over.h_good"), (flag ? 1 : 0) != 0);
			game.UI.SetVisible(game.UI.Path(planetListId, "header_idle.mouse_over.h_bad"), (!flag ? 1 : 0) != 0);
			game.UI.SetVisible(game.UI.Path(planetListId, "header_sel.idle.h_good"), (flag ? 1 : 0) != 0);
			game.UI.SetVisible(game.UI.Path(planetListId, "header_sel.idle.h_bad"), (!flag ? 1 : 0) != 0);
			game.UI.SetVisible(game.UI.Path(planetListId, "header_sel.mouse_over.h_good"), (flag ? 1 : 0) != 0);
			game.UI.SetVisible(game.UI.Path(planetListId, "header_sel.mouse_over.h_bad"), (!flag ? 1 : 0) != 0);
			FleetUI.SyncSelectablePlanetListControl(game, planetListId, "header_idle.idle", orbital.Name, color, false);
			FleetUI.SyncSelectablePlanetListControl(game, planetListId, "header_idle.mouse_over", orbital.Name, color, false);
			FleetUI.SyncSelectablePlanetListControl(game, planetListId, "header_sel.idle", orbital.Name, color, true);
			FleetUI.SyncSelectablePlanetListControl(game, planetListId, "header_sel.mouse_over", orbital.Name, color, true);
			game.UI.SetPropertyString(game.UI.Path(planetListId, "planetItemPlanetType"), "text", propertyValue);
			game.UI.AutoSize(game.UI.Path(planetListId, "expanded"));
		}

		public static void SyncPlanetListControl(
		  GameSession game,
		  string panelName,
		  IEnumerable<int> orbitalObjects)
		{
			game.UI.ClearItems(panelName);
			foreach (int orbitalObject in orbitalObjects)
			{
				OrbitalObjectInfo orbitalObjectInfo = game.GameDatabase.GetOrbitalObjectInfo(orbitalObject);
				FleetUI.SyncPlanetItemControl(game, panelName, orbitalObjectInfo);
			}
			game.UI.AutoSize(panelName);
		}

		public static void SyncPlanetListControl(
		  GameSession game,
		  string panelName,
		  int systemId,
		  IEnumerable<int> orbitalObjects)
		{
			bool flag = game.GameDatabase.IsSurveyed(game.LocalPlayer.ID, systemId);
			game.UI.ClearItems(panelName);
			List<OrbitalObjectInfo> list = game.GameDatabase.GetStarSystemOrbitalObjectInfos(systemId).ToList<OrbitalObjectInfo>();
			Dictionary<int, float> DistanceMap = new Dictionary<int, float>();
			foreach (OrbitalObjectInfo orbitalObjectInfo in list)
				DistanceMap.Add(orbitalObjectInfo.ID, orbitalObjectInfo.OrbitalPath.Scale.Y);
			list.Sort((Comparison<OrbitalObjectInfo>)((x, y) =>
		   {
			   int num = (x.ParentID.HasValue ? DistanceMap[x.ParentID.Value] : x.OrbitalPath.Scale.Y).CompareTo(y.ParentID.HasValue ? DistanceMap[y.ParentID.Value] : y.OrbitalPath.Scale.Y);
			   if (num == 0)
			   {
				   if (x.ParentID.HasValue && y.ParentID.HasValue)
					   return x.OrbitalPath.Scale.Y.CompareTo(y.OrbitalPath.Scale.Y);
				   if (x.ParentID.HasValue)
					   return 1;
				   if (y.ParentID.HasValue)
					   return -1;
			   }
			   return num;
		   }));
			if (orbitalObjects.Contains<int>(FleetUI.StarItemID))
			{
				Vector4 color = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
				StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(systemId);
				game.UI.AddItem(panelName, string.Empty, FleetUI.StarItemID, string.Empty);
				string itemGlobalId = game.UI.GetItemGlobalID(panelName, string.Empty, FleetUI.StarItemID, string.Empty);
				FleetUI.SyncSelectablePlanetListControl(game, itemGlobalId, "header_idle.idle", starSystemInfo.Name, color, false);
				FleetUI.SyncSelectablePlanetListControl(game, itemGlobalId, "header_idle.mouse_over", starSystemInfo.Name, color, false);
				FleetUI.SyncSelectablePlanetListControl(game, itemGlobalId, "header_sel.idle", starSystemInfo.Name, color, true);
				FleetUI.SyncSelectablePlanetListControl(game, itemGlobalId, "header_sel.mouse_over", starSystemInfo.Name, color, true);
				string panelId = game.UI.Path(panelName, itemGlobalId, "expand_button");
				game.UI.SetVisible(panelId, false);
			}
			if (flag)
			{
				foreach (OrbitalObjectInfo orbital in list)
				{
					if (orbital.ID != FleetUI.StarItemID && orbitalObjects.Contains<int>(orbital.ID))
						FleetUI.SyncPlanetItemControl(game, panelName, orbital);
				}
			}
			game.UI.AutoSize(panelName);
		}

		private static string GetShipClassBadge(ShipClass shipClass, string className)
		{
			string str = "CR";
			switch (shipClass)
			{
				case ShipClass.Cruiser:
					if (className == "Command")
					{
						str = "CR_CMD";
						break;
					}
					break;
				case ShipClass.Dreadnought:
					str = !(className == "Command") ? "DN" : "DN_CMD";
					break;
				case ShipClass.Leviathan:
					str = "LV_CMD";
					break;
			}
			return str;
		}

		public static void SyncShipsCount(
		  App game,
		  string panelName,
		  FleetInfo fleet,
		  List<ShipItem> listships,
		  bool essentials)
		{
		}

		public static void SyncFleetAndPlanetListWidget(
		  GameSession game,
		  string panelName,
		  int systemId,
		  bool show = true)
		{
			if (systemId == 0)
			{
				game.UI.SetVisible(panelName, false);
			}
			else
			{
				bool flag1 = game.GameDatabase.IsSurveyed(game.LocalPlayer.ID, systemId);
				List<FleetInfo> list1 = game.GameDatabase.GetFleetInfoBySystemID(systemId, FleetType.FL_NORMAL | FleetType.FL_RESERVE | FleetType.FL_GATE | FleetType.FL_ACCELERATOR).ToList<FleetInfo>();
				List<StationInfo> list2 = game.GameDatabase.GetStationForSystemAndPlayer(systemId, game.LocalPlayer.ID).ToList<StationInfo>();
				bool flag2 = true;
				if (!game.SystemHasPlayerColony(systemId, game.LocalPlayer.ID) && !StarMap.IsInRange(game.GameDatabase, game.LocalPlayer.ID, game.GameDatabase.GetStarSystemInfo(systemId), (Dictionary<int, List<ShipInfo>>)null))
					flag2 = false;
				List<int> list3 = game.GameDatabase.GetStarSystemPlanets(systemId).ToList<int>();
				if (list1.Count == 0 && list2.Count == 0 && !flag1 || !flag2 && !flag1)
				{
					game.UI.SetVisible(panelName, false);
				}
				else
				{
					bool flag3 = FleetUI.ShowFleetListDefault;
					bool flag4 = (list1.Count<FleetInfo>() != 0 && flag2 || list2.Count != 0) && show;
					bool flag5 = list3.Count<int>() != 0 && flag1 && show;
					if (flag3 && !flag4)
						flag3 = false;
					else if (!flag3 && !flag5)
						flag3 = true;
					game.UI.SetEnabled("planetsTab", flag5);
					game.UI.SetVisible("partSystemPlanets", flag5 && !flag3);
					game.UI.SetChecked("planetsTab", flag5 && !flag3);
					game.UI.SetEnabled("fleetsTab", flag4);
					game.UI.SetChecked("fleetsTab", flag4 && flag3);
					game.UI.SetVisible("partSystemFleets", flag4 && flag3);
					game.UI.SetVisible(panelName, show);
					FleetUI.SyncPlanetListControl(game, "partSystemPlanets", systemId, (IEnumerable<int>)list3);
				}
			}
		}

		public static void SyncFleetAndPlanetListWidget(
		  GameSession game,
		  string panelName,
		  int systemId,
		  IEnumerable<FleetInfo> fleets,
		  IEnumerable<int> orbitalObjectIds,
		  bool show = true)
		{
			if (systemId == 0 || fleets.Count<FleetInfo>() == 0 && orbitalObjectIds.Count<int>() == 0)
			{
				game.UI.SetVisible(panelName, false);
			}
			else
			{
				if (fleets.Count<FleetInfo>() == 0)
					game.UI.SetVisible("fleetsTab", false);
				else
					game.UI.SetVisible("fleetsTab", show);
				if (orbitalObjectIds.Count<int>() == 0)
				{
					game.UI.SetVisible("planetsTab", false);
					game.UI.SetPropertyInt("fleetsTab", "left", 30);
				}
				else
				{
					game.UI.SetVisible("planetsTab", show);
					game.UI.SetPropertyInt("fleetsTab", "left", 115);
				}
				game.UI.AutoSize("fleetsTab");
				game.UI.SetVisible(panelName, show);
				FleetUI.SyncPlanetListControl(game, "partSystemPlanets", systemId, orbitalObjectIds);
			}
		}

		public static bool HandleFleetAndPlanetWidgetInput(
		  App game,
		  string panelName,
		  string buttonPressed)
		{
			if (buttonPressed == "planetsTab")
			{
				FleetUI.ShowFleetListDefault = false;
				game.UI.SetVisible(game.UI.Path(panelName, "partSystemFleets"), false);
				game.UI.SetVisible(game.UI.Path(panelName, "partSystemPlanets"), true);
				game.UI.SetChecked(game.UI.Path(panelName, "planetsTab"), true);
				game.UI.SetChecked(game.UI.Path(panelName, "fleetsTab"), false);
				return true;
			}
			if (!(buttonPressed == "fleetsTab"))
				return false;
			FleetUI.ShowFleetListDefault = true;
			game.UI.SetVisible(game.UI.Path(panelName, "partSystemFleets"), true);
			game.UI.SetVisible(game.UI.Path(panelName, "partSystemPlanets"), false);
			game.UI.SetChecked(game.UI.Path(panelName, "planetsTab"), false);
			game.UI.SetChecked(game.UI.Path(panelName, "fleetsTab"), true);
			return true;
		}
	}
}
