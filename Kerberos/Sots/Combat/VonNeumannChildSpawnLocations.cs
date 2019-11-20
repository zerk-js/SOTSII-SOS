// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.VonNeumannChildSpawnLocations
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;

namespace Kerberos.Sots.Combat
{
	internal class VonNeumannChildSpawnLocations
	{
		private Ship m_VonNeumannMom;
		private Ship m_VonNeumannChild;
		private Vector3 m_DirectionFromMom;
		private float m_OffsetDistance;

		public Ship GetChildInSpace()
		{
			return this.m_VonNeumannChild;
		}

		public Vector3 DirectionFromMom
		{
			get
			{
				return this.m_DirectionFromMom;
			}
		}

		public float OffsetDistance
		{
			get
			{
				return this.m_OffsetDistance;
			}
		}

		public VonNeumannChildSpawnLocations(Ship mom, Vector3 dir, float offset)
		{
			this.m_DirectionFromMom = dir;
			this.m_OffsetDistance = offset;
			this.m_VonNeumannMom = mom;
			this.m_VonNeumannChild = (Ship)null;
		}

		public bool CanSpawnAtLocation()
		{
			return this.m_VonNeumannChild == null;
		}

		public Vector3 GetSpawnLocation()
		{
			Matrix rotationYpr = Matrix.CreateRotationYPR(this.m_VonNeumannMom.Maneuvering.Rotation);
			rotationYpr.Position = this.m_VonNeumannMom.Maneuvering.Position;
			return Vector3.Transform(this.m_DirectionFromMom * this.m_OffsetDistance, rotationYpr);
		}

		public void SpawnAtLocation(Ship child)
		{
			this.m_VonNeumannChild = child;
		}

		public void Clear()
		{
			this.m_VonNeumannChild = (Ship)null;
		}

		public void Update()
		{
			if (this.m_VonNeumannChild == null || (double)(this.m_VonNeumannChild.Position - this.GetSpawnLocation()).LengthSquared <= 2500.0)
				return;
			this.Clear();
		}
	}
}
