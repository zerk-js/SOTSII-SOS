// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.SwarmerSpawnerControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Encounters;
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
	internal class SwarmerSpawnerControl : CombatAIController
	{
		protected App m_Game;
		protected Ship m_SwarmerSpawner;
		protected List<Ship> m_LoadingSwarmers;
		protected List<Ship> m_LoadingGuardians;
		protected List<Ship> m_LaunchingShips;
		protected List<int> m_AttackingPlanetSwarmers;
		protected List<int> m_AttackingPlanetGardians;
		protected List<SwarmerAttackerControl> m_SpawnedSwarmers;
		protected List<SwarmerAttackerControl> m_SpawnedGuardians;
		protected List<SwarmerTarget> m_TargetList;
		protected SwarmerSpawnerStates m_State;
		protected int m_MaxSwarmers;
		protected int m_MaxGuardians;
		protected int m_UpdateListDelay;
		protected int m_MaxToCreate;
		protected int m_NumSwarmersToAttackPlanet;
		protected int m_NumGardiansToAttackPlanet;
		protected bool m_UpdateTargetList;
		private bool m_IsDeepSpace;

		public override Ship GetShip()
		{
			return this.m_SwarmerSpawner;
		}

		public override void SetTarget(IGameObject target)
		{
		}

		public override IGameObject GetTarget()
		{
			return (IGameObject)null;
		}

		public SwarmerSpawnerStates State
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

		public SwarmerSpawnerControl(App game, Ship ship, int systemId)
		{
			this.m_Game = game;
			this.m_SwarmerSpawner = ship;
			StarSystemInfo starSystemInfo = game.GameDatabase.GetStarSystemInfo(systemId);
			this.m_IsDeepSpace = starSystemInfo != (StarSystemInfo)null && starSystemInfo.IsDeepSpace;
		}

		public override void Initialize()
		{
			this.m_LoadingSwarmers = new List<Ship>();
			this.m_LoadingGuardians = new List<Ship>();
			this.m_LaunchingShips = new List<Ship>();
			this.m_AttackingPlanetSwarmers = new List<int>();
			this.m_AttackingPlanetGardians = new List<int>();
			this.m_SpawnedSwarmers = new List<SwarmerAttackerControl>();
			this.m_SpawnedGuardians = new List<SwarmerAttackerControl>();
			this.m_TargetList = new List<SwarmerTarget>();
			this.m_State = SwarmerSpawnerStates.IDLE;
			this.m_UpdateTargetList = false;
			this.m_MaxSwarmers = 30;
			this.m_MaxGuardians = 5;
			this.m_NumSwarmersToAttackPlanet = 0;
			this.m_NumGardiansToAttackPlanet = 0;
			if (this.m_Game.Game.ScriptModules != null && this.m_Game.Game.ScriptModules.Swarmers != null)
			{
				if (this.m_SwarmerSpawner.DesignID == this.m_Game.Game.ScriptModules.Swarmers.SwarmQueenDesignID)
				{
					this.m_MaxSwarmers = this.m_Game.AssetDatabase.GlobalSwarmerData.NumQueenSwarmers;
					this.m_MaxGuardians = this.m_Game.AssetDatabase.GlobalSwarmerData.NumQueenGuardians;
					this.m_NumSwarmersToAttackPlanet = this.m_MaxSwarmers / 2;
					this.m_NumGardiansToAttackPlanet = this.m_MaxGuardians / 2;
				}
				else
				{
					this.m_MaxSwarmers = this.m_Game.AssetDatabase.GlobalSwarmerData.NumHiveSwarmers;
					this.m_MaxGuardians = this.m_Game.AssetDatabase.GlobalSwarmerData.NumHiveGuardians;
				}
			}
			this.m_MaxToCreate = this.m_MaxSwarmers + this.m_MaxGuardians;
			this.m_UpdateListDelay = 0;
			this.m_SwarmerSpawner.PostSetProp("SetAsOnlyLaunchCarrier", true);
		}

		private void SpawnMaxSwarmerAttackers(Swarmers swarmers)
		{
			if (this.m_MaxToCreate <= 0)
				return;
			Matrix rotationYpr = Matrix.CreateRotationYPR(this.m_SwarmerSpawner.Maneuvering.Rotation);
			rotationYpr.Position = this.m_SwarmerSpawner.Maneuvering.Position;
			int num1 = Math.Max(this.m_SwarmerSpawner.BattleRiderMounts.Count<BattleRiderMount>() - (this.m_LoadingSwarmers.Count + this.m_LoadingGuardians.Count + this.m_LaunchingShips.Count), 0);
			if (num1 == 0)
				return;
			int val2_1 = Math.Max(this.m_MaxSwarmers - (this.m_LoadingSwarmers.Count + this.m_SpawnedSwarmers.Count), 0);
			int val2_2 = Math.Max(this.m_MaxGuardians - (this.m_LoadingGuardians.Count + this.m_SpawnedGuardians.Count), 0);
			if (val2_1 + val2_2 == 0)
				return;
			int num2 = Math.Min(num1 / 2, val2_1);
			int num3 = Math.Min(num1 - num2, val2_2);
			int num4 = Math.Min(num2 + Math.Max(num1 - (num2 + num3), 0), val2_1);
			int num5 = num4 + num3;
			for (int index = 0; index < num5; ++index)
			{
				int designId;
				if (num4 > 0 && (index % 2 == 0 || num3 == 0))
				{
					designId = swarmers.SwarmerDesignID;
					--num4;
				}
				else if (num3 > 0)
				{
					designId = swarmers.GuardianDesignID;
					--num3;
				}
				else
					break;
				Ship newShip = CombatAIController.CreateNewShip(this.m_Game.Game, rotationYpr, designId, this.m_SwarmerSpawner.ObjectID, this.m_SwarmerSpawner.InputID, this.m_SwarmerSpawner.Player.ObjectID);
				if (newShip != null)
				{
					if (designId == swarmers.GuardianDesignID)
						this.m_LoadingGuardians.Add(newShip);
					else
						this.m_LoadingSwarmers.Add(newShip);
					--this.m_MaxToCreate;
				}
				if (this.m_MaxToCreate <= 0)
					break;
			}
			if (this.m_LoadingSwarmers.Count <= 0 && this.m_LoadingGuardians.Count <= 0)
				return;
			this.m_State = SwarmerSpawnerStates.INTEGRATESWARMER;
		}

		public override void Terminate()
		{
		}

		public override void ObjectRemoved(IGameObject obj)
		{
			foreach (SwarmerAttackerControl spawnedSwarmer in this.m_SpawnedSwarmers)
			{
				if (spawnedSwarmer.GetShip() == obj)
				{
					this.m_SpawnedSwarmers.Remove(spawnedSwarmer);
					break;
				}
			}
			foreach (SwarmerAttackerControl spawnedGuardian in this.m_SpawnedGuardians)
			{
				if (spawnedGuardian.GetShip() == obj)
				{
					this.m_SpawnedGuardians.Remove(spawnedGuardian);
					break;
				}
			}
			foreach (Ship loadingSwarmer in this.m_LoadingSwarmers)
			{
				if (loadingSwarmer == obj)
				{
					this.m_LoadingSwarmers.Remove(loadingSwarmer);
					break;
				}
			}
			foreach (Ship loadingGuardian in this.m_LoadingGuardians)
			{
				if (loadingGuardian == obj)
				{
					this.m_LoadingGuardians.Remove(loadingGuardian);
					break;
				}
			}
			foreach (SwarmerTarget target in this.m_TargetList)
			{
				if (target.Target == obj)
				{
					this.m_TargetList.Remove(target);
					break;
				}
			}
			foreach (Ship launchingShip in this.m_LaunchingShips)
			{
				if (launchingShip == obj)
				{
					this.m_LaunchingShips.Remove(launchingShip);
					break;
				}
			}
		}

		public void AddChild(CombatAIController child)
		{
			if (!(child is SwarmerAttackerControl))
				return;
			foreach (Ship loadingSwarmer in this.m_LoadingSwarmers)
			{
				if (loadingSwarmer == child.GetShip())
				{
					this.m_LoadingSwarmers.Remove(loadingSwarmer);
					this.m_LaunchingShips.Add(loadingSwarmer);
					this.m_SpawnedSwarmers.Add(child as SwarmerAttackerControl);
					break;
				}
			}
			foreach (Ship loadingGuardian in this.m_LoadingGuardians)
			{
				if (loadingGuardian == child.GetShip())
				{
					this.m_LoadingGuardians.Remove(loadingGuardian);
					this.m_LaunchingShips.Add(loadingGuardian);
					this.m_SpawnedGuardians.Add(child as SwarmerAttackerControl);
					break;
				}
			}
		}

		public bool IsThisMyParent(Ship ship)
		{
			if (!this.m_LoadingSwarmers.Any<Ship>((Func<Ship, bool>)(x => x == ship)))
				return this.m_LoadingGuardians.Any<Ship>((Func<Ship, bool>)(x => x == ship));
			return true;
		}

		public override void OnThink()
		{
			if (this.m_SwarmerSpawner == null)
				return;
			this.UpdateTargetList();
			if (this.m_TargetList.Count > 0)
				this.MaintainMaxSwarmers();
			switch (this.m_State)
			{
				case SwarmerSpawnerStates.IDLE:
					this.ThinkIdle();
					break;
				case SwarmerSpawnerStates.EMITSWARMER:
					this.ThinkEmitAttackSwarmer();
					break;
				case SwarmerSpawnerStates.INTEGRATESWARMER:
					this.ThinkIntegrateAttackSwarmer();
					break;
				case SwarmerSpawnerStates.ADDINGSWARMERS:
					this.ThinkAddingSwarmers();
					break;
				case SwarmerSpawnerStates.LAUNCHSWARMER:
					this.ThinkLaunch();
					break;
				case SwarmerSpawnerStates.WAITFORLAUNCH:
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
			List<SwarmerTarget> source = new List<SwarmerTarget>();
			List<SwarmerTarget> swarmerTargetList = new List<SwarmerTarget>();
			foreach (SwarmerTarget target in this.m_TargetList)
				target.ClearNumTargets();
			source.AddRange((IEnumerable<SwarmerTarget>)this.m_TargetList);
			foreach (IGameObject gameObject in objs)
			{
				if (gameObject is Ship)
				{
					Ship ship = gameObject as Ship;
					if (Ship.IsActiveShip(ship) && (this.m_IsDeepSpace || ship.IsDetected(this.m_SwarmerSpawner.Player)) && ship.Player != this.m_SwarmerSpawner.Player)
					{
						swarmerTargetList.Add(new SwarmerTarget()
						{
							Target = (IGameObject)ship
						});
						SwarmerTarget swarmerTarget = source.FirstOrDefault<SwarmerTarget>((Func<SwarmerTarget, bool>)(x => x.Target == ship));
						if (swarmerTarget != null)
							source.Remove(swarmerTarget);
					}
				}
				else if (gameObject is StellarBody)
				{
					StellarBody planet = gameObject as StellarBody;
					if (planet.Population > 0.0)
					{
						swarmerTargetList.Add(new SwarmerTarget()
						{
							Target = (IGameObject)planet
						});
						SwarmerTarget swarmerTarget = source.FirstOrDefault<SwarmerTarget>((Func<SwarmerTarget, bool>)(x => x.Target == planet));
						if (swarmerTarget != null)
							source.Remove(swarmerTarget);
					}
				}
			}
			if (source.Count > 0)
			{
				foreach (SwarmerAttackerControl spawnedSwarmer in this.m_SpawnedSwarmers)
				{
					SwarmerAttackerControl swarmer = spawnedSwarmer;
					if (source.Any<SwarmerTarget>((Func<SwarmerTarget, bool>)(x => x.Target == swarmer.GetTarget())))
						swarmer.SetTarget((IGameObject)null);
				}
				foreach (SwarmerAttackerControl spawnedGuardian in this.m_SpawnedGuardians)
				{
					SwarmerAttackerControl guardian = spawnedGuardian;
					if (source.Any<SwarmerTarget>((Func<SwarmerTarget, bool>)(x => x.Target == guardian.GetTarget())))
						guardian.SetTarget((IGameObject)null);
				}
			}
			if (swarmerTargetList.Count > this.m_TargetList.Count)
			{
				foreach (SwarmerTarget target in this.m_TargetList)
				{
					target.ClearNumTargets();
					int num1 = 0;
					int num2 = 0;
					if (target.Target is Ship)
					{
						ShipClass shipClass = (target.Target as Ship).ShipClass;
						num1 = SwarmerAttackerControl.NumSwarmersPerShip(shipClass);
						num2 = SwarmerAttackerControl.NumGuardiansPerShip(shipClass);
					}
					else if (target.Target is StellarBody)
					{
						num1 = this.m_NumSwarmersToAttackPlanet;
						num2 = this.m_NumGardiansToAttackPlanet;
					}
					foreach (SwarmerAttackerControl spawnedSwarmer in this.m_SpawnedSwarmers)
					{
						if (spawnedSwarmer.GetTarget() == target.Target)
						{
							if (num1 > 0)
							{
								--num1;
								target.IncSwarmersOnTarget();
							}
							else
								spawnedSwarmer.SetTarget((IGameObject)null);
						}
					}
					foreach (SwarmerAttackerControl spawnedGuardian in this.m_SpawnedGuardians)
					{
						if (spawnedGuardian.GetTarget() == target.Target)
						{
							if (num2 > 0)
							{
								--num2;
								target.IncGuardiansOnTarget();
							}
							else
								spawnedGuardian.SetTarget((IGameObject)null);
						}
					}
				}
			}
			else
			{
				foreach (SwarmerAttackerControl spawnedSwarmer in this.m_SpawnedSwarmers)
				{
					SwarmerAttackerControl swarmer = spawnedSwarmer;
					if (swarmer.GetTarget() != null)
						this.m_TargetList.FirstOrDefault<SwarmerTarget>((Func<SwarmerTarget, bool>)(x => x.Target == swarmer.GetTarget()))?.IncSwarmersOnTarget();
				}
				foreach (SwarmerAttackerControl spawnedGuardian in this.m_SpawnedGuardians)
				{
					SwarmerAttackerControl guardian = spawnedGuardian;
					if (guardian.GetTarget() != null)
						this.m_TargetList.FirstOrDefault<SwarmerTarget>((Func<SwarmerTarget, bool>)(x => x.Target == guardian.GetTarget()))?.IncGuardiansOnTarget();
				}
			}
			foreach (SwarmerTarget swarmerTarget in swarmerTargetList)
			{
				SwarmerTarget target = swarmerTarget;
				if (!this.m_TargetList.Any<SwarmerTarget>((Func<SwarmerTarget, bool>)(x => x.Target == target.Target)))
				{
					target.ClearNumTargets();
					this.m_TargetList.Add(target);
				}
			}
			this.m_UpdateTargetList = false;
		}

		public void RequestTargetFromParent(SwarmerAttackerControl sac)
		{
			if (this.m_TargetList.Count == 0)
				return;
			SwarmerTarget swarmerTarget1 = (SwarmerTarget)null;
			SwarmerTarget swarmerTarget2 = (SwarmerTarget)null;
			SwarmerTarget swarmerTarget3 = (SwarmerTarget)null;
			float num1 = float.MaxValue;
			float num2 = float.MaxValue;
			float num3 = float.MaxValue;
			foreach (SwarmerTarget target1 in this.m_TargetList)
			{
				int num4 = 0;
				int num5 = 0;
				float num6 = 0.0f;
				if (target1.Target is Ship)
				{
					Ship target2 = target1.Target as Ship;
					ShipClass shipClass = target2.ShipClass;
					num4 = SwarmerAttackerControl.NumSwarmersPerShip(shipClass) - target1.SwarmersOnTarget;
					num5 = SwarmerAttackerControl.NumGuardiansPerShip(shipClass) - target1.GuardiansOnTarget;
					num6 = (target2.Position - sac.GetShip().Position).LengthSquared;
					if ((double)num6 < (double)num3)
					{
						num3 = num6;
						swarmerTarget3 = target1;
					}
				}
				else if (target1.Target is StellarBody)
				{
					StellarBody target2 = target1.Target as StellarBody;
					int num7 = this.m_NumSwarmersToAttackPlanet - this.m_AttackingPlanetSwarmers.Count;
					int num8 = this.m_NumGardiansToAttackPlanet - this.m_AttackingPlanetGardians.Count;
					float lengthSquared = (target2.Parameters.Position - sac.GetShip().Position).LengthSquared;
					if ((double)lengthSquared < (double)num2)
					{
						num2 = lengthSquared;
						swarmerTarget2 = target1;
						continue;
					}
					continue;
				}
				if (sac.Type == SwarmerAttackerType.GAURDIAN)
				{
					if (num5 <= 0)
						continue;
				}
				else if (num4 <= 0)
					continue;
				if ((double)num6 < (double)num1)
				{
					num1 = num6;
					swarmerTarget1 = target1;
				}
			}
			if (swarmerTarget2 != null)
			{
				if (this.m_AttackingPlanetSwarmers.Contains(sac.GetShip().ObjectID) || this.m_AttackingPlanetGardians.Contains(sac.GetShip().ObjectID))
					swarmerTarget1 = swarmerTarget2;
				else if ((sac.Type == SwarmerAttackerType.GAURDIAN ? this.m_NumGardiansToAttackPlanet - this.m_AttackingPlanetGardians.Count : this.m_NumSwarmersToAttackPlanet - this.m_AttackingPlanetSwarmers.Count) > 0)
					swarmerTarget1 = swarmerTarget2;
			}
			if (swarmerTarget1 != null || swarmerTarget3 != null)
			{
				if (sac.Type == SwarmerAttackerType.GAURDIAN)
				{
					if (swarmerTarget1 != null)
						swarmerTarget1.IncGuardiansOnTarget();
					else
						swarmerTarget3.IncGuardiansOnTarget();
				}
				else if (swarmerTarget1 != null)
					swarmerTarget1.IncSwarmersOnTarget();
				else
					swarmerTarget3.IncSwarmersOnTarget();
				if (swarmerTarget1 != null)
				{
					sac.SetTarget(swarmerTarget1.Target);
					if (!(swarmerTarget1.Target is StellarBody) || this.m_AttackingPlanetSwarmers.Contains(sac.GetShip().ObjectID) || this.m_AttackingPlanetGardians.Contains(sac.GetShip().ObjectID))
						return;
					if (sac.Type == SwarmerAttackerType.GAURDIAN)
						this.m_AttackingPlanetGardians.Add(sac.GetShip().ObjectID);
					else
						this.m_AttackingPlanetSwarmers.Add(sac.GetShip().ObjectID);
				}
				else
					sac.SetTarget(swarmerTarget3.Target);
			}
			else
				sac.SetTarget((IGameObject)null);
		}

		protected void MaintainMaxSwarmers()
		{
			if (!this.ReadyToEmitFighters() || this.m_MaxToCreate <= 0 || this.m_LoadingSwarmers.Count + this.m_LoadingGuardians.Count + (this.m_SpawnedSwarmers.Count + this.m_SpawnedGuardians.Count) >= this.m_MaxSwarmers + this.m_MaxGuardians)
				return;
			this.m_State = SwarmerSpawnerStates.EMITSWARMER;
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

		public override bool NeedsAParent()
		{
			return false;
		}

		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
		}

		protected virtual void ThinkIdle()
		{
		}

		protected virtual void ThinkEmitAttackSwarmer()
		{
			if (this.m_MaxToCreate > 0 && this.m_Game.Game.ScriptModules.Swarmers != null)
				this.SpawnMaxSwarmerAttackers(this.m_Game.Game.ScriptModules.Swarmers);
			else
				this.m_State = SwarmerSpawnerStates.IDLE;
		}

		protected virtual void ThinkIntegrateAttackSwarmer()
		{
			if (this.m_LoadingSwarmers.Count + this.m_LoadingGuardians.Count == 0)
			{
				this.m_State = SwarmerSpawnerStates.IDLE;
			}
			else
			{
				bool flag = true;
				foreach (GameObject loadingSwarmer in this.m_LoadingSwarmers)
				{
					if (loadingSwarmer.ObjectStatus != GameObjectStatus.Ready)
						flag = false;
				}
				foreach (GameObject loadingGuardian in this.m_LoadingGuardians)
				{
					if (loadingGuardian.ObjectStatus != GameObjectStatus.Ready)
						flag = false;
				}
				if (!flag)
					return;
				foreach (Ship loadingSwarmer in this.m_LoadingSwarmers)
				{
					loadingSwarmer.Player = this.m_SwarmerSpawner.Player;
					loadingSwarmer.Active = true;
					this.m_Game.CurrentState.AddGameObject((IGameObject)loadingSwarmer, false);
				}
				foreach (Ship loadingGuardian in this.m_LoadingGuardians)
				{
					loadingGuardian.Player = this.m_SwarmerSpawner.Player;
					loadingGuardian.Active = true;
					this.m_Game.CurrentState.AddGameObject((IGameObject)loadingGuardian, false);
				}
				this.m_State = SwarmerSpawnerStates.ADDINGSWARMERS;
			}
		}

		protected virtual void ThinkAddingSwarmers()
		{
			if (this.m_LoadingSwarmers.Count + this.m_LoadingGuardians.Count != 0)
				return;
			if (this.m_TargetList.Count == 0)
				this.m_State = SwarmerSpawnerStates.IDLE;
			else
				this.m_State = SwarmerSpawnerStates.LAUNCHSWARMER;
		}

		protected virtual void ThinkLaunch()
		{
			if (this.m_LaunchingShips.Count == 0)
			{
				this.m_State = SwarmerSpawnerStates.IDLE;
			}
			else
			{
				bool flag = true;
				foreach (Ship launchingShip in this.m_LaunchingShips)
				{
					if (!launchingShip.DockedWithParent)
					{
						flag = false;
						break;
					}
				}
				if (!flag)
					return;
				this.m_SwarmerSpawner.PostSetProp("LaunchBattleriders");
				this.m_State = SwarmerSpawnerStates.WAITFORLAUNCH;
			}
		}

		protected virtual void ThinkWaitForLaunch()
		{
			List<Ship> shipList = new List<Ship>();
			foreach (Ship launchingShip in this.m_LaunchingShips)
			{
				if (!launchingShip.DockedWithParent)
					shipList.Add(launchingShip);
			}
			foreach (Ship ship in shipList)
				this.m_LaunchingShips.Remove(ship);
			if (this.m_LaunchingShips.Count != 0)
				return;
			this.m_State = SwarmerSpawnerStates.IDLE;
		}

		public bool ReadyToEmitFighters()
		{
			switch (this.m_State)
			{
				case SwarmerSpawnerStates.EMITSWARMER:
				case SwarmerSpawnerStates.INTEGRATESWARMER:
				case SwarmerSpawnerStates.ADDINGSWARMERS:
				case SwarmerSpawnerStates.LAUNCHSWARMER:
				case SwarmerSpawnerStates.WAITFORLAUNCH:
					return false;
				default:
					return true;
			}
		}
	}
}
