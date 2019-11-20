// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.VonNeumannSeekerControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Encounters;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using System.Collections.Generic;

namespace Kerberos.Sots.Combat
{
	internal class VonNeumannSeekerControl : CombatAIController
	{
		private const int MAX_SEEKER_PODS = 12;
		private App m_Game;
		private Ship m_VonNeumannSeeker;
		private List<Ship> m_LoadingPods;
		private List<Ship> m_VonNeumannSeekerPods;
		private List<PlanetTarget> m_AvailableTargets;
		private StellarBody m_CurrentTarget;
		private VonNeumannSeekerStates m_State;
		private int m_NumPodsRemain;
		private bool m_Vanished;

		public bool Vanished
		{
			get
			{
				return this.m_Vanished;
			}
		}

		public override Ship GetShip()
		{
			return this.m_VonNeumannSeeker;
		}

		public override void SetTarget(IGameObject target)
		{
			if (target == null)
			{
				this.m_CurrentTarget = (StellarBody)null;
			}
			else
			{
				if (!(target is StellarBody))
					return;
				this.m_CurrentTarget = target as StellarBody;
			}
		}

		public override IGameObject GetTarget()
		{
			return (IGameObject)this.m_CurrentTarget;
		}

		public VonNeumannSeekerControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_VonNeumannSeeker = ship;
			this.m_NumPodsRemain = 0;
		}

