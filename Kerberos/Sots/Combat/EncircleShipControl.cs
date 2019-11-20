// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.EncircleShipControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.Combat
{
	internal class EncircleShipControl : BaseAttackShipControl
	{
		public EncircleShipControl(App game, TacticalObjective to, CombatAI commanderAI, float offset)
		  : base(game, to, commanderAI)
		{
			this.m_Type = ShipControlType.Encircle;
		}

		protected override void OnAttackUpdate(int framesElapsed)
		{
		}
	}
}
