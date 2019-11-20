// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.StarHelper
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots
{
	internal static class StarHelper
	{
		private static readonly KeyValuePair<StellarType, Vector4>[] IconColorTable = new KeyValuePair<StellarType, Vector4>[7]
		{
	  new KeyValuePair<StellarType, Vector4>(StellarType.O, new Vector4(0.0f, 0.75f, 1f, 1f)),
	  new KeyValuePair<StellarType, Vector4>(StellarType.B, new Vector4(0.5f, 0.75f, 1f, 1f)),
	  new KeyValuePair<StellarType, Vector4>(StellarType.A, new Vector4(1f, 1f, 1f, 1f)),
	  new KeyValuePair<StellarType, Vector4>(StellarType.F, new Vector4(1f, 1f, 0.65f, 1f)),
	  new KeyValuePair<StellarType, Vector4>(StellarType.G, new Vector4(1f, 1f, 0.0f, 1f)),
	  new KeyValuePair<StellarType, Vector4>(StellarType.K, new Vector4(1f, 0.5f, 0.0f, 1f)),
	  new KeyValuePair<StellarType, Vector4>(StellarType.M, new Vector4(0.9f, 0.0f, 0.0f, 1f))
		};
		private static readonly KeyValuePair<StellarType, Vector4>[] ModelColorTable = new KeyValuePair<StellarType, Vector4>[7]
		{
	  new KeyValuePair<StellarType, Vector4>(StellarType.O, new Vector4(0.3f, 0.4f, 0.5f, 1f)),
	  new KeyValuePair<StellarType, Vector4>(StellarType.B, new Vector4(0.45f, 0.5f, 0.5f, 1f)),
	  new KeyValuePair<StellarType, Vector4>(StellarType.A, new Vector4(0.5f, 0.5f, 0.5f, 1f)),
	  new KeyValuePair<StellarType, Vector4>(StellarType.F, new Vector4(0.5f, 0.5f, 0.45f, 1f)),
	  new KeyValuePair<StellarType, Vector4>(StellarType.G, new Vector4(0.5f, 0.5f, 0.3f, 1f)),
	  new KeyValuePair<StellarType, Vector4>(StellarType.K, new Vector4(0.5f, 0.45f, 0.0f, 1f)),
	  new KeyValuePair<StellarType, Vector4>(StellarType.M, new Vector4(0.5f, 0.4f, 0.4f, 1f))
		};
		private static Dictionary<StellarType, StarDisplayParams> _displayParams = StarHelper.CreateDisplayParams();

		private static IEnumerable<Weighted<StellarType>> StarTypeWeights
		{
			get
			{
				yield return new Weighted<StellarType>(StellarType.O, StarSystem.Vars.StarTypeWeightO);
				yield return new Weighted<StellarType>(StellarType.B, StarSystem.Vars.StarTypeWeightB);
				yield return new Weighted<StellarType>(StellarType.A, StarSystem.Vars.StarTypeWeightA);
				yield return new Weighted<StellarType>(StellarType.F, StarSystem.Vars.StarTypeWeightF);
				yield return new Weighted<StellarType>(StellarType.G, StarSystem.Vars.StarTypeWeightG);
				yield return new Weighted<StellarType>(StellarType.K, StarSystem.Vars.StarTypeWeightK);
				yield return new Weighted<StellarType>(StellarType.M, StarSystem.Vars.StarTypeWeightM);
			}
		}

		public static StellarClass ResolveStellarClass(
		  Random random,
		  string typeStr,
		  string subTypeStr,
		  string sizeStr)
		{
			StellarType type;
			if (!StellarClass.TryParseType(typeStr, out type))
				type = StarHelper.ChooseStellarType(random);
			int subtype;
			if (!StellarClass.TryParseSubType(subTypeStr, out subtype))
				subtype = StarHelper.ChooseStellarSubType(random);
			StellarSize size;
			if (!StellarClass.TryParseSize(sizeStr, out size))
				size = StarHelper.ChooseStellarSize(random, type, subtype);
			return new StellarClass(type, subtype, size);
		}

		public static StellarType ChooseStellarType(Random random)
		{
			return WeightedChoices.Choose<StellarType>(random, StarHelper.StarTypeWeights);
		}

		public static int ChooseStellarSubType(Random random)
		{
			return new Dice("1D10+-1").Roll(random);
		}

		private static IEnumerable<Weighted<StellarSize>> StarSizeWeights
		{
			get
			{
				yield return new Weighted<StellarSize>(StellarSize.Ia, StarSystem.Vars.StarSizeWeightIa);
				yield return new Weighted<StellarSize>(StellarSize.Ib, StarSystem.Vars.StarSizeWeightIb);
				yield return new Weighted<StellarSize>(StellarSize.II, StarSystem.Vars.StarSizeWeightII);
				yield return new Weighted<StellarSize>(StellarSize.III, StarSystem.Vars.StarSizeWeightIII);
				yield return new Weighted<StellarSize>(StellarSize.IV, StarSystem.Vars.StarSizeWeightIV);
				yield return new Weighted<StellarSize>(StellarSize.V, StarSystem.Vars.StarSizeWeightV);
				yield return new Weighted<StellarSize>(StellarSize.VI, StarSystem.Vars.StarSizeWeightVI);
				yield return new Weighted<StellarSize>(StellarSize.VII, StarSystem.Vars.StarSizeWeightVII);
			}
		}

		public static StellarSize ChooseStellarSize(
		  Random random,
		  StellarType type,
		  int subtype)
		{
			StellarSize stellarSize = WeightedChoices.Choose<StellarSize>(random, StarHelper.StarSizeWeights);
			if (stellarSize == StellarSize.IV && type >= StellarType.K && (subtype >= 5 && type <= StellarType.M) && subtype <= 9)
			{
				StarSystem.Trace("Special Rule: IF Star is type K5 through M9, THEN treat result IV as V.");
				stellarSize = StellarSize.V;
			}
			if (stellarSize == StellarSize.VI && type >= StellarType.B && (subtype >= 0 && type <= StellarType.F) && subtype <= 4)
			{
				StarSystem.Trace("Special Rule: IF Star is type B0 through F4, THEN treat result VI as V");
				stellarSize = StellarSize.V;
			}
			return stellarSize;
		}

		public static StellarClass ChooseStellarClass(Random random)
		{
			StellarType type = StarHelper.ChooseStellarType(random);
			int subtype = StarHelper.ChooseStellarSubType(random);
			StellarSize size = StarHelper.ChooseStellarSize(random, type, subtype);
			return new StellarClass(type, subtype, size);
		}

		public static float CalcRadius(StellarSize size)
		{
			return StarSystem.Vars.StarRadius(size);
		}

		private static Vector4 CalcColor(
		  StellarType type,
		  KeyValuePair<StellarType, Vector4>[] colorTable)
		{
			return ((IEnumerable<KeyValuePair<StellarType, Vector4>>)colorTable).First<KeyValuePair<StellarType, Vector4>>((Func<KeyValuePair<StellarType, Vector4>, bool>)(x => x.Key == type)).Value;
		}

		private static Vector4 CalcColor(
		  StellarType type,
		  int subtype,
		  KeyValuePair<StellarType, Vector4>[] colorTable)
		{
			return Vector4.Lerp(StarHelper.CalcColor(type, colorTable), StarHelper.CalcColor((StellarType)Math.Min(6, (int)(type + 1)), colorTable), (float)subtype / 10f);
		}

		public static Vector4 CalcIconColor(StellarClass stellarClass)
		{
			return StarHelper.CalcColor(stellarClass.Type, stellarClass.SubType, StarHelper.IconColorTable);
		}

		public static Vector4 CalcModelColor(StellarClass stellarClass)
		{
			return StarHelper.CalcColor(stellarClass.Type, stellarClass.SubType, StarHelper.ModelColorTable);
		}

		private static IEnumerable<Weighted<Orbits>> OrbitCountWeights
		{
			get
			{
				yield return new Weighted<Orbits>(Orbits.Zero, StarSystem.Vars.StarOrbitsWeightZero);
				yield return new Weighted<Orbits>(Orbits.One, StarSystem.Vars.StarOrbitsWeightOne);
				yield return new Weighted<Orbits>(Orbits.Two, StarSystem.Vars.StarOrbitsWeightTwo);
				yield return new Weighted<Orbits>(Orbits.Three, StarSystem.Vars.StarOrbitsWeightThree);
				yield return new Weighted<Orbits>(Orbits.Four, StarSystem.Vars.StarOrbitsWeightFour);
				yield return new Weighted<Orbits>(Orbits.Five, StarSystem.Vars.StarOrbitsWeightFive);
				yield return new Weighted<Orbits>(Orbits.Six, StarSystem.Vars.StarOrbitsWeightSix);
			}
		}

		private static IEnumerable<Weighted<OrbitContents>> OrbitContentWeights
		{
			get
			{
				yield return new Weighted<OrbitContents>(OrbitContents.Empty, StarSystemVars.Instance.StarSatelliteWeightNone);
				yield return new Weighted<OrbitContents>(OrbitContents.Planet, StarSystemVars.Instance.StarSatelliteWeightPlanet);
				yield return new Weighted<OrbitContents>(OrbitContents.AsteroidBelt, StarSystemVars.Instance.StarSatelliteWeightAsteroidBelt);
				yield return new Weighted<OrbitContents>(OrbitContents.GasGiantSmall, StarSystemVars.Instance.StarSatelliteWeightGasGiantSmall);
				yield return new Weighted<OrbitContents>(OrbitContents.GasGiantLarge, StarSystemVars.Instance.StarSatelliteWeightGasGiantLarge);
			}
		}

		private static int ChooseOrbitCount(Random random, StellarClass stellarClass)
		{
			Orbits orbits = WeightedChoices.Choose<Orbits>(random, StarHelper.OrbitCountWeights);
			int num1 = Math.Max(0, (int)orbits);
			int val2 = num1;
			if ((Orbits)val2 != orbits)
				throw new NotImplementedException(string.Format("Orbits.{0} not handled for Stars", (object)orbits));
			switch (stellarClass.Size)
			{
				case StellarSize.Ia:
				case StellarSize.Ib:
				case StellarSize.II:
					StarSystem.Trace("Special Rule: If Star Size I or II THEN # of orbits = # of orbits + 4");
					val2 += 4;
					break;
				case StellarSize.III:
					StarSystem.Trace("Special Rule: If Star Size III THEN # of orbits = # of orbits + 2");
					val2 += 2;
					break;
			}
			switch (stellarClass.Type)
			{
				case StellarType.K:
					StarSystem.Trace("Special Rule: If Star Type K THEN # of orbits = # of orbits - 1");
					--val2;
					break;
				case StellarType.M:
					StarSystem.Trace("Special Rule: If Star Type M THEN # of orbits = # of orbits - 2");
					val2 -= 2;
					break;
			}
			int num2 = Math.Max(0, val2);
			StarSystem.Trace("Final orbit count: {0}\n", (object)num2);
			if (num2 == 0 && num1 > 0)
				return 1;
			return num2;
		}

		private static int CalcMinOrbitCore(StellarClass value)
		{
			switch (value.Size)
			{
				case StellarSize.Ia:
					if (StellarClass.Contains("B0", "B4", value))
						return 8;
					if (StellarClass.Contains("B5", "A9", value))
						return 7;
					if (StellarClass.Contains("F0", "F9", value))
						return 6;
					if (StellarClass.Contains("G0", "M4", value))
						return 7;
					if (StellarClass.Contains("M5", "M9", value))
						return 8;
					break;
				case StellarSize.Ib:
					if (StellarClass.Contains("B0", "B4", value))
						return 8;
					if (StellarClass.Contains("B5", "B9", value))
						return 6;
					if (StellarClass.Contains("A0", "F4", value))
						return 5;
					if (StellarClass.Contains("F5", "G4", value))
						return 4;
					if (StellarClass.Contains("G5", "K4", value))
						return 5;
					if (StellarClass.Contains("K5", "M4", value))
						return 6;
					if (StellarClass.Contains("M5", "M8", value))
						return 7;
					if (StellarClass.Contains("M9", "M9", value))
						return 8;
					break;
				case StellarSize.II:
					if (StellarClass.Contains("B0", "B4", value))
						return 7;
					if (StellarClass.Contains("B5", "B9", value))
						return 5;
					if (StellarClass.Contains("A0", "A4", value))
						return 3;
					if (StellarClass.Contains("A5", "K4", value))
						return 2;
					if (StellarClass.Contains("K5", "K9", value))
						return 3;
					if (StellarClass.Contains("M0", "M4", value))
						return 4;
					if (StellarClass.Contains("M5", "M9", value))
						return 6;
					break;
				case StellarSize.III:
					if (StellarClass.Contains("B0", "B4", value))
						return 7;
					if (StellarClass.Contains("B5", "B9", value))
						return 5;
					if (StellarClass.Contains("M0", "M4", value))
						return 2;
					if (StellarClass.Contains("M5", "M8", value))
						return 4;
					if (StellarClass.Contains("M9", "M9", value))
						return 5;
					break;
				case StellarSize.IV:
					if (StellarClass.Contains("B0", "B4", value))
						return 7;
					if (StellarClass.Contains("B5", "B9", value))
						return 3;
					break;
				case StellarSize.V:
					if (StellarClass.Contains("B0", "B4", value))
						return 6;
					if (StellarClass.Contains("B5", "B9", value))
						return 3;
					break;
			}
			return 1;
		}

		public static int CalcMinOrbit(StellarClass value)
		{
			int num = StarHelper.CalcMinOrbitCore(value);
			if (num > 1)
				StarSystem.Trace("Invalidating orbits below {0} due to size and heat of {1} star.", (object)num, (object)value);
			return num;
		}

		private static Range<int> ChooseOrbits(Random random, StellarClass stellarClass)
		{
			Range<int> range = new Range<int>(0, 0);
			int num = StarHelper.ChooseOrbitCount(random, stellarClass);
			if (num <= 0)
				return range;
			int max = num;
			int min = StarHelper.CalcMinOrbit(stellarClass);
			if (min > max)
				return range;
			return new Range<int>(min, max);
		}

		private static float CalcOrbitContentRoll(Random random, int orbitNumber)
		{
			return (float)((double)orbitNumber * 0.1 - 0.35 + random.NextDouble());
		}

		public static IEnumerable<Kerberos.Sots.Data.StarMapFramework.Orbit> ChooseOrbitContents(
		  Random random,
		  StellarClass stellarClass)
		{
			Range<int> orbits = StarHelper.ChooseOrbits(random, stellarClass);
			int asteroidBeltCount = 0;
			for (int orbitNumber = 1; orbitNumber <= orbits.Max; ++orbitNumber)
			{
				OrbitContents orbitContents = OrbitContents.Empty;
				if (orbitNumber >= orbits.Min)
				{
					bool flag = false;
					while (!flag)
					{
						flag = true;
						orbitContents = WeightedChoices.Choose<OrbitContents>((double)StarHelper.CalcOrbitContentRoll(random, orbitNumber), StarHelper.OrbitContentWeights);
						if (orbitContents == OrbitContents.AsteroidBelt && asteroidBeltCount >= 2)
							flag = false;
					}
				}
				if (orbitContents == OrbitContents.AsteroidBelt)
					++asteroidBeltCount;
				Kerberos.Sots.Data.StarMapFramework.Orbit orbiter = StarSystemHelper.CreateOrbiterParams(orbitContents);
				orbiter.OrbitNumber = orbitNumber;
				yield return orbiter;
			}
		}

		private static Dictionary<StellarType, StarDisplayParams> CreateDisplayParams()
		{
			return new Dictionary<StellarType, StarDisplayParams>()
	  {
		{
		  StellarType.O,
		  new StarDisplayParams()
		  {
			AssetPath = "props/TESTSUN_blue.scene",
			ImposterColor = DefaultStarModelParameters.ImposterColor
		  }
		},
		{
		  StellarType.B,
		  new StarDisplayParams()
		  {
			AssetPath = "props/TESTSUN_bluewhite.scene",
			ImposterColor = DefaultStarModelParameters.ImposterColor
		  }
		},
		{
		  StellarType.A,
		  new StarDisplayParams()
		  {
			AssetPath = "props/TESTSUN_white.scene",
			ImposterColor = DefaultStarModelParameters.ImposterColor
		  }
		},
		{
		  StellarType.F,
		  new StarDisplayParams()
		  {
			AssetPath = "props/TESTSUN_yellowWhite.scene",
			ImposterColor = DefaultStarModelParameters.ImposterColor
		  }
		},
		{
		  StellarType.G,
		  new StarDisplayParams()
		  {
			AssetPath = "props/TESTSUN_yellow.scene",
			ImposterColor = DefaultStarModelParameters.ImposterColor
		  }
		},
		{
		  StellarType.K,
		  new StarDisplayParams()
		  {
			AssetPath = "props/TESTSUN_orange.scene",
			ImposterColor = DefaultStarModelParameters.ImposterColor
		  }
		},
		{
		  StellarType.M,
		  new StarDisplayParams()
		  {
			AssetPath = "props/TESTSUN_Red.scene",
			ImposterColor = DefaultStarModelParameters.ImposterColor
		  }
		}
	  };
		}

		public static StarDisplayParams GetDisplayParams(StellarClass stellarClass)
		{
			return StarHelper._displayParams[stellarClass.Type];
		}
	}
}
