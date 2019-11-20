// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.BaseAttackShipControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal class BaseAttackShipControl : TaskGroupShipControl
	{
		private static int kUpdateTargetSelection = 180;
		private static int kUpdateClearTarget = 3;
		private int m_UpdateTargetSelection;
		private int m_ClearTargetsUpdate;

		public BaseAttackShipControl(App game, TacticalObjective to, CombatAI commanderAI)
		  : base(game, to, commanderAI)
		{
			this.m_ClearTargetsUpdate = 0;
			this.m_UpdateTargetSelection = 0;
			this.m_CanMerge = false;
		}

		public override void Update(int framesElapsed)
		{
			this.m_UpdateTargetSelection -= framesElapsed;
			if (this.m_UpdateTargetSelection <= 0)
			{
				this.UpdateTargetSelection();
				this.m_UpdateTargetSelection = BaseAttackShipControl.kUpdateTargetSelection;
			}
			bool flag = true;
			if (this.m_TaskGroupObjective is AttackGroupObjective && this.m_TaskGroupObjective.m_TargetEnemyGroup != null)
			{
				float num1 = TaskGroup.ATTACK_GROUP_RANGE + this.m_TaskGroupObjective.m_TargetEnemyGroup.GetGroupRadius();
				Vector3 vector3_1 = this.GetCurrentPosition() - this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition;
				float length = vector3_1.Length;
				if ((double)length > 0.0 && this.m_Ships.Count > 0)
				{
					Vector3 v0 = vector3_1 / length;
					Vector3 vector3_2 = this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownDestination - this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition;
					if ((double)length > (double)num1 && (double)vector3_2.LengthSquared > 25000000.0 && (double)Vector3.Dot(v0, Vector3.Normalize(vector3_2)) < 0.899999976158142)
					{
						float num2 = 0.0f;
						foreach (Ship ship in this.m_Ships)
							num2 += ship.Maneuvering.MaxShipSpeed;
						float num3 = num2 / (float)this.m_Ships.Count * 0.8f;
						Vector3 dest = (this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition + this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownDestination) * ((double)this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownHeading.LengthSquared > (double)num3 * (double)num3 ? 0.75f : 0.5f);
						dest.Y = 0.0f;
						Vector3 destFacing = dest - this.GetCurrentPosition();
						destFacing.Y = 0.0f;
						double num4 = (double)destFacing.Normalize();
						foreach (Ship ship in this.m_Ships)
						{
							float lengthSquared = (ship.Position - this.m_TaskGroupObjective.m_TargetEnemyGroup.m_LastKnownPosition).LengthSquared;
							float num5 = 16000f;
							if (ship.Maneuvering.SpeedState == ShipSpeedState.Overthrust)
							{
								float num6 = num5 - 14000f;
								if ((double)lengthSquared < (double)num6 * (double)num6)
									this.SetShipSpeed(ship, ShipSpeedState.Normal);
							}
							else if ((double)lengthSquared > (double)num5 * (double)num5)
								this.SetShipSpeed(ship, ShipSpeedState.Overthrust);
						}
						this.SetFUP(dest, destFacing);
						flag = false;
					}
					else
					{
						foreach (Ship ship in this.m_Ships)
							this.SetShipSpeed(ship, ShipSpeedState.Normal);
					}
				}
			}
			foreach (Ship ship in this.m_Ships)
			{
				if (ship.WeaponControlsIsInitilized && ship.WeaponControls.Any<SpecWeaponControl>((Func<SpecWeaponControl, bool>)(x => x.RequestHoldShip())))
				{
					flag = false;
					break;
				}
			}
			if (!flag)
				return;
			this.OnAttackUpdate(framesElapsed);
		}

		private void UpdateTargetSelection()
		{
			List<Ship> source = new List<Ship>();
			if (this.m_TaskGroupObjective.m_TargetEnemyGroup != null)
			{
				foreach (Ship ship in this.m_TaskGroupObjective.m_TargetEnemyGroup.m_Ships)
				{
					if (Ship.IsActiveShip(ship) && ship.IsDetected(this.m_CommanderAI.m_Player))
						source.Add(ship);
				}
			}
			bool flag = false;
			if (source.Count > 0)
			{
				--this.m_ClearTargetsUpdate;
				if (this.m_ClearTargetsUpdate <= 0)
				{
					flag = true;
					this.m_ClearTargetsUpdate = BaseAttackShipControl.kUpdateClearTarget;
				}
			}
			else
				this.m_ClearTargetsUpdate = BaseAttackShipControl.kUpdateClearTarget;
			List<Ship> shipList = new List<Ship>();
			foreach (Ship ship in this.m_Ships)
			{
				shipList.Add(ship);
				if (ship.Target != null && ship.Target is Ship)
				{
					Ship s = ship.Target as Ship;
					if (!Ship.IsActiveShip(s) || !source.Any<Ship>((Func<Ship, bool>)(x => x == s)) || flag)
						this.SetNewTarget(ship, (IGameObject)null);
				}
			}
			if (shipList.Count == 0 || source.Count == 0)
				return;
			ShipTargetComparision targetComparision = new ShipTargetComparision(this.m_CommanderAI, this.GetCurrentPosition());
			source.Sort((IComparer<Ship>)targetComparision);
			this.m_GroupPriorityTarget = (Ship)null;
			foreach (Ship ship in source)
			{
				Ship e = ship;
				if (!this.m_CommanderAI.GetTaskGroups().Any<TaskGroup>((Func<TaskGroup, bool>)(x => x.IsDesiredGroupTargetTaken((TaskGroupShipControl)this, e))))
				{
					this.m_GroupPriorityTarget = e;
					break;
				}
			}
			if (this.m_GroupPriorityTarget == null)
				this.m_GroupPriorityTarget = source.First<Ship>();
			int targetShipScore = this.m_CommanderAI.GetTargetShipScore(this.m_GroupPriorityTarget);
			while (targetShipScore > 0 && shipList.Count > 0)
			{
				Ship ship1 = (Ship)null;
				int num1 = 0;
				float num2 = float.MaxValue;
				foreach (Ship ship2 in shipList)
				{
					float lengthSquared = (ship2.Maneuvering.Position - this.m_GroupPriorityTarget.Maneuvering.Position).LengthSquared;
					if ((double)lengthSquared < (double)num2)
					{
						num1 = CombatAI.GetShipStrength(this.m_GroupPriorityTarget);
						num2 = lengthSquared;
						ship1 = ship2;
					}
				}
				if (ship1 != null)
				{
					this.SetNewTarget(ship1, (IGameObject)this.m_GroupPriorityTarget);
					targetShipScore -= num1;
					shipList.Remove(ship1);
				}
				else
					break;
			}
			if (shipList.Count <= 0)
				return;
			foreach (Ship ship1 in shipList)
			{
				Ship ship2 = (Ship)null;
				float num = float.MaxValue;
				foreach (Ship ship3 in source)
				{
					if (ship3 != this.m_GroupPriorityTarget)
					{
						float lengthSquared = (ship1.Maneuvering.Position - ship3.Maneuvering.Position).LengthSquared;
						if ((double)lengthSquared < (double)num)
						{
							num = lengthSquared;
							ship2 = ship3;
						}
					}
				}
				if (ship2 != null)
					this.SetNewTarget(ship1, (IGameObject)ship2);
				else
					this.SetNewTarget(ship1, (IGameObject)this.m_GroupPriorityTarget);
			}
		}

		protected virtual void OnAttackUpdate(int framesElapsed)
		{
		}

		protected Vector3 GetFlyByPosition(
		  Vector3 currentPos,
		  Vector3 flybyCenter,
		  float offsetDist,
		  float deviationAngle = 0.0f)
		{
			Vector3 forward = flybyCenter - currentPos;
			if ((double)forward.LengthSquared <= 9.99999997475243E-07)
				return currentPos;
			forward.Y = 0.0f;
			double num1 = (double)forward.Normalize();
			Vector3 position = flybyCenter + forward * offsetDist;
			StellarBody containingPosition = this.m_CommanderAI.GetPlanetContainingPosition(position);
			if (containingPosition == null)
				return position;
			Matrix matrix = Matrix.CreateRotationYPR(MathHelper.DegreesToRadians(deviationAngle), 0.0f, 0.0f) * Matrix.CreateWorld(flybyCenter, forward, Vector3.UnitZ);
			Matrix mat = Matrix.Inverse(matrix);
			Vector3 vec = Vector3.Transform(containingPosition.Parameters.Position, mat);
			vec.X += (float)(((double)vec.X < 0.0 ? 1.0 : -1.0) * ((double)containingPosition.Parameters.Radius + 750.0 + 500.0));
			vec = Vector3.Transform(vec, matrix);
			forward = vec - flybyCenter;
			forward.Y = 0.0f;
			double num2 = (double)forward.Normalize();
			return flybyCenter + forward * offsetDist;
		}

		public float GetAddedFlyByAngle(
		  Matrix currentAttackMatrix,
		  Vector3 attackCenter,
		  float offsetDist)
		{
			float num1 = 0.0f;
			StellarBody containingPosition = this.m_CommanderAI.GetPlanetContainingPosition(attackCenter + currentAttackMatrix.Forward * offsetDist);
			if (containingPosition == null)
				return num1;
			Matrix mat = Matrix.Inverse(currentAttackMatrix);
			float num2 = (double)Vector3.Transform(containingPosition.Parameters.Position, mat).X < 0.0 ? -1f : 1f;
			float radians = MathHelper.DegreesToRadians(30f);
			float num3 = (float)((double)containingPosition.Parameters.Radius + 750.0 + 500.0);
			for (bool flag = false; !flag; flag = false)
			{
				num1 += num2 * radians;
				Matrix rotationYpr = Matrix.CreateRotationYPR(currentAttackMatrix.EulerAngles.X + num1, 0.0f, 0.0f);
				if ((double)(attackCenter + rotationYpr.Forward * offsetDist - containingPosition.Parameters.Position).LengthSquared > (double)num3 * (double)num3)
					break;
			}
			return num1;
		}
	}
}
