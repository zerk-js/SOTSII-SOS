// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.StarSystemVars
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System.Xml;

namespace Kerberos.Sots
{
	public class StarSystemVars
	{
		public static StarSystemVars _instance = new StarSystemVars();
		public static readonly string DefaultFileName = "StarSystemVars.xml";
		public readonly float StandardModelRadius = 10f;
		public float StarRadiusIa;
		public float StarRadiusIb;
		public float StarRadiusII;
		public float StarRadiusIII;
		public float StarRadiusIV;
		public float StarRadiusV;
		public float StarRadiusVI;
		public float StarRadiusVII;
		public float GasGiantMinRadiusSmall;
		public float GasGiantMaxRadiusSmall;
		public float GasGiantMinRadiusLarge;
		public float GasGiantMaxRadiusLarge;
		public float PlanetMinRadius;
		public float PlanetMaxRadius;
		public float MoonMinRadius;
		public float MoonMaxRadius;
		public float PlanetoidMinRadius;
		public float PlanetoidMaxRadius;
		public float AsteroidMinRadiusLarge;
		public float AsteroidMaxRadiusLarge;
		public float AsteroidMinRadiusMedium;
		public float AsteroidMaxRadiusMedium;
		public float AsteroidMinRadiusSmall;
		public float AsteroidMaxRadiusSmall;
		public int HabitalMoonMinSize;
		public int HabitalMoonMaxSize;
		public float ArtifactMinRadius;
		public float ArtifactMaxRadius;
		public float StarOrbitStep;
		public float PlanetOrbitStep;
		public float GasGiantOrbitStep;
		public float OrbitEccentricityMin;
		public float OrbitEccentricityMax;
		public float OrbitInclinationMin;
		public float OrbitInclinationMax;
		public int StarTypeWeightO;
		public int StarTypeWeightB;
		public int StarTypeWeightA;
		public int StarTypeWeightF;
		public int StarTypeWeightG;
		public int StarTypeWeightK;
		public int StarTypeWeightM;
		public int StarSizeWeightIa;
		public int StarSizeWeightIb;
		public int StarSizeWeightII;
		public int StarSizeWeightIII;
		public int StarSizeWeightIV;
		public int StarSizeWeightV;
		public int StarSizeWeightVI;
		public int StarSizeWeightVII;
		public int StarOrbitsWeightZero;
		public int StarOrbitsWeightOne;
		public int StarOrbitsWeightTwo;
		public int StarOrbitsWeightThree;
		public int StarOrbitsWeightFour;
		public int StarOrbitsWeightFive;
		public int StarOrbitsWeightSix;
		public int StarSatelliteWeightNone;
		public int StarSatelliteWeightPlanet;
		public int StarSatelliteWeightAsteroidBelt;
		public int StarSatelliteWeightGasGiantSmall;
		public int StarSatelliteWeightGasGiantLarge;
		public int GasGiantOrbitsWeightZero;
		public int GasGiantOrbitsWeightRing;
		public int GasGiantOrbitsWeightOne;
		public int GasGiantOrbitsWeightTwo;
		public int GasGiantOrbitsWeightThree;
		public int GasGiantOrbitsWeightFour;
		public int GasGiantSatelliteWeightNone;
		public int GasGiantSatelliteWeightArtifact;
		public int GasGiantSatelliteWeightMoon;
		public int GasGiantSatelliteWeightPlanet;
		public int AsteroidBeltMinAsteroids;
		public int AsteroidBeltMaxAsteroids;
		public int AsteroidBeltMinPlanetoids;
		public int AsteroidBeltMaxPlanetoids;
		public float AsteroidBeltMinWidth;
		public float AsteroidBeltMaxWidth;
		public float AsteroidBeltMinHeight;
		public float AsteroidBeltMaxHeight;
		public float AsteroidBeltAsteroidMinSeparation;
		public float AsteroidBeltAsteroidMaxSeparation;
		public int AsteroidRadiusWeightSmall;
		public int AsteroidRadiusWeightMedium;
		public int AsteroidRadiusWeightLarge;
		public int StationOrbitDistance;

		public static StarSystemVars Instance
		{
			get
			{
				return StarSystemVars._instance;
			}
		}

		public float StarRadius(StellarSize size)
		{
			switch (size)
			{
				case StellarSize.Ia:
					return this.StarRadiusIa;
				case StellarSize.Ib:
					return this.StarRadiusIb;
				case StellarSize.II:
					return this.StarRadiusII;
				case StellarSize.III:
					return this.StarRadiusIII;
				case StellarSize.IV:
					return this.StarRadiusIV;
				case StellarSize.V:
					return this.StarRadiusV;
				case StellarSize.VI:
					return this.StarRadiusVI;
				case StellarSize.VII:
					return this.StarRadiusVII;
				default:
					return 0.0f;
			}
		}

