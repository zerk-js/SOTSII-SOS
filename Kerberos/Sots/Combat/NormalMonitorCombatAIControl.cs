// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.NormalMonitorCombatAIControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using System.Collections.Generic;

namespace Kerberos.Sots.Combat
{
	internal class NormalMonitorCombatAIControl : CombatAIController
	{
		private App m_Game;
		private Ship m_Monitor;
		private CommandMonitorCombatAIControl m_Command;
		private MonitorAIStates m_State;
		private bool m_HadParent;
		private bool m_DisableWeapons;

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

		public NormalMonitorCombatAIControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_Monitor = ship;
		}

		public override void Initialize()
		{
			this.m_State = MonitorAIStates.DISABLE;
			this.m_Command = (CommandMonitorCombatAIControl)null;
			this.m_HadParent = false;
			this.m_DisableWeapons = false;
			foreach (IGameObject weaponBank in this.m_Monitor.WeaponBanks)
				weaponBank.PostSetProp("DisableAllTurrets", true);
		}

		public override void Terminate()
		{
		}

		public override void ObjectRemoved(IGameObject obj)
		{
			if (this.m_Command == null || obj != this.m_Command.GetShip())
				return;
			this.m_Command = (CommandMonitorCombatAIControl)null;
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
			if (!this.m_HadParent)
				return false;
			if (this.m_Command != null)
				return this.m_Command.VictoryConditionIsMet();
			return true;
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
			if (!this.m_HadParent)
				return this.m_Command == null;
			return false;
		}

		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
			foreach (CombatAIController controller in controllers)
			{
				if (controller is CommandMonitorCombatAIControl)
				{
					this.m_Command = controller as CommandMonitorCombatAIControl;
					this.m_Command.AddNormal(this);
					break;
				}
			}
			if (this.m_Command == null)
				return;
			this.m_HadParent = true;
		}

		public void ForceDisable()
		{
			this.m_DisableWeapons = true;
		}

		private void ThinkIdle()
		{
			if (this.m_Command == null || this.m_Command.GetShip() == null || this.m_Command.GetShip().IsDestroyed)
				this.m_Command = (CommandMonitorCombatAIControl)null;
			if (!this.m_Monitor.IsDestroyed && (!this.m_HadParent || this.m_Command != null) && !this.m_DisableWeapons)
				return;
			foreach (IGameObject weaponBank in this.m_Monitor.WeaponBanks)
				weaponBank.PostSetProp("DisableAllTurrets", true);
			this.m_State = MonitorAIStates.DISABLE;
		}

		private void ThinkDisable()
		{
			if (this.m_Monitor.IsDestroyed || this.m_Command == null || this.m_DisableWeapons)
				return;
			foreach (IGameObject weaponBank in this.m_Monitor.WeaponBanks)
				weaponBank.PostSetProp("DisableAllTurrets", false);
			this.m_State = MonitorAIStates.IDLE;
		}
	}
}
