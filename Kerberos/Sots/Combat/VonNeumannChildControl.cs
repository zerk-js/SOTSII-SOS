// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.VonNeumannChildControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.WeaponFramework;
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
	internal class VonNeumannChildControl : CombatAIController
	{
		private App m_Game;
		private Ship m_VonNeumannChild;
		private VonNeumannMomControl m_VonNeumannMom;
		private IGameObject m_Target;
		private VonNeumannChildStates m_State;
		private VonNeumannDisintegrationBeam m_Beam;
		private LogicalWeapon m_BeamWeapon;
		private float m_ApproachRange;
		private int m_RUStore;
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
			return this.m_VonNeumannChild;
		}

		public override void SetTarget(IGameObject target)
		{
			if (target == this.m_Target)
				return;
			this.m_Target = target;
			int num = target != null ? target.ObjectID : 0;
			CombatStance stance = num != 0 ? CombatStance.PURSUE : CombatStance.NO_STANCE;
			if (this.m_VonNeumannChild.CombatStance != stance)
				this.m_VonNeumannChild.SetCombatStance(stance);
			this.m_VonNeumannChild.Maneuvering.PostSetProp("SetStanceTarget", (object)num, (object)this.m_ApproachRange);
		}

		public override IGameObject GetTarget()
		{
			return this.m_Target;
		}

		public VonNeumannMomControl VonNeumanMom
		{
			get
			{
				return this.m_VonNeumannMom;
			}
			set
			{
				this.m_VonNeumannMom = value;
			}
		}

		public VonNeumannChildStates State
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

		public VonNeumannChildControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_VonNeumannChild = ship;
		}

		public override void Initialize()
		{
			this.m_Target = (IGameObject)null;
			this.m_VonNeumannMom = (VonNeumannMomControl)null;
			this.m_State = VonNeumannChildStates.SEEK;
			this.m_Vanished = false;
			this.m_RUStore = 0;
			this.m_Beam = (VonNeumannDisintegrationBeam)null;
			this.m_ApproachRange = 1000f;
			this.m_BeamWeapon = this.m_Game.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => ((IEnumerable<WeaponEnums.WeaponTraits>)x.Traits).Any<WeaponEnums.WeaponTraits>((Func<WeaponEnums.WeaponTraits, bool>)(k => k == WeaponEnums.WeaponTraits.Disintegrating))));
			if (this.m_BeamWeapon == null)
				return;
			this.m_BeamWeapon.AddGameObjectReference();
		}

		public override void Terminate()
		{
			this.ClearBeam();
			if (this.m_BeamWeapon == null)
				return;
			this.m_BeamWeapon.ReleaseGameObjectReference();
		}

		public override void ObjectRemoved(IGameObject obj)
		{
			if (this.m_VonNeumannMom != null && this.m_VonNeumannMom.GetShip() == obj)
			{
				if (this.m_VonNeumannMom.GetShip().IsDestroyed && this.m_VonNeumannChild != null)
					this.m_VonNeumannChild.KillShip(false);
				this.m_VonNeumannMom = (VonNeumannMomControl)null;
			}
			if (this.m_Target != obj)
				return;
			this.m_Target = (IGameObject)null;
		}

		public override void OnThink()
		{
			if (this.m_VonNeumannChild == null)
				this.ClearBeam();
			else if (this.m_VonNeumannMom != null && this.m_VonNeumannMom.GetShip().IsDestroyed)
			{
				this.m_VonNeumannChild.KillShip(false);
			}
			else
			{
				switch (this.m_State)
				{
					case VonNeumannChildStates.SEEK:
						this.ThinkSeek();
						break;
					case VonNeumannChildStates.TRACK:
						this.ThinkTrack();
						break;
					case VonNeumannChildStates.INITCOLLECT:
						this.ThinkInitCollect();
						break;
					case VonNeumannChildStates.COLLECT:
						this.ThinkCollect();
						break;
					case VonNeumannChildStates.RETURN:
						this.ThinkReturn();
						break;
					case VonNeumannChildStates.EMIT:
						this.ThinkEmit();
						break;
					case VonNeumannChildStates.EMITTING:
						this.ThinkEmiting();
						break;
					case VonNeumannChildStates.INITFLEE:
						this.ThinkInitFlee();
						break;
					case VonNeumannChildStates.FLEE:
						this.ThinkFlee();
						break;
					case VonNeumannChildStates.VANISH:
						this.ThinkVanish();
						break;
				}
			}
		}

		public override void ForceFlee()
		{
			if (this.m_State != VonNeumannChildStates.INITFLEE && this.m_State != VonNeumannChildStates.FLEE)
				this.m_State = VonNeumannChildStates.INITFLEE;
			this.ClearBeam();
		}

		public override bool VictoryConditionIsMet()
		{
			return this.m_Vanished;
		}

		public override bool RequestingNewTarget()
		{
			if (this.m_Target != null)
				return false;
			if (this.m_State != VonNeumannChildStates.SEEK)
				return this.m_State == VonNeumannChildStates.TRACK;
			return true;
		}

		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			if (this.m_VonNeumannMom != null)
				return;
			float num = float.MaxValue;
			this.m_Target = (IGameObject)null;
			foreach (IGameObject gameObject in objs)
			{
				if (gameObject is Ship)
				{
					Ship ship = gameObject as Ship;
					if (ship.Player != this.m_VonNeumannChild.Player && ship.Active && (Ship.IsActiveShip(ship) && ship.IsDetected(this.m_VonNeumannChild.Player)))
					{
						float lengthSquared = (ship.Position - this.m_VonNeumannChild.Position).LengthSquared;
						if ((double)lengthSquared < (double)num)
						{
							this.m_Target = (IGameObject)ship;
							num = lengthSquared;
						}
					}
				}
			}
		}

		public override bool NeedsAParent()
		{
			return this.m_VonNeumannMom == null;
		}

		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
			foreach (CombatAIController controller in controllers)
			{
				if (controller is VonNeumannMomControl)
				{
					VonNeumannMomControl neumannMomControl = controller as VonNeumannMomControl;
					if (neumannMomControl.IsThisMyMom(this.m_VonNeumannChild))
					{
						neumannMomControl.AddChild((CombatAIController)this);
						this.m_VonNeumannMom = neumannMomControl;
						break;
					}
				}
			}
		}

		public int GetStoredResources()
		{
			return this.m_RUStore;
		}

		private void ThinkBirth()
		{
		}

		private void ThinkSeek()
		{
			if (this.m_Target == null)
			{
				if (this.m_VonNeumannMom != null)
				{
					if (this.m_RUStore <= 0)
						return;
					this.m_State = VonNeumannChildStates.RETURN;
				}
				else
					this.m_State = VonNeumannChildStates.INITFLEE;
			}
			else
				this.m_State = VonNeumannChildStates.TRACK;
		}

		private void ThinkTrack()
		{
			if (this.m_Target == null)
			{
				this.m_State = VonNeumannChildStates.INITFLEE;
			}
			else
			{
				float num1 = 0.0f;
				float val2 = 0.0f;
				Vector3 vector3_1 = Vector3.Zero;
				if (this.m_Target is Ship)
				{
					Ship target = this.m_Target as Ship;
					vector3_1 = target.Position;
					num1 = target.ShipSphere.radius;
				}
				else if (this.m_Target is StellarBody)
				{
					StellarBody target = this.m_Target as StellarBody;
					vector3_1 = target.Parameters.Position;
					num1 = target.Parameters.Radius;
					val2 = 750f;
				}
				WeaponBank bankWithWeaponTrait = this.m_VonNeumannChild.GetWeaponBankWithWeaponTrait(WeaponEnums.WeaponTraits.Disintegrating);
				if (bankWithWeaponTrait != null)
				{
					this.m_ApproachRange = Math.Max(bankWithWeaponTrait.Weapon.RangeTable.Effective.Range, 100f);
					bankWithWeaponTrait.PostSetProp("DisableAllTurrets", true);
				}
				else
					this.m_ApproachRange = Math.Max(100f, CombatAI.GetMinEffectiveWeaponRange(this.m_VonNeumannChild, false));
				this.m_ApproachRange = Math.Max(this.m_ApproachRange, val2);
				this.m_ApproachRange += num1;
				Vector3 vector3_2 = vector3_1;
				Vector3 vector3_3 = this.m_VonNeumannChild.Position - vector3_2;
				double num2 = (double)vector3_3.Normalize();
				Vector3 targetPos = vector3_2 + vector3_3 * this.m_ApproachRange;
				Vector3 look = vector3_3 * -1f;
				this.m_VonNeumannChild.Maneuvering.PostAddGoal(targetPos, look);
				float lengthSquared = (this.m_VonNeumannChild.Position - targetPos).LengthSquared;
				float num3 = this.m_ApproachRange + 200f;
				if ((double)lengthSquared >= (double)num3 * (double)num3)
					return;
				this.m_State = VonNeumannChildStates.INITCOLLECT;
			}
		}

		private void ThinkInitCollect()
		{
			this.SpawnBeam(this.m_Target, true);
			this.m_State = VonNeumannChildStates.COLLECT;
		}

		private void ThinkCollect()
		{
			if (this.m_Beam != null && !this.m_Beam.Active && this.m_Beam.ObjectStatus == GameObjectStatus.Ready)
				this.m_Beam.Active = true;
			if (this.m_Beam == null)
			{
				this.m_State = VonNeumannChildStates.SEEK;
			}
			else
			{
				if (!this.m_Beam.Finished)
					return;
				if (this.m_Beam.Succeeded)
					this.m_RUStore += this.m_Beam.Resources;
				if (this.m_Target is StellarBody)
				{
					PlanetInfo planetInfo = this.m_Game.GameDatabase.GetPlanetInfo((this.m_Target as StellarBody).Parameters.OrbitalID);
					if (planetInfo != null)
					{
						planetInfo.Resources = Math.Max(planetInfo.Resources - this.m_Beam.Resources, 0);
						this.m_Game.GameDatabase.UpdatePlanet(planetInfo);
					}
				}
				this.m_Target = (IGameObject)null;
				if (this.m_VonNeumannChild.CombatStance != CombatStance.NO_STANCE)
					this.m_VonNeumannChild.SetCombatStance(CombatStance.NO_STANCE);
				this.m_VonNeumannChild.Maneuvering.PostSetProp("SetStanceTarget", (object)0, (object)0.0f);
				this.m_State = this.m_RUStore >= this.m_Game.AssetDatabase.GlobalVonNeumannData.ChildRUCarryCap ? (this.m_VonNeumannMom == null ? VonNeumannChildStates.INITFLEE : VonNeumannChildStates.RETURN) : VonNeumannChildStates.SEEK;
				this.ClearBeam();
			}
		}

		private void ThinkReturn()
		{
			if (this.m_VonNeumannMom != null)
			{
				Vector3 vector3 = this.m_VonNeumannChild.Position - this.m_VonNeumannMom.GetShip().Position;
				double num = (double)vector3.Normalize();
				Vector3 targetPos = this.m_VonNeumannMom.GetShip().Position + vector3 * this.m_ApproachRange;
				this.m_VonNeumannChild.Maneuvering.PostAddGoal(targetPos, -vector3);
				if ((double)(targetPos - this.m_VonNeumannChild.Position).LengthSquared >= 40000.0)
					return;
				this.m_State = VonNeumannChildStates.EMIT;
			}
			else
				this.m_State = VonNeumannChildStates.INITFLEE;
		}

		private void ThinkEmit()
		{
			this.SpawnBeam((IGameObject)this.m_VonNeumannMom.GetShip(), false);
			this.m_State = VonNeumannChildStates.EMITTING;
		}

		private void ThinkEmiting()
		{
			if (this.m_Beam != null && !this.m_Beam.Active && this.m_Beam.ObjectStatus == GameObjectStatus.Ready)
				this.m_Beam.Active = true;
			if (this.m_VonNeumannMom == null)
			{
				this.m_State = VonNeumannChildStates.INITFLEE;
				this.ClearBeam();
			}
			else
			{
				if (!this.m_Beam.Finished)
					return;
				this.ClearBeam();
				this.m_VonNeumannMom.SubmitResources(this.m_RUStore);
				this.m_RUStore = 0;
				this.m_State = VonNeumannChildStates.SEEK;
			}
		}

		private void ThinkInitFlee()
		{
			if (this.m_VonNeumannChild.CombatStance != CombatStance.NO_STANCE)
				this.m_VonNeumannChild.SetCombatStance(CombatStance.NO_STANCE);
			this.m_VonNeumannChild.Maneuvering.PostSetProp("SetStanceTarget", (object)0, (object)0.0f);
			this.m_State = VonNeumannChildStates.FLEE;
		}

		private void ThinkFlee()
		{
			if (this.m_VonNeumannMom != null && !this.m_VonNeumannMom.Vanished)
			{
				if (this.m_VonNeumannChild.CombatStance != CombatStance.PURSUE)
					this.m_VonNeumannChild.SetCombatStance(CombatStance.PURSUE);
				this.m_VonNeumannChild.Maneuvering.PostSetProp("SetStanceTarget", (object)this.m_VonNeumannMom.GetShip().ObjectID, (object)this.m_ApproachRange);
			}
			else
			{
				if (this.m_VonNeumannChild.CombatStance != CombatStance.RETREAT)
					this.m_VonNeumannChild.SetCombatStance(CombatStance.RETREAT);
				if (!this.m_VonNeumannChild.HasRetreated)
					return;
				this.m_State = VonNeumannChildStates.VANISH;
			}
		}

		private void ThinkVanish()
		{
			if (this.m_Vanished)
				return;
			this.m_VonNeumannChild.Active = false;
			this.m_Vanished = true;
		}

		private void SpawnBeam(IGameObject target, bool absorbTarget)
		{
			this.ClearBeam();
			if (this.m_BeamWeapon == null || target == null)
				return;
			Turret turretWithWeaponTrait = this.m_VonNeumannChild.GetTurretWithWeaponTrait(WeaponEnums.WeaponTraits.Disintegrating);
			int num = turretWithWeaponTrait == null ? this.m_VonNeumannChild.ObjectID : turretWithWeaponTrait.ObjectID;
			int val1 = this.m_Game.AssetDatabase.GlobalVonNeumannData.ChildRUCarryCap;
			if (target is StellarBody)
			{
				PlanetInfo planetInfo = this.m_Game.GameDatabase.GetPlanetInfo((target as StellarBody).Parameters.OrbitalID);
				if (planetInfo != null)
					val1 = Math.Min(val1, planetInfo.Resources);
			}
			this.m_Beam = this.m_Game.AddObject<VonNeumannDisintegrationBeam>(new List<object>()
	  {
		(object) target.ObjectID,
		(object) this.m_BeamWeapon.GameObject.ObjectID,
		(object) this.m_VonNeumannChild.ObjectID,
		(object) this.m_VonNeumannChild.Player.ObjectID,
		(object) num,
		(object) absorbTarget,
		(object) val1,
		(object) this.m_Game.AssetDatabase.GlobalVonNeumannData.RUTransferRateShip,
		(object) this.m_Game.AssetDatabase.GlobalVonNeumannData.RUTransferRatePlanet,
		(object) this.m_Game.AssetDatabase.GlobalVonNeumannData.ChildIntegrationTime
	  }.ToArray());
		}

		private void ClearBeam()
		{
			if (this.m_Beam == null)
				return;
			this.m_Game.ReleaseObject((IGameObject)this.m_Beam);
			this.m_Beam = (VonNeumannDisintegrationBeam)null;
		}
	}
}
