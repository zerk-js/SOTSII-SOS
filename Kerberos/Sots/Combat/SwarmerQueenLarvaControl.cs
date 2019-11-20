// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.SwarmerQueenLarvaControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using System;
using System.Collections.Generic;

namespace Kerberos.Sots.Combat
{
	internal class SwarmerQueenLarvaControl : CombatAIController
	{
		private App m_Game;
		private Ship m_SwarmerQueenLarva;
		private Ship m_SwarmerHive;
		private List<Ship> m_Enemies;
		private SwarmerQueenLarvaStates m_State;
		private IGameObject m_Target;
		private int m_EnemyUpdateRate;
		private int m_UpdateRate;
		private bool m_HasHadHive;
		private float m_BeltRadius;
		private float m_BeltThickness;
		private int m_CurrEvadeNodeIndex;
		private int m_NumEvadeNodes;
		private Vector3[] m_EvadeLocations;

		public override Ship GetShip()
		{
			return this.m_SwarmerQueenLarva;
		}

		public override void SetTarget(IGameObject target)
		{
			this.m_SwarmerQueenLarva.SetShipTarget(this.m_Target != null ? this.m_Target.ObjectID : 0, Vector3.Zero, true, 0);
			this.m_Target = target;
		}

		public override IGameObject GetTarget()
		{
			return this.m_Target;
		}

		public SwarmerQueenLarvaStates State
		{
			get
			{
				return this.m_State;
			}
			set
			{
				this.m_State = value;
			}
		}

		public SwarmerQueenLarvaControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_SwarmerQueenLarva = ship;
		}

		public override void Initialize()
		{
			this.m_Enemies = new List<Ship>();
			this.m_State = SwarmerQueenLarvaStates.SEEK;
			this.m_EnemyUpdateRate = 0;
			this.m_UpdateRate = 0;
			this.m_SwarmerHive = (Ship)null;
			this.m_Target = (IGameObject)null;
			this.m_HasHadHive = false;
			this.m_BeltRadius = this.m_SwarmerQueenLarva.Position.Length;
			this.m_BeltThickness = 1000f;
			this.m_NumEvadeNodes = 10;
			this.m_CurrEvadeNodeIndex = 0;
			float radians = MathHelper.DegreesToRadians(360f / (float)this.m_NumEvadeNodes);
			this.m_EvadeLocations = new Vector3[this.m_NumEvadeNodes];
			for (int index = 0; index < this.m_NumEvadeNodes; ++index)
				this.m_EvadeLocations[index] = new Vector3((float)Math.Sin((double)radians * (double)index), 0.0f, (float)Math.Cos((double)radians * (double)index)) * this.m_BeltRadius;
		}

		public override void Terminate()
		{
			this.m_SwarmerHive = (Ship)null;
			this.SetTarget((IGameObject)null);
		}

		public override void ObjectRemoved(IGameObject obj)
		{
			foreach (Ship enemy in this.m_Enemies)
			{
				if (enemy == obj)
				{
					this.m_Enemies.Remove(enemy);
					break;
				}
			}
		}