		private float SizeStep
		{
			get
			{
				return (float)(((double)this.PlanetMaxRadius - (double)this.PlanetMinRadius) / 10.0);
			}
		}

		public float SizeToRadius(float size)
		{
			if ((double)size >= 1.0)
				return this.PlanetMinRadius + size * this.SizeStep;
			return size * this.PlanetMinRadius;
		}

		public float RadiusToSize(float radius)
		{
			if ((double)radius >= (double)this.PlanetMinRadius)
				return (float)(int)((double)((radius - this.PlanetMinRadius) / this.SizeStep) + 0.5);
			return radius / this.PlanetMinRadius;
		}

		public Range<float> AsteroidBeltHeightRange
		{
			get
			{
				return new Range<float>(this.AsteroidBeltMinHeight, this.AsteroidBeltMaxHeight);
			}
		}

		public Range<float> AsteroidBeltWidthRange
		{
			get
			{
				return new Range<float>(this.AsteroidBeltMinWidth, this.AsteroidBeltMaxWidth);
			}
		}

		public Range<float> AsteroidBeltSeparationRange
		{
			get
			{
				return new Range<float>(this.AsteroidBeltAsteroidMinSeparation, this.AsteroidBeltAsteroidMaxSeparation);
			}
		}

		public Range<float> AsteroidRadiusRangeSmall
		{
			get
			{
				return new Range<float>(this.AsteroidMinRadiusSmall, this.AsteroidMaxRadiusSmall);
			}
		}

		public Range<float> AsteroidRadiusRangeMedium
		{
			get
			{
				return new Range<float>(this.AsteroidMinRadiusMedium, this.AsteroidMaxRadiusMedium);
			}
		}

		public Range<float> AsteroidRadiusRangeLarge
		{
			get
			{
				return new Range<float>(this.AsteroidMinRadiusLarge, this.AsteroidMaxRadiusLarge);
			}
		}

		public Range<float> OrbitEccentricityRange
		{
			get
			{
				return new Range<float>(this.OrbitEccentricityMin, this.OrbitEccentricityMax);
			}
		}

		public Range<float> OrbitInclinationRange
		{
			get
			{
				return new Range<float>(this.OrbitInclinationMin, this.OrbitInclinationMax);
			}
		}

