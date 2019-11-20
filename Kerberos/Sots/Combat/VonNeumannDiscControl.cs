// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.VonNeumannDiscControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Encounters;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Ships;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal class VonNeumannDiscControl : CombatAIController
	{
		public static readonly string[] _vonneumannDiscTypes = Enum.GetNames(typeof(VonNeumannDiscTypes));
		private const int KUpdateRate = 60;
		private App m_Game;
		private Ship m_Disc;
		private IGameObject m_Target;
		private VonNeumannNeoBerserkerControl m_ParentBerserker;
		private VonNeumannDiscTypes m_DiscType;
		private VonNeumannDiscStates m_State;
		private List<VonNeumannDiscControl> m_Discs;
		private List<Ship> m_PossessorBoardingPods;
		private int m_UpdateDelay;
		private bool m_PodsLaunched;

		public static bool DiscTypeIsAttacker(VonNeumannDiscTypes type)
		{
			switch (type)
			{
				case VonNeumannDiscTypes.IMPACTOR:
				case VonNeumannDiscTypes.POSSESSOR:
				case VonNeumannDiscTypes.DISINTEGRATOR:
				case VonNeumannDiscTypes.EMPULSER:
				case VonNeumannDiscTypes.EMITTER:
				case VonNeumannDiscTypes.ABSORBER:
				case VonNeumannDiscTypes.OPRESSOR:
					return true;
				default:
					return false;
			}
		}

		public static VonNeumannDiscTypes DiscTypeFromMissionSection(Section section)
		{
			VonNeumannDiscTypes neumannDiscTypes = VonNeumannDiscTypes.IMPACTOR;
			string upper = section.ShipSectionAsset.SectionName.ToUpper();
			for (int index = 0; index < 10; ++index)
			{
				if (upper.Contains(VonNeumannDiscControl._vonneumannDiscTypes[index]))
				{
					neumannDiscTypes = (VonNeumannDiscTypes)index;
					break;
				}
			}
			return neumannDiscTypes;
		}

		public VonNeumannDiscTypes DiscType
		{
			get
			{
				return this.m_DiscType;
			}
		}

		public override Ship GetShip()
		{
			return this.m_Disc;
		}

		public override void SetTarget(IGameObject target)
		{
			this.m_Target = target;
			this.m_Disc.SetShipTarget(target != null ? target.ObjectID : 0, Vector3.Zero, true, 0);
		}

		public override IGameObject GetTarget()
		{
			return this.m_Target;
		}

		public VonNeumannDiscControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_Disc = ship;
			this.m_DiscType = VonNeumannDiscControl.DiscTypeFromMissionSection(ship.Sections.FirstOrDefault<Section>((Func<Section, bool>)(x => x.ShipSectionAsset.Type == ShipSectionType.Mission)));
		}

		public override void Initialize()
		{
			this.m_Target = (IGameObject)null;
			this.m_ParentBerserker = (VonNeumannNeoBerserkerControl)null;
			this.m_State = VonNeumannDiscStates.SEEK;
			this.m_PodsLaunched = false;
			this.m_Discs = new List<VonNeumannDiscControl>();
			this.m_PossessorBoardingPods = new List<Ship>();
			this.m_UpdateDelay = 0;
			if (this.m_DiscType != VonNeumannDiscTypes.POSSESSOR)
				return;
			Matrix rotationYpr = Matrix.CreateRotationYPR(this.m_Disc.Maneuvering.Rotation);
			rotationYpr.Position = this.m_Disc.Maneuvering.Position;
			int num = this.m_Disc.BattleRiderMounts.Count<BattleRiderMount>();
			for (int index = 0; index < num; ++index)
			{
				Ship newShip = CombatAIController.CreateNewShip(this.m_Game.Game, rotationYpr, VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.BoardingPod].DesignId, this.m_Disc.ObjectID, this.m_Disc.InputID, this.m_Disc.Player.ObjectID);
				if (newShip != null)
					this.m_PossessorBoardingPods.Add(newShip);
			}
			this.m_State = VonNeumannDiscStates.ACTIVATEBOARDINGPODS;
		}

		public override void Terminate()
		{
			if (this.m_DiscType == VonNeumannDiscTypes.SHIELDER)
				this.ShieldDiscDestroyed();
			if (this.m_DiscType != VonNeumannDiscTypes.CLOAKER)
				return;
			this.CloakDiscDestroyed();
		}

		public override void ObjectRemoved(IGameObject obj)
		{
			if (this.m_Target == obj)
			{
				this.SetTarget((IGameObject)null);
				if (this.m_Disc.CombatStance != CombatStance.NO_STANCE)
					this.m_Disc.SetCombatStance(CombatStance.NO_STANCE);
				this.m_Disc.Maneuvering.PostSetProp("SetStanceTarget", (object)0, (object)0.0f);
			}
			if (this.m_ParentBerserker != null && this.m_ParentBerserker.GetShip() == obj)
				this.m_ParentBerserker = (VonNeumannNeoBerserkerControl)null;
			foreach (VonNeumannDiscControl disc in this.m_Discs)
			{
				if (disc.GetShip() == obj)
				{
					this.m_Discs.Remove(disc);
					break;
				}
			}
			foreach (Ship possessorBoardingPod in this.m_PossessorBoardingPods)
			{
				if (possessorBoardingPod == obj)
				{
					this.m_PossessorBoardingPods.Remove(possessorBoardingPod);
					break;
				}
			}
		}

		public override void OnThink()
		{
			if (this.m_Disc == null)
				return;
			switch (this.m_DiscType)
			{
				case VonNeumannDiscTypes.POSSESSOR:
					this.UpdatePods();
					break;
				case VonNeumannDiscTypes.SHIELDER:
					this.UpdateShieldDisc();
					break;
				case VonNeumannDiscTypes.CLOAKER:
					this.UpdateCloakDisc();
					break;
			}
			switch (this.m_State)
			{
				case VonNeumannDiscStates.SEEK:
					this.ThinkSeek();
					break;
				case VonNeumannDiscStates.TRACK:
					this.ThinkTrack();
					break;
				case VonNeumannDiscStates.LAUNCHPODS:
					this.ThinkLaunchPods();
					break;
				case VonNeumannDiscStates.WAITFORLAUNCH:
					this.ThinkWaitForLaunch();
					break;
				case VonNeumannDiscStates.ACTIVATEBOARDINGPODS:
					this.ThinkActivateBoardingPods();
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
			if (this.m_Target == null)
				return this.m_State == VonNeumannDiscStates.SEEK;
			return false;
		}

		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			if (this.m_ParentBerserker == null)
				return;
			float num = float.MaxValue;
			IGameObject target = (IGameObject)null;
			foreach (IGameObject gameObject in objs)
			{
				if (gameObject is Ship)
				{
					Ship ship = gameObject as Ship;
					if (VonNeumannDiscControl.DiscTypeIsAttacker(this.m_DiscType))
					{
						if (ship.Player == this.m_Disc.Player)
							continue;
					}
					else if (ship.Player != this.m_Disc.Player)
						continue;
					if (ship.Active && Ship.IsActiveShip(ship) && ship.IsDetected(this.m_Disc.Player))
					{
						float lengthSquared = (ship.Position - this.m_Disc.Position).LengthSquared;
						if ((double)lengthSquared < (double)num)
						{
							target = (IGameObject)ship;
							num = lengthSquared;
						}
					}
				}
			}
			this.SetTarget(target);
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
					if (berserkerControl.IsThisMyMom(this.m_Disc))
					{
						berserkerControl.AddChild((CombatAIController)this);
						this.m_ParentBerserker = berserkerControl;
						break;
					}
				}
			}
		}

		private void ThinkSeek()
		{
			if (this.m_Target == null)
				return;
			float num = VonNeumannDiscControl.DiscTypeIsAttacker(this.m_DiscType) ? Math.Max(100f, CombatAI.GetMinPointBlankWeaponRange(this.m_Disc, false)) : 400f;
			if (this.m_Disc.CombatStance != CombatStance.PURSUE)
				this.m_Disc.SetCombatStance(CombatStance.PURSUE);
			this.m_Disc.Maneuvering.PostSetProp("SetStanceTarget", (object)this.m_Target.ObjectID, (object)num);
			this.m_State = VonNeumannDiscStates.TRACK;
		}

		private void ThinkTrack()
		{
			if (this.m_Target == null)
			{
				this.m_State = VonNeumannDiscStates.SEEK;
				if (this.m_Disc.CombatStance != CombatStance.NO_STANCE)
					this.m_Disc.SetCombatStance(CombatStance.NO_STANCE);
				this.m_Disc.Maneuvering.PostSetProp("SetStanceTarget", (object)0, (object)0.0f);
			}
			else
			{
				if (this.m_DiscType != VonNeumannDiscTypes.POSSESSOR || !(this.m_Target is Ship))
					return;
				Ship target = this.m_Target as Ship;
				if (this.m_PodsLaunched || (double)(this.m_Disc.Position - target.Position).LengthSquared >= 2250000.0)
					return;
				this.m_State = VonNeumannDiscStates.LAUNCHPODS;
			}
		}

		private void ThinkLaunchPods()
		{
			bool flag = true;
			foreach (Ship possessorBoardingPod in this.m_PossessorBoardingPods)
			{
				if (!possessorBoardingPod.DockedWithParent)
					flag = false;
			}
			if (!flag)
				return;
			this.m_Disc.PostSetProp("LaunchBattleriders");
			this.m_State = VonNeumannDiscStates.WAITFORLAUNCH;
		}

		private void ThinkWaitForLaunch()
		{
			bool flag = true;
			foreach (Ship possessorBoardingPod in this.m_PossessorBoardingPods)
			{
				if (possessorBoardingPod.DockedWithParent)
					flag = false;
			}
			if (!flag)
				return;
			this.m_PodsLaunched = true;
			this.m_State = VonNeumannDiscStates.SEEK;
		}

		private void ThinkActivateBoardingPods()
		{
			bool flag = true;
			foreach (GameObject possessorBoardingPod in this.m_PossessorBoardingPods)
			{
				if (possessorBoardingPod.ObjectStatus != GameObjectStatus.Ready)
					flag = false;
			}
			if (!flag)
				return;
			foreach (Ship possessorBoardingPod in this.m_PossessorBoardingPods)
			{
				BattleRiderSquad squad = this.m_Disc.AssignRiderToSquad(possessorBoardingPod as BattleRiderShip, possessorBoardingPod.RiderIndex);
				if (squad != null)
				{
					possessorBoardingPod.ParentID = this.m_Disc.ObjectID;
					possessorBoardingPod.PostSetBattleRiderParent(squad.ObjectID);
					possessorBoardingPod.Player = this.m_Disc.Player;
				}
				possessorBoardingPod.Active = true;
				this.m_Game.CurrentState.AddGameObject((IGameObject)possessorBoardingPod, false);
			}
			this.m_State = VonNeumannDiscStates.SEEK;
		}

		public void SetListOfDiscs(List<VonNeumannDiscControl> discs)
		{
			foreach (VonNeumannDiscControl disc in discs)
			{
				if (!this.m_Discs.Contains(disc))
					this.m_Discs.Add(disc);
			}
		}

		private void UpdatePods()
		{
			if (!this.m_PodsLaunched)
				return;
			this.m_PodsLaunched = false;
			foreach (Ship possessorBoardingPod in this.m_PossessorBoardingPods)
			{
				if (!possessorBoardingPod.DockedWithParent)
				{
					this.m_PodsLaunched = true;
					break;
				}
			}
		}

		private void UpdateShieldDisc()
		{
			if (this.m_Disc == null || this.m_Disc.IsDestroyed)
				return;
			--this.m_UpdateDelay;
			if (this.m_UpdateDelay > 0)
				return;
			this.m_UpdateDelay = 60;
			foreach (VonNeumannDiscControl disc in this.m_Discs)
			{
				if (disc.GetShip().Shield != null)
				{
					if (disc.GetShip() == this.m_Disc)
					{
						if (!disc.GetShip().Shield.Active)
							disc.GetShip().Shield.Active = true;
					}
					else if ((double)(disc.GetShip().Position - this.m_Disc.Position).LengthSquared < 250000.0)
					{
						if (!disc.GetShip().Shield.Active)
							disc.GetShip().Shield.Active = true;
					}
					else if (disc.GetShip().Shield.Active)
						disc.GetShip().Shield.Active = false;
				}
			}
		}

		private void ShieldDiscDestroyed()
		{
			foreach (CombatAIController disc in this.m_Discs)
				disc.GetShip().Shield.Active = false;
		}

		private void UpdateCloakDisc()
		{
			if (this.m_Disc == null || this.m_Disc.IsDestroyed)
				return;
			--this.m_UpdateDelay;
			if (this.m_UpdateDelay > 0)
				return;
			this.m_UpdateDelay = 60;
			foreach (VonNeumannDiscControl disc in this.m_Discs)
			{
				if (disc.GetShip() == this.m_Disc)
				{
					if (disc.GetShip().CloakedState == CloakedState.None)
						disc.GetShip().SetCloaked(true);
				}
				else if ((double)(disc.GetShip().Position - this.m_Disc.Position).LengthSquared < 250000.0)
				{
					if (disc.GetShip().CloakedState == CloakedState.None)
						disc.GetShip().SetCloaked(true);
				}
				else if (disc.GetShip().CloakedState != CloakedState.None)
					disc.GetShip().SetCloaked(false);
			}
		}

		private void CloakDiscDestroyed()
		{
			foreach (CombatAIController disc in this.m_Discs)
				disc.GetShip().SetCloaked(false);
		}
	}
}