		public override void OnThink()
		{
			if (this.m_SwarmerQueenLarva == null)
				return;
			switch (this.m_State)
			{
				case SwarmerQueenLarvaStates.SEEK:
					this.ThinkSeek();
					break;
				case SwarmerQueenLarvaStates.TRACK:
					this.ThinkTrack();
					break;
				case SwarmerQueenLarvaStates.EVADE:
					this.ThinkEvade();
					break;
			}
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
			return this.m_State == SwarmerQueenLarvaStates.SEEK;
		}

		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			this.m_Enemies.Clear();
			foreach (IGameObject gameObject in objs)
			{
				if (gameObject is Ship)
				{
					Ship ship = gameObject as Ship;
					if (ship.Player != this.m_SwarmerQueenLarva.Player && ship.Active && (Ship.IsActiveShip(ship) && ship.IsDetected(this.m_SwarmerQueenLarva.Player)))
						this.m_Enemies.Add(ship);
				}
			}
			this.m_EnemyUpdateRate = 30;
		}

		private void FindTarget()
		{
			if (this.m_SwarmerHive == null)
				return;
			Ship ship = (Ship)null;
			float num = float.MaxValue;
			foreach (Ship enemy in this.m_Enemies)
			{
				float lengthSquared = (this.m_SwarmerHive.Position - enemy.Position).LengthSquared;
				if ((double)lengthSquared < (double)num)
				{
					ship = enemy;
					num = lengthSquared;
				}
			}
			this.SetTarget((IGameObject)ship);
		}

		public override bool NeedsAParent()
		{
			return !this.m_HasHadHive;
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
		}

		private bool HiveIsPresent()
		{
			return this.m_SwarmerHive != null && !this.m_SwarmerHive.IsDestroyed;
		}

		private void ThinkSeek()
		{
			if (this.m_Enemies.Count <= 0)
				return;
			if (this.HiveIsPresent())
			{
				this.FindTarget();
				this.m_State = SwarmerQueenLarvaStates.TRACK;
			}
			else
			{
				this.FindInitialNodeIndex();
				this.m_State = SwarmerQueenLarvaStates.EVADE;
			}
		}

		private void ThinkTrack()
		{
			--this.m_EnemyUpdateRate;
			if (this.m_Target == null || !this.HiveIsPresent() || this.m_EnemyUpdateRate <= 0)
			{
				this.m_State = SwarmerQueenLarvaStates.SEEK;
			}
			else
			{
				--this.m_UpdateRate;
				if (this.m_UpdateRate > 0)
					return;
				this.m_UpdateRate = 5;
				if (!(this.m_Target is Ship))
					return;
				Ship target = this.m_Target as Ship;
				Vector3 vector3_1 = target.Position - this.m_SwarmerHive.Position;
				Vector3 vector3_2 = target.Maneuvering.Destination - this.m_SwarmerHive.Position;
				Vector3 vector3_3 = (double)vector3_2.LengthSquared < (double)vector3_1.LengthSquared ? target.Maneuvering.Destination : target.Position;
				Vector3 vector3_4 = new Vector3();
				Vector3 vector3_5 = (double)vector3_2.LengthSquared >= (double)vector3_1.LengthSquared ? vector3_1 : vector3_2;
				vector3_5.Y = 0.0f;
				float length = vector3_5.Length;
				Vector3 look = vector3_5 / length;
				Vector3 position = this.m_SwarmerHive.Position;
				Vector3 targetPos;
				if ((double)length > 3000.0)
				{
					targetPos = position + look * Math.Min(length * 0.5f, 3000f);
				}
				else
				{
					float num = Math.Min((vector3_3 - this.m_SwarmerQueenLarva.Position).Length, 3000f);
					targetPos = (double)num <= 500.0 ? this.m_SwarmerQueenLarva.Position : vector3_3 - look * num;
				}
				this.m_SwarmerQueenLarva.Maneuvering.PostAddGoal(targetPos, look);
			}
		}

		private void ThinkEvade()
		{
			if (this.m_Enemies.Count == 0)
			{
				this.m_State = SwarmerQueenLarvaStates.SEEK;
			}
			else
			{
				--this.m_UpdateRate;
				if (this.m_UpdateRate > 0)
					return;
				this.m_UpdateRate = 30;
				int currEvadeNodeIndex = this.m_CurrEvadeNodeIndex;
				this.FindSafestNodeIndex();
				if (currEvadeNodeIndex == this.m_CurrEvadeNodeIndex)
					return;
				float num1 = new Random().NextInclusive(-this.m_BeltThickness, this.m_BeltThickness);
				Vector3 evadeLocation = this.m_EvadeLocations[this.m_CurrEvadeNodeIndex];
				Vector3 vector3_1 = evadeLocation - this.m_SwarmerQueenLarva.Position;
				vector3_1.Y = 0.0f;
				double num2 = (double)vector3_1.Normalize();
				Vector3 vector3_2 = Vector3.Cross(vector3_1, Vector3.UnitY);
				this.m_SwarmerQueenLarva.Maneuvering.PostAddGoal(evadeLocation + vector3_2 * num1, vector3_1);
			}
		}

		private void FindInitialNodeIndex()
		{
			this.m_CurrEvadeNodeIndex = 0;
			float num = float.MaxValue;
			for (int index = 0; index < this.m_NumEvadeNodes; ++index)
			{
				float lengthSquared = (this.m_EvadeLocations[index] - this.m_SwarmerQueenLarva.Position).LengthSquared;
				if ((double)lengthSquared < (double)num)
				{
					num = lengthSquared;
					this.m_CurrEvadeNodeIndex = index;
				}
			}
		}

		private void FindSafestNodeIndex()
		{
			int[] numArray = new int[this.m_NumEvadeNodes];
			for (int index = 0; index < this.m_NumEvadeNodes; ++index)
			{
				foreach (Ship enemy in this.m_Enemies)
				{
					if ((double)(enemy.Position - this.m_EvadeLocations[index]).LengthSquared < 25000000.0)
						++numArray[index];
				}
			}
			if (numArray[this.m_CurrEvadeNodeIndex] == 0)
				return;
			Random random = new Random();
			int index1 = this.m_CurrEvadeNodeIndex - 1;
			int index2 = this.m_CurrEvadeNodeIndex + 1;
			int num1 = this.m_NumEvadeNodes / 2;
			bool flag = false;
			for (int index3 = 0; index3 < num1; ++index3)
			{
				if (index1 < 0)
					index1 = this.m_NumEvadeNodes - 1;
				if (index2 >= this.m_NumEvadeNodes)
					index2 = 0;
				if (numArray[index1] == 0 && numArray[index2] == 0)
				{
					this.m_CurrEvadeNodeIndex = random.CoinToss(0.5) ? index1 : index2;
					flag = true;
					break;
				}
				if (numArray[index1] == 0)
				{
					this.m_CurrEvadeNodeIndex = index1;
					flag = true;
					break;
				}
				if (numArray[index2] == 0)
				{
					this.m_CurrEvadeNodeIndex = index2;
					flag = true;
					break;
				}
				++index2;
				--index1;
			}
			if (flag)
				return;
			int num2 = 500;
			float num3 = float.MaxValue;
			for (int index3 = 0; index3 < this.m_NumEvadeNodes; ++index3)
			{
				float lengthSquared = (this.m_EvadeLocations[index3] - this.m_SwarmerQueenLarva.Position).LengthSquared;
				if (num2 == numArray[index3] && (double)lengthSquared < (double)num3 || num2 < numArray[index3])
				{
					num3 = lengthSquared;
					num2 = numArray[index3];
					this.m_CurrEvadeNodeIndex = index3;
				}
			}
		}
	}
}