		public static void LoadXml(string fileName)
		{
			XmlDocument document = new XmlDocument();
			document.Load(ScriptHost.FileSystem, fileName);
			XmlElement xmlElement = document[nameof(StarSystemVars)];
			StarSystemVars starSystemVars = new StarSystemVars();
			float num = float.Parse(xmlElement["StarRadiusBase"].InnerText);
			starSystemVars.StarRadiusIa = num * float.Parse(xmlElement["StarRadiusIaFactor"].InnerText);
			starSystemVars.StarRadiusIb = num * float.Parse(xmlElement["StarRadiusIbFactor"].InnerText);
			starSystemVars.StarRadiusII = num * float.Parse(xmlElement["StarRadiusIIFactor"].InnerText);
			starSystemVars.StarRadiusIII = num * float.Parse(xmlElement["StarRadiusIIIFactor"].InnerText);
			starSystemVars.StarRadiusIV = num * float.Parse(xmlElement["StarRadiusIVFactor"].InnerText);
			starSystemVars.StarRadiusV = num * float.Parse(xmlElement["StarRadiusVFactor"].InnerText);
			starSystemVars.StarRadiusVI = num * float.Parse(xmlElement["StarRadiusVIFactor"].InnerText);
			starSystemVars.StarRadiusVII = num * float.Parse(xmlElement["StarRadiusVIIFactor"].InnerText);
			starSystemVars.GasGiantMinRadiusSmall = float.Parse(xmlElement["GasGiantMinRadiusSmall"].InnerText);
			starSystemVars.GasGiantMaxRadiusSmall = float.Parse(xmlElement["GasGiantMaxRadiusSmall"].InnerText);
			starSystemVars.GasGiantMinRadiusLarge = float.Parse(xmlElement["GasGiantMinRadiusLarge"].InnerText);
			starSystemVars.GasGiantMaxRadiusLarge = float.Parse(xmlElement["GasGiantMaxRadiusLarge"].InnerText);
			starSystemVars.PlanetMinRadius = float.Parse(xmlElement["PlanetMinRadius"].InnerText);
			starSystemVars.PlanetMaxRadius = float.Parse(xmlElement["PlanetMaxRadius"].InnerText);
			starSystemVars.MoonMinRadius = float.Parse(xmlElement["MoonMinRadius"].InnerText);
			starSystemVars.MoonMaxRadius = float.Parse(xmlElement["MoonMaxRadius"].InnerText);
			starSystemVars.PlanetoidMinRadius = float.Parse(xmlElement["PlanetoidMinRadius"].InnerText);
			starSystemVars.PlanetoidMaxRadius = float.Parse(xmlElement["PlanetoidMaxRadius"].InnerText);
			starSystemVars.AsteroidMinRadiusLarge = float.Parse(xmlElement["AsteroidMinRadiusLarge"].InnerText);
			starSystemVars.AsteroidMaxRadiusLarge = float.Parse(xmlElement["AsteroidMaxRadiusLarge"].InnerText);
			starSystemVars.AsteroidMinRadiusMedium = float.Parse(xmlElement["AsteroidMinRadiusMedium"].InnerText);
			starSystemVars.AsteroidMaxRadiusMedium = float.Parse(xmlElement["AsteroidMaxRadiusMedium"].InnerText);
			starSystemVars.AsteroidMinRadiusSmall = float.Parse(xmlElement["AsteroidMinRadiusSmall"].InnerText);
			starSystemVars.AsteroidMaxRadiusSmall = float.Parse(xmlElement["AsteroidMaxRadiusSmall"].InnerText);
			starSystemVars.ArtifactMinRadius = float.Parse(xmlElement["ArtifactMinRadius"].InnerText);
			starSystemVars.ArtifactMaxRadius = float.Parse(xmlElement["ArtifactMaxRadius"].InnerText);
			starSystemVars.HabitalMoonMinSize = int.Parse(xmlElement["HabitalMoonMinSize"].InnerText);
			starSystemVars.HabitalMoonMaxSize = int.Parse(xmlElement["HabitalMoonMaxSize"].InnerText);
			starSystemVars.StarOrbitStep = float.Parse(xmlElement["StarOrbitStep"].InnerText);
			starSystemVars.PlanetOrbitStep = float.Parse(xmlElement["PlanetOrbitStep"].InnerText);
			starSystemVars.GasGiantOrbitStep = float.Parse(xmlElement["GasGiantOrbitStep"].InnerText);
			starSystemVars.OrbitEccentricityMin = float.Parse(xmlElement["OrbitEccentricityMin"].InnerText);
			starSystemVars.OrbitEccentricityMax = float.Parse(xmlElement["OrbitEccentricityMax"].InnerText);
			starSystemVars.OrbitInclinationMin = MathHelper.DegreesToRadians(float.Parse(xmlElement["OrbitInclinationMin"].InnerText));
			starSystemVars.OrbitInclinationMax = MathHelper.DegreesToRadians(float.Parse(xmlElement["OrbitInclinationMax"].InnerText));
			starSystemVars.StarTypeWeightO = int.Parse(xmlElement["StarTypeWeightO"].InnerText);
			starSystemVars.StarTypeWeightB = int.Parse(xmlElement["StarTypeWeightB"].InnerText);
			starSystemVars.StarTypeWeightA = int.Parse(xmlElement["StarTypeWeightA"].InnerText);
			starSystemVars.StarTypeWeightF = int.Parse(xmlElement["StarTypeWeightF"].InnerText);
			starSystemVars.StarTypeWeightG = int.Parse(xmlElement["StarTypeWeightG"].InnerText);
			starSystemVars.StarTypeWeightK = int.Parse(xmlElement["StarTypeWeightK"].InnerText);
			starSystemVars.StarTypeWeightM = int.Parse(xmlElement["StarTypeWeightM"].InnerText);
			starSystemVars.StarSizeWeightIa = int.Parse(xmlElement["StarSizeWeightIa"].InnerText);
			starSystemVars.StarSizeWeightIb = int.Parse(xmlElement["StarSizeWeightIb"].InnerText);
			starSystemVars.StarSizeWeightII = int.Parse(xmlElement["StarSizeWeightII"].InnerText);
			starSystemVars.StarSizeWeightIII = int.Parse(xmlElement["StarSizeWeightIII"].InnerText);
			starSystemVars.StarSizeWeightIV = int.Parse(xmlElement["StarSizeWeightIV"].InnerText);
			starSystemVars.StarSizeWeightV = int.Parse(xmlElement["StarSizeWeightV"].InnerText);
			starSystemVars.StarSizeWeightVI = int.Parse(xmlElement["StarSizeWeightVI"].InnerText);
			starSystemVars.StarSizeWeightVII = int.Parse(xmlElement["StarSizeWeightVII"].InnerText);
			starSystemVars.StarOrbitsWeightZero = int.Parse(xmlElement["StarOrbitsWeightZero"].InnerText);
			starSystemVars.StarOrbitsWeightOne = int.Parse(xmlElement["StarOrbitsWeightOne"].InnerText);
			starSystemVars.StarOrbitsWeightTwo = int.Parse(xmlElement["StarOrbitsWeightTwo"].InnerText);
			starSystemVars.StarOrbitsWeightThree = int.Parse(xmlElement["StarOrbitsWeightThree"].InnerText);
			starSystemVars.StarOrbitsWeightFour = int.Parse(xmlElement["StarOrbitsWeightFour"].InnerText);
			starSystemVars.StarOrbitsWeightFive = int.Parse(xmlElement["StarOrbitsWeightFive"].InnerText);
			starSystemVars.StarOrbitsWeightSix = int.Parse(xmlElement["StarOrbitsWeightSix"].InnerText);
			starSystemVars.StarSatelliteWeightNone = int.Parse(xmlElement["StarSatelliteWeightNone"].InnerText);
			starSystemVars.StarSatelliteWeightPlanet = int.Parse(xmlElement["StarSatelliteWeightPlanet"].InnerText);
			starSystemVars.StarSatelliteWeightAsteroidBelt = int.Parse(xmlElement["StarSatelliteWeightAsteroidBelt"].InnerText);
			starSystemVars.StarSatelliteWeightGasGiantSmall = int.Parse(xmlElement["StarSatelliteWeightGasGiantSmall"].InnerText);
			starSystemVars.StarSatelliteWeightGasGiantLarge = int.Parse(xmlElement["StarSatelliteWeightGasGiantLarge"].InnerText);
			starSystemVars.GasGiantOrbitsWeightZero = int.Parse(xmlElement["GasGiantOrbitsWeightZero"].InnerText);
			starSystemVars.GasGiantOrbitsWeightRing = int.Parse(xmlElement["GasGiantOrbitsWeightRing"].InnerText);
			starSystemVars.GasGiantOrbitsWeightOne = int.Parse(xmlElement["GasGiantOrbitsWeightOne"].InnerText);
			starSystemVars.GasGiantOrbitsWeightTwo = int.Parse(xmlElement["GasGiantOrbitsWeightTwo"].InnerText);
			starSystemVars.GasGiantOrbitsWeightThree = int.Parse(xmlElement["GasGiantOrbitsWeightThree"].InnerText);
			starSystemVars.GasGiantOrbitsWeightFour = int.Parse(xmlElement["GasGiantOrbitsWeightFour"].InnerText);
			starSystemVars.GasGiantSatelliteWeightNone = int.Parse(xmlElement["GasGiantSatelliteWeightNone"].InnerText);
			starSystemVars.GasGiantSatelliteWeightArtifact = int.Parse(xmlElement["GasGiantSatelliteWeightArtifact"].InnerText);
			starSystemVars.GasGiantSatelliteWeightMoon = int.Parse(xmlElement["GasGiantSatelliteWeightMoon"].InnerText);
			starSystemVars.GasGiantSatelliteWeightPlanet = int.Parse(xmlElement["GasGiantSatelliteWeightPlanet"].InnerText);
			starSystemVars.AsteroidBeltMinAsteroids = int.Parse(xmlElement["AsteroidBeltMinAsteroids"].InnerText);
			starSystemVars.AsteroidBeltMaxAsteroids = int.Parse(xmlElement["AsteroidBeltMaxAsteroids"].InnerText);
			starSystemVars.AsteroidBeltMinPlanetoids = int.Parse(xmlElement["AsteroidBeltMinPlanetoids"].InnerText);
			starSystemVars.AsteroidBeltMaxPlanetoids = int.Parse(xmlElement["AsteroidBeltMaxPlanetoids"].InnerText);
			starSystemVars.AsteroidBeltMinWidth = float.Parse(xmlElement["AsteroidBeltMinWidth"].InnerText);
			starSystemVars.AsteroidBeltMaxWidth = float.Parse(xmlElement["AsteroidBeltMaxWidth"].InnerText);
			starSystemVars.AsteroidBeltMinHeight = float.Parse(xmlElement["AsteroidBeltMinHeight"].InnerText);
			starSystemVars.AsteroidBeltMaxHeight = float.Parse(xmlElement["AsteroidBeltMaxHeight"].InnerText);
			starSystemVars.AsteroidBeltAsteroidMinSeparation = float.Parse(xmlElement["AsteroidBeltAsteroidMinSeparation"].InnerText);
			starSystemVars.AsteroidBeltAsteroidMaxSeparation = float.Parse(xmlElement["AsteroidBeltAsteroidMaxSeparation"].InnerText);
			starSystemVars.AsteroidRadiusWeightSmall = int.Parse(xmlElement["AsteroidRadiusWeightSmall"].InnerText);
			starSystemVars.AsteroidRadiusWeightMedium = int.Parse(xmlElement["AsteroidRadiusWeightMedium"].InnerText);
			starSystemVars.AsteroidRadiusWeightLarge = int.Parse(xmlElement["AsteroidRadiusWeightLarge"].InnerText);
			starSystemVars.StationOrbitDistance = int.Parse(xmlElement["StationOrbitDistance"].InnerText);
			StarSystemVars._instance = starSystemVars;
		}
	}
}
