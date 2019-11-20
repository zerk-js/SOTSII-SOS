// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.StarMapElements.LegacyStarMap
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.StarMapElements
{
	internal class LegacyStarMap
	{
		private readonly List<ILegacyStarMapObject> _objects = new List<ILegacyStarMapObject>();
		private readonly List<NodeLine> _nodelines = new List<NodeLine>();
		private readonly List<Province> _provinces = new List<Province>();
		private readonly List<LegacyTerrain> _terrain = new List<LegacyTerrain>();

		public IEnumerable<ILegacyStarMapObject> Objects
		{
			get
			{
				return (IEnumerable<ILegacyStarMapObject>)this._objects;
			}
		}

		public IEnumerable<NodeLine> NodeLines
		{
			get
			{
				return (IEnumerable<NodeLine>)this._nodelines;
			}
		}

		public IEnumerable<LegacyTerrain> Terrain
		{
			get
			{
				return (IEnumerable<LegacyTerrain>)this._terrain;
			}
		}

		public IEnumerable<PlanetOrbit> PlanetOrbits
		{
			get
			{
				foreach (Kerberos.Sots.StarSystem starSystem in this._objects.OfType<Kerberos.Sots.StarSystem>())
				{
					foreach (IStellarEntity stellarEntity in starSystem.Objects.Where<IStellarEntity>((Func<IStellarEntity, bool>)(x => x.Params is PlanetOrbit)))
						yield return (PlanetOrbit)stellarEntity.Params;
				}
			}
		}

		public void Add(ILegacyStarMapObject item)
		{
			this._objects.Add(item);
		}

		public void AddRange(IEnumerable<ILegacyStarMapObject> items)
		{
			this._objects.AddRange(items);
		}

		public static LegacyStarMap CreateStarMapFromFileCore(
		  Random random,
		  string starMapPath)
		{
			Starmap s = new Starmap();
			StarMapXmlUtility.LoadStarmapFromXml(starMapPath, ref s);
			return LegacyStarMap.CreateStarMap(random, s);
		}

		public static LegacyStarMap CreateStarMap(Random random, Starmap starmapParams)
		{
			LegacyStarMap map = new LegacyStarMap();
			foreach (Feature feature in starmapParams.Features)
				map.AddRange(LegacyStarMap.CreateFeature(random, feature.LocalSpace, feature, map, (LegacyTerrain)null));
			map._nodelines.AddRange((IEnumerable<NodeLine>)starmapParams.NodeLines);
			map._provinces.AddRange((IEnumerable<Province>)starmapParams.Provinces);
			map.FixPlanetTypes();
			return map;
		}

		private void FixPlanetTypes()
		{
			foreach (PlanetOrbit planetOrbit in this.PlanetOrbits.Where<PlanetOrbit>((Func<PlanetOrbit, bool>)(x => !((IEnumerable<string>)StellarBodyTypes.TerrestrialTypes).Contains<string>(x.PlanetType.ToLowerInvariant()))))
			{
				if (!string.IsNullOrWhiteSpace(planetOrbit.PlanetType))
					App.Log.Warn(string.Format("PlanetType '{0}' for planet '{1}' is invalid.", (object)planetOrbit.PlanetType, (object)planetOrbit.Name), "data");
				planetOrbit.PlanetType = StellarBodyTypes.Normal;
			}
		}

		public static IEnumerable<ILegacyStarMapObject> CreateFeature(
		  Random random,
		  Matrix worldTransform,
		  Feature featureParams,
		  LegacyStarMap map,
		  LegacyTerrain parentTerrain)
		{
			if (featureParams.GetType() == typeof(Kerberos.Sots.Data.StarMapFramework.Terrain))
				return LegacyStarMap.CreateTerrain(random, worldTransform, featureParams as Kerberos.Sots.Data.StarMapFramework.Terrain, map);
			if (featureParams.GetType() == typeof(Kerberos.Sots.Data.StarMapFramework.StarSystem))
				return LegacyStarMap.CreateStarSystem(random, worldTransform, featureParams as Kerberos.Sots.Data.StarMapFramework.StarSystem, parentTerrain);
			if (featureParams.GetType() == typeof(StellarBody))
				return LegacyStarMap.CreateStellarObject(random, worldTransform, featureParams as StellarBody);
			throw new ArgumentException(string.Format("Unsupported starmap feature '{0}'.", (object)featureParams.GetType()));
		}

		public static IEnumerable<ILegacyStarMapObject> CreateTerrain(
		  Random random,
		  Matrix worldTransform,
		  Kerberos.Sots.Data.StarMapFramework.Terrain terrainParams,
		  LegacyStarMap map)
		{
			LegacyTerrain parentTerrain = new LegacyTerrain();
			List<ILegacyStarMapObject> legacyStarMapObjectList = new List<ILegacyStarMapObject>();
			foreach (Feature feature in terrainParams.Features)
			{
				Matrix worldTransform1 = feature.LocalSpace * worldTransform;
				legacyStarMapObjectList.AddRange(LegacyStarMap.CreateFeature(random, worldTransform1, feature, map, parentTerrain));
			}
			parentTerrain.Name = terrainParams.Name;
			parentTerrain.Origin = new Vector3(worldTransform.M41, worldTransform.M42, worldTransform.M43);
			map._terrain.Add(parentTerrain);
			map._nodelines.AddRange((IEnumerable<NodeLine>)terrainParams.NodeLines);
			map._provinces.AddRange((IEnumerable<Province>)terrainParams.Provinces);
			return (IEnumerable<ILegacyStarMapObject>)legacyStarMapObjectList;
		}

		public static IEnumerable<ILegacyStarMapObject> CreateStellarObject(
		  Random random,
		  Matrix worldTransform,
		  StellarBody stellarBodyParams)
		{
			return (IEnumerable<ILegacyStarMapObject>)new ILegacyStarMapObject[1]
			{
		(ILegacyStarMapObject) new StellarProp()
		{
		  Transform = worldTransform,
		  Params = stellarBodyParams
		}
			};
		}

		public static IEnumerable<ILegacyStarMapObject> CreateStarSystem(
		  Random random,
		  Matrix worldTransform,
		  Kerberos.Sots.Data.StarMapFramework.StarSystem systemParams,
		  LegacyTerrain parentTerrain)
		{
			return (IEnumerable<ILegacyStarMapObject>)new ILegacyStarMapObject[1]
			{
		(ILegacyStarMapObject) StarSystemHelper.CreateStarSystem(random, worldTransform, systemParams, parentTerrain)
			};
		}

		private static int CompareByOrbitNumber(IStellarEntity x, IStellarEntity y)
		{
			return x.Orbit.OrbitNumber.CompareTo(y.Orbit.OrbitNumber);
		}

		private static bool IsRandomOrbitName(Kerberos.Sots.Data.StarMapFramework.Orbit orbital)
		{
			return string.IsNullOrWhiteSpace(orbital.Name) || orbital.Name.Contains("NewOrbit") || orbital.Name.Contains("RandomOrbital");
		}

		internal void AssignEmptyPlanetTypes(Random random)
		{
			List<string> stringList = (List<string>)null;
			foreach (PlanetOrbit planetOrbit in this.PlanetOrbits.Where<PlanetOrbit>((Func<PlanetOrbit, bool>)(x => x.PlanetType.ToLowerInvariant() == StellarBodyTypes.Normal)))
			{
				if (random.NextInclusive(0, 100) <= 10)
				{
					if (stringList == null || stringList.Count == 0)
						stringList = ((IEnumerable<string>)StellarBodyTypes.TerrestrialTypes).ToList<string>();
					int index = random.NextInclusive(0, stringList.Count - 1);
					planetOrbit.PlanetType = stringList[index];
					stringList.RemoveAt(index);
				}
			}
		}

		internal void AssignEmptySystemNames(Random random, NamesPool namesPool)
		{
			foreach (Kerberos.Sots.StarSystem starSystem in this.Objects.OfType<Kerberos.Sots.StarSystem>())
			{
				if (string.IsNullOrWhiteSpace(starSystem.DisplayName) || starSystem.DisplayName.ToLower() == "random system")
					starSystem.DisplayName = namesPool.GetSystemName();
				List<IStellarEntity> list1 = starSystem.GetPlanets().ToList<IStellarEntity>();
				list1.Sort(new Comparison<IStellarEntity>(LegacyStarMap.CompareByOrbitNumber));
				int num1 = 0;
				foreach (IStellarEntity planet in list1)
				{
					++num1;
					if (LegacyStarMap.IsRandomOrbitName(planet.Params))
						planet.Params.Name = string.Format("{0} {1}", (object)starSystem.DisplayName, (object)num1);
					List<IStellarEntity> list2 = starSystem.GetMoons(planet).ToList<IStellarEntity>();
					list2.Sort(new Comparison<IStellarEntity>(LegacyStarMap.CompareByOrbitNumber));
					int num2 = 0;
					foreach (IStellarEntity stellarEntity in list2.Where<IStellarEntity>((Func<IStellarEntity, bool>)(x => LegacyStarMap.IsRandomOrbitName(x.Params))))
					{
						stellarEntity.Params.Name = string.Format("{0}{1}", (object)planet.Params.Name, (object)(char)(65 + num2));
						++num2;
					}
				}
				List<IStellarEntity> list3 = starSystem.GetAsteroidBelts().ToList<IStellarEntity>();
				list3.Sort(new Comparison<IStellarEntity>(LegacyStarMap.CompareByOrbitNumber));
				int num3 = 0;
				foreach (IStellarEntity stellarEntity in list3)
				{
					++num3;
					if (LegacyStarMap.IsRandomOrbitName(stellarEntity.Params))
						stellarEntity.Params.Name = string.Format("{0} " + App.Localize("@UI_BELT_NAME_MOD") + " {1}", (object)starSystem.DisplayName, (object)num3);
				}
			}
		}

		internal void AssignEmptyPlanetParameters(Random random)
		{
			foreach (Kerberos.Sots.StarSystem starSystem in this.Objects.OfType<Kerberos.Sots.StarSystem>())
			{
				foreach (IStellarEntity stellarEntity in starSystem.Objects)
				{
					if (stellarEntity.Params is PlanetOrbit)
					{
						PlanetOrbit planetOrbit = stellarEntity.Params as PlanetOrbit;
						if (!planetOrbit.Suitability.HasValue)
							planetOrbit.Suitability = new float?(StarSystemHelper.ChoosePlanetSuitability(random));
						if (!planetOrbit.Resources.HasValue)
							planetOrbit.Resources = new int?((int)StarSystemHelper.ChoosePlanetResources(random));
						if (!planetOrbit.Biosphere.HasValue)
							planetOrbit.Biosphere = new int?((int)StarSystemHelper.ChoosePlanetBiosphere(random));
						if (!planetOrbit.Size.HasValue)
							planetOrbit.Size = stellarEntity.Orbit == null || stellarEntity.Orbit.Parent != starSystem.Star.Params ? new int?(random.NextInclusive(StarSystemVars.Instance.HabitalMoonMinSize, StarSystemVars.Instance.HabitalMoonMaxSize)) : new int?((int)StarSystemHelper.ChooseSize(random, StarSystemVars.Instance.PlanetMinRadius, StarSystemVars.Instance.PlanetMaxRadius));
					}
					else if (stellarEntity.Params is GasGiantSmallOrbit)
					{
						GasGiantSmallOrbit gasGiantSmallOrbit = stellarEntity.Params as GasGiantSmallOrbit;
						if (!gasGiantSmallOrbit.Size.HasValue)
							gasGiantSmallOrbit.Size = new float?(StarSystemHelper.ChooseSize(random, StarSystemVars.Instance.GasGiantMinRadiusSmall, StarSystemVars.Instance.GasGiantMaxRadiusSmall));
					}
					else if (stellarEntity.Params is GasGiantLargeOrbit)
					{
						GasGiantLargeOrbit gasGiantLargeOrbit = stellarEntity.Params as GasGiantLargeOrbit;
						if (!gasGiantLargeOrbit.Size.HasValue)
							gasGiantLargeOrbit.Size = new float?(StarSystemHelper.ChooseSize(random, StarSystemVars.Instance.GasGiantMinRadiusLarge, StarSystemVars.Instance.GasGiantMaxRadiusLarge));
					}
					else if (stellarEntity.Params is MoonOrbit)
					{
						MoonOrbit moonOrbit = stellarEntity.Params as MoonOrbit;
						if (!moonOrbit.Size.HasValue)
							moonOrbit.Size = new float?(StarSystemHelper.ChooseSize(random, StarSystemVars.Instance.MoonMinRadius, StarSystemVars.Instance.MoonMaxRadius));
					}
				}
			}
		}
	}
}
