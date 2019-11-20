// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.PlanetGraphicsRules
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using Kerberos.Sots.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Kerberos.Sots.GameStates
{
	internal class PlanetGraphicsRules
	{
		private XmlElement _root;

		public PlanetGraphicsRules()
		{
			this.Reload();
		}

		public void Reload()
		{
			XmlDocument document = new XmlDocument();
			document.Load(ScriptHost.FileSystem, "commonassets.xml");
			this._root = document["CommonAssets"]["planets"];
		}

		private XmlElement GetRandomPlanetType(string name, int? typeVariant, int orbitalId)
		{
			IEnumerable<XmlElement> xmlElements = this._root.ChildNodes.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x =>
		   {
			   if (x.Name == "type")
				   return x.GetAttribute(nameof(name)) == name.ToLowerInvariant();
			   return false;
		   }));
			if (!typeVariant.HasValue)
				return new Random(orbitalId).Choose<XmlElement>(xmlElements);
			XmlElement[] array = xmlElements.ToArray<XmlElement>();
			return array[typeVariant.Value % array.Length];
		}

		private XmlElement[] GetHazardPointBrackets(
		  string type,
		  int? typeVariant,
		  float hazard,
		  int orbitalId)
		{
			XmlElement[] array = this.GetRandomPlanetType(type, typeVariant, orbitalId).ChildNodes.OfType<XmlElement>().OrderBy<XmlElement, float>((Func<XmlElement, float>)(x => float.Parse(x.GetAttribute("value")))).ToArray<XmlElement>();
			if (array.Length == 0)
				return (XmlElement[])null;
			if (array.Length == 1)
				return new XmlElement[2] { array[0], array[0] };
			float num1 = float.Parse(array[0].GetAttribute("value"));
			if ((double)num1 >= (double)hazard)
				return new XmlElement[2] { array[0], array[0] };
			for (int index = 0; index < array.Length - 1; ++index)
			{
				float num2 = num1;
				num1 = float.Parse(array[index + 1].GetAttribute("value"));
				if ((double)num2 <= (double)hazard && (double)num1 >= (double)hazard)
					return new XmlElement[2]
					{
			array[index],
			array[index + 1]
					};
			}
			return new XmlElement[2]
			{
		array[array.Length - 1],
		array[array.Length - 1]
			};
		}

		private XmlElement GetAtmo(string name)
		{
			return this._root.ChildNodes.OfType<XmlElement>().FirstOrDefault<XmlElement>((Func<XmlElement, bool>)(x =>
		   {
			   if (x.Name == "atmo")
				   return x.GetAttribute(nameof(name)) == name;
			   return false;
		   }));
		}

		private XmlElement GetHeightGen(string name)
		{
			return this._root.ChildNodes.OfType<XmlElement>().FirstOrDefault<XmlElement>((Func<XmlElement, bool>)(x =>
		   {
			   if (x.Name == "heightgen")
				   return x.GetAttribute(nameof(name)) == name;
			   return false;
		   }));
		}

		private XmlElement GetFaction(string name)
		{
			return this._root.ChildNodes.OfType<XmlElement>().FirstOrDefault<XmlElement>((Func<XmlElement, bool>)(x =>
		   {
			   if (x.Name == "faction")
				   return x.GetAttribute(nameof(name)) == name;
			   return false;
		   }));
		}

		public StellarBody.Params GetStellarBodyParams(GameSession game, int orbitalId)
		{
			OrbitalObjectInfo orbitalObjectInfo = game.GameDatabase.GetOrbitalObjectInfo(orbitalId);
			ColonyInfo colonyInfoForPlanet = game.GameDatabase.GetColonyInfoForPlanet(orbitalId);
			PlanetInfo planetInfo = game.GameDatabase.GetPlanetInfo(orbitalId);
			SystemColonyType colonyType = SystemColonyType.Normal;
			int num;
			double population;
			PlayerInfo povPlayerInfo;
			if (colonyInfoForPlanet != null)
			{
				num = colonyInfoForPlanet.PlayerID;
				population = colonyInfoForPlanet.ImperialPop;
				povPlayerInfo = game.GameDatabase.GetPlayerInfo(num);
				if (orbitalObjectInfo != null && povPlayerInfo != null)
				{
					HomeworldInfo homeworldInfo = game.GameDatabase.GetHomeworlds().FirstOrDefault<HomeworldInfo>((Func<HomeworldInfo, bool>)(x => x.SystemID == orbitalObjectInfo.StarSystemID));
					if (homeworldInfo != null && homeworldInfo.SystemID != 0 && homeworldInfo.PlayerID == povPlayerInfo.ID)
						colonyType = SystemColonyType.Home;
					else if (game.GameDatabase.GetProvinceInfos().Any<ProvinceInfo>((Func<ProvinceInfo, bool>)(x =>
				   {
					   if (x.CapitalSystemID != orbitalObjectInfo.StarSystemID || x.PlayerID != povPlayerInfo.ID)
						   return false;
					   int capitalSystemId = x.CapitalSystemID;
					   int? homeworld = povPlayerInfo.Homeworld;
					   if (capitalSystemId == homeworld.GetValueOrDefault())
						   return !homeworld.HasValue;
					   return true;
				   })))
						colonyType = SystemColonyType.Capital;
				}
			}
			else
			{
				num = 0;
				population = 0.0;
				povPlayerInfo = game.GameDatabase.GetPlayerInfo(game.LocalPlayer.ID);
			}
			FactionInfo factionInfo = game.GameDatabase.GetFactionInfo(povPlayerInfo.FactionID);
			float hazard = Math.Abs(planetInfo.Suitability - factionInfo.IdealSuitability);
			float stratModifier = (float)game.App.GetStratModifier<int>(StratModifiers.MaxColonizableHazard, povPlayerInfo.ID);
			float radius = StarSystemVars.Instance.SizeToRadius(planetInfo.Size);
			Matrix transform = orbitalObjectInfo.OrbitalPath.GetTransform(0.0);
			return this.GetStellarBodyParams(StarSystemMapUI.SelectIcon(planetInfo, game.GameDatabase.GetStarSystemOrbitalObjectInfos(orbitalObjectInfo.StarSystemID), (IEnumerable<PlanetInfo>)game.GameDatabase.GetStarSystemPlanetInfos(orbitalObjectInfo.StarSystemID)), transform.Position, radius, orbitalId, num, planetInfo.Type, hazard, stratModifier, factionInfo.Name, (float)planetInfo.Biosphere, population, new int?(), Colony.GetColonyStage(game.GameDatabase, num, (double)hazard), colonyType);
		}

		public string[] GetFactions()
		{
			try
			{
				return this._root.ChildNodes.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name == "faction")).Select<XmlElement, string>((Func<XmlElement, string>)(x => x.GetAttribute("name"))).ToArray<string>();
			}
			catch (Exception ex)
			{
				App.Log.Warn(string.Format("Parse error resolving planet display rules:\n" + ex.ToString()), "data");
				return new string[0];
			}
		}

		public string[] GetStellarBodyTypes()
		{
			try
			{
				return this._root.ChildNodes.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name == "type")).Select<XmlElement, string>((Func<XmlElement, string>)(x => x.GetAttribute("name"))).Distinct<string>().ToArray<string>();
			}
			catch (Exception ex)
			{
				App.Log.Warn(string.Format("Parse error resolving planet display rules:\n" + ex.ToString()), "data");
				return new string[0];
			}
		}

		public StellarBody.Params GetStellarBodyParams(
		  string iconSpriteName,
		  Vector3 position,
		  float radius,
		  int orbitalId,
		  int colonyPlayerId,
		  string type,
		  float hazard,
		  float maxHazard,
		  string faction,
		  float biosphere,
		  double population,
		  int? typeVariant,
		  Kerberos.Sots.Strategy.InhabitedPlanet.ColonyStage colonyStage,
		  SystemColonyType colonyType)
		{
			try
			{
				XmlElement[] hazardPointBrackets = this.GetHazardPointBrackets(type, typeVariant, hazard, orbitalId);
				float num1 = float.Parse(hazardPointBrackets[1].GetAttribute("value"));
				float num2 = float.Parse(hazardPointBrackets[0].GetAttribute("value"));
				XmlElement atmo1 = this.GetAtmo(hazardPointBrackets[0].GetAttribute("atmo"));
				XmlElement atmo2 = this.GetAtmo(hazardPointBrackets[1].GetAttribute("atmo"));
				XmlElement heightGen1 = this.GetHeightGen(hazardPointBrackets[0].GetAttribute("heightgen"));
				XmlElement heightGen2 = this.GetHeightGen(hazardPointBrackets[1].GetAttribute("heightgen"));
				XmlElement faction1 = this.GetFaction(faction);
				Vector2 vector2 = faction1 != null ? Vector2.Parse(faction1.GetAttribute("cityaltrange")) : Vector2.Zero;
				float t = 0.0f;
				if ((double)num1 - (double)num2 >= 1.40129846432482E-45)
					t = ((float)(((double)hazard - (double)num2) / ((double)num1 - (double)num2))).Saturate();
				StellarBody.Params @params = new StellarBody.Params();
				@params.Civilians = new StellarBody.PlanetCivilianData[0];
				@params.IconSpriteName = iconSpriteName;
				@params.ColonyPlayerID = colonyPlayerId;
				@params.AtmoThickness = ScalarExtensions.Lerp(float.Parse(atmo1.GetAttribute("thick")), float.Parse(atmo2.GetAttribute("thick")), t);
				@params.AtmoKm = ScalarExtensions.Lerp(float.Parse(atmo1.GetAttribute("km")), float.Parse(atmo2.GetAttribute("km")), t);
				@params.AtmoKr = ScalarExtensions.Lerp(float.Parse(atmo1.GetAttribute("kr")), float.Parse(atmo2.GetAttribute("kr")), t);
				@params.AtmoScatterWaveLengths = Vector3.Lerp(Vector3.Parse(atmo1.GetAttribute("filter")), Vector3.Parse(atmo2.GetAttribute("filter")), t);
				@params.AtmoScaleDepth = ScalarExtensions.Lerp(float.Parse(atmo1.GetAttribute("scaledepth")), float.Parse(atmo2.GetAttribute("scaledepth")), t);
				@params.CloudDiffuseTexture = atmo1.GetAttribute("clouddiffuse");
				@params.CloudSpecularTexture = atmo1.GetAttribute("cloudspecular");
				@params.CloudOpacity = ScalarExtensions.Lerp(float.Parse(atmo1.GetAttribute("cloudcover")), float.Parse(atmo2.GetAttribute("cloudcover")), t);
				@params.CloudDiffuseColor = Vector3.Lerp(Vector3.Parse(atmo1.GetAttribute("clouddiffusecolor")), Vector3.Parse(atmo2.GetAttribute("clouddiffusecolor")), t);
				@params.CloudSpecularColor = Vector3.Lerp(Vector3.Parse(atmo1.GetAttribute("cloudspecularcolor")), Vector3.Parse(atmo2.GetAttribute("cloudspecularcolor")), t);
				@params.CityEmissiveTexture = faction1.GetAttribute("cityemissive");
				@params.CitySprawl = (float)(population / 1000000000.0);
				@params.HeightGradient1Texture = hazardPointBrackets[0].GetAttribute("heightgrad");
				@params.HeightGradient2Texture = hazardPointBrackets[1].GetAttribute("heightgrad");
				@params.HeightGradient2Blend = t;
				@params.HeightGradient3Texture = faction1.GetAttribute("idealgrad");
				@params.HeightGradient3Blend = 1f - (hazard / maxHazard).Saturate();
				@params.MaxCityAltitude = vector2.Y;
				@params.MinCityAltitude = vector2.X;
				@params.WaterSpecularColor = Vector3.Lerp(Vector3.Parse(hazardPointBrackets[0].GetAttribute("waterspec")), Vector3.Parse(hazardPointBrackets[1].GetAttribute("waterspec")), t);
				@params.MaxLandHeight = ScalarExtensions.Lerp(float.Parse(hazardPointBrackets[0].GetAttribute("maxlandalt")), float.Parse(hazardPointBrackets[1].GetAttribute("maxlandalt")), t);
				@params.MaxWaterDepth = ScalarExtensions.Lerp(float.Parse(hazardPointBrackets[0].GetAttribute("maxseadepth")), float.Parse(hazardPointBrackets[1].GetAttribute("maxseadepth")), t);
				@params.WaterLevel = ScalarExtensions.Lerp(float.Parse(hazardPointBrackets[0].GetAttribute("sealevel")), float.Parse(hazardPointBrackets[1].GetAttribute("sealevel")), t);
				float num3 = ScalarExtensions.Lerp(float.Parse(hazardPointBrackets[0].GetAttribute("bumpiness")), float.Parse(hazardPointBrackets[1].GetAttribute("bumpiness")), t);
				@params.Position = position;
				@params.Radius = radius;
				@params.RandomSeed = orbitalId;
				@params.UseHeightMap = true;
				@params.TextureSize = 512;
				@params.BodyType = type;
				@params.ColonyStage = colonyStage;
				@params.ColonyType = colonyType;
				string attribute1 = heightGen1.GetAttribute(nameof(type));
				string attribute2 = heightGen2.GetAttribute(nameof(type));
				if (attribute1 == "planecuts")
				{
					StellarBody.HGPlaneCutsParams hgPlaneCutsParams = new StellarBody.HGPlaneCutsParams();
					hgPlaneCutsParams.Bumpiness = num3;
					if (attribute2 == attribute1)
					{
						hgPlaneCutsParams.BaseHeight = ScalarExtensions.Lerp(float.Parse(heightGen1.GetAttribute("baseheight")), float.Parse(heightGen2.GetAttribute("baseheight")), t);
						hgPlaneCutsParams.Iterations = (int)ScalarExtensions.Lerp((float)int.Parse(heightGen1.GetAttribute("iterations")), (float)int.Parse(heightGen2.GetAttribute("iterations")), t);
						hgPlaneCutsParams.Shift = ScalarExtensions.Lerp(float.Parse(heightGen1.GetAttribute("shift")), float.Parse(heightGen2.GetAttribute("shift")), t);
					}
					else
					{
						hgPlaneCutsParams.BaseHeight = float.Parse(heightGen1.GetAttribute("baseheight"));
						hgPlaneCutsParams.Iterations = int.Parse(heightGen1.GetAttribute("iterations"));
						hgPlaneCutsParams.Shift = float.Parse(heightGen1.GetAttribute("shift"));
					}
					@params.HeightGen = (StellarBody.HeightGenParams)hgPlaneCutsParams;
				}
				else if (attribute1 == "blend")
				{
					StellarBody.HGBlendParams hgBlendParams = new StellarBody.HGBlendParams();
					hgBlendParams.Bumpiness = num3;
					hgBlendParams.Layer1Texture = heightGen1.GetAttribute("base");
					if (attribute2 == attribute1)
					{
						hgBlendParams.Layer2Texture = heightGen2.GetAttribute("base");
						hgBlendParams.Layer2Amount = t;
					}
					else
					{
						hgBlendParams.Layer2Texture = hgBlendParams.Layer1Texture;
						hgBlendParams.Layer2Amount = 0.0f;
					}
					@params.HeightGen = (StellarBody.HeightGenParams)hgBlendParams;
				}
				return @params;
			}
			catch (Exception ex)
			{
				App.Log.Warn(string.Format("Parse error resolving planet display rules (type={0}, will use defaults instead):\n" + ex.ToString(), (object)(type ?? "null")), "data");
				return StellarBody.Params.Default;
			}
		}
	}
}
