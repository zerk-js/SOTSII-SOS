// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.CommandMonitorCombatAIControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal class CommandMonitorCombatAIControl : CombatAIController
	{
		private App m_Game;
		private Ship m_Monitor;
		private Module m_Dish;
		private bool m_HasDish;
		private bool m_Failed;
		private int m_EncounterID;
		private List<NormalMonitorCombatAIControl> m_NormalMonitors;
		private MonitorAIStates m_State;

		public override Ship GetShip()
		{
			return this.m_Monitor;
		}

		public override void SetTarget(IGameObject target)
		{
		}

		public override IGameObject GetTarget()
		{
			return (IGameObject)null;
		}

		public CommandMonitorCombatAIControl(App game, Ship ship, int encounterID)
		{
			this.m_Game = game;
			this.m_Monitor = ship;
			this.m_EncounterID = encounterID;
		}

		public override void Initialize()
		{
			this.m_State = MonitorAIStates.IDLE;
			this.m_NormalMonitors = new List<NormalMonitorCombatAIControl>();
			this.m_Dish = this.m_Monitor.Modules.FirstOrDefault<Module>();
			this.m_HasDish = this.m_Dish != null;
			this.m_Failed = false;
		}

		public override void Terminate()
		{
		}

		public override void ObjectRemoved(IGameObject obj)
		{
			foreach (NormalMonitorCombatAIControl normalMonitor in this.m_NormalMonitors)
			{
				if (normalMonitor.GetShip() == obj)
				{
					this.m_NormalMonitors.Remove(normalMonitor);
					break;
				}
			}
			if (obj != this.m_Dish)
				return;
			this.m_Dish = (Module)null;
		}

		public override void OnThink()
		{
			if (this.m_Monitor == null)
				return;
			switch (this.m_State)
			{
				case MonitorAIStates.IDLE:
					this.ThinkIdle();
					break;
				case MonitorAIStates.DISABLE:
					this.ThinkDisable();
					break;
			}
		}

		public override void ForceFlee()
		{
		}

		public override bool VictoryConditionIsMet()
		{
			if (!this.m_Failed && (this.m_Monitor.IsDestroyed || this.m_HasDish && (this.m_Dish == null || !this.m_Dish.IsAlive)))
			{
				foreach (IGameObject turret in this.m_Monitor.Turrets)
					turret.PostSetProp("Disable", true);
				foreach (NormalMonitorCombatAIControl normalMonitor in this.m_NormalMonitors)
					normalMonitor.ForceDisable();
				if (this.m_Game.Game.ScriptModules.AsteroidMonitor != null && this.m_Monitor.DesignID == this.m_Game.Game.ScriptModules.AsteroidMonitor.MonitorCommandDesignId)
				{
					AsteroidMonitorInfo asteroidMonitorInfo = this.m_Game.GameDatabase.GetAsteroidMonitorInfo(this.m_EncounterID);
					if (asteroidMonitorInfo != null)
					{
						asteroidMonitorInfo.IsAggressive = false;
						this.m_Game.GameDatabase.UpdateAsteroidMonitorInfo(asteroidMonitorInfo);
					}
				}
				ShipInfo shipInfo = this.m_Game.GameDatabase.GetShipInfo(this.m_Monitor.DatabaseID, false);
				if (shipInfo != null)
				{
					if (this.m_HasDish && this.m_Dish != null && (!this.m_Dish.IsAlive && this.m_Dish.DestroyedByPlayer != 0))
					{
						Player gameObject = this.m_Game.GetGameObject<Player>(this.m_Dish.DestroyedByPlayer);
						if (gameObject != null && shipInfo != null)
							this.m_Game.Game.InsertNewMonitorSpecialProject(gameObject.ID, this.m_EncounterID, shipInfo.FleetID);
					}
					else if (this.m_Monitor.IsDestroyed)
					{
						this.m_Game.GameDatabase.RemoveFleet(shipInfo.FleetID);
						this.m_Game.GameDatabase.RemoveEncounter(this.m_EncounterID);
					}
				}
				this.m_Failed = true;
			}
			return this.m_Failed;
		}

		public override bool RequestingNewTarget()
		{
			return false;
		}

		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
		}

		public override bool NeedsAParent()
		{
			return false;
		}

		public void AddNormal(NormalMonitorCombatAIControl cmcac)
		{
			this.m_NormalMonitors.Add(cmcac);
		}

		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
		}

		private void ThinkIdle()
		{
			if (!this.m_Monitor.IsDestroyed && (!this.m_HasDish || this.m_Dish != null && this.m_Dish.IsAlive))
				return;
			this.m_State = MonitorAIStates.DISABLE;
		}

		private void ThinkDisable()
		{
		}
	}
}
