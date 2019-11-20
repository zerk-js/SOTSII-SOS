// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GovMoralEffects
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Kerberos.Sots
{
	internal class GovMoralEffects
	{
		private Dictionary<MoralEvent, Dictionary<EffectAvailability, GovMoralEffects.MoralEffect>> MoralEffects;
		private Dictionary<EffectAvailability, GovMoralEffects.MoralEffect> AllMoralEffects;

		public GovMoralEffects()
		{
			this.MoralEffects = new Dictionary<MoralEvent, Dictionary<EffectAvailability, GovMoralEffects.MoralEffect>>();
			this.AllMoralEffects = new Dictionary<EffectAvailability, GovMoralEffects.MoralEffect>();
		}

		public void LoadMoralEffects(XmlElement moralEffects)
		{
			if (moralEffects == null)
				return;
			this.MoralEffects.Clear();
			this.AllMoralEffects.Clear();
			this.LoadEffects(EffectAvailability.All, moralEffects);
			this.LoadEffects(EffectAvailability.War, moralEffects["War"]);
			this.LoadEffects(EffectAvailability.Peace, moralEffects["Peace"]);
		}

		public void LoadEffects(EffectAvailability availability, XmlElement moralEffects)
		{
			if (moralEffects == null)
				return;
			XmlElement moralEffect1 = moralEffects["ALL"];
			if (moralEffect1 != null)
			{
				if (moralEffect1.HasAttributes)
				{
					GovMoralEffects.MoralEffect moralEffect2 = new GovMoralEffects.MoralEffect();
					string attribute1 = moralEffect1.GetAttribute("condition");
					string attribute2 = moralEffect1.GetAttribute("value");
					moralEffect2.Condition = !(attribute1 == "all") ? (!(attribute1 == "negative") ? (!(attribute1 == "positive") ? MoralCondition.None : MoralCondition.Positive) : MoralCondition.Negative) : MoralCondition.All;
					moralEffect2.Value = string.IsNullOrEmpty(attribute2) ? 0.0f : float.Parse(attribute2);
					this.AllMoralEffects.Add(availability, moralEffect2);
				}
				else
				{
					string innerText = moralEffect1.InnerText;
					GovMoralEffects.MoralEffect moralEffect2 = new GovMoralEffects.MoralEffect()
					{
						Condition = !(innerText == "immune") ? (!(innerText == "immune_neg") ? (!(innerText == "immune_pos") ? MoralCondition.None : MoralCondition.Positive) : MoralCondition.Negative) : MoralCondition.All
					};
					moralEffect2.Value = moralEffect2.Condition != MoralCondition.None ? 0.0f : float.Parse(innerText);
					this.AllMoralEffects.Add(availability, moralEffect2);
				}
			}
			foreach (MoralEvent key in Enum.GetValues(typeof(MoralEvent)))
			{
				XmlElement moralEffect2 = moralEffects[key.ToString()];
				if (moralEffect2 != null)
				{
					if (moralEffect2.HasAttributes)
					{
						GovMoralEffects.MoralEffect moralEffect3 = new GovMoralEffects.MoralEffect();
						string attribute1 = moralEffect2.GetAttribute("immune");
						string attribute2 = moralEffect2.GetAttribute("value");
						moralEffect3.Condition = !(attribute1 == "all") ? (!(attribute1 == "negative") ? (!(attribute1 == "positive") ? MoralCondition.None : MoralCondition.Positive) : MoralCondition.Negative) : MoralCondition.All;
						moralEffect3.Value = string.IsNullOrEmpty(attribute2) ? 0.0f : float.Parse(attribute2);
						this.MoralEffects.Add(key, new Dictionary<EffectAvailability, GovMoralEffects.MoralEffect>()
			{
			  {
				availability,
				moralEffect3
			  }
			});
					}
					else
					{
						string innerText = moralEffect2.InnerText;
						GovMoralEffects.MoralEffect moralEffect3 = new GovMoralEffects.MoralEffect()
						{
							Condition = !(innerText == "immune") ? (!(innerText == "immune_neg") ? (!(innerText == "immune_pos") ? MoralCondition.None : MoralCondition.Positive) : MoralCondition.Negative) : MoralCondition.All
						};
						moralEffect3.Value = moralEffect3.Condition != MoralCondition.None ? 0.0f : float.Parse(innerText);
						this.MoralEffects.Add(key, new Dictionary<EffectAvailability, GovMoralEffects.MoralEffect>()
			{
			  {
				availability,
				moralEffect3
			  }
			});
					}
				}
			}
		}

		private int GetValueWithImmunity(GovMoralEffects.MoralEffect effect, int moral)
		{
			switch (effect.Condition)
			{
				case MoralCondition.All:
					moral = 0;
					break;
				case MoralCondition.Negative:
					if (moral < 0)
					{
						moral = (double)effect.Value == 0.0 ? 0 : moral + (int)((double)moral * (double)effect.Value);
						break;
					}
					break;
				case MoralCondition.Positive:
					if (moral > 0)
					{
						moral = (double)effect.Value == 0.0 ? 0 : moral + (int)((double)moral * (double)effect.Value);
						break;
					}
					break;
				default:
					moral += (int)((double)moral * (double)effect.Value);
					break;
			}
			return moral;
		}

		public int GetResultingMoral(GameDatabase gamedb, MoralEvent me, int player, int moral)
		{
			bool flag1 = false;
			bool flag2 = false;
			if (this.AllMoralEffects.Keys.Any<EffectAvailability>((Func<EffectAvailability, bool>)(x =>
		   {
			   if (x != EffectAvailability.Peace)
				   return x == EffectAvailability.War;
			   return true;
		   })))
			{
				flag1 = GovernmentEffects.IsPlayerAtWar(gamedb, player);
				flag2 = true;
				if (flag1 && this.AllMoralEffects.ContainsKey(EffectAvailability.War))
					moral = this.GetValueWithImmunity(this.AllMoralEffects[EffectAvailability.War], moral);
				else if (!flag1 && this.AllMoralEffects.ContainsKey(EffectAvailability.Peace))
					moral = this.GetValueWithImmunity(this.AllMoralEffects[EffectAvailability.Peace], moral);
			}
			Dictionary<EffectAvailability, GovMoralEffects.MoralEffect> dictionary = new Dictionary<EffectAvailability, GovMoralEffects.MoralEffect>();
			if (this.MoralEffects.TryGetValue(me, out dictionary))
			{
				if (dictionary.Keys.Any<EffectAvailability>((Func<EffectAvailability, bool>)(x =>
			   {
				   if (x != EffectAvailability.Peace)
					   return x == EffectAvailability.War;
				   return true;
			   })))
				{
					if (!flag2)
						flag1 = GovernmentEffects.IsPlayerAtWar(gamedb, player);
					if (flag1 && dictionary.ContainsKey(EffectAvailability.War))
						moral = this.GetValueWithImmunity(dictionary[EffectAvailability.War], moral);
					else if (!flag1 && dictionary.ContainsKey(EffectAvailability.Peace))
						moral = this.GetValueWithImmunity(dictionary[EffectAvailability.Peace], moral);
				}
				else if (dictionary.ContainsKey(EffectAvailability.All))
					moral = this.GetValueWithImmunity(dictionary[EffectAvailability.All], moral);
			}
			return moral;
		}

		private struct MoralEffect
		{
			public MoralCondition Condition;
			public float Value;
		}
	}
}
