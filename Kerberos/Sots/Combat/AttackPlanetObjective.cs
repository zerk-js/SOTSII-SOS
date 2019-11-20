// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.AttackPlanetObjective
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using System.Collections.Generic;

namespace Kerberos.Sots.Combat
{
	internal class AttackPlanetObjective : TacticalObjective
	{
		public AttackPlanetObjective(StellarBody planet)
		{
			this.m_ObjectiveType = ObjectiveType.ATTACK_TARGET;
			this.m_Planet = planet;
			this.m_TaskGroups = new List<TaskGroup>();
			this.m_RequestTaskGroup = false;
		}

		public override bool IsComplete()
		{
			return this.m_Planet.Population < 1.0;
		}

		public override Vector3 GetObjectiveLocation()
		{
			return this.m_Planet.Parameters.Position;
		}

		public override int ResourceNeeds()
		{
			return 1;
		}
	}
}
