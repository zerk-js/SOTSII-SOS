// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GasGiantHelper
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;

namespace Kerberos.Sots
{
	internal static class GasGiantHelper
	{
		private static IEnumerable<Weighted<Orbits>> OrbitCountWeights
		{
			get
			{
				yield return new Weighted<Orbits>(Orbits.Zero, StarSystemVars.Instance.GasGiantOrbitsWeightZero);
				yield return new Weighted<Orbits>(Orbits.One, StarSystemVars.Instance.GasGiantOrbitsWeightOne);
				yield return new Weighted<Orbits>(Orbits.Two, StarSystemVars.Instance.GasGiantOrbitsWeightTwo);
				yield return new Weighted<Orbits>(Orbits.Three, StarSystemVars.Instance.GasGiantOrbitsWeightThree);
				yield return new Weighted<Orbits>(Orbits.Four, StarSystemVars.Instance.GasGiantOrbitsWeightFour);
				yield return new Weighted<Orbits>(Orbits.Ring, StarSystemVars.Instance.GasGiantOrbitsWeightRing);
			}
		}

		private static IEnumerable<Weighted<OrbitContents>> OrbitContentWeights
		{
			get
			{
				yield return new Weighted<OrbitContents>(OrbitContents.Empty, StarSystemVars.Instance.GasGiantSatelliteWeightNone);
				yield return new Weighted<OrbitContents>(OrbitContents.Artifact, StarSystemVars.Instance.GasGiantSatelliteWeightArtifact);
				yield return new Weighted<OrbitContents>(OrbitContents.Moon, StarSystemVars.Instance.GasGiantSatelliteWeightMoon);
				yield return new Weighted<OrbitContents>(OrbitContents.Planet, StarSystemVars.Instance.GasGiantSatelliteWeightPlanet);
			}
		}

		public static IEnumerable<Kerberos.Sots.Data.StarMapFramework.Orbit> ChooseOrbitContents(
		  Random random)
		{
			Orbits orbits = WeightedChoices.Choose<Orbits>(random.NextDouble(), GasGiantHelper.OrbitCountWeights);
			int count = StarSystemHelper.GetOrbitCount(orbits);
			if (count >= 0)
			{
				for (int orbitNumber = 1; orbitNumber <= count; ++orbitNumber)
				{
					OrbitContents chosenContents = WeightedChoices.Choose<OrbitContents>(random.NextDouble(), GasGiantHelper.OrbitContentWeights);
					Kerberos.Sots.Data.StarMapFramework.Orbit orbiter = StarSystemHelper.CreateOrbiterParams(chosenContents);
					orbiter.OrbitNumber = orbitNumber;
					yield return orbiter;
				}
			}
			else if (orbits == Orbits.Ring)
			{
				int ringOrbitNumber = 1;
				Kerberos.Sots.Data.StarMapFramework.Orbit orbiter = StarSystemHelper.CreateOrbiterParams(OrbitContents.PlanetaryRing);
				orbiter.OrbitNumber = ringOrbitNumber;
				yield return orbiter;
			}
		}
	}
}
