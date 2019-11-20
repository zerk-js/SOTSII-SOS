// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GovStratModifiers
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Kerberos.Sots
{
	internal class GovStratModifiers
	{
		private Dictionary<StratModifiers, KeyValuePair<EffectAvailability, float>> StratEffects;

		public GovStratModifiers()
		{
			this.StratEffects = new Dictionary<StratModifiers, KeyValuePair<EffectAvailability, float>>();
		}

		public void LoadStratEffects(XmlElement stratEffects)
		{
			if (stratEffects == null)
				return;
			this.StratEffects.Clear();
			foreach (StratModifiers key in Enum.GetValues(typeof(StratModifiers)))
			{
				XmlElement stratEffect = stratEffects[key.ToString()];
				if (stratEffect != null)
				{
					if (stratEffect.HasAttributes)
						this.StratEffects.Add(key, new KeyValuePair<EffectAvailability, float>(stratEffect.GetAttribute("condition") == "war" ? EffectAvailability.War : EffectAvailability.Peace, float.Parse(stratEffect.GetAttribute("value"))));
					else
						this.StratEffects.Add(key, new KeyValuePair<EffectAvailability, float>(EffectAvailability.All, float.Parse(stratEffect.InnerText)));
				}
			}
		}

		public float GetResultingStratModifierValue(
		  GameDatabase gamedb,
		  StratModifiers sm,
		  int player,
		  float modValue)
		{
			KeyValuePair<EffectAvailability, float> keyValuePair;
			if (this.StratEffects.TryGetValue(sm, out keyValuePair))
			{
				if (keyValuePair.Key != EffectAvailability.All)
				{
					bool flag = GovernmentEffects.IsPlayerAtWar(gamedb, player);
					if (flag && keyValuePair.Key == EffectAvailability.War || !flag && keyValuePair.Key == EffectAvailability.Peace)
						modValue += keyValuePair.Value;
				}
				else
					modValue += keyValuePair.Value;
			}
			return modValue;
		}

		public int GetResultingStratModifierValue(
		  GameDatabase gamedb,
		  StratModifiers sm,
		  int player,
		  int modValue)
		{
			KeyValuePair<EffectAvailability, float> keyValuePair;
			if (this.StratEffects.TryGetValue(sm, out keyValuePair))
			{
				if (keyValuePair.Key != EffectAvailability.All)
				{
					bool flag = GovernmentEffects.IsPlayerAtWar(gamedb, player);
					if (flag && keyValuePair.Key == EffectAvailability.War || !flag && keyValuePair.Key == EffectAvailability.Peace)
						modValue += (int)keyValuePair.Value;
				}
				else
					modValue += (int)keyValuePair.Value;
			}
			return modValue;
		}
	}
}
