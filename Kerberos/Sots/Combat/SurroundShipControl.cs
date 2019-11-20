// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.SurroundShipControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;

namespace Kerberos.Sots.Combat
{
	internal class SurroundShipControl : StandOffShipControl
	{
		private Vector3 m_AttackVec;

		public SurroundShipControl(
		  App game,
		  TacticalObjective to,
		  CombatAI commanderAI,
		  Vector3 attackVector,
		  float minDist,
		  float desiredDist)
		  : base(game, to, commanderAI, minDist, desiredDist)
		{
			this.m_Type = ShipControlType.Surround;
			this.m_AttackVec = attackVector;
		}

		protected override Vector3 GetAttackVector(Vector3 currentPos, Vector3 enemyPos)
		{
			return this.m_AttackVec;
		}
	}
}