		public override void Initialize()
		{
			this.m_VonNeumannSeekerPods = new List<Ship>();
			this.m_LoadingPods = new List<Ship>();
			this.m_AvailableTargets = new List<PlanetTarget>();
			this.m_CurrentTarget = (StellarBody)null;
			this.m_State = VonNeumannSeekerStates.SEEK;
			this.m_Vanished = false;
			Matrix rotationYpr = Matrix.CreateRotationYPR(this.m_VonNeumannSeeker.Maneuvering.Rotation);
			rotationYpr.Position = this.m_VonNeumannSeeker.Maneuvering.Position;
			int designId = VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.SeekerProbe].DesignId;
			for (int index = 0; index < 12; ++index)
			{
				Ship newShip = CombatAIController.CreateNewShip(this.m_Game.Game, rotationYpr, designId, this.m_VonNeumannSeeker.ObjectID, this.m_VonNeumannSeeker.InputID, this.m_VonNeumannSeeker.Player.ObjectID);
				if (newShip != null)
				{
					this.m_LoadingPods.Add(newShip);
					++this.m_NumPodsRemain;
				}
			}
			this.m_State = VonNeumannSeekerStates.INTEGRATECHILD;
		}

		public override void Terminate()
		{
		}

		public override void ObjectRemoved(IGameObject obj)
		{
			foreach (Ship neumannSeekerPod in this.m_VonNeumannSeekerPods)
			{
				if (neumannSeekerPod == obj)
				{
					--this.m_NumPodsRemain;
					this.m_VonNeumannSeekerPods.Remove(neumannSeekerPod);
					break;
				}
			}
			foreach (PlanetTarget availableTarget in this.m_AvailableTargets)
			{
				if (availableTarget.Planet == obj)
				{
					this.m_AvailableTargets.Remove(availableTarget);
					break;
				}
			}
			if (obj != this.m_CurrentTarget)
				return;
			this.m_CurrentTarget = (StellarBody)null;
		}

		public override void OnThink()
		{
			if (this.m_VonNeumannSeeker == null)
				return;
			switch (this.m_State)
			{
				case VonNeumannSeekerStates.INTEGRATECHILD:
					this.ThinkIntegrateChild();
					break;
				case VonNeumannSeekerStates.SEEK:
					this.ThinkSeek();
					break;
				case VonNeumannSeekerStates.TRACK:
					this.ThinkTrack();
					break;
				case VonNeumannSeekerStates.BOMBARD:
					this.ThinkBombard();
					break;
				case VonNeumannSeekerStates.LAUNCHING:
					this.ThinkLaunching();
					break;
				case VonNeumannSeekerStates.WAIT:
					this.ThinkWait();
					break;
				case VonNeumannSeekerStates.INITFLEE:
					this.ThinkInitFlee();
					break;
				case VonNeumannSeekerStates.FLEE:
					this.ThinkFlee();
					break;
				case VonNeumannSeekerStates.VANISH:
					this.ThinkVanish();
					break;
			}
		}

		public override void ForceFlee()
		{
			if (this.m_State == VonNeumannSeekerStates.INITFLEE || this.m_State == VonNeumannSeekerStates.FLEE)
				return;
			this.m_State = VonNeumannSeekerStates.INITFLEE;
		}

		public override bool VictoryConditionIsMet()
		{
			return this.m_Vanished;
		}

		public override bool RequestingNewTarget()
		{
			if (this.m_State == VonNeumannSeekerStates.SEEK)
				return this.m_AvailableTargets.Count == 0;
			return false;
		}

		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			this.m_AvailableTargets.Clear();
			foreach (IGameObject gameObject in objs)
			{
				if (gameObject is StellarBody)
				{
					StellarBody planet = gameObject as StellarBody;
					if (planet.Population > 0.0)
						this.m_AvailableTargets.Add(new PlanetTarget(planet));
				}
			}
		}

		public override bool NeedsAParent()
		{
			return false;
		}

		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
		}

		private void ThinkIntegrateChild()
		{
			if (this.m_LoadingPods.Count == 0)
			{
				this.m_State = VonNeumannSeekerStates.SEEK;
			}
			else
			{
				bool flag = true;
				foreach (GameObject loadingPod in this.m_LoadingPods)
				{
					if (loadingPod.ObjectStatus != GameObjectStatus.Ready)
						flag = false;
				}
				if (!flag)
					return;
				foreach (Ship loadingPod in this.m_LoadingPods)
				{
					loadingPod.Player = this.m_VonNeumannSeeker.Player;
					loadingPod.Visible = this.m_VonNeumannSeeker.Visible;
					loadingPod.Active = true;
					this.m_Game.CurrentState.AddGameObject((IGameObject)loadingPod, false);
					this.m_VonNeumannSeekerPods.Add(loadingPod);
				}
				this.m_LoadingPods.Clear();
				this.m_State = VonNeumannSeekerStates.SEEK;
			}
		}

		private void ThinkSeek()
		{
			if (this.m_NumPodsRemain == 0)
				this.m_State = VonNeumannSeekerStates.INITFLEE;
			else if (this.m_CurrentTarget == null)
			{
				float num = float.MaxValue;
				foreach (PlanetTarget availableTarget in this.m_AvailableTargets)
				{
					if (!availableTarget.HasBeenVisted)
					{
						float lengthSquared = (availableTarget.Planet.Parameters.Position - this.m_VonNeumannSeeker.Position).LengthSquared;
						if ((double)lengthSquared < (double)num)
						{
							num = lengthSquared;
							this.m_CurrentTarget = availableTarget.Planet;
						}
					}
				}
				this.m_VonNeumannSeeker.SetShipTarget(this.m_CurrentTarget != null ? this.m_CurrentTarget.ObjectID : 0, Vector3.Zero, true, 0);
				if (this.m_CurrentTarget != null)
					return;
				this.m_State = VonNeumannSeekerStates.INITFLEE;
			}
			else
				this.m_State = VonNeumannSeekerStates.TRACK;
		}

		private void ThinkTrack()
		{
			if (this.m_CurrentTarget == null)
			{
				this.m_State = VonNeumannSeekerStates.SEEK;
			}
			else
			{
				Vector3 position = this.m_CurrentTarget.Parameters.Position;
				float num1 = (float)((double)this.m_CurrentTarget.Parameters.Radius + 750.0 + 500.0);
				Vector3 vector3_1 = position;
				Vector3 vector3_2 = this.m_VonNeumannSeeker.Position - vector3_1;
				vector3_2.Y = 0.0f;
				double num2 = (double)vector3_2.Normalize();
				Vector3 targetPos = vector3_1 + vector3_2 * (num1 - 100f);
				Vector3 look = vector3_2 * -1f;
				this.m_VonNeumannSeeker.Maneuvering.PostAddGoal(targetPos, look);
				if ((double)(this.m_VonNeumannSeeker.Position - targetPos).LengthSquared >= (double)num1 * (double)num1)
					return;
				this.m_State = VonNeumannSeekerStates.BOMBARD;
			}
		}

		private void ThinkBombard()
		{
			if (this.m_CurrentTarget == null)
			{
				this.m_State = VonNeumannSeekerStates.SEEK;
			}
			else
			{
				this.m_VonNeumannSeeker.PostSetProp("LaunchBattleriders");
				this.m_State = VonNeumannSeekerStates.LAUNCHING;
			}
		}

		private void ThinkLaunching()
		{
			bool flag = true;
			foreach (Ship neumannSeekerPod in this.m_VonNeumannSeekerPods)
			{
				if (neumannSeekerPod.DockedWithParent)
				{
					flag = false;
					break;
				}
			}
			if (!flag)
				return;
			this.m_State = VonNeumannSeekerStates.WAIT;
		}

		private void ThinkWait()
		{
			bool flag = true;
			foreach (Ship neumannSeekerPod in this.m_VonNeumannSeekerPods)
			{
				if (!neumannSeekerPod.DockedWithParent)
				{
					flag = false;
					break;
				}
			}
			if (!flag)
				return;
			foreach (PlanetTarget availableTarget in this.m_AvailableTargets)
			{
				if (availableTarget.Planet == this.m_CurrentTarget)
				{
					availableTarget.HasBeenVisted = true;
					break;
				}
			}
			this.m_CurrentTarget = (StellarBody)null;
			this.m_State = VonNeumannSeekerStates.SEEK;
		}

		private void ThinkInitFlee()
		{
			this.m_State = VonNeumannSeekerStates.FLEE;
			this.m_VonNeumannSeeker.PostSetProp("RecoverBattleriders");
		}

		private void ThinkFlee()
		{
			if (this.m_VonNeumannSeeker.CombatStance != CombatStance.RETREAT)
				this.m_VonNeumannSeeker.SetCombatStance(CombatStance.RETREAT);
			if (!this.m_VonNeumannSeeker.HasRetreated)
				return;
			this.m_State = VonNeumannSeekerStates.VANISH;
		}

		private void ThinkVanish()
		{
			if (this.m_Vanished)
				return;
			this.m_VonNeumannSeeker.Active = false;
			this.m_Vanished = true;
		}
	}
}
