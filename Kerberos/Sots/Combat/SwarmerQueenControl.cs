// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.SwarmerQueenControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;

namespace Kerberos.Sots.Combat
{
	internal class SwarmerQueenControl : SwarmerSpawnerControl
	{
		private Ship m_SwarmerHive;
		private Vector3 m_SpawnPos;
		private float m_SpawnRadius;
		private bool m_HasHadHive;
		private int m_AttemptsToFindHive;
		private int m_TrackRate;
		private int m_ResetIdleDirDelay;
		private int m_ResetTargetRate;
		private int m_ResetHoldPos;
		private int m_CurrMaxResetHoldPos;
		private int m_HoldPosDuration;
		private float m_RotDir;
		private float m_HoldDist;
		private IGameObject m_Target;

		public override void SetTarget(IGameObject target)
		{
			this.m_SwarmerSpawner.SetShipTarget(target != null ? target.ObjectID : 0, Vector3.Zero, true, 0);
			this.m_Target = target;
		}

		public override IGameObject GetTarget()
		{
			return this.m_Target;
		}

		public SwarmerQueenControl(App game, Ship ship, int systemId)
		  : base(game, ship, systemId)
		{
		}

		public override void Initialize()
		{
			base.Initialize();
			this.m_Target = (IGameObject)null;
			this.m_TrackRate = 0;
			this.m_SwarmerHive = (Ship)null;
			this.m_HasHadHive = false;
			this.m_AttemptsToFindHive = 60;
			this.m_ResetIdleDirDelay = 0;
			this.m_ResetTargetRate = 0;
			this.m_ResetHoldPos = 0;
			this.m_HoldPosDuration = 0;
			this.m_CurrMaxResetHoldPos = 0;
			this.m_RotDir = -1f;
			this.m_HoldDist = 0.0f;
			this.m_SpawnPos = this.m_SwarmerSpawner.Position;
			this.m_SpawnPos.Y = 0.0f;
			this.m_SpawnRadius = this.m_SpawnPos.Length;
		}

		public override void OnThink()
		{
			if (this.m_SwarmerSpawner == null)
				return;
			this.UpdateTargetList();
			if (this.m_TargetList.Count > 0)
				this.MaintainMaxSwarmers();
			switch (this.m_State)
			{
				case SwarmerSpawnerStates.IDLE:
					this.ThinkIdle();
					break;
				case SwarmerSpawnerStates.EMITSWARMER:
					this.ThinkEmitAttackSwarmer();
					break;
				case SwarmerSpawnerStates.INTEGRATESWARMER:
					this.ThinkIntegrateAttackSwarmer();
					break;
				case SwarmerSpawnerStates.ADDINGSWARMERS:
					this.ThinkAddingSwarmers();
					break;
				case SwarmerSpawnerStates.LAUNCHSWARMER:
					this.ThinkLaunch();
					break;
				case SwarmerSpawnerStates.WAITFORLAUNCH:
					this.ThinkWaitForLaunch();
					break;
				case SwarmerSpawnerStates.SEEK:
					this.ThinkSeek();
					break;
				case SwarmerSpawnerStates.TRACK:
					this.ThinkTrack();
					break;
			}
		}

		protected override void ThinkIdle()
		{
			base.ThinkIdle();
			if (this.m_Target == null)
			{
				--this.m_ResetIdleDirDelay;
				if (this.m_ResetIdleDirDelay <= 0)
				{
					this.PickIdleDestination();
					this.m_ResetIdleDirDelay = 200;
				}
				this.FindQueenTarget();
			}
			if (this.m_Target == null)
				return;
			this.m_State = SwarmerSpawnerStates.SEEK;
		}

		protected void ThinkSeek()
		{
			if (this.m_Target != null)
				this.m_State = SwarmerSpawnerStates.TRACK;
			else
				this.m_State = SwarmerSpawnerStates.IDLE;
		}

