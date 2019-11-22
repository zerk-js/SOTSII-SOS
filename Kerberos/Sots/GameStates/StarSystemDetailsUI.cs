// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.StarSystemDetailsUI
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.GameStates
{
	internal static class StarSystemDetailsUI
	{
		public const string UITradeSlider = "partTradeSlider";
		public const string UITerraSlider = "partTerraSlider";
		public const string UIInfraSlider = "partInfraSlider";
		public const string UIOverDevSlider = "partOverDevelopment";
		public const string UIShipConSlider = "partShipConSlider";
		public const string UIOverharvestSlider = "partOverharvestSlider";
		public const string UICivPopulationSlider = "partCivSlider";
		public const string UISlaveWorkSlider = "partWorkRate";

		public static int StarItemID
		{
			get
			{
				return -1;
			}
		}

		public static string GetInfraString(float infra)
		{
			return ((int)((double)infra * 100.0)).ToString();
		}

		public static string GetHazardString(float hazard)
		{
			return ((int)Math.Abs(hazard)).ToString();
		}

		public static string GetSizeString(float size)
		{
			if ((double)size >= 1.0)
				return string.Format("{0}", (object)(int)size);
			return string.Format("{0:N}", (object)size);
		}

		public static IEnumerable<int> CollectPlanetListItemsForSupportMission(
		  App game,
		  int systemId)
		{
			IEnumerable<int> planetIds = game.GameDatabase.GetStarSystemPlanets(systemId);
			foreach (int planetId in planetIds)
			{
				if (game.GameDatabase.CanSupportPlanet(game.LocalPlayer.ID, planetId))
					yield return planetId;
			}
		}

		public static IEnumerable<int> CollectPlanetListItemsForInvasionMission(
		  App game,
		  int systemId)
		{
			IEnumerable<int> planetIds = game.GameDatabase.GetStarSystemPlanets(systemId);
			foreach (int planetId in planetIds)
			{
				if (game.GameDatabase.CanInvadePlanet(game.LocalPlayer.ID, planetId))
					yield return planetId;
			}
		}

		public static IEnumerable<int> CollectPlanetListItemsForColonizeMission(
		  App game,
		  int systemId,
		  int playerId)
		{
			IEnumerable<int> planetIds = game.GameDatabase.GetStarSystemPlanets(systemId);
			foreach (int num in planetIds)
			{
				if (game.GameDatabase.CanColonizePlanet(playerId, num, game.GameDatabase.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, playerId)))
				{
					IEnumerable<MissionInfo> infos = game.GameDatabase.GetMissionsByPlanetDest(num);
					bool cool = true;
					foreach (MissionInfo missionInfo in infos)
					{
						if (missionInfo.Type == MissionType.COLONIZATION)
						{
							cool = false;
							break;
						}
					}
					if ((double)game.GameDatabase.GetPlanetHazardRating(playerId, num, false) > (double)game.GameDatabase.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, playerId))
						cool = false;
					if (cool || game.AssetDatabase.GetFaction(game.GameDatabase.GetPlayerInfo(playerId).FactionID).Name == "loa")
						yield return num;
				}
			}
		}

		public static IEnumerable<int> CollectPlanetListItemsForEvacuateMission(
		  App game,
		  int systemId,
		  int playerId)
		{
			List<int> planetIds = game.GameDatabase.GetStarSystemPlanets(systemId).ToList<int>();
			foreach (int num in planetIds)
			{
				ColonyInfo ci = game.GameDatabase.GetColonyInfoForPlanet(num);
				if (ci != null && ci.PlayerID == playerId && game.GameDatabase.GetCivilianPopulation(num, 0, false) > 0.0)
					yield return num;
			}
		}

		public static IEnumerable<int> CollectPlanetListItemsForConstructionMission(
		  App game,
		  int systemId)
		{
			IEnumerable<PlanetInfo> planets = game.GameDatabase.GetPlanetInfosOrbitingStar(systemId);
			foreach (PlanetInfo planetInfo in planets)
				yield return planetInfo.ID;
			yield return StarSystemDetailsUI.StarItemID;
		}

		public static bool IsOutputRateSlider(string panelName)
		{
			if (!(panelName == "partTradeSlider") && !(panelName == "partTerraSlider") && (!(panelName == "partInfraSlider") && !(panelName == "partOverDevelopment")))
				return panelName == "partShipConSlider";
			return true;
		}

		public static int OutputRateToSliderValue(float value)
		{
			return (int)Math.Ceiling((double)value * 100.0);
		}

		public static float SliderValueToOutputRate(int value)
		{
			return (float)value / 100f;
		}

		public static void SetOutputRate(App game, int orbitalId, string panelName, string valueStr)
		{
			PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(orbitalId);
			ColonyInfo colonyInfoForPlanet = game.GameDatabase.GetColonyInfoForPlanet(orbitalId);
			Colony.OutputRate rate = Colony.OutputRate.Trade;
			if (panelName == "partInfraSlider")
				rate = Colony.OutputRate.Infra;
			else if (panelName == "partTerraSlider")
				rate = Colony.OutputRate.Terra;
			else if (panelName == "partShipConSlider")
				rate = Colony.OutputRate.ShipCon;
			else if (panelName == "partOverDevelopment")
				rate = Colony.OutputRate.OverDev;
			float outputRate = StarSystemDetailsUI.SliderValueToOutputRate(int.Parse(valueStr));
			Colony.SetOutputRate(game.GameDatabase, game.AssetDatabase, ref colonyInfoForPlanet, planetInfo, rate, outputRate);
			game.GameDatabase.UpdateColony(colonyInfoForPlanet);
		}

		public static void SetOutputRateNew(
		  App game,
		  int orbitalId,
		  string panelName,
		  string valueStr)
		{
			PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(orbitalId);
			ColonyInfo colonyInfoForPlanet = game.GameDatabase.GetColonyInfoForPlanet(orbitalId);
			Colony.OutputRate rate = Colony.OutputRate.Trade;
			if (panelName.Contains("partInfraSlider"))
				rate = Colony.OutputRate.Infra;
			else if (panelName.Contains("partTerraSlider"))
				rate = Colony.OutputRate.Terra;
			else if (panelName.Contains("partShipConSlider"))
				rate = Colony.OutputRate.ShipCon;
			else if (panelName.Contains("partOverDevelopment"))
				rate = Colony.OutputRate.OverDev;
			float outputRate = StarSystemDetailsUI.SliderValueToOutputRate(int.Parse(valueStr));
			Colony.SetOutputRate(game.GameDatabase, game.AssetDatabase, ref colonyInfoForPlanet, planetInfo, rate, outputRate);
			game.GameDatabase.UpdateColony(colonyInfoForPlanet);
		}

		private enum FleetDetail
		{
			Admiral,
			SpeedRange,
			ShipCounts,
			Destination,
			Location,
		}
	}
}
