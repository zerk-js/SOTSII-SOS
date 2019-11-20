// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.SpecterCombatAIControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

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
	internal class SpecterCombatAIControl : CombatAIController
	{
		private App m_Game;
		private Ship m_Spector;
		private IGameObject m_CurrentTarget;
		private int m_RefreshTarget;
		private SimpleAIStates m_State;

		public override Ship GetShip()
		{
			return this.m_Spector;
		}

		public override void SetTarget(IGameObject target)
		{
			this.m_CurrentTarget = target;
		}

		public override IGameObject GetTarget()
		{
			return this.m_CurrentTarget;
		}

		public SpecterCombatAIControl(App game, Ship ship)
		{
			this.m_Game = game;
			this.m_Spector = ship;
		}

		public override void Initialize()
		{
			this.m_CurrentTarget = (IGameObject)null;
			this.m_RefreshTarget = 200;
			this.m_State = SimpleAIStates.SEEK;
			SpectreGlobalData globalSpectreData = this.m_Game.Game.AssetDatabase.GlobalSpectreData;
			SpectreGlobalData.SpectreSize spectreSize = SpectreGlobalData.SpectreSize.Small;
			if (this.m_Game.Game.ScriptModules.Spectre != null)
			{
				if (this.m_Spector.DesignID == this.m_Game.Game.ScriptModules.Spectre.SmallDesignId)
					spectreSize = SpectreGlobalData.SpectreSize.Small;
				else if (this.m_Spector.DesignID == this.m_Game.Game.ScriptModules.Spectre.MediumDesignId)
					spectreSize = SpectreGlobalData.SpectreSize.Medium;
				else if (this.m_Spector.DesignID == this.m_Game.Game.ScriptModules.Spectre.BigDesignId)
					spectreSize = SpectreGlobalData.SpectreSize.Large;
			}
			this.m_Spector.Maneuvering.PostSetProp("SetCombatAIDamage", (object)globalSpectreData.Damage[(int)spectreSize].Crew, (object)globalSpectreData.Damage[(int)spectreSize].Population, (object)globalSpectreData.Damage[(int)spectreSize].InfraDamage, (object)globalSpectreData.Damage[(int)spectreSize].TeraDamage);
		}

		public override void Terminate()
		{
		}

		public override void ObjectRemoved(IGameObject obj)
		{
			if (obj != this.m_CurrentTarget)
				return;
			this.m_CurrentTarget = (IGameObject)null;
		}

		public override void OnThink()
		{
			if (this.m_Spector == null)
				return;
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

		public override void ForceFlee()
		{
		}

		public override bool VictoryConditionIsMet()
		{
			return false;
		}

		public override bool RequestingNewTarget()
		{
			if (this.m_State == SimpleAIStates.SEEK)
				return this.m_CurrentTarget == null;
			return false;
		}

		public override void FindNewTarget(IEnumerable<IGameObject> objs)
		{
			this.m_CurrentTarget = (IGameObject)null;
			float num1 = 1E+15f;
			List<StellarBody> stellarBodyList = new List<StellarBody>();
			List<Ship> shipList1 = new List<Ship>();
			List<Ship> shipList2 = new List<Ship>();
			List<Ship> source = new List<Ship>();
			bool flag = this.HasTargetInRange(objs);
			foreach (IGameObject gameObject in objs)
			{
				if (gameObject != this.m_Spector)
				{
					if (gameObject is Ship)
					{
						Ship ship = gameObject as Ship;
						if (ship.Player.ID == this.m_Spector.Player.ID)
							source.Add(ship);
						else if (Ship.IsActiveShip(ship) && !Ship.IsBattleRiderSize(ship.RealShipClass) && !(ship.Faction.Name == "loa") && (!flag || ship.IsDetected(this.m_Spector.Player)))
							shipList1.Add(ship);
					}
					else if (gameObject is StellarBody)
					{
						StellarBody stellarBody = gameObject as StellarBody;
						if (stellarBody.Population > 0.0 && stellarBody.Parameters.ColonyPlayerID != this.m_Spector.Player.ID)
							stellarBodyList.Add(stellarBody);
					}
				}
			}
			int num2 = 100;
			foreach (Ship ship in shipList1)
			{
				Ship targ = ship;
				int spectersForTarget = SpecterCombatAIControl.GetNumSpectersForTarget(targ.ShipClass);
				int num3 = source.Where<Ship>((Func<Ship, bool>)(x => x.Target == targ)).Count<Ship>();
				if (num3 < spectersForTarget && num3 <= num2)
				{
					float lengthSquared = (this.m_Spector.Position - targ.Position).LengthSquared;
					if ((double)lengthSquared < (double)num1 || num3 < num2)
					{
						num1 = lengthSquared;
						this.m_CurrentTarget = (IGameObject)targ;
						num2 = num3;
					}
				}
			}
			if (this.m_CurrentTarget == null)
			{
				int num3 = 100;
				foreach (Ship ship in shipList2)
				{
					Ship targ = ship;
					int spectersForTarget = SpecterCombatAIControl.GetNumSpectersForTarget(targ.ShipClass);
					int num4 = source.Where<Ship>((Func<Ship, bool>)(x => x.Target == targ)).Count<Ship>();
					if (num4 < spectersForTarget && num4 <= num3)
					{
						float lengthSquared = (this.m_Spector.Position - targ.Position).LengthSquared;
						if ((double)lengthSquared < (double)num1 || num4 < num3)
						{
							num1 = lengthSquared;
							this.m_CurrentTarget = (IGameObject)targ;
							num3 = num4;
						}
					}
				}
			}
			if (this.m_CurrentTarget != null)
				return;
			foreach (StellarBody stellarBody in stellarBodyList)
			{
				float lengthSquared = (this.m_Spector.Position - stellarBody.Parameters.Position).LengthSquared;
				if ((double)lengthSquared < (double)num1)
				{
					num1 = lengthSquared;
					this.m_CurrentTarget = (IGameObject)stellarBody;
				}
			}
		}

		private bool HasTargetInRange(IEnumerable<IGameObject> objs)
		{
			bool flag = false;
			foreach (IGameObject gameObject in objs)
			{
				if (gameObject != this.m_Spector)
				{
					if (gameObject is Ship)
					{
						Ship ship = gameObject as Ship;
						if (ship.Player != this.m_Spector.Player && Ship.IsActiveShip(ship) && (!Ship.IsBattleRiderSize(ship.RealShipClass) && ship.IsDetected(this.m_Spector.Player)))
						{
							flag = true;
							break;
						}
					}
					else if (gameObject is StellarBody)
					{
						StellarBody stellarBody = gameObject as StellarBody;
						if (stellarBody.Population > 0.0 && stellarBody.Parameters.ColonyPlayerID != this.m_Spector.Player.ID)
						{
							float num = (float)((double)stellarBody.Parameters.Radius + (double)this.m_Spector.ShipSphere.radius + 15000.0);
							if ((double)(stellarBody.Parameters.Position - this.m_Spector.Position).LengthSquared < (double)num * (double)num)
							{
								flag = true;
								break;
							}
						}
					}
				}
			}
			return flag;
		}

		public static int GetNumSpectersForTarget(ShipClass sc)
		{
			switch (sc)
			{
				case ShipClass.Cruiser:
					return 2;
				case ShipClass.Dreadnought:
					return 3;
				case ShipClass.Leviathan:
					return 4;
				case ShipClass.BattleRider:
					return 1;
				case ShipClass.Station:
					return 5;
				default:
					return 1;
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
			if (this.m_CurrentTarget == null)
				return;
			if (this.m_CurrentTarget is Ship)
				this.m_Spector.SetShipTarget(this.m_CurrentTarget.ObjectID, (this.m_CurrentTarget as Ship).ShipSphere.center, true, 0);
			else if (this.m_CurrentTarget is StellarBody)
				this.m_Spector.SetShipTarget(this.m_CurrentTarget.ObjectID, Vector3.Zero, true, 0);
			this.m_RefreshTarget = 200;
			this.m_State = SimpleAIStates.TRACK;
		}

		private void ThinkTrack()
		{
			if (this.m_CurrentTarget == null)
			{
				this.m_State = SimpleAIStates.SEEK;
			}
			else
			{
				bool flag = false;
				float num = 0.0f;
				if (this.m_CurrentTarget is Ship)
				{
					Ship currentTarget = this.m_CurrentTarget as Ship;
					this.m_Spector.Maneuvering.PostAddGoal(currentTarget.Position, -Vector3.UnitZ);
					if (!Ship.IsActiveShip(currentTarget))
						this.m_CurrentTarget = (IGameObject)null;
				}
				else if (this.m_CurrentTarget is StellarBody)
				{
					this.m_Spector.Maneuvering.PostAddGoal((this.m_CurrentTarget as StellarBody).Parameters.Position, -Vector3.UnitZ);
					flag = true;
				}
				if ((double)num <= 25000000.0 && !flag)
					return;
				--this.m_RefreshTarget;
				if (this.m_RefreshTarget > 0)
					return;
				this.m_CurrentTarget = (IGameObject)null;
			}
		}
	}
}
