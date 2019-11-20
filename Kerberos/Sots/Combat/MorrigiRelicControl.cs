// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.MorrigiRelicControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal class MorrigiRelicControl : CombatAIController
	{
		private App m_Game;
		private Ship m_MorrigiRelic;
		private List<ShipInfo> m_AvailableCrows;
		private List<Ship> m_LoadingCrows;
		private List<Ship> m_LaunchingCrows;
		private List<MorrigiCrowControl> m_SpawnedCrows;
		private IGameObject m_Target;
		private MorrigiRelicStates m_State;
		private int m_FleetId;
		private bool m_IsAggressive;

		public static MorrigiRelicGlobalData.RelicType GetMorrigiRelicTypeFromName(
		  string sectionName)
		{
			MorrigiRelicGlobalData.RelicType relicType = MorrigiRelicGlobalData.RelicType.Pristine1;
			int num = 0;
			if (sectionName.Contains("levelone"))
				num = 1;
			if (sectionName.Contains("leveltwo"))
				num = 2;
			if (sectionName.Contains("levelthree"))
				num = 3;
			if (sectionName.Contains("levelfour"))
				num = 4;
			if (sectionName.Contains("levelfive"))
				num = 5;
			if (sectionName.Contains("pristine"))
			{
				switch (num)
				{
					case 1:
						relicType = MorrigiRelicGlobalData.RelicType.Pristine1;
						break;
					case 2:
						relicType = MorrigiRelicGlobalData.RelicType.Pristine2;
						break;
					case 3:
						relicType = MorrigiRelicGlobalData.RelicType.Pristine3;
						break;
					case 4:
						relicType = MorrigiRelicGlobalData.RelicType.Pristine4;
						break;
					case 5:
						relicType = MorrigiRelicGlobalData.RelicType.Pristine5;
						break;
				}
			}
			else if (sectionName.Contains("stealth"))
			{
				switch (num)
				{
					case 1:
						relicType = MorrigiRelicGlobalData.RelicType.Stealth1;
						break;
					case 2:
						relicType = MorrigiRelicGlobalData.RelicType.Stealth2;
						break;
					case 3:
						relicType = MorrigiRelicGlobalData.RelicType.Stealth3;
						break;
					case 4:
						relicType = MorrigiRelicGlobalData.RelicType.Stealth4;
						break;
					case 5:
						relicType = MorrigiRelicGlobalData.RelicType.Stealth5;
						break;
				}
			}
			return relicType;
		}

		public override Ship GetShip()
		{
			return this.m_MorrigiRelic;
		}

		public override void SetTarget(IGameObject target)
		{
			this.m_Target = target;
		}

		public override IGameObject GetTarget()
		{
			return this.m_Target;
		}

		public MorrigiRelicStates State
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

		public MorrigiRelicControl(App game, Ship ship, int fleetId)
		{
			this.m_Game = game;
			this.m_MorrigiRelic = ship;
			this.m_FleetId = fleetId;
		}

		public override void Initialize()
		{
			this.m_LoadingCrows = new List<Ship>();
			this.m_LaunchingCrows = new List<Ship>();
			this.m_SpawnedCrows = new List<MorrigiCrowControl>();
			this.m_State = MorrigiRelicStates.IDLE;
			this.m_Target = (IGameObject)null;
			this.m_AvailableCrows = this.m_Game.GameDatabase.GetBattleRidersByParentID(this.m_MorrigiRelic.DatabaseID).ToList<ShipInfo>();
			this.m_MorrigiRelic.PostSetProp("SetAsOnlyLaunchCarrier", true);
			this.m_IsAggressive = false;
			MorrigiRelicInfo morrigiRelicInfo = this.m_Game.GameDatabase.GetMorrigiRelicInfos().ToList<MorrigiRelicInfo>().FirstOrDefault<MorrigiRelicInfo>((Func<MorrigiRelicInfo, bool>)(x => x.FleetId == this.m_FleetId));
			if (morrigiRelicInfo != null)
				this.m_IsAggressive = morrigiRelicInfo.IsAggressive;
			if (!this.m_IsAggressive)
				return;
			this.SpawnMaxCrows();
		}

		public override void Terminate()
		{
		}

		public override void ObjectRemoved(IGameObject obj)
		{
			foreach (MorrigiCrowControl spawnedCrow in this.m_SpawnedCrows)
			{
				if (spawnedCrow.GetShip() == obj)
				{
					this.m_SpawnedCrows.Remove(spawnedCrow);
					break;
				}
			}
			foreach (Ship loadingCrow in this.m_LoadingCrows)
			{
				if (loadingCrow == obj)
				{
					this.m_LoadingCrows.Remove(loadingCrow);
					break;
				}
			}
			if (this.m_Target != obj)
				return;
			this.m_Target = (IGameObject)null;
		}

		public override void OnThink()
		{
			if (this.m_MorrigiRelic == null)
				return;
			switch (this.m_State)
			{
				case MorrigiRelicStates.IDLE:
					this.ThinkIdle();
					break;
				case MorrigiRelicStates.EMITCROW:
					this.ThinkEmitCrow();
					break;
				case MorrigiRelicStates.INTEGRATECROW:
					this.ThinkIntegrateCrow();
					break;
				case MorrigiRelicStates.WAITFORDOCKED:
					this.ThinkWaitForDocked();
					break;
				case MorrigiRelicStates.LAUNCHCROW:
					this.ThinkLaunch();
					break;
				case MorrigiRelicStates.WAITFORLAUNCH:
					this.ThinkWaitForLaunch();
					break;
			}
		}

		public override void ForceFlee()
		{
		}

		public override bool VictoryConditionIsMet()
		{
			if (this.m_IsAggressive && this.m_AvailableCrows.Count == 0 && this.m_LaunchingCrows.Count == 0)
				return this.m_LoadingCrows.Count == 0;
			return false;
		}

		public override bool RequestingNewTarget()
		{
			bool flag = false;
			foreach (CombatAIController spawnedCrow in this.m_SpawnedCrows)
			{
				if (spawnedCrow.RequestingNewTarget())
				{
					flag = true;
					break;
				}
			}
			return flag;
		}

		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			List<MorrigiRelicControl.TargetingData> source = new List<MorrigiRelicControl.TargetingData>();
			foreach (IGameObject gameObject in objs)
			{
				if (gameObject is Ship)
				{
					Ship ship = gameObject as Ship;
					if (ship.Active && Ship.IsActiveShip(ship) && ship.Player != this.m_MorrigiRelic.Player)
					{
						bool flag = ship.IsDetected(this.m_MorrigiRelic.Player);
						int num = 0;
						foreach (MorrigiCrowControl spawnedCrow in this.m_SpawnedCrows)
						{
							if (ship == spawnedCrow.GetTarget())
							{
								if (!flag)
									spawnedCrow.SetTarget((IGameObject)null);
								else
									++num;
							}
						}
						if (flag)
							source.Add(new MorrigiRelicControl.TargetingData()
							{
								Target = ship,
								NumTargeting = num
							});
					}
				}
			}
			foreach (MorrigiCrowControl spawnedCrow in this.m_SpawnedCrows)
			{
				if (spawnedCrow.RequestingNewTarget())
				{
					Vector3 position = spawnedCrow.GetShip().Position;
					float num1 = float.MaxValue;
					int num2 = -1;
					Ship closestTarg = (Ship)null;
					foreach (MorrigiRelicControl.TargetingData targetingData in source)
					{
						float lengthSquared = (targetingData.Target.Position - position).LengthSquared;
						if (num2 < 0 || targetingData.NumTargeting < num2 || num2 == targetingData.NumTargeting && (double)lengthSquared < (double)num1)
						{
							closestTarg = targetingData.Target;
							num1 = lengthSquared;
							num2 = targetingData.NumTargeting;
						}
					}
					if (closestTarg != null)
					{
						spawnedCrow.SetTarget((IGameObject)closestTarg);
						MorrigiRelicControl.TargetingData targetingData = source.FirstOrDefault<MorrigiRelicControl.TargetingData>((Func<MorrigiRelicControl.TargetingData, bool>)(x => x.Target == closestTarg));
						if (targetingData != null)
							++targetingData.NumTargeting;
					}
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

		public bool IsThisMyRelic(Ship ship)
		{
			return this.m_LoadingCrows.Any<Ship>((Func<Ship, bool>)(x => x == ship));
		}

		public void AddCrow(CombatAIController crow)
		{
			if (!(crow is MorrigiCrowControl))
				return;
			foreach (Ship loadingCrow in this.m_LoadingCrows)
			{
				if (loadingCrow == crow.GetShip())
				{
					this.m_LoadingCrows.Remove(loadingCrow);
					this.m_LaunchingCrows.Add(loadingCrow);
					break;
				}
			}
			this.m_SpawnedCrows.Add(crow as MorrigiCrowControl);
		}

		private void ThinkIdle()
		{
			if (this.m_LoadingCrows.Count + this.m_SpawnedCrows.Count >= this.m_AvailableCrows.Count)
				return;
			this.m_State = MorrigiRelicStates.EMITCROW;
		}

		private void ThinkEmitCrow()
		{
			this.SpawnMaxCrows();
		}

		private void ThinkIntegrateCrow()
		{
			if (this.m_LoadingCrows.Count <= 0)
				return;
			bool flag = true;
			foreach (GameObject loadingCrow in this.m_LoadingCrows)
			{
				if (loadingCrow.ObjectStatus != GameObjectStatus.Ready)
					flag = false;
			}
			if (!flag)
				return;
			foreach (Ship loadingCrow in this.m_LoadingCrows)
			{
				loadingCrow.Player = this.m_MorrigiRelic.Player;
				loadingCrow.Active = true;
				this.m_Game.CurrentState.AddGameObject((IGameObject)loadingCrow, false);
			}
			this.m_State = MorrigiRelicStates.WAITFORDOCKED;
		}

		private void ThinkWaitForDocked()
		{
			if (this.m_LoadingCrows.Count > 0)
				return;
			bool flag = true;
			foreach (Ship launchingCrow in this.m_LaunchingCrows)
			{
				if (!launchingCrow.DockedWithParent)
				{
					flag = false;
					break;
				}
			}
			if (!flag)
				return;
			this.m_State = MorrigiRelicStates.LAUNCHCROW;
		}

		private void ThinkLaunch()
		{
			if (this.m_LaunchingCrows.Count == 0)
			{
				this.m_State = MorrigiRelicStates.IDLE;
			}
			else
			{
				this.m_MorrigiRelic.PostSetProp("LaunchBattleriders");
				this.m_State = MorrigiRelicStates.WAITFORLAUNCH;
			}
		}

		private void ThinkWaitForLaunch()
		{
			List<Ship> shipList = new List<Ship>();
			foreach (Ship launchingCrow in this.m_LaunchingCrows)
			{
				if (!launchingCrow.DockedWithParent)
					shipList.Add(launchingCrow);
			}
			foreach (Ship ship in shipList)
				this.m_LaunchingCrows.Remove(ship);
			if (this.m_LaunchingCrows.Count != 0)
				return;
			if (this.m_LoadingCrows.Count + this.m_SpawnedCrows.Count < this.m_AvailableCrows.Count)
				this.m_State = MorrigiRelicStates.EMITCROW;
			else
				this.m_State = MorrigiRelicStates.IDLE;
		}

		private void SpawnMaxCrows()
		{
			int num = Math.Min(Math.Max(this.m_AvailableCrows.Count - (this.m_LoadingCrows.Count + this.m_SpawnedCrows.Count), 0), Math.Max(this.m_MorrigiRelic.BattleRiderMounts.Count<BattleRiderMount>() - this.m_LoadingCrows.Count, 0));
			List<ShipInfo> shipInfoList = new List<ShipInfo>();
			Matrix rotationYpr = Matrix.CreateRotationYPR(this.m_MorrigiRelic.Maneuvering.Rotation);
			rotationYpr.Position = this.m_MorrigiRelic.Maneuvering.Position;
			for (int index = 0; index < num; ++index)
			{
				this.m_AvailableCrows[index].RiderIndex = 0;
				Ship ship = Ship.CreateShip(this.m_Game.Game, rotationYpr, this.m_AvailableCrows[index], this.m_MorrigiRelic.ObjectID, this.m_MorrigiRelic.InputID, this.m_MorrigiRelic.Player.ObjectID, false, (IEnumerable<Player>)null);
				if (ship != null)
				{
					ship.ParentDatabaseID = this.m_AvailableCrows[index].ParentID;
					ship.Maneuvering.RetreatDestination = this.m_MorrigiRelic.Maneuvering.RetreatDestination;
					this.m_LoadingCrows.Add(ship);
				}
				shipInfoList.Add(this.m_AvailableCrows[index]);
			}
			foreach (ShipInfo shipInfo in shipInfoList)
				this.m_AvailableCrows.Remove(shipInfo);
			if (this.m_LoadingCrows.Count <= 0)
				return;
			this.m_State = MorrigiRelicStates.INTEGRATECROW;
		}

		public class TargetingData
		{
			public Ship Target;
			public int NumTargeting;
		}
	}
}
