// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.StarSystemMapUI
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.StarMapElements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.UI
{
	public class StarSystemMapUI
	{
		public const string SystemMapSpritePlanetMoon = "sysmap_planetmoon";
		public const string SystemMapSpritePlanet = "sysmap_planet";
		public const string SystemMapSpriteGasGiantPlanet = "sysmap_gasgiantplanet";
		public const string SystemMapSpriteGasGiantMoon = "sysmap_gasgiantmoon";
		public const string SystemMapSpriteGasGiantRings = "sysmap_gasgiantrings";
		public const string SystemMapSpriteGasGiant = "sysmap_gasgiant";

		internal static string SelectIcon(
		  PlanetInfo planetInfo,
		  IEnumerable<OrbitalObjectInfo> orbitalObjectInfos,
		  IEnumerable<PlanetInfo> planetInfos)
		{
			if (StarSystemMapUI.IsTerrestrialPlanet(planetInfo))
				return StarSystemMapUI.HasMoons(planetInfo, orbitalObjectInfos, planetInfos) ? "sysmap_planetmoon" : "sysmap_planet";
			if (!StarSystemMapUI.IsGasGiant(planetInfo))
				return string.Empty;
			if (StarSystemMapUI.HasTerrestrialMoons(planetInfo, orbitalObjectInfos, planetInfos))
				return "sysmap_gasgiantplanet";
			if (StarSystemMapUI.HasMoons(planetInfo, orbitalObjectInfos, planetInfos))
				return "sysmap_gasgiantmoon";
			return StarSystemMapUI.HasRing(planetInfo) ? "sysmap_gasgiantrings" : "sysmap_gasgiant";
		}

		internal static void Sync(App game, int systemId, string mapPanelId, bool isClickable)
		{
			bool flag = StarMap.IsInRange(game.Game.GameDatabase, game.LocalPlayer.ID, game.GameDatabase.GetStarSystemInfo(systemId), (Dictionary<int, List<ShipInfo>>)null);
			StarSystemMapUI.ResetMap(game, mapPanelId);
			if (systemId == 0)
				return;
			float time = 0.0f;
			GameDatabase gameDatabase = game.GameDatabase;
			IEnumerable<OrbitalObjectInfo> orbitalObjectInfos = gameDatabase.GetStarSystemOrbitalObjectInfos(systemId);
			StarSystemInfo starSystemInfo = gameDatabase.GetStarSystemInfo(systemId);
			StellarClass stellarClass = StellarClass.Parse(starSystemInfo.StellarClass);
			if (starSystemInfo.IsDeepSpace)
				return;
			float num1 = StarHelper.CalcRadius(StellarSize.Ia);
			float num2 = StarHelper.CalcRadius(StellarSize.VII);
			float num3 = ScalarExtensions.Lerp(0.67f, 3f, (float)(((double)StarHelper.CalcRadius(stellarClass.Size) - (double)num2) / ((double)num1 - (double)num2)));
			StarSystemMapUI.IconParams iconParams1 = StarSystemMapUI.IconParams.Default;
			iconParams1.ObjectID = StarSystemDetailsUI.StarItemID;
			iconParams1.Text = starSystemInfo.Name;
			iconParams1.Icon = "sysmap_star";
			iconParams1.X = 0.0f;
			iconParams1.Y = 0.0f;
			iconParams1.Scale = num3;
			iconParams1.Color = StarHelper.CalcIconColor(stellarClass);
			iconParams1.Clickable = isClickable;
			StarSystemMapUI.AddMapIcon(game, mapPanelId, iconParams1);
			foreach (AsteroidBeltInfo asteroidBeltInfo in gameDatabase.GetStarSystemAsteroidBeltInfos(systemId))
			{
				AsteroidBeltInfo asteroidBelt = asteroidBeltInfo;
				OrbitalObjectInfo orbitalObjectInfo = orbitalObjectInfos.First<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(x => x.ID == asteroidBelt.ID));
				StarSystemMapUI.IconParams iconParams2 = new StarSystemMapUI.IconParams();
				iconParams2.SetPos(game, orbitalObjectInfos, time, orbitalObjectInfo.ID);
				iconParams2.ObjectID = orbitalObjectInfo.ID;
				iconParams2.Icon = "sysmap_roiddust";
				iconParams2.Scale = 0.85f;
				iconParams2.Color = Vector4.One;
				iconParams2.Text = orbitalObjectInfo.Name;
				iconParams2.Clickable = false;
				StarSystemMapUI.AddMapIcon(game, mapPanelId, iconParams2);
			}
			PlanetInfo[] systemPlanetInfos = gameDatabase.GetStarSystemPlanetInfos(systemId);
			foreach (OrbitalObjectInfo orbitalObjectInfo in orbitalObjectInfos.Where<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(x => !x.ParentID.HasValue)))
			{
				OrbitalObjectInfo orbital = orbitalObjectInfo;
				PlanetInfo planetInfo = ((IEnumerable<PlanetInfo>)systemPlanetInfos).FirstOrDefault<PlanetInfo>((Func<PlanetInfo, bool>)(x => x.ID == orbital.ID));
				if (planetInfo != null)
				{
					string str = StarSystemMapUI.SelectIcon(planetInfo, orbitalObjectInfos, (IEnumerable<PlanetInfo>)systemPlanetInfos);
					if (string.IsNullOrEmpty(str))
					{
						App.Log.Trace(string.Format("Planet {0} does not have an icon to represent it in the mini system map.", (object)orbital.Name), "gui");
					}
					else
					{
						AIColonyIntel colonyIntelForPlanet = game.GameDatabase.GetColonyIntelForPlanet(game.LocalPlayer.ID, planetInfo.ID);
						if (colonyIntelForPlanet != null && flag)
						{
							Vector3 primaryColor = game.GameDatabase.GetPlayerInfo(colonyIntelForPlanet.OwningPlayerID).PrimaryColor;
							Vector4 vector4 = new Vector4(primaryColor.X, primaryColor.Y, primaryColor.Z, 1f);
							StarSystemMapUI.IconParams iconParams2 = StarSystemMapUI.IconParams.Default;
							iconParams2.SetPos(game, orbitalObjectInfos, time, planetInfo.ID);
							iconParams2.ObjectID = 0;
							iconParams2.Icon = "sysmap_ownerring";
							iconParams2.Scale = 0.85f;
							iconParams2.Color = vector4;
							iconParams2.Text = string.Empty;
							iconParams2.Clickable = false;
							StarSystemMapUI.AddMapIcon(game, mapPanelId, iconParams2);
						}
						StarSystemMapUI.IconParams iconParams3 = new StarSystemMapUI.IconParams();
						iconParams3.SetPos(game, orbitalObjectInfos, time, planetInfo.ID);
						iconParams3.ObjectID = planetInfo.ID;
						iconParams3.Icon = str;
						iconParams3.Scale = 0.85f;
						iconParams3.Color = Vector4.One;
						iconParams3.Text = orbital.Name;
						iconParams3.Clickable = isClickable;
						StarSystemMapUI.AddMapIcon(game, mapPanelId, iconParams3);
					}
				}
			}
		}

		private static bool HasRing(PlanetInfo planetInfo)
		{
			return planetInfo.RingID.HasValue;
		}

		private static bool HasMoons(
		  PlanetInfo planetInfo,
		  IEnumerable<OrbitalObjectInfo> orbitalObjectInfos,
		  IEnumerable<PlanetInfo> planetInfos)
		{
			foreach (OrbitalObjectInfo orbitalObjectInfo in orbitalObjectInfos.Where<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(x =>
		   {
			   if (x.ParentID.HasValue)
				   return x.ParentID.Value == planetInfo.ID;
			   return false;
		   })))
			{
				OrbitalObjectInfo moon = orbitalObjectInfo;
				if (planetInfos.Any<PlanetInfo>((Func<PlanetInfo, bool>)(x => x.ID == moon.ID)))
					return true;
			}
			return false;
		}

		private static bool HasTerrestrialMoons(
		  PlanetInfo planetInfo,
		  IEnumerable<OrbitalObjectInfo> orbitalObjectInfos,
		  IEnumerable<PlanetInfo> planetInfos)
		{
			foreach (OrbitalObjectInfo orbitalObjectInfo in orbitalObjectInfos.Where<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(x =>
		   {
			   if (x.ParentID.HasValue)
				   return x.ParentID.Value == planetInfo.ID;
			   return false;
		   })))
			{
				OrbitalObjectInfo moon = orbitalObjectInfo;
				PlanetInfo planetInfo1 = planetInfos.FirstOrDefault<PlanetInfo>((Func<PlanetInfo, bool>)(x => x.ID == moon.ID));
				if (planetInfo1 != null && StarSystemMapUI.IsTerrestrialPlanet(planetInfo1))
					return true;
			}
			return false;
		}

		private static bool IsGasGiant(PlanetInfo planetInfo)
		{
			return planetInfo.Type.ToLowerInvariant() == StellarBodyTypes.Gaseous;
		}

		private static bool IsTerrestrialPlanet(PlanetInfo planetInfo)
		{
			return StellarBodyTypes.IsTerrestrial(planetInfo.Type.ToLowerInvariant());
		}

		private static void ResetMap(App game, string mapPanelId)
		{
			float num = Orbit.CalcOrbitRadius(StarHelper.CalcRadius(StellarSize.Ia), StarSystemVars.Instance.StarOrbitStep, 12) * 1.1f;
			game.UI.Send((object)nameof(ResetMap), (object)mapPanelId, (object)num);
		}

		private static void AddMapIcon(
		  App game,
		  string mapPanelId,
		  StarSystemMapUI.IconParams iconParams)
		{
			game.UI.Send((object)nameof(AddMapIcon), (object)mapPanelId, (object)iconParams.ObjectID, (object)iconParams.X, (object)iconParams.Y, (object)iconParams.Scale, (object)iconParams.Color.X, (object)iconParams.Color.Y, (object)iconParams.Color.Z, (object)iconParams.Icon, (object)iconParams.Text, (object)iconParams.HasOrbit, (object)iconParams.Clickable);
		}

		private struct IconParams
		{
			public int ObjectID;
			public float X;
			public float Y;
			public float Scale;
			public Vector4 Color;
			public string Icon;
			public bool HasOrbit;
			public bool Clickable;
			public string Text;

			public void SetPos(
			  App game,
			  IEnumerable<OrbitalObjectInfo> orbitalObjectInfos,
			  float time,
			  int objectId)
			{
				Matrix matrix = GameDatabase.CalcTransform(objectId, time, orbitalObjectInfos);
				this.X = matrix.Position.X;
				this.Y = matrix.Position.Z;
				this.HasOrbit = orbitalObjectInfos.First<OrbitalObjectInfo>((Func<OrbitalObjectInfo, bool>)(o => o.ID == objectId)).ParentID.HasValue;
			}

			public static StarSystemMapUI.IconParams Default
			{
				get
				{
					return new StarSystemMapUI.IconParams()
					{
						ObjectID = 0,
						X = 0.0f,
						Y = 0.0f,
						Scale = 1f,
						Color = Vector4.One,
						Icon = string.Empty,
						HasOrbit = false,
						Clickable = false,
						Text = string.Empty
					};
				}
			}
		}
	}
}
