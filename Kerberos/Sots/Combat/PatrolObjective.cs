// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.PatrolObjective
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using System;
using System.Collections.Generic;

namespace Kerberos.Sots.Combat
{
	internal class PatrolObjective : TacticalObjective
	{
		private Vector3 m_Destination;
		private float m_MinRadius;
		private float m_MaxRadius;
		private bool m_ForPolice;

		public PatrolObjective(Vector3 dest, float minRadius, float maxRadius)
		{
			this.m_ObjectiveType = ObjectiveType.PATROL;
			this.m_Destination = dest;
			this.m_TaskGroups = new List<TaskGroup>();
			this.m_MinRadius = minRadius;
			this.m_MaxRadius = maxRadius;
			this.m_RequestTaskGroup = false;
			this.m_ForPolice = false;
		}

		public PatrolObjective(Ship police, Vector3 dest, float minRadius, float maxRadius)
		{
			this.m_ObjectiveType = ObjectiveType.PATROL;
			this.m_Destination = dest;
			this.m_TaskGroups = new List<TaskGroup>();
			this.m_MinRadius = minRadius;
			this.m_MaxRadius = maxRadius;
			this.m_RequestTaskGroup = false;
			this.m_PoliceOwner = police;
			this.m_ForPolice = true;
		}

		public override void AssignResources(TaskGroup group)
		{
			base.AssignResources(group);
		}

		public override Vector3 GetObjectiveLocation()
		{
			return this.m_Destination;
		}

		public void SetPatrolDestination(Vector3 dest)
		{
			this.m_Destination = dest;
		}

		public override bool IsComplete()
		{
			if (!this.m_ForPolice)
				return false;
			if (this.m_PoliceOwner != null && this.m_PoliceOwner.IsPolicePatrolling)
				return !Ship.IsActiveShip(this.m_PoliceOwner);
			return true;
		}

		public override int ResourceNeeds()
		{
			return 1;
		}

		public List<Vector3> GetPatrolWaypoints(PatrolType pt, Vector3 dir, float maxDist)
		{
			List<Vector3> vector3List = new List<Vector3>();
			float num1 = Math.Min(this.m_MaxRadius - this.m_MinRadius, maxDist);
			Vector3 vector3_1 = this.m_Destination + dir * this.m_MinRadius;
			Vector3 vector3_2 = Vector3.Cross(dir, Vector3.UnitY);
			switch (pt)
			{
				case PatrolType.Circular:
					float num2 = num1 * 0.5f;
					Vector3 vector3_3 = vector3_1 + dir * num2;
					int num3 = 8;
					float radians1 = MathHelper.DegreesToRadians(360f / (float)num3);
					for (int index = 0; index < num3; ++index)
					{
						float num4 = radians1 * (float)index;
						Vector3 vector3_4 = new Vector3((float)Math.Sin((double)num4), 0.0f, -(float)Math.Cos((double)num4));
						vector3List.Add(vector3_3 + vector3_4 * num2);
					}
					break;
				case PatrolType.Line:
					float num5 = 1000f;
					Vector3 vector3_5 = vector3_1 + dir * num5;
					vector3List.Add(vector3_5 + vector3_2 * num1);
					vector3List.Add(vector3_5 - vector3_2 * num1);
					break;
				case PatrolType.Box:
					vector3List.Add(this.m_Destination + dir * this.m_MaxRadius + vector3_2 * num1);
					vector3List.Add(this.m_Destination + dir * this.m_MaxRadius - vector3_2 * num1);
					vector3List.Add(this.m_Destination + dir * this.m_MinRadius + vector3_2 * num1);
					vector3List.Add(this.m_Destination + dir * this.m_MinRadius - vector3_2 * num1);
					break;
				case PatrolType.Orbit:
					float num6 = 1000f;
					int num7 = 8;
					float radians2 = MathHelper.DegreesToRadians(360f / (float)num7);
					for (int index = 0; index < num7; ++index)
					{
						float num4 = radians2 * (float)index;
						Vector3 vector3_4 = new Vector3((float)Math.Sin((double)num4), 0.0f, -(float)Math.Cos((double)num4));
						vector3List.Add(this.m_Destination + vector3_4 * (this.m_MinRadius + num6));
					}
					break;
			}
			return vector3List;
		}
	}
}
