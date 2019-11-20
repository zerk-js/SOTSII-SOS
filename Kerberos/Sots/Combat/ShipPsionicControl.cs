// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.ShipPsionicControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.ShipFramework;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Combat
{
	internal class ShipPsionicControl
	{
		private static int kMaxThinkDelay = 1800;
		private static int kMinThinkDelay = 600;
		private static int kSuulkaMaxThinkDelay = 300;
		private static int kSuulkaMinThinkDelay = 200;
		private App m_Game;
		private CombatAI m_CommanderAI;
		private PsionicControlState m_State;
		private Psionic m_HoldPsionic;
		private Ship m_Ship;
		private int m_CurrUpdateFrame;
		private int m_CurrMaxUpdateFrame;
		private int m_MinFrames;
		private int m_MaxFrames;

		public static float GetWeightForPsionic(CombatAI cmdAI, Ship ship, Psionic psi)
		{
			float num = 0.0f;
			if (psi != null && ship != null && !psi.IsActive)
			{
				switch (psi.Type)
				{
					case SectionEnumerations.PsionicAbility.TKFist:
					case SectionEnumerations.PsionicAbility.AbaddonLaser:
						num = 50f;
						break;
					case SectionEnumerations.PsionicAbility.Hold:
					case SectionEnumerations.PsionicAbility.Repair:
					case SectionEnumerations.PsionicAbility.Fear:
					case SectionEnumerations.PsionicAbility.WildFire:
					case SectionEnumerations.PsionicAbility.Control:
					case SectionEnumerations.PsionicAbility.Mirage:
					case SectionEnumerations.PsionicAbility.FalseFriend:
						num = 10f;
						break;
					case SectionEnumerations.PsionicAbility.Crush:
					case SectionEnumerations.PsionicAbility.Reflector:
					case SectionEnumerations.PsionicAbility.Posses:
					case SectionEnumerations.PsionicAbility.LifeDrain:
					case SectionEnumerations.PsionicAbility.Invisibility:
					case SectionEnumerations.PsionicAbility.Movement:
						num = 15f;
						break;
					case SectionEnumerations.PsionicAbility.Inspiration:
					case SectionEnumerations.PsionicAbility.Block:
						bool flag1 = false;
						foreach (EnemyGroup enemyGroup in cmdAI.GetEnemyGroups())
						{
							foreach (Ship ship1 in enemyGroup.m_Ships)
							{
								using (IEnumerator<Psionic> enumerator = ship1.Psionics.GetEnumerator())
								{
									if (enumerator.MoveNext())
									{
										Psionic current = enumerator.Current;
										flag1 = true;
									}
								}
								if (flag1)
									break;
							}
							if (flag1)
								break;
						}
						num = !flag1 ? 0.0f : 50f;
						break;
					case SectionEnumerations.PsionicAbility.Reveal:
						bool flag2 = false;
						foreach (EnemyGroup enemyGroup in cmdAI.GetEnemyGroups())
						{
							foreach (Ship ship1 in enemyGroup.m_Ships)
							{
								if ((double)cmdAI.GetCloakedDetectionPercent(ship1) < 1.0)
								{
									flag2 = true;
									break;
								}
							}
							if (flag2)
								break;
						}
						num = !flag2 ? 0.0f : 50f;
						break;
					case SectionEnumerations.PsionicAbility.PsiDrain:
						if (ship.CurrentPsiPower < (int)((double)ship.MaxPsiPower * 0.899999976158142))
						{
							num = 10f;
							if (ship.CurrentPsiPower < ship.MaxPsiPower / 2)
							{
								num += 20f;
								break;
							}
							break;
						}
						num = 0.0f;
						break;
				}
			}
			return num;
		}

		public static bool IsHoldPsionic(Psionic psi)
		{
			if (psi == null)
				return false;
			switch (psi.Type)
			{
				case SectionEnumerations.PsionicAbility.TKFist:
				case SectionEnumerations.PsionicAbility.Hold:
				case SectionEnumerations.PsionicAbility.Crush:
				case SectionEnumerations.PsionicAbility.Repair:
				case SectionEnumerations.PsionicAbility.Fear:
				case SectionEnumerations.PsionicAbility.Posses:
				case SectionEnumerations.PsionicAbility.PsiDrain:
				case SectionEnumerations.PsionicAbility.Control:
				case SectionEnumerations.PsionicAbility.LifeDrain:
				case SectionEnumerations.PsionicAbility.Movement:
					return true;
				case SectionEnumerations.PsionicAbility.Reflector:
				case SectionEnumerations.PsionicAbility.AbaddonLaser:
				case SectionEnumerations.PsionicAbility.Inspiration:
				case SectionEnumerations.PsionicAbility.Reveal:
				case SectionEnumerations.PsionicAbility.Listen:
				case SectionEnumerations.PsionicAbility.Block:
				case SectionEnumerations.PsionicAbility.WildFire:
				case SectionEnumerations.PsionicAbility.Mirage:
				case SectionEnumerations.PsionicAbility.FalseFriend:
					return false;
				default:
					return false;
			}
		}

		public static bool CanUsePsionic(Ship ship, Psionic psi, CombatAI cmdAI)
		{
			if (ship == null || psi == null || psi == null)
				return false;
			switch (psi.Type)
			{
				case SectionEnumerations.PsionicAbility.TKFist:
				case SectionEnumerations.PsionicAbility.Hold:
				case SectionEnumerations.PsionicAbility.Crush:
				case SectionEnumerations.PsionicAbility.Reflector:
				case SectionEnumerations.PsionicAbility.AbaddonLaser:
				case SectionEnumerations.PsionicAbility.Fear:
				case SectionEnumerations.PsionicAbility.Posses:
				case SectionEnumerations.PsionicAbility.WildFire:
				case SectionEnumerations.PsionicAbility.Control:
					if (ship.Target is Ship)
					{
						Ship target = ship.Target as Ship;
						float num = ship.SensorRange + target.ShipSphere.radius;
						if ((double)(ship.Position - target.Position).Length < (double)num * (double)num)
							return true;
						break;
					}
					break;
				case SectionEnumerations.PsionicAbility.Repair:
				case SectionEnumerations.PsionicAbility.Inspiration:
				case SectionEnumerations.PsionicAbility.Reveal:
				case SectionEnumerations.PsionicAbility.Block:
				case SectionEnumerations.PsionicAbility.Mirage:
				case SectionEnumerations.PsionicAbility.FalseFriend:
				case SectionEnumerations.PsionicAbility.Movement:
					return true;
				case SectionEnumerations.PsionicAbility.PsiDrain:
				case SectionEnumerations.PsionicAbility.LifeDrain:
					if (ship.Target != null)
					{
						if (ship.Target is Ship)
						{
							Ship target = ship.Target as Ship;
							float num = ship.SensorRange + target.ShipSphere.radius;
							if ((double)(ship.Position - target.Position).Length < (double)num * (double)num)
								return true;
							break;
						}
						if (ship.Target is StellarBody)
						{
							StellarBody target = ship.Target as StellarBody;
							float num = ship.SensorRange + target.Parameters.Radius;
							if ((double)(ship.Position - target.Parameters.Position).Length < (double)num * (double)num)
								return true;
							break;
						}
						break;
					}
					break;
			}
			return false;
		}

		public static bool PsionicsCanBeUsed(Ship ship, CombatAI cmdAI)
		{
			if (ship == null || ship.CurrentPsiPower <= 0 || ship.Psionics.Count<Psionic>() == 0)
				return false;
			bool flag = false;
			foreach (Psionic psionic in ship.Psionics)
			{
				if (ShipPsionicControl.CanUsePsionic(ship, psionic, cmdAI))
				{
					flag = true;
					break;
				}
			}
			return flag;
		}

		public Ship ControlledShip
		{
			get
			{
				return this.m_Ship;
			}
		}

		public ShipPsionicControl(App game, CombatAI commanderAI, Ship ship)
		{
			this.m_Game = game;
			this.m_CommanderAI = commanderAI;
			this.m_Ship = ship;
			this.m_HoldPsionic = (Psionic)null;
			this.m_MinFrames = ship.IsSuulka ? ShipPsionicControl.kSuulkaMinThinkDelay : ShipPsionicControl.kMinThinkDelay;
			this.m_MaxFrames = ship.IsSuulka ? ShipPsionicControl.kSuulkaMaxThinkDelay : ShipPsionicControl.kMaxThinkDelay;
			this.m_CurrMaxUpdateFrame = commanderAI.AIRandom.NextInclusive(this.m_MinFrames, this.m_MaxFrames);
			this.m_CurrUpdateFrame = this.m_CurrMaxUpdateFrame;
			this.m_State = PsionicControlState.Think;
		}

		public virtual void Shutdown()
		{
		}

		public virtual void ObjectRemoved(IGameObject obj)
		{
			if (this.m_Ship != obj)
				return;
			this.m_Ship = (Ship)null;
		}

		public virtual bool RemoveWeaponControl()
		{
			if (this.m_Ship == null)
				return true;
			--this.m_CurrUpdateFrame;
			return this.m_CurrUpdateFrame <= 0;
		}

		public bool CanChangeTarget()
		{
			return this.m_State != PsionicControlState.HoldPsionic;
		}

		public virtual void Update(int framesElapsed)
		{
			if (this.m_Ship == null)
				return;
			switch (this.m_State)
			{
				case PsionicControlState.Think:
					this.Think(framesElapsed);
					break;
				case PsionicControlState.ChoosePsionic:
					this.ChoosePsionic();
					break;
				case PsionicControlState.HoldPsionic:
					this.HoldPsionic();
					break;
			}
		}

		private void Think(int framesElapsed)
		{
			if (this.m_Ship.Target == null || !ShipPsionicControl.PsionicsCanBeUsed(this.m_Ship, this.m_CommanderAI))
				this.m_CurrUpdateFrame = this.m_CurrMaxUpdateFrame;
			else if (this.m_Ship.Target is Ship && !Ship.IsActiveShip(this.m_Ship.Target as Ship) || this.m_Ship.Target is StellarBody && (this.m_Ship.Target as StellarBody).Population <= 0.0)
			{
				this.m_CurrUpdateFrame = this.m_CurrMaxUpdateFrame;
			}
			else
			{
				this.m_CurrUpdateFrame -= framesElapsed;
				if (this.m_CurrUpdateFrame > 0)
					return;
				this.m_CurrMaxUpdateFrame = this.m_CommanderAI.AIRandom.NextInclusive(this.m_MinFrames, this.m_MaxFrames);
				this.m_CurrUpdateFrame = this.m_CurrMaxUpdateFrame;
				this.m_State = PsionicControlState.ChoosePsionic;
			}
		}

		private void ChoosePsionic()
		{
			if (this.m_Ship == null || this.m_Ship.Target == null)
				return;
			this.m_HoldPsionic = (Psionic)null;
			Psionic psionic1 = (Psionic)null;
			float maxValue = 0.0f;
			Dictionary<Psionic, float> dictionary = new Dictionary<Psionic, float>();
			foreach (Psionic psionic2 in this.m_Ship.Psionics)
			{
				float weightForPsionic = ShipPsionicControl.GetWeightForPsionic(this.m_CommanderAI, this.m_Ship, psionic2);
				if ((double)weightForPsionic > 0.0)
				{
					dictionary.Add(psionic2, weightForPsionic);
					maxValue += weightForPsionic;
				}
			}
			if (dictionary.Count == 0)
			{
				this.m_State = PsionicControlState.Think;
			}
			else
			{
				float num = this.m_CommanderAI.AIRandom.NextInclusive(1f, maxValue);
				foreach (KeyValuePair<Psionic, float> keyValuePair in dictionary)
				{
					if ((double)num <= (double)keyValuePair.Value)
					{
						psionic1 = keyValuePair.Key;
						break;
					}
					num -= keyValuePair.Value;
				}
				if (psionic1 == null)
					return;
				psionic1.Activate();
				if (ShipPsionicControl.IsHoldPsionic(psionic1))
				{
					this.m_State = PsionicControlState.HoldPsionic;
					this.m_HoldPsionic = psionic1;
				}
				else
				{
					switch (psionic1.Type)
					{
						case SectionEnumerations.PsionicAbility.Repair:
						case SectionEnumerations.PsionicAbility.Inspiration:
						case SectionEnumerations.PsionicAbility.Block:
							psionic1.PostSetProp("SetTarget", (IGameObject)this.m_CommanderAI.GetFriendlyShips()[this.m_CommanderAI.AIRandom.Next(this.m_CommanderAI.GetFriendlyShips().Count)]);
							break;
						default:
							psionic1.PostSetProp("SetTarget", this.m_Ship.Target.ObjectID);
							break;
					}
					psionic1.Deactivate();
					this.m_State = PsionicControlState.Think;
				}
			}
		}

		private void HoldPsionic()
		{
			if (this.m_Ship == null || this.m_HoldPsionic == null)
				return;
			if (this.m_Ship.Target == null)
				this.ClearHoldPsionic();
			else if (this.m_HoldPsionic.IsActive)
			{
				if ((double)this.m_HoldPsionic.PercentConsumed > 0.949999988079071)
					this.ClearHoldPsionic();
				else if (this.m_HoldPsionic is PsiDrain && this.m_Ship.CurrentPsiPower >= this.m_Ship.MaxPsiPower)
					this.ClearHoldPsionic();
			}
			else
				this.ClearHoldPsionic();
			if (this.m_HoldPsionic != null)
				return;
			this.m_State = PsionicControlState.Think;
		}

		private void ClearHoldPsionic()
		{
			if (this.m_HoldPsionic == null)
				return;
			this.m_HoldPsionic.Deactivate();
			this.m_HoldPsionic = (Psionic)null;
		}
	}
}
