// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.VonNeumannMomControl
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
	internal class VonNeumannMomControl : CombatAIController
	{
		private App m_Game;
		private Ship m_VonNeumannMom;
		private List<Ship> m_LoadingChilds;
		private List<VonNeumannChildControl> m_SpawnedChilds;
		private List<VonNeumannMomControl> m_SpawnedMoms;
		private List<VonNeumannChildSpawnLocations> m_SpawnLocations;
		private List<Ship> m_SpawnedDefensePlatforms;
		private List<Ship> m_SpawnedEnemies;
		private Ship m_LoadingMom;
		private IGameObject m_Target;
		private VonNeumannOrders m_Orders;
		private VonNeumannMomStates m_State;
		private int m_NumChildrenToMaintain;
		private int m_VonNeumannID;
		private int m_FleetID;
		private int m_RUStore;
		private bool m_Vanished;
		private bool m_TryFindParent;

		public static int CalcNumChildren(VonNeumannGlobalData vgd, int numSats, int numShips)
		{
			if (vgd == null || vgd.NumShipsPerChild <= 0 || vgd.NumSatelitesPerChild <= 0)
				return 0;
			return numSats / vgd.NumSatelitesPerChild + numShips / vgd.NumShipsPerChild;
		}

		public bool Vanished
		{
			get
			{
				return this.m_Vanished;
			}
		}

		public override Ship GetShip()
		{
			return this.m_VonNeumannMom;
		}

		public override void SetTarget(IGameObject target)
		{
			this.m_Target = target;
		}

		public override IGameObject GetTarget()
		{
			return this.m_Target;
		}

		public VonNeumannMomStates State
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

		public VonNeumannMomControl(App game, Ship ship, int fleetid)
		{
			this.m_Game = game;
			this.m_VonNeumannMom = ship;
			this.m_FleetID = fleetid;
		}

		public override void Initialize()
		{
			this.m_LoadingMom = (Ship)null;
			this.m_LoadingChilds = new List<Ship>();
			this.m_SpawnedChilds = new List<VonNeumannChildControl>();
			this.m_SpawnedMoms = new List<VonNeumannMomControl>();
			this.m_SpawnLocations = new List<VonNeumannChildSpawnLocations>();
			this.m_SpawnedDefensePlatforms = new List<Ship>();
			this.m_SpawnedEnemies = new List<Ship>();
			this.m_State = VonNeumannMomStates.COLLECT;
			this.m_Orders = VonNeumannOrders.COLLECT;
			this.m_NumChildrenToMaintain = 10;
			this.m_Vanished = false;
			this.m_TryFindParent = true;
			this.m_VonNeumannID = 0;
			VonNeumannInfo vonNeumannInfo = this.m_Game.GameDatabase.GetVonNeumannInfos().ToList<VonNeumannInfo>().FirstOrDefault<VonNeumannInfo>((Func<VonNeumannInfo, bool>)(x =>
		   {
			   int? fleetId1 = x.FleetId;
			   int fleetId2 = this.m_FleetID;
			   if (fleetId1.GetValueOrDefault() == fleetId2)
				   return fleetId1.HasValue;
			   return false;
		   }));
			if (vonNeumannInfo != null)
				this.m_VonNeumannID = vonNeumannInfo.Id;
			Matrix rotationYpr = Matrix.CreateRotationYPR(this.m_VonNeumannMom.Maneuvering.Rotation);
			rotationYpr.Position = this.m_VonNeumannMom.Maneuvering.Position + rotationYpr.Forward * 500f;
			Vector3 right = rotationYpr.Right;
			int designId = VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.CollectorProbe].DesignId;
			int childrenToMaintain1 = this.m_NumChildrenToMaintain;
			float radians = MathHelper.DegreesToRadians(360f / (float)childrenToMaintain1);
			float num1 = radians * 0.5f;
			float num2 = 1f;
			for (int index = 0; index < childrenToMaintain1; ++index)
			{
				float num3 = radians * (float)((index % childrenToMaintain1 + 1) / 2) * num2 + num1;
				this.m_SpawnLocations.Add(new VonNeumannChildSpawnLocations(this.m_VonNeumannMom, new Vector3((float)Math.Sin((double)num3), 0.0f, -(float)Math.Cos((double)num3)), 300f));
				num2 *= -1f;
			}
			int childrenToMaintain2 = this.m_Game.AssetDatabase.GlobalVonNeumannData.MinChildrenToMaintain;
			for (int index = 0; index < childrenToMaintain2; ++index)
			{
				VonNeumannChildSpawnLocations childSpawnLocations = this.m_SpawnLocations.FirstOrDefault<VonNeumannChildSpawnLocations>((Func<VonNeumannChildSpawnLocations, bool>)(x => x.CanSpawnAtLocation()));
				if (childSpawnLocations != null)
				{
					Matrix worldMat = rotationYpr;
					worldMat.Position = childSpawnLocations.GetSpawnLocation();
					Ship newShip = CombatAIController.CreateNewShip(this.m_Game.Game, worldMat, designId, 0, this.m_VonNeumannMom.InputID, this.m_VonNeumannMom.Player.ObjectID);
					if (newShip != null)
					{
						this.m_LoadingChilds.Add(newShip);
						childSpawnLocations.SpawnAtLocation(newShip);
					}
				}
			}
			if (this.m_LoadingChilds.Count <= 0)
				return;
			this.m_State = VonNeumannMomStates.INTEGRATECHILD;
		}

		public override void Terminate()
		{
			if (this.m_VonNeumannMom != null && !this.m_VonNeumannMom.IsDestroyed)
				this.ThinkVanish();
			this.m_SpawnedDefensePlatforms.Clear();
			this.m_SpawnedEnemies.Clear();
		}

		public override void ObjectRemoved(IGameObject obj)
		{
			foreach (VonNeumannMomControl spawnedMom in this.m_SpawnedMoms)
			{
				if (spawnedMom.GetShip() == obj)
				{
					this.m_SpawnedMoms.Remove(spawnedMom);
					break;
				}
			}
			foreach (VonNeumannChildControl spawnedChild in this.m_SpawnedChilds)
			{
				if (spawnedChild.GetShip() == obj)
				{
					this.m_SpawnedChilds.Remove(spawnedChild);
					break;
				}
			}
			if (this.m_Target == obj)
				this.m_Target = (IGameObject)null;
			foreach (VonNeumannChildSpawnLocations spawnLocation in this.m_SpawnLocations)
			{
				if (spawnLocation.GetChildInSpace() == obj)
					spawnLocation.Clear();
			}
			List<Ship> shipList = new List<Ship>();
			foreach (Ship spawnedDefensePlatform in this.m_SpawnedDefensePlatforms)
			{
				if (spawnedDefensePlatform == obj)
					shipList.Add(spawnedDefensePlatform);
			}
			foreach (Ship spawnedEnemy in this.m_SpawnedEnemies)
			{
				if (spawnedEnemy == obj)
					shipList.Add(spawnedEnemy);
			}
			foreach (Ship ship in shipList)
			{
				this.m_SpawnedDefensePlatforms.Remove(ship);
				this.m_SpawnedEnemies.Remove(ship);
			}
		}

		public void SubmitResources(int amount)
		{
			if (amount <= 0)
				return;
			this.m_RUStore += amount;
		}

		public void AddChild(CombatAIController child)
		{
			if (child is VonNeumannChildControl)
			{
				foreach (Ship loadingChild in this.m_LoadingChilds)
				{
					if (loadingChild == child.GetShip())
					{
						this.m_LoadingChilds.Remove(loadingChild);
						break;
					}
				}
				this.m_SpawnedChilds.Add(child as VonNeumannChildControl);
			}
			else if (child is VonNeumannMomControl)
			{
				if (child.GetShip() == this.m_LoadingMom)
					this.m_LoadingMom = (Ship)null;
				this.m_SpawnedMoms.Add(child as VonNeumannMomControl);
			}
			switch (this.m_State)
			{
				case VonNeumannMomStates.INITFLEE:
				case VonNeumannMomStates.FLEE:
				case VonNeumannMomStates.VANISH:
					child.ForceFlee();
					break;
			}
		}

		public bool IsThisMyMom(Ship ship)
		{
			if (!this.m_LoadingChilds.Any<Ship>((Func<Ship, bool>)(x => x == ship)))
				return this.m_LoadingMom == ship;
			return true;
		}

		public override void OnThink()
		{
			if (this.m_VonNeumannMom == null)
				return;
			foreach (VonNeumannChildSpawnLocations spawnLocation in this.m_SpawnLocations)
				spawnLocation.Update();
			if (this.m_VonNeumannMom.IsDestroyed)
				this.m_State = VonNeumannMomStates.INITFLEE;
			else
				this.ValidateChildren();
			switch (this.m_State)
			{
				case VonNeumannMomStates.COLLECT:
					this.ThinkCollect();
					break;
				case VonNeumannMomStates.EMITCHILD:
					this.ThinkEmitChild();
					break;
				case VonNeumannMomStates.INTEGRATECHILD:
					this.ThinkIntegrateChild();
					break;
				case VonNeumannMomStates.EMITMOM:
					this.ThinkEmitMom();
					break;
				case VonNeumannMomStates.INTEGRATEMOM:
					this.ThinkIntegrateMom();
					break;
				case VonNeumannMomStates.INITFLEE:
					this.ThinkInitFlee();
					break;
				case VonNeumannMomStates.FLEE:
					this.ThinkFlee();
					break;
				case VonNeumannMomStates.VANISH:
					this.ThinkVanish();
					break;
			}
		}

		public override void ForceFlee()
		{
			if (this.m_State == VonNeumannMomStates.INITFLEE || this.m_State == VonNeumannMomStates.FLEE)
				return;
			this.m_State = VonNeumannMomStates.INITFLEE;
		}

		public override bool VictoryConditionIsMet()
		{
			return this.m_Vanished;
		}

		public override bool RequestingNewTarget()
		{
			bool flag = false;
			if (this.m_State == VonNeumannMomStates.COLLECT)
			{
				foreach (CombatAIController spawnedChild in this.m_SpawnedChilds)
				{
					if (spawnedChild.RequestingNewTarget())
					{
						flag = true;
						break;
					}
				}
			}
			return flag;
		}

		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			this.m_SpawnedDefensePlatforms.Clear();
			this.m_SpawnedEnemies.Clear();
			List<Ship> shipList = new List<Ship>();
			List<StellarBody> stellarBodyList = new List<StellarBody>();
			foreach (IGameObject gameObject in objs)
			{
				if (gameObject is Ship)
				{
					Ship ship = gameObject as Ship;
					if (ship.Active && Ship.IsActiveShip(ship) && ship.Player != this.m_VonNeumannMom.Player)
					{
						if (ship.RealShipClass == RealShipClasses.Platform)
							this.m_SpawnedDefensePlatforms.Add(ship);
						else
							this.m_SpawnedEnemies.Add(ship);
						bool flag1 = ship.IsDetected(this.m_VonNeumannMom.Player);
						bool flag2 = false;
						foreach (VonNeumannChildControl spawnedChild in this.m_SpawnedChilds)
						{
							if (ship == spawnedChild.GetTarget())
							{
								flag2 = true;
								if (!flag1)
								{
									spawnedChild.SetTarget((IGameObject)null);
									break;
								}
								break;
							}
						}
						if (flag1 && !flag2)
							shipList.Add(ship);
					}
				}
				else if (gameObject is StellarBody)
				{
					StellarBody stellarBody = gameObject as StellarBody;
					if (stellarBody.Parameters.ColonyPlayerID != this.m_VonNeumannMom.Player.ID && stellarBody.Population > 0.0)
						stellarBodyList.Add(stellarBody);
				}
			}
			bool flag = shipList.Count == 0;
			foreach (VonNeumannChildControl spawnedChild in this.m_SpawnedChilds)
			{
				if (spawnedChild.RequestingNewTarget())
				{
					Vector3 position = spawnedChild.GetShip().Position;
					float num = float.MaxValue;
					if (shipList.Count > 0)
					{
						Ship ship1 = (Ship)null;
						foreach (Ship ship2 in shipList)
						{
							float lengthSquared = (ship2.Position - position).LengthSquared;
							if ((double)lengthSquared < (double)num)
							{
								ship1 = ship2;
								num = lengthSquared;
							}
						}
						if (ship1 != null)
						{
							spawnedChild.SetTarget((IGameObject)ship1);
							shipList.Remove(ship1);
						}
					}
					flag = shipList.Count == 0;
				}
				if (flag)
					break;
			}
			if (!flag)
				return;
			StellarBody stellarBody1 = (StellarBody)null;
			float num1 = float.MaxValue;
			foreach (StellarBody stellarBody2 in stellarBodyList)
			{
				float lengthSquared = (stellarBody2.Parameters.Position - this.m_VonNeumannMom.Position).LengthSquared;
				if ((double)lengthSquared < (double)num1)
				{
					stellarBody1 = stellarBody2;
					num1 = lengthSquared;
				}
			}
			if (stellarBody1 == null)
				return;
			foreach (VonNeumannChildControl spawnedChild in this.m_SpawnedChilds)
			{
				if (spawnedChild.RequestingNewTarget())
					spawnedChild.SetTarget((IGameObject)stellarBody1);
			}
		}

		public override bool NeedsAParent()
		{
			return this.m_TryFindParent;
		}

		public override void FindParent(IEnumerable<CombatAIController> controllers)
		{
			foreach (CombatAIController controller in controllers)
			{
				if (controller is VonNeumannMomControl)
				{
					VonNeumannMomControl neumannMomControl = controller as VonNeumannMomControl;
					if (neumannMomControl.IsThisMyMom(this.m_VonNeumannMom))
					{
						neumannMomControl.AddChild((CombatAIController)this);
						break;
					}
				}
			}
			this.m_TryFindParent = false;
		}

		public int GetStoredResources()
		{
			return this.m_RUStore;
		}

		private int GetNumChildrenToMaintain()
		{
			return VonNeumannMomControl.CalcNumChildren(this.m_Game.AssetDatabase.GlobalVonNeumannData, this.m_SpawnedDefensePlatforms.Count, this.m_SpawnedEnemies.Count);
		}

		private void ValidateChildren()
		{
			List<VonNeumannChildControl> neumannChildControlList = new List<VonNeumannChildControl>();
			foreach (VonNeumannChildControl spawnedChild in this.m_SpawnedChilds)
			{
				if (spawnedChild.GetShip() == null || spawnedChild.GetShip().IsDestroyed)
					neumannChildControlList.Add(spawnedChild);
			}
			foreach (VonNeumannChildControl neumannChildControl in neumannChildControlList)
				this.m_SpawnedChilds.Remove(neumannChildControl);
		}

		private void ThinkCollect()
		{
			if (this.m_RUStore >= this.m_Game.AssetDatabase.GlobalVonNeumannData.MomRUCost)
			{
				switch (this.m_Orders)
				{
					case VonNeumannOrders.COLLECT:
						this.m_State = VonNeumannMomStates.INITFLEE;
						break;
					default:
						this.m_State = VonNeumannMomStates.EMITMOM;
						break;
				}
			}
			else if (this.m_RUStore >= this.m_Game.AssetDatabase.GlobalVonNeumannData.ChildRUCost)
			{
				if (this.m_SpawnedChilds.Count >= this.m_NumChildrenToMaintain)
					return;
				this.m_State = VonNeumannMomStates.EMITCHILD;
			}
			else
			{
				if (this.m_SpawnedChilds.Count >= this.m_Game.AssetDatabase.GlobalVonNeumannData.MinChildrenToMaintain || this.m_LoadingChilds.Count != 0)
					return;
				this.m_RUStore += this.m_Game.AssetDatabase.GlobalVonNeumannData.ChildRUCost;
				this.m_State = VonNeumannMomStates.EMITCHILD;
			}
		}

		private void ThinkEmitChild()
		{
			VonNeumannChildSpawnLocations childSpawnLocations = this.m_SpawnLocations.FirstOrDefault<VonNeumannChildSpawnLocations>((Func<VonNeumannChildSpawnLocations, bool>)(x => x.CanSpawnAtLocation()));
			if (childSpawnLocations == null)
				return;
			Matrix rotationYpr = Matrix.CreateRotationYPR(this.m_VonNeumannMom.Maneuvering.Rotation);
			rotationYpr.Position = childSpawnLocations.GetSpawnLocation();
			Ship newShip = CombatAIController.CreateNewShip(this.m_Game.Game, rotationYpr, VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.CollectorProbe].DesignId, 0, this.m_VonNeumannMom.InputID, this.m_VonNeumannMom.Player.ObjectID);
			newShip.Maneuvering.RetreatDestination = this.m_VonNeumannMom.Maneuvering.RetreatDestination;
			if (newShip == null)
			{
				this.m_State = VonNeumannMomStates.COLLECT;
			}
			else
			{
				childSpawnLocations.SpawnAtLocation(newShip);
				this.m_RUStore -= this.m_Game.AssetDatabase.GlobalVonNeumannData.ChildRUCost;
				this.m_State = VonNeumannMomStates.INTEGRATECHILD;
				this.m_LoadingChilds.Add(newShip);
			}
		}

		private void ThinkIntegrateChild()
		{
			if (this.m_LoadingChilds.Count == 0)
			{
				this.m_State = VonNeumannMomStates.COLLECT;
			}
			else
			{
				bool flag = true;
				foreach (GameObject loadingChild in this.m_LoadingChilds)
				{
					if (loadingChild.ObjectStatus != GameObjectStatus.Ready)
						flag = false;
				}
				if (!flag)
					return;
				foreach (Ship loadingChild in this.m_LoadingChilds)
				{
					loadingChild.Player = this.m_VonNeumannMom.Player;
					loadingChild.Active = true;
					this.m_Game.CurrentState.AddGameObject((IGameObject)loadingChild, false);
				}
				this.m_State = VonNeumannMomStates.COLLECT;
			}
		}

		private void ThinkEmitMom()
		{
			this.m_RUStore -= this.m_Game.AssetDatabase.GlobalVonNeumannData.MomRUCost;
			Matrix rotationYpr = Matrix.CreateRotationYPR(this.m_VonNeumannMom.Maneuvering.Rotation);
			rotationYpr.Position = this.m_VonNeumannMom.Maneuvering.Position + rotationYpr.Forward * 500f;
			Ship newShip = CombatAIController.CreateNewShip(this.m_Game.Game, rotationYpr, VonNeumann.StaticShipDesigns[VonNeumann.VonNeumannShipDesigns.CollectorMothership].DesignId, 0, this.m_VonNeumannMom.InputID, this.m_VonNeumannMom.Player.ObjectID);
			if (newShip == null)
			{
				this.m_State = VonNeumannMomStates.COLLECT;
			}
			else
			{
				this.m_RUStore -= this.m_Game.AssetDatabase.GlobalVonNeumannData.ChildRUCost;
				this.m_State = VonNeumannMomStates.INTEGRATEMOM;
				this.m_LoadingMom = newShip;
			}
		}

		private void ThinkIntegrateMom()
		{
			if (this.m_LoadingMom == null)
			{
				this.m_State = VonNeumannMomStates.COLLECT;
			}
			else
			{
				if (this.m_LoadingMom.ObjectStatus != GameObjectStatus.Ready)
					return;
				this.m_LoadingMom.Active = true;
				this.m_LoadingMom.Player = this.m_VonNeumannMom.Player;
				this.m_Game.CurrentState.AddGameObject((IGameObject)this.m_LoadingMom, false);
				this.m_State = VonNeumannMomStates.INITFLEE;
			}
		}

		private void ThinkInitFlee()
		{
			this.m_State = VonNeumannMomStates.FLEE;
			foreach (CombatAIController spawnedChild in this.m_SpawnedChilds)
				spawnedChild.ForceFlee();
			foreach (CombatAIController spawnedMom in this.m_SpawnedMoms)
				spawnedMom.ForceFlee();
		}

		private void ThinkFlee()
		{
			if (this.m_VonNeumannMom.CombatStance != CombatStance.RETREAT)
				this.m_VonNeumannMom.SetCombatStance(CombatStance.RETREAT);
			if (!this.m_VonNeumannMom.HasRetreated)
				return;
			this.m_State = VonNeumannMomStates.VANISH;
		}

		private void ThinkVanish()
		{
			if (this.m_Vanished)
				return;
			int ruStore = this.m_RUStore;
			foreach (VonNeumannChildControl spawnedChild in this.m_SpawnedChilds)
				ruStore += spawnedChild.GetStoredResources();
			VonNeumann.HandleMomRetreated(this.m_Game.Game, this.m_VonNeumannID, ruStore);
			this.m_VonNeumannMom.Active = false;
			this.m_Vanished = true;
		}
	}
}
