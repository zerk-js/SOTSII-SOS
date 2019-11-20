// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.MeteorCombatAIControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal class MeteorCombatAIControl : CombatAIController
	{
		private App m_Game;
		private Ship m_Meteor;
		private bool m_VictoryConditionsMet;
		private bool m_FailureConditionMet;
		internal int m_AddedResources;
		internal bool m_StruckPlanet;
		private bool m_CanSubDivide;
		private bool m_CanApplyResources;
		private int m_UpdateRate;
		private MeteorShowerGlobalData.MeteorSizes m_Size;
		private StellarBody m_Target;
		private Vector3 m_TargetPosition;
		private Vector3 m_TargetFacing;
		private List<StellarBody> m_Planets;
		private SimpleAIStates m_State;

		public static MeteorShowerGlobalData.MeteorSizes GetMeteorSize(
		  App game,
		  int designID)
		{
			if (game.Game.ScriptModules.MeteorShower == null)
				return MeteorShowerGlobalData.MeteorSizes.Small;
			DesignInfo designInfo = game.GameDatabase.GetDesignInfo(designID);
			if (designInfo != null)
			{
				DesignSectionInfo designSectionInfo = ((IEnumerable<DesignSectionInfo>)designInfo.DesignSections).FirstOrDefault<DesignSectionInfo>();
				if (designSectionInfo != null)
				{
					if (designSectionInfo.ShipSectionAsset.FileName.Contains("Medium"))
						return MeteorShowerGlobalData.MeteorSizes.Medium;
					if (designSectionInfo.ShipSectionAsset.FileName.Contains("Large"))
						return MeteorShowerGlobalData.MeteorSizes.Large;
				}
			}
			return MeteorShowerGlobalData.MeteorSizes.Small;
		}

		public static bool CanSubDivide(MeteorShowerGlobalData.MeteorSizes meteorSize)
		{
			return meteorSize != MeteorShowerGlobalData.MeteorSizes.Small;
		}

		public static List<int> GetAvailableSubMeteorDesignIDs(
		  App game,
		  MeteorShowerGlobalData.MeteorSizes meteorSize)
		{
			List<int> intList = new List<int>();
			if (game.Game.ScriptModules.MeteorShower == null)
				return intList;
			int[] meteorDesignIds = game.Game.ScriptModules.MeteorShower.MeteorDesignIds;
			int num = 0;
			switch (meteorSize)
			{
				case MeteorShowerGlobalData.MeteorSizes.Medium:
					num = meteorDesignIds.Length - 7;
					break;
				case MeteorShowerGlobalData.MeteorSizes.Large:
					num = meteorDesignIds.Length - 5;
					break;
			}
			for (int index = 0; index < meteorDesignIds.Length && index < num; ++index)
				intList.Add(meteorDesignIds[index]);
			return intList;
		}

		public override Ship GetShip()
		{
			return this.m_Meteor;
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

		public MeteorCombatAIControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_Meteor = ship;
		}

		public override void Initialize()
		{
			this.m_State = SimpleAIStates.SEEK;
			this.m_VictoryConditionsMet = false;
			this.m_FailureConditionMet = false;
			this.m_Planets = new List<StellarBody>();
			this.m_UpdateRate = 0;
			this.m_Size = MeteorCombatAIControl.GetMeteorSize(this.m_Game, this.m_Meteor.DesignID);
			this.m_CanSubDivide = MeteorCombatAIControl.CanSubDivide(this.m_Size);
			this.m_CanApplyResources = true;
			this.m_Target = (StellarBody)null;
			this.m_TargetFacing = -Vector3.UnitZ;
			this.m_TargetPosition = Vector3.Zero;
			MeteorShowerGlobalData meteorShowerData = this.m_Game.Game.AssetDatabase.GlobalMeteorShowerData;
			this.m_Meteor.Maneuvering.PostSetProp("SetCombatAIDamage", (object)meteorShowerData.Damage[(int)this.m_Size].Crew, (object)meteorShowerData.Damage[(int)this.m_Size].Population, (object)meteorShowerData.Damage[(int)this.m_Size].InfraDamage, (object)meteorShowerData.Damage[(int)this.m_Size].TeraDamage);
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
			if (obj != this.m_Meteor)
				return;
			if (this.m_Meteor.IsDestroyed)
			{
				this.SpawnSmallerMeteors();
				this.ApplyResourcesToPlanet();
			}
			this.m_Meteor = (Ship)null;
		}

		public override void OnThink()
		{
			if (this.m_Meteor == null)
				return;
			if (this.m_Meteor.IsDestroyed)
			{
				this.ApplyResourcesToPlanet();
				this.SpawnSmallerMeteors();
			}
			else
			{
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
		}

		public override void ForceFlee()
		{
		}

		public override bool VictoryConditionIsMet()
		{
			if (!this.m_VictoryConditionsMet)
				return this.m_FailureConditionMet;
			return true;
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
			this.m_Meteor.Maneuvering.PostAddGoal(this.m_TargetPosition, this.m_TargetFacing);
			this.m_State = SimpleAIStates.TRACK;
		}

		private void ThinkTrack()
		{
			if (this.m_FailureConditionMet)
				return;
			--this.m_UpdateRate;
			if (this.m_UpdateRate > 0)
				return;
			this.m_UpdateRate = 15;
			this.m_Meteor.Maneuvering.PostAddGoal(this.m_TargetPosition, this.m_TargetFacing);
			this.CheckForPlanetsInPath();
		}

		private void ObtainPositionAndFacing()
		{
			this.m_TargetFacing = Matrix.CreateRotationYPR(this.m_Meteor.Maneuvering.Rotation).Forward;
			this.m_TargetPosition = this.m_Meteor.Maneuvering.Position + this.m_TargetFacing * 500000f;
		}

		private void CheckForPlanetsInPath()
		{
			Matrix rotationYpr = Matrix.CreateRotationYPR(this.m_Meteor.Maneuvering.Rotation);
			rotationYpr.Position = this.m_Meteor.Maneuvering.Position;
			Matrix mat = Matrix.Inverse(rotationYpr);
			float radius = this.m_Meteor.ShipSphere.radius;
			bool flag = false;
			float num1 = float.MaxValue;
			StellarBody stellarBody = (StellarBody)null;
			foreach (StellarBody planet in this.m_Planets)
			{
				if (this.m_Game.GameDatabase.GetColonyInfoForPlanet(planet.Parameters.OrbitalID) != null)
				{
					Vector3 vector3 = Vector3.Transform(planet.Parameters.Position, mat);
					if ((double)Math.Abs(vector3.X) < (double)radius + (double)planet.Parameters.Radius && (double)Math.Abs(vector3.Y) < (double)radius + (double)planet.Parameters.Radius && (double)vector3.Z < 0.0)
					{
						flag = true;
						float num2 = -vector3.Z - planet.Parameters.Radius;
						if ((double)num2 < (double)num1)
						{
							num1 = num2;
							stellarBody = planet;
						}
					}
				}
			}
			if (stellarBody != this.m_Target)
			{
				this.m_Meteor.SetShipTarget(stellarBody != null ? stellarBody.ObjectID : 0, Vector3.Zero, true, 0);
				this.SetTarget((IGameObject)stellarBody);
			}
			if (flag)
				return;
			this.m_FailureConditionMet = true;
		}

		private void SpawnSmallerMeteors()
		{
			if (!this.m_CanSubDivide || this.m_Meteor.InstantlyKilled)
				return;
			this.m_CanSubDivide = false;
			Random random = new Random();
			Vector3 forward = this.m_Target != null ? this.m_Target.Parameters.Position - this.m_Meteor.Maneuvering.Position : this.m_TargetPosition - this.m_Meteor.Maneuvering.Position;
			double num1 = (double)forward.Normalize();
			Matrix world = Matrix.CreateWorld(this.m_Meteor.Maneuvering.Position, forward, Vector3.UnitY);
			Sphere shipSphere = this.m_Meteor.ShipSphere;
			int numBreakoffMeteors = this.m_Game.AssetDatabase.GlobalMeteorShowerData.NumBreakoffMeteors;
			float radians = MathHelper.DegreesToRadians(30f);
			List<int> subMeteorDesignIds = MeteorCombatAIControl.GetAvailableSubMeteorDesignIDs(this.m_Game, this.m_Size);
			if (subMeteorDesignIds.Count <= 0)
				return;
			for (int index = 0; index < numBreakoffMeteors; ++index)
			{
				Vector3 vector3 = new Vector3();
				vector3.X = (random.CoinToss(0.5) ? -1f : 1f) * random.NextInclusive(10f, 85f);
				vector3.Y = (random.CoinToss(0.5) ? -1f : 1f) * random.NextInclusive(10f, 85f);
				vector3.Z = (random.CoinToss(0.5) ? -1f : 1f) * random.NextInclusive(10f, 85f);
				double num2 = (double)vector3.Normalize();
				vector3 *= random.NextInclusive(shipSphere.radius * 0.1f, shipSphere.radius);
				Matrix worldMat = Matrix.PolarDeviation(random, radians);
				worldMat.Position = vector3;
				worldMat *= world;
				int designId = subMeteorDesignIds[random.NextInclusive(0, subMeteorDesignIds.Count - 1)];
				this.m_Game.CurrentState.AddGameObject((IGameObject)CombatAIController.CreateNewShip(this.m_Game.Game, worldMat, designId, 0, this.m_Meteor.InputID, this.m_Meteor.Player.ObjectID), true);
			}
		}

		private void ApplyResourcesToPlanet()
		{
			if (this.m_Target == null || this.m_Target == null || !this.m_CanApplyResources)
				return;
			this.m_CanApplyResources = false;
			this.m_StruckPlanet = this.m_Meteor.InstantlyKilled;
			int num = (int)((double)this.m_Game.AssetDatabase.GlobalMeteorShowerData.ResourceBonuses[(int)this.m_Size] * (this.m_Meteor.InstantlyKilled ? 0.100000001490116 : 1.0));
			PlanetInfo planetInfo = this.m_Game.GameDatabase.GetPlanetInfo(this.m_Target.Parameters.OrbitalID);
			if (planetInfo == null)
				return;
			planetInfo.Resources += num;
			this.m_Game.GameDatabase.UpdatePlanet(planetInfo);
			this.m_AddedResources = num;
		}
	}
}