		protected void ThinkTrack()
		{
			--this.m_ResetTargetRate;
			if (this.m_Target == null || this.m_ResetTargetRate <= 0)
			{
				if (this.m_Target == null)
				{
					this.m_HoldPosDuration = 0;
					this.m_ResetHoldPos = 0;
				}
				this.SetTarget((IGameObject)null);
				this.m_State = SwarmerSpawnerStates.SEEK;
			}
			else
			{
				--this.m_TrackRate;
				if (this.m_TrackRate > 0)
					return;
				this.m_TrackRate = 3;
				Vector3 vector3_1 = Vector3.Zero;
				if (this.m_Target is Ship)
					vector3_1 = (this.m_Target as Ship).Position;
				float num1 = Math.Max(1500f, CombatAI.GetMinEffectiveWeaponRange(this.m_SwarmerSpawner, false));
				Vector3 position = vector3_1;
				position.Y = 0.0f;
				Vector3 vector3_2 = position - this.m_SwarmerSpawner.Position;
				vector3_2.Y = 0.0f;
				Vector3 look = Vector3.Normalize(vector3_2);
				Vector3 forward = look;
				float num2 = num1 + 500f;
				float num3 = 0.0f;
				float lengthSquared = vector3_2.LengthSquared;
				if ((double)lengthSquared < (double)num2 * (double)num2)
				{
					if (this.m_HoldPosDuration <= 0)
					{
						if ((double)lengthSquared > (double)num3 * (double)num3)
						{
							this.m_ResetHoldPos -= this.m_TrackRate;
							if (this.m_ResetHoldPos <= 0)
							{
								Random random = new Random();
								this.m_CurrMaxResetHoldPos = random.NextInclusive(600, 800);
								this.m_ResetHoldPos = this.m_CurrMaxResetHoldPos;
								this.m_HoldPosDuration = 600;
								this.m_RotDir = random.CoinToss(0.5) ? -1f : 1f;
								this.m_HoldDist = vector3_2.Length * 0.9f;
							}
						}
						forward = (Matrix.CreateRotationY(this.m_RotDir * MathHelper.DegreesToRadians(30f)) * Matrix.CreateWorld(position, forward, Vector3.UnitY)).Forward;
					}
				}
				else
				{
					this.m_ResetHoldPos = this.m_CurrMaxResetHoldPos;
					this.m_HoldPosDuration = 0;
				}
				if (this.m_HoldPosDuration > 0)
				{
					this.m_HoldPosDuration -= this.m_TrackRate;
					num1 = this.m_HoldDist;
				}
				this.m_SwarmerSpawner.Maneuvering.PostAddGoal(position - forward * num1, look);
			}
		}

		private void PickIdleDestination()
		{
			Random random = new Random();
			Matrix matrix = Matrix.CreateRotationY(MathHelper.DegreesToRadians(random.NextInclusive(-10f, 10f))) * Matrix.CreateWorld(Vector3.Zero, Vector3.Normalize(this.m_SpawnPos), Vector3.UnitY);
			matrix.Position = matrix.Forward * (this.m_SpawnRadius + random.NextInclusive(-2500f, 2500f));
			this.m_SwarmerSpawner.Maneuvering.PostAddGoal(matrix.Position, -matrix.Forward);
		}

		private void FindQueenTarget()
		{
			if (this.m_TargetList.Count == 0 || this.m_SwarmerSpawner == null)
				return;
			Vector3 vector3 = this.HiveIsPresent() ? this.m_SwarmerHive.Position : this.m_SwarmerSpawner.Position;
			IGameObject target1 = (IGameObject)null;
			ShipClass sc2 = ShipClass.BattleRider;
			float num = float.MaxValue;
			foreach (SwarmerTarget target2 in this.m_TargetList)
			{
				if (target2.Target is Ship)
				{
					Ship target3 = target2.Target as Ship;
					ShipClass shipClass = target3.ShipClass;
					if (Ship.IsShipClassBigger(shipClass, sc2, true))
					{
						float lengthSquared = (target3.Position - vector3).LengthSquared;
						if ((double)lengthSquared < (double)num || sc2 != shipClass)
						{
							num = lengthSquared;
							target1 = target2.Target;
						}
						sc2 = shipClass;
					}
				}
			}
			this.SetTarget(target1);
			this.m_ResetTargetRate = 30;
		}

		public override bool NeedsAParent()
		{
			if (!this.m_HasHadHive)
				return this.m_AttemptsToFindHive > 0;
			return false;
		}

		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
			foreach (CombatAIController controller in controllers)
			{
				if (controller is SwarmerHiveControl)
				{
					this.m_HasHadHive = true;
					this.m_SwarmerHive = controller.GetShip();
					break;
				}
			}
			--this.m_AttemptsToFindHive;
		}

		private bool HiveIsPresent()
		{
			return this.m_SwarmerHive != null && !this.m_SwarmerHive.IsDestroyed;
		}
	}
}
