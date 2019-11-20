// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.StarSystemHelper
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.StarMapElements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots
{
	internal static class StarSystemHelper
	{
		private static float CalcRadius(Kerberos.Sots.Data.StarMapFramework.Orbit orbital)
		{
			if (orbital is StarOrbit)
				return StarSystemVars.Instance.StarRadius(StellarClass.Parse((orbital as StarOrbit).StellarClass).Size);
			PlanetInfo planetInfo = StarSystemHelper.InferPlanetInfo(orbital);
			if (planetInfo != null)
				return StarSystemVars.Instance.SizeToRadius(planetInfo.Size);
			throw new ArgumentException("unexpected type");
		}

		public static float CalcOrbitStep(IStellarEntity orbitParent)
		{
			return StarSystemHelper.CalcOrbitStep(orbitParent.Params);
		}

		private static float CalcOrbitStep(Kerberos.Sots.Data.StarMapFramework.Orbit orbital)
		{
			if (orbital is StarOrbit)
				return StarSystemVars.Instance.StarOrbitStep;
			if (orbital is GasGiantLargeOrbit || orbital is GasGiantSmallOrbit)
				return StarSystemVars.Instance.GasGiantOrbitStep;
			if (orbital is PlanetOrbit)
				return StarSystemVars.Instance.PlanetOrbitStep;
			throw new ArgumentException("unexpected type");
		}

		public static float CalcOrbitRadius(Kerberos.Sots.Data.StarMapFramework.Orbit orbitParent, int orbitNumber)
		{
			return Orbit.CalcOrbitRadius(StarSystemHelper.CalcRadius(orbitParent), StarSystemHelper.CalcOrbitStep(orbitParent), orbitNumber);
		}

		public static float ChoosePlanetSuitability(Random random)
		{
			return random.NextInclusive(Constants.MinSuitability, Constants.MaxSuitability);
		}

		public static PlanetInfo InferPlanetInfo(Kerberos.Sots.Data.StarMapFramework.Orbit orbit)
		{
			if (orbit is PlanetOrbit)
			{
				PlanetOrbit planetOrbit = orbit as PlanetOrbit;
				Random safeRandom = App.GetSafeRandom();
				float num1 = planetOrbit.Size.HasValue ? (float)planetOrbit.Size.Value : (float)safeRandom.Next(1, 10);
				string str = !string.IsNullOrEmpty(planetOrbit.PlanetType) ? planetOrbit.PlanetType : (safeRandom.NextNormal(0.0, 1.0) > 0.75 ? StellarBodyTypes.Normal : StellarBodyTypes.SpecialTerrestrialTypes[safeRandom.Next(0, ((IEnumerable<string>)StellarBodyTypes.SpecialTerrestrialTypes).Count<string>())]);
				float num2 = planetOrbit.Suitability.HasValue ? planetOrbit.Suitability.Value : (float)safeRandom.Next(-1000, 1000);
				int num3 = planetOrbit.Biosphere.HasValue ? planetOrbit.Biosphere.Value : safeRandom.Next(500, 1500);
				int num4 = planetOrbit.Resources.HasValue ? planetOrbit.Resources.Value : safeRandom.Next(1500, 8000);
				return new PlanetInfo()
				{
					Size = num1,
					Type = str,
					Suitability = num2,
					Biosphere = num3,
					Resources = num4
				};
			}
			if (orbit is MoonOrbit)
			{
				MoonOrbit moonOrbit = orbit as MoonOrbit;
				Random safeRandom = App.GetSafeRandom();
				float num1 = moonOrbit.Size.HasValue ? moonOrbit.Size.Value : (float)(0.100000001490116 + (double)safeRandom.NextSingle() * 0.400000005960464);
				int num2 = safeRandom.Next(1000, 7000);
				return new PlanetInfo()
				{
					Size = num1,
					Resources = num2,
					Type = StellarBodyTypes.Barren
				};
			}
			if (orbit is GasGiantSmallOrbit)
			{
				GasGiantSmallOrbit gasGiantSmallOrbit = orbit as GasGiantSmallOrbit;
				Random safeRandom = App.GetSafeRandom();
				float num = gasGiantSmallOrbit.Size.HasValue ? gasGiantSmallOrbit.Size.Value : (float)safeRandom.Next(13, 18);
				return new PlanetInfo()
				{
					Size = num,
					Type = StellarBodyTypes.Gaseous
				};
			}
			if (!(orbit is GasGiantLargeOrbit))
				return (PlanetInfo)null;
			GasGiantLargeOrbit gasGiantLargeOrbit = orbit as GasGiantLargeOrbit;
			Random safeRandom1 = App.GetSafeRandom();
			if (!gasGiantLargeOrbit.Size.HasValue)
			{
				safeRandom1.Next(19, 30);
			}
			else
			{
				double num5 = (double)gasGiantLargeOrbit.Size.Value;
			}
			return new PlanetInfo()
			{
				Size = gasGiantLargeOrbit.Size.Value,
				Type = StellarBodyTypes.Gaseous
			};
		}

		public static float ChooseSize(Random random, float minRadius, float maxRadius)
		{
			float num1 = 1f;
			float num2 = (float)(((double)minRadius + (double)maxRadius) / 2.0) * num1;
			return StarSystemVars.Instance.RadiusToSize((float)random.NextNormal((double)minRadius, (double)maxRadius, (double)num2));
		}

		public static float ChoosePlanetResources(Random random)
		{
			int num = (int)(5000.0 * 1.0);
			return (float)(int)random.NextNormal(2000.0, 8000.0, (double)num);
		}

		public static float ChoosePlanetBiosphere(Random random)
		{
			int num = (int)(4250.0 * 1.0);
			return (float)(int)random.NextNormal(500.0, 8000.0, (double)num);
		}

		private static List<Kerberos.Sots.Data.StarMapFramework.Orbit> PopulateRandomOrbitsCore(
		  Random random,
		  Kerberos.Sots.Data.StarMapFramework.Orbit parentOrbital)
		{
			if (parentOrbital is StarOrbit)
			{
				StellarClass stellarClass = StellarClass.Parse((parentOrbital as StarOrbit).StellarClass);
				return StarHelper.ChooseOrbitContents(random, stellarClass).ToList<Kerberos.Sots.Data.StarMapFramework.Orbit>();
			}
			if (parentOrbital is GasGiantLargeOrbit)
				return GasGiantHelper.ChooseOrbitContents(random).ToList<Kerberos.Sots.Data.StarMapFramework.Orbit>();
			if (parentOrbital is GasGiantSmallOrbit)
				return GasGiantHelper.ChooseOrbitContents(random).ToList<Kerberos.Sots.Data.StarMapFramework.Orbit>();
			return new List<Kerberos.Sots.Data.StarMapFramework.Orbit>();
		}

		private static List<Kerberos.Sots.Data.StarMapFramework.Orbit> PopulateRandomOrbits(
		  Random random,
		  Kerberos.Sots.Data.StarMapFramework.Orbit parentOrbital)
		{
			List<Kerberos.Sots.Data.StarMapFramework.Orbit> orbitList = StarSystemHelper.PopulateRandomOrbitsCore(random, parentOrbital);
			orbitList.ForEach((Action<Kerberos.Sots.Data.StarMapFramework.Orbit>)(x => x.Parent = parentOrbital.Name));
			return orbitList;
		}

		private static IStellarEntity FindOrbitParent(
		  StarSystem system,
		  IStellarEntity orbiter)
		{
			if (!system.Objects.Contains<IStellarEntity>(orbiter))
				throw new ArgumentOutOfRangeException("System does not contain orbiter.");
			if (orbiter.Params == null)
				return (IStellarEntity)null;
			if (!string.IsNullOrEmpty(orbiter.Params.Parent))
				return system.Objects.FirstOrDefault<IStellarEntity>((Func<IStellarEntity, bool>)(x =>
			   {
				   if (x.Params != null)
					   return x.Params.Name == orbiter.Params.Parent;
				   return false;
			   }));
			if (orbiter.Params is StarOrbit)
				return (IStellarEntity)null;
			return system.Star;
		}

		private static void AssignOrbitNumbers(StellarClass stellarClass, List<Kerberos.Sots.Data.StarMapFramework.Orbit> orbitals)
		{
			int orbitNumber = StarHelper.CalcMinOrbit(stellarClass);
			foreach (Kerberos.Sots.Data.StarMapFramework.Orbit orbit in orbitals.Where<Kerberos.Sots.Data.StarMapFramework.Orbit>((Func<Kerberos.Sots.Data.StarMapFramework.Orbit, bool>)(x => x.OrbitNumber < 1)))
			{
				while (orbitals.Any<Kerberos.Sots.Data.StarMapFramework.Orbit>((Func<Kerberos.Sots.Data.StarMapFramework.Orbit, bool>)(x => x.OrbitNumber == orbitNumber)))
					++orbitNumber;
				orbit.OrbitNumber = orbitNumber;
				++orbitNumber;
			}
		}

		public static StarSystem CreateStarSystem(Random random, Matrix worldTransform, Kerberos.Sots.Data.StarMapFramework.StarSystem systemParams, LegacyTerrain parentTerrain)
		{
			StellarClass stellarClass = StarHelper.ResolveStellarClass(random, systemParams.Type, systemParams.SubType, systemParams.Size);
			Kerberos.Sots.Data.StarMapFramework.StarOrbit starOrbit = new StarOrbit();
			starOrbit.Name = systemParams.Name;
			starOrbit.StellarClass = stellarClass.ToString();
			int randomOrbital = 1;
			List<Kerberos.Sots.Data.StarMapFramework.Orbit> list = new List<Kerberos.Sots.Data.StarMapFramework.Orbit>();
			list.Add(starOrbit);
			for (int i = 0; i < list.Count; i++)
			{
				Kerberos.Sots.Data.StarMapFramework.Orbit thisOrbital = list[i];
				if (!(thisOrbital is EmptyOrbit))
				{
					List<Kerberos.Sots.Data.StarMapFramework.Orbit> predefinedOrbitals = new List<Kerberos.Sots.Data.StarMapFramework.Orbit>();
					predefinedOrbitals.AddRange(systemParams.Orbits.Where(delegate (Kerberos.Sots.Data.StarMapFramework.Orbit x)
					{
						if (string.IsNullOrEmpty(x.Parent))
						{
							return thisOrbital is StarOrbit;
						}
						return x.Parent == thisOrbital.Name;
					}));
					if (thisOrbital is StarOrbit)
					{
						StarSystemHelper.AssignOrbitNumbers(stellarClass, predefinedOrbitals);
					}
					else
					{
						int orbitNumber = 1;
						predefinedOrbitals.ForEach(delegate (Kerberos.Sots.Data.StarMapFramework.Orbit x)
						{
							x.OrbitNumber = orbitNumber++;
						});
					}
					List<Kerberos.Sots.Data.StarMapFramework.Orbit> list2 = StarSystemHelper.PopulateRandomOrbits(random, thisOrbital);
					list2.RemoveAll((Kerberos.Sots.Data.StarMapFramework.Orbit x) => predefinedOrbitals.Any((Kerberos.Sots.Data.StarMapFramework.Orbit y) => y.OrbitNumber == x.OrbitNumber));
					list2.ForEach(delegate (Kerberos.Sots.Data.StarMapFramework.Orbit x)
					{
						x.Name = string.Format("RandomOrbital{0}", ++randomOrbital);
					});
					list.AddRange(predefinedOrbitals);
					list.AddRange(list2);
				}
			}
			StarSystem starSystem = new StarSystem();
			starSystem.Params = systemParams;
			starSystem.WorldTransform = worldTransform;
			starSystem.DisplayName = systemParams.Name;
			starSystem.IsStartPosition = systemParams.isStartLocation;
			starSystem.WorldTransform = worldTransform;
			starSystem.Terrain = parentTerrain;
			foreach (Kerberos.Sots.Data.StarMapFramework.Orbit orbit in list)
			{
				bool isOrbitingStar = orbit.Parent == starOrbit.Name;
				List<IStellarEntity> objs = new List<IStellarEntity>(StarSystemHelper.CreateOrbiters(random, orbit, isOrbitingStar));
				starSystem.AddRange(objs);
			}
			foreach (IStellarEntity stellarEntity in starSystem.Objects)
			{
				IStellarEntity stellarEntity2 = StarSystemHelper.FindOrbitParent(starSystem, stellarEntity);
				if (stellarEntity2 != null)
				{
					stellarEntity.Orbit = StarSystem.SetOrbit(random, stellarEntity2.Params, stellarEntity.Params);
				}
			}
			return starSystem;
		}

		public static IEnumerable<IStellarEntity> CreateOrbiters(
		  Random random,
		  Kerberos.Sots.Data.StarMapFramework.Orbit orbiterParams,
		  bool isOrbitingStar)
		{
			if (orbiterParams.GetType() == typeof(EmptyOrbit))
				return Enumerable.Empty<IStellarEntity>();
			if (orbiterParams.GetType() == typeof(StarOrbit))
				return StarSystemHelper.CreateStar(random, orbiterParams as StarOrbit);
			if (orbiterParams.GetType() == typeof(ArtifactOrbit))
				return StarSystemHelper.CreateArtifact(random, orbiterParams as ArtifactOrbit);
			if (orbiterParams.GetType() == typeof(GasGiantSmallOrbit))
				return StarSystemHelper.CreateGasGiantSmall(random, orbiterParams as GasGiantSmallOrbit);
			if (orbiterParams.GetType() == typeof(GasGiantLargeOrbit))
				return StarSystemHelper.CreateGasGiantLarge(random, orbiterParams as GasGiantLargeOrbit);
			if (orbiterParams.GetType() == typeof(MoonOrbit))
				return StarSystemHelper.CreateMoon(random, orbiterParams as MoonOrbit);
			if (orbiterParams.GetType() == typeof(PlanetaryRingOrbit))
				return StarSystemHelper.CreatePlanetaryRing(random, orbiterParams as PlanetaryRingOrbit);
			if (orbiterParams.GetType() == typeof(PlanetOrbit))
				return StarSystemHelper.CreatePlanet(random, orbiterParams as PlanetOrbit, isOrbitingStar);
			if (orbiterParams.GetType() == typeof(AsteroidOrbit))
				return StarSystemHelper.CreateAsteroidBelt(random, orbiterParams as AsteroidOrbit);
			throw new ArgumentException(string.Format("Unsupported orbit type '{0}'.", (object)orbiterParams.GetType()));
		}

		public static IEnumerable<IStellarEntity> CreateStar(
		  Random random,
		  StarOrbit orbiterParams)
		{
			StellarClass.Parse(orbiterParams.StellarClass);
			return (IEnumerable<IStellarEntity>)new IStellarEntity[1]
			{
		(IStellarEntity) new StellarEntity()
		{
		  Params = (Kerberos.Sots.Data.StarMapFramework.Orbit) orbiterParams
		}
			};
		}

		public static IEnumerable<IStellarEntity> CreateArtifact(
		  Random random,
		  ArtifactOrbit orbiterParams)
		{
			return (IEnumerable<IStellarEntity>)new IStellarEntity[1]
			{
		(IStellarEntity) new StellarEntity()
		{
		  Params = (Kerberos.Sots.Data.StarMapFramework.Orbit) orbiterParams
		}
			};
		}

		public static IEnumerable<IStellarEntity> CreateGasGiantSmall(
		  Random random,
		  GasGiantSmallOrbit orbiterParams)
		{
			if (!orbiterParams.Size.HasValue)
				orbiterParams.Size = new float?(StarSystemHelper.ChooseSize(random, StarSystemVars.Instance.GasGiantMinRadiusSmall, StarSystemVars.Instance.GasGiantMaxRadiusSmall));
			return (IEnumerable<IStellarEntity>)new IStellarEntity[1]
			{
		(IStellarEntity) new StellarEntity()
		{
		  Params = (Kerberos.Sots.Data.StarMapFramework.Orbit) orbiterParams
		}
			};
		}

		public static IEnumerable<IStellarEntity> CreateGasGiantLarge(
		  Random random,
		  GasGiantLargeOrbit orbiterParams)
		{
			if (!orbiterParams.Size.HasValue)
				orbiterParams.Size = new float?(StarSystemHelper.ChooseSize(random, StarSystemVars.Instance.GasGiantMinRadiusLarge, StarSystemVars.Instance.GasGiantMaxRadiusLarge));
			return (IEnumerable<IStellarEntity>)new IStellarEntity[1]
			{
		(IStellarEntity) new StellarEntity()
		{
		  Params = (Kerberos.Sots.Data.StarMapFramework.Orbit) orbiterParams
		}
			};
		}

		public static IEnumerable<IStellarEntity> CreateMoon(
		  Random random,
		  MoonOrbit orbiterParams)
		{
			if (!orbiterParams.Size.HasValue)
				orbiterParams.Size = new float?(StarSystemHelper.ChooseSize(random, StarSystemVars.Instance.MoonMinRadius, StarSystemVars.Instance.MoonMaxRadius));
			return (IEnumerable<IStellarEntity>)new IStellarEntity[1]
			{
		(IStellarEntity) new StellarEntity()
		{
		  Params = (Kerberos.Sots.Data.StarMapFramework.Orbit) orbiterParams
		}
			};
		}

		public static IEnumerable<IStellarEntity> CreatePlanetaryRing(
		  Random random,
		  PlanetaryRingOrbit orbiterParams)
		{
			return (IEnumerable<IStellarEntity>)new IStellarEntity[1]
			{
		(IStellarEntity) new StellarEntity()
		{
		  Params = (Kerberos.Sots.Data.StarMapFramework.Orbit) orbiterParams
		}
			};
		}

		public static IEnumerable<IStellarEntity> CreatePlanet(
		  Random random,
		  PlanetOrbit orbiterParams,
		  bool isOrbitingStar)
		{
			if (!orbiterParams.Size.HasValue)
				orbiterParams.Size = !isOrbitingStar ? new int?(random.NextInclusive(StarSystemVars.Instance.HabitalMoonMinSize, StarSystemVars.Instance.HabitalMoonMaxSize)) : new int?((int)StarSystemHelper.ChooseSize(random, StarSystemVars.Instance.PlanetMinRadius, StarSystemVars.Instance.PlanetMaxRadius));
			return (IEnumerable<IStellarEntity>)new IStellarEntity[1]
			{
		(IStellarEntity) new StellarEntity()
		{
		  Params = (Kerberos.Sots.Data.StarMapFramework.Orbit) orbiterParams
		}
			};
		}

		public static IEnumerable<IStellarEntity> CreateAsteroidBelt(
		  Random random,
		  AsteroidOrbit orbiterParams)
		{
			return (IEnumerable<IStellarEntity>)new IStellarEntity[1]
			{
		(IStellarEntity) new StellarEntity()
		{
		  Params = (Kerberos.Sots.Data.StarMapFramework.Orbit) orbiterParams
		}
			};
		}

		public static int GetOrbitCount(Orbits orbits)
		{
			return Math.Max(0, (int)orbits);
		}

		public static Kerberos.Sots.Data.StarMapFramework.Orbit CreateOrbiterParams(
		  OrbitContents contents)
		{
			switch (contents)
			{
				case OrbitContents.Empty:
					return (Kerberos.Sots.Data.StarMapFramework.Orbit)new EmptyOrbit();
				case OrbitContents.Artifact:
					return (Kerberos.Sots.Data.StarMapFramework.Orbit)new ArtifactOrbit();
				case OrbitContents.Planet:
					return (Kerberos.Sots.Data.StarMapFramework.Orbit)new PlanetOrbit();
				case OrbitContents.GasGiantLarge:
					return (Kerberos.Sots.Data.StarMapFramework.Orbit)new GasGiantLargeOrbit();
				case OrbitContents.GasGiantSmall:
					return (Kerberos.Sots.Data.StarMapFramework.Orbit)new GasGiantSmallOrbit();
				case OrbitContents.AsteroidBelt:
					return (Kerberos.Sots.Data.StarMapFramework.Orbit)new AsteroidOrbit();
				case OrbitContents.Moon:
					return (Kerberos.Sots.Data.StarMapFramework.Orbit)new MoonOrbit();
				case OrbitContents.PlanetaryRing:
					return (Kerberos.Sots.Data.StarMapFramework.Orbit)new PlanetaryRingOrbit();
				default:
					throw new ArgumentOutOfRangeException(string.Format("Unhandled OrbitContents.{0}", (object)contents));
			}
		}

		internal static void VerifyStarMap(LegacyStarMap starmap)
		{
			foreach (StarSystem starSystem in starmap.Objects.OfType<StarSystem>())
			{
				foreach (PlanetOrbit planetOrbit in starSystem.Objects.Where<IStellarEntity>((Func<IStellarEntity, bool>)(x => x.Params is PlanetOrbit)).Select<IStellarEntity, PlanetOrbit>((Func<IStellarEntity, PlanetOrbit>)(x => (PlanetOrbit)x.Params)).ToList<PlanetOrbit>())
					;
			}
		}
	}
}
