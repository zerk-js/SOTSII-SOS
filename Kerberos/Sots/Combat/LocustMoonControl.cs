// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.LocustMoonControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;

namespace Kerberos.Sots.Combat
{
	internal class LocustMoonControl : LocustNestControl
	{
		public LocustMoonControl(App game, Ship ship, int fleetId)
		  : base(game, ship, fleetId)
		{
		}

		protected override void PickTarget()
		{
			IGameObject target1 = (IGameObject)null;
			float num = float.MaxValue;
			ShipClass sc1 = ShipClass.Cruiser;
			foreach (LocustTarget target2 in this.m_TargetList)
			{
				if (target2.Target != null && !Ship.IsShipClassBigger(sc1, target2.Target.ShipClass, false))
				{
					float lengthSquared = (target2.Target.Position - this.m_LocustNest.Position).LengthSquared;
					if (target1 == null || (double)lengthSquared < (double)num)
					{
						target1 = (IGameObject)target2.Target;
						num = lengthSquared;
					}
				}
			}
			if (target1 == null)
			{
				foreach (StellarBody planet in this.m_Planets)
				{
					float lengthSquared = (planet.Parameters.Position - this.m_LocustNest.Maneuvering.Position).LengthSquared;
					if ((double)lengthSquared < (double)num)
					{
						target1 = (IGameObject)planet;
						num = lengthSquared;
					}
				}
			}
			this.SetTarget(target1);
		}
	}
}
