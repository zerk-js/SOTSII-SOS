// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.CloakedShipDetection
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal class CloakedShipDetection
	{
		private static int kCloakTimeoutFrames = 600;
		private Dictionary<Ship, int> m_EnemyShips;

		public CloakedShipDetection()
		{
			this.m_EnemyShips = new Dictionary<Ship, int>();
		}

		public void AddShip(Ship ship)
		{
			if (this.m_EnemyShips.ContainsKey(ship))
				return;
			this.m_EnemyShips.Add(ship, CloakedShipDetection.kCloakTimeoutFrames);
		}

		public void RemoveShip(Ship ship)
		{
			if (!this.m_EnemyShips.ContainsKey(ship))
				return;
			this.m_EnemyShips.Remove(ship);
		}

		public void ShipSpotted(Ship ship)
		{
			if (!this.m_EnemyShips.ContainsKey(ship))
				return;
			this.m_EnemyShips[ship] = CloakedShipDetection.kCloakTimeoutFrames;
		}

		public float GetVisibilityPercent(Ship ship)
		{
			if (this.m_EnemyShips.ContainsKey(ship))
				return (float)this.m_EnemyShips[ship] / (float)CloakedShipDetection.kCloakTimeoutFrames;
			return 1f;
		}

		public void UpdateCloakedDetection(int elapsedFrames)
		{
			if (this.m_EnemyShips.Count == 0)
				return;
			foreach (Ship index in this.m_EnemyShips.Keys.ToList<Ship>())
				this.m_EnemyShips[index] = index.CloakedState != CloakedState.Cloaking ? CloakedShipDetection.kCloakTimeoutFrames : Math.Max(this.m_EnemyShips[index] - elapsedFrames, 0);
		}
	}
}
