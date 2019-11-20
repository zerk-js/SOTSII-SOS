// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.StellarBody
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;

namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_STELLARBODY)]
	internal class StellarBody : GameObject, IActive, IDisposable
	{
		private List<PlanetWeaponBank> _weaponBanks = new List<PlanetWeaponBank>();
		private double _totalPopulation;
		private IGameObject _lastAttackingObject;
		private PlanetInfo _planetInfo;
		private bool _active;
		private StellarBody.Params _params;

		public double Population
		{
			get
			{
				return this._totalPopulation;
			}
			set
			{
				this._totalPopulation = value;
			}
		}

		public IGameObject LastAttackingObject
		{
			get
			{
				return this._lastAttackingObject;
			}
		}

		public List<PlanetWeaponBank> WeaponBanks
		{
			get
			{
				return this._weaponBanks;
			}
			set
			{
				this._weaponBanks = value;
			}
		}

		public PlanetInfo PlanetInfo
		{
			get
			{
				return this._planetInfo;
			}
			set
			{
				this._planetInfo = value;
			}
		}

		public bool Active
		{
			get
			{
				return this._active;
			}
			set
			{
				if (value == this._active)
					return;
				this._active = value;
				this.PostSetActive(this._active);
			}
		}

		public StellarBody.Params Parameters
		{
			get
			{
				return this._params;
			}
			set
			{
				this._params = value;
			}
		}

		public static StellarBody Create(App game, StellarBody.Params p)
		{
			Player player = game.GetPlayer(p.ColonyPlayerID);
			List<object> objectList = new List<object>();
			objectList.AddRange((IEnumerable<object>)new object[38]
			{
		(object) (player != null ? player.ObjectID : 0),
		(object) p.OrbitalID,
		(object) p.IconSpriteName,
		(object) (p.SurfaceMaterial ?? "planet_earth2"),
		(object) p.Position,
		(object) p.Radius,
		(object) p.IsInCombat,
		(object) game.AssetDatabase.DefaultPlanetSensorRange,
		(object) p.AtmoThickness,
		(object) p.AtmoScatterWaveLengths,
		(object) p.AtmoKm,
		(object) p.AtmoKr,
		(object) p.AtmoScaleDepth,
		(object) p.UseHeightMap,
		(object) p.RandomSeed,
		(object) p.TextureSize,
		(object) p.HeightGradient1Texture,
		(object) p.HeightGradient2Texture,
		(object) p.HeightGradient3Texture,
		(object) p.HeightGradient2Blend,
		(object) p.HeightGradient3Blend,
		(object) p.CityEmissiveTexture,
		(object) p.MinCityAltitude,
		(object) p.MaxCityAltitude,
		(object) p.CitySprawl,
		(double) p.CloudOpacity != 0.0 ? (object) p.CloudDiffuseTexture : (object) string.Empty,
		(double) p.CloudOpacity != 0.0 ? (object) p.CloudSpecularTexture : (object) string.Empty,
		(object) p.CloudOpacity,
		(object) p.CloudDiffuseColor,
		(object) p.CloudSpecularColor,
		(object) p.WaterLevel,
		(object) p.WaterSpecularColor,
		(object) p.MaxWaterDepth,
		(object) p.MaxLandHeight,
		(object) p.BodyType,
		(object) (p.BodyName ?? string.Empty),
		(object) (int) p.ColonyStage,
		(object) (int) p.ColonyType
			});
			if (p.HeightGen != null)
			{
				if (p.HeightGen is StellarBody.HGBlendParams)
				{
					StellarBody.HGBlendParams heightGen = p.HeightGen as StellarBody.HGBlendParams;
					objectList.AddRange((IEnumerable<object>)new object[5]
					{
			(object) "blend",
			(object) heightGen.Bumpiness,
			(object) heightGen.Layer1Texture,
			(object) heightGen.Layer2Texture,
			(object) heightGen.Layer2Amount
					});
				}
				else if (p.HeightGen is StellarBody.HGPlaneCutsParams)
				{
					StellarBody.HGPlaneCutsParams heightGen = p.HeightGen as StellarBody.HGPlaneCutsParams;
					objectList.AddRange((IEnumerable<object>)new object[5]
					{
			(object) "planecuts",
			(object) heightGen.Bumpiness,
			(object) heightGen.BaseHeight,
			(object) heightGen.Iterations,
			(object) heightGen.Shift
					});
				}
				else
					objectList.Add((object)"none");
			}
			else
				objectList.Add((object)"none");
			objectList.Add((object)p.Civilians.Length);
			foreach (StellarBody.PlanetCivilianData civilian in p.Civilians)
			{
				objectList.Add((object)civilian.Faction);
				objectList.Add((object)civilian.Population);
			}
			objectList.Add((object)p.ImperialPopulation);
			objectList.Add((object)p.Infrastructure);
			objectList.Add((object)p.Suitability);
			objectList.Add((object)Constants.MinSuitability);
			objectList.Add((object)Constants.MaxSuitability);
			StellarBody stellarBody = game.AddObject<StellarBody>(objectList.ToArray());
			stellarBody.Parameters = p;
			return stellarBody;
		}

		public override bool OnEngineMessage(InteropMessageID messageId, ScriptMessageReader message)
		{
			if (messageId == InteropMessageID.IMID_SCRIPT_OBJECT_SETPROP)
			{
				if (message.ReadString() == "PlanetPopUpdate")
				{
					this.Population = message.ReadDouble();
					this._lastAttackingObject = this.App.GetGameObject(message.ReadInteger());
					return true;
				}
			}
			else
				App.Log.Warn("Unhandled message (id=" + (object)messageId + ").", "game");
			return false;
		}

		public void Dispose()
		{
			this._lastAttackingObject = (IGameObject)null;
			foreach (IGameObject weaponBank in this._weaponBanks)
				this.App.ReleaseObject(weaponBank);
			this._weaponBanks.Clear();
			this.App.ReleaseObject((IGameObject)this);
		}

		public class HeightGenParams
		{
			public float Bumpiness = 1f;
		}

		public class HGBlendParams : StellarBody.HeightGenParams
		{
			public string Layer1Texture = string.Empty;
			public string Layer2Texture = string.Empty;
			public float Layer2Amount;
		}

		public class HGPlaneCutsParams : StellarBody.HeightGenParams
		{
			public float BaseHeight = 0.5f;
			public int Iterations = 5000;
			public float Shift = 1000f;
		}

		public struct PlanetCivilianData
		{
			public string Faction;
			public double Population;
		}

		public struct Params
		{
			public static readonly StellarBody.Params Default = new StellarBody.Params()
			{
				ColonyPlayerID = 0,
				OrbitalID = 0,
				IconSpriteName = "sysmap_planet",
				SurfaceMaterial = "planet_earth2",
				Position = Vector3.Zero,
				Radius = 5000f,
				AtmoThickness = 0.06f,
				AtmoScatterWaveLengths = new Vector3(0.65f, 0.57f, 0.475f),
				AtmoKm = 1f / 400f,
				AtmoKr = 1f / 1000f,
				AtmoScaleDepth = 0.05f,
				UseHeightMap = false,
				IsInCombat = true,
				RandomSeed = 0,
				TextureSize = 512,
				HeightGradient1Texture = string.Empty,
				HeightGradient2Texture = string.Empty,
				HeightGradient3Texture = string.Empty,
				HeightGradient2Blend = 0.0f,
				HeightGradient3Blend = 0.0f,
				CityEmissiveTexture = string.Empty,
				MinCityAltitude = 0.0f,
				MaxCityAltitude = 1f,
				CitySprawl = 1f,
				CloudDiffuseTexture = "props\\textures\\Earth_Clouds_Diffuse.tga",
				CloudSpecularTexture = "props\\textures\\Earth_Clouds_Specular.tga",
				CloudOpacity = 1f,
				CloudDiffuseColor = Vector3.One,
				CloudSpecularColor = Vector3.One,
				WaterLevel = 0.5f,
				WaterSpecularColor = Vector3.One,
				MaxWaterDepth = 0.5f,
				MaxLandHeight = 0.5f,
				HeightGen = (StellarBody.HeightGenParams)null,
				Civilians = new StellarBody.PlanetCivilianData[0],
				ImperialPopulation = 0.0,
				Infrastructure = 0.0f,
				Suitability = 0.0f,
				BodyType = "normal",
				BodyName = "unnamed_body",
				ColonyStage = Kerberos.Sots.Strategy.InhabitedPlanet.ColonyStage.Open,
				ColonyType = SystemColonyType.Normal
			};
			public int ColonyPlayerID;
			public int OrbitalID;
			public string IconSpriteName;
			public string SurfaceMaterial;
			public Vector3 Position;
			public float Radius;
			public float AtmoThickness;
			public Vector3 AtmoScatterWaveLengths;
			public float AtmoKm;
			public float AtmoKr;
			public float AtmoScaleDepth;
			public bool UseHeightMap;
			public bool IsInCombat;
			public int RandomSeed;
			public int TextureSize;
			public string HeightGradient1Texture;
			public string HeightGradient2Texture;
			public string HeightGradient3Texture;
			public float HeightGradient2Blend;
			public float HeightGradient3Blend;
			public string CityEmissiveTexture;
			public float MinCityAltitude;
			public float MaxCityAltitude;
			public float CitySprawl;
			public string CloudDiffuseTexture;
			public string CloudSpecularTexture;
			public float CloudOpacity;
			public Vector3 CloudDiffuseColor;
			public Vector3 CloudSpecularColor;
			public float WaterLevel;
			public Vector3 WaterSpecularColor;
			public float MaxWaterDepth;
			public float MaxLandHeight;
			public StellarBody.HeightGenParams HeightGen;
			public StellarBody.PlanetCivilianData[] Civilians;
			public double ImperialPopulation;
			public float Infrastructure;
			public float Suitability;
			public string BodyType;
			public string BodyName;
			public Kerberos.Sots.Strategy.InhabitedPlanet.ColonyStage ColonyStage;
			public SystemColonyType ColonyType;
		}
	}
}
