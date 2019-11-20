// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.VonNeumannPyramidControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using System.Collections.Generic;

namespace Kerberos.Sots.Combat
{
	internal class VonNeumannPyramidControl : CombatAIController
	{
		private App m_Game;
		private Ship m_Pyramid;
		private IGameObject m_Target;
		private VonNeumannNeoBerserkerControl m_ParentBerserker;

		public override Ship GetShip()
		{
			return this.m_Pyramid;
		}

		public override void SetTarget(IGameObject target)
		{
			if (target == this.m_Target || this.m_Pyramid == null)
				return;
			this.m_Target = target;
			this.m_Pyramid.SetShipTarget(target != null ? target.ObjectID : 0, Vector3.Zero, true, 0);
		}

		public override IGameObject GetTarget()
		{
			return this.m_Target;
		}

		public VonNeumannPyramidControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_Pyramid = ship;
		}

		public override void Initialize()
		{
			this.m_Target = (IGameObject)null;
			this.m_ParentBerserker = (VonNeumannNeoBerserkerControl)null;
		}

		public override void Terminate()
		{
		}

		public override void ObjectRemoved(IGameObject obj)
		{
			if (this.m_Target == obj)
				this.SetTarget((IGameObject)null);
			if (this.m_ParentBerserker == null || this.m_ParentBerserker.GetShip() != obj)
				return;
			this.m_ParentBerserker = (VonNeumannNeoBerserkerControl)null;
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
			return this.m_Target == null;
		}

		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			if (this.m_ParentBerserker == null || !(this.m_ParentBerserker.GetTarget() is StellarBody))
			{
				float num = float.MaxValue;
				IGameObject target = (IGameObject)null;
				foreach (IGameObject gameObject in objs)
				{
					if (gameObject is StellarBody)
					{
						StellarBody stellarBody = gameObject as StellarBody;
						if (stellarBody.Parameters.ColonyPlayerID != this.m_Pyramid.Player.ID)
						{
							float lengthSquared = (stellarBody.Parameters.Position - this.m_Pyramid.Position).LengthSquared;
							if ((double)lengthSquared < (double)num)
							{
								target = (IGameObject)stellarBody;
								num = lengthSquared;
							}
						}
					}
				}
				this.SetTarget(target);
			}
			else
				this.SetTarget(this.m_ParentBerserker.GetTarget());
		}

		public override bool NeedsAParent()
		{
			return this.m_ParentBerserker == null;
		}

		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
			foreach (CombatAIController controller in controllers)
			{
				if (controller is VonNeumannNeoBerserkerControl)
				{
					VonNeumannNeoBerserkerControl berserkerControl = controller as VonNeumannNeoBerserkerControl;
					if (berserkerControl.IsThisMyMom(this.m_Pyramid))
					{
						berserkerControl.AddChild((CombatAIController)this);
						this.m_ParentBerserker = berserkerControl;
						break;
					}
				}
			}
		}
	}
}
