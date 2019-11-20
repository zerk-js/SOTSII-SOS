// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.GardenerCombatAIControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using System.Collections.Generic;

namespace Kerberos.Sots.Combat
{
	internal class GardenerCombatAIControl : CombatAIController
	{
		private App m_Game;
		private Ship m_Gardener;

		public override void SetTarget(IGameObject target)
		{
			if (this.m_Gardener == null)
				return;
			this.m_Gardener.Target = target;
		}

		public override IGameObject GetTarget()
		{
			if (this.m_Gardener == null)
				return (IGameObject)null;
			return this.m_Gardener.Target;
		}

		public override Ship GetShip()
		{
			return this.m_Gardener;
		}

		public GardenerCombatAIControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_Gardener = ship;
		}

		public override void Initialize()
		{
		}

		public override void Terminate()
		{
		}

		public override void ObjectRemoved(IGameObject obj)
		{
		}

		public override void OnThink()
		{
		}

		public override void ForceFlee()
		{
		}

		public override bool VictoryConditionIsMet()
		{
			return false;
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

		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
		}
	}
}
