// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.TaskGroupShipControl
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
	internal class TaskGroupShipControl
	{
		protected App m_Game;
		protected CombatAI m_CommanderAI;
		private float m_SensorRange;
		protected bool m_CanMerge;
		protected Ship m_GroupPriorityTarget;
		protected List<Ship> m_Ships;
		public ShipFormation m_Formation;
		protected ShipControlType m_Type;
		public TacticalObjective m_TaskGroupObjective;

		public float SensorRange
		{
			get
			{
				return this.m_SensorRange;
			}
		}

		public bool CanMerge
		{
			get
			{
				return this.m_CanMerge;
			}
		}

		public Ship GroupPriorityTarget
		{
			get
			{
				return this.m_GroupPriorityTarget;
			}
		}

		public ShipControlType Type
		{
			get
			{
				return this.m_Type;
			}
		}

		public TaskGroupShipControl(App game, TacticalObjective to, CombatAI commanderAI)
		{
			this.m_Game = game;
			this.m_CommanderAI = commanderAI;
			this.m_Type = ShipControlType.None;
			this.m_Ships = new List<Ship>();
			this.m_TaskGroupObjective = to;
			this.m_Formation = new ShipFormation(game);
			this.m_Formation.BackLineOffsetDist = 500f;
			this.m_SensorRange = game.AssetDatabase.DefaultTacSensorRange;
			this.m_CanMerge = true;
			this.m_GroupPriorityTarget = (Ship)null;
		}

		public void ShutDown()
		{
			if (this.m_Formation != null)
				this.m_Formation.Destroy(this.m_Game);
			foreach (Ship ship in this.m_Ships)
				this.SetNewTarget(ship, (IGameObject)null);
		}

		public void AddShip(Ship ship, bool backLine)
		{
			this.m_Ships.Add(ship);
			if (backLine)
				this.m_Formation.AddShipToBackLine(ship);
			else
				this.m_Formation.AddShip(ship);
			this.m_SensorRange = Math.Max(this.m_SensorRange, ship.SensorRange);
		}

		public void RemoveShip(Ship ship)
		{
			this.m_Ships.Remove(ship);
			if (this.m_Formation != null)
				this.m_Formation.RemoveShip(this.m_Game, ship);
			this.m_SensorRange = this.m_Game.AssetDatabase.DefaultTacSensorRange;
			foreach (Ship ship1 in this.m_Ships)
				this.m_SensorRange = Math.Max(this.m_SensorRange, ship1.SensorRange);
		}

		public void ClearPriorityTarget()
		{
			this.m_GroupPriorityTarget = (Ship)null;
		}

		public void ShipRemoved(Ship ship)
		{
			this.m_Ships.Remove(ship);
			if (this.m_Formation == null)
				return;
			this.m_Formation.RemoveShip(this.m_Game, ship);
		}

		public bool ContainsShip(Ship ship)
		{
			return this.m_Ships.Contains(ship);
		}

		public int GetShipCount()
		{
			return this.m_Ships.Count;
		}

		public List<Ship> GetShips()
		{
			return this.m_Ships;
		}

		public Vector3 GetCurrentPosition()
		{
			return this.m_Formation.GetCurrentPosition();
		}

		public virtual void Update(int framesElapsed)
		{
		}

		public void SetFUP(Vector3 dest, Vector3 destFacing)
		{
			if (this.m_Formation == null || (double)(this.m_Formation.Destination - dest).LengthSquared <= 10000.0 && (double)Vector3.Dot(this.m_Formation.Facing, destFacing) >= 0.75 && this.m_Formation.DestinationSet)
				return;
			Vector3 pos = dest;
			Vector3 facing = destFacing;
			if (!this.m_Formation.HasReceivedAnUpdate)
			{
				if (this.m_Ships.Count > 0)
				{
					Vector3 zero = Vector3.Zero;
					foreach (Ship ship in this.m_Ships)
						zero += ship.Position;
					pos = zero / (float)this.m_Ships.Count;
					pos.Y = 0.0f;
					facing = -Vector3.Normalize(pos);
				}
			}
			else
				pos = this.m_CommanderAI.GetSafeDestination(this.GetCurrentPosition(), dest);
			this.m_Formation.SetDestination(this.m_Game, pos, facing, false);
		}

		protected void FaceShipsToTarget()
		{
			foreach (Ship ship in this.m_Formation.Ships)
			{
				if (ship.Target != null)
					this.FaceShipToTarget(ship, ship.Target);
			}
		}

		public void SetNewTarget(Ship ship, IGameObject target)
		{
			if (ship.Target == target || ship.BlindFireActive || !this.m_CommanderAI.ShipCanChangeTarget(ship))
				return;
			bool flag = false;
			if (target is Ship)
			{
				Ship ship1 = target as Ship;
				if (ship1.CloakedState == CloakedState.Cloaking)
				{
					float detectionPercent = this.m_CommanderAI.GetCloakedDetectionPercent(ship1);
					Vector3 vector3 = new Vector3()
					{
						X = (this.m_CommanderAI.AIRandom.CoinToss(0.5) ? -1f : 1f) * this.m_CommanderAI.AIRandom.NextInclusive(0.0001f, 1f),
						Y = 0.0f
					};
					vector3.X = (this.m_CommanderAI.AIRandom.CoinToss(0.5) ? -1f : 1f) * this.m_CommanderAI.AIRandom.NextInclusive(0.0001f, 1f);
					double num1 = (double)vector3.Normalize();
					float num2 = this.m_CommanderAI.AIRandom.NextInclusive(0.0f, (float)(200.0 * (1.0 - (double)detectionPercent)));
					ship.SetBlindFireTarget(ship1.ShipSphere.center + ship1.Position + vector3 * num2, Vector3.Zero, ship1.ShipSphere.radius * 1.25f, 5f);
					flag = true;
				}
			}
			if (flag)
				return;
			int subTargetId = 0;
			if (target is Ship)
			{
				Ship ship1 = target as Ship;
				if (!ship1.MissionSection.IsAlive)
				{
					Section section = ship1.Sections.FirstOrDefault<Section>((Func<Section, bool>)(x => x.ShipSectionAsset.Type == ShipSectionType.Command));
					if (section != null)
						subTargetId = section.ObjectID;
				}
			}
			ship.SetShipTarget(target != null ? target.ObjectID : 0, Vector3.Zero, false, subTargetId);
		}

		public bool IsPlanetSeparatingTarget(Vector3 desiredPos, Vector3 targetsPos, float width)
		{
			if (this.m_CommanderAI == null)
				return false;
			Vector3 forward = targetsPos - desiredPos;
			forward.Y = 0.0f;
			float num = forward.Normalize();
			Matrix mat = Matrix.Inverse(Matrix.CreateWorld(desiredPos, forward, Vector3.UnitY));
			bool flag = false;
			foreach (StellarBody stellarBody in this.m_CommanderAI.PlanetsInSystem)
			{
				Vector3 vector3 = Vector3.Transform(stellarBody.Parameters.Position, mat);
				if ((double)vector3.Z + (double)stellarBody.Parameters.Radius * 0.5 >= -(double)num && (double)vector3.Z - (double)stellarBody.Parameters.Radius * 0.5 <= 0.0 && (double)Math.Abs(vector3.X) < (double)width + (double)stellarBody.Parameters.Radius + 750.0)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				foreach (StellarBody stellarBody in this.m_CommanderAI.PlanetsInSystem)
				{
					Vector3 vector3 = Vector3.Transform(stellarBody.Parameters.Position, mat);
					if ((double)vector3.Z + (double)stellarBody.Parameters.Radius * 0.5 >= -(double)num && (double)vector3.Z - (double)stellarBody.Parameters.Radius * 0.5 <= 0.0 && (double)Math.Abs(vector3.X) < (double)width + (double)stellarBody.Parameters.Radius + 7500.0)
					{
						flag = true;
						break;
					}
				}
			}
			return flag;
		}

		protected void FaceShipToTarget(Ship ship, IGameObject target)
		{
			Vector3 vector3 = Vector3.Zero;
			if (target is Ship)
				vector3 = (target as Ship).Maneuvering.Position;
			else if (target is StellarBody)
				vector3 = (target as StellarBody).Parameters.Position;
			Vector3 look = vector3 - ship.Maneuvering.Position;
			look.Y = 0.0f;
			double num = (double)look.Normalize();
			ship.Maneuvering.PostSetLook(look);
		}

		protected void FaceBroadsideRight(Ship ship, Ship target)
		{
			Vector3 look = target.Maneuvering.Position - ship.Maneuvering.Position;
			double num = (double)new Vector3(-look.Z, 0.0f, look.X).Normalize();
			ship.Maneuvering.PostSetLook(look);
		}

		protected void FaceBroadSideLeft(Ship ship, Ship target)
		{
			Vector3 look = target.Maneuvering.Position - ship.Maneuvering.Position;
			double num = (double)new Vector3(look.Z, 0.0f, -look.X).Normalize();
			ship.Maneuvering.PostSetLook(look);
		}

		protected void SetShipSpeed(Ship ship, ShipSpeedState sss)
		{
			if (ship == null)
				return;
			ship.Maneuvering.SpeedState = sss;
		}
	}
}
