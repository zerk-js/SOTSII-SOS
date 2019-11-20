// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.DetectionSpheres
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;

namespace Kerberos.Sots.GameStates
{
	public class DetectionSpheres
	{
		public int playerID;
		public Vector3 center;
		public float minRadius;
		public float sensorRange;
		public float slewModeRange;
		public bool isPlanet;
		public bool ignoreNeutralPlanets;

		public DetectionSpheres(int p, Vector3 c)
		{
			this.playerID = p;
			this.center = c;
			this.minRadius = 0.0f;
			this.sensorRange = 0.0f;
			this.slewModeRange = 0.0f;
			this.isPlanet = false;
			this.ignoreNeutralPlanets = false;
		}
	}
}
