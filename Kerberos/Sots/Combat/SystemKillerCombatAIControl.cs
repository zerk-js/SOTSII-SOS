// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.SystemKillerCombatAIControl
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
	internal class SystemKillerCombatAIControl : CombatAIController
	{
		protected App m_Game;
		protected Ship m_SystemKiller;
		protected WeaponBank m_BeamBank;
		protected bool m_VictoryConditionsMet;
		protected bool m_SpaceBattle;
		protected List<StellarBody> m_Planets;
		protected List<MoonData> m_Moons;
		protected List<StarModel> m_Stars;
		protected IGameObject m_CurrentTarget;
		protected Vector3 m_TargetCenter;
		protected Vector3 m_TargetLook;
		protected int m_TrackUpdateRate;
		protected float m_PlanetOffsetDist;
		protected SystemKillerStates m_State;

		public override Ship GetShip()
		{
			return this.m_SystemKiller;
		}

		public override void SetTarget(IGameObject target)
		{
			this.m_CurrentTarget = target;
		}

		public override IGameObject GetTarget()
		{
			return this.m_CurrentTarget;
		}

		public SystemKillerCombatAIControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_SystemKiller = ship;
		}

		public override void Initialize()
		{
			this.m_Planets = new List<StellarBody>();
			this.m_Moons = new List<MoonData>();
			this.m_Stars = new List<StarModel>();
			this.m_VictoryConditionsMet = false;
			this.m_SpaceBattle = false;
			this.m_TrackUpdateRate = 0;
			this.m_PlanetOffsetDist = 0.0f;
			this.m_BeamBank = this.m_SystemKiller.GetWeaponBankWithWeaponTrait(WeaponEnums.WeaponTraits.PlanetKilling);
			if (this.m_BeamBank == null)
				return;
			this.m_BeamBank.PostSetProp("RequestFireStateChange", true);
			this.m_BeamBank.PostSetProp("DisableAllTurrets", true);
		}

		public override void Terminate()
		{
		}

		public override void ObjectRemoved(IGameObject obj)
		{
			if (obj == this.m_CurrentTarget)
				this.m_CurrentTarget = (IGameObject)null;
			if (this.m_BeamBank == obj)
				this.m_BeamBank = (WeaponBank)null;
			if (obj is StellarBody)
			{
				StellarBody stellarBody = obj as StellarBody;
				this.m_Planets.Remove(stellarBody);
				List<MoonData> moonDataList = new List<MoonData>();
				foreach (MoonData moon in this.m_Moons)
				{
					if (moon.Moon == stellarBody)
						moonDataList.Add(moon);
					else if (moon.ParentID == stellarBody.Parameters.OrbitalID)
						moonDataList.Add(moon);
				}
				foreach (MoonData moonData in moonDataList)
					this.m_Moons.Remove(moonData);
			}
			if (!(obj is StarModel))
				return;
			this.m_Stars.Remove(obj as StarModel);
		}

		public override void OnThink()
		{
			if (this.m_SystemKiller == null)
				return;
			switch (this.m_State)
			{
				case SystemKillerStates.SEEK:
					this.ThinkSeek();
					break;
				case SystemKillerStates.TRACK:
					this.ThinkTrack();
					break;
				case SystemKillerStates.FIREBEAM:
					this.ThinkFireBeam();
					break;
				case SystemKillerStates.FIRINGBEAM:
					this.ThinkFiringBeam();
					break;
				case SystemKillerStates.SPACEBATTLE:
					this.ThinkSpaceBattle();
					break;
				case SystemKillerStates.VICTORY:
					this.ThinkVictory();
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
			if (this.m_State == SystemKillerStates.SEEK && this.m_Planets.Count == 0 && this.m_Stars.Count == 0)
				return !this.m_SpaceBattle;
			return false;
		}

		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			this.m_SpaceBattle = true;
			List<StellarBody> source = new List<StellarBody>();
			List<MoonData> moonDataList = new List<MoonData>();
			List<StarModel> starModelList = new List<StarModel>();
			foreach (IGameObject gameObject in objs)
			{
				if (gameObject is StellarBody)
				{
					StellarBody stellarBody = gameObject as StellarBody;
					source.Add(stellarBody);
					this.m_SpaceBattle = false;
				}
				else if (gameObject is StarModel)
				{
					StarModel starModel = gameObject as StarModel;
					starModelList.Add(starModel);
					this.m_SpaceBattle = false;
				}
			}
			foreach (StellarBody stellarBody in source)
			{
				foreach (OrbitalObjectInfo moon in this.m_Game.GameDatabase.GetMoons(stellarBody.Parameters.OrbitalID))
				{
					OrbitalObjectInfo ooi = moon;
					moonDataList.Add(new MoonData()
					{
						ParentID = ooi.ParentID.Value,
						Moon = source.First<StellarBody>((Func<StellarBody, bool>)(x => x.Parameters.OrbitalID == ooi.ID))
					});
				}
			}
			foreach (MoonData moonData in moonDataList)
				source.Remove(moonData.Moon);
			this.m_Planets = source;
			this.m_Moons = moonDataList;
			this.m_Stars = starModelList;
			if (!this.m_SpaceBattle)
				return;
			this.m_State = SystemKillerStates.SPACEBATTLE;
		}

		public override bool NeedsAParent()
		{
			return false;
		}

		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
		}

		protected virtual void ThinkSeek()
		{
			if (this.m_CurrentTarget == null)
				this.FindCurrentTarget(false);
			else
				this.m_State = SystemKillerStates.TRACK;
		}

		protected virtual void ThinkTrack()
		{
			if (this.m_CurrentTarget == null)
			{
				this.m_State = SystemKillerStates.SEEK;
			}
			else
			{
				--this.m_TrackUpdateRate;
				if (this.m_TrackUpdateRate > 0)
					return;
				this.m_TrackUpdateRate = 10;
				Vector3 vector3 = this.m_SystemKiller.Position - this.m_TargetCenter;
				double num = (double)vector3.Normalize();
				Vector3 targetPos = this.m_TargetCenter + vector3 * this.m_PlanetOffsetDist;
				this.m_TargetLook = -vector3;
				this.m_SystemKiller.Maneuvering.PostAddGoal(targetPos, this.m_TargetLook);
				Matrix rotationYpr = Matrix.CreateRotationYPR(this.m_SystemKiller.Rotation);
				if (this.m_SystemKiller.Target == null || ((double)(this.m_SystemKiller.Position - targetPos).LengthSquared > 9000.0 || (double)Vector3.Dot(rotationYpr.Forward, this.m_TargetLook) <= 0.800000011920929))
					return;
				if (this.m_BeamBank != null)
					this.m_BeamBank.PostSetProp("DisableAllTurrets", false);
				this.m_State = SystemKillerStates.FIREBEAM;
			}
		}

		private void ThinkFireBeam()
		{
			if (this.m_BeamBank == null || this.m_CurrentTarget == null)
			{
				this.m_State = SystemKillerStates.SEEK;
			}
			else
			{
				if (this.m_SystemKiller.ListenTurretFiring != Turret.FiringEnum.Firing)
					return;
				this.m_State = SystemKillerStates.FIRINGBEAM;
			}
		}

		private void ThinkFiringBeam()
		{
			if (this.m_BeamBank == null || this.m_CurrentTarget == null)
			{
				this.m_State = SystemKillerStates.SEEK;
			}
			else
			{
				if (this.m_SystemKiller.ListenTurretFiring == Turret.FiringEnum.Firing)
					return;
				if (this.m_SystemKiller.ListenTurretFiring == Turret.FiringEnum.Completed)
				{
					this.m_SystemKiller.PostSetProp("FullyHealShip");
					if (this.m_CurrentTarget is StellarBody)
						this.m_Planets.Remove(this.m_CurrentTarget as StellarBody);
					else if (this.m_CurrentTarget is StarModel)
						this.m_Stars.Remove(this.m_CurrentTarget as StarModel);
					this.m_CurrentTarget = (IGameObject)null;
					this.m_VictoryConditionsMet = true;
				}
				this.m_State = SystemKillerStates.SEEK;
			}
		}

		private void ThinkSpaceBattle()
		{
			this.m_TargetCenter = Matrix.CreateRotationYPR(this.m_SystemKiller.Rotation).Forward * (((IEnumerable<float>)Kerberos.Sots.GameStates.StarSystem.CombatZoneMapRadii).Last<float>() * 5700f);
			this.m_TargetLook = this.m_SystemKiller.Position - this.m_TargetCenter;
			this.m_TargetLook.Y = 0.0f;
			double num = (double)this.m_TargetLook.Normalize();
			Vector3 targetPos = this.m_TargetCenter + this.m_TargetLook * this.m_PlanetOffsetDist;
			this.m_SystemKiller.Maneuvering.PostAddGoal(targetPos, this.m_TargetLook);
			if ((double)(this.m_SystemKiller.Position - targetPos).LengthSquared > 90000.0)
				return;
			this.m_VictoryConditionsMet = true;
			this.m_State = SystemKillerStates.SEEK;
		}

		private void ThinkVictory()
		{
		}

		protected void FindCurrentTarget(bool needColony = false)
		{
			if (this.m_SpaceBattle || this.m_SystemKiller == null)
				return;
			float num1 = 0.0f;
			float num2 = 5000f;
			Vector3 vector3_1 = new Vector3();
			int orbitalObjectID = 0;
			foreach (StellarBody planet in this.m_Planets)
			{
				if (!needColony || planet.Population != 0.0)
				{
					float lengthSquared = planet.Parameters.Position.LengthSquared;
					if ((double)lengthSquared > (double)num1)
					{
						num1 = lengthSquared;
						this.m_CurrentTarget = (IGameObject)planet;
						orbitalObjectID = planet.Parameters.OrbitalID;
						vector3_1 = planet.Parameters.Position;
						num2 = planet.Parameters.Radius + 750f;
					}
				}
			}
			if (!needColony)
			{
				if (this.m_CurrentTarget != null)
				{
					num1 = 0.0f;
					IGameObject gameObject = (IGameObject)null;
					foreach (MoonData moonData in this.m_Moons.Where<MoonData>((Func<MoonData, bool>)(x => x.ParentID == orbitalObjectID)))
					{
						float lengthSquared = moonData.Moon.Parameters.Position.LengthSquared;
						if ((double)lengthSquared > (double)num1)
						{
							num1 = lengthSquared;
							gameObject = (IGameObject)moonData.Moon;
							vector3_1 = moonData.Moon.Parameters.Position;
							num2 = moonData.Moon.Parameters.Radius + 750f;
						}
					}
					if (gameObject != null)
						this.m_CurrentTarget = gameObject;
				}
				if (this.m_CurrentTarget == null)
				{
					foreach (StarModel star in this.m_Stars)
					{
						float lengthSquared = star.Position.LengthSquared;
						if ((double)lengthSquared <= (double)num1)
						{
							num1 = lengthSquared;
							this.m_CurrentTarget = (IGameObject)star;
							vector3_1 = star.Position;
							num2 = star.Radius + 7500f;
						}
					}
				}
			}
			if (this.m_CurrentTarget != null)
			{
				Vector3 vector3_2 = this.m_SystemKiller.Position - vector3_1;
				double num3 = (double)vector3_2.Normalize();
				this.m_PlanetOffsetDist = (float)((double)num2 + (double)this.m_SystemKiller.ShipSphere.radius + 500.0);
				this.m_TargetCenter = vector3_1;
				this.m_TargetLook = -vector3_2;
			}
			else if (!needColony)
				this.m_VictoryConditionsMet = true;
			this.m_SystemKiller.Target = this.m_CurrentTarget;
			if (this.m_CurrentTarget == null || this.m_BeamBank == null)
				return;
			this.m_BeamBank.PostSetProp("SetTarget", (object)this.m_CurrentTarget.ObjectID, (object)Vector3.Zero, (object)true);
		}
	}
}
