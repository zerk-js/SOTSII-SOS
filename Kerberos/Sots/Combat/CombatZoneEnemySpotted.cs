// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.CombatZoneEnemySpotted
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal class CombatZoneEnemySpotted
	{
		private Dictionary<Ship, bool> m_EnemyShips;
		private Kerberos.Sots.GameStates.StarSystem m_StarSystem;

		public CombatZoneEnemySpotted(Kerberos.Sots.GameStates.StarSystem starSystem)
		{
			this.m_EnemyShips = new Dictionary<Ship, bool>();
			this.m_StarSystem = starSystem;
		}

		public void AddShip(Ship ship, bool seenByDefault = false)
		{
			if (this.m_EnemyShips.ContainsKey(ship))
				return;
			this.m_EnemyShips.Add(ship, seenByDefault);
		}

		public void RemoveShip(Ship ship)
		{
			if (!this.m_EnemyShips.ContainsKey(ship))
				return;
			this.m_EnemyShips.Remove(ship);
		}

		public void SetEnemySpotted(Ship ship)
		{
			if (!this.m_EnemyShips.ContainsKey(ship))
				return;
			this.m_EnemyShips[ship] = true;
		}

		public bool IsShipSpotted(Ship ship)
		{
			if (this.m_EnemyShips.ContainsKey(ship))
				return this.m_EnemyShips[ship];
			return false;
		}

		public void UpdateSpottedShipsInZone(CombatZonePositionInfo cz)
		{
			if (this.m_StarSystem == null || cz == null || cz.Player == 0)
				return;
			int combatZoneIndex = Kerberos.Sots.GameStates.StarSystem.GetCombatZoneIndex(cz.RingIndex, cz.ZoneIndex);
			List<Ship> shipList = new List<Ship>();
			foreach (KeyValuePair<Ship, bool> enemyShip in this.m_EnemyShips)
			{
				if (enemyShip.Key.Player != null && enemyShip.Key.Player.ID == cz.Player && (!enemyShip.Value && combatZoneIndex == this.m_StarSystem.GetCombatZoneIndexAtPosition(enemyShip.Key.Maneuvering.Position)))
					shipList.Add(enemyShip.Key);
			}
			foreach (Ship index in shipList)
				this.m_EnemyShips[index] = true;
		}

		public List<Ship> GetDetectedShips()
		{
			List<Ship> shipList = new List<Ship>();
			foreach (KeyValuePair<Ship, bool> enemyShip in this.m_EnemyShips)
			{
				if (enemyShip.Value)
					shipList.Add(enemyShip.Key);
			}
			return shipList;
		}

		public List<Vector3> GetAllEntryPoints()
		{
			List<Vector3> source = new List<Vector3>();
			if (this.m_StarSystem != null)
			{
				CombatZonePositionInfo zonePositionInfo = this.m_StarSystem.GetCombatZonePositionInfo(this.m_StarSystem.GetFurthestRing() - 1, 0);
				float num = zonePositionInfo != null ? zonePositionInfo.RadiusUpper : (float)(((double)this.m_StarSystem.GetBaseOffset() + (double)((IEnumerable<float>)Kerberos.Sots.GameStates.StarSystem.CombatZoneMapRadii).Last<float>()) * 5700.0);
				foreach (NeighboringSystemInfo neighboringSystem in this.m_StarSystem.NeighboringSystems)
				{
					Vector3 pos = neighboringSystem.DirFromSystem * num;
					if (!source.Any<Vector3>((Func<Vector3, bool>)(x => (double)(x - pos).LengthSquared < 10000.0)))
						source.Add(pos);
				}
			}
			return source;
		}
	}
}
