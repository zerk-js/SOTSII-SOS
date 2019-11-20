// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.CometCombatAIControl
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
	internal class CometCombatAIControl : CombatAIController
	{
		private App m_Game;
		private Ship m_Comet;
		private bool m_VictoryConditionsMet;
		private int m_UpdateRate;
		private StellarBody m_Target;
		private Vector3 m_TargetPosition;
		private Vector3 m_TargetFacing;
		private List<StellarBody> m_Planets;
		private SimpleAIStates m_State;

		public override Ship GetShip()
		{
			return this.m_Comet;
		}

		public override void SetTarget(IGameObject target)
		{
			if (!(target is StellarBody))
				return;
			this.m_Target = target as StellarBody;
		}

		public override IGameObject GetTarget()
		{
			return (IGameObject)this.m_Target;
		}

		public CometCombatAIControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_Comet = ship;
		}

		public override void Initialize()
		{
			this.m_State = SimpleAIStates.SEEK;
			this.m_VictoryConditionsMet = false;
			this.m_Planets = new List<StellarBody>();
			this.m_UpdateRate = 0;
			this.m_Target = (StellarBody)null;
			this.m_TargetFacing = -Vector3.UnitZ;
			this.m_TargetPosition = Vector3.Zero;
			CometGlobalData globalCometData = this.m_Game.Game.AssetDatabase.GlobalCometData;
			this.m_Comet.Maneuvering.PostSetProp("SetCombatAIDamage", (object)globalCometData.Damage.Crew, (object)globalCometData.Damage.Population, (object)globalCometData.Damage.InfraDamage, (object)globalCometData.Damage.TeraDamage);
		}

		public override void Terminate()
		{
		}

		public override void ObjectRemoved(IGameObject obj)
		{
			foreach (StellarBody planet in this.m_Planets)
			{
				if (planet == obj)
				{
					this.m_Planets.Remove(planet);
					break;
				}
			}
			if (obj != this.m_Comet)
				return;
			this.m_Comet = (Ship)null;
		}

		public override void OnThink()
		{
			if (this.m_Comet == null)
				return;
			switch (this.m_State)
			{
				case SimpleAIStates.SEEK:
					this.ThinkSeek();
					break;
				case SimpleAIStates.TRACK:
					this.ThinkTrack();
					break;
			}
		}

		public override void ForceFlee()
		{
		}

		public override bool VictoryConditionIsMet()
		{
			return this.m_VictoryConditionsMet;
		}

		public override bool RequestingNewTarget()
		{
			if (this.m_State == SimpleAIStates.SEEK)
				return this.m_Planets.Count == 0;
			return false;
		}

		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			foreach (IGameObject gameObject in objs)
			{
				if (gameObject is StellarBody)
					this.m_Planets.Add(gameObject as StellarBody);
			}
		}

		public override bool NeedsAParent()
		{
			return false;
		}

		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
		}

		private void ThinkSeek()
		{
			if (this.m_Planets.Count <= 0)
				return;
			this.ObtainPositionAndFacing();
			this.m_Comet.Maneuvering.PostAddGoal(this.m_TargetPosition, this.m_TargetFacing);
			this.m_State = SimpleAIStates.TRACK;
		}

		private void ThinkTrack()
		{
			--this.m_UpdateRate;
			if (this.m_UpdateRate > 0)
				return;
			this.m_UpdateRate = 15;
			this.m_Comet.Maneuvering.PostAddGoal(this.m_TargetPosition, this.m_TargetFacing);
		}

		private void ObtainPositionAndFacing()
		{
			this.m_TargetFacing = Matrix.CreateRotationYPR(this.m_Comet.Maneuvering.Rotation).Forward;
			this.m_TargetPosition = this.m_Comet.Maneuvering.Position + this.m_TargetFacing * 500000f;
		}
	}
}
