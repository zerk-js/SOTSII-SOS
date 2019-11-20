// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.StarSystem
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Framework;
using Kerberos.Sots.StarMapElements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots
{
	internal class StarSystem : ILegacyStarMapObject
	{
		private List<IStellarEntity> _objects = new List<IStellarEntity>();

		public int ID { get; set; }

		Feature ILegacyStarMapObject.Params
		{
			get
			{
				return (Feature)this.Params;
			}
		}

		public Kerberos.Sots.Data.StarMapFramework.StarSystem Params { get; set; }

		public string DisplayName { get; set; }

		public Matrix WorldTransform { get; set; }

		public bool IsStartPosition { get; set; }

		public LegacyTerrain Terrain { get; set; }

		public IStellarEntity Star
		{
			get
			{
				return this.Objects.FirstOrDefault<IStellarEntity>((Func<IStellarEntity, bool>)(x => x.Params is StarOrbit));
			}
		}

		public StellarClass StellarClass
		{
			get
			{
				return StellarClass.Parse((this.Star.Params as StarOrbit).StellarClass);
			}
		}

		public IEnumerable<IStellarEntity> Objects
		{
			get
			{
				return (IEnumerable<IStellarEntity>)this._objects;
			}
		}

		public void Add(IStellarEntity obj)
		{
			this._objects.Add(obj);
		}

		public void AddRange(IEnumerable<IStellarEntity> objs)
		{
			this._objects.AddRange(objs);
		}

		public IEnumerable<IStellarEntity> GetPlanets()
		{
			foreach (IStellarEntity stellarEntity in this.Objects.Where<IStellarEntity>((Func<IStellarEntity, bool>)(x =>
		   {
			   if (x.Orbit != null)
				   return x.Orbit.Parent == this.Star.Params;
			   return false;
		   })))
			{
				if (stellarEntity.Params is PlanetOrbit || stellarEntity.Params is GasGiantSmallOrbit || stellarEntity.Params is GasGiantLargeOrbit)
					yield return stellarEntity;
			}
		}

		public IEnumerable<IStellarEntity> GetAsteroidBelts()
		{
			foreach (IStellarEntity stellarEntity in this.Objects.Where<IStellarEntity>((Func<IStellarEntity, bool>)(x =>
		   {
			   if (x.Orbit != null)
				   return x.Orbit.Parent == this.Star.Params;
			   return false;
		   })))
			{
				if (stellarEntity.Params is AsteroidOrbit)
					yield return stellarEntity;
			}
		}

		public IEnumerable<IStellarEntity> GetColonizableWorlds(
		  bool planetsOnly)
		{
			foreach (IStellarEntity stellarEntity in this.Objects)
			{
				PlanetOrbit planet = stellarEntity.Params as PlanetOrbit;
				if (planet != null && stellarEntity.Orbit != null && (!planetsOnly || stellarEntity.Orbit.Parent == this.Star.Params))
					yield return stellarEntity;
			}
		}

		public IEnumerable<IStellarEntity> GetMoons(IStellarEntity planet)
		{
			foreach (IStellarEntity stellarEntity in this.Objects.Where<IStellarEntity>((Func<IStellarEntity, bool>)(x =>
		   {
			   if (x.Orbit != null)
				   return x.Orbit.Parent == planet.Params;
			   return false;
		   })))
			{
				if (stellarEntity.Params is MoonOrbit || stellarEntity.Params is PlanetOrbit)
					yield return stellarEntity;
			}
		}

		internal static Orbit SetOrbit(Random random, Kerberos.Sots.Data.StarMapFramework.Orbit orbitParent, Kerberos.Sots.Data.StarMapFramework.Orbit orbiter)
		{
			if (orbiter.OrbitNumber < 1)
				throw new ArgumentOutOfRangeException(string.Format("Orbit numbers start at 1."));
			float eccentricity = orbiter.Eccentricity.HasValue ? orbiter.Eccentricity.Value : (random == null ? 0.0f : random.NextNormal(StarSystemVars.Instance.OrbitEccentricityRange));
			float num1 = orbiter.Inclination.HasValue ? orbiter.Inclination.Value : (random == null ? 0.0f : random.NextNormal(StarSystemVars.Instance.OrbitInclinationRange));
			float semiMajorAxis = StarSystemHelper.CalcOrbitRadius(orbitParent, orbiter.OrbitNumber);
			float num2 = Ellipse.CalcSemiMinorAxis(semiMajorAxis, eccentricity);
			return new Orbit()
			{
				Parent = orbitParent,
				SemiMajorAxis = semiMajorAxis,
				SemiMinorAxis = num2,
				OrbitNumber = orbiter.OrbitNumber,
				Inclination = num1,
				Position = random.NextInclusive(0.0f, 1f)
			};
		}

		public static StarSystemVars Vars
		{
			get
			{
				return StarSystemVars.Instance;
			}
		}

		public static bool TraceEnabled { get; set; }

		public static void Trace(string format, params object[] args)
		{
			if (!StarSystem.TraceEnabled)
				return;
			App.Log.Trace(string.Format(format, args), nameof(StarSystem));
		}
	}
}
