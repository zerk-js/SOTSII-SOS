// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.LocustNestControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
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
	internal class LocustNestControl : CombatAIController
	{
		private App m_Game;
		protected Ship m_LocustNest;
		private List<Ship> m_LoadingFighters;
		private List<Ship> m_LaunchingFighters;
		protected List<StellarBody> m_Planets;
		private List<LocustFighterControl> m_SpawnedFighters;
		protected List<LocustTarget> m_TargetList;
		private IGameObject m_Target;
		private LocustNestStates m_State;
		private int m_FleetID;
		private int m_LocustSwarmID;
		private int m_MaxNumFighters;
		private int m_MaxNumFightersTotal;
		private int m_NumDestroyedDrones;
		private int m_NumLocustsReachedPlanet;
		protected int m_UpdateListDelay;
		protected bool m_UpdateTargetList;

		public override Ship GetShip()
		{
			return this.m_LocustNest;
		}

		public override void SetTarget(IGameObject target)
		{
			this.m_Target = target;
		}

		public override IGameObject GetTarget()
		{
			return this.m_Target;
		}

		public LocustNestStates State
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

		public LocustNestControl(App game, Ship ship, int fleetId)
		{
			this.m_Game = game;
			this.m_LocustNest = ship;
			this.m_TargetList = new List<LocustTarget>();
			this.m_FleetID = fleetId;
		}

		public override void Initialize()
		{
			this.m_LoadingFighters = new List<Ship>();
			this.m_LaunchingFighters = new List<Ship>();
			this.m_SpawnedFighters = new List<LocustFighterControl>();
			this.m_Planets = new List<StellarBody>();
			this.m_State = LocustNestStates.SEEK;
			this.m_Target = (IGameObject)null;
			this.m_MaxNumFighters = this is LocustMoonControl ? this.m_Game.AssetDatabase.GlobalLocustData.MaxMoonCombatDrones : this.m_Game.AssetDatabase.GlobalLocustData.MaxCombatDrones;
			this.m_NumDestroyedDrones = 0;
			this.m_UpdateListDelay = 0;
			this.m_UpdateTargetList = false;
			this.m_LocustNest.PostSetProp("SetAsOnlyLaunchCarrier", true);
			this.m_NumLocustsReachedPlanet = this.m_Game.AssetDatabase.GlobalLocustData.NumToLand;
			if (this is LocustMoonControl)
				this.m_LocustNest.Maneuvering.TargetFacingAngle = TargetFacingAngle.BroadSide;
			LocustSwarmInfo locustSwarmInfo = this.m_Game.GameDatabase.GetLocustSwarmInfos().ToList<LocustSwarmInfo>().FirstOrDefault<LocustSwarmInfo>((Func<LocustSwarmInfo, bool>)(x =>
		   {
			   int? fleetId1 = x.FleetId;
			   int fleetId2 = this.m_FleetID;
			   if (fleetId1.GetValueOrDefault() == fleetId2)
				   return fleetId1.HasValue;
			   return false;
		   }));
			this.m_LocustSwarmID = 0;
			this.m_MaxNumFightersTotal = this.m_Game.AssetDatabase.GlobalLocustData.MaxDrones;
			if (locustSwarmInfo != null)
			{
				this.m_LocustSwarmID = locustSwarmInfo.Id;
				this.m_MaxNumFightersTotal = locustSwarmInfo.NumDrones;
			}
			this.SpawnMaxFighters();
		}

		public override void Terminate()
		{
			LocustSwarmInfo locustSwarmInfo = this.m_Game.GameDatabase.GetLocustSwarmInfo(this.m_LocustSwarmID);
			if (locustSwarmInfo == null)
				return;
			locustSwarmInfo.NumDrones = Math.Max(locustSwarmInfo.NumDrones - this.m_NumDestroyedDrones, 0);
			this.m_Game.GameDatabase.UpdateLocustSwarmInfo(locustSwarmInfo);
		}

		public override void ObjectRemoved(IGameObject obj)
		{
			foreach (LocustFighterControl spawnedFighter in this.m_SpawnedFighters)
			{
				if (spawnedFighter.GetShip() == obj)
				{
					if (spawnedFighter.GetShip().IsDestroyed)
						++this.m_NumDestroyedDrones;
					this.m_SpawnedFighters.Remove(spawnedFighter);
					break;
				}
			}
			foreach (Ship loadingFighter in this.m_LoadingFighters)
			{
				if (loadingFighter == obj)
				{
					if (loadingFighter.IsDestroyed)
						++this.m_NumDestroyedDrones;
					this.m_LoadingFighters.Remove(loadingFighter);
					break;
				}
			}
			foreach (Ship launchingFighter in this.m_LaunchingFighters)
			{
				if (launchingFighter == obj)
				{
					if (launchingFighter.IsDestroyed)
						++this.m_NumDestroyedDrones;
					this.m_LaunchingFighters.Remove(launchingFighter);
					break;
				}
			}
			foreach (LocustTarget target in this.m_TargetList)
			{
				if (target.Target == obj)
				{
					this.m_TargetList.Remove(target);
					break;
				}
			}
			if (this.m_Target != obj)
				return;
			this.m_Target = (IGameObject)null;
		}

		public override void OnThink()
		{
			if (this.m_LocustNest == null)
				return;
			this.UpdateTargetList();
			switch (this.m_State)
			{
				case LocustNestStates.SEEK:
					this.ThinkSeek();
					break;
				case LocustNestStates.TRACK:
					this.ThinkTrack();
					break;
				case LocustNestStates.EMITFIGHTER:
					this.ThinkEmitFighter();
					break;
				case LocustNestStates.INTEGRATEFIGHTER:
					this.ThinkIntegrateFighter();
					break;
				case LocustNestStates.WAITFORDOCKED:
					this.ThinkWaitForDocked();
					break;
				case LocustNestStates.LAUNCHFIGHTER:
					this.ThinkLaunch();
					break;
				case LocustNestStates.WAITFORLAUNCH:
					this.ThinkWaitForLaunch();
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
			return this.m_UpdateTargetList;
		}

		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			this.m_Planets.Clear();
			List<LocustTarget> locustTargetList = new List<LocustTarget>();
			List<LocustTarget> source = new List<LocustTarget>();
			foreach (LocustTarget target in this.m_TargetList)
				target.ClearNumTargets();
			source.AddRange((IEnumerable<LocustTarget>)this.m_TargetList);
			foreach (IGameObject gameObject in objs)
			{
				if (gameObject is Ship)
				{
					Ship ship = gameObject as Ship;
					if (Ship.IsActiveShip(ship) && ship.Player != this.m_LocustNest.Player)
					{
						bool flag = ship.IsDetected(this.m_LocustNest.Player);
						foreach (LocustFighterControl spawnedFighter in this.m_SpawnedFighters)
						{
							if (ship == spawnedFighter.GetTarget())
							{
								if (!flag)
								{
									spawnedFighter.SetTarget((IGameObject)null);
									break;
								}
								break;
							}
						}
						if (flag)
						{
							locustTargetList.Add(new LocustTarget()
							{
								Target = ship
							});
							LocustTarget locustTarget = source.FirstOrDefault<LocustTarget>((Func<LocustTarget, bool>)(x => x.Target == ship));
							if (locustTarget != null)
								source.Remove(locustTarget);
						}
					}
				}
				if (gameObject is StellarBody)
				{
					StellarBody stellarBody = gameObject as StellarBody;
					if (this.m_Game.GameDatabase.GetColonyInfoForPlanet(stellarBody.Parameters.OrbitalID) != null)
						this.m_Planets.Add(stellarBody);
				}
			}
			if (source.Count > 0)
			{
				foreach (LocustFighterControl spawnedFighter in this.m_SpawnedFighters)
				{
					LocustFighterControl fighter = spawnedFighter;
					if (source.Any<LocustTarget>((Func<LocustTarget, bool>)(x => x.Target == fighter.GetTarget())))
						fighter.SetTarget((IGameObject)null);
				}
			}
			if (locustTargetList.Count > this.m_TargetList.Count)
			{
				foreach (LocustTarget target in this.m_TargetList)
				{
					ShipClass shipClass = target.Target.ShipClass;
					target.ClearNumTargets();
					int num = LocustFighterControl.NumFightersPerShip(shipClass);
					foreach (LocustFighterControl spawnedFighter in this.m_SpawnedFighters)
					{
						if (spawnedFighter.GetTarget() == target.Target)
						{
							if (num > 0)
							{
								--num;
								target.IncFightersOnTarget();
							}
							else
								spawnedFighter.SetTarget((IGameObject)null);
						}
					}
				}
			}
			else
			{
				foreach (LocustFighterControl spawnedFighter in this.m_SpawnedFighters)
				{
					LocustFighterControl fighter = spawnedFighter;
					if (fighter.GetTarget() != null)
						this.m_TargetList.FirstOrDefault<LocustTarget>((Func<LocustTarget, bool>)(x => x.Target == fighter.GetTarget()))?.IncFightersOnTarget();
				}
			}
			foreach (LocustTarget locustTarget in locustTargetList)
			{
				LocustTarget target = locustTarget;
				if (!this.m_TargetList.Any<LocustTarget>((Func<LocustTarget, bool>)(x => x.Target == target.Target)))
				{
					target.ClearNumTargets();
					this.m_TargetList.Add(target);
				}
			}
			this.m_UpdateTargetList = false;
		}

		public override bool NeedsAParent()
		{
			return false;
		}

		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
		}

		public bool IsThisMyNest(Ship ship)
		{
			return this.m_LoadingFighters.Any<Ship>((Func<Ship, bool>)(x => x == ship));
		}

		protected virtual void PickTarget()
		{
			IGameObject target = (IGameObject)null;
			float num = float.MaxValue;
			foreach (StellarBody planet in this.m_Planets)
			{
				float lengthSquared = (planet.Parameters.Position - this.m_LocustNest.Maneuvering.Position).LengthSquared;
				if ((double)lengthSquared < (double)num)
				{
					target = (IGameObject)planet;
					num = lengthSquared;
				}
			}
			this.SetTarget(target);
		}

		public void AddFighter(CombatAIController fighter)
		{
			if (fighter is LocustFighterControl)
			{
				foreach (Ship loadingFighter in this.m_LoadingFighters)
				{
					if (loadingFighter == fighter.GetShip())
					{
						this.m_LoadingFighters.Remove(loadingFighter);
						this.m_LaunchingFighters.Add(loadingFighter);
						break;
					}
				}
				this.m_SpawnedFighters.Add(fighter as LocustFighterControl);
			}
			if (this.m_LoadingFighters.Count != 0)
				return;
			this.m_State = LocustNestStates.WAITFORDOCKED;
		}

		private void ThinkSeek()
		{
			if (this.m_LoadingFighters.Count + this.m_SpawnedFighters.Count < this.m_MaxNumFighters)
				this.m_State = LocustNestStates.EMITFIGHTER;
			else if (this.m_Target != null)
				this.m_State = LocustNestStates.TRACK;
			else
				this.PickTarget();
		}

		private void ThinkTrack()
		{
			if (this.m_LoadingFighters.Count + this.m_SpawnedFighters.Count < this.m_MaxNumFighters)
				this.m_State = LocustNestStates.EMITFIGHTER;
			else if (this.m_Target == null)
			{
				this.m_State = LocustNestStates.SEEK;
			}
			else
			{
				Vector3 targetPos = Vector3.Zero;
				if (this.m_Target is StellarBody)
					targetPos = (this.m_Target as StellarBody).Parameters.Position;
				else if (this.m_Target is Ship)
					targetPos = (this.m_Target as Ship).Position;
				Vector3 look = targetPos - this.m_LocustNest.Maneuvering.Position;
				look.Y = 0.0f;
				double num = (double)look.Normalize();
				this.m_LocustNest.Maneuvering.PostAddGoal(targetPos, look);
				if (this.m_LocustNest.Target == this.m_Target)
					return;
				this.m_LocustNest.SetShipTarget(this.m_Target.ObjectID, Vector3.Zero, true, 0);
			}
		}

		private void ThinkEmitFighter()
		{
			this.SpawnMaxFighters();
		}

		private void ThinkIntegrateFighter()
		{
			if (this.m_LoadingFighters.Count == 0)
			{
				this.m_State = LocustNestStates.SEEK;
			}
			else
			{
				bool flag = true;
				foreach (GameObject loadingFighter in this.m_LoadingFighters)
				{
					if (loadingFighter.ObjectStatus != GameObjectStatus.Ready)
						flag = false;
				}
				if (!flag)
					return;
				foreach (Ship loadingFighter in this.m_LoadingFighters)
				{
					loadingFighter.Player = this.m_LocustNest.Player;
					loadingFighter.Active = true;
					this.m_Game.CurrentState.AddGameObject((IGameObject)loadingFighter, false);
				}
				this.m_State = LocustNestStates.SEEK;
			}
		}

		private void ThinkWaitForDocked()
		{
			bool flag = true;
			foreach (Ship launchingFighter in this.m_LaunchingFighters)
			{
				if (!launchingFighter.DockedWithParent)
				{
					flag = false;
					break;
				}
			}
			if (!flag)
				return;
			this.m_State = LocustNestStates.LAUNCHFIGHTER;
		}

		private void ThinkLaunch()
		{
			if (this.m_LaunchingFighters.Count == 0)
			{
				this.m_State = LocustNestStates.SEEK;
			}
			else
			{
				this.m_LocustNest.PostSetProp("LaunchBattleriders");
				this.m_State = LocustNestStates.WAITFORLAUNCH;
			}
		}

		private void ThinkWaitForLaunch()
		{
			List<Ship> shipList = new List<Ship>();
			foreach (Ship launchingFighter in this.m_LaunchingFighters)
			{
				if (!launchingFighter.DockedWithParent)
					shipList.Add(launchingFighter);
			}
			foreach (Ship ship in shipList)
				this.m_LaunchingFighters.Remove(ship);
			if (this.m_LaunchingFighters.Count != 0)
				return;
			if (this.m_LoadingFighters.Count + this.m_SpawnedFighters.Count < this.m_MaxNumFighters)
				this.m_State = LocustNestStates.EMITFIGHTER;
			else
				this.m_State = LocustNestStates.SEEK;
		}

		public void RequestTargetFromParent(LocustFighterControl lfc)
		{
			if (this.m_TargetList.Count == 0 && !(this.m_Target is StellarBody))
			{
				lfc.SetTarget((IGameObject)this.m_LocustNest);
			}
			else
			{
				LocustTarget locustTarget = (LocustTarget)null;
				float num1 = float.MaxValue;
				foreach (LocustTarget target in this.m_TargetList)
				{
					int num2 = LocustFighterControl.NumFightersPerShip(target.Target.ShipClass);
					if (target.FightersOnTarget < num2)
					{
						float lengthSquared = (target.Target.Position - lfc.GetShip().Position).LengthSquared;
						if ((double)lengthSquared < (double)num1)
						{
							num1 = lengthSquared;
							locustTarget = target;
						}
					}
				}
				if (locustTarget != null)
				{
					locustTarget.IncFightersOnTarget();
					lfc.SetTarget((IGameObject)locustTarget.Target);
				}
				else if (this.m_NumLocustsReachedPlanet > 0 && this.m_Target is StellarBody)
					lfc.SetTarget(this.m_Target);
				else
					lfc.SetTarget((IGameObject)this.m_LocustNest);
			}
		}

		public void NotifyFighterHasLanded()
		{
			--this.m_NumLocustsReachedPlanet;
			if (this.m_NumLocustsReachedPlanet > 0)
				return;
			foreach (LocustFighterControl spawnedFighter in this.m_SpawnedFighters)
				spawnedFighter.ClearPlanetTarget();
		}

		private void SpawnMaxFighters()
		{
			if (this.m_Game.Game.ScriptModules.Locust == null || this.m_MaxNumFightersTotal <= 0)
				return;
			int num = Math.Min(Math.Max(Math.Min(this.m_MaxNumFighters, this.m_MaxNumFightersTotal) - (this.m_LoadingFighters.Count + this.m_SpawnedFighters.Count), 0), Math.Max(this.m_LocustNest.BattleRiderMounts.Count<BattleRiderMount>() - this.m_LoadingFighters.Count, 0));
			Matrix rotationYpr = Matrix.CreateRotationYPR(this.m_LocustNest.Maneuvering.Rotation);
			rotationYpr.Position = this.m_LocustNest.Maneuvering.Position;
			int needleShipDesignId = this.m_Game.Game.ScriptModules.Locust.NeedleShipDesignId;
			for (int index = 0; index < num; ++index)
			{
				Ship newShip = CombatAIController.CreateNewShip(this.m_Game.Game, rotationYpr, needleShipDesignId, this.m_LocustNest.ObjectID, this.m_LocustNest.InputID, this.m_LocustNest.Player.ObjectID);
				if (newShip != null)
				{
					this.m_LoadingFighters.Add(newShip);
					--this.m_MaxNumFightersTotal;
				}
			}
			if (this.m_LoadingFighters.Count <= 0)
				return;
			this.m_State = LocustNestStates.INTEGRATEFIGHTER;
		}

		protected void UpdateTargetList()
		{
			if (this.m_UpdateTargetList)
				return;
			--this.m_UpdateListDelay;
			if (this.m_UpdateListDelay > 0)
				return;
			this.m_UpdateListDelay = 30;
			this.m_UpdateTargetList = true;
		}

		private bool FighterRequestingTarget()
		{
			bool flag = false;
			foreach (CombatAIController spawnedFighter in this.m_SpawnedFighters)
			{
				if (spawnedFighter.RequestingNewTarget())
				{
					flag = true;
					break;
				}
			}
			return flag;
		}
	}
}
