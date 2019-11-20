// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.VonNeumannBerserkerControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Encounters;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal class VonNeumannBerserkerControl : VonNeumannNeoBerserkerControl
	{
		private const int MAX_PYRAMIDS = 14;
		private SpinningWheelFormation m_PyramidFormation;
		private List<Ship> m_LoadingPyramids;
		private List<VonNeumannPyramidControl> m_Pyramids;
		private bool m_FormationInitialized;
		private bool m_PyramidsLaunched;

		public VonNeumannBerserkerControl(App game, Ship ship)
		  : base(game, ship)
		{
		}

		public override void Initialize()
		{
			base.Initialize();
			this.m_FormationInitialized = false;
			this.m_LoadingPyramids = new List<Ship>();
			this.m_Pyramids = new List<VonNeumannPyramidControl>();
			Matrix rotationYpr = Matrix.CreateRotationYPR(this.m_VonNeumannBerserker.Maneuvering.Rotation);
			rotationYpr.Position = this.m_VonNeumannBerserker.Maneuvering.Position;
			int designId = VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.Pyramid].DesignId;
			for (int index = 0; index < 14; ++index)
			{
				Ship newShip = CombatAIController.CreateNewShip(this.m_Game.Game, rotationYpr, designId, this.m_VonNeumannBerserker.ObjectID, this.m_VonNeumannBerserker.InputID, this.m_VonNeumannBerserker.Player.ObjectID);
				if (newShip != null)
					this.m_LoadingPyramids.Add(newShip);
			}
			this.m_PyramidsLaunched = false;
			this.m_State = VonNeumannBerserkerStates.INTEGRATECHILDS;
		}

		private void InitPyramidFormation()
		{
			if (this.m_FormationInitialized)
				return;
			List<object> objectList = new List<object>();
			objectList.Add((object)Vector3.Zero);
			objectList.Add((object)-Vector3.UnitZ);
			objectList.Add((object)this.m_Pyramids.Count);
			Matrix rotationYpr = Matrix.CreateRotationYPR(this.m_VonNeumannBerserker.Maneuvering.Rotation);
			Vector3 pyramidsCom = this.GetPyramidsCOM();
			rotationYpr.Position = pyramidsCom;
			Matrix mat = Matrix.Inverse(rotationYpr);
			foreach (CombatAIController pyramid in this.m_Pyramids)
			{
				Ship ship = pyramid.GetShip();
				objectList.Add((object)ship.ObjectID);
				Vector3 vector3 = Vector3.Transform(ship.Position, mat);
				objectList.Add((object)vector3);
			}
			this.m_PyramidFormation = this.m_Game.AddObject<SpinningWheelFormation>(objectList.ToArray());
			this.m_FormationInitialized = true;
		}

		private Vector3 GetPyramidsCOM()
		{
			if (this.m_Pyramids.Count == 0)
				return this.m_VonNeumannBerserker.Maneuvering.Position;
			Vector3 com = Vector3.Zero;
			this.m_Pyramids.ForEach((Action<VonNeumannPyramidControl>)(x => com += x.GetShip().Position));
			return com / (float)this.m_Pyramids.Count;
		}

		public override void Terminate()
		{
			if (this.m_PyramidFormation == null)
				return;
			this.m_Game.ReleaseObject((IGameObject)this.m_PyramidFormation);
		}

		public override void ObjectRemoved(IGameObject obj)
		{
			base.ObjectRemoved(obj);
			foreach (VonNeumannPyramidControl pyramid in this.m_Pyramids)
			{
				if (pyramid.GetShip() == obj)
				{
					this.m_Pyramids.Remove(pyramid);
					break;
				}
			}
			foreach (Ship loadingPyramid in this.m_LoadingPyramids)
			{
				if (loadingPyramid == obj)
				{
					this.m_LoadingPyramids.Remove(loadingPyramid);
					break;
				}
			}
		}

		public override bool IsThisMyMom(Ship ship)
		{
			if (!this.m_LoadingPyramids.Any<Ship>((Func<Ship, bool>)(x => x == ship)))
				return base.IsThisMyMom(ship);
			return true;
		}

		public override void AddChild(CombatAIController child)
		{
			base.AddChild(child);
			if (!(child is VonNeumannPyramidControl))
				return;
			foreach (Ship loadingPyramid in this.m_LoadingPyramids)
			{
				if (loadingPyramid == child.GetShip())
				{
					this.m_LoadingPyramids.Remove(loadingPyramid);
					break;
				}
			}
			this.m_Pyramids.Add(child as VonNeumannPyramidControl);
		}

		public override void OnThink()
		{
			if (this.m_VonNeumannBerserker == null)
				return;
			switch (this.m_State)
			{
				case VonNeumannBerserkerStates.INTEGRATECHILDS:
					this.ThinkIntegrateChild();
					break;
				case VonNeumannBerserkerStates.SEEK:
					this.ThinkSeek();
					break;
				case VonNeumannBerserkerStates.TRACK:
					this.ThinkTrack();
					break;
				case VonNeumannBerserkerStates.BOMBARD:
					this.ThinkBombard();
					break;
				case VonNeumannBerserkerStates.LAUNCHING:
					this.ThinkLaunching();
					break;
				case VonNeumannBerserkerStates.ACTIVATINGDISCS:
					this.ThinkActivatingDiscs();
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
				return this.m_State == VonNeumannBerserkerStates.SEEK;
			return false;
		}

		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			base.FindNewTarget(objs);
			float num = float.MaxValue;
			IGameObject target = (IGameObject)null;
			foreach (IGameObject gameObject in objs)
			{
				if (gameObject is StellarBody)
				{
					StellarBody stellarBody = gameObject as StellarBody;
					if (stellarBody.Parameters.ColonyPlayerID != this.m_VonNeumannBerserker.Player.ID)
					{
						float lengthSquared = (stellarBody.Parameters.Position - this.m_VonNeumannBerserker.Position).LengthSquared;
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

		public override bool NeedsAParent()
		{
			return false;
		}

		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
		}

		private void ThinkIntegrateChild()
		{
			if (this.m_LoadingDiscs.Count == 0 && this.m_LoadingPyramids.Count == 0)
			{
				this.m_State = VonNeumannBerserkerStates.SEEK;
			}
			else
			{
				bool flag = true;
				foreach (GameObject loadingDisc in this.m_LoadingDiscs)
				{
					if (loadingDisc.ObjectStatus != GameObjectStatus.Ready)
						flag = false;
				}
				foreach (GameObject loadingPyramid in this.m_LoadingPyramids)
				{
					if (loadingPyramid.ObjectStatus != GameObjectStatus.Ready)
						flag = false;
				}
				if (!flag)
					return;
				foreach (Ship loadingDisc in this.m_LoadingDiscs)
				{
					loadingDisc.Player = this.m_VonNeumannBerserker.Player;
					loadingDisc.Active = false;
					this.m_Game.CurrentState.AddGameObject((IGameObject)loadingDisc, false);
				}
				foreach (Ship loadingPyramid in this.m_LoadingPyramids)
				{
					loadingPyramid.Player = this.m_VonNeumannBerserker.Player;
					loadingPyramid.Active = true;
					this.m_Game.CurrentState.AddGameObject((IGameObject)loadingPyramid, false);
				}
				this.m_State = VonNeumannBerserkerStates.SEEK;
			}
		}

		protected override void ThinkTrack()
		{
			base.ThinkTrack();
			if (this.m_Target == null)
				return;
			Vector3 vector3 = this.m_VonNeumannBerserker.Position - this.m_Target.Parameters.Position;
			double num = (double)vector3.Normalize();
			Vector3 targetPos = this.m_Target.Parameters.Position + vector3 * (float)((double)this.m_Target.Parameters.Radius + 750.0 + 1000.0);
			this.m_VonNeumannBerserker.Maneuvering.PostAddGoal(targetPos, -vector3);
			if ((double)(this.m_VonNeumannBerserker.Position - targetPos).LengthSquared >= 100.0)
				return;
			if (!this.m_DiscsActivated)
			{
				this.m_State = VonNeumannBerserkerStates.ACTIVATINGDISCS;
			}
			else
			{
				if (this.m_PyramidsLaunched)
					return;
				this.m_State = VonNeumannBerserkerStates.BOMBARD;
			}
		}

		private void ThinkBombard()
		{
			if (this.m_Target == null || this.m_Target == null)
			{
				this.m_State = VonNeumannBerserkerStates.SEEK;
			}
			else
			{
				this.m_VonNeumannBerserker.PostSetProp("LaunchBattleriders");
				this.m_State = VonNeumannBerserkerStates.LAUNCHING;
			}
		}

		private void ThinkLaunching()
		{
			bool flag = true;
			foreach (CombatAIController pyramid in this.m_Pyramids)
			{
				if (pyramid.GetShip().DockedWithParent)
				{
					flag = false;
					break;
				}
			}
			if (!flag)
				return;
			this.InitPyramidFormation();
			if (this.m_PyramidFormation.ObjectStatus != GameObjectStatus.Ready)
				return;
			foreach (CombatAIController pyramid in this.m_Pyramids)
				pyramid.SetTarget((IGameObject)this.m_Target);
			Vector3 facing = this.m_Target.Parameters.Position - this.m_VonNeumannBerserker.Position;
			double num = (double)facing.Normalize();
			this.m_PyramidFormation.PostFormationDefinition(this.GetPyramidsCOM(), facing, Vector3.Zero);
			this.m_PyramidFormation.PostSetActive(true);
			this.m_PyramidsLaunched = true;
			this.m_State = VonNeumannBerserkerStates.SEEK;
		}
	}
}
